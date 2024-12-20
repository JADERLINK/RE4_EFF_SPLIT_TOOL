﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RE4_GCWII_EFF_SPLIT
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            Console.WriteLine("# RE4 GCWII EFF SPLIT");
            Console.WriteLine("# By JADERLINK");
            Console.WriteLine("# VERSION 1.2.0 (2024-11-23)");
            Console.WriteLine("# youtube.com/@JADERLINK");

            if (args.Length == 0)
            {
                Console.WriteLine("For more information read:");
                Console.WriteLine("https://github.com/JADERLINK/RE4_EFF_SPLIT_TOOL");
                Console.WriteLine("Press any key to close the console.");
                Console.ReadKey();
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
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
                                    EFF_SPLIT.Extract.ExtractFileGCWII(fileInfo.FullName);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Error: " + Environment.NewLine + ex);
                                }

                            }
                            else if (fileInfo.Extension.ToUpperInvariant() == ".IDX_GCWII_EFF_SPLIT")
                            {
                                try
                                {
                                    EFF_SPLIT.Repack.RepackFileGCWII(fileInfo.FullName);
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
            }

            Console.WriteLine("Finished!!!");
        }
    }
}
