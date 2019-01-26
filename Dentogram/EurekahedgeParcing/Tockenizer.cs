using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dentogram.EurekahedgeParcing
{
    public class Tockenizer<TTokenType>
    {
        private readonly List<TokenDefinition> tokenDefinitions = new List<TokenDefinition>();

        public void AddTokenDefinition(TTokenType returnsToken, string regexPattern, int precedence = 1)
        {
            tokenDefinitions.Add(new TokenDefinition(returnsToken, regexPattern, precedence));
        }
            
        public IEnumerable<TokenMatch> Tokenize(string text)
        {
            IEnumerable<TokenMatch> tokenMatches = FindTokenMatches(text);

            IEnumerable<IGrouping<int, TokenMatch>> groupedByIndex = tokenMatches
                .GroupBy(x => x.StartIndex)
                .OrderBy(x => x.Key)
                .ToList();

            TokenMatch lastMatch = null;
            foreach (IGrouping<int, TokenMatch> group in groupedByIndex)
            {
                var bestMatch = group.OrderBy(x => x.Precedence).First();
                if (lastMatch != null && bestMatch.StartIndex < lastMatch.EndIndex)
                    continue;

                yield return bestMatch;

                lastMatch = bestMatch;
            }
        }

        private IEnumerable<TokenMatch> FindTokenMatches(string text)
        {
            return tokenDefinitions.SelectMany(x => x.FindMatches(text));
        }

        private class TokenDefinition
        {
            private readonly Regex regex;
            private readonly TTokenType tokenType;
            private readonly int precedence;

            public TokenDefinition(TTokenType tokenType, string regexPattern, int precedence)
            {
                regex = new Regex(regexPattern, RegexOptions.IgnoreCase|RegexOptions.Compiled);
                this.tokenType = tokenType;
                this.precedence = precedence;
            }

            public IEnumerable<TokenMatch> FindMatches(string inputString)
            {
                var matches = regex.Matches(inputString);

                foreach (Match match in matches)
                {
                    yield return new TokenMatch
                    {
                        OriginalMatch = match,
                        TokenType = tokenType,
                        Precedence = precedence,
                    };
                }
            }
        }

        public class TokenMatch
        {
            public TTokenType TokenType { get; set; }
            public int Precedence { get; set; }
            
            public Match OriginalMatch { get; set; }
            public int StartIndex => OriginalMatch.Index;
            public int EndIndex => OriginalMatch.Index + OriginalMatch.Length;
            public string Value => OriginalMatch.Value;
        }
    }
}