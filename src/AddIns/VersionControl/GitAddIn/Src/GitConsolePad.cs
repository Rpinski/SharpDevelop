// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.GitAddIn
{
	/// <summary>
	/// Class representing the Git Console pad.
	/// </summary>
	public class GitConsolePad : AbstractConsolePad
	{
		private Queue<string> outputQueue = new Queue<string>();
		char[] buffer;
		private bool isExecuting = false;
		private ProcessRunner gitProcessRunner;
		private StreamReader gitOutputStreamReader;
//		internal readonly Process gitProcess = null;
//		internal readonly ProcessRunner gitProcessRunner = null;
		
		public GitConsolePad()
		{
			buffer = new char[4096];
			
//			string gitExe = Git.FindGit();
//			if (gitExe != null)
//			{
//				gitProcessRunner = new ProcessRunner();
//				gitProcessRunner.CreationFlags = (ProcessCreationFlags) 0x00000008;
//				gitProcessRunner.RedirectStandardOutput = true;
//				gitProcessRunner.EnvironmentVariables.Add("DISPLAY", @":9999");
//				gitProcessRunner.EnvironmentVariables.Add("GIT_ASKPASS", @"C:\Program Files\TortoiseGit\bin\SshAskPass.exe");
//
//				gitProcess = new Process();
//				gitProcess.StartInfo.FileName = gitExe; // = @"E:\Andreas\projekte\SharpDevelop5_work\GitConsoleTest.exe";
//				gitProcess.StartInfo.UseShellExecute = false;
//				gitProcess.StartInfo.CreateNoWindow = true;
//				gitProcess.StartInfo.RedirectStandardError = true;
//				gitProcess.StartInfo.RedirectStandardInput = true;
//				gitProcess.StartInfo.RedirectStandardOutput = true;
//				gitProcess.EnableRaisingEvents = true;
//				gitProcess.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
//				{
//					lock (outputQueue)
//					{
//						outputQueue.Enqueue(e.Data);
//					}
//					SD.MainThread.InvokeAsyncAndForget(ReadAll);
//				};
//				gitProcess.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
//				{
//					lock (outputQueue)
//					{
//						outputQueue.Enqueue(e.Data);
//						LoggingService.WarnFormatted("GitConsole: Output: {0}", e.Data);
//					}
//					SD.MainThread.InvokeAsyncAndForget(ReadAll);
//				};
//				gitProcess.Exited += delegate(object sender, EventArgs e)
//				{
//					gitProcess.WaitForExit();
//					lock (outputQueue)
//					{
			////						gitProcess.CancelErrorRead();
			////						gitProcess.CancelOutputRead();
			////						isExecuting = false;
//					}
//					SD.MainThread.InvokeAsyncAndForget(GitExit);
//				};
//			}
		}
		
		void StartGit(string[] commandLineArguments)
		{
//			if (gitProcess != null)
//			{
//				isExecuting = true;
//				gitProcess.StartInfo.Arguments = commandLineArguments;
//				gitProcess.StartInfo.WorkingDirectory = @"E:\Andreas\projekte\SharpDevelop5_work";
//				gitProcess.Start();
//				gitProcess.BeginErrorReadLine();
//				gitProcess.BeginOutputReadLine();
//			}
			
			string gitExe = Git.FindGit();
			if (gitExe != null)
			{
				gitProcessRunner = new ProcessRunner();
//				gitProcessRunner.CreationFlags |= (ProcessCreationFlags) (0x00000008 | 0x00000200 | 0x08000000);
				gitProcessRunner.CreationFlags |= (ProcessCreationFlags) (0x00000200 | 0x08000000);
				gitProcessRunner.RedirectStandardError = true;
				gitProcessRunner.RedirectStandardOutput = true;
				gitProcessRunner.RedirectStandardOutputAndErrorToSingleStream = true;
				gitProcessRunner.EnvironmentVariables.Add("DISPLAY", @":9999");
				gitProcessRunner.EnvironmentVariables.Add("GIT_ASKPASS", @"C:\Program Files\TortoiseGit\bin\SshAskPass.exe");
				gitProcessRunner.EnvironmentVariables.Add("SSH_ASKPASS", @"C:\Program Files\TortoiseGit\bin\SshAskPass.exe");
				gitProcessRunner.EnvironmentVariables.Add("GIT_SSH", @"C:\Program Files (x86)\Git\bin\ssh.exe");
//				gitProcessRunner.EnvironmentVariables.Add("TERM", "dumb");
				if (!gitProcessRunner.EnvironmentVariables.ContainsKey("HOME"))
				{
					string homeDrive = Environment.GetEnvironmentVariable("HOMEDRIVE");
					if (!homeDrive.EndsWith("\\") && !homeDrive.EndsWith("/"))
					{
						homeDrive += "\\";
					}
					string homePath = Environment.GetEnvironmentVariable("HOMEPATH");
					if (homePath.StartsWith("\\") || homePath.StartsWith("/"))
					{
						homePath = homePath.Substring(1);
					}
					gitProcessRunner.EnvironmentVariables.Add("HOME", Path.Combine(homeDrive, homePath));
				}
				gitProcessRunner.EnvironmentVariables["PATH"] = gitProcessRunner.EnvironmentVariables["PATH"] + @";C:\Program Files (x86)\Git\bin";
				gitProcessRunner.WorkingDirectory = @"E:\Andreas\projekte\SharpDevelop5_work";
				gitProcessRunner.Start(Git.FindGit(), commandLineArguments);
//				gitProcessRunner.Start(@"C:\Users\WheizWork\Documents\Visual Studio 2012\Projects\GitConsoleTest\GitConsoleTest\bin\Debug\GitConsoleTest.exe", commandLineArguments);
				gitOutputStreamReader = gitProcessRunner.OpenStandardOutputReader();
				
//					Action<ProcessRunner> readOutputMethod = ReadOutput;
//					AsyncCallback asyncCallback = delegate(IAsyncResult result) {};
//					readOutputMethod.BeginInvoke(gitProcessRunner, asyncCallback, null);
				
				StringBuilder debugEnv = new StringBuilder();
				foreach (var envvar in gitProcessRunner.EnvironmentVariables)
				{
					debugEnv.AppendLine(envvar.Key + " = " + envvar.Value);
				}
				
				ReadOutput();
//					ReadOutputAsync(gitProcessRunner);
//					gitProcessRunner.WaitForExitAsync();
			}
		}
		
		private void GitExit()
		{
//			lock (outputQueue)
//			{
			isExecuting = false;
//			}
			
			// Free resources
			gitOutputStreamReader.Dispose();
			gitOutputStreamReader = null;
			gitProcessRunner.Dispose();
			gitProcessRunner = null;
			
			AppendPrompt();
		}
		
		private async Task ReadOutputAsync(ProcessRunner process)
		{
//			char[] buffer = new char[4096];
//			StringBuilder builder = new StringBuilder();
//
//			using (StreamReader reader = process.OpenStandardOutputReader())
//			{
//				do
//				{
//					int charsRead = 0;
//					while ((charsRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
//					{
//						// Output
//						builder.Append(buffer);
			////						Console.Write(builder.ToString(0, charsRead));
//						int offset = 0;
//						InsertBeforePrompt(builder.ToString(offset, charsRead - offset));
			////						Append(builder.ToString(0, charsRead - offset));
//						builder.Clear();
//					}
//				} while (!process.HasExited);
//			}
			
//			AppendLine("");
//			AppendPrompt();

			char[] buffer = new char[4096];
			StringBuilder outputBuilder = new StringBuilder();
			StringBuilder tempBuilder = new StringBuilder();

			AppendLine("");
			
			using (StreamReader reader = process.OpenStandardOutputReader())
			{
//				while (!process.HasExited)
//				{
				int totalCharsRead = 0;
				int charsRead = 0;
				while ((charsRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
				{
					// Output
					string readString = new String(buffer);
					readString = readString.Substring(0, charsRead);
					InsertBeforePrompt(readString);
//					totalCharsRead += charsRead;
//					builder.Clear();
				}
//				string output = builder.ToString(0, totalCharsRead);
				
//				}
			}
		}
		
		private void ReadFinishCallback(Task<int> task)
		{
			try
			{
				LoggingService.WarnFormatted("ReadFinishCallback: hasExited = {0}", gitProcessRunner.HasExited);
				
				if (task.Result > 0)
				{
					string readString = new String(buffer);
					lock (outputQueue)
					{
						outputQueue.Enqueue(readString.Substring(0, task.Result));
					}
					SD.MainThread.InvokeAsyncAndForget(ReadAll);
					
					StartAsyncRead();
				}
				else
				{
					// Finished
					Task exitTask = gitProcessRunner.WaitForExitAsync();
					SD.MainThread.InvokeAsyncAndForget(GitExit);
				}
			}
			catch (Exception ex)
			{
				LoggingService.Error("Git ReadCallback failure", ex);
			}
		}
		
		private void StartAsyncRead()
		{
			gitOutputStreamReader.ReadAsync(buffer, 0, buffer.Length).ContinueWith(ReadFinishCallback);
		}
		
		private void ReadOutput()
		{
//			lock (outputQueue)
//			{
			isExecuting = true;
//			}
			
			StartAsyncRead();
			
//		StringBuilder outputBuilder = new StringBuilder();
//		StringBuilder tempBuilder = new StringBuilder();

//			AppendLine("");
			
//		using (StreamReader reader = process.OpenStandardOutputReader())
//		{
//			StartAsyncRead(reader);
			
//				while (!process.HasExited)
//				{
//				int totalCharsRead = 0;
//				int charsRead = 0;
//				while ((charsRead = reader.Read(buffer, 0, buffer.Length)) > 0)
//				{
//					// Output
//					string readString = new String(buffer);
			////						outputBuilder.Append(readString.Substring(0, charsRead));
//					lock (outputQueue)
//					{
//						outputQueue.Enqueue(readString.Substring(0, charsRead));
//					}
//					SD.MainThread.InvokeAsyncAndForget(ReadAll);
			////					totalCharsRead += charsRead;
			////					builder.Clear();
//				}
//				string output = builder.ToString(0, totalCharsRead);
//				AppendLine(outputBuilder.ToString());
//				}
//		}
		}
		
//		int expectedPrompts;
//
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
//					LoggingService.WarnFormatted("GitConsole: Prompt (isExecuting = {0})", isExecuting);
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
				LoggingService.WarnFormatted("GitConsole: AcceptCommand (isExecuting = {0})", isExecuting);
				if (isExecuting)
				{
//					if (gitProcess != null)
//					{
//						gitProcess.StandardInput.WriteLine(command);
//					}
					return false;
				}
				else
				{
					string[] arguments = command.Split(' ');
					StartGit(arguments);
				}
			}
			return true;
		}
	}
}
