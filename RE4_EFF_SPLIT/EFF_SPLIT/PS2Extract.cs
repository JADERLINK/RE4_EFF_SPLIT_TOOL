using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SimpleEndianBinaryIO;

namespace EFF_SPLIT
{
    internal static class PS2Extract
    {
        public static FileContent[] ExtractTable05_TPL(EndianBinaryReader br, long StartOffset, long EndOffset) 
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
                    br.BaseStream.Position = start;
                    long end;
                    if (i + 1 < Amount1)
                    {
                        end = offsetArray[i + 1] + StartOffset;
                    }
                    else
                    {
                        end = EndOffset;
                    }

                    int length = (int)(end - start);
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

        public static ModelFileContent[] ExtractTable10_MODEL(EndianBinaryReader br, long StartOffset, long EndOffset) 
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

                    uint Fixed2 = br.ReadUInt32(); // sempre 0x02 no ps2 // em GCWII é 0x04 
                    uint BIN_OFFSET = br.ReadUInt32();
                    uint TPL_OFFSET = br.ReadUInt32();

                    //bin
                    long binStart = BIN_OFFSET + start;
                    long binEnd = TPL_OFFSET + start;
                    int binLen = (int)(binEnd - binStart);

                    br.BaseStream.Position = binStart;
                    FileContent BINcontent = new FileContent();
                    BINcontent.Arr = br.ReadBytes(binLen);
                    files[i].BIN = BINcontent;

                    //tpl
                    long tplStart = TPL_OFFSET + start;
                    long tplEnd;
                    if (i + 1 < Amount1)
                    {
                        tplEnd = offsetArray[i + 1] + StartOffset;
                    }
                    else
                    {
                        tplEnd = EndOffset;
                    }
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
    }

}
