using Sapphire_Extract_Helpers;
using System;

namespace HIFFDecompile
{
    internal static class Utils
    {
        public static void FatalError()
        {
            Console.WriteLine("\nFatal Error. Exiting...");
            System.Environment.Exit(-1);
        }

        public static void ParseDeps(BetterBinaryReader InStream)
        {
            //number of dependencies
            //LITTLE Endian
            int numDeps = InStream.ReadInt("Num deps: ");
            //TODO:if numdeps != 0
            if (numDeps != 0 && InStream.debugprint) { Console.WriteLine("    ---Ref---"); }

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
        }
    }
}
