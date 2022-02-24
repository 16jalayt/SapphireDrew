using Sapphire_Extract_Helpers;
using System;
using System.IO;

namespace HIFFDecompile
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine($"Usage is HIFFDecompile.exe filename");
                return;
            }

            string FileName = args[0];

            if (!File.Exists(FileName))
            {
                Console.WriteLine($"The file: '{FileName}' does not exist.");
                return;
            }
            BetterBinaryReader InStream = new BetterBinaryReader(FileName);

            DecompChunk(InStream);
        }

        static void DecompChunk(BetterBinaryReader InStream)
        {
            if (!Helpers.AssertString(InStream, "DATA"))
            {
                Console.WriteLine($"The file: '{InStream.FileName}' has an invalid header.\n");
                return;
            }

            //TODO:Length checking
            int ChunkLength = InStream.ReadInt();

            //Name length variable?
            //string ChunkType = Helpers.String(InStream.ReadBytes(nameLength));
            //switch ()
        }
    }
}
