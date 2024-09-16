using System;

namespace CIFPack
{
    internal static class CIF
    {
        //Logic to select which format to use
        public static void PackCIFTree(string FileName, int gamenum)
        {
            if (gamenum == 32)
            {
                //TODO:
                //Console.WriteLine("SEA not currently supported.");
                Console.WriteLine("Game SEA does not pack it's resources. Creating loose files instead.");
                return;
            }
            else if (gamenum == 0)
            {
                //TODO:
                //Console.WriteLine("VAM not currently supported.");
                Console.WriteLine("Game VAM does not pack it's resources. Creating loose files instead.");
                return;
            }

            //SCK through SKULL are v2
            if (gamenum >= 1 && gamenum <= 17)
            {
                V2.generateCIFTree(FileName, gamenum);
            }

            //VEN through LIES are v3
            if (gamenum >= 18 && gamenum <= 31)
            {
                V3.generateCIFTree(FileName, gamenum);
            }
        }

        //Logic to select which format to use
        public static void PackCIFFile(string FileName, int gamenum)
        {
            //SCK through SKULL are v2
            if (gamenum >= 1 && gamenum <= 17)
            {
                V2.generateCIFChunk(FileName, gamenum);
            }

            //VEN through LIES are v3
            if (gamenum >= 18 && gamenum <= 31)
            {
                V3.generateCIFFile(FileName, gamenum);
            }
        }
    }
}