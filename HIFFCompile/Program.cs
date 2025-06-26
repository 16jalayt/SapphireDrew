using ManyConsole;
using Sapphire_Extract_Helpers;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace HIFFCompile
{
    internal static class Program
    {
        public class Parameters : ConsoleCommand
        {
            public Parameters()
            {
                IsCommand("Decompile", "Decompiles Nancy Drew HIFF files");

                //TODO: description
                HasOption("o|older=", "Used for older games", o => Verbose = o == null ? true : Convert.ToBoolean(o));

                //HasOption("f|flags=", "A file containing the flags for this game. Can be Flags.hiff or Flags.htxt", p => FileName = p);
                HasOption("f|flags=", "A file containing the flags for this game. Must be Flags.hiff", p => FlagsFileName = p);

                HasOption("v|verbose:", "Prints debug information to console",
            t => Verbose = t == null ? true : Convert.ToBoolean(t));

                AllowsAnyAdditionalArguments("File to extract");
                SkipsCommandSummaryBeforeRunning();
            }

            public override int Run(string[] remainingArguments)
            {
                if (remainingArguments.Length != 1)
                {
                    throw new ConsoleHelpAsException("Invalid syntax.");
                }
                if (!new FileInfo(remainingArguments[0]).Exists)
                {
                    throw new ConsoleHelpAsException("Unable to find file: " + remainingArguments[0]);
                }
                FileName = remainingArguments[0];
                return 0;
            }
        }

        private static bool olderGame = false;
        private static bool Verbose = false;
        private static string? FileName;
        private static string? FlagsFileName;

        //TODO:delete partial file on faliure

        public static void Main(string[] args)
        {
            var commands = ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));

            int returncode = ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
            //Exit if invalid parameters
            if (returncode == -1)
                return;

            //Tool status
            Console.WriteLine("CURRENTLY BROKEN\n");
            //Console.WriteLine("EXPEREMENTAL\n");
            //Console.WriteLine("UNVALIDATED\n");

            if (FileName == "Invalid File" || !File.Exists(FileName))
            {
                Console.WriteLine($"The file: '{FileName}' does not exist.");
                return;
            }

            //TODO: check if exists in subdirectory
            if (FlagsFileName != null)
                Helpers.PopulateFlags(FlagsFileName, Verbose);

            Console.WriteLine($"Compiling: '{FileName}'\n");

            //Only used to get full path of input
            BetterBinaryReader testFile = new BetterBinaryReader(FileName);

            FileInfo outFile = new(Path.GetDirectoryName(testFile.FilePath) + "/Output/" + Path.GetFileNameWithoutExtension(testFile.FilePath) + ".hiff");
            testFile.Dispose();
            if (outFile.Directory != null)
            {
                outFile.Directory.Create();
                BinaryWriter outStream = new BinaryWriter(new FileStream(outFile.FullName, FileMode.Create), Encoding.UTF8);

                //Insert placeholder at zero so line numbers match
                InFile.lines = InFile.lines.Concat(File.ReadLines(FileName).ToArray()).ToArray();

                //Queue up first line so it is ready to use
                InFile.GetNextLine();

                ParseHIFF.Parse(olderGame, outStream, FileName);

                outStream.Close();
            }
        }
    }
}