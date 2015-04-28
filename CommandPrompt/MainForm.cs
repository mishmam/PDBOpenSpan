using System;
using System.IO;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;
using System.Object;
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
            
            using(CommandPrompt commandPrompt = new CommandPrompt("symchk", param))
            {
                    commandPrompt.Exited += commandPrompt_Exited;
                    commandPrompt.OutputDataReceived += commandPrompt_DataReceived;
                    commandPrompt.ErrorDataReceived += commandPrompt_DataReceived;
                    commandPrompt.Run();
                    labelStatus.Text = "Command is running...";
                      
            }
             
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
                    param = " -p " + filename;
                    textBox3.Text = param;
                    textBox4.Text = filename;
                    CommandPrompt commandPrompt = new CommandPrompt("D:\\OpenSpan\\Dia2Dump.exe", param);
                    commandPrompt.Exited += Dia2Dump_Exited;
                    commandPrompt.OutputDataReceived += Dia2Dump_DataReceived;
                    commandPrompt.ErrorDataReceived += commandPrompt_DataReceived;
                    commandPrompt.BeginRun();
                    string output = commandPrompt.StandardOutput.ReadToEnd();
                    textBoxOutput.AppendText(output);
                    outputdata.Append(output);
                    
                 //   searchtext(tempname);
                }
            
        }
        public void HookSearch()
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
            listWords.Add("NtMapViewOfSection");
            listWords.Add("LoadLibraryExW");
            listWords.Add("FreeLibrary");
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
            StringSearchResult[] results = searchAlg.FindAll(textBoxOutput.ToString());
        }
        
        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        void Dia2Dump_DataReceived(object sender, DataEventArgs e)
        {
            textBoxOutput.Invoke((Action)(() => textBoxOutput.AppendText(e.Data + Environment.NewLine)));
        }
        void Dia2Dump_Exited(object sender, EventArgs e)
        {
            labelStatus.Invoke((Action)(() => labelStatus.Text = "Dia2Dump has finished executing."));

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
       

    }
}
