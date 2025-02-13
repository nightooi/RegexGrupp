using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;
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
        private delegate Func<IEnumerable<string>, MatchCollection> SelectSearchEndoFunctor(string position, string data);
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
        private Regex SectionEvaluation = new Regex(_FindData, RegexOptions.Multiline, TimeSpan.FromSeconds(10));

        private static DataFetch _fetchItems = new DataFetch();
        private static string _FindData = "(?:.+?)(?<time>((07:\\d{2}:\\d{2})|((0[7-9]|1[0-8]):\\d{2}:\\d{2})))(?:(\\W*?))(?<sensorPos><POSITION>)(?:(\\W*?))(?<temp>\\d{2}\\.\\d)(?:(\\W*?))(?<humidity>\\d{2})";
        private static readonly string _Position = "<POSITION>";
        private static readonly string _AssertDateFormat = @"(?<Date>(?<Year>20\d{2})(?<del>[-/])(?<Month>((0[1-9]))|(1[0-2]))(\k<del>)(?<DayCheck>(0[1-9]|1[0-9]|2[0-8])|((?<Valid31>(?<!(0[246]|11)((\k<del>)))31)|(?<Valid30>(?<!02(\k<del>))(29|30)))))";
        private static readonly string _AssertDateRange = @"(?<Date>(?<year>(2016))(?<deli>([/-]))(?<month>(0[6-9]|1[1-2]))(\k<deli>)(?<Day>(0[1-9]|1[0-9]|2[0-9]|30)|(?((?<!(0[9]|11))((\k<deli>)31))|(?<=(0[78]|1[0-2])(\k<deli>))31)))";
        private static readonly string _FindByDateDayEnd = @"(?<Date><DateStart>)(?:(\W*?))(?<time>((19:\d{2}:\d{2})|((0[7-9]|1[0-8]):\d{2}:\d{2})))(?:(\W*?))(?<sensorPos><POSITION>)(?:(\W*?))(?<temp>\d{2}\.\d)(?:(\W*?))(?<humidity>\d{2})";
        private static readonly string _FindByDate       = @"(?<Date><DateStart>)(?:(\W*?))(?<time>((07:\d{2}:\d{2})|((0[7-9]|1[0-8]):\d{2}:\d{2})))";
        private static readonly string _FindEnd =          @"(?<Date><DateStart>)(?:(\W*?))(?<time>((19:\d{2}:\d{2})|(2[0-3]):\d{2}:\d{2}))";
        private static readonly string _Inside = @"[Ii]{1,4}[Nn]{1,4}[Ee]{1,4}";
        private static readonly string _Outside = @"[Uu]{1,4}[Tt]{1,4}[Ee]{1,4}";
        public async Task AverageTemInsidepAsync()
        {
             Action<string> display = (result) =>
            {
                Console.WriteLine($"The Average Temperature Inside was: {result}");
            };
            await CalcAveragePerDay("inside", "temp", BuildEndo, display);
        }
        public async Task AverageHumidityInsidePerDayAsync()
        {
            Action<string> display = (result) =>
            {
                Console.WriteLine($"The Avereage Humidity Inside on the given day Was {result}");
            };
            await CalcAveragePerDay("inside", "humidity", BuildEndo, display);
        }
        private Func<IEnumerable<string>, MatchCollection> BuildEndo(string position, string data)
        {
            string Insertion = string.Empty;
            if(position == "inside")
            {
                Insertion = _Inside;
            }
            if(position == "outside")
            {
                Insertion = _Outside;
            }
           var searchPattern = Regex.Replace(_FindData, $"({_Position})", Insertion);
            Regex regex = new Regex(searchPattern, RegexOptions.Multiline, TimeSpan.FromSeconds(10));

            return (section)=> { 
                var sb = new StringBuilder();
            foreach(var line in section)
            {
                sb.AppendLine(line);
            }
            return regex.Matches(sb.ToString());};
        }
        private async Task CalcAveragePerDay(string position, string data, SelectSearchEndoFunctor buildSearch, Action<string> display)
        {
            var evalfile = new EvaluateFile();
            var matchedData = await evalfile.EvalWholeSectionAsync(FindStart, FindEnd, buildSearch(position, data));
            float temp = 0.0f;
            int len = 0;
            foreach(var item in matchedData)
            {
                var res = item
                    .Where(x => x.Groups[data].Success)
                    .Select(x => float.Parse(x.Groups[data].Value))
                    .ToList();

                foreach (var i in res)
                    temp += i;
                len += res.ToArray().Length;
            }
            var average = temp / len;
            display(average.ToString());
        }
        private MatchCollection FindData(IEnumerable<string> section)
        {
            var sb = new StringBuilder();
            foreach(var line in section)
            {
                sb.AppendLine(line);
            }
            return SectionEvaluation.Matches(sb.ToString());
        }
        private bool FindStart(string lineFromFile)
        {
            var file = new EvaluateFile();
            Regex reg = new(_dayRegex);
            bool som = reg.IsMatch(lineFromFile);
            return som;
        }
        private bool FindEnd(string lineFromFile)
        {
            var file = new EvaluateFile();
            Regex reg = new(_endOfDayRegex);
            var regexRex = reg.IsMatch(lineFromFile);
            if (regexRex)
            {
                foreach(Group group in reg.Match(lineFromFile).Groups.Values.Where(x=> x.Value != null))
                Console.WriteLine(group.Value);
            }
            return regexRex;
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
            _dayRegex = Regex.Replace(_FindByDate, "(<DateStart>)", userinput);
           _endOfDayRegex = Regex.Replace(_FindEnd, "(<DateStart>)", userinput);
            return _dayRegex;
        }
        private void FormatAssertionFailed(string groupName)
        {
           // Console.WriteLine($"{(groupName == "del" ? "Delitmiter" : groupName)}: Incorrect Format expected: {DateFormats[groupName]}");
            Console.WriteLine($"{(groupName == "del" ? "Delitmiter" : groupName)}: Incorrect Format expected");
        }
    }
}
