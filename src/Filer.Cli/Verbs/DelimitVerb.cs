using CommandLine;
using System.Collections.Generic;

namespace Filer.Cli.Verbs
{
    [Verb("delimit", HelpText = "Changes the delimiter in a CSV file")]
    class DelimitVerb : OutVerb
    {
        [Value(0, HelpText = "Name of CSV file to modify", Required = true)]
        public string FileName { get; set; }

        [Value(1, HelpText = "Current delimiter", Required = true)]
        public string CurrentDelimiter { get; set; }

        [Value(2, HelpText = "New delimiter", Required = true)]
        public string NewDelimiter { get; set; }
    }
}
