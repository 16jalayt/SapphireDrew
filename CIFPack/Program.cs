using Sapphire_Extract_Helpers;
using System;
using System.IO;

namespace CIFPack
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //Tool status
            Console.WriteLine($"CURRENTLY BROKEN\n");
            //Console.WriteLine($"EXPEREMENTAL\n");
            //Console.WriteLine($"UNVALIDATED\n");

            if (args.Length < 2)
            {
                Console.WriteLine("Usage is CIFExtract.exe [input filename or folder] gamenumber.");
                return;
            }

            //input validation aka dumb user proofing
            //If invalid, helper will terminate program with message.
            int gamenum = Helpers.ValidateGameNum(args[1]);

            string FileName = args[0];

            if (File.Exists(FileName))
            {
                CIF.packCIFFile(FileName, gamenum);
            }
            else if (Directory.Exists(FileName))
            {
                CIF.packCIFTree(FileName, gamenum);
            }
            else
            {
                Console.WriteLine($"The file or directory: '{FileName}' does not exist.");
                return;
            }

            Console.WriteLine("Done.");
        }
    }
}