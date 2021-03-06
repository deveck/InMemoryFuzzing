/* Copyright 2010 Andreas Reiter <andreas.reiter@student.tugraz.at>, 
 *                Georg Neubauer <georg.neubauer@student.tugraz.at>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


// Author: Andreas Reiter <andreas.reiter@student.tugraz.at>
// Author: Georg Neubauer <georg.neubauer@student.tugraz.at>

using System;
using System.IO;
using Iaik.Utils.Hash;

namespace Iaik.Utils
{


	public static class ConsoleUtils
	{

		public static ProtectedPasswordStorage ReadPassword (string hintText)
		{
			Console.Write (hintText);
			
			ConsoleKeyInfo consoleKeyInfo;
			ProtectedPasswordStorage pws = new ProtectedPasswordStorage();
			
			
				while (true)
				{
					consoleKeyInfo = Console.ReadKey(true);
					if (consoleKeyInfo.Key == ConsoleKey.Enter)
					{
						Console.WriteLine ();
						return pws;
					}
					else if (consoleKeyInfo.Key == ConsoleKey.Escape)
					{
						Console.WriteLine ();
						return null;
					}
					else
						pws.AppendPasswordChar (consoleKeyInfo.KeyChar);
				}
			}

		}
		
		
		
	}

