using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Filer.Cli
{
    public class Globber
    {
        /// <summary>
        /// Returns all file paths in the specified <paramref name="directory"/> and its subdirectories that match the specified glob patterns.
        /// </summary>
        /// <param name="directory">Directory in which to look for files</param>
        /// <param name="patterns">Glob patterns that files must match. Patterns are negated if prefixed with an exclamation mark (!)</param>
        /// <returns>an <see cref="IEnumerable{T}"/> yielding all file paths that match the specified glob patterns</returns>
        public static IEnumerable<string> GlobFiles(string directory, params string[] patterns) =>
            GlobFiles(ListFiles(directory), patterns);

        /// <summary>
        /// Returns all paths that match the specified glob patterns.
        /// </summary>
        /// <param name="relativePaths">Paths that are relative to the root of directory tree</param>
        /// <param name="patterns">Glob patterns that files must match. Patterns are negated if prefixed with an exclamation mark (!)</param>
        /// <returns>An <see cref="IEnumerable{T}"/> yielding the subset of paths in <paramref name="relativePaths"/> that match the specified glob patterns</returns>
        public static IEnumerable<string> GlobFiles(IEnumerable<string> relativePaths, params string[] patterns)
        {
            var pat = patterns
                .Select(p => new
                {
                    Negate = p.StartsWith("!"),
                    Glob = DotNet.Globbing.Glob.Parse((p.StartsWith("!") ? p.Substring(1) : p).ToLowerInvariant())
                })
                .ToArray();

            // paths must match at least one positive pattern, but NO negative patterns:
            var isMatch = new Predicate<string>(path =>
            {
                var isNegativeMatch = pat.Where(p => p.Negate).Any(p => p.Glob.IsMatch(path));
                var isPositiveMatch = pat.Where(p => !p.Negate).Any(p => p.Glob.IsMatch(path));
                return !isNegativeMatch && isPositiveMatch;
            });

            return relativePaths.Where(f => isMatch(f.ToLowerInvariant()));
        }

        static IEnumerable<string> ListFiles(string root)
        {
            var rootDir = new DirectoryInfo(root);
            var queue = new Queue<string>(new[] { root });
            while (queue.Count > 0)
            {
                var currentPath = queue.Dequeue(); // get current directory

                // return all files in current directory
                foreach (var file in Directory.GetFiles(currentPath))
                {
                    // remove start/of/path/, +1 to trim ending slash, too
                    var relativePath = file.Substring(rootDir.FullName.Length).TrimStart('/', '\\');
                    yield return relativePath;
                }

                foreach (var subDir in Directory.GetDirectories(currentPath))
                {
                    queue.Enqueue(subDir);
                }
            }
        }
    }
}
