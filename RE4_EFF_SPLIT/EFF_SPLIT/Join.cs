using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace EFF_SPLIT
{
    internal class Join
    {
        public delegate void CustomTable(BinaryWriter bw, bool IsUHD);

        public CustomTable WriteTable05;
        public CustomTable WriteTable10;

        private TablesGroup tables = null;

        public Join(TablesGroup tables) 
        { 
            this.tables = tables;
            WriteTable05 = CustomTableEmpty;
            WriteTable10 = CustomTableEmpty;
        }

        public void Create_EFF_File(Stream stream, bool IsUHD) 
        {
            var bw = new BinaryWriter(stream);
            bw.Write((uint)0x0B);
            bw.Write(new byte[0x2C]);
            if (IsUHD)
            {
                bw.Write(new byte[0x10]);
            }
            uint offsetTable00 = (uint)bw.BaseStream.Position;
            WriteTableIndex(bw, tables.Table00, IsUHD);

            uint offsetTable01 = (uint)bw.BaseStream.Position;
            WriteTableIndex(bw, tables.Table01, IsUHD);

            uint offsetTable02 = (uint)bw.BaseStream.Position;
            WriteTableIndex(bw, tables.Table02, IsUHD);

            uint offsetTable03 = (uint)bw.BaseStream.Position;
            WriteTableIndex(bw, tables.Table03, IsUHD);

            uint offsetTable04 = (uint)bw.BaseStream.Position;
            WriteTableIndex(bw, tables.Table04, IsUHD);

            uint offsetTable05 = (uint)bw.BaseStream.Position;
            WriteTable05(bw, IsUHD);

            uint offsetTable06 = (uint)bw.BaseStream.Position;
            WriteTable06(bw, tables.Table06, IsUHD);

            uint offsetTable07 = (uint)bw.BaseStream.Position;
            Write_Effect_Type(bw, tables.Table07_Effect_0_Type, IsUHD);

            uint offsetTable08 = (uint)bw.BaseStream.Position;
            Write_Effect_Type(bw, tables.Table08_Effect_1_Type, IsUHD);

            uint offsetTable09 = (uint)bw.BaseStream.Position;
            WriteTable09(bw, tables.Table09, IsUHD);

            uint offsetTable10 = (uint)bw.BaseStream.Position;
            WriteTable10(bw, IsUHD);

            bw.BaseStream.Position = 4;
            bw.Write(offsetTable00);
            bw.Write(offsetTable01);
            bw.Write(offsetTable02);
            bw.Write(offsetTable03);
            bw.Write(offsetTable04);
            bw.Write(offsetTable05);
            bw.Write(offsetTable06);
            bw.Write(offsetTable07);
            bw.Write(offsetTable08);
            bw.Write(offsetTable09);
            bw.Write(offsetTable10);
        }

        private void WriteTableIndex(BinaryWriter bw, TableIndex Table, bool IsUHD) 
        {
            if (Table != null && Table.Entries.Length != 0)
            {
                bw.Write((uint)Table.Entries.Length);
                for (int i = 0; i < Table.Entries.Length; i++)
                {
                    bw.Write(Table.Entries[i].Value);
                }
                AddPadding(bw, IsUHD);
            }
            else if (IsUHD)
            {
                bw.Write(new byte[0x20]);
            }
            else 
            {
                bw.Write(new byte[0x10]);
            }
        }

        private void WriteTable06(BinaryWriter bw, TableIndex Table06, bool IsUHD) 
        {
            if (Table06 != null && Table06.Entries.Length != 0)
            {
                uint entryByteLength = IsUHD ? 32u : 16u;

                uint Length = (uint)Table06.Entries.Length;
                uint calc = 4 + (Length * 4);
                uint _line = calc / 16;
                uint rest = calc % 16;
                _line += rest != 0 ? 1u : 0u;
                calc = _line * 16;
                calc += Length * entryByteLength;
                byte[] res = new byte[calc];
                BinaryWriter ms = new BinaryWriter(new MemoryStream(res));

                ms.Write(Length);
                uint offsetToOffset = 4;
                uint offset = _line * 16;

                for (int i = 0; i < Table06.Entries.Length; i++)
                {
                    ms.BaseStream.Position = offsetToOffset;
                    ms.Write(offset);
                    ms.BaseStream.Position = offset;
                    if (IsUHD)
                    {
                        ms.Write(Table06.Entries[i].Value);
                    }
                    else
                    {
                        ms.Write(Table06.Entries[i].Value.Take(16).ToArray());
                    }
                    offsetToOffset += 4;
                    offset = (uint)ms.BaseStream.Position;
                }

                ms.Close();
                bw.Write(res);
            }
            else if (IsUHD)
            {
                bw.Write(new byte[0x20]);
            }
            else
            {
                bw.Write(new byte[0x10]);
            }
        }

        private void WriteTable09(BinaryWriter bw, Table09 table09, bool IsUHD) 
        {
            if (table09 != null && table09.Entries.Length != 0)
            {
                uint Length = (uint)table09.Entries.Length;
                uint calc = 4 + (Length * 4);
                uint _line = calc / 16;
                uint rest = calc % 16;
                _line += rest != 0 ? 1u : 0u;
                calc = _line * 16;

                for (int i = 0; i < Length; i++)
                {
                    ushort Length2 = (ushort)table09.Entries[i].Entries.Length;
                    calc += 4u + (Length2 * 40u);
                    uint _line2 = calc / 16;
                    uint rest2 = calc % 16;
                    _line2 += rest2 != 0 ? 1u : 0u;
                    calc = _line2 * 16;
                }
               
                byte[] res = new byte[calc];
                BinaryWriter ms = new BinaryWriter(new MemoryStream(res));

                ms.Write(Length);
                uint offsetToOffset = 4;
                uint offset = _line * 16;

                for (int i = 0; i < table09.Entries.Length; i++)
                {
                    ushort Length2 = (ushort)table09.Entries[i].Entries.Length;

                    ms.BaseStream.Position = offsetToOffset;
                    ms.Write(offset);
                    ms.BaseStream.Position = offset;

                    ms.Write(Length2);
                    ms.Write((ushort)0);

                    for (int j = 0; j < Length2; j++)
                    {
                        ms.Write(table09.Entries[i].Entries[j].Value);
                    }

                    long current = ms.BaseStream.Position;
                    long line2 = current / 16;
                    long rest2 = current % 16;
                    line2 += rest2 != 0 ? 1 : 0;
                    long total = line2 * 16;
                    long dif = total - current;
                    ms.Write(new byte[dif]);

                    offsetToOffset += 4;
                    offset = (uint)ms.BaseStream.Position;
                }

                ms.Close();
                bw.Write(res);

            }
            else if (IsUHD)
            {
                bw.Write(new byte[0x20]);
            }
            else
            {
                bw.Write(new byte[0x10]);
            }
        }

        private void Write_Effect_Type(BinaryWriter bw, TableEffectType table, bool IsUHD) 
        {
            if (table != null && table.Groups.Length != 0) 
            {
                uint Length = (uint)table.Groups.Length;
                uint calc = 4 + (Length * 4);
                uint _line = calc / 16;
                uint rest = calc % 16;
                _line += rest != 0 ? 1u : 0u;
                calc = _line * 16;

                for (int i = 0; i < Length; i++)
                {
                    ushort Length2 = (ushort)table.Groups[i].Entries.Length;
                    calc += 48u + (Length2 * 300u);
                    uint _line2 = calc / 16;
                    uint rest2 = calc % 16;
                    _line2 += rest2 != 0 ? 1u : 0u;
                    calc = _line2 * 16;
                }

                byte[] res = new byte[calc];
                BinaryWriter ms = new BinaryWriter(new MemoryStream(res));

                ms.Write(Length);
                uint offsetToOffset = 4;
                uint offset = _line * 16;

                for (int i = 0; i < table.Groups.Length; i++)
                {
                    ms.BaseStream.Position = offsetToOffset;
                    ms.Write(offset);
                    ms.BaseStream.Position = offset;

                    ms.Write(table.Groups[i].Header);

                    for (int j = 0; j < table.Groups[i].Entries.Length; j++)
                    {
                        ms.Write(table.Groups[i].Entries[j].Value);
                    }

                    long current = ms.BaseStream.Position;
                    long line2 = current / 16;
                    long rest2 = current % 16;
                    line2 += rest2 != 0 ? 1 : 0;
                    long total = line2 * 16;
                    long dif = total - current;
                    ms.Write(new byte[dif]);

                    offsetToOffset += 4;
                    offset = (uint)ms.BaseStream.Position;
                }

                ms.Close();
                bw.Write(res);

            }
            else if (IsUHD)
            {
                bw.Write(new byte[0x20]);
            }
            else
            {
                bw.Write(new byte[0x10]);
            }
        }


        private void AddPadding(BinaryWriter bw, bool IsUHD) 
        {
            long _base = IsUHD == true ? 32 : 16;

            long current = bw.BaseStream.Position;
            long lines = current / _base;
            long rest = current % _base;
            lines += rest != 0 ? 1 : 0;
            long total = lines * _base;
            long dif = total - current;
            bw.Write(new byte[dif]);
        }

        private void CustomTableEmpty(BinaryWriter bw, bool IsUHD) 
        {
            if (IsUHD)
            {
                bw.Write(new byte[0x20]);
            }
            else
            {
                bw.Write(new byte[0x10]);
            }
        }
    }
}
