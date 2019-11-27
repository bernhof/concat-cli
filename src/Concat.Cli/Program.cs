using CommandLine;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Concat.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var parser = Parser.Default.ParseArguments<ConcatOptions>(args)
                .WithParsed((ConcatOptions options) =>
                {
                    string sep = string.Empty;
                    string head = string.Empty;
                    bool writeToFile = !string.IsNullOrEmpty(options.OutputFile);

                    if (options.Separator != null) sep = ApplyNewLines(options.Separator);
                    if (options.Header != null) head = ApplyNewLines(options.Header);

                    using var writer = writeToFile ? new StreamWriter(options.OutputFile) : Console.Out;
                    var files = Globber.GlobFiles(Environment.CurrentDirectory, options.GlobPatterns.ToArray());
                    bool first = true;
                    foreach (var file in files.OrderBy(fn => fn, StringComparer.OrdinalIgnoreCase))
                    {
                        string filePath = Path.Combine(Environment.CurrentDirectory, file);
                        try
                        {
                            using var reader = new StreamReader(filePath);
                            if (!first) writer.Write(sep);
                            first = false;
                            writer.Write(head, file, filePath);
                            char[] buffer = new char[1024 * 10];
                            int charsRead = 0;
                            do
                            {
                                charsRead = reader.Read(buffer, 0, buffer.Length);
                                writer.Write(buffer, 0, charsRead);
                            } while (charsRead > 0);
                        }
                        catch (IOException ex)
                        {
                            Console.Error.WriteLine(ex.Message);
                        }
                    }
                });
        }

        static string ApplyNewLines(string input) => 
            Regex
                .Replace(input, @"(?<!\\)\\n", "\n")
                .Replace("\\\\n", "\\n");
    }
}
