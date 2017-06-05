using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Text;
using Telerik.RazorConverter;
using Telerik.RazorConverter.Razor.DOM;

namespace aspx2razor
{
    public class WebFormsToRazorConverter
    {
        public DirectoryHandler DirectoryHandler { get; }

        [Import]
        private IWebFormsParser Parser { get; set; }

        [Import]
        private IWebFormsConverter<IRazorNode> Converter { get; set; }

        [Import]
        private IRenderer<IRazorNode> Renderer { get; set; }

        public WebFormsToRazorConverter(DirectoryHandler directoryHandler)
        {
            DirectoryHandler = directoryHandler;
            var catalog = new AssemblyCatalog(typeof(IWebFormsParser).Assembly);
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }

        public (int successCount, IList<string> fails) Convert(IEnumerable<string> files)
        {
            int successCount = 0;
            var fails = new List<string>();

            foreach (var file in files)
            {
                Console.WriteLine("Converting {0}", file);

                try
                {
                    var webFormsPageSource = File.ReadAllText(file, Encoding.UTF8);
                    var webFormsDocument = Parser.Parse(webFormsPageSource);
                    var razorDom = Converter.Convert(webFormsDocument);
                    var razorPage = Renderer.Render(razorDom);

                    var outputFileName = ReplaceExtension(DirectoryHandler.GetOutputFileName(file), ".cshtml");
                    Console.WriteLine("Writing    {0}", outputFileName);
                    EnsureDirectory(Path.GetDirectoryName(outputFileName));
                    File.WriteAllText(outputFileName, razorPage, Encoding.UTF8);

                    Console.WriteLine("Done");
                    successCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception Thrown!");
                    AddFail(file, ex.Message, fails);
                }
                finally
                {
                    Console.WriteLine();
                }
            }
            return (successCount, fails);
        }

        private static void AddFail(string fileName, string errorMessage, IList<string> failList)
        {
            string fail = string.Format("- '{1}':{0} {2}", Environment.NewLine, fileName, errorMessage);
            failList.Add(fail);
        }

        private static void EnsureDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static string ReplaceExtension(string fileName, string newExtension)
        {
            var targetFolder = Path.GetDirectoryName(fileName);
            return Path.Combine(targetFolder, Path.GetFileNameWithoutExtension(fileName) + newExtension);
        }
    }
}