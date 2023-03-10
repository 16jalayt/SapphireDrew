using Sapphire_Extract_Helpers;
using System;
using System.IO;
using System.Text;

namespace HISPack
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //Tool status
            //Console.WriteLine("EXPEREMENTAL\n");
            Console.WriteLine("UNVALIDATED\n");
            //Console.WriteLine("CURRENTLY VERY BROKEN\n");

            if (args.Length < 1)
            {
                Console.WriteLine("Usage is HISPack.exe filename\n");
                return;
            }

            //TODO: add batch folder support
            string FileName = args[0];

            if (!File.Exists(FileName))
            {
                Console.WriteLine($"The file: '{FileName}' does not exist.\n");
                return;
            }

            //InStream created lower because nvorbis needs to open first
            BinaryWriter OutStream = new BinaryWriter(new FileStream(@Path.GetFileNameWithoutExtension(FileName) + ".his", FileMode.Create));

            if (Path.GetExtension(FileName) == ".ogg")
            {
                using (var vorbis = new NVorbis.VorbisReader(FileName))
                {
                    //magic
                    OutStream.Write(Encoding.UTF8.GetBytes("HIS\0"));

                    //version
                    OutStream.Write(2);

                    //Contents of header:
                    //Format should be 1 for pcm
                    OutStream.Write((short)1);
                    //num channels 1 or 2
                    OutStream.Write((short)vorbis.Channels);
                    //Samplerate: ~44100
                    OutStream.Write((int)vorbis.SampleRate);
                    //Calculated data rate: (Sample Rate * BitsPerSample * Channels) / 8
                    OutStream.Write((int)vorbis.SampleRate * vorbis.Channels);
                    //Bits per sample
                    short temp = (short)(vorbis.Channels * 2);
                    OutStream.Write(temp);
                    //Short Bits per sample: Always 16
                    OutStream.Write((short)16);
                }

                BetterBinaryReader InStream = new BetterBinaryReader(FileName);

                //File length
                OutStream.Write((int)InStream.Length());

                //TODO: Later games have short not int
                //version
                OutStream.Write(2);

                //File content
                byte[] contents = InStream.ReadBytes((int)InStream.Length());
                OutStream.Write(contents);

                OutStream.Close();
            }
            else if (Path.GetExtension(FileName) == ".wav")
            {
                BetterBinaryReader InStream = new BetterBinaryReader(FileName);
                if (args.Length > 1 && args[1] == "-v")
                    InStream.debugprint = true;

                if (!Helpers.AssertString(InStream, "RIFF"))
                {
                    Console.WriteLine($"The file: '{FileName}' is not a valid RIFF container.\n");
                    return;
                }

                int fileLength = InStream.ReadInt("file length: ");

                if (!Helpers.AssertString(InStream, "WAVEfmt "))
                {
                    Console.WriteLine($"The file: '{FileName}' is not a valid wave file.\n");
                    return;
                }

                int headerLength = InStream.ReadInt("header length: ");

                short wavFormat = InStream.ReadShort("wavFormat: ");
                short numChannels = InStream.ReadShort("numChannels: ");
                int samplerate = InStream.ReadInt("samplerate: ");
                int avgBytesPerSecond = InStream.ReadInt("avgBytesPerSecond: ");
                short bitsPerSample = InStream.ReadShort("bitsPerSample: ");
                short blockAlign = InStream.ReadShort("blockAlign: ");

                if (!Helpers.AssertString(InStream, "data"))
                {
                    Console.WriteLine($"The file: '{FileName}' has an invalid data chunk.\n");
                    return;
                }

                //magic
                OutStream.Write(Encoding.UTF8.GetBytes("HIS\0"));

                //version?
                OutStream.Write(1);

                OutStream.Write(wavFormat);
                OutStream.Write(numChannels);
                OutStream.Write(samplerate);
                OutStream.Write(avgBytesPerSecond);
                OutStream.Write(bitsPerSample);
                OutStream.Write(blockAlign);

                //version?
                //OutStream.Write(1);

                OutStream.Write(InStream.ReadBytes(fileLength));

                //OutStream.Write(contents);
                OutStream.Close();
            }
            else
            {
                Console.WriteLine($"File: '{FileName}' is not a valid sound file. Must be wav or ogg.\n");
            }
        }
    }
}