using Sapphire_Extract_Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIFFCompile.Chunks
{
    internal class OVL
    {
        public static bool Overlay(ref BinaryWriter outStream)
        {
            //RefOvlStat: Name of OVL file without extension
            if (!InFile.GetNextString(ref outStream, 33))
                return false;

            //Z-Order, 1-4
            if (!InFile.GetNextObject<short>(ref outStream, enumType: Enums.z))
                return false;

            if (InFile.GetNextLine() != "BeginCount int")
            {
                Console.WriteLine($"Unknown count contents: '{InFile.GetLine()}'");
                return false;
            }

            //numDeps placeholder
            long numOVLsPlace = outStream.BaseStream.Position;
            outStream.Write((short)-1);
            short numOVLs = 0;

            while (InFile.GetNextLine() != "EndCount int")
            {
                //Scene frame to show this ovl in
                if (!InFile.GetObject<short>(ref outStream))
                    return false;
                //Src rect
                if (!InFile.GetNextObject<int>(ref outStream))
                    return false;
                //Dest rect
                if (!InFile.GetNextObject<int>(ref outStream))
                    return false;
                numOVLs++;
            }

            long endOVLs = outStream.BaseStream.Position;
            outStream.Seek((int)numOVLsPlace, SeekOrigin.Begin);
            outStream.Write((short)numOVLs);
            outStream.Seek((int)endOVLs, SeekOrigin.Begin);

            return true;
        }
    }
}