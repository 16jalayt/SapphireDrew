using Sapphire_Extract_Helpers;
using System;
using System.IO;

namespace CIFExtract
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine($"Usage is XSheetDecompile.exe filename\n");
                return;
            }

            string FileName = args[0];

            if (!File.Exists(FileName))
            {
                Console.WriteLine($"The file: '{FileName}' does not exist.\n");
                return;
            }
            BetterBinaryReader InStream = new BetterBinaryReader(FileName);

            //If the file has wrong id, say we can't extract
            if (!Helpers.AssertHeader(InStream, "CIF FILE HerInteractive") || !Helpers.AssertHeader(InStream, "CIF TREE WayneSikes")|| !Helpers.AssertHeader(InStream, "CIF TREE WayneSikes"))
            {
                Console.WriteLine($"The file: '{FileName}' has an invalid header.\n");
                return;
            }

            //0 padding
            InStream.Skip(9);
        }
    }
}
