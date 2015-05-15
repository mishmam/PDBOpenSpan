using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.IO;
using System.ComponentModel;

namespace CommandPrompt
{
    // Class used to store string data.
    public class DataEventArgs : EventArgs
    {
        public string Data { get; private set; }

        public DataEventArgs(string data)
        {
            Data = data;
        }
    }

    public class ExitedEventArgs : EventArgs
    {
        public int ExitCode { get; private set; }

        public ExitedEventArgs(int code)
        {
            ExitCode = code;
        }
    }

    // Class that allows running commands and receiving output.
    public class CommandPrompt
    {
        private StringBuilder mStandardOutput = new StringBuilder();
        private StringBuilder mStandardError = new StringBuilder();
        private string mFileName;
        private string mArguments;
        private string mWorkingDirectory;
        private int mTimeout = -1;
        private bool mDisposed = false;
        System.Threading.AutoResetEvent mExitedWaitHandle = new System.Threading.AutoResetEvent(false);

        #region Construction

        public CommandPrompt(string fileName, string arguments = "", string workingDirectory = "")
        {
            mFileName = fileName;
            mArguments = arguments;
            mWorkingDirectory = workingDirectory;
            TimeOut = -1;
        }

        #endregion

        public void Run()
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += new DoWorkEventHandler(
            delegate(object o, DoWorkEventArgs args)
            {
                using (System.Threading.AutoResetEvent outputWaitHandle = new System.Threading.AutoResetEvent(false))
                using (System.Threading.AutoResetEvent errorWaitHandle = new System.Threading.AutoResetEvent(false))
                {
                    using (Process process = new Process())
                    {
                        // Apparently putting EnableraisingEvents as true should be BEFORE setting StartInfo parameters!
                        process.EnableRaisingEvents = true;

                        process.StartInfo.FileName = mFileName;
                        process.StartInfo.Arguments = mArguments;
                        process.StartInfo.WorkingDirectory = mWorkingDirectory;
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        // process.StartInfo.RedirectStandardInput = true;


                        try
                        {
                            process.OutputDataReceived += (sender, e) =>
                            {
                                if (e.Data == null)
                                {
                                    outputWaitHandle.Set();
                                }
                                else
                                {
                                    mStandardOutput.AppendLine(e.Data);
                                    OutputDataReceived(this, new DataEventArgs(e.Data));
                                }
                            };

                            process.ErrorDataReceived += (sender, e) =>
                            {
                                if (e.Data == null)
                                {
                                    errorWaitHandle.Set();
                                }
                                else
                                {
                                    mStandardError.AppendLine(e.Data);
                                    ErrorDataReceived(this, new DataEventArgs(e.Data));
                                }
                            };

                            mExitedWaitHandle.Reset();
                            process.Start();
                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();

                            if (process.WaitForExit(TimeOut))
                            {
                                ExitCode = process.ExitCode;
                                Exited(this, new ExitedEventArgs(ExitCode));
                            }
                            else
                            {
                                ExitCode = -1;
                                Exited(this, new ExitedEventArgs(ExitCode));
                            }

                        }
                        finally
                        {
                            outputWaitHandle.WaitOne(TimeOut);
                            errorWaitHandle.WaitOne(TimeOut);
                            mExitedWaitHandle.Set();
                        }
                    }
                }
            });

            backgroundWorker.RunWorkerAsync();
        }

        public int ProcessId { get; private set; }

        public int ExitCode { get; private set; }

        public int TimeOut { get; set; }

        public string StandardOutput
        {
            get
            {
                return mStandardOutput.ToString();
            }
        }


        public string StandardError
        {
            get
            {
                return mStandardError.ToString();
            }
        }

        public event EventHandler<DataEventArgs> OutputDataReceived = (sender, args) => { };

        public event EventHandler<DataEventArgs> ErrorDataReceived = (sender, args) => { };

        public event EventHandler<ExitedEventArgs> Exited = (sender, args) => { };

        public void Wait()
        {
            mExitedWaitHandle.WaitOne(TimeOut);
        }

        //public void WriteToStandardInput(string command)
        //{
        //	if (IsRunning && !HasExited)
        //	{
        //		_process.StandardInput.Write(command);
        //	}
        //}

        //public void Kill(bool killChildProcesses = false)
        //{
        //	if (killChildProcesses && ProcessId != 0)
        //	{
        //		KillChildProcesses(ProcessId);
        //	}
        //	else if (IsRunning && !HasExited)
        //	{
        //		_process.Kill();
        //	}
        //}

        //private void KillChildProcesses(int parentPid)
        //{
        //	using (var searcher = new ManagementObjectSearcher("select ProcessId from Win32_Process where ParentProcessId=" + parentPid))
        //	using (ManagementObjectCollection objCollection = searcher.Get())
        //	{
        //		foreach (ManagementObject obj in objCollection)
        //		{
        //			int pid = Convert.ToInt32(obj["ProcessID"]);
        //			KillChildProcesses(pid);
        //		}
        //	}

        //	try
        //	{
        //		Process.GetProcessById(parentPid).Kill();
        //	}
        //	catch (ArgumentException)
        //	{
        //	}
        //}

    }
}
