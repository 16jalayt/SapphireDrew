using Sapphire_Extract_Helpers;
using System;
using System.IO;

namespace HIFFDecompile.Chunks
{
    internal static class ACT
    {
        //Act defines a overlay or hotzone
        public static void Act(BetterBinaryReader InStream, StreamWriter writetext)
        {
            if (InStream.debugprint) { Console.WriteLine($"---ACT {InStream.Position()}---"); }

            int ChunkLength = InStream.ReadIntBE("Chunk Length: ");
            //string ActDesc = Helpers.String(InStream.ReadBytes(48)).TrimEnd('\0');
            InStream.Skip(48);
            byte type = InStream.ReadByte("Act type: ");
            InStream.Skip(-49);

            switch (type)
            {
                //AT_OVERLAY
                case 52:
                    SOVL(InStream, writetext);
                    break;
                //AT_FLAGS or AT_FLAGS_HS
                case 90:
                case 91:
                    HS(InStream, writetext);
                    break;
                //AT_SCENE_FRAME_HS = 19, AT_SCENE_FRAME = 16, noral change = 15
                case 19:
                case 16:
                case 15:
                    SC(InStream, writetext);
                    break;

                /*case "Fade":
                    FadeOut(InStream, writetext);
                    break;*/
                //AT_START_SOUND
                case 145:
                    Sound(InStream, writetext);
                    break;

                //AT_SET_VOLUME
                case 147:
                    SetVolume(InStream, writetext);
                    break;

                //AT_SAVE_CONTINUE_GAME
                case 102:
                    SaveContinue(InStream, writetext);
                    break;

                //TODO:value conflict
                //AT_SET_VALUE
                case 78:
                    SetValue(InStream, writetext);
                    break;
                //TODO:value conflict
                //AT_SET_VALUE_COMBO
                /*case 78:
                    SetValueCombo(InStream, writetext);
                    break;*/

                //TODO:value conflict
                //AT_SET_VALUE
                /*case 78:
                    SpecialEffect(InStream, writetext);
                    break;*/

                default:
                    Console.WriteLine($"Unknown act type: '{type}'");
                    Utils.FatalError();
                    break;
            }
            if (InStream.debugprint) { Console.WriteLine("---END ACT---\n"); }
        }

        private static void SOVL(BetterBinaryReader InStream, StreamWriter writetext)
        {
            if (InStream.debugprint) { Console.WriteLine("   ---SOVL---"); }

            string ActDesc = Helpers.String(InStream.ReadBytes(48)).TrimEnd('\0');
            if (InStream.debugprint) { Console.WriteLine(ActDesc); }

            //AT_OVERLAY = 52
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
            short ZOrder = InStream.ReadByte("ZOrder: ");

            //TODO: have to loop here
            short numOVLs = InStream.ReadByte("numOVLs: ");

            //Scene frame to draw in
            short frame = InStream.ReadByte("frame: ");

            //graphic on img
            NancyRect src = new NancyRect(InStream, true);
            if (InStream.debugprint) { Console.WriteLine("src: " + src); }
            //graphic on screen
            NancyRect dest = new NancyRect(InStream, true);
            if (InStream.debugprint) { Console.WriteLine("dest: " + dest); }

            //Does this do anything?
            //Helpers.AssertIntBE(InStream, 0);
            //?padding to even chunk length?
            int padding = InStream.ReadByte("padding check");
            if (padding != 0)
                InStream.Seek(-1);

            //MISSING: Scene frame and begin count
            //int frame = InStream.ReadInt();
            //Is 0 in all leakded sources
            if (InStream.debugprint) { Console.WriteLine("   ---END SOVL---"); }
        }

        private static void HS(BetterBinaryReader InStream, StreamWriter writetext)
        {
            if (InStream.debugprint) { Console.WriteLine("   ---HS---"); }

            string ActDesc = Helpers.String(InStream.ReadBytes(48)).TrimEnd('\0');
            if (InStream.debugprint) { Console.WriteLine(ActDesc); }

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
        private static void SC(BetterBinaryReader InStream, StreamWriter writetext)
        {
            string ActDesc = Helpers.String(InStream.ReadBytes(48)).TrimEnd('\0');
            if (InStream.debugprint) { Console.WriteLine(ActDesc); }

            //Type of HS
            //AT_SCENE_FRAME_HS = 19, AT_SCENE_FRAME = 16, noral change = 15
            byte HSType = InStream.ReadByte("HSType: ");

            if (HSType == 19)
            {
                if (InStream.debugprint) { Console.WriteLine($"---Scene Change with hot {InStream.Position()}---"); }
            }
            else if (HSType == 15)
            {
                if (InStream.debugprint) { Console.WriteLine($"---Scene Change {InStream.Position()}---"); }
            }
            else if (HSType == 16)
            {
                if (InStream.debugprint) { Console.WriteLine($"---Scene Change with frame {InStream.Position()}---"); }
            }

            //AE_SINGLE_EXEC = 1
            //AE_MULTI_EXEC	= 2
            byte HSExec = InStream.ReadByte("HSExec: ");

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
        private static void FadeOut(BetterBinaryReader InStream, StreamWriter writetext)
        {
            if (InStream.debugprint) { Console.WriteLine("   ---Fade Out---"); }

            string ActDesc = Helpers.String(InStream.ReadBytes(48)).TrimEnd('\0');
            if (InStream.debugprint) { Console.WriteLine(ActDesc); }

            byte type = InStream.ReadByte("Type: ");
            //Once or multiple
            byte trigger = InStream.ReadByte("Trigger: ");

            //Not entirely sure what the bit widths are supposed to be

            Utils.ParseDeps(InStream);

            InStream.Skip(36);
            Console.WriteLine("Fade Out Unimplemented");

            if (InStream.debugprint) { Console.WriteLine("   ---END Fade Out---"); }
        }

        private static void Sound(BetterBinaryReader InStream, StreamWriter writetext)
        {
            if (InStream.debugprint) { Console.WriteLine("   ---Sound---"); }

            string ActDesc = Helpers.String(InStream.ReadBytes(48)).TrimEnd('\0');
            if (InStream.debugprint) { Console.WriteLine(ActDesc); }

            //AT_START_SOUND = 145
            byte type = InStream.ReadByte("Type: ");
            //Once or multiple
            byte trigger = InStream.ReadByte("Trigger: ");

            Utils.Dependency[] deps = Utils.ParseDeps(InStream);

            if (InStream.debugprint) { Console.WriteLine("    ---RefSound---"); }

            short numRefSound = InStream.ReadShort();
            string[] refSounds = new string[numRefSound];
            for (int i = 0; i < numRefSound; i++)
            {
                refSounds[i] = Helpers.String(InStream.ReadBytes(33)).TrimEnd('\0');
                if (InStream.debugprint) { Console.WriteLine("Dep Name: " + refSounds[i]); }
            }
            if (InStream.debugprint) { Console.WriteLine("    ---End RefSound---"); }

            //SS_SPEC_EFFECT_CHAN1 = 9
            short chan = InStream.ReadShort("Channel: ");

            //LOOP_ONCE = 1
            int loop = InStream.ReadInt("Loop: ");

            //Unknown. volume?
            short unknown = InStream.ReadShort("Unknown: ");

            //next scene before sound ends
            byte nextScene = InStream.ReadByte("Next Scene: ");

            //Referenced scene
            short refScene = InStream.ReadShort("Referenced Scene: ");

            //CCTEXT_TYPE_AUTO = 0
            byte textType = InStream.ReadByte("TextType: ");

            //when sound happens
            short numRefSetFlags = InStream.ReadShort("numRefSetFlags: ");
            short[] refSetFlags = new short[numRefSound];
            short[] RefSetFlagTruths = new short[numRefSound];
            //TODO:fix formatting
            for (int i = 0; i < numRefSetFlags; i++)
            {
                refSetFlags[i] = InStream.ReadShort("Ref set: ");

                RefSetFlagTruths[i] = InStream.ReadShort("Bool: ");
            }

            //Padded in test file
            if (InStream.IsEOF() != true && InStream.ReadByte() != 0)
                InStream.Skip(-1);

            //Unknown if shortinable
            /*if (!Utils.preferLong)
            {
                writetext.Write($"sound");

                writetext.Write($"\n");
            }
            else
            {*/

            //TODO:print deps
            writetext.WriteLine($"CHUNK ACT {{");
            writetext.WriteLine($"char[48]  \"{ActDesc}\"");
            writetext.WriteLine($"byte    {Enums.ACT_Type[type]}");
            writetext.WriteLine($"byte    {Enums.execType[trigger]}");
            writetext.WriteLine($"BeginCount  RefSound");
            for (int i = 0; i < numRefSound; i++)
            {
                writetext.WriteLine($"RefSound  \"{refSounds[i]}\"");
            }
            writetext.WriteLine($"EndCount  RefSound");
            writetext.WriteLine($"int     {Enums.soundChannel[chan]}");
            writetext.WriteLine($"long    {Enums.loop[loop]}");
            writetext.WriteLine($"int     {unknown}");
            writetext.WriteLine($"byte    {Enums.tf[nextScene]}         // next scene before sound ends?");
            if (refScene == 9999)
                writetext.WriteLine($"RefScene  NO_SCENE");
            else
                writetext.WriteLine($"RefScene  {refScene}");
            writetext.WriteLine($"// the name of the text key must match the name of the sound file");
            writetext.WriteLine($"byte    {Enums.CCTEXT_TYPE[textType]}    // _SCROLL, _SHORT, _NONE");
            writetext.WriteLine($"BeginCount  RefSetFlag");
            for (int i = 0; i < numRefSetFlags; i++)
            {
                if (refSetFlags[i] == -1)
                    writetext.WriteLine($"RefSetFlag  EV_NO_EVENT     // when sound begins");
                else
                    writetext.WriteLine($"RefSetFlag  {refSetFlags[i]}     // when sound begins");

                writetext.WriteLine($"int     {Enums.tf[RefSetFlagTruths[i]]}");
            }
            writetext.WriteLine($"EndCount  RefSetFlag");
            if (deps.Length > 0)
            {
                writetext.WriteLine($"// ------------ Dependency -------------");
                for (int i = 0; i < deps.Length; i++)
                {
                    writetext.WriteLine($"RefDep      {deps[i].depType}");
                    writetext.WriteLine($"RefFlag   {deps[i].depRefFlag}");
                    writetext.WriteLine($"int     {Enums.tf[deps[i].depState]}");
                    writetext.WriteLine($"int     {Enums.depFlag[deps[i].depFlag]}");
                    writetext.WriteLine($"int     {deps[i].rect.p1x},{deps[i].rect.p1y},{deps[i].rect.p2x},{deps[i].rect.p2y}");
                }
            }

            writetext.WriteLine($"}}\n");

            if (InStream.debugprint) { Console.WriteLine("   ---END Sound---"); }
        }

        private static void SetVolume(BetterBinaryReader InStream, StreamWriter writetext)
        {
            if (InStream.debugprint) { Console.WriteLine("   ---Set Volume---"); }

            string ActDesc = Helpers.String(InStream.ReadBytes(48)).TrimEnd('\0');
            if (InStream.debugprint) { Console.WriteLine(ActDesc); }

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
        private static void SaveContinue(BetterBinaryReader InStream, StreamWriter writetext)
        {
            if (InStream.debugprint) { Console.WriteLine("   ---Save Continue---"); }

            string ActDesc = Helpers.String(InStream.ReadBytes(48)).TrimEnd('\0');
            if (InStream.debugprint) { Console.WriteLine(ActDesc); }

            //AT_SAVE_CONTINUE_GAME = 102
            byte type = InStream.ReadByte("Type: ");
            //Once or multiple
            byte trigger = InStream.ReadByte("Trigger: ");

            Utils.ParseDeps(InStream);

            if (InStream.debugprint) { Console.WriteLine("   ---END Save Continue---"); }
        }

        private static void SetValue(BetterBinaryReader InStream, StreamWriter writetext)
        {
            if (InStream.debugprint) { Console.WriteLine("   ---Set Value---"); }

            string ActDesc = Helpers.String(InStream.ReadBytes(48)).TrimEnd('\0');
            if (InStream.debugprint) { Console.WriteLine(ActDesc); }

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

        private static void SetValueCombo(BetterBinaryReader InStream, StreamWriter writetext)
        {
            if (InStream.debugprint) { Console.WriteLine("   ---Set Value Combo---"); }

            string ActDesc = Helpers.String(InStream.ReadBytes(48)).TrimEnd('\0');
            if (InStream.debugprint) { Console.WriteLine(ActDesc); }

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

        private static void SpecialEffect(BetterBinaryReader InStream, StreamWriter writetext)
        {
            if (InStream.debugprint) { Console.WriteLine("   ---Special Effect---"); }

            string ActDesc = Helpers.String(InStream.ReadBytes(48)).TrimEnd('\0');
            if (InStream.debugprint) { Console.WriteLine(ActDesc); }

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