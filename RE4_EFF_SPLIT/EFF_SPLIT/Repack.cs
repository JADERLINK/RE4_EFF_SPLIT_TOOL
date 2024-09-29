using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace EFF_SPLIT
{
    internal static class Repack
    {
        public static void RepackFilePS2(string fileFullName) 
        {
            RepackFile(fileFullName, false);
        }

        public static void RepackFileUHD(string fileFullName)
        {
            RepackFile(fileFullName, true);
        }

        private static void RepackFile(string fileFullName, bool IsUHD)
        {
            string baseDirectory = Path.GetDirectoryName(fileFullName);
            string baseFileName = Path.GetFileNameWithoutExtension(fileFullName);

            string baseDirectoryPath = Path.Combine(baseDirectory, baseFileName);

            string pattern = "^(00)([0-9]{2})$";
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.CultureInvariant);

            if (regex.IsMatch(baseFileName))
            {
                baseDirectoryPath = Path.Combine(baseDirectory, baseFileName + "_EFF");
            }

            string effBlobPath = Path.Combine(baseDirectoryPath, $"{baseFileName}.EFFBLOB");
            if (!File.Exists(effBlobPath))
            {
                Console.WriteLine($"{baseFileName}.EFFBLOB, Does not exist.");
                return;
            }

            BinaryReader br = new BinaryReader(File.OpenRead(effBlobPath));
            uint Magic = br.ReadUInt32(); //sempre 0x0B
            if (Magic != 0x0B)
            {
                br.Close();
                Console.WriteLine($"{baseFileName}.EFFBLOB, Invalid file!");
                return;
            }

            uint offset_0_Texture_IDs = br.ReadUInt32();
            uint offset_1_Effect_IDs = br.ReadUInt32();
            uint offset_2_EAR_Link = br.ReadUInt32();
            uint offset_3_Unknown_Table = br.ReadUInt32();
            uint offset_4_Model_IDs = br.ReadUInt32();
            uint offset_5_TPL_Offsets = br.ReadUInt32();
            uint offset_6_Texture_Metadata = br.ReadUInt32();
            uint offset_7_Effect_0_Type = br.ReadUInt32();
            uint offset_8_Effect_1_Type = br.ReadUInt32();
            uint offset_9_Paths = br.ReadUInt32();
            uint offset_10_Data_Offset = br.ReadUInt32();

            TablesGroup tables = new TablesGroup();
            tables.Table00 = Separate.TableIndexEntry(br, offset_0_Texture_IDs, out _);
            tables.Table01 = Separate.TableIndexEntry(br, offset_1_Effect_IDs, out _);
            tables.Table02 = Separate.TableIndexEntry(br, offset_2_EAR_Link, out _);
            tables.Table03 = Separate.TableIndexEntry(br, offset_3_Unknown_Table, out _);
            tables.Table04 = Separate.TableIndexEntry(br, offset_4_Model_IDs, out _);
            tables.Table06 = Separate.Table06(br, offset_6_Texture_Metadata, out _, IsUHD);
            tables.Table09 = Separate.Table09(br, offset_9_Paths, out _);

            tables.Table07_Effect_0_Type = Separate.Effect_Type(br, offset_7_Effect_0_Type, out _);
            tables.Table08_Effect_1_Type = Separate.Effect_Type(br, offset_8_Effect_1_Type, out _);

            br.Close();

            ExtraRepack extra = new ExtraRepack();
            extra.Table05Directory = Path.Combine(baseDirectoryPath, "Effect TPL");
            extra.Table10Directory = Path.Combine(baseDirectoryPath, "Effect Models");

            var eff = new FileInfo(Path.Combine(baseDirectory, $"{baseFileName}.EFF")).Create();
            Join join = new Join(tables);
            join.WriteTable05 = extra.Table05;
            join.WriteTable10 = extra.Table10;
            join.Create_EFF_File(eff, IsUHD);
            eff.Close();

        }

        private class ExtraRepack 
        {
            public string Table05Directory = "";
            public string Table10Directory = "";

            public void Table05(BinaryWriter bw, bool IsUHD)
            {
                uint offsetTable05 = (uint)bw.BaseStream.Position;

                uint iCount = 0;
                bool asFile = true;

                while (asFile)
                {
                    string tplPath = Path.Combine(Table05Directory, iCount.ToString("D") + ".TPL");

                    if (File.Exists(tplPath))
                    {
                        iCount++;
                    }
                    else
                    {
                        asFile = false;
                    }
                }

                bw.Write(iCount); // quantidade
                uint offsetToOffset = (uint)bw.BaseStream.Position;

                uint calc = 4 + (iCount * 4);
                uint _line = calc / 16;
                uint rest = calc % 16;
                _line += rest != 0 ? 1u : 0u;
                calc = (_line * 16) - 4;
                bw.Write(new byte[calc]);

                uint nextOffset = (uint)bw.BaseStream.Position;

                for (int i = 0; i < iCount; i++)
                {
                    bw.BaseStream.Position = offsetToOffset;
                    uint WriteOffset = nextOffset - offsetTable05;
                    bw.Write(WriteOffset);
                    bw.BaseStream.Position = nextOffset;

                    string tplPath = Path.Combine(Table05Directory, i.ToString("D") + ".TPL");
                    FileInfo fileinfo = new FileInfo(tplPath);

                    var fileStream = fileinfo.OpenRead();
                    fileStream.CopyTo(bw.BaseStream);
                    fileStream.Close();

                    //alinhamento
                    uint aLine = (uint)bw.BaseStream.Position / 16;
                    uint aRest = (uint)bw.BaseStream.Position % 16;
                    aLine += aRest != 0 ? 1u : 0u;
                    int aDif = (int)((aLine * 16) - bw.BaseStream.Position);
                    bw.Write(new byte[aDif]);

                    nextOffset = (uint)bw.BaseStream.Position;
                    offsetToOffset += 4;
                }

                Console.WriteLine("Inserted " + iCount + " Effect TPL;");
            }

            public void Table10(BinaryWriter bw, bool IsUHD)
            {
                uint offsetTable10 = (uint)bw.BaseStream.Position;

                uint iCount = 0;
                bool asFile = true;

                while (asFile)
                {
                    string binPath = Path.Combine(Table10Directory, iCount.ToString("D") + ".BIN");
                    string tplPath = Path.Combine(Table10Directory, iCount.ToString("D") + ".TPL");

                    if (File.Exists(tplPath) && File.Exists(binPath))
                    {
                        iCount++;
                    }
                    else
                    {
                        asFile = false;
                    }
                }

                bw.Write(iCount); // quantidade
                uint offsetToOffset = (uint)bw.BaseStream.Position;

                uint calc = 4 + (iCount * 4);
                uint _line = calc / 16;
                uint rest = calc % 16;
                _line += rest != 0 ? 1u : 0u;
                calc = (_line * 16) - 4;
                bw.Write(new byte[calc]);

                uint nextOffset = (uint)bw.BaseStream.Position;

                for (int i = 0; i < iCount; i++)
                {
                    bw.BaseStream.Position = offsetToOffset;
                    uint WriteOffset = nextOffset - offsetTable10;
                    bw.Write(WriteOffset);
                    bw.BaseStream.Position = nextOffset;

                    uint Fixed = IsUHD ? 4u : 2u;
                    bw.Write(Fixed);
                    bw.Write(new byte[12]);
                    if (IsUHD)
                    {
                        bw.Write(new byte[16]);
                    }

                    uint BinOffset = (uint)bw.BaseStream.Position;
                    {
                        string binPath = Path.Combine(Table10Directory, i.ToString("D") + ".BIN");
                        FileInfo fileinfo = new FileInfo(binPath);

                        var fileStream = fileinfo.OpenRead();
                        fileStream.CopyTo(bw.BaseStream);
                        fileStream.Close();

                        //alinhamento
                        uint aLine = (uint)bw.BaseStream.Position / 16;
                        uint aRest = (uint)bw.BaseStream.Position % 16;
                        aLine += aRest != 0 ? 1u : 0u;
                        int aDif = (int)((aLine * 16) - bw.BaseStream.Position);
                        bw.Write(new byte[aDif]);
                    }


                    uint TplOffset = (uint)bw.BaseStream.Position;
                    {
                        string tplPath = Path.Combine(Table10Directory, i.ToString("D") + ".TPL");
                        FileInfo fileinfo = new FileInfo(tplPath);

                        var fileStream = fileinfo.OpenRead();
                        fileStream.CopyTo(bw.BaseStream);
                        fileStream.Close();

                        //alinhamento
                        uint aLine = (uint)bw.BaseStream.Position / 16;
                        uint aRest = (uint)bw.BaseStream.Position % 16;
                        aLine += aRest != 0 ? 1u : 0u;
                        int aDif = (int)((aLine * 16) - bw.BaseStream.Position);
                        bw.Write(new byte[aDif]);
                    }

                    uint currentNextOffset = nextOffset;
                    nextOffset = (uint)bw.BaseStream.Position;
                    offsetToOffset += 4;

                    // offset bin/tpl
                    bw.BaseStream.Position = currentNextOffset + 4;
                    uint WriteBinOffset = BinOffset - currentNextOffset;
                    uint WriteTplOffset = TplOffset - currentNextOffset;
                    bw.Write(WriteBinOffset);
                    bw.Write(WriteTplOffset);
                }

                Console.WriteLine("Inserted " + iCount + " Effect Models;");
            }

        }

    }
}
