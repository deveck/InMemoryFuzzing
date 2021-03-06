// GDBLogger.cs
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
using Fuzzer.TargetConnectors.GDB;
using System.IO;
namespace Fuzzer.DataLoggers
{
	/// <summary>
	/// Logs all relevant data generated by a GDB connector
	/// </summary>
	/// <remarks>
	/// This logger saves a gdb reverse execution log (core dump) 
	/// in the specified folder and the specified prefix+".execlog"
	/// </remarks>
	public class GDBLogger : IDataLogger
	{
		/// <summary>
		/// Path to save log files at
		/// </summary>
		private string _path;
		
		/// <summary>
		/// The associated connector
		/// </summary>
		private GDBConnector _connector;
	
		private string _prefix = "";
		
		public GDBLogger (GDBConnector gdbConn, string path)
		{
			_path = path;
			_connector = gdbConn;
		}
	
		#region IDataLogger implementation
		public string Prefix 
		{
			get { return _prefix; }
			set { _prefix = value; }
		}
		
		public void FinishedFuzzRun ()
		{
			_connector.SaveExecutionLog (BuildExecutionLogFile ());
		}

		public void StartingFuzzRun ()
		{
		}
		#endregion
			
		/// <summary>
		/// Builds the next execution logfile, depending on the specified path and prefix
		/// </summary>
		/// <returns></returns>
		private string BuildExecutionLogFile ()
		{
			string filename = "";
			
			if (_prefix != null && _prefix != String.Empty)
				filename = _prefix + ".execlog";
			else
				filename = "gdb.execlog";
			
			return Path.Combine (_path, filename);
				
		}
	}
}

