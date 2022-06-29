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

            CIF3.Extract(InStream);
            /*if (verMajor == 3 && verMinor == 0)
                CIF3.Extract(InStream);
            else if (verMajor == 2 && verMinor == 0)
                CIF2_0.Extract(InStream);
            //else if (verMajor == 2 && verMinor == 1)
            //CIF2_1.Extract(InStream);
            else
                Console.WriteLine($"CIFF version not recognised.");*/
        }
    }
}