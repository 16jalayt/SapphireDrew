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

            //TODO: replace with tryparseint?
            int gamenum = Int32.Parse(args[1]);

            if (gamenum == 33)
            {
                Console.WriteLine("Midnight in Salem uses Unity. This is not supported.");
                return;
            }

            if (gamenum < 0 || gamenum > 32)
            {
                Console.WriteLine("Invalid game number. Please enter a number between 0 and 32.");
                return;
            }

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