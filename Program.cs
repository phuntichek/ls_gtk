using System;
using System.IO;
using System.Threading;
using Gtk;

namespace gtk_ls
{
    
    class MainClass
    {
        //some global variables
        static public Label searchDirCurrent;
        static public Label searchFileLast;
        static public Label searchTimeLabel;

        static public bool isSearching = false;
        static public bool isPaused = false;

        static DateTime lastUpdateTime;
        static TimeSpan timeSpan;

        static public Window window;
        static public DirFinder mainDF;
        static public TreeStore store;

        static public string regex = "*";
        static public string dirEntry = "/Users/phuntik/Projects/net_regex_ls";
        public static string dirEntryParent = null;
        public static string dirEntryName = null;

        public static string searchFileLastStr = "Last file: ";
        public static string searchDirCurStr = "Searching dir: ";

        public static Gtk.Entry dir;
        public static Gtk.Entry reg;

        public static TreeIter treeIter = TreeIter.Zero;

        static public VBox vb = new VBox(false, 3);
        static public HBox hb = new HBox();
        static public HBox hbinfo = new HBox();

        public static Button start;
        public static Button stop;
        public static Button pause;
        public static TreeView view;

        static public Thread currentThread;

        public static EventWaitHandle waitHandle = new ManualResetEvent(initialState: true);

        public static void Main(string[] args)
        {
            //start window
            timeSpan = TimeSpan.FromSeconds(0);
            Application.Init();
            window = new Window("Editor Window");
            window.SetPosition(WindowPosition.Center);
            window.HeightRequest = 600;
            window.WidthRequest = 900;

            //new TreeView
            view = new TreeView();
            view.WidthRequest = 900;

            TreeViewColumn column = new TreeViewColumn();
            column.Title = "Please, hire me!!";

            //init labels and buttons
            searchTimeLabel = new Label("Search time: ");
            searchFileLast = new Label("Last file: ".PadRight(60));
            searchFileLast.SetAlignment(0, 0.65f);

            searchDirCurrent = new Label("Searching dir: ".PadRight(60));
            searchDirCurrent.SetAlignment(0.1f, 0.65f);

            start = new Button("Start");
            start.Clicked += new EventHandler(Start);
            pause = new Button("Pause");
            pause.Sensitive = false;
            pause.Clicked += new EventHandler(Pause);
            stop = new Button("Stop");
            stop.Sensitive = false;
            stop.Clicked += new EventHandler(Stop);
            dir = new Entry("enter the directory");
            reg = new Entry("enter the regex");
            ScrolledWindow scroller = new ScrolledWindow();
            scroller.BorderWidth = 5;
            scroller.ShadowType = ShadowType.In;

            CellRendererText cell = new CellRendererText();

            column.PackStart(cell, true);

            view.AppendColumn(column);

            column.AddAttribute(cell, "text", 0);

            store = new TreeStore(typeof(string));
            TreeIter treeIter = store.AppendValues("SearchResult");

            view.Model = store;
            view.ShowExpanders = true;

            //set labels and buttons
            hb.PackStart(start, false, false, 5);
            hb.PackStart(pause, false, false, 5);
            hb.PackStart(stop, false, false, 5);
            hb.Add(dir);
            hb.Add(reg);
            searchTimeLabel.SetAlignment(0.2f, 0.65f);
            hb.Add(searchTimeLabel);

            hbinfo.Add(searchDirCurrent);
            hbinfo.Add(searchFileLast);

            vb.PackStart(hb, false, false, 5);
            vb.PackStart(hbinfo, false, false, 5);

            scroller.Add(view);
            vb.Add(scroller);
            GLib.Timeout.Add(1, updateTimeLabel);
            GLib.Timeout.Add(1, updateLastFileLabel);
            GLib.Timeout.Add(1, updateCurrentDir);

            window.Add(vb);

            window.Destroyed += new EventHandler(onClosed);
            
            window.ShowAll();

            //show all items in window
            Application.Run();
        }

        //evnt handler for start button
        static void Start(object obj, EventArgs e)
        {
            waitHandle.Set();
            if (!isSearching)
            {
                try
                {
                    FileAttributes attr = File.GetAttributes(dir.Text);

                    if (attr.HasFlag(FileAttributes.Directory) == false)
                        return ;
                }
                catch (Exception ex)
                {
                    return ;
                }
                
                if (reg.Text == "")
                    return ;
                dirEntry = dir.Text;
                regex = reg.Text;
                store = new TreeStore(typeof(string));
                view.Model = store;
                
                dirEntryName = Path.GetFileName(dirEntry);
                dirEntryParent = Path.GetDirectoryName(dirEntry);

                treeIter = store.AppendValues(dirEntryName);

                mainDF = new DirFinder(dirEntryParent, dirEntryName, treeIter);

                timeSpan = TimeSpan.FromSeconds(0);

                isSearching = true;
                isPaused = false;

                stop.Sensitive = true;
                start.Sensitive = false;
                pause.Sensitive = true;
                dir.Sensitive = false;
                reg.Sensitive = false;


                ThreadStart starter = new ThreadStart(mainDF.fill_data);
                starter += () =>
                {
                    onSearchEnd();
                };
                currentThread = new Thread(starter) { IsBackground = true };
                currentThread.Start();
            }
            else
            {
                pause.Sensitive = true;
                start.Sensitive = false;
                isPaused = false;
            }
        }

        //event handler for stop button
        static void Stop(object obj, EventArgs e)
        {
            if (currentThread != null)
                currentThread.Abort();
            onSearchEnd();
        }

        //event handler for pause button
        static void Pause(object obj, EventArgs e)
        {
            start.Label = "Resume";
            isPaused = true;
            waitHandle.Reset();
            pause.Sensitive = false;
            start.Sensitive = true;
        }

        //updating time supporting pause
        public static bool updateTimeLabel()
        {
            if (isSearching && !isPaused)
            {
                TimeSpan ts = DateTime.Now.Subtract(lastUpdateTime);
                timeSpan += ts;
                searchTimeLabel.Text = "Search time: " + timeSpan.ToString();
            }
            lastUpdateTime = DateTime.Now;
            return true;
        }

        //set value of last checked file
        public static bool updateLastFileLabel()
        {
            if (isSearching)
            {
                searchFileLast.Text = searchFileLastStr;
            }
            return true;
        }

        //set value of last checked directory
        public static bool updateCurrentDir()
        {
            if (isSearching)
            {
                searchDirCurrent.Text = searchDirCurStr;
            }
            return true;
        }

        //what to do when searching is end
        public static void onSearchEnd()
        {
            isSearching = false;
            isPaused = false;

            start.Sensitive = true;
            pause.Sensitive = false;
            stop.Sensitive = false;

            dir.Sensitive = true;
            reg.Sensitive = true;

            waitHandle.Reset();

            currentThread = null;
            updateCurrentDir();
            updateTimeLabel();
            updateLastFileLabel();
            start.Label = "Start";
        }

        //label of current directory
        public static void setCurDir(string dir)
        {
            string text = "Searching dir: " + dir;
            searchDirCurStr = text;
        }

        //label of current file
        public static void setCurFile(string file)
        {
            string text = "Last file: " + file;
            searchFileLastStr = text;
        }

        //stop all threads on close button
        private static void onClosed(object o, EventArgs args)
        {
            Application.Quit();
        }
    }
}
