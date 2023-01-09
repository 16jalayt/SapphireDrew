using Sapphire_Extract_Helpers;
using System;
using System.IO;

namespace HIFFDecompile
{
    internal static class Utils
    {
        public static bool preferLong = true;

        public static void FatalError()
        {
            Console.WriteLine("\nFatal Error. Exiting...");
            System.Environment.Exit(-1);
        }

        public struct Dependency
        {
            public byte depType;
            public short depRefFlag;
            public short depState;
            public short depFlag;
            public NancyRect rect;
        }

        public static Dependency[] ParseDeps(BetterBinaryReader InStream)
        {
            //number of dependencies
            //LITTLE Endian
            int numDeps = InStream.ReadInt("Num deps: ");
            Dependency[] deps = new Dependency[numDeps];

            //TODO:if numdeps != 0
            if (numDeps != 0 && InStream.debugprint) { Console.WriteLine("    ---Ref---"); }

            for (int i = 0; i < numDeps; i++)
            {
                //type of RefDep
                deps[i].depType = InStream.ReadByte("Dep type: ");

                //unknown 0. I don't think belongs to another field.
                Helpers.AssertByte(InStream, 0);

                //type of RefFlag
                deps[i].depRefFlag = InStream.ReadShort("Dep ref: ");

                //TODO: more here, need different dependency to test
                //All Shorts?
                //InStream.Skip(12);
                deps[i].depState = InStream.ReadShort("Dep state: ");
                deps[i].depFlag = InStream.ReadShort("Dep flag: ");
                deps[i].rect = new NancyRect(InStream.ReadShort(), InStream.ReadShort(), InStream.ReadShort(), InStream.ReadShort());
                if (InStream.debugprint) { Console.WriteLine(deps[i].rect); }
                if (InStream.debugprint) { Console.WriteLine("    ---End Ref---"); }
            }

            Helpers.AssertString(InStream, "EndOfDeps", true);
            //Assert only compares provided string length. Need to skip rest of field.
            InStream.Skip(23);

            return deps;
        }
    }
}