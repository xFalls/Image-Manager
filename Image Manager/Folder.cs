using System;
using System.Collections.Generic;
using System.IO;

namespace Image_Manager
{
    public class Folder
    {
        private string folderName;
        private string folderPath;
        private bool isRoot;
        private Folder parentFolder;
        private List<Folder> childFolders = new List<Folder>();

        private static List<Folder> allFolders = new List<Folder>();
        private static List<Folder> shownFolders = new List<Folder>();
        private static int currentFolderDepth = 0;
        private int thisFolderDepth;


        public Folder(string folderPath)
        {
            this.folderPath = folderPath;
            folderName = new DirectoryInfo(folderPath).Name;
            if (!folderPath.Contains("_"))
            {
                allFolders.Add(this);
                shownFolders.Add(this);
            }

            thisFolderDepth = currentFolderDepth;
            currentFolderDepth++;
            CreateFolderStructure();
            currentFolderDepth--;
        }

        // Create folder structure
        private void CreateFolderStructure()
        {
            foreach (string foundFolder in Directory.GetDirectories(folderPath, "*", SearchOption.TopDirectoryOnly))
            {
                // Exlude folders started with an underscore
                if (Path.GetDirectoryName(foundFolder).Contains("_")) continue;

                Folder child = new Folder(foundFolder);
                child.SetParentFolder(this);
                AddChildFolder(child);
            }
        }

        public int GetFolderDepth()
        {
            return thisFolderDepth;
        }

        public List<Folder> GetAllFolders()
        {
            return allFolders;
        }

        public List<Folder> GetAllShownFolders()
        {
            return shownFolders;
        }

        public void RemoveAllShownFolders()
        {
            shownFolders.Clear();
        }

        public string GetFolderName()
        {
            return folderName;
        }

        public string GetFolderPath()
        {
            return folderPath;
        }

        public void SetParentFolder(Folder folder)
        {
            this.parentFolder = folder;
        }

        public Folder GetParentFolder()
        {
            return parentFolder;
        }

        public void AddChildFolder(Folder folder)
        {
            childFolders.Add(folder);
        }

        public List<Folder> GetChildFolders()
        {
            return childFolders;
        }

        public void SetRoot()
        {
            isRoot = true;
        }

        public bool GetRoot()
        {
            return isRoot;
        }


    }
}