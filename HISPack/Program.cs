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
            //Console.WriteLine("UNVALIDATED\n");
            Console.WriteLine("CURRENTLY VERY BROKEN\n");

            if (args.Length < 2)
            {
                Console.WriteLine("Usage is HISPack.exe filename gamenum\n");
                return;
            }

            //input validation aka dumb user proofing
            //If invalid, helper will terminate program with message.
            int gamenum = Helpers.ValidateGameNum(args[1]);

            //TODO: add batch folder support
            string FileName = args[0];

            if (!File.Exists(FileName))
            {
                Console.WriteLine($"The file: '{FileName}' does not exist.\n");
                return;
            }

            BinaryWriter OutStream = new BinaryWriter(new FileStream(@Path.GetFileNameWithoutExtension(FileName) + ".his", FileMode.Create));
            using (var vorbis = new NVorbis.VorbisReader(FileName))
            {
                //magic
                OutStream.Write(Encoding.UTF8.GetBytes("HIS\0"));

                //version?
                OutStream.Write((int)2);

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

            //version?
            OutStream.Write((int)2);

            //File content
            byte[] contents = InStream.ReadBytes((int)InStream.Length());
            OutStream.Write(contents);

            OutStream.Close();
        }
    }
}