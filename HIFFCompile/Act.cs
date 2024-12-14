using HIFFCompile.ActionTypes;
using System;
using System.IO;

namespace HIFFCompile
{
    internal static class Act
    {
        public static bool ParseAct(ref BinaryWriter outStream, int actType)
        {
            switch (actType)
            {
                //AT_SCENE_FRAME_HS
                case 19:
                    return Scene.SC(ref outStream, true, true);
                //AT_HIDE_CURSOR_AND_DISABLE_INPUT1
                case 28:
                    return Misc.Cur_Hide(ref outStream);
                //AT_OVERLAY
                //Display static image
                case 52:
                    return OVL.Overlay(ref outStream);

                //AT_FLAGS
                case 90:
                    return Hot.FlagsHS(ref outStream, false);

                //AT_FLAGS_HS
                //Hotspot that sets a flag
                case 91:
                    return Hot.FlagsHS(ref outStream, true);

                //AT_SAVE_CONTINUE_GAME
                case 102:
                    return Misc.SaveSecondChance(ref outStream);

                //AT_POP_SCENE
                case 111:
                    return Misc.POP(ref outStream);

                default:
                    Console.WriteLine($"Unknown ACT chunk type:'{actType}' from:'{InFile.lines[InFile.pos - 1]}' on line {InFile.pos}");
                    return false;
            }
        }
    }
}