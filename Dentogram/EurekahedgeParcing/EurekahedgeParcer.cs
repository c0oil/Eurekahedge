using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dentogram.EurekahedgeParcing
{
    public enum LockupDurationPeriod
    {
        Quarters = 0,
        Months = 1,
        Weeks = 2,
        Days = 3
    }

    public enum LockupType
    {
        None = 0,
        Soft = 1,
        Hard = 2
    }

    public struct LockupValue
    {
        public double? LockupDuration { get; set; }
        public double? LockupFee { get; set; }
        public LockupDurationPeriod? LockupPeriod { get; set; }
        public LockupType? LockupType { get; set; }
        public bool Warning { get; set; }

        public override string ToString()
        {
            return $"Type:{LockupType}; Duration:{LockupDuration}; Period:{LockupPeriod}; Fee:{LockupFee};";
        }

        public string ToTestString()
        {
            return $"{LockupType}; {LockupDuration}; {LockupPeriod}; {LockupFee};";
        }
    }

    public class EurekahedgeLockupParcer
    {
        private class EurekahedgeLockupTockenizer : Tockenizer<EurekahedgeLockupTokenType> { }

        private enum EurekahedgeLockupTokenType
        {
            Undefined,

            Year,
            Quarter,
            Day,
            Week,
            Month,

            Yes,
            No,
            Soft,
            Hard,

            NumberValue,
            PercentValue,
            
            WordNumberValue,

            Space,
            Dash,
            Plus,
        }

        /*
        private List<Tuple<string, List<string>>> keyWords1 = new List<Tuple<string, List<string>>>
        {
            new Tuple<string, List<string>>("[y]", new List<string> { "calendar year", "years?", "2?yrs?", "annual" }),
            new Tuple<string, List<string>>("[q]", new List<string> { "quarter(?:ly|s)?" }),
            new Tuple<string, List<string>>("[m]", new List<string> { "calendar months", "month(?:ly|s)?", "moths?", "mths?",  "m" }),
            new Tuple<string, List<string>>("[w]", new List<string> { "weeks?" }),
            new Tuple<string, List<string>>("[d]", new List<string> { "days?" }),
            

            new Tuple<string, List<string>>("[no]", new List<string> { "no" }),
            new Tuple<string, List<string>>("[yes]", new List<string> { "yes" }),
            new Tuple<string, List<string>>("[soft]", new List<string> { "soft(?:[- ]lock(?:[- ]?up)?)?" }),
            new Tuple<string, List<string>>("[hard]", new List<string> { "hard(?:[- ]lock(?:[- ]?up)?)?" }),
            new Tuple<string, List<string>>("[num]", new List<string> { "one", "1st", "first" }),
            new Tuple<string, List<string>>("[num]", new List<string> { "two" }),
            new Tuple<string, List<string>>("[num][y]", new List<string> { "1yr" }),
            new Tuple<string, List<string>>("[empty]", new List<string> { "n/a", "none", "not disclosed", "undisclosed", "not applicable" }),
        };
        */

        private readonly HashSet<EurekahedgeLockupTokenType> numbers = 
            new HashSet<EurekahedgeLockupTokenType>(new[]
            {
                EurekahedgeLockupTokenType.NumberValue,
                EurekahedgeLockupTokenType.WordNumberValue,
            });

        private readonly HashSet<EurekahedgeLockupTokenType> period = 
            new HashSet<EurekahedgeLockupTokenType>(new[]
            {
                EurekahedgeLockupTokenType.Year,
                EurekahedgeLockupTokenType.Quarter,
                EurekahedgeLockupTokenType.Month,
                EurekahedgeLockupTokenType.Week,
                EurekahedgeLockupTokenType.Day,
            });

        private enum KeyWordOptions
        {
            None,
            Word,
        }

        private readonly List<Tuple<EurekahedgeLockupTokenType, string, KeyWordOptions>> keyWords = 
            new List<Tuple<EurekahedgeLockupTokenType, string, KeyWordOptions>>
            {
                new Tuple<EurekahedgeLockupTokenType, string, KeyWordOptions>(EurekahedgeLockupTokenType.Year, "calendar year|years?|yrs?|annual", KeyWordOptions.None ),
                new Tuple<EurekahedgeLockupTokenType, string, KeyWordOptions>(EurekahedgeLockupTokenType.Quarter, "quarter(?:ly|s)?", KeyWordOptions.None),
                new Tuple<EurekahedgeLockupTokenType, string, KeyWordOptions>(EurekahedgeLockupTokenType.Month, @"calendar months|month(?:ly|s)?|moths?|mths?|\bm\b", KeyWordOptions.None),
                new Tuple<EurekahedgeLockupTokenType, string, KeyWordOptions>(EurekahedgeLockupTokenType.Week, "weeks?", KeyWordOptions.None),
                new Tuple<EurekahedgeLockupTokenType, string, KeyWordOptions>(EurekahedgeLockupTokenType.Day, "days?", KeyWordOptions.None),

                new Tuple<EurekahedgeLockupTokenType, string, KeyWordOptions>(EurekahedgeLockupTokenType.No, "no", KeyWordOptions.Word),
                new Tuple<EurekahedgeLockupTokenType, string, KeyWordOptions>(EurekahedgeLockupTokenType.Yes, "yes", KeyWordOptions.Word),
                new Tuple<EurekahedgeLockupTokenType, string, KeyWordOptions>(EurekahedgeLockupTokenType.Soft,  "soft(?:[- ]lock(?:[- ]?up)?)?", KeyWordOptions.Word),
                new Tuple<EurekahedgeLockupTokenType, string, KeyWordOptions>(EurekahedgeLockupTokenType.Hard, "hard(?:[- ]lock(?:[- ]?up)?)?", KeyWordOptions.Word),
                new Tuple<EurekahedgeLockupTokenType, string, KeyWordOptions>(EurekahedgeLockupTokenType.WordNumberValue, "1st|first|i|one|two|six|nine", KeyWordOptions.Word),

                //new Tuple<EurekahedgeLockupTokenType, List<string>>(EurekahedgeLockupTokenType.None, new List<string> { "n/a", "none", "not disclosed", "undisclosed", "not applicable" }),

                //new Tuple<EurekahedgeLockupTokenType, List<string>>(EurekahedgeLockupTokenType.ClassX, new List<string> { "(?:(sub-)?class|series) [a-z]" }),
            };

        private readonly EurekahedgeLockupTockenizer tockenizer;
        private readonly EurekahedgeLockupDurationParcer durationParcer;
        private readonly CultureInfo formatProvider;
        private readonly Dictionary<string, LockupValue> cache;

        public EurekahedgeLockupParcer()
        {
            durationParcer = new EurekahedgeLockupDurationParcer();

            tockenizer = new EurekahedgeLockupTockenizer();
            foreach (Tuple<EurekahedgeLockupTokenType, string, KeyWordOptions> keyWord in keyWords)
            {
                tockenizer.AddTokenDefinition(keyWord.Item1, KeyWordOptions.Word == keyWord.Item3 ? $@"\b(?:{keyWord.Item2})\b" : $"{keyWord.Item2}");
            }
            tockenizer.AddTokenDefinition(EurekahedgeLockupTokenType.Dash, "-", 2);
            tockenizer.AddTokenDefinition(EurekahedgeLockupTokenType.Plus, "\\+", 2);
            tockenizer.AddTokenDefinition(EurekahedgeLockupTokenType.PercentValue, @"\b\d+(?:\.\d+)?\b%");
            tockenizer.AddTokenDefinition(EurekahedgeLockupTokenType.NumberValue, @"\b\d+(?:\.\d+)?", 2);

            formatProvider = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            formatProvider.NumberFormat.NumberDecimalSeparator = ".";

            cache = new Dictionary<string, LockupValue>();
        }

        public LockupValue Parce(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new LockupValue();
            }

            LockupValue result;
            if (cache.TryGetValue(text, out result))
            {
                return result;
            }

            IEnumerable<Tockenizer<EurekahedgeLockupTokenType>.TokenMatch> tokens = tockenizer.Tokenize(text);
            IEnumerable<Tockenizer<EurekahedgeLockupTokenType>.TokenMatch> formatTokens = FormatMatches(text, tokens, keyWord => keyWord, x => x.OriginalMatch);
            result =  Parce(formatTokens.ToList());
            cache[text] = result;
            return result;
        }

        private LockupValue Parce(List<EurekahedgeLockupTockenizer.TokenMatch> tokens)
        {
            LockupValue result = new LockupValue();

            Tockenizer<EurekahedgeLockupTokenType>.TokenMatch numberMatch;
            Tockenizer<EurekahedgeLockupTokenType>.TokenMatch periodMatch;
            FindNumberPeriodPattern(tokens, out numberMatch, out periodMatch);
            Tockenizer<EurekahedgeLockupTokenType>.TokenMatch percentMatch = tokens.FirstOrDefault(x => x.TokenType == EurekahedgeLockupTokenType.PercentValue);
            Tockenizer<EurekahedgeLockupTokenType>.TokenMatch softOrHardMatch = tokens.FirstOrDefault(x => IsSoftOrHard(x.TokenType));
            Tockenizer<EurekahedgeLockupTokenType>.TokenMatch yesOrNoMatch = tokens.FirstOrDefault(x => IsYesOrNo(x.TokenType));

            // Duration
            if (periodMatch != null)
            {
                result.LockupDuration = ToDuration(numberMatch, periodMatch);
            }

            // Period
            if (periodMatch != null)
            {
                result.LockupPeriod = ToPeriod(periodMatch.TokenType);
            }
            
            // Fee
            if (percentMatch != null && periodMatch != null)
            {
                result.LockupFee = ToFee(percentMatch);
            }
            
            // LockupType
            if (softOrHardMatch != null)
            {
                result.LockupType = softOrHardMatch.TokenType == EurekahedgeLockupTokenType.Soft ? LockupType.Soft : LockupType.Hard;
            }
            else if (yesOrNoMatch?.TokenType == EurekahedgeLockupTokenType.No && periodMatch != null)
            {
                result.LockupType = LockupType.Soft;
            }
            else if (percentMatch != null && result.LockupFee != null && periodMatch != null)
            {
                result.LockupType = LockupType.Soft;
            }
            else if (yesOrNoMatch?.TokenType == EurekahedgeLockupTokenType.Yes)
            {
                result.LockupType = LockupType.Hard;
            }
            else if (periodMatch != null)
            {
                result.LockupType = LockupType.Hard;
            }

            if (!result.LockupFee.HasValue && (!result.LockupDuration.HasValue || !result.LockupPeriod.HasValue))
            {
                if (tokens.Exists(x => IsPeriodType(x.TokenType)) || tokens.Exists(x => IsNumberType(x.TokenType)))
                {
                    result.Warning = true;
                }
            }

            return result;
        }
        
        public string Replace(string text)
        {
            string lower = text;

            IEnumerable<EurekahedgeLockupTockenizer.TokenMatch> tokens = tockenizer.Tokenize(lower);
            string result = string.Join(string.Empty, FormatMatches(lower, tokens, keyWord => keyWord, x => x.OriginalMatch).Select(x => ToTockenTypeStr(x.TokenType)));
            return result;
        }

        private static IEnumerable<EurekahedgeLockupTockenizer.TokenMatch> FormatMatches<T>(string inText, IEnumerable<T> matches, Func<T, EurekahedgeLockupTockenizer.TokenMatch> formatWord, Func<T, Group> getGroup)
        {
            List<EurekahedgeLockupTockenizer.TokenMatch> result = new List<EurekahedgeLockupTockenizer.TokenMatch>();

            Action<int, int> tryAppendUndefinedTocken = (start, length) =>
            {
                if (length > 0)
                {
                    result.AddRange(FormatUndefined(inText.Substring(start, length)));
                }
            };

            StringFormatUtils.EnumerateMatches(matches, 
                (s, l) => tryAppendUndefinedTocken(s, l), 
                (s, l, info) => result.Add(formatWord(info)),
                info => getGroup(info).Index,
                info => getGroup(info).Length,
                inText.Length);

            return result;
        }

        private static IEnumerable<EurekahedgeLockupTockenizer.TokenMatch> FormatUndefined(string undefinedStr)
        {
            string undefinedStrTrim = undefinedStr.Trim();
            if (undefinedStrTrim.Length == undefinedStr.Length)
            {
                yield return new Tockenizer<EurekahedgeLockupTokenType>.TokenMatch { TokenType = EurekahedgeLockupTokenType.Undefined };
            }
            else if (undefinedStrTrim.Length == 0)
            {
                yield return new Tockenizer<EurekahedgeLockupTokenType>.TokenMatch { TokenType = EurekahedgeLockupTokenType.Space };
            }
            else 
            {
                if (undefinedStrTrim[0] != undefinedStr[0])
                {
                    yield return new Tockenizer<EurekahedgeLockupTokenType>.TokenMatch { TokenType = EurekahedgeLockupTokenType.Space };
                }
                yield return new Tockenizer<EurekahedgeLockupTokenType>.TokenMatch { TokenType = EurekahedgeLockupTokenType.Undefined };
                if (undefinedStrTrim[undefinedStrTrim.Length - 1] != undefinedStr[undefinedStr.Length - 1])
                {
                    yield return new Tockenizer<EurekahedgeLockupTokenType>.TokenMatch { TokenType = EurekahedgeLockupTokenType.Space };
                }
            }
        }

        private string ToTockenTypeStr(EurekahedgeLockupTokenType type)
        {
            switch (type)
            {
                case EurekahedgeLockupTokenType.Year:
                case EurekahedgeLockupTokenType.Quarter:
                case EurekahedgeLockupTokenType.Day:
                case EurekahedgeLockupTokenType.Week:
                case EurekahedgeLockupTokenType.Month:
                    return "[freq]";

                case EurekahedgeLockupTokenType.Yes:
                    return "[yes]";
                case EurekahedgeLockupTokenType.No:
                    return "[no]";

                case EurekahedgeLockupTokenType.Soft:
                    return "[soft]";
                case EurekahedgeLockupTokenType.Hard:
                    return "[hard]";
                    
                case EurekahedgeLockupTokenType.WordNumberValue:
                case EurekahedgeLockupTokenType.NumberValue:
                    return "[num]";
                case EurekahedgeLockupTokenType.PercentValue:
                    return "[perc]";

                case EurekahedgeLockupTokenType.Space:
                    return "[ ]";
                case EurekahedgeLockupTokenType.Plus:
                    return "[+]";
                case EurekahedgeLockupTokenType.Dash:
                    return "[-]";
                case EurekahedgeLockupTokenType.Undefined:
                    return "[...]";
                    
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private double? ToFee(Tockenizer<EurekahedgeLockupTokenType>.TokenMatch percentMatch)
        {
            double result;
            if (!double.TryParse(percentMatch.Value.Substring(0, percentMatch.Value.Length - 1), NumberStyles.Float, formatProvider, out result))
            {
                return null;
            }

            return result > 0 && result < 10 
                ? result / 100D 
                : (double?)null;
        }

        private int? ToDuration(Tockenizer<EurekahedgeLockupTokenType>.TokenMatch numberMatch, Tockenizer<EurekahedgeLockupTokenType>.TokenMatch periodMatch)
        {
            double result;
            if (numberMatch == null)
            {
                result = 1;
            }
            else if (numberMatch.TokenType == EurekahedgeLockupTokenType.WordNumberValue)
            {
                result = ToNumber(numberMatch.Value);
            }
            else if (!double.TryParse(numberMatch.Value, NumberStyles.Float, formatProvider, out result))
            {
                return null;
            }

            double multi;
            switch (periodMatch.TokenType)
            {
                case EurekahedgeLockupTokenType.Year:
                    multi = 12;
                    break;

                case EurekahedgeLockupTokenType.Quarter:
                    multi = 3;
                    break;

                case EurekahedgeLockupTokenType.Month:
                case EurekahedgeLockupTokenType.Week:
                case EurekahedgeLockupTokenType.Day:
                    multi = 1;
                    break;

                default:
                    return null;
            }

            return (int) Math.Truncate(result * multi);
        }

        private LockupDurationPeriod? ToPeriod(EurekahedgeLockupTokenType type)
        {
            switch (type)
            {
                case EurekahedgeLockupTokenType.Year:
                case EurekahedgeLockupTokenType.Quarter:
                case EurekahedgeLockupTokenType.Month:
                    return LockupDurationPeriod.Months;
                    
                case EurekahedgeLockupTokenType.Week:
                    return LockupDurationPeriod.Weeks;

                case EurekahedgeLockupTokenType.Day:
                    return LockupDurationPeriod.Days;
            }

            return null;
        }

        private int ToNumber(string wordNumber)
        {
            switch (wordNumber.ToLower())
            {
                case "1st":
                case "first":
                case "i":
                case "one" : return 1;

                case "two" : return 2;
                case "three" : return 3;
                case "four" : return 5;
                case "five" : return 6;
                case "six" : return 6;
                case "seven" : return 7;
                case "eight" : return 8;
                case "nine" : return 9;
                default:
                    throw new ArgumentOutOfRangeException(nameof(wordNumber), wordNumber, null);
            }
        }

        private bool IsNumberType(EurekahedgeLockupTokenType type)
        {
            return numbers.Contains(type);
        }

        private bool IsPeriodType(EurekahedgeLockupTokenType type)
        {
            return period.Contains(type);
        }

        private bool IsSoftOrHard(EurekahedgeLockupTokenType type)
        {
            return type == EurekahedgeLockupTokenType.Hard || 
                   type == EurekahedgeLockupTokenType.Soft;
        }

        private bool IsYesOrNo(EurekahedgeLockupTokenType type)
        {
            return type == EurekahedgeLockupTokenType.Yes || 
                   type == EurekahedgeLockupTokenType.No;
        }
        
        private void FindNumberPeriodPattern(List<EurekahedgeLockupTockenizer.TokenMatch> tokens, out EurekahedgeLockupTockenizer.TokenMatch numberMatch, out EurekahedgeLockupTockenizer.TokenMatch periodMatch)
        {
            numberMatch = null;
            periodMatch = null;

            periodMatch = tokens.FirstOrDefault(x => IsPeriodType(x.TokenType));
            if (periodMatch == null)
            {
                return;
            }

            if (!tokens.Any(x => IsNumberType(x.TokenType)))
            {
                return;
            }

            periodMatch = null;

            int? numberMatchIndex;
            int? periodMatchIndex;
            if (durationParcer.Parce(tokens.Select(x => x.TokenType), out numberMatchIndex, out periodMatchIndex))
            {
                numberMatch = tokens[numberMatchIndex.Value];
                periodMatch = tokens[periodMatchIndex.Value];
            }
        }

        private class EurekahedgeLockupDurationParcer
        {
            private class EurekahedgePatternMatcher : PatternMatcher<EurekahedgeLockupDurationType> { }

            private enum EurekahedgeLockupDurationType
            {
                Undefined,

                Plus,
                Number,
                Separator,
                Period,
            }
            
            private readonly EurekahedgePatternMatcher patternMatcher;
            
            public EurekahedgeLockupDurationParcer()
            {
                patternMatcher = new EurekahedgePatternMatcher();
                patternMatcher.AddPattern(new [] { EurekahedgeLockupDurationType.Number, EurekahedgeLockupDurationType.Separator, EurekahedgeLockupDurationType.Period });
                patternMatcher.AddPattern(new [] { EurekahedgeLockupDurationType.Period, EurekahedgeLockupDurationType.Separator, EurekahedgeLockupDurationType.Number });
                patternMatcher.AddPattern(new [] { EurekahedgeLockupDurationType.Number, EurekahedgeLockupDurationType.Period });
                patternMatcher.AddPattern(new [] { EurekahedgeLockupDurationType.Number, EurekahedgeLockupDurationType.Plus, EurekahedgeLockupDurationType.Separator, EurekahedgeLockupDurationType.Period });
            }
            
            public bool Parce(IEnumerable<EurekahedgeLockupTokenType> tokens, out int? numberMatchIndex, out int? periodMatchIndex)
            {
                numberMatchIndex = null;
                periodMatchIndex = null;
                
                EurekahedgeLockupDurationType[] sequence = tokens.Select(ToDurationTocken).ToArray();
                EurekahedgePatternMatcher.PatternMatch match = patternMatcher.Match(sequence).FirstOrDefault();
                if (match == null)
                {
                    return false;
                }

                EurekahedgeLockupDurationType[] matchTockens = sequence.Skip(match.StartIndex).Take(match.EndIndex - match.StartIndex).ToArray();
                for (int i = 0; i < matchTockens.Length; i++)
                {
                    if (match.Value[i] == EurekahedgeLockupDurationType.Number)
                    {
                        numberMatchIndex = match.StartIndex + i;
                    }
                    else if (match.Value[i] == EurekahedgeLockupDurationType.Period)
                    {
                        periodMatchIndex = match.StartIndex + i;
                    }
                }
                return true;
            }

            private EurekahedgeLockupDurationType ToDurationTocken(EurekahedgeLockupTokenType type)
            {
                switch (type)
                {
                    case EurekahedgeLockupTokenType.Year:
                    case EurekahedgeLockupTokenType.Quarter:
                    case EurekahedgeLockupTokenType.Day:
                    case EurekahedgeLockupTokenType.Week:
                    case EurekahedgeLockupTokenType.Month:
                        return EurekahedgeLockupDurationType.Period;
                        
                        
                    case EurekahedgeLockupTokenType.WordNumberValue:
                    case EurekahedgeLockupTokenType.NumberValue:
                        return EurekahedgeLockupDurationType.Number;

                    case EurekahedgeLockupTokenType.Space:
                    case EurekahedgeLockupTokenType.Dash:
                        return EurekahedgeLockupDurationType.Separator;

                    case EurekahedgeLockupTokenType.Plus:
                        return EurekahedgeLockupDurationType.Plus;

                    default:
                        return EurekahedgeLockupDurationType.Undefined;
                }
            }
        }
    }
}