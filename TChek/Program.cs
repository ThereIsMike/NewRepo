using System;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive;

namespace TChek
{
    internal class Program
    {
        private static IDisposable Cancel;
        private static int CountNewItems = 0;

        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            while (Console.ReadKey().Key != ConsoleKey.Escape)
            {
                if (Console.ReadKey().Key != ConsoleKey.T)
                {
                    CountNewItems++;
                    if (Cancel != null)
                    {
                        Cancel.Dispose();
                        Cancel = null;
                    }
                    Cancel = Observable
                     .Timer(TimeSpan.FromSeconds(10))
                     .Subscribe(
                     x =>
                     {
                         Console.WriteLine("Added" + CountNewItems);
                         CountNewItems = 0;
                     });
                }
            }
        }
    }
}