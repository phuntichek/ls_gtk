using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Gtk;

namespace gtk_ls
{


    public class DirFinder
    {
        static public Thread currentThread;

        string dir_path;
        string dir_name;
        string[] files = new string[0];
        string[] subdirs = new string[0];
        TreeIter treeIter;
        List<DirFinder> subDirFinders = new List<DirFinder>();

        public DirFinder(string parentDir, string name, TreeIter parentTreeIter)
        {
            dir_path = parentDir + "/" + name;
            dir_name = name;
            treeIter = MainClass.store.AppendValues(parentTreeIter, dir_name);
        }

        public void fillThreaded()
        {
            ThreadStart starter = new ThreadStart(this.fill);
            starter += () => {
                // Do what you want in the callback
                onFillComplete();
            };
            currentThread = new Thread(starter) { IsBackground = true };
            currentThread.Start();
        }

        public void fill()
        {
            

            try
            {
                files = Directory.GetFiles(dir_path, MainClass.regex);
            }
            catch (Exception e)
            {

            }
            try
            {
                subdirs = Directory.GetDirectories(dir_path);
            }
            catch (Exception e)
            {
                
            }
            if (subdirs != null)
            {
                for (int i = 0; i < subdirs.Length; i++)
                {
                    subdirs[i] = subdirs[i].Substring(dir_path.Length + 1);
                }
                foreach (string dir in subdirs)
                {
                    subDirFinders.Add(new DirFinder(dir_path, dir, treeIter));
                }
            }
            if (files != null)
            {
                for (int i = 0; i < files.Length; i++)
                {
                    files[i] = files[i].Substring(dir_path.Length + 1);
                    // call GUI to add file
                    //MainClass.store.AppendValues(treeIter, files[i]);
                }
            }
        }

        //public void fillRecursive(string regex)
        //{
        //    fill(regex);
        //    foreach (DirFinder df in subDirFinders)
        //    {
        //        df.fillRecursive(regex);
        //    }
        //}

        public void onFillComplete()
        {
            if (files != null)
            {
                foreach (string file in files)
                {
                    MainClass.store.AppendValues(treeIter, file);
                }
            }
            foreach (DirFinder df in subDirFinders)
            {
                df.fillThreaded();
            }
        }
    }
}
