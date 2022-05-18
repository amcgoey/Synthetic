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
        /// <summary>
        /// List of Paths to be searched.
        /// </summary>
        public List<string> Paths { get; set; }

        /// <summary>
        /// List of files to search for.
        /// </summary>
        public List<string> Files { get; set; }

        /// <summary>
        /// Dictionary keyed by file name of the files that have been found.
        /// </summary>
        public Dictionary<string, string> FilesFound { get; set; }
        
        /// <summary>
        /// List of files not found at any of the paths.
        /// </summary>
        public List<string> FilesMissing { get; set; }

        /// <summary>
        /// Creates a new search path object with empty filesFound and filesMissing.  Run the Search function.
        /// </summary>
        /// <param name="Paths">List of Paths to be searched.</param>
        /// <param name="Files">List of Files to search for.</param>
        public SearchPaths(List<string> Paths, List<string> Files)
        {
            this.Paths = Paths;
            this.Files = Files;
            this.FilesFound = new Dictionary<string, string>();
            this.FilesMissing = new List<string>();

            this.Search();
        }

        /// <summary>
        /// Execute the search of files acorss all paths.
        /// </summary>
        private void Search()
        {
            List<string> filesToSearch = this.Files;

            foreach (string path in this.Paths)
            {
                this.FilesMissing = new List<string>();
                List<string> filesAll = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).ToList();                

                foreach (string file in filesToSearch)
                {
                    string filePath = null;
                    filePath = filesAll.Where(f => f.EndsWith(file)).FirstOrDefault();
                    if (filePath != null)
                    {
                        this.FilesFound.Add(file, filePath);
                    }
                    else
                    {
                        this.FilesMissing.Add(file);
                    }
                }

                filesToSearch = this.FilesMissing;
            }

            this.FilesMissing = filesToSearch;
        }

        /// <summary>
        /// Given a Dictionary KeyValuePair with the file name and path, FilePath will return a string of the full filename including the path
        /// </summary>
        /// <param name="File">A Dictionary KeyValuePair with the file name as the key and the path as the value.</param>
        /// <returns name="FilePath">Returns a string of the full filename including the path.</returns>
        public static string FilePath(SearchPaths SearchPath, string Filename)
        {
            string fileFullName = null;
            if (SearchPath.FilesFound.ContainsKey(Filename))
            {
                fileFullName = SearchPath.FilesFound[Filename] + Filename;
            }
            return fileFullName;
        }
    }
}
