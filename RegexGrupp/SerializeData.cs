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
        public async Task Add(ResultsPerDay result)
        {
            if (_resultsPerDay is null)
                _resultsPerDay = new();

            _resultsPerDay.Add(result);
            if(_resultsPerDay.Count > 20)
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
                    await writer.WriteLineAsync($"{item.Date}: {item.AverageTemp}, {item.AverageHumidity}, {item.Position.ToString()}");
            }
        }
    }
}
