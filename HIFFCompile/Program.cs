using Sapphire_Extract_Helpers;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Text;

namespace HIFFCompile
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //Tool status
            Console.WriteLine($"CURRENTLY BROKEN\n");
            //Console.WriteLine($"EXPEREMENTAL\n");
            //Console.WriteLine($"UNVALIDATED\n");

            if (args.Length < 1)
            {
                Console.WriteLine($"Usage is XSheetCompile.exe filename\n");
                return;
            }
            string FileName = args[0];

            bool olderGame = false;
            if (args.Length > 1 && args[1] == "-o")
                olderGame = true;

            //only used to get full path of input
            BetterBinaryReader testFile = new BetterBinaryReader(FileName);

            if (!File.Exists(FileName))
            {
                Console.WriteLine($"The file: '{FileName}' does not exist.\n");
                return;
            }

            FileInfo outFile = new FileInfo(Path.GetDirectoryName(testFile.FilePath) + "/Output/" + Path.GetFileNameWithoutExtension(testFile.FilePath) + ".hiff");
            testFile.Dispose();
            outFile.Directory.Create();
            BinaryWriter outStream = new BinaryWriter(new FileStream(outFile.FullName, FileMode.Create), Encoding.UTF8);

            //If newer game write FLAGEVNT block
            if (!olderGame)
            {
                outStream.Write(Encoding.UTF8.GetBytes("DATA"));
                //Chunk length placeholder
                outStream.Write((int)-1);
                long FdataPlace = outStream.BaseStream.Position;

                outStream.Write(Encoding.UTF8.GetBytes("FLAGEVNT"));

                outStream.Write((int)0);

                long FendChunk = outStream.BaseStream.Position;
                outStream.Seek((int)FdataPlace - 4, SeekOrigin.Begin);
                int Flength = BinaryPrimitives.ReverseEndianness((int)(FendChunk - FdataPlace));
                outStream.Write(Flength);
                outStream.Seek((int)FendChunk, SeekOrigin.Begin);
            }

            outStream.Write(Encoding.UTF8.GetBytes("DATA"));
            //Chunk length placeholder
            outStream.Write((int)-1);
            long dataPlace = outStream.BaseStream.Position;

            //write data header

            string[] lines = File.ReadLines(FileName).ToArray();
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("//") || lines[i] == "")
                    continue;
                else if (lines[i] == "CHUNK TSUM {")
                {
                    outStream.Write(Encoding.UTF8.GetBytes("SCENTSUM"));
                    outStream.Write((int)-1);
                    long scenPlace = outStream.BaseStream.Position;

                    string sceneBack = lines[i + 1];
                    sceneBack = sceneBack.Substring(sceneBack.IndexOf("\"") + 1);
                    sceneBack = sceneBack.Substring(0, sceneBack.LastIndexOf("\""));
                    sceneBack = sceneBack.PadRight(50, '\0');
                    outStream.Write(Encoding.UTF8.GetBytes(sceneBack));

                    long endChunk = outStream.BaseStream.Position;
                    outStream.Seek((int)scenPlace - 4, SeekOrigin.Begin);
                    int length = BinaryPrimitives.ReverseEndianness((int)(endChunk - scenPlace));
                    outStream.Write(length);
                    outStream.Seek((int)endChunk, SeekOrigin.Begin);

                    i += 8;
                }
                else if (lines[i] == "CHUNK USE {")
                {
                    outStream.Write(Encoding.UTF8.GetBytes("USE\0"));
                    outStream.Write((int)-1);
                    long usePlace = outStream.BaseStream.Position;

                    //numDeps placeholder
                    outStream.Write((short)-1);
                    long numDepsPlace = outStream.BaseStream.Position;

                    if (lines[i + 1] != "  BeginCount RefHif")
                        Console.WriteLine($"Unknown use contents: '{lines[i + 1]}'");

                    int j = 0;
                    while (lines[i + 2 + j] != "  EndCount RefHif")
                    {
                        string useRef = lines[i + 2 + j];
                        useRef = useRef.Substring(useRef.IndexOf("\"") + 1);
                        useRef = useRef.Substring(0, useRef.LastIndexOf("\""));
                        useRef = useRef.PadRight(50, '\0');
                        outStream.Write(Encoding.UTF8.GetBytes(useRef));
                    }

                    long endDeps = outStream.BaseStream.Position;
                    outStream.Seek((int)usePlace - 2, SeekOrigin.Begin);
                    int deplength = BinaryPrimitives.ReverseEndianness((int)(endDeps - numDepsPlace));
                    outStream.Write(deplength);
                    outStream.Seek((int)endDeps, SeekOrigin.Begin);

                    long endChunk = outStream.BaseStream.Position;
                    outStream.Seek((int)usePlace - 4, SeekOrigin.Begin);
                    int length = BinaryPrimitives.ReverseEndianness((int)(endChunk - usePlace));
                    outStream.Write(length);
                    outStream.Seek((int)endChunk, SeekOrigin.Begin);

                    i += i + j + 3;
                }
                else
                    Console.WriteLine($"Unknown line contents: '{lines[i]}'");
            }
            outStream.Close();
        }
    }
}