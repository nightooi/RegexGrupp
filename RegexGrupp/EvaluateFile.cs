using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace RegexGrupp
{
    internal class EvaluateFile
    {
        private DataFetch _dataFetch;
        private async IAsyncEnumerable<IEnumerable<string>> EnumerableStringsAsync(Func<string, bool> sectionMatchStart,
            Func<string, bool> sectionMatchEnd)
        {
            List<Match> Matches = new();
            using (var reader = (_dataFetch = new()).GetDataReader())
            {
                List<Task<Match>> matchTasks = new();
                string[] lineBuffer = new string[128];
                string? currentLine = string.Empty;
                int i = 0;
                bool start = false;
                List<CancellationTokenSource> TaskCancel = new();
                while((currentLine = await reader.ReadLineAsync()) is not null){
                    if (sectionMatchStart(currentLine) && !start)
                        start = true;

                    if (start && !sectionMatchEnd(currentLine))
                    {
                        lineBuffer[i] = currentLine;
                        i++;
                        if (i == 128)
                        {
                            yield return lineBuffer;
                            i = 0;
                            lineBuffer = new string[128];
                        }
                        continue;
                    }

                    yield return ClearRemainder(lineBuffer, i, 128);
                }
                yield return ClearRemainder(lineBuffer, i, 128);
            }
        }
        private string[] ClearRemainder(string[] arr, int lastwrite, int max)
        {
            for(int k = lastwrite; k < max; k++)
            {
                arr[k] = string.Empty;
            }
            return arr;
        }
        public async Task<IEnumerable<Match>> EvalSectionByLineAsync(
            Func<string, bool> sectionMatchStart,
            Func<string, bool> sectionMatchEnd,
            Func<string, Match> matchedResults)
        {
            List<Match> Matches = new();
            using (var reader = (_dataFetch = new()).GetDataReader())
            {
                string? currentLine = string.Empty;
                bool start = false;
                while((currentLine = await reader.ReadLineAsync()) is not null){

                    if (sectionMatchStart(currentLine) && !start)
                        start = true;

                    if (start && !sectionMatchEnd(currentLine))
                    {
                        Matches.Add(matchedResults(currentLine));
                        continue;
                    }
                    return Matches;
                }
            return Matches;
            }
        }

        /// <summary>
        /// SUPPLY THE REGEX OBJECT WITH A MATCH TIMEOUT TO ENSURE YIELDING IN CASE OF CATASTROPHIC BACKTRACKING!!
        /// </summary>
        /// <param name="sectionMatchStart"></param>
        /// <param name="sectionMatchEnd"></param>
        /// <param name="matchedResults"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Match>> EvalWholeSectionAsync(
            Func<string, bool> sectionMatchStart,
            Func<string, bool> sectionMatchEnd,
            Func<IEnumerable<string>, Match> matchedResults)
        {
            int maxAllowedThreads = 11;
            var semaphore = new SemaphoreSlim(maxAllowedThreads);
            List<Task<Match>> matchTasks = new();
            List<CancellationTokenSource> cancelTokens = new();
            try
            {
                await foreach(var line in this.EnumerableStringsAsync(sectionMatchStart, sectionMatchEnd))
                {
                    await semaphore.WaitAsync();
                    var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    var run = Task.Run<Match?>(() =>
                    {
                        try {

                            return matchedResults(line);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });
                    cancelTokens.Add(tokenSource);
                    matchTasks.Add(run);
                }
                return await Task.WhenAll(matchTasks);
            }
            catch(Exception exc)
            {
                Console.WriteLine($"{exc.Message}\n Inner Exception: {(exc.InnerException)?.Message}");
                throw new Exception($"{exc.Message}\n Inner Exception: {(exc.InnerException)?.Message}");
            }
            finally
            {
                foreach(var cts in cancelTokens)
                {
                    cts.Dispose();
                }
            }
        }
    }
}
