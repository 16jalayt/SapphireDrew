using Sapphire_Extract_Helpers;
using System;
using System.IO;

namespace HIFFCompile.ActionTypes
{
    internal static class Scene
    {
        public static bool SC(ref BinaryWriter outStream, bool withHS, bool withFrame)
        {
            //RefScene
            if (!InFile.GetNextObject<short>(ref outStream, "RefScene"))
            {
                Console.WriteLine($"{InFile.pos + 1}Missing RefScene");
                return false;
            }

            //frame
            if (!InFile.GetNextObject<int>(ref outStream, "long"))
                return false;

            //TODO: use getCursorTemp instead of aray
            //HS cursor
            if (!InFile.GetNextObject<int>(ref outStream, "long", dictType: Enums.cursorDict))
                return false;

            //Hot rect
            if (!InFile.GetNextObject<int>(ref outStream, "long"))
                return false;

            return true;
        }
    }
}