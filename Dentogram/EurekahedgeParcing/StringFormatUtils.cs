using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Dentogram.EurekahedgeParcing
{
    public static class StringFormatUtils
    {
        public static string ReplaceMatches<T>(string inText, IEnumerable<T> matches, Func<T, string> formatWord, Func<T, Group> getGroup)
        {
            StringBuilder outText = new StringBuilder();
            Action<int, int> tryAppendText = (start, length) =>
            {
                if (length > 0)
                    outText.Append(inText.Substring(start, length));
            };

            EnumerateMatches(matches, 
                (s, l) => tryAppendText(s, l), 
                (s, l, info) => outText.Append(formatWord(info)),
                info => getGroup(info).Index,
                info => getGroup(info).Length,
                inText.Length);

            return outText.ToString();
        }

        public static void EnumerateMatches<T>(IEnumerable<T> matches,
            Action<int, int> doNoMatched, Action<int, int, T> doMatched, 
            Func<T, int> getStart, Func<T, int> getLength,
            int endPosition)
        {
            int currPosition = 0;
            foreach (T info in matches)
            {
                int start = getStart(info);
                int length = getLength(info);
                if (start - currPosition > 0)
                {
                    doNoMatched(currPosition, start - currPosition);
                }
                if (length > 0)
                {
                    doMatched(start, length, info);
                }
                currPosition = start + length;
            }
            doNoMatched(currPosition, endPosition - currPosition);
        }
    }
}