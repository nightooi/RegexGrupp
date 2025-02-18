using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexGrupp
{
    internal class Menu
    {
        private Parser parser;
        public Menu()
        {
            this.parser = new Parser();
            Task.Run(parser.CalcResultsAsync);

        }
        public void displayWinter()
        {
            parser.FindWinter();
        }
        public void displayAutumn()
        {
            parser.FindAutumn();
        }
        public void selectDateInside()
        {
            Console.WriteLine("Insert Date");
            var input = Console.ReadLine();
            parser.AssertDateDay(input);
            Task.Run(parser.AverageHumidityInsidePerDayAsync);
            Task.Run(parser.AverageTemInsidepAsync);
        }
        public void selectOutside()
        {
            Console.WriteLine("Insert Date");
            var input = Console.ReadLine();
            parser.AssertDateDay(input);
            Task.Run(parser.AverageTempOutsideAsync);
        }
        public void run()
        {
            Console.WriteLine("1. Select Date Inside");
            Console.WriteLine("2. Select Date Outside");
            Console.WriteLine("3. Display Winter");
            Console.WriteLine("4. Display Autumn");
            Console.WriteLine("5. Select Sort");
            Console.WriteLine("6. Exit");
            var result = Console.ReadLine();
            int parseresult = -1;
            if (!int.TryParse(result, out parseresult) && parseresult > 0 && parseresult < 7)
            {
                Console.WriteLine("Invalid input");
                return;
            }
            if (parseresult == 1)
            {
                selectDateInside();
            }
            else if (parseresult == 2)
            {
                selectOutside();
            }
            else if (parseresult == 3)
            {
                displayWinter();
            }
            else if (parseresult == 4)
            {
                displayAutumn();
            }
            else if (parseresult == 5)
            {
                selectSort();
            }
            else
            {
                return;
            }
            run();
        }
        public void selectSort()
        {
            Console.WriteLine("Select Sort");
            Console.WriteLine("1. Inside");
            Console.WriteLine("2. Outside");

            var result = Console.ReadLine();
            int parseresult = -1;
            if (!int.TryParse(result, out parseresult) && parseresult > -1 && parseresult < 3)
            {
                Console.WriteLine("Invalid input");
                return;
            }
            Parser.SortBy sort;
            if (parseresult == 1)
            {
                sort = Parser.SortBy.Inside;
            }
            else
            {
                sort = Parser.SortBy.Outside;
            }

            Console.WriteLine("1. Mold");
            Console.WriteLine("2. Temp");
            Console.WriteLine("3. Humidity");
            Console.WriteLine("4. Exit");
            Console.ReadLine();
            var choice = Console.ReadLine();
            int choiceParsed = -1;

            if (!int.TryParse(choice, out choiceParsed) && choiceParsed > 0 && choiceParsed < 5)
            {
                Console.WriteLine("Invalid input");
                return;
            }
            if (choiceParsed == 1)
            {
                sort = sort | Parser.SortBy.Mold;
            }
            else if (choiceParsed == 2)
            {
                sort = sort | Parser.SortBy.Temp;
            }
            else if (choiceParsed == 3)
            {
                sort = sort | Parser.SortBy.Humidity;
            }
            else
            {
                return;
            }
            parser.Sort(sort);
        }
    }
}
