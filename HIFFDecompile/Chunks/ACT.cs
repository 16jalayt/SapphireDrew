using Sapphire_Extract_Helpers;
using System;

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

                case "Event Flags":
                    HS(InStream);
                    break;

                case "Event Flags with Cursor and HS":
                    HS(InStream);
                    break;

                case "Scene Change":
                    SC(InStream);
                    break;

                case "Scene Change with Hotspot":
                    SC(InStream);
                    break;

                case "Scene Change with Frame":
                    SC(InStream);
                    break;

                case "Fade":
                    FadeOut(InStream);
                    break;

                case "Sound":
                    Sound(InStream);
                    break;

                case "Set_Volume":
                    SetVolume(InStream);
                    break;

                case "Save a Continue Game":
                    SaveContinue(InStream);
                    break;

                case "Set Value":
                    SetValue(InStream);
                    break;

                case "Set Value Combo":
                    SetValueCombo(InStream);
                    break;
                case "Special Effect Action Record":
                    SpecialEffect(InStream);
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

            Utils.ParseDeps(InStream);

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

        private static void HS(BetterBinaryReader InStream)
        {
            if (InStream.debugprint) { Console.WriteLine("   ---HS---"); }

            //AT_FLAGS = 90, AT_FLAGS_HS = 91
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

            Utils.ParseDeps(InStream);

            //Number of variables to set by Hotzone
            short numVars = InStream.ReadShort("Num vars: ");

            for (int i = 0; i < numVars; i++)
            {
                short varid = InStream.ReadShort("Var ID: ");
                short state = InStream.ReadShort("State: ");
            }

            //AT_FLAGS_HS
            if (type == 91)
            {
                //Might be enum for whatever field scale is.
                int unk = InStream.ReadInt("Unknown field: ");

                //Action?
                int action = InStream.ReadInt("Action: ");

                //LITTLE ENDIAN
                //y has 65 added?
                NancyRect pos = new NancyRect(InStream);
                if (InStream.debugprint) { Console.WriteLine("pos: " + pos); }
            }


            if (InStream.debugprint) { Console.WriteLine("   ---END HS---"); }
        }

        //Conditionally change scene.
        private static void SC(BetterBinaryReader InStream)
        {

            //Type of HS
            //AT_SCENE_FRAME_HS = 19, AT_SCENE_FRAME = 16, noral change = 15
            byte HSType = InStream.ReadByte("HSType: ");

            //AE_SINGLE_EXEC = 1
            //AE_MULTI_EXEC	= 2
            byte HSExec = InStream.ReadByte("HSExec: ");

            if (HSType == 19)
            {
                if (InStream.debugprint) { Console.WriteLine($"---Scnene Change with hot {InStream.Position()}---"); }
            }
            else if (HSType == 15)
            {
                if (InStream.debugprint) { Console.WriteLine($"---Scnene Change {InStream.Position()}---"); }
            }
            else if (HSType == 16)
            {
                if (InStream.debugprint) { Console.WriteLine($"---Scnene Change with frame {InStream.Position()}---"); }
            }


            Utils.ParseDeps(InStream);

            short sceneNumber = InStream.ReadShort("Switch to: ");

            if (HSType == 19)
            {
                int frame = InStream.ReadInt("Frame: ");
                //FORWARD_CURSOR = 12
                //UTURN_CURSOR = 19
                int cursor = InStream.ReadInt("Cursor: ");

                //Hotzone position
                NancyRect pos = new NancyRect(InStream);
                if (InStream.debugprint) { Console.WriteLine(pos); }
            }
            else if (HSType == 16)
            {
                int frame = InStream.ReadInt("Frame: ");
            }

            if (InStream.debugprint) { Console.WriteLine("---END SC---\n"); }
        }

        //Found in Ven
        private static void FadeOut(BetterBinaryReader InStream)
        {
            if (InStream.debugprint) { Console.WriteLine("   ---Fade Out---"); }

            //TODO: convert to enum
            byte type = InStream.ReadByte("Type: ");
            //Once or multiple
            byte trigger = InStream.ReadByte("Trigger: ");

            //Not entirely sure what the bit widths are supposed to be

            Utils.ParseDeps(InStream);

            InStream.Skip(36);
            Console.WriteLine("Fade Out Unimplemented");

            if (InStream.debugprint) { Console.WriteLine("   ---END Fade Out---"); }
        }
        private static void Sound(BetterBinaryReader InStream)
        {
            if (InStream.debugprint) { Console.WriteLine("   ---Sound---"); }

            //AT_START_SOUND = 145
            byte type = InStream.ReadByte("Type: ");
            //Once or multiple
            byte trigger = InStream.ReadByte("Trigger: ");

            Utils.ParseDeps(InStream);

            if (InStream.debugprint) { Console.WriteLine("    ---RefSound---"); }

            short numRefSound = InStream.ReadShort();
            for (int i = 0; i < numRefSound; i++)
            {
                string RefSound = Helpers.String(InStream.ReadBytes(33)).TrimEnd('\0');
                if (InStream.debugprint) { Console.WriteLine("Dep Name: " + RefSound); }
            }
            if (InStream.debugprint) { Console.WriteLine("    ---End RefSound---"); }

            //SS_SPEC_EFFECT_CHAN1 = 9
            short chan = InStream.ReadShort("Channel: ");

            //LOOP_ONCE = 1
            int loop = InStream.ReadInt("Loop: ");

            //Unknown
            short unknown = InStream.ReadShort("Unknown: ");

            //next scene before sound ends
            byte nextScene = InStream.ReadByte("Next Scene: ");

            //Referenced scene
            short refScene = InStream.ReadShort("Referenced Scene: ");

            //CCTEXT_TYPE_AUTO = 0
            byte textType = InStream.ReadByte("TextType: ");

            //when sound happens
            short numRefSetFlags = InStream.ReadShort("numRefSetFlags: ");

            short RefSetFlag = InStream.ReadShort("Ref set: ");

            short unknownBool = InStream.ReadShort("Bool: ");

            //Padded in test file
            if (InStream.IsEOF() != true && InStream.ReadByte() != 0)
                InStream.Skip(-1);

            if (InStream.debugprint) { Console.WriteLine("   ---END Sound---"); }
        }

        private static void SetVolume(BetterBinaryReader InStream)
        {
            if (InStream.debugprint) { Console.WriteLine("   ---Set Volume---"); }

            //AT_SET_VOLUME = 147
            byte type = InStream.ReadByte("Type: ");
            //Once or multiple
            byte trigger = InStream.ReadByte("Trigger: ");

            Utils.ParseDeps(InStream);

            //SS_SPEC_EFFECT_CHAN1 = 9
            short channel = InStream.ReadShort("Channel: ");

            int volume = InStream.ReadInt("Volume: ");

            if (InStream.debugprint) { Console.WriteLine("   ---END Set Volume---"); }
        }

        //Autosave?
        private static void SaveContinue(BetterBinaryReader InStream)
        {
            if (InStream.debugprint) { Console.WriteLine("   ---Save Continue---"); }

            //AT_SAVE_CONTINUE_GAME = 102
            byte type = InStream.ReadByte("Type: ");
            //Once or multiple
            byte trigger = InStream.ReadByte("Trigger: ");

            Utils.ParseDeps(InStream);

            if (InStream.debugprint) { Console.WriteLine("   ---END Save Continue---"); }
        }

        private static void SetValue(BetterBinaryReader InStream)
        {
            if (InStream.debugprint) { Console.WriteLine("   ---Set Value---"); }

            //AT_SET_VALUE = 78
            byte type = InStream.ReadByte("Type: ");
            //Once or multiple
            byte trigger = InStream.ReadByte("Trigger: ");

            Utils.ParseDeps(InStream);

            //TABLE_INDEX20 = 20
            byte idx = InStream.ReadByte("Idx: ");
            //ADD_TO_VALUE = 0
            byte operation = InStream.ReadByte("Operation: ");

            short value = InStream.ReadShort("Value: ");

            if (InStream.debugprint) { Console.WriteLine("   ---END Set Value---"); }
        }

        private static void SetValueCombo(BetterBinaryReader InStream)
        {
            if (InStream.debugprint) { Console.WriteLine("   ---Set Value Combo---"); }

            //AT_SET_VALUE_COMBO = 78
            byte type = InStream.ReadByte("Type: ");
            //Once or multiple
            byte trigger = InStream.ReadByte("Trigger: ");

            Utils.ParseDeps(InStream);

            //TABLE_INDEX20  = 20
            byte tableIndex = InStream.ReadByte("Table Index: ");

            //"Values to combine to make new value"
            //TABLE_INDEX_DAY_COUNT = 255
            byte tableIndexCount = InStream.ReadByte("Table Index Count: ");

            //weighting
            short weighting = InStream.ReadShort("Weighting: ");

            if (InStream.debugprint) { Console.WriteLine("    ---Table---"); }
            //Variable length? no counter
            //tableIndexCount - 1 / 2 ??
            for (int i = 0; i < 9; i++)
            {
                //NO_TABLE_INDEX = 252
                byte idx = InStream.ReadByte("Index: ");

                short val = InStream.ReadShort("Value: ");
            }

            if (InStream.debugprint) { Console.WriteLine("    ---End Table---"); }

            //Padded in test file
            if (InStream.IsEOF() != true && InStream.ReadByte() != 0)
                InStream.Skip(-1);

            if (InStream.debugprint) { Console.WriteLine("   ---END Set Value Combo---"); }
        }

        private static void SpecialEffect(BetterBinaryReader InStream)
        {
            if (InStream.debugprint) { Console.WriteLine("   ---Special Effect---"); }

            //AT_SET_VALUE = 78
            byte type = InStream.ReadByte("Type: ");
            //Once or multiple
            byte trigger = InStream.ReadByte("Trigger: ");

            Utils.ParseDeps(InStream);

            //"MS for each fade"
            int fadeTIme = InStream.ReadInt("Fade MS: ");
            //"MS to hold on middle color"
            int middle = InStream.ReadInt("Middle time: ");

            //"Color to fade through (X, R, G, B)"
            NancyRect color = new NancyRect(InStream.ReadByte(), InStream.ReadByte(), InStream.ReadByte(), InStream.ReadByte());
            if (InStream.debugprint) { Console.WriteLine("Color: " + color); }

            if (InStream.debugprint) { Console.WriteLine("   ---END Special Effect---"); }
        }
    }
}
