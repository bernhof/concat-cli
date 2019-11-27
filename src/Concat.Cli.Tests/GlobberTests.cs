using System;
using System.Linq;
using Xunit;

namespace Concat.Cli.Tests
{
    public class GlobberTests
    {
        [Fact]
        public void Negates_patterns_with_exclamation_mark()
        {
            var paths = new[]
            {
                "pick me.sql",
                "not me.sql"
            };
            var actual = Globber.GlobFiles(paths, "*.sql", "!not me.sql").ToList();
            Assert.Equal(new[] { "pick me.sql" }, actual);
        }
    }
}
