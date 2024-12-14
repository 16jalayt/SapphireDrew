using Sapphire_Extract_Helpers;
using System;
using System.IO;

namespace CIFExtract
{
    internal static class Program
    {
        //TODO: cal files
        private static void Main(string[] args)
        {
            //Tool status
            //Console.WriteLine("CURRENTLY BROKEN\n");
            //Console.WriteLine("EXPEREMENTAL\n");
            Console.WriteLine("UNVALIDATED\n");

            if (args.Length < 1)
            {
                Console.WriteLine("Usage is CIFExtract.exe filename.");
                return;
            }

            //TODO: -h help

            if (args.Length == 2 && args[1] == "-r")
            {
                CIF.dontdec = true;
            }
            if (args.Length == 2 && args[1] == "-c")
            {
                CIF.keepcif = true;
            }

            string FileName = args[0];

            if (!File.Exists(FileName))
            {
                Console.WriteLine($"The file: '{FileName}' does not exist.");
                return;
            }
            BetterBinaryReader InStream = new BetterBinaryReader(FileName);

            CIF.Extract(InStream);
        }
    }
}