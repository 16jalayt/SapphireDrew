using HIFFDecompile.Chunks;
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
     * NOT description string. Nex byte is (usually) trigger type
     * */

    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage is HIFFDecompile.exe filename");
                return;
            }

            //Tool status
            Console.WriteLine("CURRENTLY BROKEN\n");
            //Console.WriteLine("EXPEREMENTAL\n");
            //Console.WriteLine("UNVALIDATED\n");

            string FileName = args[0];

            if (!File.Exists(FileName))
            {
                Console.WriteLine($"The file: '{FileName}' does not exist.");
                return;
            }
            BetterBinaryReader InStream = new BetterBinaryReader(FileName);

            if (args.Length > 1 && args[1] == "-v")
                InStream.debugprint = true;

            Console.WriteLine($"Printout of: '{FileName}'\n");

            DecompChunk(InStream);
        }

        //Notes:data is top level chunk. Can be multiple chunks
        //BIG ENDIAN!!!
        private static void DecompChunk(BetterBinaryReader InStream)
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
                            while (!InStream.IsEOF())
                            {
                                string name = Helpers.String(InStream.ReadBytes(33)).TrimEnd('\0');
                                byte[] data = InStream.ReadBytes(2);
                                Array.Reverse(data);
                                Array.Resize(ref data, 4);
                                int num = BitConverter.ToInt32(data, 0);
                                Console.WriteLine($"'{name}' - '{num}'");
                            }
                            break;

                        case "SCEN":
                            //Terse Summeray
                            Helpers.AssertString(InStream, "TSUM");
                            Scentsum(InStream);
                            break;

                        case "TEXT":
                            //convo text
                            Helpers.AssertString(InStream, "CVTX");
                            Text(InStream);
                            break;

                        case "BOOT":
                            //Not known if can be differnet
                            Helpers.AssertString(InStream, "BSUM");
                            Boot_Bsum(InStream);
                            break;

                        case "FONT":
                            //Not known if can be differnet
                            Helpers.AssertString(InStream, "FONT");
                            Font(InStream);
                            break;

                        default:
                            Console.WriteLine($"Unknown data chunk type: '{ChunkType}'");
                            break;
                    }
                    break;

                case "ACT\0":
                    ACT.Act(InStream);
                    break;

                case "USE\0":
                    Use(InStream);
                    break;

                case "FONT":
                    Font(InStream);
                    break;

                default:
                    Console.WriteLine($"Unknown chunk type: '{ChunkType}'");
                    Utils.FatalError();
                    break;
            }

            //If not end of file, must be another chunk
            if (InStream.Position() < InStream.Length())
                DecompChunk(InStream);
        }

        private static void Scentsum(BetterBinaryReader InStream)
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

            if (InStream.debugprint) { Console.WriteLine("---END Scentsum---\n"); }
        }

        private static void Boot_Bsum(BetterBinaryReader InStream)
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

        private static void Text(BetterBinaryReader InStream)
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

        private static void Font(BetterBinaryReader InStream)
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
        private static void Use(BetterBinaryReader InStream)
        {
            if (InStream.debugprint) { Console.WriteLine($"--USE {InStream.Position()}---"); }

            int ChunkLength = InStream.ReadIntBE("Chunk Length: ");

            //num refs
            short NumRefs = InStream.ReadShort("Num refs: ");

            for (int i = 0; i < NumRefs; i++)
            {
                //Wierd length?
                string name = Helpers.String(InStream.ReadBytes(34)).TrimEnd('\0');
                if (InStream.debugprint) { Console.WriteLine("   -" + name); }
            }

            if (InStream.debugprint) { Console.WriteLine("---END USE---\n"); }
        }
    }
}