using HIFFCompile.Chunks;
using Sapphire_Extract_Helpers;
using System;
using System.IO;

namespace HIFFCompile
{
    internal class Act
    {
        public static bool ParseAct(ref BinaryWriter outStream, int actType)
        {
            switch (actType)
            {
                //AT_OVERLAY
                //Display static image
                case 52:
                    return OVL.Overlay(ref outStream);

                //AT_FLAGS_HS
                //Hotspot that sets a flag
                case 91:
                    return Hot.FlagsHS(ref outStream);

                default:
                    Console.WriteLine($"Unknown ACT chunk type:'{actType}' from:'{InFile.GetLine()}' on line {InFile.pos}");
                    return false;
            }
        }
    }
}