using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SimpleEndianBinaryIO;

namespace EFF_SPLIT
{
    internal static class Extract
    {
        public static void ExtractFilePS2(string fileFullName) 
        {
            ExtractFile(fileFullName, IsVersion.IsPS2);
            GenerateIdx(fileFullName, "IDX_PS2_EFF_SPLIT", "RE4 PS2 EFF SPLIT");
        }

        public static void ExtractFileUHD(string fileFullName)
        {
            ExtractFile(fileFullName, IsVersion.IsUHD);
            GenerateIdx(fileFullName, "IDX_UHD_EFF_SPLIT", "RE4 UHD EFF SPLIT");
        }

        public static void ExtractFilePS4NS(string fileFullName)
        {
            ExtractFile(fileFullName, IsVersion.IsPS4NS);
            GenerateIdx(fileFullName, "IDX_PS4NS_EFF_SPLIT", "RE4 PS4NS EFF SPLIT");
        }

        public static void ExtractFileGCWII(string fileFullName) 
        {
            ExtractFile(fileFullName, IsVersion.IsGCWII);
            GenerateIdx(fileFullName, "IDX_GCWII_EFF_SPLIT", "RE4 GCWII EFF SPLIT");
        }

        public static void ExtractFileX360(string fileFullName)
        {
            ExtractFile(fileFullName, IsVersion.IsX360);
            GenerateIdx(fileFullName, "IDX_X360_EFF_SPLIT", "RE4 X360 EFF SPLIT");
        }

        private static void GenerateIdx(string fileFullName, string idxFormat, string toolName) 
        {
            string baseDirectory = Path.GetDirectoryName(fileFullName);
            string baseFileName = Path.GetFileNameWithoutExtension(fileFullName);
            var IDX_EFF_SPLIT = new FileInfo(Path.Combine(baseDirectory, $"{baseFileName}.{idxFormat}")).CreateText();
            IDX_EFF_SPLIT.WriteLine("# github.com/JADERLINK/RE4_EFF_SPLIT_TOOL");
            IDX_EFF_SPLIT.WriteLine($"# {toolName}");
            IDX_EFF_SPLIT.WriteLine("# By JADERLINK");
            IDX_EFF_SPLIT.WriteLine("# youtube.com/@JADERLINK");
            IDX_EFF_SPLIT.Close();
        }


        private static void ExtractFile(string fileFullName, IsVersion version) 
        {
            Endianness endianness = Endianness.LittleEndian;
            string effBlobFormat = "EFFBLOB";
            if (version == IsVersion.IsGCWII || version == IsVersion.IsX360)
            {
                endianness = Endianness.BigEndian;
                effBlobFormat = "EFFBLOBBIG";
            }

            string baseDirectory = Path.GetDirectoryName(fileFullName);
            string baseFileName = Path.GetFileNameWithoutExtension(fileFullName);

            string baseDirectoryPath = Path.Combine(baseDirectory, baseFileName);

            string pattern = "^(00)([0-9]{2})$";
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.CultureInvariant);

            if (regex.IsMatch(baseFileName))
            {
                baseDirectoryPath = Path.Combine(baseDirectory, baseFileName + "_EFF");
            }

            EndianBinaryReader br = new EndianBinaryReader(File.OpenRead(fileFullName), endianness);
            uint Magic = br.ReadUInt32(); //sempre 0x0B
            if (Magic != 0x0B)
            {
                Console.WriteLine("Invalid file!");
                br.Close();
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
            tables.Table06 = Separate.Table06(br, offset_6_Texture_Metadata, out _, version != IsVersion.IsPS2);
            tables.Table09 = Separate.Table09(br, offset_9_Paths, out _);

            tables.Table07_Effect_0_Type = Separate.Effect_Type(br, offset_7_Effect_0_Type, out _, endianness);
            tables.Table08_Effect_1_Type = Separate.Effect_Type(br, offset_8_Effect_1_Type, out _, endianness);

            FileContent[] table05;
            ModelFileContent[] table10;

            if (version == IsVersion.IsUHD || version == IsVersion.IsPS4NS || version == IsVersion.IsX360)
            {
                //uhd or PS4NS or x360
                table05 = UhdExtract.ExtractTable05_TPL(br, offset_5_TPL_Offsets, version == IsVersion.IsPS4NS, endianness);
                table10 = UhdExtract.ExtractTable10_MODEL(br, offset_10_Data_Offset, version == IsVersion.IsPS4NS, endianness);
            }
            else
            {
                //ps2 or GCWII
                table05 = PS2Extract.ExtractTable05_TPL(br, offset_5_TPL_Offsets, offset_6_Texture_Metadata);
                table10 = PS2Extract.ExtractTable10_MODEL(br, offset_10_Data_Offset, br.BaseStream.Length);
            }

            br.Close();

            //parte que grava os arquivos

            string Effect_TPL_Path = Path.Combine(baseDirectoryPath, "Effect TPL");
            string Effect_Models_Path = Path.Combine(baseDirectoryPath, "Effect Models");

            // Create folder
            try
            {
                Directory.CreateDirectory(baseDirectoryPath);
                Directory.CreateDirectory(Effect_TPL_Path);
                Directory.CreateDirectory(Effect_Models_Path);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating directory: " + baseDirectoryPath);
                Console.WriteLine(ex);
            }


            for (int i = 0; i < table05.Length; i++)
            {
                BinaryWriter bwTPL = new BinaryWriter(File.Create(Path.Combine(Effect_TPL_Path, i.ToString("D") + ".TPL")));
                bwTPL.Write(table05[i].Arr);
                bwTPL.Close();
            }

            for (int i = 0; i < table10.Length; i++)
            {
                BinaryWriter bwBIN = new BinaryWriter(File.Create(Path.Combine(Effect_Models_Path, i.ToString("D") + ".BIN")));
                bwBIN.Write(table10[i].BIN.Arr);
                bwBIN.Close();
                BinaryWriter bwTPL = new BinaryWriter(File.Create(Path.Combine(Effect_Models_Path, i.ToString("D") + ".TPL")));
                bwTPL.Write(table10[i].TPL.Arr);
                bwTPL.Close();
            }

            //EFFBLOB
            IsVersion effBlobVersion = version == IsVersion.IsPS2 ? IsVersion.IsUHD : version;
            var effBlob = new FileInfo(Path.Combine(baseDirectoryPath, $"{baseFileName}.{effBlobFormat}")).Create();
            Join join = new Join(tables);
            join.Create_EFF_File(effBlob, effBlobVersion);
            effBlob.Close();
        }

    }
}
