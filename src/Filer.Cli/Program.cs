using CommandLine;
using CsvHelper;
using Filer.Cli.Verbs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Filer.Cli
{
    class Program
    {
        const int _exifDateTakenId = 0x9003;
        const string _exifDateFormat = "yyyy:MM:dd HH:mm:ss\0";

        static async Task<int> Main(string[] args)
        {
            try
            {
                return await Parser.Default.ParseArguments<ConcatVerb, DelimitVerb, ShiftDateTakenVerb>(args).MapResult(
                    (ConcatVerb o) => Concat(o),
                    (DelimitVerb o) => CsvDelim(o),
                    (ShiftDateTakenVerb o) => ShiftDateTaken(o),
                    OnParseError);
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(ex.ToString());
                return 2;
            }
        }

        private static Task<int> OnParseError(IEnumerable<Error> _)
        {
            return Task.FromResult(1);
        }

        private static async Task<int> ShiftDateTaken(ShiftDateTakenVerb options)
        {
            var files = Globber.GlobFiles(Environment.CurrentDirectory, options.GlobPatterns.ToArray());
            var utf8 = System.Text.Encoding.UTF8;
            var errors = 0;

            foreach (var file in files)
            {
                try
                {
                    var filePath = Path.Combine(Environment.CurrentDirectory, file);
                    using var buffer = new MemoryStream();
                    using (var fileStream = File.OpenRead(filePath))
                    {
                        await fileStream.CopyToAsync(buffer);
                    }
                    buffer.Position = 0;
                    using var image = Image.FromStream(buffer);
                    var dateTakenProperty = image.GetPropertyItem(_exifDateTakenId);
                    var dateTakenString = utf8.GetString(dateTakenProperty.Value);
                    var dateTaken = DateTime.ParseExact(dateTakenString, _exifDateFormat, null);
                    var newDateTaken = dateTaken
                        .AddYears(options.ShiftYears)
                        .AddMonths(options.ShiftMonths)
                        .AddDays(options.ShiftDays)
                        .AddHours(options.ShiftHours)
                        .AddMinutes(options.ShiftMinutes)
                        .AddSeconds(options.ShiftSeconds);
                    dateTakenProperty.Value = utf8.GetBytes(newDateTaken.ToString(_exifDateFormat, CultureInfo.InvariantCulture));
                    image.SetPropertyItem(dateTakenProperty);
                    image.Save(filePath);
                }
                catch (Exception ex)
                {
                    await Console.Error.WriteLineAsync(string.Format("Could not process file {0}: {1}", file, ex.Message));
                    errors++;
                }
            }

            return errors;
        }

        private static async Task<int> CsvDelim(DelimitVerb options)
        {
            ApplyEncoding(options);
            var currentDelim = ParseSpecialChars(options.CurrentDelimiter);
            var newDelim = ParseSpecialChars(options.NewDelimiter);
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
            var sep = string.Empty;
            var head = string.Empty;

            if (options.Separator != null) sep = ParseSpecialChars(options.Separator);
            if (options.Header != null) head = ParseSpecialChars(options.Header);
            var writer = Console.Out;
            var files = Globber.GlobFiles(Environment.CurrentDirectory, options.GlobPatterns.ToArray());
            var first = true;
            foreach (var file in files.OrderBy(fn => fn, StringComparer.OrdinalIgnoreCase))
            {
                var filePath = Path.Combine(Environment.CurrentDirectory, file);
                try
                {
                    using var reader = new StreamReader(filePath);
                    if (!first) await writer.WriteAsync(sep);
                    first = false;
                    await writer.WriteAsync(string.Format(head, file, filePath));
                    var buffer = new char[1024 * 10];
                    var charsRead = 0;
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
            Console.OutputEncoding = int.TryParse(outVerb.OutputEncoding, out var codePage)
                ? System.Text.Encoding.GetEncoding(codePage)
                : System.Text.Encoding.GetEncoding(outVerb.OutputEncoding);
    }
}
