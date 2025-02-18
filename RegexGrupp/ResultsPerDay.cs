using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RegexGrupp
{
    internal class ResultsPerDay(Position pos, DateOnly date, float temp, int humidity, float? moldRisk)
    {
        public Position Position { get; set; } = pos;
        public DateOnly Date { get; set; } = date;
        public float AverageTemp { get; set; } = temp;
        public int AverageHumidity { get; set; } = humidity;
        public float? MoldRisk { get; set; } = moldRisk;
    }
}
internal enum Position { Inside, Outside}
