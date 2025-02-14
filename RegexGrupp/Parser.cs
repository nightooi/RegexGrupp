using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace RegexGrupp
{
    internal class Parser
    {
        //◦ Medeltemperatur och luftfuktighet per dag, för valt datum (sökmöjlighet med
        //validering)

        //(\d[{2,4} ^16])-(\d[{1,2} ^0])-(\d[{1,2}]+)
        //2016-05-31 14:22:27,Inne,24.9,42
        //
        object listLock = new();
        List<ResultsPerDay> ResultsPerDay { get; set; }
        private delegate Func<IEnumerable<string>, TResult> SelectSearchEndoFunctor<out TResult>(string position, string data);
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
        private const string _dateAssert = "<ASSERTDATE>";
        private static DataFetch _fetchItems = new DataFetch();
        private static string _FindData = "((<ASSERTDATE>))(\\s|\\W)*?(?<time>((0[0-9]|1[0-9]|2[0-3]):(\\d{0,2}):(\\d{0,2})))(?:(\\W*?))(?<sensorPos>[Ii]{1,4}[Nn]{1,4}[Ee]{1,4}|[Uu]{1,4}[Tt]{1,4}[Ee]{1,4})(?:(\\W*?))(?<temp>\\d{0,2}\\.\\d{0,2})(?:(\\W*?))(?<humidity>\\d{0,2})(\\s|\\W)*?$";
        private static readonly string _Position = "<POSITION>";
        private static readonly string _AssertDateFormat = @"(?<Date>(?<Year>20\d{2})(?<del>[-/])(?<Month>((0[1-9]))|(1[0-2]))(\k<del>)(?<DayCheck>(0[1-9]|1[0-9]|2[0-8])|((?<Valid31>(?<!(0[246]|11)((\k<del>)))31)|(?<Valid30>(?<!02(\k<del>))(29|30)))))";
        private static readonly string _AssertDateRange =  @"(?<Date>(?<year>(2016))(?<deli>([/-]))(?<month>(0[6-9]|1[0-2]))(\k<deli>)(?<Day>(0[1-9]|1[0-9]|2[0-9]|30)|(?((?<!(0[9]|11))((\k<deli>)31))|(?<=(0[78]|1[0-2])(\k<deli>))31)))";
        private static readonly string _FindByDateDayEnd = @"(?<Date><DateStart>)(?:(\W*?))(?<time>((00:\d{2}:\d{2})|((0[1-9]|1[0-8]):\d{2}:\d{2})))(?:(\W*?))(?<sensorPos><POSITION>)(?:(\W*?))(?<temp>\d{2}\.\d)(?:(\W*?))(?<humidity>\d{2})";
        private static readonly string _FindByDate       = @"(?<Date><DateStart>)(?:(\s\W*?))(?<time>(((0[0-9]|1[0-9]|(2[0-3])):\d{2}:\d{2})))";
        private static readonly string _FindEnd =          @"(?<Date><DateStart>)(?:(\s\W*?))(?<time>(23:59:\d{2}))";
        private static readonly string _Inside = @"[Ii]{1,4}[Nn]{1,4}[Ee]{1,4}";
        private static readonly string _Outside = @"[Uu]{1,4}[Tt]{1,4}[Ee]{1,4}";
        public async Task<float> AverageTemInsidepAsync()
        {
             Action<string> display = (result) =>
            {
                Console.WriteLine($"The Average Temperature Inside was: {result}");
            };
            return await CalcAveragePerDay("inside", "temp", BuildEndo, display);
        }
        public async Task SortAverageHumidity()
        {
            DateOnly dateOnly = new DateOnly(2016, 06, 01);
            const int daysLeft = 140;
            
            for(int i =0; i < daysLeft; i++)
            {
                this.AssertDateDay(dateOnly.AddDays(i).ToString());

            }
        }
        public async Task<float> AverageHumidityInsidePerDayAsync()
        {
            Action<string> display = (result) =>
            {
                Console.WriteLine($"The Avereage Humidity Inside on the given day Was {result}");
            };
            return await CalcAveragePerDay("inside", "humidity", BuildEndo, display);
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
            var searchPatter = Regex.Replace(searchPattern, $"{_dateAssert}", _AssertDateRange);
            Regex regex = new Regex(searchPattern, RegexOptions.Multiline, TimeSpan.FromSeconds(10));

            return (section)=> { 
                var sb = new StringBuilder();
            foreach(var line in section)
            {
                sb.AppendLine(line);
            }
            return regex.Matches(sb.ToString());};
        }
        public void CalcMold()
        {
            var mold = new Mold();
            var orderedResults = ResultsPerDay.OrderBy(x => x.Date);
            int i = 0;
            foreach(var item in orderedResults)
            {
                if(i > 0)
                {
                    item.MoldRisk = mold.CalcMoldRisk(item.AverageHumidity, item.AverageTemp, orderedResults.ElementAt(i-1).MoldRisk);
                    continue;
                }
                item.MoldRisk = mold.CalcMoldRisk(item.AverageHumidity, item.AverageTemp, 0.0f);
                i++;
            }
        }
        public async Task CalcResultsAsync()
        {
            this.ResultsPerDay = await CalcBoth();
            var serialize = new SerializeData();
            foreach(var i in ResultsPerDay)
            {
                await serialize.Add(i);
                Console.WriteLine($"{i.Date}: {i.AverageHumidity} - {i.AverageTemp} - {i.Position}");
            }
        }
        [Flags]
        public enum SortBy { Inside, Outside, Humidity, Temp, Mold}
        public void FindAutumn()
        {
            FindWindow(new DateOnly(2016, 08, 01), 10.0f, (obj) => {
                Console.WriteLine($"First day of Autumn in the Year 2016 was {obj.Date}");
            });
        }
        public void FindWinter()
        {
            FindWindow(new DateOnly(2016, 08, 01), 0.0f, (obj) =>
            {
                Console.WriteLine($"First day of Winter in the Year 2016 was {obj.Date}");
            });
        }
        public void FindWindow(DateOnly Starting, float Temp, Action<ResultsPerDay> Display)
        {
            var res = ResultsPerDay
                .Where(x => x.Date > Starting)
                .Where(x => x.Position == Position.Outside)
                .OrderBy(x => x.Date)
                .ToArray();
            int highestConsecutive = 0;
            int offset = 0;
            for(int i = 0; i < res.Length; i++)
            {
                ResultsPerDay?[] window = new ResultsPerDay[5];
                for(int k = 0; k < 5; k++)
                {
                    if (i+k+1 < res.Length)
                    {
                        window[k] = (res[i + k].Date.AddDays(1) == res[i + k + 1].Date) ? res[i + k] : null;
                        if (window[k] == null)
                        {
                            i = i + k;
                            break;
                        }
                    }
                }
                var items = window.TakeWhile(x => x?.AverageTemp < Temp).ToList();
                if(items.Count == 5)
                {
                    Display(items[0]);
                    return;
                }
                if(items.Count > highestConsecutive)
                {
                    highestConsecutive = items.Count;
                    offset = i;
                }
                for (int j = 0; j < window.Length; j++)
                    if (window[j]?.AverageTemp > Temp)
                        i = i + j;
            }
            Console.WriteLine($"Couldn't assert in the current dataset. Closes result was{res[offset]}: with {highestConsecutive} days consecutive");
        }
        public void Sort(SortBy sort)
        {
            if(sort == SortBy.Inside && sort == SortBy.Humidity)
            {
                var humidInside = ResultsPerDay
                    .Where(x => x.Position == Position.Inside)
                     .OrderBy(x => x.AverageHumidity)
                     .ToList();
                ShowEnumerated(humidInside);
            }
            else if(sort == SortBy.Inside && SortBy.Temp == sort)
            {
                var tempInside = ResultsPerDay
                    .Where(x => x.Position == Position.Inside)
                    .OrderBy(x => x.AverageTemp)
                    .ToList();
                ShowEnumerated(tempInside);
            }
            else if(sort == SortBy.Inside && SortBy.Mold == sort)
            {
                var tempInside = ResultsPerDay
                    .Where(x => x.Position == Position.Inside)
                    .OrderBy(x => x.MoldRisk)
                    .ToList();
                ShowEnumerated(tempInside);
            }
            else if(sort == SortBy.Outside && sort == SortBy.Humidity)
            {
                var humidOutside = ResultsPerDay
                    .Where(x => x.Position == Position.Inside)
                    .OrderBy(x => x.AverageHumidity)
                    .ToList();
                ShowEnumerated(humidOutside);
            }
            else if(sort == SortBy.Outside && sort == SortBy.Temp)
            {
                var humidOutside = ResultsPerDay
                    .Where(x => x.Position == Position.Inside)
                    .OrderBy(x => x.AverageTemp)
                    .ToList();
                ShowEnumerated(humidOutside);
            }
            else if(sort == SortBy.Outside && sort == SortBy.Mold)
            {
                var humidOutside = ResultsPerDay
                    .Where(x => x.Position == Position.Inside)
                    .OrderBy(x => x.MoldRisk)
                    .ToList();
                ShowEnumerated(humidOutside);
            }
        }
        public void ShowEnumerated(List<ResultsPerDay> results)
        {
            foreach(var item in results)
            Console.WriteLine($"{item.Date}: H {item.AverageTemp} - T {item.AverageHumidity}");
        }
        private Func<IEnumerable<string>, MatchCollection> BuildEndoTRes(string position, string data)
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
        private async Task<List<ResultsPerDay>> CalcBoth()
        {
            List<ResultsPerDay> results = new();
            string inside = _FindData;
            string outside = _FindData;
            var joinedPos = string.Join("|", new string[]{ _Inside, _Outside });
            var regexExpression = Regex.Replace(_FindData, $"({_Position})", joinedPos);
            regexExpression = Regex.Replace(regexExpression, $"({_dateAssert})", _AssertDateRange);
            var evalFile = new EvaluateFile();
            var regex = new Regex(regexExpression, RegexOptions.Multiline, TimeSpan.FromSeconds(10));

            var res =  await evalFile.EvalWholeSectionAsync(FindDate(new DateOnly(2016, 06, 01), true), FindDate(new DateOnly(2016, 12, 31), false),
                (x)=>
            {
                return regex.Matches(MatchesJoined(x));
            });
            foreach(var i in res)
            {
                foreach(var x in HandleResults(i))
                {
                    UpdateIfExists(results, x);
                }
            }
            return results;
        }
        private List<ResultsPerDay> HandleResults(MatchCollection matches)
        {
            List<ResultsPerDay> resultPerday = new();
            var dateAsstert = new Regex(_AssertDateRange);

            // "(?:.+?)(?<time>((07:\\d{2}:\\d{2})|((0[7-9]|1[0-8]):\\d{2}:\\d{2})))(?:(\\W*?))(?<sensorPos><POSITION>)(?:(\\W*?))(?<temp>\\d{2}\\.\\d)(?:(\\W*?))(?<humidity>\\d{2})";
            var positionMatch = new Regex(_Inside);
                var items = matches
                    .Where(x => x.Success)
                    .Select(x => new
                    {
                        pos = x.Groups["sensorPos"].Value,
                        date = x.Groups["Date"].Value,
                        temp = x.Groups["temp"].Value,
                        humidity = x.Groups["humidity"].Value
                    })
                    .GroupBy(x => new { pos = x.pos, date = x.date })
                    .Select(x => new
                    {
                        date = x.Key.date,
                        pos = x.Key.pos,
                        temp = x.Select(x => x.temp).ToList(),
                        humidity = x.Select(x => x.humidity).ToList()
                    });
                var parsed = ItemTransformer(items, (i) =>
                {
                    Position pos = Position.Outside;
                    if (positionMatch.IsMatch(i.pos))
                    {
                        pos = Position.Inside;
                    }
                       var current = new ResultsPerDay(
                        pos,
                        DateOnly.Parse(i.date),
                        CalcAverage(i.temp),
                        (int)CalcAverage(i.humidity)
                    );
                    return current;
                });
                foreach(var i in parsed)
                {
                    UpdateIfExists(resultPerday, i);
                }
            return resultPerday;
        }
       // private IEnumerable<float> AdjustTemp(float[] temp, DateTime[] DateTime)
       // {
       //     for(int i = 0; i <temp.Length; i++)
       //     {
       //         if (DateTime[i].Month == 1)
       //         {
       //         }
       //     }
       // }
        private float BiasBasedOnTimeAndMonth(DateTime dateTime)
        {
            List<double> MonthBiasesMax = [0.0063, 0.00932, 0.00734, 0.00634, 0.0048, 0.0032];
            if (dateTime.TimeOfDay.Hours > 09.00 - (dateTime.Month -5) && dateTime.TimeOfDay.Hours <= 16 - (dateTime.Month - 6))
            {
                return (float)double.Lerp(0.000, MonthBiasesMax.ElementAt(dateTime.Month - 6), dateTime.TimeOfDay.Hours + dateTime.TimeOfDay.Minutes / 60 / 24.0);
            }
            else
                return (float)(1.0 - double.Lerp(0.000, MonthBiasesMax.ElementAt(dateTime.Month - 6), dateTime.TimeOfDay.Hours + dateTime.TimeOfDay.Minutes / 60 / 24.0));
        }
        private void UpdateIfExists(List<ResultsPerDay> resultPerDay, ResultsPerDay current)
        {
            ResultsPerDay? results;
            if((results = resultPerDay.SingleOrDefault(x=> x.Date == current.Date && x.Position == current.Position)) is not null)
            {
                results.AverageHumidity = (results.AverageHumidity + current.AverageHumidity) / 2;
                results.AverageTemp = (results.AverageTemp + current.AverageTemp) / 2;
            }
            if(results is null)
            resultPerDay.Add(current);
        }
        private IEnumerable<ResultsPerDay> ItemTransformer<T>(IEnumerable<T> items, Func<T, ResultsPerDay> transformer)
        {
            List<ResultsPerDay> results = new();
            foreach(var item in items)
            {
                results.Add(transformer(item));
            }
            return results;
        }
        private string MatchesJoined(IEnumerable<string> lines)
        {
            StringBuilder sb = new();
            sb.AppendJoin('\n', lines);
            return sb.ToString();
        }
        private Func<string, bool> FindDate(DateOnly date, bool morning)
        {
            string expression = string.Empty;
            const string dateFormat = "yyyy-MM-dd";
            if (morning)
            {
                expression = Regex.Replace(_FindByDate, "(<DateStart>)", date.ToString(dateFormat));
            }
            else
                expression = Regex.Replace(_FindEnd, "(<DateStart>)", date.ToString(dateFormat));
            var regex = new Regex(expression, RegexOptions.Multiline, TimeSpan.FromSeconds(10));
            return (line) =>
            {
                return regex.IsMatch(line);
            };
        }
        private float CalcAverage(IEnumerable<string> items)
        {
            return items.Select(x => float.Parse(x))
                .Average();
        }
        private async Task<float> CalcAveragePerDay(string position, string data, SelectSearchEndoFunctor<MatchCollection> buildSearch, Action<string> display)
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
            return temp;
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
