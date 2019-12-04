using CommandLine;
using Filer.Cli.Verbs;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Filer.Cli
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                return await Parser.Default.ParseArguments<ConcatVerb, DelimitVerb>(args).MapResult(
                    (ConcatVerb o) => Concat(o),
                    (DelimitVerb o) => CsvDelim(o),
                    OnParseError);
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(ex.ToString());
                return 2;
            }
        }

        private static Task<int> OnParseError(IEnumerable<Error> arg)
        {
            return Task.FromResult(1);
        }

        private static async Task<int> CsvDelim(DelimitVerb options)
        {
            ApplyEncoding(options);
            string currentDelim = ParseSpecialChars(options.CurrentDelimiter);
            string newDelim = ParseSpecialChars(options.NewDelimiter);
            using var file = new StreamReader(options.FileName);
            using var csv = new CsvParser(file, new CsvHelper.Configuration.Configuration() { Delimiter = currentDelim });
            using var writer = Console.Out;
            using var csvOut = new CsvSerializer(writer, new CsvHelper.Configuration.Configuration() { Delimiter = newDelim });

            while (true)
            {
                var record = await csv.ReadAsync();
                if (record == null) break;
                await csvOut.WriteAsync(record);
                await csvOut.WriteLineAsync();
            }

            return 0;
        }

        private static async Task<int> Concat(ConcatVerb options)
        {
            ApplyEncoding(options);
            string sep = string.Empty;
            string head = string.Empty;

            if (options.Separator != null) sep = ParseSpecialChars(options.Separator);
            if (options.Header != null) head = ParseSpecialChars(options.Header);
            var writer = Console.Out;
            var files = Globber.GlobFiles(Environment.CurrentDirectory, options.GlobPatterns.ToArray());
            bool first = true;
            foreach (var file in files.OrderBy(fn => fn, StringComparer.OrdinalIgnoreCase))
            {
                string filePath = Path.Combine(Environment.CurrentDirectory, file);
                try
                {
                    using var reader = new StreamReader(filePath);
                    if (!first) await writer.WriteAsync(sep);
                    first = false;
                    await writer.WriteAsync(string.Format(head, file, filePath));
                    char[] buffer = new char[1024 * 10];
                    int charsRead = 0;
                    do
                    {
                        charsRead = await reader.ReadAsync(buffer, 0, buffer.Length);
                        await writer.WriteAsync(buffer, 0, charsRead);
                    } while (charsRead > 0);
                }
                catch (IOException ex)
                {
                    await Console.Error.WriteLineAsync(ex.Message);
                }
            }
            return 0;
        }

        static string ParseSpecialChars(string input)
        {
            input = Regex.Replace(input, @"(?<!\\)\\n", "\n");
            input = Regex.Replace(input, @"(?<!\\)\\t", "\t")
                .Replace("\\\\n", "\\n")
                .Replace("\\\\t", "\\t");
            return input;
        }

        static void ApplyEncoding(OutVerb outVerb) => 
            Console.OutputEncoding = int.TryParse(outVerb.EncodingName, out var codePage)
                ? System.Text.Encoding.GetEncoding(codePage)
                : System.Text.Encoding.GetEncoding(outVerb.EncodingName);
    }
}
