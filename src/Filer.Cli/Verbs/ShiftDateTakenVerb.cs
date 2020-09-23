using CommandLine;
using System.Collections.Generic;

namespace Filer.Cli.Verbs
{
    [Verb("shift-date-taken", HelpText = "Shifts Exif \"Date Taken\" property on image files")]
    class ShiftDateTakenVerb
    {
        /// <summary>
        /// Glob patterns the specify which files to include
        /// </summary>
        [Value(0, HelpText = "Glob patterns that specify files to concatenate. Prefix a pattern with ! to exclude files.", Required = true)]
        public IEnumerable<string> GlobPatterns { get; set; }

        /// <summary>
        /// Number of years to shift date
        /// </summary>
        [Option('y', "shift-year", Required = false, Default = 0, HelpText = "Number of years to shift date")]
        public int ShiftYears { get; set; } = 0;

        /// <summary>
        /// Number of months to shift date
        /// </summary>
        [Option('m', "shift-months", Required = false, Default = 0, HelpText = "Number of months to shift date")]
        public int ShiftMonths { get; set; } = 0;

        /// <summary>
        /// Number of months to shift date
        /// </summary>
        [Option('d', "shift-days", Required = false, Default = 0, HelpText = "Number of days to shift date")]
        public double ShiftDays { get; internal set; }

        /// <summary>
        /// Number of hours to shift date
        /// </summary>
        [Option('h', "shift-hours", Required = false, Default = 0, HelpText = "Number of hours to shift date")]
        public double ShiftHours { get; internal set; }

        /// <summary>
        /// Number of minutes to shift date
        /// </summary>
        [Option('n', "shift-minutes", Required = false, Default = 0, HelpText = "Number of minutes to shift date")]
        public double ShiftMinutes { get; internal set; }

        /// <summary>
        /// Number of seconds to shift date
        /// </summary>
        [Option('s', "shift-seconds", Required = false, Default = 0, HelpText = "Number of seconds to shift date")]
        public double ShiftSeconds { get; internal set; }
    }
}
