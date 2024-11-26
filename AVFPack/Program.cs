using Sapphire_Extract_Helpers;
using System;
using System.IO;

namespace AVFPack
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            //Tool status
            //Console.WriteLine("EXPEREMENTAL\n");
            //Console.WriteLine("UNVALIDATED\n");
            Console.WriteLine("CURRENTLY BROKEN\n");

            if (args.Length < 1)
            {
                Console.WriteLine("Usage is AVFPack.exe filename\n");
                return;
            }

            //TODO: add batch folder support
            string FileName = args[0];

            if (!File.Exists(FileName))
            {
                Console.WriteLine($"The file: '{FileName}' does not exist.\n");
                return;
            }
            BetterBinaryReader InStream = new BetterBinaryReader(FileName);
        }
    }
}