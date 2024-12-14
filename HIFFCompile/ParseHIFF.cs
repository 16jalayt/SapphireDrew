using Sapphire_Extract_Helpers;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace HIFFCompile
{
    internal static class ParseHIFF
    {
        public static void Parse(bool olderGame, ref BinaryWriter outStream, string inFile)
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

                Utils.WriteLength(ref outStream, FdataPlace);
            }

            outStream.Write(Encoding.UTF8.GetBytes("DATA"));
            //Chunk length placeholder
            outStream.Write((int)-1);
            long lengthPlace = outStream.BaseStream.Position;

            for (; InFile.pos < InFile.lines.Length; InFile.pos++)
            {
                if (InFile.GetLine().StartsWith("//") || InFile.GetLine()?.Length == 0)
                {
                    continue;
                }
                //TODO: change to generic switch token insted of exact match
                else if (InFile.GetLine() == "CHUNK ACT {")
                {
                    outStream.Write(Encoding.UTF8.GetBytes("ACT\0"));
                    long actPlace = outStream.BaseStream.Position;
                    outStream.Write((int)-1);

                    string? sceneDescription = InFile.WriteNextString(ref outStream, 48);
                    //chunk description
                    if (sceneDescription == null)
                        break;

                    int actType = -1;
                    //Act Chunk Type
                    if (!InFile.GetNextObject<byte>(ref outStream, "byte", out actType, dictType: Enums.ACT_Type))
                    {
                        Console.WriteLine($"Unknown action record type with desc \"{sceneDescription}\" at {InFile.pos + 1}.");
                        break;
                    }

                    //Exec type
                    if (!InFile.GetNextObject<byte>(ref outStream, "byte", enumType: Enums.execType))
                        break;

                    int posEndDeps = Utils.ParseDeps(ref outStream, actType);
                    //Error parsing
                    if (posEndDeps == -1)
                        break;

                    if (!Act.ParseAct(ref outStream, actType))
                        break;

                    int actLength = Utils.WriteLength(ref outStream, actPlace);

                    //Pad to even chunk length
                    //Next chunk should start even
                    if (outStream.BaseStream.Position % 2 != 0)
                        outStream.Write((byte)0);

                    InFile.pos = posEndDeps;

                    if (InFile.GetNextLine() != "}")
                    {
                        Console.WriteLine($"ACT chunk not closed on line {InFile.pos + 1}.");
                    }
                }
                else if (InFile.GetLine() == "CHUNK TSUM {")
                {
                    outStream.Write(Encoding.UTF8.GetBytes("SCENTSUM"));
                    long scenPlace = outStream.BaseStream.Position;
                    outStream.Write((int)-1);

                    //Scene description
                    if (InFile.WriteNextString(ref outStream, 50) == null)
                        break;
                    //Background file without extension
                    if (InFile.WriteNextString(ref outStream, 33) == null)
                        break;
                    //Background sound
                    if (InFile.WriteNextString(ref outStream, 33) == null)
                        break;

                    //Channel of backgound sound
                    if (!InFile.GetNextObject<short>(ref outStream, "int", enumType: Enums.soundChannel))
                        break;
                    //Loop background sound?
                    if (!InFile.GetNextObject<int>(ref outStream, "long", enumType: Enums.loop))
                        break;
                    //Left channel volume for background sound
                    if (!InFile.GetNextObject<short>(ref outStream, "int", enumType: null))
                        break;
                    //Right channel volume for background sound
                    if (!InFile.GetNextObject<short>(ref outStream, "int", enumType: null))
                        break;

                    Utils.WriteLength(ref outStream, scenPlace);

                    if (InFile.GetNextLine() != "}")
                    {
                        Console.WriteLine($"TSUM chunk not closed on line {InFile.pos + 1}.");
                    }
                }
                else if (InFile.GetLine() == "CHUNK USE {")
                {
                    outStream.Write(Encoding.UTF8.GetBytes("USE\0"));
                    long usePlace = outStream.BaseStream.Position;
                    outStream.Write((int)-1);

                    //numDeps placeholder
                    long numDepsPlace = outStream.BaseStream.Position;
                    outStream.Write((short)-1);

                    if (InFile.GetNextLine() != "BeginCount RefHif")
                    {
                        Console.WriteLine($"Unknown use contents: '{InFile.GetLine()}'");
                        break;
                    }

                    short numDeps = 0;
                    while (InFile.GetNextLine() != "EndCount RefHif")
                    {
                        InFile.WriteString(ref outStream, 33);
                        numDeps++;
                    }

                    long endDeps = outStream.BaseStream.Position;
                    outStream.Seek((int)numDepsPlace, SeekOrigin.Begin);
                    outStream.Write(numDeps);
                    outStream.Seek((int)endDeps, SeekOrigin.Begin);

                    Utils.WriteLength(ref outStream, usePlace);

                    if (InFile.GetNextLine() != "}")
                    {
                        Console.WriteLine($"TSUM chunk not closed on line {InFile.pos + 1}.");
                    }
                }
                else
                {
                    Console.WriteLine($"Unknown line contents: '{InFile.GetLine()}' on line {InFile.pos + 1}");
                    break;
                }
            }

            if (InFile.pos < InFile.lines.Length)
            {
                Console.WriteLine($"\nSyntax error in: '{inFile}'");
            }
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
    }
}