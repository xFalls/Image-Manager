using System.Collections.Generic;
using System.IO;

namespace Image_Manager
{
    /// <summary>
    /// A folder which makes up the folder structure used in the UI,
    /// allowing for files to be easily moved.
    /// </summary>
    public class Folder
    {
        private readonly string _folderName;
        private readonly string _folderPath;
        private bool _isRoot;
        private Folder _parentFolder;
        private readonly List<Folder> _childFolders = new List<Folder>();

        private static readonly List<Folder> AllFolders = new List<Folder>();
        private static readonly List<Folder> ShownFolders = new List<Folder>();
        private static int _currentFolderDepth;
        private readonly int _thisFolderDepth;

        /// <summary>
        /// Initializes all default values.
        /// </summary>
        /// <param name="folderPath">The path to the folder in the filesystem.</param>
        public Folder(string folderPath)
        {
            _folderPath = folderPath;
            _folderName = new DirectoryInfo(folderPath).Name;
            if (!folderPath.Contains("_"))
            {
                AllFolders.Add(this);
                ShownFolders.Add(this);
            }

            _thisFolderDepth = _currentFolderDepth;
            _currentFolderDepth++;
            CreateFolderStructure();
            _currentFolderDepth--;
        }

        // Create folder structure
        private void CreateFolderStructure()
        {
            foreach (string foundFolder in Directory.GetDirectories(_folderPath, "*", SearchOption.TopDirectoryOnly))
            {
                // Exlude folders started with an underscore
                if (Path.GetDirectoryName(foundFolder).Contains("_")) continue;

                Folder child = new Folder(foundFolder);
                child.SetParentFolder(this);
                AddChildFolder(child);
            }
        }

        /// <summary>
        /// Gets the how deep this folder is nested.
        /// </summary>
        /// <returns>The number of levels deep this folder is. The topmost folder is 0.</returns>
        public int GetFolderDepth()
        {
            return _thisFolderDepth;
        }

        /// <summary>
        /// Gets a list of all the folders that have been created.
        /// </summary>
        /// <returns>A list containing all folders.</returns>
        public List<Folder> GetAllFolders()
        {
            return AllFolders;
        }

        /// <summary>
        /// Gets a list of all folders currently being shown in the UI.
        /// </summary>
        /// <returns>A list containing all shown folders.</returns>
        public List<Folder> GetAllShownFolders()
        {
            return ShownFolders;
        }

        /// <summary>
        /// Removes all folders to be shown, making the UI empty.
        /// </summary>
        public void RemoveAllShownFolders()
        {
            ShownFolders.Clear();
        }

        /// <summary>
        /// Gets the name of this folder.
        /// </summary>
        /// <returns>Name of the folder as a string.</returns>
        public string GetFolderName()
        {
            return _folderName;
        }

        /// <summary>
        /// Gets the filepath to this folder.
        /// </summary>
        /// <returns>The entire path of the folder as a string.</returns>
        public string GetFolderPath()
        {
            return _folderPath;
        }

        /// <summary>
        /// Sets the relationship between this and another folder.
        /// </summary>
        /// <param name="folder">The folder to be set as this object's parent.</param>
        public void SetParentFolder(Folder folder)
        {
            _parentFolder = folder;
        }

        /// <summary>
        /// Gets the folder set as this folder's parent.
        /// </summary>
        /// <returns>A folder one step up in the folder hierarchy.</returns>
        public Folder GetParentFolder()
        {
            return _parentFolder;
        }

        /// <summary>
        /// Adds a folder as a member of this folder's children.
        /// </summary>
        /// <param name="folder">The child folder.</param>
        public void AddChildFolder(Folder folder)
        {
            _childFolders.Add(folder);
        }

        /// <summary>
        /// Gets a list of all children to this folder.
        /// </summary>
        /// <returns>A list containing all child folders.</returns>
        public List<Folder> GetChildFolders()
        {
            return _childFolders;
        }

        /// <summary>
        /// Sets whether this folder should act as the topmost initial folder to branch out from.
        /// </summary>
        public void SetRoot()
        {
            _isRoot = true;
        }

        /// <summary>
        /// Checks if this folder is the initial root folder.
        /// </summary>
        /// <returns>Gets whether this is the topmost folder</returns>
        public bool GetRoot()
        {
            return _isRoot;
        }


    }
}