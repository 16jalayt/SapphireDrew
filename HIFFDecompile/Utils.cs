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
    }
}
