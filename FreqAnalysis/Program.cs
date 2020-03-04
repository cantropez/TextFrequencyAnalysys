using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FreqAnalysis
{
    class Program
    {

        #region Vars
        static volatile bool exit = false;

        static Dictionary<string, int> tripletz = new Dictionary<string, int>();

        static Stopwatch stopwatch = new Stopwatch();
        #endregion

        #region FindTriplets
        private static Task<Dictionary<string, int>> FindTripletZ(string str, CancellationToken token)
        {
            string strClear = Regex.Replace(str, @"[^A-Za-zА-Яа-я]+", " ");
            //
            return Task.Run(() =>
            {
                for (int i = 0; i < strClear.Length - 3; i++)
                {
                    if (strClear[i] == ' ' || strClear[i + 1] == ' ' || strClear[i + 2] == ' ')
                        continue;

                    string triplet = strClear[i].ToString() + strClear[i + 1].ToString() + strClear[i + 2].ToString();

                    Regex regex = new Regex(@"(\w*)" + triplet + @"(\w*)");

                    if (!tripletz.ContainsKey(triplet))
                        tripletz.Add(triplet, regex.Matches(str).Count);

                }
                return tripletz;
            });
            
        }

        public static async Task AsyncFindTripletZ(string str)
        {
            stopwatch.Start();
            //
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var keyBoardTask = Task.Run(() =>
                {
                    Console.WriteLine("Press enter to cancel");
                    Console.ReadKey(true);

                    // Cancel the task
                    cancellationTokenSource.Cancel();

                    if (!exit)
                    {
                        var sortedDict = (from entery in tripletz orderby entery.Value descending select entery).Take(10);
                        foreach (var itemz in sortedDict)
                            Console.WriteLine("Triplet: " + itemz.Key + " - " + "count: " + itemz.Value);
                        //
                        stopwatch.Stop();
                        Console.Write("Program runtime - " + stopwatch.ElapsedMilliseconds.ToString());
                        exit = true;
                    }
                });

                try
                {
                    var longRunningTask = FindTripletZ(str, cancellationTokenSource.Token);

                    var result = await longRunningTask;
                    //
                    stopwatch.Stop();
                    
                    //
                    if (!exit)
                    {
                        var sortedDict = (from entery in tripletz orderby entery.Value descending select entery).Take(10);
                        foreach (var itemz in sortedDict)
                            Console.WriteLine("Triplet: " + itemz.Key + " - " + "count: " + itemz.Value);
                        //
                        Console.WriteLine("Program runtime - " + stopwatch.ElapsedMilliseconds.ToString());
                        exit = true;
                    }

                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Task was cancelled");
                }

                await keyBoardTask;
            }
        }
        #endregion

        #region Main
        static void Main(string[] args)
        {

            Console.WriteLine("Enter the path: ");
            try
            {
                var path = Console.ReadLine();

                if (File.Exists(path))
                    _ = AsyncFindTripletZ(new StreamReader(path).ReadToEnd());
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:\n" + e.Message);
            }
            //
            Console.ReadKey();
        }
        #endregion
    }
}
