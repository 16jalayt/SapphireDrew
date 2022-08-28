using Sapphire_Extract_Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIFFDecompile.Chunks
{
    internal static class ACT
    {
        //Act defines a overlay or hotzone
        public static void Act(BetterBinaryReader InStream)
        {
            if (InStream.debugprint) { Console.WriteLine($"---ACT {InStream.Position()}---"); }

            int ChunkLength = InStream.ReadIntBE("Chunk Length: ");
            string ActType = Helpers.String(InStream.ReadBytes(48)).TrimEnd('\0');

            switch (ActType)
            {
                case "Static Overlay Image":
                    SOVL(InStream);
                    break;

                case "Event Flags with Cursor and HS":
                    HS(InStream);
                    break;

                case "Scene Change":
                    SC(InStream, false);
                    break;

                case "Scene Change with Hotspot":
                    SC(InStream, true);
                    break;

                default:
                    Console.WriteLine($"Unknown act type: '{ActType}'");
                    Utils.FatalError();
                    break;
            }
            if (InStream.debugprint) { Console.WriteLine("---END ACT---\n"); }
        }

        private static void SOVL(BetterBinaryReader InStream)
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
            NancyRect src = new NancyRect(InStream, true);
            if (InStream.debugprint) { Console.WriteLine("src: " + src); }
            //graphic on screen
            NancyRect dest = new NancyRect(InStream, true);
            if (InStream.debugprint) { Console.WriteLine("dest: " + dest); }

            //Does this do anything?
            Helpers.AssertIntBE(InStream, 0);

            //MISSING: Scene frame and begin count
            //int frame = InStream.ReadInt();
            //Is 0 in all leakded sources
            if (InStream.debugprint) { Console.WriteLine("   ---END SOVL---"); }
        }

        //TODO: incorrrect?
        private static void HS(BetterBinaryReader InStream)
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
            NancyRect pos = new NancyRect(InStream);
            if (InStream.debugprint) { Console.WriteLine("pos: " + pos); }

            if (InStream.debugprint) { Console.WriteLine("   ---END HS---"); }
        }

        //Conditionally change scene.
        private static void SC(BetterBinaryReader InStream, bool withHS)
        {
            if (withHS)
            {
                if (InStream.debugprint) { Console.WriteLine($"---GO WITH HOT {InStream.Position()}---"); }
            }
            else
                if (InStream.debugprint) { Console.WriteLine($"---GO {InStream.Position()}---"); }

            //Type of HS
            //AT_SCENE_FRAME_HS = 19
            byte HSType = InStream.ReadByte("HSType: ");

            //AE_SINGLE_EXEC = 1
            //AE_MULTI_EXEC	= 2
            byte HSExec = InStream.ReadByte("HSExec: ");

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

            //len32
            Helpers.AssertString(InStream, "EndOfDeps", true);
            //Assert only compares provided string length. Need to skip rest of field.
            InStream.Skip(23);

            short sceneNumber = InStream.ReadShort("Switch to: ");

            if (withHS)
            {
                int frame = InStream.ReadInt("Frame: ");
                //FORWARD_CURSOR = 12
                //UTURN_CURSOR = 19
                int cursor = InStream.ReadInt("Cursor: ");

                //Hotzone position
                NancyRect pos = new NancyRect(InStream);
                if (InStream.debugprint) { Console.WriteLine(pos); }
            }

            if (InStream.debugprint) { Console.WriteLine("---END SC---\n"); }
        }
    }
}
