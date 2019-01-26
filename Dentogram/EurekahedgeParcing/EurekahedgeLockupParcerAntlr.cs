using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Antlr4.Runtime;

namespace Dentogram.EurekahedgeParcing
{
    public class EurekahedgeLockupParcerAntlr
    {
        private readonly Dictionary<string, LockupValue> cache;
        private readonly CultureInfo formatProvider;

        public EurekahedgeLockupParcerAntlr()
        {
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

            result =  ParceInternal(text);
            cache[text] = result;
            return result;
        }

        private LockupValue ParceInternal(string text)
        {
            var textStream = new AntlrInputStream(text.ToLower());
            var lexer = new EurekahedgeLockupAntlrLexer(textStream);
            var tockenStream = new CommonTokenStream(lexer);
            var parcer = new EurekahedgeLockupAntlrParser(tockenStream);
            
            var visitor = new EurekahedgeLockupAntlrVisitor();
            visitor.VisitGeneral(parcer.general());
            
            LockupValue result = new LockupValue();

            // Duration
            if (visitor.periodMatch != null)
            {
                result.LockupDuration = ToDuration(visitor.numberMatch, visitor.periodMatch);
            }

            // Period
            if (visitor.periodMatch != null)
            {
                result.LockupPeriod = ToPeriod(visitor.periodMatch.TokenType);
            }
            
            // Fee
            if (visitor.percentMatch != null && visitor.periodMatch != null)
            {
                result.LockupFee = ToFee(visitor.percentMatch);
            }
            
            // LockupType
            if (visitor.softOrHardMatch != null)
            {
                result.LockupType = visitor.softOrHardMatch.TokenType == EurekahedgeLockupTokenType.Soft ? LockupType.Soft : LockupType.Hard;
            }
            else if (visitor.yesOrNoMatch?.TokenType == EurekahedgeLockupTokenType.No && visitor.periodMatch != null)
            {
                result.LockupType = LockupType.Soft;
            }
            else if (visitor.percentMatch != null && result.LockupFee != null && visitor.periodMatch != null)
            {
                result.LockupType = LockupType.Soft;
            }
            else if (visitor.yesOrNoMatch?.TokenType == EurekahedgeLockupTokenType.Yes)
            {
                result.LockupType = LockupType.Hard;
            }
            else if (visitor.periodMatch != null)
            {
                result.LockupType = LockupType.Hard;
            }

            return result;
        }
        
        private double? ToFee(EurekahedgeLockupAntlrVisitor.TokenMatch percentMatch)
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

        private int? ToDuration(EurekahedgeLockupAntlrVisitor.TokenMatch numberMatch, EurekahedgeLockupAntlrVisitor.TokenMatch periodMatch)
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

        public enum EurekahedgeLockupTokenType
        {
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
        }

        public class EurekahedgeLockupAntlrVisitor : EurekahedgeLockupAntlrBaseVisitor<object>
        {
            public class TokenMatch
            {
                public string Value { get; set; }
                public EurekahedgeLockupTokenType TokenType { get; set; }
                public int StartIndex { get; set; }
                public int EndIndex => StartIndex + Value.Length;
            }

            public TokenMatch numberMatch;
            public TokenMatch periodMatch;
            public TokenMatch percentMatch;
            public TokenMatch softOrHardMatch;
            public TokenMatch yesOrNoMatch;

            public override object VisitLockupDuration(EurekahedgeLockupAntlrParser.LockupDurationContext context)
            {
                var numberContext = context.number();
                if (!numberContext.IsEmpty)
                {
                    if (numberContext.NUMBER() != null)
                    {
                        numberMatch = new TokenMatch { Value = numberContext.GetText(), StartIndex = numberContext.Start.StartIndex};
                        numberMatch.TokenType = EurekahedgeLockupTokenType.NumberValue;
                    }
                    else if (numberContext.WORDNUMBERVALUE() != null)
                    {
                        numberMatch = new TokenMatch { Value = numberContext.WORDNUMBERVALUE().GetText(), StartIndex = numberContext.Start.StartIndex};
                        numberMatch.TokenType = EurekahedgeLockupTokenType.WordNumberValue;
                    }
                }

                var periodContext = context.period();
                if (!periodContext.IsEmpty)
                {
                    periodMatch = new TokenMatch { Value = periodContext.GetText(), StartIndex = numberContext.Start.StartIndex};
                    if (periodContext.YEAR() != null)
                    {
                        periodMatch.TokenType = EurekahedgeLockupTokenType.Year;
                    }
                    else if (periodContext.QUARTER() != null)
                    {
                        periodMatch.TokenType = EurekahedgeLockupTokenType.Quarter;
                    }
                    else if (periodContext.month()?.IsEmpty == false)
                    {
                        periodMatch.TokenType = EurekahedgeLockupTokenType.Month;
                    }
                    else if (periodContext.WEEK() != null)
                    {
                        periodMatch.TokenType = EurekahedgeLockupTokenType.Week;
                    }
                    else if (periodContext.DAY() != null)
                    {
                        periodMatch.TokenType = EurekahedgeLockupTokenType.Day;
                    }
                }
                return base.VisitLockupDuration(context);
            }

            public override object VisitYesNo(EurekahedgeLockupAntlrParser.YesNoContext context)
            {
                yesOrNoMatch = new TokenMatch { Value = context.GetText(), StartIndex = context.Start.StartIndex};
                if (context.YES() != null)
                {
                    yesOrNoMatch.TokenType = EurekahedgeLockupTokenType.Yes;
                }
                else if (context.NO() != null)
                {
                    yesOrNoMatch.TokenType = EurekahedgeLockupTokenType.No;
                }
                return base.VisitYesNo(context);
            }

            public override object VisitPercent(EurekahedgeLockupAntlrParser.PercentContext context)
            {
                if (context.PERCENT() != null)
                {
                    percentMatch = new TokenMatch { Value = context.GetText(), StartIndex = context.Start.StartIndex };
                    percentMatch.TokenType = EurekahedgeLockupTokenType.PercentValue;
                }
                return base.VisitPercent(context);
            }

            public override object VisitSoftHard(EurekahedgeLockupAntlrParser.SoftHardContext context)
            {
                softOrHardMatch = new TokenMatch { Value = context.GetText(), StartIndex = context.Start.StartIndex };
                if (context.HARD() != null)
                {
                    softOrHardMatch.TokenType = EurekahedgeLockupTokenType.Hard;
                }
                else if (context.SOFT() != null)
                {
                    softOrHardMatch.TokenType = EurekahedgeLockupTokenType.Soft;
                }
                return base.VisitSoftHard(context);
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                if (numberMatch != null)
                {
                    builder.Append($"n: {numberMatch?.Value}; ");
                }
                if (percentMatch != null)
                {
                    builder.Append($"p: {percentMatch?.Value}; ");
                }
                if (periodMatch != null)
                {
                    builder.Append($"f: {periodMatch?.Value}; ");
                }
                if (softOrHardMatch != null)
                {
                    builder.Append($"sh: {softOrHardMatch?.Value}; ");
                }
                if (yesOrNoMatch != null)
                {
                    builder.Append($"ny: {yesOrNoMatch?.Value}; ");
                }
                return builder.ToString();
            }
        }
    }
}