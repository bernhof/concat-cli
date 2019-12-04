using CommandLine;

namespace Filer.Cli.Verbs
{
    internal class OutVerb
    {
        [Option('e',"encoding", HelpText= "Specifies output encoding either by name or code page. Defaults to \"utf-8\".", Default = "utf-8")]
        public string OutputEncoding { get; set; }
    }
}