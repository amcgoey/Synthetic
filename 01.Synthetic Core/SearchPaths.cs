using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Synthetic.Core
{
    /// <summary>
    /// SearchPaths are utility functions that search recursively through a list of paths for a list of files and provides the paths of the found files and a list of files not found.
    /// </summary>
    public class SearchPaths
    {
        private List<string> paths;
        private Dictionary<string, string> fileLibrary;

        /// <summary>
        /// List of Paths to be searched.
        /// </summary>
        public List<string> Paths 
        {
            get { return this.paths; }
            set
            {
                this.paths = value;
                this.fileLibrary = this._GetFiles(value);
            }
        }

        /// <summary>
        /// List of files in the search path.  Only one unique file name in all the paths is included.
        /// </summary>
        public Dictionary<string, string> FileLibrary { get { return fileLibrary; } }

        /// <summary>
        /// Creates a new search path object with a library of all the unique files and their paths.  Only the first instance of a filename is included so the order of the Paths give priority.
        /// </summary>
        /// <param name="Paths">List of Paths to be searched.</param>
        public SearchPaths(List<string> Paths)
        {
            this.Paths = Paths;
            this.fileLibrary = this._GetFiles(this.Paths);
        }

        /// <summary>
        /// Searches the paths for a file and returns if path if found, otherwise returns null.
        /// </summary>
        /// <param name="File">Name of the file to search for.</param>
        /// <returns name="FilePath">A string of the file path if found.  If the file is not found, it returns null.</returns>
        public string GetFilePath (string File)
        {
            if (File != null && this.fileLibrary.ContainsKey(File))
                return this.fileLibrary[File];
            else
                return null;
        }

        /// <summary>
        /// Searches the paths for a file and returns a relative path if found, otherwise returns null.
        /// </summary>
        /// <param name="File">A string that is the path to the a file in the search paths</param>
        /// <returns name="RelativePath">A string of the relative file path if found.  If the file is not found, it returns null.  The relative path removes the search path from the FilePath</returns>
        public string GetRelativeFilePath(string File)
        {
            string relativePath = null;

            if (File != null && this.fileLibrary.ContainsKey(File))
            {
                string FilePath = this.fileLibrary[File];

                foreach (string path in this.Paths)
                {
                    if (File.Contains(path))
                    {
                        relativePath = File.Replace(File, path);
                    }
                }
            }
            return relativePath;
        }

        /// <summary>
        /// Gets all the files in the search paths.  If more than one file has the same name, the only the first file found will be included.  This gives priority to paths listed first in the SearchPaths.
        /// </summary>
        /// <returns>A Dictionary with the file name as the key and the path as the value.</returns>
        private Dictionary<string,string> _GetFiles(List<string> Paths)
        {
            Dictionary<string, string> files = new Dictionary<string, string>();

            foreach (string path in Paths)
            {

                if (Directory.Exists(path))
                {
                    List<string> filesAll = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).ToList();

                    foreach (string filepath in filesAll)
                    {
                        if (!files.ContainsKey(Path.GetFileName(filepath)))
                        {
                            files.Add(Path.GetFileName(filepath), filepath);
                        }
                    }
                }
            }
            return files;
        }

        /// <summary>
        /// Prints the Searchpaths as a string including all the files in the library.
        /// </summary>
        /// <returns name="string">Converts to a string.</returns>
        public override string ToString()
        {
            Type t = typeof(SearchPaths);

            string s = t.Namespace + "." + GetType().Name;

            int i = 0;
            foreach (KeyValuePair<string, string> file in this.fileLibrary)
            {
                s = s + string.Format("\n  {0} file-> \"{1}\" path-> \"{2}\"", i, file.Key, file.Value);
                i++;
            }
            return s;
        }
    }
}
