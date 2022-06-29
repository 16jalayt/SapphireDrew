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

            //unconfirmed: version, seems to be 1 in older games?
            if (!Helpers.AssertInt(InStream, 2))
                Console.WriteLine($"The unknown value of 2 at the begining is different\n");

            //format should be 1 for pcm
            short wavFormat = InStream.ReadShort();
            short numChannels = InStream.ReadShort();

            //skip header info, as we just need to trim header off for ogg
            //Contents of header:
            //Samplerate: ~44100
            //Calculated data rate: (Sample Rate * BitsPerSample * Channels) / 8
            //Short: Either 2 or 4: (BitsPerSample * Channels)
            //Short Bits per sample: Always 16
            //Int: Changes between files
            //Short or int 

            InStream.Skip(16);
            if (VerMajor == 1 && VerMinor == 1)
                InStream.Skip(2);
            else if (VerMajor == 1 && VerMinor == 2)
                InStream.Skip(4);
            else
            {
                Console.WriteLine($"Unknown HIS version: {VerMajor}.{VerMinor}\n");
                return;
            }

            int RemainingLength = (int)(InStream.Length() - InStream.Position());

            byte[] FileContents = InStream.ReadBytes(RemainingLength);
            Helpers.Write(InStream.FilePath, InStream.FileNameWithoutExtension + ".ogg", FileContents, false);
        }
    }
}
