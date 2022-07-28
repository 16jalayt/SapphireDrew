using Sapphire_Extract_Helpers;
using System;
using System.IO;

namespace CIFExtract
{
    internal class Program
    {
        //TODO: cal files
        private static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine($"Usage is CIFExtract.exe filename");
                return;
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