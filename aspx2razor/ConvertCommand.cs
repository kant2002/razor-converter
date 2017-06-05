using System;
using System.Collections.Generic;
using System.Linq;
using ManyConsole;

namespace aspx2razor
{
    public class ConvertCommand : ConsoleCommand
    {
        private const int Success = 0;
        private const int Failure = 2;

        public bool ConvertRecursively { get; set; }
        public string InputDirectory { get; set; }
        public string OutputDirectory { get; set; }

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
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                DirectoryHandler directoryHandler;
                try
                {
                    directoryHandler = new DirectoryHandler(InputDirectory, OutputDirectory);
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