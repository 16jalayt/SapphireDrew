using Sapphire_Extract_Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIFFCompile
{
    internal class ActChunks
    {
        public static void ACTSorter(BinaryWriter outStream, string actType)
        {
            switch (actType)
            {
                case "AT_SCENE_FRAME_HS":
                    SceneChange(outStream);
                    break;

                case "AT_START_SOUND":
                    Sound(outStream);
                    break;

                default:
                    throw new Exception($"Unknown ACT type: '{actType}' at line: {InFile.pos}");
            }
        }

        public static void SceneChange(BinaryWriter outStream)
        {
            //RefScene
            InFile.WriteObject(outStream, "RefScene");

            //TODO: split
            //Frame to change to
            InFile.WriteObject(outStream, "long");

            //Hover cursor
            InFile.WriteObject(outStream, "long", Enums.cursorDictReverse);

            //Hotspot rect
            //TODO: find better way?
            InFile.GetNextLine();
            InFile.WriteTokenObject(outStream, "long");
            InFile.WriteTokenObject(outStream, "long");
            InFile.WriteTokenObject(outStream, "long");
            InFile.WriteTokenObject(outStream, "long");
        }

        public static void Sound(BinaryWriter outStream)
        {
            long soundPlace = outStream.BaseStream.Position;
            outStream.Write((short)-1);

            if (InFile.GetNextLine() != "BeginCount RefSound")
            {
                throw new Exception($"Unknown use contents: '{InFile.GetLine()}'. Should be: 'BeginCount RefSound'");
            }

            short numSounds = 0;
            while (InFile.GetNextLine() != "EndCount RefSound")
            {
                //Must be immediate string because we already advanced the line pointer with the loop
                InFile.WriteImmediateString(outStream, "RefSound", 33);
                numSounds++;
            }

            Utils.WriteShortAtPos(outStream, numSounds, soundPlace);

            InFile.WriteObject(outStream, "int", Enums.soundChannel);
            InFile.WriteObject(outStream, "long", Enums.loop);
            InFile.WriteObject(outStream, "int"); //volume
            InFile.WriteObject(outStream, "byte", Enums.tf); // next scene before sound ends?
            InFile.WriteObject(outStream, "RefScene"); //scene to change to
            InFile.WriteObject(outStream, "byte", Enums.CCTEXT_TYPE);

            long refPlace = outStream.BaseStream.Position;
            outStream.Write((short)-1);

            if (InFile.GetNextLine() != "BeginCount RefSetFlag")
            {
                throw new Exception($"Unknown use contents: '{InFile.GetLine()}'. Should be: 'BeginCount RefSetFlag'");
            }

            short numDeps = 0;
            while (InFile.PeekNextLine() != "EndCount RefSetFlag")
            {
                //Must be immediate string because we already advanced the line pointer with the loop
                InFile.WriteObject(outStream, "RefSetFlag", Enums.flagsReverse);
                //InFile.GetNextLine();
                InFile.WriteObject(outStream, "int", Enums.tf);
                numDeps++;
            }
            InFile.pos++;

            Utils.WriteShortAtPos(outStream, numDeps, refPlace);
            outStream.Flush();
        }
    }
}