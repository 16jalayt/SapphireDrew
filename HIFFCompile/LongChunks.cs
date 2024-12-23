using Sapphire_Extract_Helpers;
using System;
using System.IO;
using System.Text;

namespace HIFFCompile
{
    internal static class LongChunks
    {
        public static void ACTChunk(BinaryWriter outStream)
        {
        }

        public static void TSUMChunk(BinaryWriter outStream)
        {
            outStream.Write(Encoding.UTF8.GetBytes("SCENTSUM"));
            long scenPlace = outStream.BaseStream.Position;
            outStream.Write((int)-1);

            //Scene description
            InFile.WriteString(outStream, "char[50]", 50);
            //Background file without extension
            InFile.WriteString(outStream, "RefAVF", 33);
            //Background sound
            InFile.WriteString(outStream, "RefSound", 33);

            //Channel of backgound sound
            InFile.WriteObject(outStream, "int", Enums.soundChannel);
            //Loop background sound?
            InFile.WriteObject(outStream, "long", Enums.loop);
            //Left channel volume for background sound
            InFile.WriteObject(outStream, "int");
            //Right channel volume for background sound
            InFile.WriteObject(outStream, "int");

            Utils.WriteLength(outStream, scenPlace);
        }

        public static void USEChunk(BinaryWriter outStream)
        {
        }
    }
}