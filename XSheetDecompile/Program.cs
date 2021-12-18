using Sapphire_Extract_Helpers;
using System;
using System.IO;

namespace XSheetDecompile
{
    internal class Program
    {
        private static void Main(string[] args)
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
            if (!Helpers.AssertString(InStream, "XSHEET HerInteractive"))
            {
                Console.WriteLine($"The file: '{FileName}' has an invalid header.\n");
                return;
            }

            //0 padding
            InStream.Skip(9);

            if (!Helpers.AssertInt(InStream, 2)) ;
                Console.WriteLine($"The unknown value of 2 at the begining is different\n");

            short NumFrames = InStream.ReadShort();
            Console.WriteLine($"Number of frames: {NumFrames}");

            if (!Helpers.AssertShort(InStream, 2)) ;
            Console.WriteLine($"The unknown value of 2 after numframes is different\n");

            string BodyName = Helpers.String(InStream.ReadBytes(33)).Trim('\0').ToLower();
            Console.WriteLine($"BodyName: {BodyName}");

            string HeadName = Helpers.String(InStream.ReadBytes(99)).Trim('\0').ToLower();
            Console.WriteLine($"HeadName: {HeadName}");

            int Bodyx1 = InStream.ReadInt();
            int Bodyy1 = InStream.ReadInt();
            int Bodyx2 = InStream.ReadInt();
            int Bodyy2 = InStream.ReadInt();

            int Headx1 = InStream.ReadInt();
            int Heady1 = InStream.ReadInt();
            int Headx2 = InStream.ReadInt();
            int Heady2 = InStream.ReadInt();

            //null padding
            InStream.Skip(32);

            if (!Helpers.AssertInt(InStream, 66)) ;
                Console.WriteLine($"The unknown value of 66 before the frames is different\n");

            int[] framestorage = new int[NumFrames];
            for (int i=0; i<NumFrames; i++)
            {
                int framenum = InStream.ReadInt();
                framestorage[i] = framenum;
                InStream.Skip(20);
            }

            InStream.Dispose();

            FileInfo file = new FileInfo("Output/" + FileName);
            file.Directory.Create();
            using (StreamWriter writetext = new StreamWriter(file.FullName))
            {
                writetext.WriteLine("XS1\n[Options]\nFPS=15");
                writetext.WriteLine($"CalLevel 1={BodyName}");
                writetext.WriteLine($"VidRect 1= {Bodyx1}, {Bodyy1}, {Bodyx2}, {Bodyy2}");
                writetext.WriteLine($"VidLevel 2={HeadName}");
                writetext.WriteLine($"VidRect 2= {Headx1}, {Heady1}, {Headx2}, {Heady2}");
                writetext.WriteLine("[Level 1]");

                for (int i = 0; i < NumFrames; i++)
                {
                    writetext.WriteLine($"FRAME {i+1}={framestorage[i]}");
                }
            }
        }
    }
}