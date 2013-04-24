// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)
using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.GitAddIn
{
	/// <summary>
	/// Class representing the Git Console pad.
	/// </summary>
	public class GitConsolePad : AbstractConsolePad
	{
		public GitConsolePad()
		{
		}
		
		protected override string Prompt
		{
			get
			{
				return "> git ";
			}
		}
		
		protected override bool AcceptCommand(string command)
		{
			Append(Environment.NewLine + "executed command '" + command + "'");
			return true;
		}
	}
}
