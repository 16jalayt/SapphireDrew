using System;
using System.IO;

namespace Sapphire_Extract_Helpers
{
    internal static class Writer
    {
        public static bool OverwriteAll;

        //TODO: implement
        public static bool AutoRename;

        //TODO: implement
        public static bool OverwriteNone;

        public static string WriteFile(string filePath, string fileName, byte[] fileContents, bool subdir)
        {
            string outPath;
            if (subdir)
            {
                outPath = Path.Combine(Path.GetDirectoryName(filePath)!, Path.GetFileNameWithoutExtension(filePath), fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
            }
            else
            {
                outPath = Path.Combine(Path.GetDirectoryName(filePath)!, fileName);
            }
            //Log.Debug("output file: " + outPath);

            if (OverwriteAll)
            {
                File.WriteAllBytes(outPath, fileContents);
            }
            else
            {
                if (File.Exists(outPath))
                {
                    //Prompt user
                    Console.WriteLine("File exists. Overwrite (yes,all,no,skip all):");
                    string? response = Console.ReadLine();
                    if (response != null)
                        response = response.ToLower();
                    switch (response)
                    {
                        case "y":
                        case "yes":
                            File.WriteAllBytes(outPath, fileContents);
                            break;

                        case "a":
                        case "all":
                            OverwriteAll = true;
                            File.WriteAllBytes(outPath, fileContents);
                            break;

                        case "n":
                        case "no":
                            break;

                        case "s":
                        case "skip":
                            OverwriteNone = true;
                            File.WriteAllBytes(outPath, fileContents);
                            break;

                        default:
                            Console.WriteLine("Invalid response. Skipping:");
                            break;
                    }
                }
                else
                {
                    File.WriteAllBytes(outPath, fileContents);
                }
            }
            return outPath;
        }
    }
}