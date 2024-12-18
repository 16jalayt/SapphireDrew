﻿using HIFFDecompile.Chunks;
using ManyConsole;
using Sapphire_Extract_Helpers;
using System;
using System.IO;

namespace HIFFDecompile
{
    /*Notes: int is short and long is int. no short. byte is byte
     * MIXED ENDIANNESS
     * Most the leaked sources from wolf are for bonusmsg
     * Chunk Length because need length for c copy funcs.
     * Copy chunks to memory to read?
     * Load chunks out of order?
     *
     * In all but use and tsum, first byte after description
     * is what the engine looks at when determining chunk type
     * NOT description string. Next byte is (usually) trigger type

	 * Either ssum or tsum required and handled outside selection tree
	 * ssum and tsum checked against string
	 * RefFlag can be substituted for int and compile. Adendum: is int in dt_sound. Might be type dependent
	 *
	 * TODO:look into: int long byte can be either quoted or not
     * */

    //QUOT chunk in boot is for easter egg
    //ini set PlayerWonTheGame=StillWorkingOnIt to PlayerWonTheGame=AcedTheGame
    //do shift-ctrl-tab-v for version info, now shows random quote.
    //Not updated since stay tuned?

    internal static class Program
    {
        public class Parameters : ConsoleCommand
        {
            public Parameters()
            {
                IsCommand("Decompile", "Decompiles Nancy Drew HIFF files");

                //HasLongDescription("This can be used to quickly read a file's contents " +
                //"while optionally stripping out the ',' character.");

                //HasOption("f|flags=", "A file containing the flags for this game. Can be Flags.hiff or Flags.htxt", p => FileName = p);
                HasOption("f|flags=", "A file containing the flags for this game. Must be Flags.hiff", p => FlagsFileName = p);

                HasOption("v|verbose:", "Prints debug information to console",
            t => Verbose = t == null ? true : Convert.ToBoolean(t));

                HasOption("l|preferLong:", "Prevents shortening most chunks",
            t => Utils.preferLong = t == null ? true : Convert.ToBoolean(t));

                AllowsAnyAdditionalArguments("File to extract");
                SkipsCommandSummaryBeforeRunning();
            }

            public override int Run(string[] remainingArguments)
            {
                if (remainingArguments.Length != 1)
                {
                    throw new ConsoleHelpAsException("Invalid syntax.");
                }
                if (!new FileInfo(remainingArguments[0]).Exists)
                {
                    throw new ConsoleHelpAsException("Unable to find file: " + remainingArguments[0]);
                }
                FileName = remainingArguments[0];
                return 0;
            }
        }

        private static bool Verbose = false;
        private static string? FileName;
        private static string? FlagsFileName;

        private static void Main(string[] args)
        {
            var commands = ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));

            int returncode = ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
            //Exit if invalid parameters
            if (returncode == -1)
                return;

            //Tool status
            Console.WriteLine("CURRENTLY BROKEN\n");
            //Console.WriteLine("EXPEREMENTAL\n");
            //Console.WriteLine("UNVALIDATED\n");

            if (FileName == "Invalid File" || !File.Exists(FileName))
            {
                Console.WriteLine($"The file: '{FileName}' does not exist.");
                return;
            }
            BetterBinaryReader InStream = new BetterBinaryReader(FileName);

            if (Verbose)
                InStream.debugprint = true;

            if (FlagsFileName != null)
                Utils.PopulateFlags(FlagsFileName, Verbose);

            Console.WriteLine($"Printout of: '{InStream.FilePath}'\n");

            FileInfo file = new FileInfo(Path.GetDirectoryName(InStream.FilePath) + "/Output/" + Path.GetFileNameWithoutExtension(InStream.FilePath) + ".htxt");
            file.Directory?.Create();
            StreamWriter writetext = new StreamWriter(file.FullName);
            if (Verbose)
                writetext.AutoFlush = true;

            DecompChunk(InStream, writetext);
            writetext.Close();
        }

        //Notes:data is top level chunk. Can be multiple chunks
        //BIG ENDIAN chunk length only
        private static void DecompChunk(BetterBinaryReader InStream, StreamWriter writetext)
        {
            string ChunkType = Helpers.String(InStream.ReadBytes(4));
            switch (ChunkType)
            {
                //DATA should be more top level and itter underneath
                case "DATA":
                    //TODO:Length checking
                    int ChunkLength = InStream.ReadIntBE();
                    ChunkType = Helpers.String(InStream.ReadBytes(4));
                    switch (ChunkType)
                    {
                        case "FLAG":
                            //Not known if can be differnet
                            Helpers.AssertString(InStream, "EVNT");

                            //event type
                            int unknown1 = InStream.ReadIntBE();
                            if (unknown1 == 0)
                                break;

                            writetext.WriteLine("CHUNK EVNT {");

                            while (!InStream.IsEOF())
                            {
                                int padding = InStream.ReadByte();
                                if (padding == 0)
                                    break;
                                InStream.Seek(-1, SeekOrigin.Current);

                                string name = Helpers.String(InStream.ReadBytes(33)).TrimEnd('\0');
                                //Why? Wolf is little endian
                                //byte[] data = InStream.ReadBytes(2);
                                //Array.Reverse(data);
                                //Array.Resize(ref data, 4);
                                //int num = BitConverter.ToInt32(data, 0);
                                //Console.WriteLine($"'{name}' - '{num}'");
                                short num = InStream.ReadShort();

                                if (InStream.debugprint)
                                    Console.WriteLine($"'{name}' - '{num}'");

                                //probably auto incremented from 2000 with some sort of tag like RefFlag
                                //writetext.WriteLine($"char[33]    \"{name}\"");
                                //writetext.WriteLine($"int         {num}");
                                if (num < 100)
                                    writetext.WriteLine($"RefINV    \"{name}\"");
                                else
                                    writetext.WriteLine($"RefFlag    \"{name}\"");
                            }

                            writetext.WriteLine("}\n");

                            break;

                        case "SCEN":
                            //Can also be SSUM

                            //scen is chunk and tsum or ssum data in chunk
                            //both should be in same file

                            //Terse Summery
                            Helpers.AssertString(InStream, "TSUM");
                            Scentsum(InStream, writetext);
                            break;

                        case "TEXT":
                            //convo text
                            Helpers.AssertString(InStream, "CVTX");
                            Text(InStream, writetext);
                            break;

                        case "BOOT":
                            //Not known if can be differnet
                            Helpers.AssertString(InStream, "BSUM");
                            Boot_Bsum(InStream, writetext);
                            break;

                        case "FONT":
                            //Not known if can be differnet
                            Helpers.AssertString(InStream, "FONT");
                            Font(InStream, writetext);
                            break;

                        default:
                            Console.WriteLine($"Unknown data chunk type: '{ChunkType}'");
                            break;
                    }
                    break;

                case "ACT\0":
                    ACT.Act(InStream, writetext);
                    break;

                case "USE\0":
                    Use(InStream, writetext);
                    break;

                case "FONT":
                    Font(InStream, writetext);
                    break;

                default:
                    Console.WriteLine($"Unknown chunk type: '{ChunkType}'");
                    Utils.FatalError();
                    break;
            }

            //If not end of file, must be another chunk
            if (InStream.Position() < InStream.Length())
                DecompChunk(InStream, writetext);
        }

        private static void Scentsum(BetterBinaryReader InStream, StreamWriter writetext)
        {
            if (InStream.debugprint) { Console.WriteLine($"---Scentsum {InStream.Position()}---"); }

            int ChunkLength = InStream.ReadIntBE("Chunk Length: ");

            //Scene Description
            //char[50]  "Scene description" in src
            string SceneDesc = Helpers.String(InStream.ReadBytes(50)).TrimEnd('\0');
            if (InStream.debugprint) { Console.WriteLine("SceneDesc: " + SceneDesc); }

            //The name of the scene background.
            //Same length as file name field in cifftree?
            string RefAVF = Helpers.String(InStream.ReadBytes(33)).TrimEnd('\0');
            if (InStream.debugprint) { Console.WriteLine("RefAVF: " + RefAVF); }

            //The name of the scene zone? Seems to be autogened prefix.
            //Same length as file name field in cifftree?
            string RefSound = Helpers.String(InStream.ReadBytes(33)).TrimEnd('\0');
            if (InStream.debugprint) { Console.WriteLine("RefSound: " + RefSound); }

            //Scene specific sound channel
            short sceneChan = InStream.ReadShort("Chan: ");

            //Loop. 0 infinite
            int loop = InStream.ReadInt("loop: ");

            //Unknown. Audio levels?
            short chan1 = InStream.ReadShort("chan1: ");
            short chan2 = InStream.ReadShort("chan2: ");

            //can shorten to bg
            if (chan1 == 85 && chan2 == 85 && loop == 1 && sceneChan == 0 && !Utils.preferLong)
            {
                //TODO: Nitpick: clean up when no sound
                writetext.WriteLine($"bg {RefAVF} {RefSound}\n");
            }
            else
            {
                writetext.WriteLine("CHUNK TSUM {");
                writetext.WriteLine($"char[50]  \"{SceneDesc}\"");
                writetext.WriteLine($"RefAVF    \"{RefAVF}\"");
                writetext.WriteLine($"RefSound  \"{RefSound}\"");
                writetext.WriteLine($"int     {Enums.soundChannel[sceneChan]}");
                writetext.WriteLine($"long    {Enums.loop[loop]}");
                writetext.WriteLine($"int     {chan1}");
                writetext.WriteLine($"int      {chan2}");
                writetext.WriteLine("}\n");
            }

            if (InStream.debugprint) { Console.WriteLine("---END Scentsum---\n"); }
        }

        private static void Boot_Bsum(BetterBinaryReader InStream, StreamWriter writetext)
        {
            if (InStream.debugprint) { Console.WriteLine($"---BSUM {InStream.Position()}---"); }

            int ChunkLength = InStream.ReadIntBE("Chunk Length: ");

            string Bk = Helpers.String(InStream.ReadBytes(33)).TrimEnd('\0');
            if (InStream.debugprint) { Console.WriteLine("BK: " + Bk); }

            //ignores chunk?
            //pcal is cals to preload
            //quot is dev comments for some reason

            if (InStream.debugprint) { Console.WriteLine("---END BSUM---\n"); }
        }

        private static void Text(BetterBinaryReader InStream, StreamWriter writetext)
        {
            if (InStream.debugprint) { Console.WriteLine($"---Text {InStream.Position()}---"); }

            int ChunkLength = InStream.ReadIntBE("Chunk Length: ");

            //num entries?
            short count = InStream.ReadShort("Number: ");

            for (int i = 0; i < count; i++)
            {
                //The name of the convo as reffed by code.
                //Same length as file name field in cifftree?
                string category = Helpers.String(InStream.ReadBytes(33)).TrimEnd('\0');
                if (InStream.debugprint) { Console.WriteLine("Convo name: " + category); }

                //Length of text
                short len = InStream.ReadShort("Length: ");

                string text = Helpers.String(InStream.ReadBytes(len)).TrimEnd('\0');
                if (InStream.debugprint) { Console.WriteLine(text + "\n"); }
            }

            if (InStream.debugprint) { Console.WriteLine("---END Text---\n"); }
        }

        private static void Font(BetterBinaryReader InStream, StreamWriter writetext)
        {
            if (InStream.debugprint) { Console.WriteLine($"---Font {InStream.Position()}---"); }

            int ChunkLength = InStream.ReadIntBE("Chunk Length: ");

            int unk1 = InStream.ReadIntBE("Unknown 1: ");

            //The name of the font as reffed by code.
            //Same length as file name field in cifftree?
            string category = Helpers.String(InStream.ReadBytes(33)).TrimEnd('\0');
            if (InStream.debugprint) { Console.WriteLine("Font name: " + category); }

            //The name of the font from system.
            //Same length as file name field in cifftree?
            string font = Helpers.String(InStream.ReadBytes(33)).TrimEnd('\0');
            if (InStream.debugprint) { Console.WriteLine("Font: " + font); }

            //font size?
            //LITTLE ENDIAN
            int size = InStream.ReadInt("Size: ");

            //LITTLE ENDIAN
            int unk2 = InStream.ReadInt("Unknown 2: ");

            if (InStream.debugprint) { Console.WriteLine("---END Font---\n"); }
        }

        //Use seems to be an import/func call
        //Note: Compiler removes unused uses
        private static void Use(BetterBinaryReader InStream, StreamWriter writetext)
        {
            if (InStream.debugprint) { Console.WriteLine($"--USE {InStream.Position()}---"); }

            int ChunkLength = InStream.ReadIntBE("Chunk Length: ");

            //num refs
            short NumRefs = InStream.ReadShort("Num refs: ");
            string[] refs = new string[NumRefs];

            for (int i = 0; i < NumRefs; i++)
            {
                string name = Helpers.String(InStream.ReadBytes(33)).TrimEnd('\0');
                if (InStream.debugprint) { Console.WriteLine("   -" + name); }
                refs[i] = name;
            }

            //might be byte aligned
            if (!InStream.IsEOF() && InStream.ReadByte() != 0)
                InStream.Skip(-1);

            //can shorten to use
            if (!Utils.preferLong)
            {
                writetext.Write("use");
                for (int i = 0; i < NumRefs; i++)
                {
                    writetext.Write($" {refs[i]}");
                }
                writetext.Write("\n");
            }
            else
            {
                writetext.WriteLine("CHUNK USE {");
                writetext.WriteLine("  BeginCount RefHif");
                for (int i = 0; i < NumRefs; i++)
                {
                    writetext.WriteLine($"    RefHif    \"{refs[i]}\"     // Hif file to include (without the \".hif\")");
                }

                writetext.WriteLine("  EndCount RefHif");
                writetext.WriteLine("}\n");
            }

            if (InStream.debugprint) { Console.WriteLine("---END USE---\n"); }
        }
    }
}