using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace EFF_SPLIT
{
    internal static class UhdExtract
    {
        public static FileContent[] ExtractTable05_TPL(BinaryReader br, long StartOffset, bool IsPS4NS)
        {
            if (StartOffset != 0)
            {
                br.BaseStream.Position = StartOffset;
                uint Amount1 = br.ReadUInt32();

                uint[] offsetArray = new uint[Amount1];

                for (int i = 0; i < Amount1; i++)
                {
                    offsetArray[i] = br.ReadUInt32();
                }
                FileContent[] files = new FileContent[Amount1];

                for (int i = 0; i < Amount1; i++)
                {
                    long start = offsetArray[i] + StartOffset;
                    long end = start;
                    UhdTPLDecoder(br.BaseStream, start, out end, IsPS4NS);
                    int length = (int)(end - start);

                    br.BaseStream.Position = start;
                    FileContent content = new FileContent();
                    content.Arr = br.ReadBytes(length);
                    files[i] = content;
                }
                return files;
            }
            else
            {
                return new FileContent[0];
            }
        }

        public static ModelFileContent[] ExtractTable10_MODEL(BinaryReader br, long StartOffset, bool IsPS4NS)
        {
            if (StartOffset != 0)
            {
                br.BaseStream.Position = StartOffset;
                uint Amount1 = br.ReadUInt32();

                uint[] offsetArray = new uint[Amount1];

                for (int i = 0; i < Amount1; i++)
                {
                    offsetArray[i] = br.ReadUInt32();
                }
                ModelFileContent[] files = new ModelFileContent[Amount1];

                for (int i = 0; i < Amount1; i++)
                {
                    files[i] = new ModelFileContent();

                    long start = offsetArray[i] + StartOffset;
                    br.BaseStream.Position = start;

                    uint Fixed4 = br.ReadUInt32(); // sempre 0x04
                    uint BIN_OFFSET = br.ReadUInt32();
                    uint TPL_OFFSET = br.ReadUInt32();

                    //bin
                    long binStart = BIN_OFFSET + start;
                    long binEnd = binStart;
                    UhdBINDecoder(br.BaseStream, binStart, out binEnd, IsPS4NS);
                    int binLen = (int)(binEnd - binStart);

                    br.BaseStream.Position = binStart;
                    FileContent BINcontent = new FileContent();
                    BINcontent.Arr = br.ReadBytes(binLen);
                    files[i].BIN = BINcontent;

                    //tpl
                    long tplStart = TPL_OFFSET + start;
                    long tplEnd = tplStart;

                    UhdTPLDecoder(br.BaseStream, tplStart, out tplEnd, IsPS4NS);

                    int tplLen = (int)(tplEnd - tplStart);

                    br.BaseStream.Position = tplStart;
                    FileContent TPLcontent = new FileContent();
                    TPLcontent.Arr = br.ReadBytes(tplLen);
                    files[i].TPL = TPLcontent;
                }
                return files;
            }
            else
            {
                return new ModelFileContent[0];
            }
        }

        private static void UhdTPLDecoder(Stream stream, long startOffset, out long endOffset, bool IsPs4NS)
        {
            BinaryReader br = new BinaryReader(stream);
            br.BaseStream.Position = startOffset;

            uint magic = br.ReadUInt32();
            if (! (magic == 0x78563412 || magic == 0x12345678) )
            {
                throw new ArgumentException("Invalid TPL file!");
            }
            uint TplAmount = br.ReadUInt32();
            uint StartOffset = br.ReadUInt32();

            br.BaseStream.Position = StartOffset + startOffset;

            uint[] offsets = new uint[TplAmount];

            for (int i = 0; i < TplAmount; i++)
            {
                uint image_data_offset = br.ReadUInt32();
                if (IsPs4NS)
                {
                    _ = br.ReadUInt32(); // image_data_offset part2
                }

                uint palette_offset = br.ReadUInt32(); // não usado
                if (IsPs4NS)
                {
                    _ = br.ReadUInt32(); // palette_offset part2
                }

                offsets[i] = image_data_offset;
            }

            for (int i = 0; i < TplAmount; i++)
            {
                br.BaseStream.Position = offsets[i] + startOffset;

                ushort width = br.ReadUInt16();
                ushort height = br.ReadUInt16();
                uint PixelFormatType = br.ReadUInt32();
                uint secundOffset = br.ReadUInt32();
                // proximos campos omitidos

                br.BaseStream.Position = secundOffset + startOffset;
                uint PackID = br.ReadUInt32();
                uint TextureID = br.ReadUInt32();
            }

            endOffset = br.BaseStream.Position;
        }

        private static void UhdBINDecoder(Stream stream, long startOffset, out long endOffset, bool IsPs4NS)
        {
            BinaryReader br = new BinaryReader(stream);
            br.BaseStream.Position = startOffset;

            var (material_count, material_offset) = IsPs4NS ? GetHeaderPS4NS(br) : GetHeaderUHD(br);

            Materials(br, material_offset + startOffset, material_count);
            endOffset = br.BaseStream.Position;
        }

        private static (ushort material_count, uint material_offset) GetHeaderUHD(BinaryReader br) 
        {
            uint bone_offset = br.ReadUInt32(); //--headersize // 60 00 00 00
            if (!(bone_offset == 0x00000060 || bone_offset == 0x00000040 || bone_offset == 0x00000050))
            {
                throw new ArgumentException("Invalid BIN file!");
            }
            br.ReadBytes(22); // pula campos

            ushort material_count = br.ReadUInt16();
            uint material_offset = br.ReadUInt32();

            return (material_count, material_offset);
        }

        private static (ushort material_count, uint material_offset) GetHeaderPS4NS(BinaryReader br)
        {
            uint bone_offset = br.ReadUInt32(); //--headersize // 98 00 00 00
            if (! (bone_offset == 0x00000098))
            {
                throw new ArgumentException("Invalid BIN file!");
            }
            br.ReadBytes(38); // pula campos

            ushort material_count = br.ReadUInt16();
            _ = br.ReadUInt32(); // nada padding
            uint material_offset = br.ReadUInt32();

            return (material_count, material_offset);
        }

        private static void Materials(BinaryReader br, long offset, ushort MatCount)
        {
            br.BaseStream.Position = offset;

            for (int i = 0; i < MatCount; i++)
            {
                Get_Material(br);
            }
        }

        private static void Get_Material(BinaryReader br)
        {
            br.ReadBytes(24); // material
            Get_face_index(br);
        }

        private static void Get_face_index(BinaryReader br)
        {
            uint buffer_size = br.ReadUInt32();
            uint count = br.ReadUInt32(); //unused

            long tempOffset = br.BaseStream.Position;
            uint strip_count = br.ReadUInt32();

            br.BaseStream.Position = tempOffset + buffer_size;
        }
    }
}
