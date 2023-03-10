using Sapphire_Extract_Helpers;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace HIFFCompile
{
    internal class Utils
    {
        public static int WriteLength(ref BinaryWriter outStream, long placeholder)
        {
            long endChunk = outStream.BaseStream.Position;
            outStream.Seek((int)placeholder, SeekOrigin.Begin);
            int length = BinaryPrimitives.ReverseEndianness((int)(endChunk - placeholder - 4));
            outStream.Write(length);
            outStream.Seek((int)endChunk, SeekOrigin.Begin);
            return length;
        }

        public static int ParseDeps(ref BinaryWriter outStream)
        {
            int posPlaceholder = InFile.pos;
            long depsPleceholder = outStream.BaseStream.Position;
            outStream.Write((int)-1);

            ///break out into func?
            int numDeps = 0;
            //while (lines[pos] != "}")
            //{
            //TODO: seperate recursive func insted of loops
            while (InFile.lines[InFile.pos] != "// ------------ Dependency -------------" && InFile.lines[InFile.pos] != "}" && InFile.pos < InFile.lines.Length - 1)
            { InFile.pos++; }

            if (InFile.lines[InFile.pos] == "// ------------ Dependency -------------")
            {
                numDeps++;
                //Console.WriteLine($"Dep start {pos + 1}.");
                //TODO: double check length
                if (!InFile.GetNextObject<short>(ref outStream, enumType: Enums.depType))
                    return -1;
                //TODO: ??? game specific. Need table or something.
                ////Then again, decompiled would be number anyway
                if (!InFile.GetNextObject<short>(ref outStream, enumType: Enums.execType))
                    return -1;

                //Not sure how next two combine
                if (!InFile.GetNextObject<short>(ref outStream, enumType: Enums.tf))
                    return -1;
                if (!InFile.GetNextObject<short>(ref outStream, enumType: Enums.depFlag))
                    return -1;

                //Unknown rect
                if (!InFile.GetNextObject<short>(ref outStream))
                    return -1;
            }
            else
                InFile.pos--;
            //Console.WriteLine($"no dep {pos + 1}.");

            //pos++;
            //}

            ///end deps
            int posEndDeps = InFile.pos;
            InFile.pos = posPlaceholder;

            long depstemp = outStream.BaseStream.Position;
            outStream.Seek((int)depsPleceholder, SeekOrigin.Begin);
            outStream.Write(numDeps);
            outStream.Seek((int)depstemp, SeekOrigin.Begin);

            string endOfDeps = "EndOfDeps";
            endOfDeps = endOfDeps.PadRight(32, '\0');
            outStream.Write(Encoding.UTF8.GetBytes(endOfDeps));

            return posEndDeps;
        }
    }
}