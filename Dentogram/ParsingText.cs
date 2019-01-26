﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dentogram
{
    public class ParsingText
    {
        public struct ParseResult
        {
            public string NamePersonPrefix;
            public string NamePersonValue;
            public string NamePersonPostfix;
            public string NamePerson;

            public string NamePersonTest;
            public string NamePersonTest1;

            public string AggregatedAmountPrefix;
            public string AggregatedAmountValue;
            public string AggregatedAmountPostfix;
            public string AggregatedAmount;

            public string PercentOwnedPrefix;
            public string PercentOwnedValue;
            public string PercentOwnedPostfix;
            public string PercentOwned;

            public string Region;
        }


        private static readonly string ss = 
            "(?:" +
                "S" +
            "|" +
                "\\(S\\)" +
            ")";
        private static readonly string irs = 
            "(?:" +
                "\\(?I\\.? ?R\\.? ?S\\.?" +
            "|" +
                "RS" +
            ")";

        private static readonly string ident = 
            "(?:" +
                "IN?DENT" +
                "(?:" +
                    "IFICATIOI?N" +
                ")?" +
                "(?:" +
                    " ?N" +
                    "(?:" +
                        $"O{ss}?\\.?" +
                    "|" +
                        "UMBERS?" +
                    ")" +
                ")?" +
            "|" +
                "NUMBER" +
            ")";

        private static readonly string name = 
            "NAMES?";

        private static readonly string rep_person = 
            "(?:" +
                $"OF REPORTING PER ?SON{ss}?" +
                "(?:" +
                    " ?[\\.,]" +
                ")?" +
            ")";

        private static readonly string irs_ident = 
            $"(?:{irs} ?{ident})";

        private static readonly string ss_or_and_1 = 
            "(?:" +
                "(?:" +
                    "1\\.? ?" +
                ")?" +
                "S\\.?S\\.? ?O[FR]" +
            "|" +
                "AND" +
            "|" +
                "OR" +
            "|" +
                "(?:" +
                    "1\\.? " +
                ")?" +
            ")";
        private static readonly string above_per = 
            "(?:" +
                "O[FR] ?" +
                "(?:" +
                    "ABOVE" +
                "|" +
                    "SUCH" +
                ")?" +
                $" PERSON{ss}?" +
                "(?:" +
                    " ?[\\.,]" +
                ")?" +
            ")";
        private static readonly string entity = 
            "(?:" +
                "\\(ENTIT" +
                "(?:" +
                    "IES" +
                "|" +
                    "Y" +
                ")" +
                "(?:" +
                    " ONLY" +
                ")?" +
                "(?:" +
                    " ?[\\.,]" +
                ")?" +
                "\\)?" +
                "(?:" +
                    " ?[\\.,]" +
                ")?" +
            ")";
        private static readonly string see_inst = 
            "(?:" +
                "\\(SEE INSTRUCTIONS\\)" +
            ")";

        /*
[name][rep_person] [irs_ident]
[name][rep_person] [irs_ident] [entit]
[name][rep_person] [irs_ident] [above_per]
[name][rep_person] [irs_ident] [above_per] [entit]
[name][rep_person] [irs_ident] [rep_person]
[name][rep_person] [irs_ident] [rep_person] [entit]

[name][rep_person] [ss_or_and_1] [irs_ident]
[name][rep_person] [ss_or_and_1] [irs_ident] [above_per]
[name][rep_person] [ss_or_and_1] [irs_ident] [above_per] [entit]
[name][rep_person] [ss_or_and_1] [irs_ident] [rep_person]
[name][rep_person] [ss_or_and_1] [irs_ident] [rep_person] [entit]

[name][rep_person]
[name][rep_person] (SEE INSTRUCTIONS)
[name][rep_person] [entit]
//[name][rep_person] [ss_or_and_1]
[name][rep_person] ([ss_or_and_1] [irs_ident] [above_per])

[name][ss_or_and_1] [irs_ident] [rep_person]
         */

        // Name Person
        public readonly Regex[] namePersonPrefixRegex =
        {
            new Regex(
                $"{name} ?{rep_person}" +
                "(?:" +
                    $" ?{ss_or_and_1}" +
                ")?" +
                $" ?{irs_ident} ?" +
                "(?:" +
                    $"{entity}" +
                "|" +
                    $"{above_per}(?: ?{entity})?" +
                "|" +
                    $"{rep_person}(?: ?{entity})?" +
                ")?", 
                RegexOptions.Compiled | RegexOptions.IgnoreCase),
            
            new Regex(
                $"{name} ?{rep_person}" +
                "(?:" +
                    " ?" +
                    "(?:" +
                        $"\\( ?{ss_or_and_1} ?{irs_ident} ?{above_per} ?\\)" +
                    "|" +
                        $"{see_inst}" +
                    "|" +
                        $"{entity}" +
                    "|" +
                        $"{rep_person}" +
                    ")" +
                ")?", 
                RegexOptions.Compiled | RegexOptions.IgnoreCase),

            new Regex(
                $@"{name} ?{ss_or_and_1} ?{irs_ident} ?{rep_person}", 
                RegexOptions.Compiled | RegexOptions.IgnoreCase),

            new Regex(
                $@"1 {entity}", 
                RegexOptions.Compiled | RegexOptions.IgnoreCase),
        };
        /*
        private readonly Regex[] namePersonPrefixRegex =
        {
            new Regex(@"NAMES? OF REPORTING(?: PER ?SONS?\(?S?\)?)(?:(?: [1\(]? ?SS)? ?O[FR]| AND(?:  SS OR)?)?(?: ?(?:I ?R ?S|\(?I?RS|1 IRS)(?: ?IN?DENT(?:IFICATIOI?N)?)? ?(?:NUMBERS?|NO\(?S?S?\)?)?(?: ?O[FR](?: ABOVE| REPORTING| SUCH)?(?: PERSON\(?S?S?\)?)?)?)?(?: ?\(ENTIT(?:Y|IES)(?: ONLY)?\)?)?(?: \((?:SEE INSTRUCTIONS|2)\))?", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"NAME (?:AND|OR) IRS(?: IDENTIFICATION)? (?:NO|NUMBER) OF REPORTING PERSONS?", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"1 \(ENTITIES ONLY\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        };
        */

/*
[name person] [(word)]
[name person] [date prefix] [date]
[name person] IRS IDENTIFICATION NO
[name person] IDENTIFICATION NOS OF ABOVE PERSONS
[name person] [id prefix] [id]
[start] (1|2) [name person] [id] [end]
[start] [name person] [word] [end]
[start] ([id]|[(word)]) [name person] [end]
 */
 
        private const string no = "NO\\.?";
        private const string nameValue = @"[\w\s\(\),\.]{4,}";

        public readonly Regex[] namePersonValueRegex =
        {

            new Regex(
                $"({nameValue}?)" + //match
                    "\\( ?" +
                    "(?:" +
                        "1" +
                    "|" +
                        "N"+
                        "(?:"+
                            "O IRS IDENTIFICATION N(?:O\\.?|UMBER)" +
                        "|" +
                            "ONE" +
                        "|" +
                            "O EIN" +
                        "|" +
                            " A" +
                        ")"+
                    "|" +
                        "IRS NO N A" +
                    "|" +
                        "THE REPORTING PERSON" +
                    "|" +
                        "SEE ITEM \\d" +
                    ")" +
                    " ?\\)", 
                RegexOptions.Compiled | RegexOptions.IgnoreCase),

            new Regex(
                $"({nameValue}?)" + //match
                "(?:" +
                    "DATED" +
                "|" +
                    "U ?A ?D(?:ATED)?" +
                "|" +
                    "U ?T ?[AD](?: DATED)?" +
                "|" +
                    "DTD" +
                ") " +
                "(?:" +
                    "[A-Z]{3,11} \\d{1,2},? \\d{4,4}" +
                "|" +
                    "\\d{1,2} \\d{2,2} \\d{2,2}" +
                ")", 
                RegexOptions.Compiled | RegexOptions.IgnoreCase),

            new Regex(
                $"({nameValue}?)" + //match
                "(?:" +
                    $"(?: ?{ss_or_and_1})? ?{irs} ?{ident} ?" +
                "|" +
                    "IDENTIFICATION NOS\\. OF ABOVE PERSONS" +
                "|" +
                    "NOT INDIVIDUALLY," +
                "|" +
                    "CIK " +
                ")", 
                RegexOptions.Compiled | RegexOptions.IgnoreCase),
            
            new Regex(
                $"({nameValue}?)" + //match
                "\\(? ?" +
                "(?:" +
                    "(?:\\( ?(?:B ?\\) ?)?)?" +
                    "TAX" +
                    "(?:" +
                        $" I\\.?D\\.?(?: {no})?" +
                    "|" +
                        "PAYER IDENTIFICATION NUMBER" +
                    ")?" +
                "|" +
                    $"I\\.?D\\.?(?:ENTIFICATION)?(?: {no})?" +
                "|" +
                    "I\\.?R\\.?S\\.?" +
                    "(?:" +
                        " (?:" +
                            $"I\\.?D\\.?(?: {no})?" +
                        "|" +
                            "EIN" +
                        "|" +
                            $"{no}" +
                        ")" +
                    ")?" +
                "|" +
                    $"EIN(?: {no})?" +
                "|" +
                    $"{no}" +
                //"|" +
                //    "CIK" +
                "|" +
                    "FEDERAL IDENTIFICATION NUMBER" +
                ")" +
                " ?" +
                "(?:" +
                    "(?:\\d{2,2} )?\\d{6,8}\\.?" +
                ")", 
                RegexOptions.Compiled | RegexOptions.IgnoreCase),

            new Regex(
                "^" +
                    " ?" +
                    "(?:" +
                        "\\( ?[12] ?\\)" +
                    "|" +
                        "1 " +
                    ")?" +
                    $"({nameValue}?)" + //match
                    "\\(? ?" +
                    "(?:" +
                        "\\d{2,2} \\d{6,8}" +
                    "|" +
                        "\\d{7,9}" +
                    "|" +
                        "\\d{2,2} \\d{3,3} \\d{4,4}" +
                    "|" +
                        "[\\dX]{3,3} [\\dX]{2,2} [\\dX]{4,4}" +
                    ")" +
                    " ?\\)?\\.? ?", 
                RegexOptions.Compiled | RegexOptions.IgnoreCase),

            new Regex(
                "^" +
                    $"({nameValue}?)" + //match
                    " " +
                    "(?:" +
                        "ID N\\.?(?:A\\.?|O\\.?(?:T APPLICABLE)?)" +
                    "|" +
                        "I\\.?R\\.?S\\.?" +
                    "|" +
                        "SEE ITEM \\d FOR IDENTIFICATION OF THE (?:GENERAL PARTNER|MANAGING MEMBERS)" +
                    ")" +
                    " ?" +
                "$", 
                RegexOptions.Compiled | RegexOptions.IgnoreCase),

            new Regex(
                "^" +
                    " ?" +
                    "(?:" +
                        "\\d{2,2} \\d{6,8} " +
                    "|" +
                        "\\d{6,9} " +
                    "|" +
                        "\\(? ?[12] ?\\)" +
                    "|" +
                        "[12] " +
                    "|" +
                        "\\(VOLUNTARY\\)" +
                        "(?: EIN NO\\.?)?" +
                    "|" +
                        "\\(OPTIONAL\\)" +
                    ")" +
                    $"({nameValue})" + //match
                "$", 
                RegexOptions.Compiled | RegexOptions.IgnoreCase),
            //  PLEASE CREATE A SEPARATE COVER SHEET FOR EACH ENTITY
        };

        private readonly Regex[] namePersonPostfixRegex =
        {
            new Regex(@"(?:\(?(?:2|14)\.?\)?(?: ?\( ?a ?\))? ?(?:CHECK|(?:IF A )?MEMBER)|CHECK THE APPROPRIATE)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        };
        
        // Aggregated Amount
        private readonly Regex aggregatedAmountPrefixRegex = new Regex(@"(?:\(?(?:9|11)\.?\)? ?)?AGGREGATED? AMOUN?T(?:(?: OF)? BENE?FICI?AL?LY)? OWNED(?: BY(?: EACH)?(?: REPORTING)? ?PERSON(?: \(DISCRETIONARY NON DISCRETIONARY ACCOUNTS\))?)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex aggregatedAmountPostfixRegex = new Regex(@"(?:\(?1[02]\.?\)? ?)?(?:CHECK(?: BOX)? IF(?: THE)? AGGREGATE|AGGREGATE AMOUNT)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex aggregatedAmountValueRegex = new Regex(@"(?:^|[^\(])((?:(?:\d{1,3}(?: \d{3,3})+)|\d+(?:\,\d+)*))(?!\))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Percent Owned
        private readonly Regex percentOwnedPrefixRegex = new Regex(@"\(?1?[123]\.?\)? ?PERCENT(?:AGE)? OF (?:CLASS|SERIES) REPRESENTED(?: BY)?(?: AMOUNT)?(?: IN| OF)? (?:ROW|BOX)(?: ?(?:\( ?)?(?:9|11)(?: ?\))?)?(?: ?\(SEE ITEM 5\))?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex percentOwnedPostfixRegex = new Regex(@"(?:\(?1[24]\.?\)? ?TYPE|TYPE OF REPORTING PERSON)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex percentOwnedValueRegex = new Regex(@"(?:^|[^\(])(?:\d+(?:\,\d+)+ \d+(?:\,\d+)+ )?((?:\d+(?:\.\d+)?)|\*) ?%?(?!\))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly Regex namePersonFullRegex = new Regex(@"(CUSIP(?: NUMBER)? [\w]+ ITEM 1 REPORTING PERSON) ([\s\S]{0,200}?)(?:\d{2,2} \d{6,8})? (ITEM \d)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex aggregatedFullAmountRegex = new Regex(@"(ITEM 9) ((?:\d+(?:\,\d+)*)|\*|NONE) (ITEM 11)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex percentOwnedFullRegex = new Regex(@"(ITEM 11) ((?:\d+(?:\.\d+)?)|\*) ?%? (ITEM 12)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly NumberFormatInfo thousandSpaceFormat;

        public ParsingText()
        {
            thousandSpaceFormat = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            thousandSpaceFormat.NumberGroupSeparator = " ";
        }

        public string TrimForClustering(string text)
        {
            string trimText = Regex.Replace(text, @"[\.,\d]", "");
            //trimText = Regex.Replace(trimText, @" \w{1,3}(?= )", " ");
            trimText = Regex.Replace(trimText, @"\s+", " ");
            return trimText;
        }

        public string TrimForParsing(string text)
        {
            string trimText = text;
            trimText = Regex.Replace(trimText, @"\.{2,}|,{2,}", "");
            trimText = Regex.Replace(trimText, "_", ""); // Replace separately cause '_' include in \w
            trimText = Regex.Replace(trimText, @"[^\w\s\.,\(\)]", " ");
            trimText = Regex.Replace(trimText, @"\s+", " ");
            return trimText;
        }

        public IEnumerable<ParseResult> ParseBySearch(string trimText)
        {
            List<StringSearchResult> namePersonPrefixResult;
            List<StringSearchResult> namePersonValueResult;
            List<StringSearchResult> namePersonPostfixResult;
            ParseNamePersons(trimText, out namePersonPrefixResult, out namePersonValueResult, out namePersonPostfixResult);

            if (namePersonValueResult.Any())
            {
                for (int i = 0; i < namePersonPrefixResult.Count; i++)
                {
                    StringSearchResult namePersonPrefixMatch = namePersonPrefixResult[i];
                    StringSearchResult namePersonValueMatch = namePersonValueResult[i];
                    StringSearchResult namePersonPostfixMatch = namePersonPostfixResult[i];

                    if (namePersonValueMatch.IsEmpty)
                    {
                        continue;
                    }

                    int namePersonRegionLength = i + 1 < namePersonPrefixResult.Count 
                        ? namePersonPrefixResult[i + 1].Index - namePersonPrefixMatch.Index
                        : Math.Min(namePersonPrefixMatch.Keyword.Length + namePersonValueMatch.Keyword.Length + ParsingTextHelper.NamePersonRegionMaxLength, trimText.Length - namePersonPrefixMatch.Index);
                    string namePersonRegion = trimText.Substring(namePersonPrefixMatch.Index, namePersonRegionLength);
                
                    StringSearchResult aggregatedAmountPrefixResult;
                    StringSearchResult aggregatedAmountValueResult;
                    StringSearchResult aggregatedAmountPostfixResult;
                    ParseRegionValue(namePersonRegion, aggregatedAmountPrefixRegex, aggregatedAmountPostfixRegex, aggregatedAmountValueRegex, aggregatedFullAmountRegex,
                        out aggregatedAmountPrefixResult, out aggregatedAmountValueResult, out aggregatedAmountPostfixResult);

                    StringSearchResult percentOwnedPrefixResult;
                    StringSearchResult percentOwnedValueResult;
                    StringSearchResult percentOwnedPostfixResult;
                    ParseRegionValue(namePersonRegion, percentOwnedPrefixRegex, percentOwnedPostfixRegex, percentOwnedValueRegex, percentOwnedFullRegex,
                        out percentOwnedPrefixResult, out percentOwnedValueResult, out percentOwnedPostfixResult);

                    double t1;
                    if (TryParceDouble(aggregatedAmountValueResult.Keyword, out t1))
                    {
                        
                    }

                    double t2;
                    if (TryParcePercent(percentOwnedValueResult.Keyword, out t2))
                    {
                        
                    }

                    yield return new ParseResult
                    {
                        NamePersonPrefix = namePersonPrefixMatch.Keyword,
                        NamePersonValue = namePersonValueMatch.Keyword,
                        NamePersonPostfix = namePersonPostfixMatch.Keyword,
                        NamePerson = Test(trimText, namePersonPrefixMatch, namePersonPostfixMatch),

                        NamePersonTest = Test1(trimText, namePersonPrefixMatch, namePersonValueMatch, namePersonPostfixMatch),
                        NamePersonTest1 = namePersonValueMatch.RegexIndex.ToString(),
                        //NamePersonTest = namePersonValueMatch.RegexIndex.ToString(),

                        AggregatedAmountPrefix = aggregatedAmountPrefixResult.Keyword,
                        AggregatedAmountValue = aggregatedAmountValueResult.Keyword,
                        AggregatedAmountPostfix = aggregatedAmountPostfixResult.Keyword,
                        AggregatedAmount = Test(namePersonRegion, aggregatedAmountPrefixResult, aggregatedAmountPostfixResult),

                        PercentOwnedPrefix = percentOwnedPrefixResult.Keyword,
                        PercentOwnedValue = percentOwnedValueResult.Keyword,
                        PercentOwnedPostfix = percentOwnedPostfixResult.Keyword,
                        PercentOwned = Test(namePersonRegion, percentOwnedPrefixResult, percentOwnedPostfixResult),

                        Region = namePersonRegion.Substring(0, namePersonRegion.LastIndexOf(' '))
                    };
                }
            }
            else
            {
                yield return new ParseResult();
            }
        }

        private string Test(string namePersonRegion, StringSearchResult prefixResult, StringSearchResult postfixResult)
        {
            try
            {
                return prefixResult.IsEmpty || postfixResult.IsEmpty
                    ? string.Empty
                    : namePersonRegion.Substring(
                        prefixResult.Index + prefixResult.Keyword.Length,
                        postfixResult.Index - (prefixResult.Index + prefixResult.Keyword.Length));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private string Test1(string namePersonRegion, StringSearchResult prefixResult, StringSearchResult valueResult, StringSearchResult postfixResult)
        {
            try
            {
                return valueResult.IsEmpty
                    ? string.Empty
                    : namePersonRegion.Substring(prefixResult.Index + prefixResult.Keyword.Length, valueResult.Index - (prefixResult.Index + prefixResult.Keyword.Length)) +
                      $"{{{valueResult.RegexIndex}}}" +
                      namePersonRegion.Substring(valueResult.Index + valueResult.Keyword.Length, postfixResult.Index - (valueResult.Index + valueResult.Keyword.Length));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private bool TryParceDouble(string value, out double result)
        {
            bool success = double.TryParse(value, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result);
            if (!success && !string.IsNullOrEmpty(value))
            {
                success = double.TryParse(value, NumberStyles.AllowThousands, thousandSpaceFormat, out result);
            }
            return success;
        }

        private bool TryParcePercent(string value, out double result)
        {
            bool success =  double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
            return success;
        }

        private static void ParseRegionValue(string text, Regex valuePrefixRegex, Regex valuePostfixRegex, Regex valueRegex, Regex fullValueRegex,
            out StringSearchResult valuePrefixResult, out StringSearchResult valueResult, out StringSearchResult valuePostfixResult)
        {
            valuePrefixResult = StringSearchResult.Empty;
            valueResult = StringSearchResult.Empty;
            valuePostfixResult = StringSearchResult.Empty;

            valuePrefixResult = ParseFirstByRegexp(text, valuePrefixRegex, 0);
            if (valuePrefixResult.IsEmpty)
            {
                Match regexMatch = fullValueRegex.Match(text);
                if (regexMatch.Success)
                {
                    valuePrefixResult = new StringSearchResult(regexMatch.Groups[1].Index, regexMatch.Groups[1].Value);
                    valueResult = new StringSearchResult(regexMatch.Groups[2].Index, regexMatch.Groups[2].Value);
                    valuePostfixResult = new StringSearchResult(regexMatch.Groups[3].Index, regexMatch.Groups[3].Value);
                }
                return;
            }
            
            int iValue = valuePrefixResult.Index + valuePrefixResult.Keyword.Length;
            valuePostfixResult = ParseFirstByRegexp(text.Substring(iValue, text.Length - iValue), valuePostfixRegex, 0).OffsetResult(iValue);
            valueResult = valuePostfixResult.IsEmpty 
                ? StringSearchResult.Empty
                : ParseFirstByRegexp(text.Substring(iValue, valuePostfixResult.Index - iValue), valueRegex, 1).OffsetResult(iValue);
        }

        private void ParseNamePersons(string trimText, out List<StringSearchResult> namePersonPrefixResult, out List<StringSearchResult> namePersonValueResult, out List<StringSearchResult> namePersonPostfixResult)
        {
            namePersonPrefixResult = new List<StringSearchResult>();
            namePersonValueResult = new List<StringSearchResult>();
            namePersonPostfixResult = new List<StringSearchResult>();
            
            List<StringSearchResult> searchMatches = ParseAllByRegexp(trimText, namePersonPrefixRegex);
            if (searchMatches.Any())
            {
                foreach (StringSearchResult namePersonPrefixMatch in searchMatches)
                {
                    int iNamePersonValue = namePersonPrefixMatch.Index + namePersonPrefixMatch.Keyword.Length;
                    StringSearchResult namePersonPostfixMatch = ParseFirstByRegexp(trimText.Substring(iNamePersonValue, ParsingTextHelper.NamePersonMaxLength), namePersonPostfixRegex, 0).OffsetResult(iNamePersonValue);
                    string namePersonValueText = namePersonPostfixMatch.IsEmpty ? string.Empty: trimText.Substring(iNamePersonValue, namePersonPostfixMatch.Index - iNamePersonValue);

                    StringSearchResult namePersonValueMatch = namePersonPostfixMatch.IsEmpty 
                        ? StringSearchResult.Empty
                        : ParseFirstByRegexp(namePersonValueText, namePersonValueRegex, 1).OffsetResult(iNamePersonValue);
                    if (!namePersonPostfixMatch.IsEmpty && namePersonValueMatch.IsEmpty)
                    {
                        namePersonValueMatch = new StringSearchResult(iNamePersonValue, namePersonValueText);
                    }

                    namePersonPrefixResult.Add(namePersonPrefixMatch);
                    namePersonValueResult.Add(namePersonValueMatch);
                    namePersonPostfixResult.Add(namePersonPostfixMatch);
                }
            }
            else
            {
                MatchCollection regexMatches = namePersonFullRegex.Matches(trimText);
                foreach (Match namePersonMatch in regexMatches)
                {
                    var namePersonPrefixMatch = new StringSearchResult(
                        namePersonMatch.Groups[1].Index,
                        namePersonMatch.Groups[1].Value);
                    var namePersonValueMatch = new StringSearchResult(
                        namePersonMatch.Groups[2].Index,
                        namePersonMatch.Groups[2].Value);
                    var namePersonPostfixMatch = new StringSearchResult(
                        namePersonMatch.Groups[3].Index,
                        namePersonMatch.Groups[3].Value);

                    namePersonPrefixResult.Add(namePersonPrefixMatch);
                    namePersonValueResult.Add(namePersonValueMatch);
                    namePersonPostfixResult.Add(namePersonPostfixMatch);
                }
            }
        }

        private static List<StringSearchResult> ParseAllByRegexp(string text, IEnumerable<Regex> regexps)
        {
            int regexIndex = 0;
            foreach (Regex regexp in regexps)
            {
                var matches = regexp.Matches(text);
                if (matches.Count > 0)
                {
                    return matches.
                        OfType<Match>().
                        Select(match => new StringSearchResult(match.Index, match.Value, regexIndex)).
                        ToList();
                }
                regexIndex++;
            }

            return new List<StringSearchResult>();
        }
        
        private static StringSearchResult ParseFirstByRegexp(string regionText, Regex regexp, int groupIndex)
        {
            return ParseFirstByRegexp(regionText, new [] { regexp }, groupIndex);
        }

        public static StringSearchResult ParseFirstByRegexp(string regionText, IEnumerable<Regex> regexps, int groupIndex)
        {
            int regexIndex = 0;
            foreach (Regex regexp in regexps)
            {
                Match match = regexp.Match(regionText);
                if (match.Success)
                {
                    return new StringSearchResult(match.Groups[groupIndex].Index, match.Groups[groupIndex].Value, regexIndex);
                }

                regexIndex++;
            }

            return StringSearchResult.Empty;
        }
    }

    public static class ParsingTextHelper
    {
        public static int NamePersonRegionMaxLength = 1400;
        public static int NamePersonMaxLength = 450;

        public static StringSearchResult OffsetResult(this StringSearchResult result, int offset)
        {
            return result.IsEmpty ? result : new StringSearchResult(result.Index + offset, result.Keyword, result.RegexIndex);
        }
    }
}
