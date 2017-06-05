using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ManyConsole;
using Telerik.RazorConverter.Razor;

namespace aspx2razor
{
    public class ConvertCommand : ConsoleCommand
    {
        private const int Success = 0;
        private const int Failure = 2;

        private static readonly IEnumerable<string> DefaultExtensionFilter = new[]
        {
            ".aspx",
            ".ascx"
        };

        public bool ConvertRecursively { get; set; }
        public string InputDirectory { get; set; }
        public string OutputDirectory { get; set; }

        public string MasterFolderPath { get; set; } = "~/Views/Shared/Master";
        public string LayoutFolderPath { get; set; } = "~/Views/Shared/Layout";
        public string DefaultMasterName { get; set; } = "Default.Master";
        public string DefaultLayoutName { get; set; } = "DefaultLayout.cshtml";
        public string LayoutSuffix { get; set; } = "Layout";

        private IEnumerable<string> extensionsFilters;

        public IEnumerable<string> ExtensionsFilters
        {
            get => extensionsFilters ?? DefaultExtensionFilter;
            set => extensionsFilters = value;
        }

        public ConvertCommand()
        {
            IsCommand("convert", "the main commad to conver views");
            HasLongDescription("This tool converts WebForms pages (.aspx, .ascx) into a Razor views (.cshtml)");

            // Required options/flags, append '=' to obtain the required value.
            HasRequiredOption("i|input=", "Input Directory", e => InputDirectory = e);
            // Optional options/flags, append ':' to obtain an optional value, or null if not specified.
            HasOption(
                "o|output=",
                "Output Directory",
                e => OutputDirectory = e);
            HasOption(
                "r|recursively", 
                "Convert directories and their contents recursively", 
                e => ConvertRecursively = true);
            HasOption(
                "e|extensions=",
                @"Converted file's extensions in format "".aspx, .ascx"". By default - both formats.",
                e => ExtensionsFilters = e.Split(new[] {", "}, StringSplitOptions.RemoveEmptyEntries));

            HasOption(
                "mfp|masterFolderPath=",
                @"Master folder path. By default - ""~/Views/Shared/Master""",
                e => MasterFolderPath = e);
            HasOption(
                "lfp|layoutFolderPath=",
                @"Layout folder path. By default - ""~/Views/Shared/Layout""",
                e => LayoutFolderPath = e);
            HasOption(
                "dmn|defaultMasterName=",
                @"Default master file name. By default - ""Default.Master""",
                e => DefaultMasterName = e);
            HasOption(
                "dln|defaultLayoutName=",
                @"Layout folder path. By default - ""DefaultLayout.cshtml""",
                e => DefaultLayoutName = e);
            HasOption(
                "lp|layoutPrefix=",
                @"Layout prefix. Used for build name of Layouts. F.e. if layoutPrefix = ""Layout"" ""Default.Master"" will convert to ""DefaultLayout.cshtml"". By default - ""Layout""",
                e => LayoutSuffix = e);
        }

        public override int? OverrideAfterHandlingArgumentsBeforeRun(string[] remainingArguments)
        {
            TemplateSettings.CurrentSettings = new TemplateSettings(MasterFolderPath,
                LayoutFolderPath, DefaultMasterName, DefaultLayoutName, LayoutSuffix);
            return base.OverrideAfterHandlingArgumentsBeforeRun(remainingArguments);
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                DirectoryHandler directoryHandler;
                try
                {
                    directoryHandler = new DirectoryHandler(InputDirectory, OutputDirectory, ExtensionsFilters);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine("The given directories were not valid: {0}", ex.Message);
                    return -1;
                }

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                var files = directoryHandler.GetFiles(ConvertRecursively);
                var converter = new WebFormsToRazorConverter(directoryHandler);
                var convertResults = converter.Convert(files);

                Console.WriteLine();
                Console.WriteLine($"{convertResults.successCount} files converted");
                WriteFilesFailed(convertResults.fails);

                Console.WriteLine($"Elapsed: {stopwatch.Elapsed} seconds");
                return Success;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);

                return Failure;
            }
        }

        private static void WriteFilesFailed(IList<string> fails)
        {
            if (fails.Any())
            {
                Console.WriteLine();
                Console.WriteLine("{0} files failed:", fails.Count);
                foreach (var fail in fails)
                {
                    Console.WriteLine(fail);
                }
                Console.WriteLine();
            }
        }
    }
}