using Sapphire_Extract_Helpers;
using System;
using System.IO;
using System.Text;

namespace HISExtract
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //Tool status
            //Console.WriteLine("EXPEREMENTAL\n");
            Console.WriteLine("UNVALIDATED\n");
            //Console.WriteLine("CURRENTLY BROKEN\n");

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

            if (args.Length > 1 && args[1] == "-v")
                InStream.debugprint = true;

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

            //get version to tell if wav or ogg
            InStream.Seek(28);
            int version = InStream.ReadShort("version?: ");

            //wav. Need to create header.
            if (version == 1)
            {
                InStream.Seek(8);
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
                //version
                InStream.ReadShort();

                //Calculated values
                samplerate = samplerate / numChannels;
                //defined in original file
                //short blockAlign = (short) (bitsPerSample / 8 * numChannels);
                //int avgbytes   = samplerate * blockAlign;

                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter OutStream = new BinaryWriter(stream))
                    {
                        OutStream.Write(Encoding.UTF8.GetBytes("RIFF"));
                        //length of wav data + full header
                        OutStream.Write(fileLength + 32);
                        OutStream.Write(Encoding.UTF8.GetBytes("WAVEfmt "));
                        //length of header
                        OutStream.Write(16);

                        OutStream.Write(wavFormat);
                        OutStream.Write(numChannels);
                        OutStream.Write(samplerate);
                        OutStream.Write(avgBytesPerSecond);
                        OutStream.Write(bitsPerSample);
                        OutStream.Write(blockAlign);

                        OutStream.Write(Encoding.UTF8.GetBytes("data"));
                        OutStream.Write(fileLength);

                        OutStream.Write(InStream.ReadBytes(fileLength));
                    }
                    Helpers.Write(InStream.FilePath, InStream.FileNameWithoutExtension + ".wav", stream.ToArray(), false);
                }

                return;
            }
            //ogg. Just need to trim header off.
            if (version == 2)
            {
                //looks like length of this var changed between games.
                //figure out length
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

                int RemainingLength = (int)(InStream.Length() - InStream.Position());

                byte[] FileContents = InStream.ReadBytes(RemainingLength);
                Helpers.Write(InStream.FilePath, InStream.FileNameWithoutExtension + ".ogg", FileContents, false);
            }
        }
    }
}