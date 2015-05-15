using System;
using System.IO;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Collections;

namespace CommandPrompt
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        StringBuilder outputdata = new StringBuilder();

        private void buttonRun_Click(object sender, EventArgs e)
        {
            textBoxOutput.Clear();
            string param = " /r " + textBox1.Text + " /s SRV*" + textBox2.Text + "*http://msdl.microsoft.com/download/symbols";

            CommandPrompt commandPrompt = new CommandPrompt("symchk", param);
            commandPrompt.Exited += OnSymChk_Exited;
            commandPrompt.OutputDataReceived += commandPrompt_DataReceived;
            commandPrompt.ErrorDataReceived += commandPrompt_DataReceived;
            commandPrompt.TimeOut = 1000000;
            labelStatus.Text = "SymChk is running...";
            commandPrompt.Run();


        }
        
        void OnSymChk_Exited(object sender, ExitedEventArgs e)
		{
			labelStatus.Invoke((Action)(() => labelStatus.Text = "SymChk has finished executing."));

			if(e.ExitCode == 0)
			{
				DirectoryInfo di = new DirectoryInfo(textBox2.Text);
				WalkDirectory(di);
                System.IO.File.WriteAllText(@"D:\output.txt", outputdata.ToString());

			}
		}
        public void WalkDirectory(System.IO.DirectoryInfo root)
        {

            string[] allFiles = System.IO.Directory.GetFiles(root.FullName, "*.pdb", System.IO.SearchOption.AllDirectories);
            
                foreach (string filename in allFiles)
                {
                    // operation for each PDB file
                    string pdbsig = readpdbsignature(filename);
                    // function finds windows version from pdbsignature 
                    string version = matchDLL(pdbsig);
                    outputdata.Append("  " + filename + "  " + pdbsig);
                    outputdata.AppendLine(" ");

                    string param = filename;
                    int len = param.Length - 4;
                    param = param.Substring(0, len);    
                    string tempname = string.Concat(param, ".txt");
                    param = " -p " + filename;
                    CommandPrompt commandPrompt = new CommandPrompt("Dia2Dump", param);
                    commandPrompt.Exited += Dia2Dump_Exited;
                    commandPrompt.OutputDataReceived += Dia2Dump_DataReceived;
                    commandPrompt.ErrorDataReceived += commandPrompt_DataReceived;
                    commandPrompt.TimeOut = 1000000;
                    labelStatus.Invoke((Action)(() => labelStatus.Text = "Dia2Dump is running..."));
                    commandPrompt.Run();
                    //string output = commandPrompt.StandardOutput.ReadToEnd();
                    //textBoxOutput.AppendText(output);
                    //rawdata.Append(output);
                    //HookSearch();
                    //textBox3.Text = outputdata.ToString();
                }
        }
        void Dia2Dump_DataReceived(object sender, DataEventArgs e)
        {
            textBoxOutput.Invoke((Action)(() => textBoxOutput.AppendText(e.Data + Environment.NewLine)));
        }
        void Dia2Dump_Exited(object sender, EventArgs e)
        {
            labelStatus.Invoke((Action)(() => labelStatus.Text = "Dia2Dump has finished executing."));
            CommandPrompt o = sender as CommandPrompt;
            if (o != null)
            {
                string output = o.StandardOutput;
              //  textBoxOutput.AppendText(output);
                HookSearch(output);

            }
        }
        public void HookSearch(string input)
        {
            string[] lines = input.Split(new Char[] { '\n' });

            foreach (string line in lines)
            {
               // if (filterfunction(line) == 1)
                //{
                    if (line.IndexOf("PublicSymbol:") == -1)
                        continue;
                    // PublicSymbol: [01234567][0123:01234567] DecoratedName(UndecoratedName)
                    int decoratedNameEnd = line.IndexOf('(');
                    if (decoratedNameEnd == -1)
                        decoratedNameEnd = line.Length;
                    decoratedNameEnd--;
                    int decoratedNameStart = line.IndexOf("] ") + 2;
                    int decoratedNameLength = decoratedNameEnd - decoratedNameStart + 1;
                    string decoratedSymbolName = line.Substring(decoratedNameStart, decoratedNameLength);

                    int rvaStart = line.IndexOf('[') + 1;
                    string rvaString = line.Substring(rvaStart, 8);
                    int rva = Convert.ToInt32(rvaString, 16);
                    string linetoadd = decoratedSymbolName + "  RVA:" + rvaString;
                    outputdata.Append(linetoadd);
                    outputdata.AppendLine(" ");
                //}
                
            }
            outputdata.AppendLine(" ");
            outputdata.AppendLine(" ");
        }
        
        public string readpdbsignature(string filename)
        {
            string param = " " + filename + " info";
            CommandPrompt commandPrompt = new CommandPrompt("dbh", param);
           // commandPrompt.Exited += commandPrompt_Exited;
         //   commandPrompt.OutputDataReceived += commandPrompt_DataReceived;
            commandPrompt.ErrorDataReceived += commandPrompt_DataReceived;
            commandPrompt.TimeOut = 1000000;

            commandPrompt.Run();
            commandPrompt.Wait();
            string output = commandPrompt.StandardOutput;
            int start = output.IndexOf("PdbSig70");
            if (start == -1)
                return "NotFound";
            start = start + 11;
            int end = output.IndexOf("PdbAge");
            int length = end - start;
            string pdbsignature = output.Substring(start, length);
            return pdbsignature;

        }
        public string matchDLL(string pdbsig)
        {
            DirectoryInfo di = new DirectoryInfo(textBox1.Text);
            System.IO.DirectoryInfo root = di;
            string version=null;
            string[] allFiles = System.IO.Directory.GetFiles(root.FullName, "*.dll", System.IO.SearchOption.AllDirectories);
            foreach (string filename in allFiles)
            {
                // operation for each DLL file
                string dllsig = readpdbsignature(filename);
                if (pdbsig == dllsig)
                {
                    FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(filename);
                    version = myFileVersionInfo.ToString();
                    version = DLLWinversion(version);
                    outputdata.AppendLine(filename + "   " + version);
                }
             }
            return version;
        
        }
        public string DLLWinversion(string input)
        {
            // need logic to differentiate between server and client versions 
            // need logic for bitness
            int start = input.IndexOf("ProductVersion:");
            int end = input.IndexOf("Debug:");
            string sub = input.Substring(start, end-start);
            int delimeter = sub.IndexOf('.');
            string majorversion = sub.Substring(delimeter-1, 1);
            string minorversion = sub.Substring(delimeter + 1, 1);
            string buildnumber = sub.Substring(delimeter + 3, 4);
            string revision = sub.Substring(delimeter + 8, 5);
            StringBuilder versionname = new StringBuilder(); 
            versionname.AppendLine(sub);
            if (majorversion == "6")
            {
                if (minorversion == "3")
                    versionname.Append("Windows 8.1 ");
                if(minorversion == "2")
                    versionname.Append("Windows 8 ");
                if (minorversion == "1")
                    versionname.Append("Windows 7 ");
                if (minorversion == "0")
                    versionname.Append("Windows Vista ");
            }
            if (majorversion == "5")
            {
                if (minorversion == "2")
                    versionname.Append("Windows XP x64 ");
                if (minorversion == "1")
                    versionname.Append("Windows XP ");
                if (minorversion == "1")
                    versionname.Append("Windows 7 ");
            }
            // first digit of revision identifies service pack
            if (revision[0] == '2')
                versionname.Append("Service Pack 2 ");
            if (revision[0] == '1')
                versionname.Append("Service Pack 1 ");
            if (revision[0] == '3')
                versionname.Append("Service Pack 3 ");
            return versionname.ToString();
          
        }
        /* 

         {File:             D:\DLLs\kernel32.dll
 InternalName:     kernel32
 OriginalFilename: kernel32
 FileVersion:      6.1.7601.17932 (win7sp1_gdr.120820-0419)
 FileDescription:  Windows NT BASE API Client DLL
 Product:          Microsoft® Windows® Operating System
 ProductVersion:   6.1.7601.17932
 Debug:            False
 Patched:          False
 PreRelease:       False
 PrivateBuild:     False
 SpecialBuild:     False
 Language:         English (United States)
 }

        
        
         public void filterfunction()
         {
             ArrayList listWords = new ArrayList();
             listWords.Add("AppendMenuA");
             listWords.Add("AppendMenuW");
             listWords.Add("NtUserBeginPaint");
             listWords.Add("CheckMenuItem");
             listWords.Add("CheckMenuRadioItem");
             listWords.Add("DrawTextA");
             listWords.Add("DrawTextExA");
             listWords.Add("DrawTextExW");
             listWords.Add("EnableMenuItem");
             listWords.Add("FillRect");
             listWords.Add("EnableMenuItem");
             listWords.Add("NtUserGetAncestor");
             listWords.Add("NtUserGetCursorInfo");
             listWords.Add("GetCursorPos");
             listWords.Add("NtUserGetDC");
             listWords.Add("NtUserGetDCEx");
             listWords.Add("NtUserGetForegroundWindow");
             listWords.Add("GetKeyState");
             listWords.Add("GetMessageA");
             listWords.Add("GetMessagePos");
             listWords.Add("GetMessageTime");
             listWords.Add("GetMessageW");
             listWords.Add("GetUpdateRect");
             listWords.Add("GetUpdateRgn");
             listWords.Add("NtUserGetWindowDC");
             listWords.Add("GetWindowLongA");
             listWords.Add("GetWindowLongPtrA");
             listWords.Add("GetWindowLongPtrW");
             listWords.Add("GetWindowLongW");
             listWords.Add("GetWindowRect");
             listWords.Add("InsertMenuItemA");
             listWords.Add("InsertMenuItemW");
             listWords.Add("NtUserInvalidateRgn");
             listWords.Add("NtUserInvalidateRect");
             listWords.Add("IsWindowVisible");
             listWords.Add("PeekMessageA");
             listWords.Add("PeekMessageW");
             listWords.Add("SetForegroundWindow");
             listWords.Add("SetMenu");
             listWords.Add("SetMenuItemInfoA");
             listWords.Add("SetMenuItemInfoW");
             listWords.Add("NtUserSetParent");
             listWords.Add("SetWindowLongA");
             listWords.Add("SetWindowLongPtrA");
             listWords.Add("SetWindowLongPtrW");
             listWords.Add("SetWindowLongW");
             listWords.Add("NtUserSetWindowPlacement");
             listWords.Add("NtUserSetWindowPos");
             listWords.Add("NtUserTrackPopupMenuEx");
             listWords.Add("NtUserSetWindowPos");
             listWords.Add("NtUserWindowFromPoint");
             listWords.Add(" LdrpLoadDll(Ldrp");
             listWords.Add("NtMapViewOfSection@40");
             listWords.Add("LoadLibraryExW");
             listWords.Add("FreeLibrary@4");
             listWords.Add("SetConsoleActiveScreenBuffer");
             listWords.Add("CreateProcessInternalW");
             listWords.Add("BitBlt");
             listWords.Add("CreateCompatibleDC");
             listWords.Add("CreateDCA");
             listWords.Add("CreateDCW");
             listWords.Add("CreateFontIndirectW");
             listWords.Add("CreateFontW");
             listWords.Add("DeleteDC");
             listWords.Add("ExtTextOutA");
             listWords.Add("FillRgn");
             listWords.Add("GetClipBox");
             listWords.Add("PaintRgn");
             listWords.Add("PolyTextOutA");
             listWords.Add("PolyTextOutW");
             listWords.Add("TextOutA");
             listWords.Add("TextOutW");
             string[] arrayWords = (string[])listWords.ToArray(typeof(string));
             IStringSearchAlgorithm searchAlg = new StringSearch();
             searchAlg.Keywords = arrayWords;
             StringSearchResult[] results = searchAlg.FindAll(rawdata.ToString());
             string input = rawdata.ToString();

         }
         */
        public int filterfunction(string input)
        {
            int rtnvariable = 0;
            if (input.Contains("AppendMenuA"))
                rtnvariable = 1;
            if (input.Contains("AppendMenuW"))
                rtnvariable = 1;
            if (input.Contains("NtUserBeginPaint"))
                rtnvariable = 1;
            if (input.Contains("CheckMenuItem"))
                rtnvariable = 1;
            if (input.Contains("CheckMenuRadioItem"))
                rtnvariable = 1;
            if (input.Contains("NtUserDeferWindowPos"))
                rtnvariable = 1;
            if (input.Contains("DrawTextExA"))
                rtnvariable = 1;
            if (input.Contains("DrawTextExW"))
                rtnvariable = 1;
            if (input.Contains("DrawTextA"))
                rtnvariable = 1;
            if (input.Contains("DrawTextW"))
                rtnvariable = 1;
            if (input.Contains("EnableMenuItem"))
                rtnvariable = 1;
             if (input.Contains("FillRect"))
                rtnvariable = 1;
             if (input.Contains("NtUserEndPaint"))
                 rtnvariable = 1;
             if (input.Contains("NtUserGetAncestor"))
                rtnvariable = 1;
             if (input.Contains("NtUserGetCursorInfo"))
                rtnvariable = 1;
             if (input.Contains("GetCursorPos"))
                rtnvariable = 1;
             if (input.Contains("NtUserGetDC"))
                rtnvariable = 1;
             if (input.Contains("NtUserGetDCEx"))
                rtnvariable = 1;
             if (input.Contains("NtUserGetForegroundWindow"))
                rtnvariable = 1;
            if (input.Contains("GetKeyState"))
                rtnvariable = 1;
            if (input.Contains("GetMessageA"))
                rtnvariable = 1;
            if (input.Contains("GetMessageW"))
                rtnvariable = 1;
            if (input.Contains("GetMessagePos"))
                rtnvariable = 1;
            if (input.Contains("GetMessageTime"))
                rtnvariable = 1;
            if (input.Contains("GetUpdateRect"))
                rtnvariable = 1;
            if (input.Contains("GetUpdateRgn"))
                rtnvariable = 1;
            if (input.Contains("NtUserGetWindowDC"))
                rtnvariable = 1;
            if (input.Contains("GetWindowLongA"))
                rtnvariable = 1;
            if (input.Contains("GetWindowLongPtrA"))
                rtnvariable = 1;
            if (input.Contains("GetWindowLongPtrW"))
                rtnvariable = 1;
            if (input.Contains("GetWindowLongW"))
                rtnvariable = 1;
            if (input.Contains("GetWindowRect"))
                rtnvariable = 1;
            if (input.Contains("InsertMenuItemA"))
                rtnvariable = 1;
            if (input.Contains("InsertMenuItemW"))
                rtnvariable = 1;
            if (input.Contains("NtUserInvalidateRgn"))
                rtnvariable = 1;
            if (input.Contains("NtUserInvalidateRect"))
                rtnvariable = 1;
            if (input.Contains("IsIconic"))
                rtnvariable = 1;
            if (input.Contains("IsWindowVisible"))
                rtnvariable = 1;
            if (input.Contains("PeekMessageA"))
                rtnvariable = 1;
            if (input.Contains("PeekMessageW"))
                rtnvariable = 1;
            if (input.Contains("ModifyMenuA"))
                rtnvariable = 1;
            if (input.Contains("ModifyMenuW"))
                rtnvariable = 1;
            if (input.Contains("SetForegroundWindow"))
                rtnvariable = 1;
            if (input.Contains("SetMenu"))
                rtnvariable = 1;
            if (input.Contains("SetMenuItemInfoA"))
                rtnvariable = 1;
            if (input.Contains("SetMenuItemInfoW"))
                rtnvariable = 1;
            if (input.Contains("NtUserSetParent"))
                rtnvariable = 1;
            if (input.Contains("SetWindowLongA"))
                rtnvariable = 1;
            if (input.Contains("SetWindowLongPtrA"))
                rtnvariable = 1;
            if (input.Contains("SetWindowLongPtrW"))
                rtnvariable = 1;
            if (input.Contains("SetWindowLongW"))
                rtnvariable = 1;
            if (input.Contains("NtUserSetWindowPlacement"))
                rtnvariable = 1;
            if (input.Contains("NtUserSetWindowPos"))
                rtnvariable = 1;
            if (input.Contains("VerNtUserCreateWindowEx"))
                rtnvariable = 1;
            if (input.Contains("NtUserTrackPopupMenuEx"))
                rtnvariable = 1;
            if (input.Contains("NtUserSetWindowPos"))
                rtnvariable = 1;
            if (input.Contains("NtUserWindowFromPoint"))
                rtnvariable = 1;
            if (input.Contains(" LdrpLoadDll(Ldrp"))
                rtnvariable = 1;
            if (input.Contains("NtMapViewOfSection"))
                rtnvariable = 1;
            if (input.Contains("LoadLibraryExW"))
                rtnvariable = 1;
            if (input.Contains("FreeLibrary"))
                rtnvariable = 1;
            if (input.Contains("SetConsoleActiveScreenBuffer"))
                rtnvariable = 1;
            if (input.Contains("CreateProcessInternalW"))
                rtnvariable = 1;
            if (input.Contains("BitBlt"))
                rtnvariable = 1;
            if (input.Contains("CreateCompatibleDC"))
                rtnvariable = 1;
            if (input.Contains("CreateDCA"))
                rtnvariable = 1;
            if (input.Contains("CreateDCW"))
                rtnvariable = 1;
            if (input.Contains("CreateFontIndirectW"))
                rtnvariable = 1;
            if (input.Contains("CreateFontW"))
                rtnvariable = 1;
            if (input.Contains("DeleteDC"))
                rtnvariable = 1;
            if (input.Contains("ExtTextOutA"))
                rtnvariable = 1;
            if (input.Contains("FillRgn"))
                rtnvariable = 1;
            if (input.Contains("GetClipBox"))
                rtnvariable = 1;
            if (input.Contains("PaintRgn"))
                rtnvariable = 1;            
            if (input.Contains("PolyTextOutA"))
                rtnvariable = 1;
            if (input.Contains("PolyTextOutW"))
                rtnvariable = 1;
            if (input.Contains("TextOutA"))
                rtnvariable = 1;
            if (input.Contains("TextOutW"))
                rtnvariable = 1;
            return rtnvariable;
        }

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
        

        public interface IStringSearchAlgorithm
        {
            #region Methods & Properties

            /// <summary>
            /// List of keywords to search for
            /// </summary>
            string[] Keywords { get; set; }


            /// <summary>
            /// Searches passed text and returns all occurrences of any keyword
            /// </summary>
            /// <param name="text">Text to search</param>
            /// <returns>Array of occurrences</returns>
            StringSearchResult[] FindAll(string text);

            /// <summary>
            /// Searches passed text and returns first occurrence of any keyword
            /// </summary>
            /// <param name="text">Text to search</param>
            /// <returns>First occurrence of any keyword (or StringSearchResult.Empty if text doesn't contain any keyword)</returns>
            StringSearchResult FindFirst(string text);

            /// <summary>
            /// Searches passed text and returns true if text contains any keyword
            /// </summary>
            /// <param name="text">Text to search</param>
            /// <returns>True when text contains any keyword</returns>
            bool ContainsAny(string text);

            #endregion
        }

        /// <summary>
        /// Structure containing results of search 
        /// (keyword and position in original text)
        /// </summary>
        public struct StringSearchResult
        {
            #region Members

            private int _index;
            private string _keyword;

            /// <summary>
            /// Initialize string search result
            /// </summary>
            /// <param name="index">Index in text</param>
            /// <param name="keyword">Found keyword</param>
            public StringSearchResult(int index, string keyword)
            {
                _index = index; _keyword = keyword;
            }


            /// <summary>
            /// Returns index of found keyword in original text
            /// </summary>
            public int Index
            {
                get { return _index; }
            }


            /// <summary>
            /// Returns keyword found by this result
            /// </summary>
            public string Keyword
            {
                get { return _keyword; }
            }


            /// <summary>
            /// Returns empty search result
            /// </summary>
            public static StringSearchResult Empty
            {
                get { return new StringSearchResult(-1, ""); }
            }

            #endregion
        }


        /// <summary>
        /// Class for searching string for one or multiple 
        /// keywords using efficient Aho-Corasick search algorithm
        /// </summary>
        public class StringSearch : IStringSearchAlgorithm
        {
            #region Objects

            /// <summary>
            /// Tree node representing character and its 
            /// transition and failure function
            /// </summary>
            class TreeNode
            {
                #region Constructor & Methods

                /// <summary>
                /// Initialize tree node with specified character
                /// </summary>
                /// <param name="parent">Parent node</param>
                /// <param name="c">Character</param>
                public TreeNode(TreeNode parent, char c)
                {
                    _char = c; _parent = parent;
                    _results = new ArrayList();
                    _resultsAr = new string[] { };
                   
                    _transitionsAr = new TreeNode[] { };
                    _transHash = new Hashtable();
                }


                /// <summary>
                /// Adds pattern ending in this node
                /// </summary>
                /// <param name="result">Pattern</param>
                public void AddResult(string result)
                {
                    if (_results.Contains(result)) return;
                    _results.Add(result);
                    _resultsAr = (string[])_results.ToArray(typeof(string));
                }

                /// <summary>
                /// Adds trabsition node
                /// </summary>
                /// <param name="node">Node</param>
                public void AddTransition(TreeNode node)
                {
                    _transHash.Add(node.Char, node);
                    TreeNode[] ar = new TreeNode[_transHash.Values.Count];
                    _transHash.Values.CopyTo(ar, 0);
                    _transitionsAr = ar;
                }


                /// <summary>
                /// Returns transition to specified character (if exists)
                /// </summary>
                /// <param name="c">Character</param>
                /// <returns>Returns TreeNode or null</returns>
                public TreeNode GetTransition(char c)
                {
                    return (TreeNode)_transHash[c];
                }


                /// <summary>
                /// Returns true if node contains transition to specified character
                /// </summary>
                /// <param name="c">Character</param>
                /// <returns>True if transition exists</returns>
                public bool ContainsTransition(char c)
                {
                    return GetTransition(c) != null;
                }

                #endregion
                #region Properties

                private char _char;
                private TreeNode _parent;
                private TreeNode _failure;
                private ArrayList _results;
                private TreeNode[] _transitionsAr;
                private string[] _resultsAr;
                private Hashtable _transHash;

                /// <summary>
                /// Character
                /// </summary>
                public char Char
                {
                    get { return _char; }
                }


                /// <summary>
                /// Parent tree node
                /// </summary>
                public TreeNode Parent
                {
                    get { return _parent; }
                }


                /// <summary>
                /// Failure function - descendant node
                /// </summary>
                public TreeNode Failure
                {
                    get { return _failure; }
                    set { _failure = value; }
                }


                /// <summary>
                /// Transition function - list of descendant nodes
                /// </summary>
                public TreeNode[] Transitions
                {
                    get { return _transitionsAr; }
                }


                /// <summary>
                /// Returns list of patterns ending by this letter
                /// </summary>
                public string[] Results
                {
                    get { return _resultsAr; }
                }

                #endregion
            }

            #endregion
            #region Local fields

            /// <summary>
            /// Root of keyword tree
            /// </summary>
            private TreeNode _root;

            /// <summary>
            /// Keywords to search for
            /// </summary>
            private string[] _keywords;

            #endregion

            #region Initialization

            /// <summary>
            /// Initialize search algorithm (Build keyword tree)
            /// </summary>
            /// <param name="keywords">Keywords to search for</param>
            public StringSearch(string[] keywords)
            {
                Keywords = keywords;
            }


            /// <summary>
            /// Initialize search algorithm with no keywords
            /// (Use Keywords property)
            /// </summary>
            public StringSearch()
            { }

            #endregion
            #region Implementation

            /// <summary>
            /// Build tree from specified keywords
            /// </summary>
            void BuildTree()
            {
                // Build keyword tree and transition function
                _root = new TreeNode(null, ' ');
                foreach (string p in _keywords)
                {
                    // add pattern to tree
                    TreeNode nd = _root;
                    foreach (char c in p)
                    {
                        TreeNode ndNew = null;
                        foreach (TreeNode trans in nd.Transitions)
                            if (trans.Char == c) { ndNew = trans; break; }

                        if (ndNew == null)
                        {
                            ndNew = new TreeNode(nd, c);
                            nd.AddTransition(ndNew);
                        }
                        nd = ndNew;
                    }
                    nd.AddResult(p);
                }

                // Find failure functions
                ArrayList nodes = new ArrayList();
                // level 1 nodes - fail to root node
                foreach (TreeNode nd in _root.Transitions)
                {
                    nd.Failure = _root;
                    foreach (TreeNode trans in nd.Transitions) nodes.Add(trans);
                }
                // other nodes - using BFS
                while (nodes.Count != 0)
                {
                    ArrayList newNodes = new ArrayList();
                    foreach (TreeNode nd in nodes)
                    {
                        TreeNode r = nd.Parent.Failure;
                        char c = nd.Char;

                        while (r != null && !r.ContainsTransition(c)) r = r.Failure;
                        if (r == null)
                            nd.Failure = _root;
                        else
                        {
                            nd.Failure = r.GetTransition(c);
                            foreach (string result in nd.Failure.Results)
                                nd.AddResult(result);
                        }

                        // add child nodes to BFS list 
                        foreach (TreeNode child in nd.Transitions)
                            newNodes.Add(child);
                    }
                    nodes = newNodes;
                }
                _root.Failure = _root;
            }


            #endregion
            #region Methods & Properties

            /// <summary>
            /// Keywords to search for (setting this property is slow, because
            /// it requieres rebuilding of keyword tree)
            /// </summary>
            public string[] Keywords
            {
                get { return _keywords; }
                set
                {
                    _keywords = value;
                    BuildTree();
                }
            }


            /// <summary>
            /// Searches passed text and returns all occurrences of any keyword
            /// </summary>
            /// <param name="text">Text to search</param>
            /// <returns>Array of occurrences</returns>
            public StringSearchResult[] FindAll(string text)
            {
                ArrayList ret = new ArrayList();
                TreeNode ptr = _root;
                int index = 0;

                while (index < text.Length)
                {
                    TreeNode trans = null;
                    while (trans == null)
                    {
                        trans = ptr.GetTransition(text[index]);
                        if (ptr == _root) break;
                        if (trans == null) ptr = ptr.Failure;
                    }
                    if (trans != null) ptr = trans;

                    foreach (string found in ptr.Results)
                        ret.Add(new StringSearchResult(index - found.Length + 1, found));
                    index++;
                }
                return (StringSearchResult[])ret.ToArray(typeof(StringSearchResult));
            }


            /// <summary>
            /// Searches passed text and returns first occurrence of any keyword
            /// </summary>
            /// <param name="text">Text to search</param>
            /// <returns>First occurrence of any keyword (or StringSearchResult.Empty if text doesn't contain any keyword)</returns>
            public StringSearchResult FindFirst(string text)
            {
                ArrayList ret = new ArrayList();
                TreeNode ptr = _root;
                int index = 0;

                while (index < text.Length)
                {
                    TreeNode trans = null;
                    while (trans == null)
                    {
                        trans = ptr.GetTransition(text[index]);
                        if (ptr == _root) break;
                        if (trans == null) ptr = ptr.Failure;
                    }
                    if (trans != null) ptr = trans;

                    foreach (string found in ptr.Results)
                        return new StringSearchResult(index - found.Length + 1, found);
                    index++;
                }
                return StringSearchResult.Empty;
            }


            /// <summary>
            /// Searches passed text and returns true if text contains any keyword
            /// </summary>
            /// <param name="text">Text to search</param>
            /// <returns>True when text contains any keyword</returns>
            public bool ContainsAny(string text)
            {
                TreeNode ptr = _root;
                int index = 0;

                while (index < text.Length)
                {
                    TreeNode trans = null;
                    while (trans == null)
                    {
                        trans = ptr.GetTransition(text[index]);
                        if (ptr == _root) break;
                        if (trans == null) ptr = ptr.Failure;
                    }
                    if (trans != null) ptr = trans;

                    if (ptr.Results.Length > 0) return true;
                    index++;
                }
                return false;
            }

            #endregion
        }

		private void textBox1_TextChanged(object sender, EventArgs e)
		{

		}
       

    }
}
