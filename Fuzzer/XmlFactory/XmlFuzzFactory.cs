// XmlFuzzFactory.cs
//  
//  Author:
//       Andreas Reiter <andreas.reiter@student.tugraz.at>
// 
//  Copyright 2011  Andreas Reiter
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.IO;
using System.Xml;
using Fuzzer.RemoteControl;
using Iaik.Utils;
using System.Collections;
using System.Collections.Generic;
using Fuzzer.TargetConnectors;
using Iaik.Utils.CommonFactories;
namespace Fuzzer.XmlFactory
{
	/// <summary>
	/// Creates all components of the fuzzer by using a supplied xml description file
	/// See SampleConfigs/SampleFuzzDescription.xml for a fully documented sample 
	/// description file
	/// </summary>
	/// <remarks>
	/// The file is parsed at object creation, which means that the syntactical correctness is
	/// checked. But it is not checked that all required sections are available
	/// </remarks>
	public class XmlFuzzFactory
	{
		public enum ExecutionTriggerEnum
		{
			/// <summary>
			/// Executes program once on initialization
			/// </summary>
			Immediate,
			
			/// <summary>
			/// Executes program at the beginning of every fuzzer run
			/// </summary>
			OnFuzzStart,
			
			/// <summary>
			/// Executes program at the end of every fuzzer run
			/// </summary>
			OnFuzzStop
		}
		
		/// <summary>
		/// The description document
		/// </summary>
		private XmlDocument _doc;
		
		/// <summary>
		/// Connection to the target system for remote controlling
		/// </summary>
		private RemoteControlProtocol _remoteControlProtocol = null;
		
		/// <summary>
		/// Connector to the fuzzing stub
		/// </summary>
		private ITargetConnector _connector = null;
		
		/// <summary>
		/// Contains all triggered executions
		/// </summary>
		private IDictionary<ExecutionTriggerEnum, List<RemoteExecCommand>> _triggeredExecutions =
			new Dictionary<ExecutionTriggerEnum, List<RemoteExecCommand>>();
				
		/// <summary>
		/// Contains informations about the current program being launched (on the remote side)
		/// </summary>
		private RemoteExecutionInfo _currentRemoteExecInfo = null;
		
		
		public XmlFuzzFactory (string path)
		{
			if (File.Exists (path) == false)
				throw new FileNotFoundException (string.Format ("The specified xml description file '{0}' does not exist", path));
			
			
			_doc = new XmlDocument ();
			_doc.Load (path);			
		}
		
		/// <summary>
		/// Initializes the fuzzing environment
		/// </summary>
		public void Init()
		{
			InitRemote();
			InitTargetConnection();
		}
		
		/// <summary>
		/// Initializes the remote control and extracts the commands to execute 
		/// from the configuration file
		/// </summary>
		private void InitRemote()
		{
			if(_remoteControlProtocol != null)
			{
				_remoteControlProtocol.Dispose();
				_remoteControlProtocol = null;
			}
			
			XmlElement remoteControlNode = (XmlElement)_doc.DocumentElement.SelectSingleNode("RemoteControl");

			//remote control is not mandatory, but strongly recommended.
			//Without remote control there is no way to capture remote memory allocations
			//and therefore a lot information to analyze gets lost
			if(remoteControlNode != null)
			{
				_remoteControlProtocol = new RemoteControlProtocol();
				_remoteControlProtocol.SetConnection(RemoteControlConnectionBuilder.Connect(
					  XmlHelper.ReadString(remoteControlNode, "Host"), XmlHelper.ReadInt(remoteControlNode, "Port", 0)));
				
				_remoteControlProtocol.ExecStatus += Handle_remoteControlProtocolExecStatus;
			
				
				foreach(XmlElement execNode in remoteControlNode.SelectNodes("Exec"))
				{
					ExecutionTriggerEnum execTrigger = 
						(ExecutionTriggerEnum)Enum.Parse(typeof(ExecutionTriggerEnum), execNode.GetAttribute("trigger"), true);
					
					
					string cmd = XmlHelper.ReadString(execNode, "Cmd");
					
					if(cmd == null)
						throw new ArgumentException("Exec node without cmd");
					
					List<string> arguments = new List<string>(XmlHelper.ReadStringArray(execNode, "Arg"));
					List<string> environment = new List<string>(XmlHelper.ReadStringArray(execNode, "Env"));
					
					if(_triggeredExecutions.ContainsKey(execTrigger) == false)
						_triggeredExecutions[execTrigger] = new List<RemoteExecCommand>();
					
					_triggeredExecutions[execTrigger].Add(
						new RemoteExecCommand(cmd, cmd, arguments, environment));
				}
				
				RemoteExec(ExecutionTriggerEnum.Immediate);
			}			
		}
		
		/// <summary>
		/// Execute all programs that are registered for the specified trigger
		/// </summary>
		/// <param name="toExec"></param>
		private void RemoteExec(ExecutionTriggerEnum toExec)
		{
			if(_triggeredExecutions.ContainsKey(toExec))
			{
				foreach(RemoteExecCommand cmdExec in _triggeredExecutions[toExec])
				{
					_currentRemoteExecInfo = 
						new RemoteExecutionInfo(cmdExec);
		
					_remoteControlProtocol.SendCommand(cmdExec);
					
					if(!_currentRemoteExecInfo.SyncEvent.WaitOne(5000))
						throw new ArgumentException(
						   string.Format("Could not execute command '{0}', check the connection and the " +
						                 "remote terminal for errors", cmdExec.Path));
					
					if(_currentRemoteExecInfo.ExecStatus != RemoteExecutionInfo.ExecutionStatus.Success)
						throw new ArgumentException(string.Format(
						   "Remote program reported an errorcode #{0}", _currentRemoteExecInfo.ErrorCode));
				}
			}
		}
		
		/// <summary>
		/// Initializes the connection to the target
		/// </summary>
		private void InitTargetConnection()
		{
			XmlElement connectorRoot = (XmlElement)_doc.DocumentElement.SelectSingleNode("TargetConnection");
			
			if(connectorRoot == null)
				throw new ArgumentException("Could not find 'TargetConnection' node");
			
			string connectorIdentifier = XmlHelper.ReadString(connectorRoot, "Connector");
			ITargetConnector connector = GenericClassIdentifierFactory.CreateFromClassIdentifierOrType<ITargetConnector>(connectorIdentifier);
			
			if(connector == null)
				throw new ArgumentException(string.Format("Could not find connector with identifier '{0}'", connectorIdentifier));

			IDictionary<string, string > configuration = new Dictionary<string, string>();
			
			foreach(XmlElement configNode in connectorRoot.SelectNodes("Config"))
				configuration.Add(configNode.GetAttribute("key"), configNode.InnerXml);
			
			connector.Setup(configuration);
			connector.Connect();
			_connector = connector;
		}

		/// <summary>
		/// Called after an exec call has been sent to the remote target
		/// </summary>
		/// <param name="name"></param>
		/// <param name="pid"></param>
		/// <param name="status"></param>
		private void Handle_remoteControlProtocolExecStatus (string name, int pid, int status)
		{
			if(_currentRemoteExecInfo != null && _currentRemoteExecInfo.Cmd.Name == name)
			{
				_currentRemoteExecInfo.ExecStatus = (status == 0 ? RemoteExecutionInfo.ExecutionStatus.Success:
					RemoteExecutionInfo.ExecutionStatus.Error);
				_currentRemoteExecInfo.ErrorCode = status;
				_currentRemoteExecInfo.PID = pid;
				_currentRemoteExecInfo.SyncEvent.Set();
			}
		}
	}
}
