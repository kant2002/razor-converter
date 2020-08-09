using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace aspx2razor {

    /// <summary>
    /// An object that will handle the traversing of directories and collection of files
    /// </summary>
    public class DirectoryHandler
    {
        private readonly string inputDirectory;
        private readonly string outputDirectory;
        private readonly IEnumerable<string> extensionFilter;

        /// <summary>
        /// Initializes a new DirectoryHandler instance
        /// </summary>
        /// <param name="inputDirectory">The initial directory to start inspections at</param>
        /// <param name="outputDirectory">The output directory to output to</param>
        /// <param name="extensionFilter">File extensions in format '.aspx', '.ascx'. Default - both format.</param>
        public DirectoryHandler(string inputDirectory, string outputDirectory, IEnumerable<string> extensionFilter)
        {
            this.inputDirectory = GetFullPathOrDefault(inputDirectory);
            this.outputDirectory = string.IsNullOrEmpty(outputDirectory) ? this.inputDirectory : Path.GetFullPath(outputDirectory);
            this.extensionFilter = extensionFilter;
        }

        public IEnumerable<string> GetFiles(bool includeSubdirectories) {
            return GetFiles(inputDirectory, includeSubdirectories);
        }

        public string GetOutputFileName(string fileName) {
            var fullFileName = Path.GetFullPath(fileName);
            var relative = MakeRelative(fullFileName, inputDirectory);
            return Path.Combine(outputDirectory, relative);
        }

        private static string MakeRelative(string filePath, string referencePath)
        {
            var fileUri = new Uri(filePath);
            var referenceUri = new Uri(referencePath);
            return referenceUri.MakeRelativeUri(fileUri).ToString();
        }

        private List<string> GetFiles(string inputDirectory, bool includeSubdirectories) {
            var files = GetFileRecursive(new List<string>(), inputDirectory, includeSubdirectories);
            return files;
        }

        private List<string> GetFileRecursive(List<string> list, string directoryPath, bool recursive) {
            var directory = new DirectoryInfo(directoryPath);
            var files = directory.GetFiles().Where(file => extensionFilter.Contains(file.Extension));

            list.AddRange(files.Select(file => file.FullName));

            if(recursive) {
                var subDirectories = directory.GetDirectories();
                foreach(var subDirectory in subDirectories) {
                    GetFileRecursive(list, subDirectory.FullName, recursive);
                }
            }
            return list;
        }

        private static string GetFullPathOrDefault(string directory) {
            if(!Directory.Exists(directory)) {
                directory = Path.GetDirectoryName(directory);
            }

            if(string.IsNullOrEmpty(directory)) {
                directory = Directory.GetCurrentDirectory();
            }

            return Path.GetFullPath(directory);
        }
    }
}
