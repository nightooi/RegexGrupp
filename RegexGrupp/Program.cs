// See https://aka.ms/new-console-template for more information
using RegexGrupp;


var ins = new Parser();
Console.WriteLine("Insert Date");
var input = "2016-06-01";
ins.AssertDateDay(input);
await ins.CalcResultsAsync();
ins.FindAutumn();
ins.FindWinter();

Console.ReadLine();
