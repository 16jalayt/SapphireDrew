using Sapphire_Extract_Helpers;
using System;
using System.IO;

namespace HIFFDecompile
{
    /*Notes: int might be short and long as int.
     * All the leaked sources are for bonusmsg
     * Chunk Length because need length for c copy funcs.
     * Copy chunks to memory to read?
     * Load chunks out of order?
     * */

    internal class Program
    {
        public static void FatalError()
        {
            Console.WriteLine("\nFatal Error. Exiting...");
            System.Environment.Exit(-1);
        }

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

            if (args[1] == "-v")
                InStream.debugprint = true;

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
                    Act(InStream);
                    break;

                case "USE\0":
                    Use(InStream);
                    break;

                case "FONT":
                    Font(InStream);
                    break;

                default:
                    Console.WriteLine($"Unknown chunk type: '{ChunkType}'");
                    FatalError();
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

            //Probably some other scene parameters. Not tested yet.
            //name?
            InStream.Skip(50);

            //The name of the scene background.
            //Same length as file name field in cifftree?
            string Bk = Helpers.String(InStream.ReadBytes(33)).TrimEnd('\0');
            if (InStream.debugprint) { Console.WriteLine("BK: " + Bk); }

            //Probably some other scene parameters. Not tested yet.
            InStream.Skip(43);
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

        //Act defines a overlay or hotzone
        private static void Act(BetterBinaryReader InStream)
        {
            if (InStream.debugprint) { Console.WriteLine($"---ACT {InStream.Position()}---"); }

            int ChunkLength = InStream.ReadIntBE("Chunk Length: ");
            string ActType = Helpers.String(InStream.ReadBytes(48)).TrimEnd('\0');

            switch (ActType)
            {
                case "Static Overlay Image":
                    Act_SOVL(InStream);
                    break;

                case "Event Flags with Cursor and HS":
                    Act_HS(InStream);
                    break;

                case "Scene Change":
                    Act_go(InStream);
                    break;

                default:
                    Console.WriteLine($"Unknown act type: '{ActType}'");
                    FatalError();
                    break;
            }
            if (InStream.debugprint) { Console.WriteLine("---END ACT---\n"); }
        }

        private static void Act_SOVL(BetterBinaryReader InStream)
        {
            if (InStream.debugprint) { Console.WriteLine("   ---SOVL---"); }

            //TODO: convert to enum
            byte type = InStream.ReadByte("Type: ");
            //Once or multiple
            byte trigger = InStream.ReadByte("Trigger: ");

            //validation for reasearch //now known from header
            /*if (type != 52)
                Console.WriteLine($"Unknownvariant: The file: '{InStream.FileName}' has an unknown variant. " +
                    $"Please report the following to the developer. \rtype at '{InStream.Position()}' in '{InStream.FileName}' val of '{type}'\n");
            if (trigger != 1)
                Console.WriteLine($"Unknownvariant: The file: '{InStream.FileName}' has an unknown variant. " +
                    $"Please report the following to the developer. \rtrigger at '{InStream.Position()}' in '{InStream.FileName}' val of '{trigger}'\n");
            */

            //Not entirely sure what the bit widths are supposed to be

            //number of dependencies
            //LITTLE Endian
            int numDeps = InStream.ReadInt("Num deps: ");
            if (InStream.debugprint) { Console.WriteLine("    ---Ref---"); }

            for (int i = 0; i < numDeps; i++)
            {
                //type of RefDep
                byte depType = InStream.ReadByte("Dep type: ");

                //unknown 0. I don't think belongs to another field.
                Helpers.AssertByte(InStream, 0);

                //type of RefFlag
                short depRefFlag = InStream.ReadShort("Dep ref: ");

                //TODO: more here, need different dependency to test
                //All Shorts?
                //InStream.Skip(12);
                short depState = InStream.ReadShort("Dep state: ");
                short depFlag = InStream.ReadShort("Dep flag: ");
                NancyRect rect = new NancyRect(InStream.ReadShort(), InStream.ReadShort(), InStream.ReadShort(), InStream.ReadShort());
                if (InStream.debugprint) { Console.WriteLine(rect); }
                if (InStream.debugprint) { Console.WriteLine("    ---End Ref---"); }
            }

            Helpers.AssertString(InStream, "EndOfDeps", true);
            //Assert only compares provided string length. Need to skip rest of field.
            InStream.Skip(23);

            //Typeof: RefOvlStat
            string name = Helpers.String(InStream.ReadBytes(33)).TrimEnd('\0');
            if (InStream.debugprint) { Console.WriteLine("Name: " + name); }

            //Z order
            byte ZOrder = InStream.ReadByte("ZOrder: ");

            //Assert until can know for sure.
            //Could be 2 bytes or one short
            Helpers.AssertShortBE(InStream, 1);

            //graphic on img
            NancyRect src = new NancyRect(InStream);
            if (InStream.debugprint) { Console.WriteLine("src: " + src); }
            //graphic on screen
            NancyRect dest = new NancyRect(InStream);
            if (InStream.debugprint) { Console.WriteLine("derst: " + dest); }

            //Does this do anything?
            Helpers.AssertIntBE(InStream, 0);

            //MISSING: Scene frame and begin count
            //int frame = InStream.ReadInt();
            //Is 0 in all leakded sources
            if (InStream.debugprint) { Console.WriteLine("   ---END SOVL---"); }
        }

        private static void Act_HS(BetterBinaryReader InStream)
        {
            if (InStream.debugprint) { Console.WriteLine("   ---HS---"); }

            //TODO: convert to enum
            byte type = InStream.ReadByte("Type: ");
            //Once or multiple
            byte trigger = InStream.ReadByte("Trigger: ");

            //validation for reasearch //now known from header
            /*if (type != 52)
                Console.WriteLine($"Unknownvariant: The file: '{InStream.FileName}' has an unknown variant. " +
                    $"Please report the following to the developer. \rtype at '{InStream.Position()}' in '{InStream.FileName}' val of '{type}'\n");
            if (trigger != 1)
                Console.WriteLine($"Unknownvariant: The file: '{InStream.FileName}' has an unknown variant. " +
                    $"Please report the following to the developer. \rtrigger at '{InStream.Position()}' in '{InStream.FileName}' val of '{trigger}'\n");
            */

            //Not entirely sure what the bit widths are supposed to be

            //number of dependencies
            //LITTLE Endian
            int numDeps = InStream.ReadInt("Num deps: ");
            if (InStream.debugprint) { Console.WriteLine("    ---Ref---"); }

            for (int i = 0; i < numDeps; i++)
            {
                //type of RefDep
                byte depType = InStream.ReadByte("Dep type: ");

                //unknown 0. I don't think belongs to another field.
                Helpers.AssertByte(InStream, 0);

                //type of RefFlag
                short depRefFlag = InStream.ReadShort("Dep ref: ");

                //TODO: more here, need different dependency to test
                //All Shorts?
                //InStream.Skip(12);
                short depState = InStream.ReadShort("Dep state: ");
                short depFlag = InStream.ReadShort("Dep flag: ");
                NancyRect rect = new NancyRect(InStream.ReadShort(), InStream.ReadShort(), InStream.ReadShort(), InStream.ReadShort());
                if (InStream.debugprint) { Console.WriteLine(rect); }
                if (InStream.debugprint) { Console.WriteLine("    ---End Ref---"); }
            }

            Helpers.AssertString(InStream, "EndOfDeps", true);
            //Assert only compares provided string length. Need to skip rest of field.
            InStream.Skip(23);
            //////////////above is standard?

            //Number of variables to set by Hotzone
            short numVars = InStream.ReadShort("Num vars: ");

            for (int i = 0; i < numVars; i++)
            {
                short varid = InStream.ReadShort("Var ID: ");
                short state = InStream.ReadShort("State: ");
            }

            //Might be enum for whatever field scale is.
            int unk = InStream.ReadInt("Unknown field: ");

            //Action?
            int action = InStream.ReadInt("Action: ");

            //LITTLE ENDIAN
            //y has 65 added?
            NancyRect pos = new NancyRect(InStream, false);
            if (InStream.debugprint) { Console.WriteLine("pos: " + pos); }

            if (InStream.debugprint) { Console.WriteLine("   ---END HS---"); }
        }

        //Conditionally change scene.
        private static void Act_go(BetterBinaryReader InStream)
        {
            if (InStream.debugprint) { Console.WriteLine($"---GO {InStream.Position()}---"); }

            //NO CHUNK LENGTH?
            //int ChunkLength = InStream.ReadIntBE("Chunk Length: ");
            Helpers.AssertShortBE(InStream, 3841);

            //short unknown = InStream.ReadShort("Unknown: ");

            //number of dependencies
            //LITTLE Endian
            int numDeps = InStream.ReadInt("Num deps: ");
            if (InStream.debugprint) { Console.WriteLine("    ---Ref---"); }

            for (int i = 0; i < numDeps; i++)
            {
                //type of RefDep
                byte depType = InStream.ReadByte("Dep type: ");

                //unknown 0. I don't think belongs to another field.
                Helpers.AssertByte(InStream, 0);

                //type of RefFlag
                short depRefFlag = InStream.ReadShort("Dep ref: ");

                //TODO: more here, need different dependency to test
                //All Shorts?
                //InStream.Skip(12);
                short depState = InStream.ReadShort("Dep state: ");
                short depFlag = InStream.ReadShort("Dep flag: ");
                NancyRect rect = new NancyRect(InStream.ReadShort(), InStream.ReadShort(), InStream.ReadShort(), InStream.ReadShort());
                if (InStream.debugprint) { Console.WriteLine(rect); }
                if (InStream.debugprint) { Console.WriteLine("    ---End Ref---"); }
            }

            Helpers.AssertString(InStream, "EndOfDeps", true);
            //Assert only compares provided string length. Need to skip rest of field.
            InStream.Skip(23);

            short sceneNumber = InStream.ReadShort("Switch to: ");

            if (InStream.debugprint) { Console.WriteLine("---END GO---\n"); }
        }

        //Use seems to be an import/func call
        private static void Use(BetterBinaryReader InStream)
        {
            if (InStream.debugprint) { Console.WriteLine($"---USE {InStream.Position()}---"); }

            int ChunkLength = InStream.ReadIntBE("Chunk Length: ");

            //Don't know what this does. In "use bk" it is 256
            Helpers.AssertShortBE(InStream, 256);

            string name = Helpers.String(InStream.ReadBytes(34)).TrimEnd('\0');
            if (InStream.debugprint) { Console.WriteLine(name); }

            if (InStream.debugprint) { Console.WriteLine("---END USE---\n"); }
        }
    }
}