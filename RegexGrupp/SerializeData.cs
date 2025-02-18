using Microsoft.Win32.SafeHandles;

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
        }
        public async Task Quit()
        {
            if(_resultsPerDay.Count > 0)
            {
                await WriteAsync();
            }
        }
        private async Task WriteAsync()
        {
            using FileStream fs = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            using(var writer =  new StreamWriter(fs))
            {
                foreach (var item in _resultsPerDay.GroupBy(x=> x.Position))
                {
                    foreach(var pos in item)
                        await writer.WriteLineAsync($"{pos.Date.Month}: T:\t {pos.AverageTemp}, H:\t {pos.AverageHumidity}, M:\t {pos.MoldRisk}, P:\t {pos.Position.ToString()}");
                    await writer.WriteLineAsync("----------------------------------------------------------------------------------");
                }
            }
        }
    }
}
