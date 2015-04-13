using System;
using System.IO;
using System.Windows.Forms;
using System.Text;

namespace CommandPrompt
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        private void buttonRun_Click(object sender, EventArgs e)
        {
            textBoxOutput.Clear();
            string param = " /r " + textBox1.Text + " /s SRV*" + textBox2.Text + "*http://msdl.microsoft.com/download/symbols";
            /*
            using(CommandPrompt commandPrompt = new CommandPrompt("symchk", param))
            {
                    commandPrompt.Exited += commandPrompt_Exited;
                    commandPrompt.OutputDataReceived += commandPrompt_DataReceived;
                    commandPrompt.ErrorDataReceived += commandPrompt_DataReceived;
                    commandPrompt.Run();
                    labelStatus.Text = "Command is running...";
                      
            }
             */
            DirectoryInfo di = new DirectoryInfo(textBox2.Text);
            WalkDirectory(di);             
        }

        public void WalkDirectory(System.IO.DirectoryInfo root)
        {

            string[] allFiles = System.IO.Directory.GetFiles(root.FullName, "*.pdb", System.IO.SearchOption.AllDirectories);
          
                foreach (string filename in allFiles)
                {
                    // operation for each file

                    string param = filename;
                    int len = param.Length - 4;
                    param = param.Substring(0, len);
                    string tempname = string.Concat(param, ".txt");
                    param = filename + " > " + tempname;
                    textBox3.Text = param;
                    textBox4.Text = filename;
                    CommandPrompt commandPrompt = new CommandPrompt("Dia2Dump", param);
                    commandPrompt.Exited += commandPrompt_Exited;
                    commandPrompt.OutputDataReceived += commandPrompt_DataReceived;
                    commandPrompt.ErrorDataReceived += commandPrompt_DataReceived;
                    commandPrompt.BeginRun();
                 //   searchtext(tempname);
                }
            
        }

        /*
        public void searchtext(string logname)
        {
            string line;
            StringBuilder sb = new StringBuilder();
            using (System.IO.StreamReader file = new System.IO.StreamReader(logname))
            {
                while((line = file.ReadLine()) != null)
                {
                    if (line.Contains("AppendMenuA"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("AppendMenuW"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("NtUserBeginPaint"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("CheckMenuItem"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("CheckMenuRadioItem"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("DrawTextA"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("DrawTextExA"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("DrawTextExW"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("EnableMenuItem"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("FillRect"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("NtUserGetAncestor"))
                    {
                        sb.AppendLine(line.ToString());

                    }
                    if (line.Contains("NtUserGetCursorInfo"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("GetCursorPos"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("NtuserGetDC"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("NtUserDCEx"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("NtUserGetForegroundWindow"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("GetKeyState"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("GetMessageA"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("GetMessagePos"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("GetMessageTime"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("GetMessageW"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("GetUpdateRect"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("GetUpdateRgn"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("NtUserGetWindowDC"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("GetWindowLongA"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("GetWindowLongPtrA"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("GetWindowLongPtrW"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("GetWindowLongW"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains(" GetWindowRect"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("InsertMenuItemA"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("InsertMenuItemW"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("NtUserInvalidateRgn"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("NtUserInvalidateRect"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains(" IsWindowVisible"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("PeekMessageA"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("PeekMessageW"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("SetForegroundWindow"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("SetMenu"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("SetMenuItemInfoA"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("SetMenuItemInfoW"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("NtUserSetParent"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("SetWindowLongA"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("SetWindowLongPtrA"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("SetWindowLongPtrW"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("SetWindowLongW"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("NtUserSetWindowPlacement"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("NtUserSetWindowPos"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("NtUserTrackPopupMenuEx"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("NtUserWindowFromPoint"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("NtUserSetWindowPos"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains(" LdrpLoadDll(Ldrp"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("NtMapViewofSection"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("LoadLibraryExW"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("FreeLibrary"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("SetConsoleActiveScreenBuffer"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("CreateProcessInternalW"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("BitBlt"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("CreateCompatibleDC"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("CreateDCA"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("CreateDCW"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("CreateFontIndirectW"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("CreateFontW"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("DeleteDC"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("ExtTextOutA"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("FillRgn"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("GetClipBox"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("PaintRgn"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("PolyTextOutA"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("PolyTextOutW"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("TextOutA"))
                    {
                        sb.AppendLine(line.ToString());
                    }
                    if (line.Contains("TextOutW"))
                    {
                        sb.AppendLine(line.ToString());
                    }

                }
            }
            using (StreamWriter outfile = new StreamWriter(@"C:\pdbproject\tempfile.txt"))
             {
            outfile.Write(sb.ToString());
              }
        }
        public void parsetext ()
        */

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        void commandPrompt_DataReceived(object sender, DataEventArgs e)
        {
            textBoxOutput.Invoke((Action)(() => textBoxOutput.AppendText(e.Data + Environment.NewLine)));
        }

        void commandPrompt_Exited(object sender, EventArgs e)
        {
            labelStatus.Invoke((Action)(() => labelStatus.Text = "Command has finished executing."));

        }
        void symchk_Exited(object sender, EventArgs e)
        {
            DirectoryInfo di = new DirectoryInfo(textBox2.Text);
            WalkDirectory(di);  
        }
    }
}
