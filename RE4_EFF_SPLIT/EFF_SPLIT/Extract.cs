using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace EFF_SPLIT
{
    internal static class Extract
    {
        public static void ExtractFilePS2(string file) 
        {
            ExtractFile(file, false);
            string filename = Path.GetFileNameWithoutExtension(file);
            string directory = Path.GetDirectoryName(file);
            var IDX_EFF_SPLIT = new FileInfo($"{directory}/{filename}.IDX_PS2_EFF_SPLIT").CreateText();
            IDX_EFF_SPLIT.WriteLine("# github.com/JADERLINK/RE4_EFF_SPLIT_TOOL");
            IDX_EFF_SPLIT.WriteLine("# RE4 PS2 EFF SPLIT");
            IDX_EFF_SPLIT.WriteLine("# By JADERLINK");
            IDX_EFF_SPLIT.WriteLine("# youtube.com/@JADERLINK");
            IDX_EFF_SPLIT.Close();
        }

        public static void ExtractFileUHD(string file)
        {
            ExtractFile(file, true);
            string filename = Path.GetFileNameWithoutExtension(file);
            string directory = Path.GetDirectoryName(file);
            var IDX_EFF_SPLIT = new FileInfo($"{directory}/{filename}.IDX_UHD_EFF_SPLIT").CreateText();
            IDX_EFF_SPLIT.WriteLine("# github.com/JADERLINK/RE4_EFF_SPLIT_TOOL");
            IDX_EFF_SPLIT.WriteLine("# RE4 UHD EFF SPLIT");
            IDX_EFF_SPLIT.WriteLine("# By JADERLINK");
            IDX_EFF_SPLIT.WriteLine("# youtube.com/@JADERLINK");
            IDX_EFF_SPLIT.Close();
        }

        private static void ExtractFile(string file, bool IsUHD) 
        {
            string filename = Path.GetFileNameWithoutExtension(file);
            string directory = Path.GetDirectoryName(file);

            BinaryReader br = new BinaryReader(File.OpenRead(file));
            uint Magic = br.ReadUInt32(); //sempre 0x0B
            if (Magic != 0x0B)
            {
                Console.WriteLine("Invalid file!");
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

            FileContent[] table05 = new FileContent[0];
            ModelFileContent[] table10 = new ModelFileContent[0];

            if (IsUHD == false)
            {
                //ps2
                table05 = PS2Extract.ExtractTable05_TPL(br, offset_5_TPL_Offsets, offset_6_Texture_Metadata);
                table10 = PS2Extract.ExtractTable10_MODEL(br, offset_10_Data_Offset, br.BaseStream.Length);
            }
            else
            {
                //uhd
                table05 = UhdExtract.ExtractTable05_TPL(br, offset_5_TPL_Offsets);
                table10 = UhdExtract.ExtractTable10_MODEL(br, offset_10_Data_Offset);
            }

            //grava os arquivos

            // Create folder
            Directory.CreateDirectory(directory + "/" + filename);
            Directory.CreateDirectory(directory + "/" + filename + "/Effect TPL");
            Directory.CreateDirectory(directory + "/" + filename + "/Effect Models");

            for (int i = 0; i < table05.Length; i++)
            {
                BinaryWriter bwTPL = new BinaryWriter(File.Create($"{directory}/{filename}/Effect TPL/" + i.ToString("D") + ".TPL"));
                bwTPL.Write(table05[i].Arr);
                bwTPL.Close();
            }

            for (int i = 0; i < table10.Length; i++)
            {
                BinaryWriter bwBIN = new BinaryWriter(File.Create($"{directory}/{filename}/Effect Models/" + i.ToString("D") + ".BIN"));
                bwBIN.Write(table10[i].BIN.Arr);
                bwBIN.Close();
                BinaryWriter bwTPL = new BinaryWriter(File.Create($"{directory}/{filename}/Effect Models/" + i.ToString("D") + ".TPL"));
                bwTPL.Write(table10[i].TPL.Arr);
                bwTPL.Close();
            }

            //EFFBLOB
            var effBlob = new FileInfo($"{directory}/{filename}/{filename}.EFFBLOB").Create();
            Join join = new Join(tables);
            join.Create_EFF_File(effBlob, true);
            effBlob.Close();
        }

    }
}
