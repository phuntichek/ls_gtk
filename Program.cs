using System;
using System.Threading;
using Gtk;


namespace gtk_ls
{
    
    class MainClass
    {
        static public TreeStore store;
        static public Window window;
        static public string regex = "*";

        public static void Main(string[] args)
        {
            Application.Init();
            Window window = new Window("Editor Window");

            window.SetPosition(WindowPosition.Center);
            window.HeightRequest = 800;
            window.WidthRequest = 1200;

            TreeView view = new TreeView();

            view.WidthRequest = 500;

            TreeViewColumn column = new TreeViewColumn();
            column.Title = "Heirarchy";

            CellRendererText cell = new CellRendererText();

            column.PackStart(cell, true);

            view.AppendColumn(column);

            column.AddAttribute(cell, "text", 0);

            store = new TreeStore(typeof(string));
            TreeIter treeIter = store.AppendValues("SearchResult");
            DirFinder mainDF = new DirFinder("/Users/phuntik/Projects/", "net_regex_ls", treeIter);

            //on Button Search click
            mainDF.fillThreaded();

            //on Button Stop click
            // DirFinder.currentThread.Abort()

            view.ShowExpanders = true;

            view.Model = store;
            view.ShowExpanders = true;

            window.Add(view);

            //window.DeleteEvent += ExitWindow;

            window.ShowAll();

            Application.Run();
        }
    }
}
