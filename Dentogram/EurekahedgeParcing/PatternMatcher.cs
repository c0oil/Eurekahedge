using System;
using System.Collections.Generic;
using System.Linq;

namespace Dentogram.EurekahedgeParcing
{
    public class PatternMatcher<TTokenType>
    {
        private readonly List<TTokenType[]> patternDefinitions = new List<TTokenType[]>();

        public void AddPattern(TTokenType[] pattern)
        {
            patternDefinitions.Add(pattern);
        }

        public IEnumerable<PatternMatch> Match(TTokenType[] text)
        {
            IEnumerable<PatternMatch> tokenMatches = FindMatches(text);

            IEnumerable<IGrouping<int, PatternMatch>> groupedByIndex = tokenMatches
                .GroupBy(x => x.StartIndex)
                .OrderBy(x => x.Key);

            PatternMatch lastMatch = null;
            foreach (IGrouping<int, PatternMatch> group in groupedByIndex)
            {
                var bestMatch = group.First();
                if (lastMatch != null && bestMatch.StartIndex < lastMatch.EndIndex)
                    continue;

                yield return bestMatch;

                lastMatch = bestMatch;
            }
        }

        private IEnumerable<PatternMatch> FindMatches(TTokenType[] text)
        {
            return patternDefinitions.SelectMany(pattern => FindMatches(text, pattern));
        }
        
        private IEnumerable<PatternMatch> FindMatches(TTokenType[] source, TTokenType[] pattern)
        {
            for (int i = 0; i + pattern.Length <= source.Length; i++)
            {
                if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                {
                    yield return new PatternMatch
                    {
                        StartIndex = i,
                        EndIndex = i + pattern.Length,
                        Value = pattern,
                    };
                }
            }
        }

        public class PatternMatch
        {
            public int StartIndex { get; set; }
            public int EndIndex { get; set; }
            public TTokenType[] Value { get; set; }
        }
    }
}