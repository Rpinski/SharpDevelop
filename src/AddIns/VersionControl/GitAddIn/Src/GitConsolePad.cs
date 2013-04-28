// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;

using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.GitAddIn
{
	/// <summary>
	/// Class representing the Git Console pad.
	/// </summary>
	public class GitConsolePad : AbstractConsolePad
	{
		private Queue<string> outputQueue = new Queue<string>();
		private bool isExecuting = false;
		internal readonly Process gitProcess = null;
		
		public GitConsolePad()
		{
			string gitExe = Git.FindGit();
			if (gitExe != null)
			{
				gitProcess = new Process();
				gitProcess.StartInfo.FileName = Git.FindGit();
				gitProcess.StartInfo.UseShellExecute = false;
				gitProcess.StartInfo.CreateNoWindow = true;
				gitProcess.StartInfo.RedirectStandardError = true;
				gitProcess.StartInfo.RedirectStandardInput = true;
				gitProcess.StartInfo.RedirectStandardOutput = true;
				gitProcess.EnableRaisingEvents = true;
				gitProcess.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
				{
					lock (outputQueue)
					{
						outputQueue.Enqueue(e.Data);
					}
					SD.MainThread.InvokeAsyncAndForget(ReadAll);
				};
				gitProcess.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
				{
					lock (outputQueue)
					{
						outputQueue.Enqueue(e.Data);
						LoggingService.WarnFormatted("GitConsole: Output: {0}", e.Data);
					}
					SD.MainThread.InvokeAsyncAndForget(ReadAll);
				};
				gitProcess.Exited += delegate(object sender, EventArgs e)
				{
					gitProcess.WaitForExit();
					lock (outputQueue)
					{
//						gitProcess.CancelErrorRead();
//						gitProcess.CancelOutputRead();
//						isExecuting = false;
					}
					SD.MainThread.InvokeAsyncAndForget(GitExit);
				};
			}
		}
		
		void StartGit(string commandLineArguments)
		{
			if (gitProcess != null)
			{
				isExecuting = true;
				gitProcess.StartInfo.Arguments = commandLineArguments;
				gitProcess.StartInfo.WorkingDirectory = @"E:\Andreas\projekte\SharpDevelop5_work";
				gitProcess.Start();
				gitProcess.BeginErrorReadLine();
				gitProcess.BeginOutputReadLine();
			}
		}
		
		private void GitExit()
		{
			gitProcess.CancelErrorRead();
			gitProcess.CancelOutputRead();
			isExecuting = false;
			LoggingService.Warn("GitConsole: Exited.");
			AppendPrompt();
		}
		
//		int expectedPrompts;
		
		private void ReadAll()
		{
			StringBuilder b = new StringBuilder();
			lock (outputQueue)
			{
				while (outputQueue.Count > 0)
				{
					b.AppendLine(outputQueue.Dequeue());
				}
			}
			int offset = 0;
//			// ignore prompts inserted by fsi.exe (we only see them too late as we're reading line per line)
//			for (int i = 0; i < expectedPrompts; i++)
//			{
//				if (offset + 1 < b.Length && b[offset] == '>' && b[offset + 1] == ' ')
//					offset += 2;
//				else
//					break;
//			}
//			expectedPrompts = 0;
			InsertBeforePrompt(b.ToString(offset, b.Length - offset));
		}
		
//		protected virtual bool HandleInput(Key key)
//		{
//			if (isExecuting)
//			{
//				if (gitProcess != null)
//				{
//					// Forward key to input stream of process
//					if (gitProcess != null)
//					{
//						gitProcess.StandardInput.Write('c');
//					}
//				}
//			}
//			else
//			{
//				// Normal handling of prompt
//				return base.HandleInput(key);
//			}
//		}
		
		protected override string Prompt
		{
			get
			{
				lock (outputQueue)
				{
					LoggingService.WarnFormatted("GitConsole: Prompt (isExecuting = {0})", isExecuting);
					if (!isExecuting)
					{
						return "> git ";
					}
					else
					{
						return "";
					}
				}
			}
		}
		
		protected override bool AcceptCommand(string command)
		{
			lock (outputQueue)
			{
				if (isExecuting)
				{
					if (gitProcess != null)
					{
						gitProcess.StandardInput.WriteLine(command);
					}
				}
				else
				{
					StartGit(command);
				}
			}
			return true;
		}
	}
}
