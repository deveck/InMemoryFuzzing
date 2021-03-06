// IStackFrameInfo.cs
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
using Iaik.Utils.Serialization;
using System.Collections.Generic;
namespace Fuzzer.TargetConnectors
{
	/// <summary>
	/// Contains information about the currently active stack frame
	/// </summary>
	public interface IStackFrameInfo : ITypedStreamSerializable
	{
		/// <summary>
		/// Returns the name of all saved registers
		/// </summary>
		IEnumerable<string> SavedRegisters{ get; }
		
		/// <summary>
		/// Gets the address of a stack-saved register,
		/// Registertypes can be resolved to register names using the connector specific IRegisterTypeResolver
		/// </summary>
		/// <param name="registerName">Register names can </param>
		/// <returns></returns>
		IAddressSpecifier GetSavedRegisterAddress(string registerName);
	}
}

