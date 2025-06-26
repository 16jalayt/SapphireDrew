using Sapphire_Extract_Helpers;
using System;
using System.Buffers.Binary;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace HIFFCompile
{
    internal static class ParseHIFF
    {
        public static void Parse(bool olderGame, BinaryWriter outStream, string inFile)
        {
            //If newer game write FLAGEVNT block
            if (!olderGame)
            {
                outStream.Write(Encoding.UTF8.GetBytes("DATA"));
                //Chunk length placeholder
                long FdataPlace = outStream.BaseStream.Position;
                outStream.Write((int)-1);

                outStream.Write(Encoding.UTF8.GetBytes("FLAGEVNT"));

                outStream.Write((int)0);

                Utils.WriteLength(outStream, FdataPlace);
            }

            outStream.Write(Encoding.UTF8.GetBytes("DATA"));
            //Chunk length placeholder
            outStream.Write((int)-1);
            long lengthPlace = outStream.BaseStream.Position;

            try
            {
                ParseLoop(outStream);
                //Incomplete parse
                if (InFile.pos < InFile.lines.Length)
                {
                    Console.WriteLine($"\nSyntax error in: '{inFile}' at line '{InFile.pos}'");
                    outStream.BaseStream.Close();
                }
                //Successful parse, update pointers
                else
                {
                    //update data chunk length at beginning of file
                    long endChunk = outStream.BaseStream.Position;
                    outStream.Seek((int)lengthPlace - 4, SeekOrigin.Begin);
                    int length = BinaryPrimitives.ReverseEndianness((int)(endChunk - lengthPlace));
                    outStream.Write(length);
                    outStream.Seek((int)endChunk, SeekOrigin.Begin);

                    Console.WriteLine("Success");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"\nSyntax error in: '{inFile}' at line '{InFile.pos}'\n");
                Console.WriteLine(e);
            }
            //outStream.BaseStream.Close();
            //TODO: find way to delete on fail
        }

        private static void ParseLoop(BinaryWriter outStream)
        {
            for (; InFile.pos < InFile.lines.Length; InFile.GetNextLine())
            {
                if (InFile.GetLine().StartsWith("//") || InFile.GetLine()?.Length == 0)
                {
                    continue;
                }
                switch (InFile.GetCurrentToken())
                {
                    case "CHUNK":
                        switch (InFile.GetNextToken())
                        {
                            case "ACT":
                                Utils.CheckOpenClosure();
                                LongChunks.ACTChunk(outStream);
                                Utils.CheckCloseClosure();
                                break;

                            case "USE":
                                Utils.CheckOpenClosure();
                                LongChunks.USEChunk(outStream);
                                Utils.CheckCloseClosure();
                                break;

                            case "TSUM":
                                Utils.CheckOpenClosure();
                                LongChunks.TSUMChunk(outStream);
                                Utils.CheckCloseClosure();
                                break;

                            default:
                                //Unknown chunk
                                Console.WriteLine($"Unknown chunk type: '{InFile.GetCurrentToken()}' on line {InFile.pos + 1}");
                                return;
                        }
                        break;

                    case "ovl":
                        //parse short
                        ShortChunks.OVLChunk(outStream);
                        break;

                    case "hsflags":
                        //parse short
                        ShortChunks.FLAGChunk(outStream);
                        break;

                    case "use":
                        //parse short
                        ShortChunks.USEChunk(outStream);
                        break;

                    default:
                        //Unknown line
                        Console.WriteLine($"Unknown line contents: '{InFile.GetLine()}' on line {InFile.pos + 1}");
                        return;
                }
                //Padding
                if (outStream.BaseStream.Position % 2 != 0)
                    outStream.Write((byte)0);
            }
        }
    }
}