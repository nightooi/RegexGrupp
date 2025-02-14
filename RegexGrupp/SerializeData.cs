using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexGrupp
{
    internal class SerializeData
    {
        private List<ResultsPerDay> _resultsPerDay;

        private string _path = "..\\..\\..\\Results\\Result.txt";
        private string _subDirPath = "..\\..\\..\\Results\\";
        object lockRes = new();
        public async Task Add(ResultsPerDay result)
        {
            if (_resultsPerDay is null)
                _resultsPerDay = new();

            _resultsPerDay.Add(result);
            if(_resultsPerDay.Count > 20)
            {
                await WriteAsync();
                _resultsPerDay.Clear();
            }
        }
        private async Task Quit()
        {
            if(_resultsPerDay.Count > 0)
            {
                await WriteAsync();
            }
        }
        private async Task WriteAsync()
        {
            using FileStream fs = new FileStream(_path, FileMode.Create, FileAccess.ReadWrite);

            using(var writer =  new StreamWriter(fs))
            {
                foreach (var item in _resultsPerDay)
                    await writer.WriteLineAsync($"{item.Date}: T:\t {item.AverageTemp}, H:\t {item.AverageHumidity}, M:\t {item.MoldRisk}, P:\t {item.Position.ToString()}");
            }
        }
    }
}
