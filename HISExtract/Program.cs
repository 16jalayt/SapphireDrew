using Sapphire_Extract_Helpers;
using System;
using System.IO;

namespace HISExtract
{
    class Program
    {
        static void Main(string[] args)
        {
            //Tool status
            //Console.WriteLine($"EXPEREMENTAL\n");
            //Console.WriteLine($"UNVALIDATED\n");
            Console.WriteLine($"CURRENTLY BROKEN\n");

            if (args.Length < 1)
            {
                Console.WriteLine($"Usage is HISExtract.exe filename\n");
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
            if (!Helpers.AssertString(InStream, "HIS\0"))
            {
                Console.WriteLine($"The file: '{FileName}' has an invalid header.\n");
                return;
            }

            if (!Helpers.AssertInt(InStream, 2))
                Console.WriteLine($"The unknown value of 2 at the begining is different\n");

            short VerMajor = InStream.ReadShort();
            Console.WriteLine($"Major Version: {VerMajor}");

            short VerMinor = InStream.ReadShort();
            Console.WriteLine($"Minor Version: {VerMinor}");

            //skip header info, as we just need to trim header off
            //Contents of header:
            //Samplerate: ~44100
            //Unknown int: Same in same game
            //Short: Either 2 or 4
            //Short: Always 16?
            //Int: Changes between files
            //Short in 1.1, int in 1.2

            InStream.Skip(16);
            if(VerMajor == 1 && VerMinor == 1)
                InStream.Skip(2);
            else if (VerMajor == 1 && VerMinor == 2)
                InStream.Skip(4);
            else
            {
                Console.WriteLine($"Unknown HIS version: {VerMajor}.{VerMinor}\n");
                return;
            }

            int RemainingLength = (int) (InStream.Length() - InStream.Position());

            byte[] FileContents = InStream.ReadBytes(RemainingLength);
            Helpers.Write(InStream.FilePath, InStream.FileNameWithoutExtension + ".ogg", FileContents, false);
        }
    }
}
