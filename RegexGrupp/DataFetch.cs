using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexGrupp
{
    internal class DataFetch
    {
        private const string _dataFile = "..\\..\\..\\Assets\\tempdata5-med fel.txt";
        long lastIndex;
        private StreamReader reader;
        public StreamReader GetDataReader()
        {
           return new StreamReader(_dataFile);
        }
        public void PauseRead()
        {
            lastIndex = reader.BaseStream.Position;
            reader.Dispose();
        }
        public void ResumeRead()
        {
            reader = new(_dataFile);
            reader.BaseStream.Position = lastIndex;
        }
    }
}
