using Sapphire_Extract_Helpers;
using System;
using System.IO;

namespace HISExtract
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //Tool status
            //Console.WriteLine("EXPEREMENTAL\n");
            //Console.WriteLine("UNVALIDATED\n");
            Console.WriteLine("CURRENTLY BROKEN\n");

            if (args.Length < 1)
            {
                Console.WriteLine("Usage is HISExtract.exe filename\n");
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

            //If the file has wrong id, say we can't extract
            //TODO: SCK is "Her Interactive Sound"
            if (!Helpers.AssertString(InStream, "HIS\0"))
            {
                Console.WriteLine($"The file: '{FileName}' has an invalid header.\n");
                return;
            }

            //unconfirmed: version, seems to be 1 in older games?
            if (!Helpers.AssertInt(InStream, 2))
                Console.WriteLine("The unknown value of 2 at the begining is different\n");

            //skip header info, as we just need to trim header off for ogg
            //Contents of header:
            //format should be 1 for pcm
            //num channels 1 or 2
            //Samplerate: ~44100
            //Calculated data rate: (Sample Rate * BitsPerSample * Channels) / 8
            //Short: Either 2 or 4: (BitsPerSample * Channels)
            //Short Bits per sample: Always 16
            //Int: Changes between files
            //Short or int
            short wavFormat = InStream.ReadShort("wavFormat: ");
            short numChannels = InStream.ReadShort("numChannels: ");
            int samplerate = InStream.ReadInt("samplerate: ");
            int avgBytesPerSecond = InStream.ReadInt("avgBytesPerSecond: ");
            short bitsPerSample = InStream.ReadShort("bitsPerSample: ");
            short blockAlign = InStream.ReadShort("blockAlign: ");
            //Only seems to be valid with wav data
            int fileLength = InStream.ReadInt("fileLength: ");
            //version?
            byte version = InStream.ReadByte("version?: ");

            //looks like length of this var changed between games.
            //wav?
            if (version == 1)
            {
                Console.WriteLine("WAV based HIS files not yet supported.");
                return;
            }
            //ogg?
            if (version == 2)
            {
                //figure out length
                InStream.Skip(1);
                //if 0 then 4 bytes
                if (InStream.ReadByte() == 0)
                {
                    InStream.Skip(1);
                }
                //otherwise 2 bytes
                else
                {
                    //go back to bit just read
                    InStream.Skip(-1);
                }
            }

            int RemainingLength = (int)(InStream.Length() - InStream.Position());

            byte[] FileContents = InStream.ReadBytes(RemainingLength);
            Helpers.Write(InStream.FilePath, InStream.FileNameWithoutExtension + ".ogg", FileContents, false);
        }
    }
}