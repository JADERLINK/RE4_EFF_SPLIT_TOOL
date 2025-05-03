using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace EFF_SPLIT
{
    internal static class MainProgram
    {
        public static void Continue(string[] args, IsVersion version, string idxFormat, string toolName)
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            Console.WriteLine($"# {toolName}");
            Console.WriteLine("# By JADERLINK");
            Console.WriteLine("# VERSION 1.2.1 (2025-05-03)");
            Console.WriteLine("# youtube.com/@JADERLINK");

            bool usingBatFile = false;
            int start = 0;
            if (args.Length > 0 && args[0].ToLowerInvariant() == "-bat")
            {
                usingBatFile = true;
                start = 1;
            }

            for (int i = start; i < args.Length; i++)
            {
                if (File.Exists(args[i]))
                {
                    FileInfo fileInfo = null;

                    try
                    {
                        fileInfo = new FileInfo(args[i]);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error in the path: " + Environment.NewLine + ex);
                    }
                    if (fileInfo != null)
                    {
                        Console.WriteLine("File: " + fileInfo.Name);

                        if (fileInfo.Extension.ToUpperInvariant() == ".EFF")
                        {
                            try
                            {
                                Extract.ExtractFile(fileInfo.FullName, version);
                                Extract.GenerateIdx(fileInfo.FullName, idxFormat, toolName);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error: " + Environment.NewLine + ex);
                            }

                        }
                        else if (fileInfo.Extension.ToUpperInvariant() == $".{idxFormat}")
                        {
                            try
                            {
                                Repack.RepackFile(fileInfo.FullName, version);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error: " + Environment.NewLine + ex);
                            }
                        }
                        else
                        {
                            Console.WriteLine("The extension is not valid: " + fileInfo.Extension);
                        }

                    }
                }
                else
                {
                    Console.WriteLine("File specified does not exist: " + args[i]);
                }

            }

            if (args.Length == 0)
            {
                Console.WriteLine("For more information read:");
                Console.WriteLine("https://github.com/JADERLINK/RE4_EFF_SPLIT_TOOL");
                Console.WriteLine("Press any key to close the console.");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Finished!!!");
                if (!usingBatFile)
                {
                    Console.WriteLine("Press any key to close the console.");
                    Console.ReadKey();
                }
            }

        }

    }
}
