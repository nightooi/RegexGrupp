using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace RegexGrupp
{
    internal class Inside
    {
        //◦ Medeltemperatur och luftfuktighet per dag, för valt datum (sökmöjlighet med
        //validering)

        //(\d[{2,4} ^16])-(\d[{1,2} ^0])-(\d[{1,2}]+)
        //2016-05-31 14:22:27,Inne,24.9,42
        //
        private static Dictionary<string, string> DateFormats = new([
            new KeyValuePair<string, string>("Year", "20[0-99] digit expected"),
            new KeyValuePair<string, string>("Month", "0-12 digit expected"),
            new KeyValuePair<string, string>("Day", "Must follow calendar date rules, including leap year, digit expected"),
            new KeyValuePair<string, string>("del", "Allowed delimiter is / or -")]);

        private static Dictionary<string, string> DateAssertionFormat = new([
            new KeyValuePair<string, string>("Year", "2016 expected, no other result in document"),
            new KeyValuePair<string, string>("Month", "6-12 expected, digit expected"),
            new KeyValuePair<string, string>("Day", "Must follow calendar date rules, including leap year, digit expected"),
            new KeyValuePair<string, string>("del", "Allowed delimiter is / or -")]);
        private static DataFetch _fetchItems = new DataFetch();
        private static readonly string _AssertDateFormat = @"^(?<Date>(?<Year>20\d{2})(?<del>[-/])(?<Month>((0[1-9]))|(1[0-2]))(\k<del>)(?<DayCheck>(0[1-9]|1[0-9]|2[0-8])|((?<Valid31>(?<!(0[246]|11)((\k<del>)))31)|(?<Valid30>(?<!02(\k<del>))(29|30)))))";
        private static readonly string _AssertDateRange = @"^(?<Date>(?<year>(2015))(?<deli>([/-]))(?<month>(0[6-9]|1[1-2]))(\k<deli>)(?<Day>(0[1-9]|1[0-9]|2[0-9]|30)|(?((?<!(0[9]|11))((\k<deli>)31))|(?<=(0[78]|1[0-2])(\k<deli>))31)))";
        private static readonly string _FindByDate = @"^(?<Date><UserInput>)(?:(?!\d)(\W?))(?<time>\d{2}:\d{2}:\d{2})(?:(?!\d)(\W?))(?<sensorPos>[Ii]{1,4}[Nn]{1,4}[Ee]{1,4})(?:(?!\d)(\W?))(?( |\t| \w))(?<temp>(\d{2})\.\d)(?:(?!\d)(\W?))(?<humidity>\d{2})";

        public static IEnumerable<string> AverageTemp(string UserInput)
        {
            var res = AssertDateDay(UserInput);
        }
        public async Task<IEnumerable<string>> AverageHumidity()
        {
            var data = await _fetchItems.GetData();
            var regex = new Regex(_FindDate);
            List<Match> matches = new();
            foreach(var item in data)
            {
                matches.Add(regex.Match(item));
            }
        }

        private Regex AssertDateDay(string userinput)
        {
            string str = string.Empty;
            var regex = new Regex(_AssertDateFormat);
            var matchRes = regex.Match(userinput);
            bool failed = false;
            foreach(Group match in matchRes.Groups)
            {
                if(!(failed = match.Success) && (match.Name != string.Empty || match.Name ==" "))
                {
                    FormatAssertionFailed(match.Name);
                }
            }
            if (failed)
                return null;
            var checkDateRegex = new Regex(_AssertDateRange);
            var matches = checkDateRegex.Match(userinput);
            if(!matches.Groups.Values.Single(x=> x.Name == "Date").Success)
            {
                foreach (Group match in matches.Groups.Values.Where(x => !x.Success)) 
                {
                    FormatAssertionFailed(match.Name);
                }

            }
            var res = Regex.Replace(_FindByDate, "(<UserInput>)", "userinput");
            return new Regex(res);
        }
        private void FormatAssertionFailed(string groupName)
        {
            Console.WriteLine($"{(groupName == "del" ? "Delitmiter" : groupName)}: Incorrect Format expected: {DateFormats[groupName]}");
        }
    }
}
