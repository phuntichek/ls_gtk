using System;
using System.Collections.Generic;
using System.IO;
using Gtk;

namespace gtk_ls
{


    public class DirFinder
    {
        //some global variables
        bool has_files = false;
        bool ui_updated = false;
        string dir_path;
        string dir_name;
        string[] files = new string[0];
        string[] subdirs = new string[0];
        public bool isRoot = false;
        TreeIter treeIter;
        List<DirFinder> subDirFinders = new List<DirFinder>();

        //add directory + parent directory to the tree
        public DirFinder(string parentDir, string name, TreeIter parentTreeIter)
        {
            dir_path = parentDir + "/" + name;
            dir_name = name;
            treeIter = parentTreeIter;
        }

        //fill the tree
        public void fill_data()
        {
            MainClass.waitHandle.WaitOne();
            MainClass.setCurDir(dir_name);
            treeIter = MainClass.store.AppendValues(treeIter, dir_name);
            try
            {
                files = Directory.GetFiles(dir_path, MainClass.regex);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            try
            {
                subdirs = Directory.GetDirectories(dir_path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            if (files != null)
            {
                if (files.Length > 0)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        //filling files in tree
                        files[i] = Path.GetFileName(files[i]);
                        MainClass.store.AppendValues(treeIter, files[i]);
                        if (i == files.Length - 1)
                            MainClass.setCurFile(files[i]);
                    }
                    has_files = true;
                }
                
            }
            if (subdirs != null)
            {
                //filling directories in tree 
                for (int i = 0; i < subdirs.Length; i++)
                {
                    subdirs[i] = Path.GetFileName(subdirs[i]);
                }
                foreach (string dir in subdirs)
                {
                    subDirFinders.Add(new DirFinder(dir_path, dir, treeIter));
                }
                foreach (DirFinder df in subDirFinders)
                {
                    df.fill_data();
                    if (df.has_files)
                        has_files = true;
                }
            }
            if (!has_files)
            {
                MainClass.store.Remove(ref treeIter);
            }
        }
    }
}
