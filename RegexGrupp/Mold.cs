using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexGrupp
{
    internal class Mold
    {
        public float CalcMoldRisk(int humidity, float temp, float previousRisk)
        {
            float result = 0.0f;
            if (previousRisk > 0)
                result = (float)(DirectRisk(humidity, temp) + ((DirectRisk(humidity, temp) - previousRisk) * 0.2));
            else
                result = DirectRisk(humidity, temp);

            if (result > 99)
                return 99;
            else if (result < 0)
                return 0;
            else
                return result;
        }
        private float DirectRisk(int humidity, float temp)
        {
            return (float)((humidity - 78) * (temp / 15) / 0.22);
        }
    }
}
