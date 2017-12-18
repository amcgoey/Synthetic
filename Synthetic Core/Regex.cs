using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Autodesk.DesignScript.Runtime;

namespace Synthetic.Core
{
    /// <summary>
    /// Wrapper for System.Text.RegularExpressions
    /// </summary>
    public class RegEx
    {
        internal RegEx () {}

        /// <summary>
        /// Indicates whether the specified regular expression finds a match in the specified input string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        public static bool IsMatch (string input, string pattern)
        {
            return Regex.IsMatch(Regex.Escape(input), pattern);
        }

        /// <summary>
        /// Searches the specified input string for the first occurrence of the specified regular expression.
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="pattern">A regular expression pattern.</param>
        /// <returns name="Value">A substring that matches the given pattern.  Returns an empty string if the pattern wasn't matched.</returns>
        /// <returns name="Index">The index location where the pattern was matched.</returns>
        /// <returns name="IsMatch">True if a match was found.</returns>
        /// <returns name="subStrings">A list of substrings that match the capture groups of the pattern.</returns>
        [MultiReturn(new[] { "Value", "Index", "IsMatch", "subStrings" })]
        public static IDictionary Match (string input, string pattern)
        {
            Match match = Regex.Match(Regex.Escape(input), pattern);
            string value = Regex.Unescape(match.Value);
            int index = match.Index;
            bool isMatch = match.Success;
            
            List<string> subStrings = new List<string>();
            foreach (Capture capture in match.Captures)
            {
                subStrings.Add(capture.Value);
            }

            return new Dictionary<string, object>
            {
                {"Value", value},
                {"Index", index},
                {"IsMatch", isMatch},
                {"subStrings", subStrings }
            };
        }

        /// <summary>
        /// Searches the specified input string for all occurrences of a specified regular expression.
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="pattern">A regular expression pattern.</param>
        /// <returns name="Value">A substring that matches the given pattern.  Returns an empty string if the pattern wasn't matched.</returns>
        /// <returns name="Index">The index location where the pattern was matched.</returns>
        /// <returns name="IsMatch">True if a match was found.</returns>
        /// <returns name="subStrings">A list of substrings that match the capture groups of the pattern.</returns>
        [MultiReturn(new[] { "Value", "Index", "IsMatch", "subStrings" })]
        public static IDictionary Matches(string input, string pattern)
        {
            MatchCollection matches = Regex.Matches(Regex.Escape(input), pattern);

            List<string> values = new List<string>();
            List<int> indices = new List<int>();
            List<bool> isMatch = new List<bool>();
            List<List<string>> subStrings = new List<List<string>>();

            foreach (Match match in matches)
            {
                values.Add(Regex.Unescape(match.Value));
                indices.Add(match.Index);
                isMatch.Add(match.Success);

                List<string> s = new List<string>();
                foreach (Capture capture in match.Captures)
                {
                    s.Add(capture.Value);
                }
                subStrings.Add(s);
            }

            return new Dictionary<string, object>
            {
                {"Value", values},
                {"Index", indices},
                {"IsMatch", isMatch},
                {"subStrings", subStrings }
            };
        }

        /// <summary>
        /// In a specified input string, replaces all strings that match a specified regular expression with a specified replacement string.
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="pattern">A regular expression pattern.</param>
        /// <param name="replacement">A string to replace any matches to the pattern.</param>
        /// <returns name="string">The input string with portions replaced.  If the pattern doesn't match, the original string is returned.</returns>
        public static string Replace (string input, string pattern, string replacement)
        {
            return Regex.Unescape(
                Regex.Replace(
                    Regex.Escape(input),
                    pattern,
                    Regex.Escape(replacement)
                    )
                );
        }

        /// <summary>
        /// Splits an input string into an array of substrings at the positions defined by a regular expression pattern.
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="pattern">A regular expression pattern.</param>
        /// <returns name="split">A list of strings split by the pattern.</returns>
        public static List<string> Split (string input, string pattern)
        {
            List<string> splitStrings = new List<string>();
            string[] split = Regex.Split(Regex.Escape(input), pattern);

            foreach (string str in split)
            {
                splitStrings.Add(Regex.Unescape(str));
            }

            return splitStrings;
        }
    }
}
