﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIFFCompile.ActionTypes
{
    internal class Misc
    {
        public static bool Cur_Hide(ref BinaryWriter outStream)
        {
            //Does literaly nothing in hiff.
            //Just picked up by engine
            //Ends after deps
            return true;
        }

        public static bool POP(ref BinaryWriter outStream)
        {
            //Does literaly nothing in hiff.
            //Just picked up by engine
            //Ends after deps
            return true;
        }

        public static bool SaveSecondChance(ref BinaryWriter outStream)
        {
            //Does literaly nothing in hiff.
            //Just picked up by engine
            //Ends after deps
            return true;
        }
    }
}