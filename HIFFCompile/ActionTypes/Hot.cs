﻿using Sapphire_Extract_Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIFFCompile.ActionTypes
{
    internal class Hot
    {
        public static bool FlagsHS(ref BinaryWriter outStream, bool withHS)
        {
            if (InFile.GetNextLine() != "BeginCount RefSetFlag")
            {
                Console.WriteLine($"Unknown count contents: '{InFile.GetLine()}'");
                return false;
            }

            //numFlags placeholder
            long numFlagsPlace = outStream.BaseStream.Position;
            outStream.Write((short)-1);
            short numFlags = 0;

            while (InFile.GetNextLine() != "EndCount RefSetFlag")
            {
                //Flag to set
                if (!InFile.GetObject<short>(ref outStream))
                    return false;
                //Value to set
                if (!InFile.GetNextObject<short>(ref outStream, Enums.tf))
                    return false;
                numFlags++;
            }

            long endFlags = outStream.BaseStream.Position;
            outStream.Seek((int)numFlagsPlace, SeekOrigin.Begin);
            outStream.Write((short)numFlags);
            outStream.Seek((int)endFlags, SeekOrigin.Begin);

            if (withHS)
            {
                //Set hover cursor
                if (!InFile.GetNextObject<int>(ref outStream, Enums.cursor))
                    return false;

                if (InFile.GetNextLine() != "BeginCount long")
                {
                    Console.WriteLine($"Unknown count contents: '{InFile.GetLine()}'");
                    return false;
                }

                //numDeps placeholder
                long numHotsPlace = outStream.BaseStream.Position;
                outStream.Write((short)-1);
                short numHots = 0;

                //TODO: convert counts to lambdas?
                while (InFile.GetNextLine() != "EndCount long")
                {
                    //Frame hotspot is active in
                    if (!InFile.GetObject<short>(ref outStream))
                        return false;
                    //screen rect
                    if (!InFile.GetNextObject<int>(ref outStream))
                        return false;
                    numHots++;
                }

                long endHots = outStream.BaseStream.Position;
                outStream.Seek((int)numHotsPlace, SeekOrigin.Begin);
                outStream.Write((short)numHots);
                outStream.Seek((int)endHots, SeekOrigin.Begin);
            }

            return true;
        }
    }
}