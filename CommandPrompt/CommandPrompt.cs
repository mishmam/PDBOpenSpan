using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;

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

    // Class that allows running commands and receiving output.
    internal class CommandPrompt : IDisposable
    {
        private Process _process;

        void IDisposable.Dispose()
        { }
  
        private ProcessStartInfo _startInfo;

        private StringBuilder _standardOutput;

        private StringBuilder _standardError;

        public const int NoTimeOut = 0;
        
        public bool IsRunning { get; private set; }

        public bool HasExited { get; private set; }

        public int ProcessId { get; private set; }

        public int ExitCode { get; private set; }

        public string StandardOutput
        {
            get
            {
                return _standardOutput.ToString();
            }
        }

        public string StandardError
        {
            get
            {
                return _standardError.ToString();
            }
        }

        public event EventHandler<DataEventArgs> OutputDataReceived = (sender, args) => { };

        public event EventHandler<DataEventArgs> ErrorDataReceived = (sender, args) => { };

        public event EventHandler Exited = (sender, args) => { };


        public CommandPrompt(string exe, string arguments = "", string workingDirectory = "")
        {
            _standardOutput = new StringBuilder();
            _standardError = new StringBuilder();

            _startInfo = new ProcessStartInfo()
            {
                FileName = exe,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,                // This is required to redirect stdin, stdout and stderr
                CreateNoWindow = true,                  // Don't create a window
                RedirectStandardOutput = true,          // Capture standard output
                RedirectStandardError = true,           // Capture standard error
                RedirectStandardInput = true,           // Enable sending commands to standard input
            };

            _process = new Process()
            {
                StartInfo = _startInfo,
                EnableRaisingEvents = true,
            };
            _process.OutputDataReceived += _process_OutputDataReceived;
            _process.ErrorDataReceived += _process_ErrorDataReceived;
            _process.Exited += _process_Exited;
        }

        public void Run()
        {
            if (!IsRunning && !HasExited)
            {
                BeginRun();
                _process.WaitForExit(10000);
}
        }

        public void BeginRun()
        {
            if (!IsRunning && !HasExited)
            {
                if (_process.Start())
                {
                    IsRunning = true;
                    ProcessId = _process.Id;

                    _process.BeginOutputReadLine();
                    _process.BeginErrorReadLine();
                }
            }
        }

        public void WriteToStandardInput(string command)
        {
            if (IsRunning && !HasExited)
            {
                _process.StandardInput.Write(command);
            }
        }

        public void Kill(bool killChildProcesses = false)
        {
            if (killChildProcesses && ProcessId != 0)
            {
                KillChildProcesses(ProcessId);
            }
            else if (IsRunning && !HasExited)
            {
                _process.Kill();
            }
        }

        private void KillChildProcesses(int parentPid)
        {
            using (var searcher = new ManagementObjectSearcher("select ProcessId from Win32_Process where ParentProcessId=" + parentPid))
            using (ManagementObjectCollection objCollection = searcher.Get())
            {
                foreach (ManagementObject obj in objCollection)
                {
                    int pid = Convert.ToInt32(obj["ProcessID"]);
                    KillChildProcesses(pid);
                }
            }

            try
            {
                Process.GetProcessById(parentPid).Kill();
            }
            catch (ArgumentException)
            {
            }
        }

        private void _process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            _standardOutput.AppendLine(e.Data);

            OutputDataReceived(this, new DataEventArgs(e.Data));
        }

        private void _process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            _standardError.AppendLine(e.Data);

            ErrorDataReceived(this, new DataEventArgs(e.Data));
        }

        private void _process_Exited(object sender, EventArgs e)
        {
            HasExited = true;
            IsRunning = false;
            ExitCode = _process.ExitCode;
            Exited(this, e);
        }
    }
}
