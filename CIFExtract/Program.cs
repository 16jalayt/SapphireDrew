using Sapphire_Extract_Helpers;
using System;
using System.IO;

namespace CIFExtract
{
    class Program
    {
        //TODO: cal files
        static void Main(string[] args)
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

            string header = Helpers.String(InStream.ReadBytes(24)).Trim('\0');
            //Console.WriteLine($"header: {header}");

            //If the file has wrong id, say we can't extract
            //CIF files and trees are same structure, just one or many files.
            if (header != "CIF FILE HerInteractive" && header != "CIF TREE HerInteractive" && header != "CIF TREE WayneSikes" && header != "CIF FILE WayneSikes")
            {
                Console.WriteLine($"The file: '{FileName}' has an invalid header.");
                return;
            }

            short verMajor = InStream.ReadShort();
            short verMinor = InStream.ReadShort();
            Console.WriteLine($"CIFF version: {verMajor}.{verMinor}\n");

            if (verMajor == 3 && verMinor == 0)
                CIF3.Extract(InStream);
            else if (verMajor == 2 && verMinor == 0)
                CIF2_0.Extract(InStream);
            //else if (verMajor == 2 && verMinor == 1)
                //CIF2_1.Extract(InStream);
            else
                Console.WriteLine($"CIFF version not recognised.");
        }
    }
}
