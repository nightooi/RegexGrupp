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
        string _endOfDayRegex = string.Empty;
        string _dayRegex = string.Empty;
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
        private static readonly string _AssertDateFormat = @"(?<Date>(?<Year>20\d{2})(?<del>[-/])(?<Month>((0[1-9]))|(1[0-2]))(\k<del>)(?<DayCheck>(0[1-9]|1[0-9]|2[0-8])|((?<Valid31>(?<!(0[246]|11)((\k<del>)))31)|(?<Valid30>(?<!02(\k<del>))(29|30)))))";
        private static readonly string _AssertDateRange = @"(?<Date>(?<year>(2016))(?<deli>([/-]))(?<month>(0[6-9]|1[1-2]))(\k<deli>)(?<Day>(0[1-9]|1[0-9]|2[0-9]|30)|(?((?<!(0[9]|11))((\k<deli>)31))|(?<=(0[78]|1[0-2])(\k<deli>))31)))";
        private static readonly string _FindByDate       = @"((?<Date>((<DateStart>)))(?:(?!\d)(\W?))(?<time>(0[7-9]|1[0-7]):\d{2}:\d{2})(?:(?!\d)(\W?))(?<sensorPos>[Ii]{1,4}[Nn]{1,4}[Ee]{1,4})(?:(?!\d)(\W?))(?( |\t| \w))(?<temp>(\d{2})\.\d)(?:(?!\d)(\W?))(?<humidity>\d{2}))";
        private static readonly string _FindByDateDayEnd = @"((?<Date>((<DateStart>)))(?:(?!\d)(\W?))(?<time>(1[8-9]|2[0-3]|00):\d{2}:\d{2})(?:(?!\d)(\W?))(?<sensorPos>[Ii]{1,4}[Nn]{1,4}[Ee]{1,4})(?:(?!\d)(\W?))(?( |\t| \w))(?<temp>(\d{2})\.\d)(?:(?!\d)(\W?))(?<humidity>\d{2}))";

       // public static IEnumerable<string> AverageTemp(string UserInput)
       // {
       //     var res = AssertDateDay(UserInput);
       // }
        public async Task AverageHumidity(string input)
        {
            var evalfile = new EvaluateFile();
            await foreach(var listMatch in evalfile.EnumerableStringsAsync(FindEnd, FindEnd))
            {
                foreach(var line in listMatch)
                {
                    if(line != string.Empty)
                    {
                        Console.ReadLine();
                    }
                    Console.WriteLine(line);
                }
            }
        }
        public bool FindStart(string lineFromFile)
        {
            var file = new EvaluateFile();
            var startRegex = Regex.Replace(_dayRegex, "(<HourStart>)", "07");

            Regex reg = new(startRegex);
            bool som;
            if (som = reg.IsMatch(lineFromFile))
            {
                Console.Read();
            }
            return som;
        }
        public bool FindEnd(string lineFromFile)
        {
            var file = new EvaluateFile();
            Regex reg = new(_endOfDayRegex);
            var regexRex = reg.Match(lineFromFile);
            var boolRes = false;
            if (boolRes = regexRex.Groups.Values.Where(x=> x.Name == "Date").FirstOrDefault().Success)
            {
                Console.WriteLine("End OF Read");
            }
            return boolRes;
        }
        public string AssertDateDay(string userinput)
        {
            string str = string.Empty;
            var regex = new Regex(_AssertDateFormat);
            var matchRes = regex.Match(userinput);
            if(!matchRes.Success)
            {
                foreach(Group match in matchRes.Groups)
                {
                    FormatAssertionFailed(match.Name);
                }
            }
            if (!matchRes.Success)
                return null;

            var checkDateRegex = new Regex(_AssertDateRange);
            var matches = checkDateRegex.Match(userinput);
            if (!matches.Groups["Date"].Success)
            {
                foreach (Group match in matches.Groups.Values.Where(x => !x.Success)) 
                {
                    FormatAssertionFailed(match.Name);
                }

            }
            var res = Regex.Replace(_FindByDate, "(<DateStart>)", userinput);
           _endOfDayRegex = Regex.Replace(_FindByDateDayEnd, "(<DateStart>)", userinput);
            _dayRegex = res;
            return res;
        }
        private void FormatAssertionFailed(string groupName)
        {
           // Console.WriteLine($"{(groupName == "del" ? "Delitmiter" : groupName)}: Incorrect Format expected: {DateFormats[groupName]}");
            Console.WriteLine($"{(groupName == "del" ? "Delitmiter" : groupName)}: Incorrect Format expected");
        }
    }
}
