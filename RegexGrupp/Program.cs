// See https://aka.ms/new-console-template for more information
using RegexGrupp;


var ins = new Inside();
Console.WriteLine("Insert Date");
var input = "2016-06-01";
ins.AssertDateDay(input);
await ins.AverageTemInsidepAsync();
await ins.AverageHumidityInsidePerDayAsync();
Console.ReadLine();
