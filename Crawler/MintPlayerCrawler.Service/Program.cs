using System;
using System.Linq;

namespace MintPlayerCrawler.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Any())
                {
                    switch (args[0])
                    {
                        case "install":
                            break;
                        case "sayhello":
                            Console.WriteLine("Hello World!");
                            break;
                        default:
                            throw new Exception();
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine("  Usage:");
                Console.WriteLine("  MintPlayerCrawler.Service.exe sayhello");
                Console.WriteLine();
                Console.ResetColor();
            }
        }
    }
}
