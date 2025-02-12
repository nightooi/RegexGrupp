using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexGrupp
{
    internal class DataFetch
    {
        private const string _dataFile = "..\\Assets\\tempdata5-med fel.txt";
        public StreamReader GetDataReader()
        {
           return new StreamReader(_dataFile);
        }
    }
}
