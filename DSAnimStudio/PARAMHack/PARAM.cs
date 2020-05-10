using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A general-purpose configuration file used throughout the series.
    /// </summary>
    public partial class PARAM_Hack : SoulsFile<PARAM_Hack>
    {
        /// <summary>
        /// Whether the file is big-endian; true for PS3/360 files, false otherwise.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Flags indicating format of the file.
        /// </summary>
        public FormatFlags1 Format2D { get; set; }

        /// <summary>
        /// More flags indicating format of the file.
        /// </summary>
        public FormatFlags2 Format2E { get; set; }

        /// <summary>
        /// Originally matched the paramdef for version 101, but since is always 0 or 0xFF.
        /// </summary>
        public byte ParamdefFormatVersion { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public short Unk06 { get; set; }

        /// <summary>
        /// Indicates a revision of the row data structure.
        /// </summary>
        public short ParamdefDataVersion { get; set; }

        /// <summary>
        /// Identifies corresponding params and paramdefs.
        /// </summary>
        public string ParamType { get; set; }

        /// <summary>
        /// Automatically determined based on spacing of row offsets; -1 if param had no rows.
        /// </summary>
        public long DetectedSize { get; private set; }

        /// <summary>
        /// The rows of this param; must be loaded with PARAM_Hack.ApplyParamdef() before cells can be used.
        /// </summary>
        public List<Row> Rows { get; set; }

        /// <summary>
        /// The current applied PARAMDEF.
        /// </summary>
        public PARAMDEF AppliedParamdef { get; private set; }

        /// <summary>
        /// Returns a reader for the row passed for custom row reading logic
        /// </summary>
        public BinaryReaderEx GetRowReader(Row row)
        {
            RowReader.Position = row.DataOffset;

            return RowReader;
        }
		
        private BinaryReaderEx RowReader;

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.Position = 0x2C;
            br.BigEndian = BigEndian = br.AssertByte(0, 0xFF) == 0xFF;
            Format2D = (FormatFlags1)br.ReadByte();
            Format2E = (FormatFlags2)br.ReadByte();
            ParamdefFormatVersion = br.ReadByte();
            br.Position = 0;

            // Make a private copy of the file to read row data from later
            byte[] copy = br.GetBytes(0, (int)br.Stream.Length);
            RowReader = new BinaryReaderEx(BigEndian, copy);

            // The strings offset in the header is highly unreliable; only use it as a last resort
            long actualStringsOffset = 0;
            long stringsOffset = br.ReadUInt32();
            if (Format2D.HasFlag(FormatFlags1.Flag01) && Format2D.HasFlag(FormatFlags1.IntDataOffset) || Format2D.HasFlag(FormatFlags1.LongDataOffset))
            {
                br.AssertInt16(0);
            }
            else
            {
                br.ReadUInt16(); // Data start
            }
            Unk06 = br.ReadInt16();
            ParamdefDataVersion = br.ReadInt16();
            ushort rowCount = br.ReadUInt16();
            if (Format2D.HasFlag(FormatFlags1.OffsetParamType))
            {
                br.AssertInt32(0);
                long paramTypeOffset = br.ReadInt64();
                br.AssertPattern(0x14, 0x00);
                ParamType = br.GetASCII(paramTypeOffset);
                actualStringsOffset = paramTypeOffset;
            }
            else
            {
                ParamType = br.ReadFixStr(0x20);
            }
            br.Skip(4); // Format
            if (Format2D.HasFlag(FormatFlags1.Flag01) && Format2D.HasFlag(FormatFlags1.IntDataOffset))
            {
                br.ReadInt32(); // Data start
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
            }
            else if (Format2D.HasFlag(FormatFlags1.LongDataOffset))
            {
                br.ReadInt64(); // Data start
                br.AssertInt64(0);
            }

            Rows = new List<Row>(rowCount);
            for (int i = 0; i < rowCount; i++)
                Rows.Add(new Row(br, this, ref actualStringsOffset));

            if (Rows.Count > 1)
                DetectedSize = Rows[1].DataOffset - Rows[0].DataOffset;
            else if (Rows.Count == 1)
                DetectedSize = (actualStringsOffset == 0 ? stringsOffset : actualStringsOffset) - Rows[0].DataOffset;
            else
                DetectedSize = -1;
        }

        /// <summary>
        /// Returns the first row with the given ID, or null if not found.
        /// </summary>
        public Row this[int id] => Rows.Find(row => row.ID == id);

        /// <summary>
        /// First set of flags indicating file format; highly speculative.
        /// </summary>
        [Flags]
        public enum FormatFlags1 : byte
        {
            /// <summary>
            /// No flags set.
            /// </summary>
            None = 0,

            /// <summary>
            /// Unknown.
            /// </summary>
            Flag01 = 0b0000_0001,

            /// <summary>
            /// Expanded header with 32-bit data offset.
            /// </summary>
            IntDataOffset = 0b0000_0010,

            /// <summary>
            /// Expanded header with 64-bit data offset.
            /// </summary>
            LongDataOffset = 0b0000_0100,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag08 = 0b0000_1000,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag10 = 0b0001_0000,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag20 = 0b0010_0000,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag40 = 0b0100_0000,

            /// <summary>
            /// Param type string is written separately instead of fixed-width in the header.
            /// </summary>
            OffsetParamType = 0b1000_0000,
        }

        /// <summary>
        /// Second set of flags indicating file format; highly speculative.
        /// </summary>
        [Flags]
        public enum FormatFlags2 : byte
        {
            /// <summary>
            /// No flags set.
            /// </summary>
            None = 0,

            /// <summary>
            /// Row names are written as UTF-16.
            /// </summary>
            UnicodeRowNames = 0b0000_0001,

            /// <summary>
            /// Unknown.
            /// </summary>
            Flag02 = 0b0000_0010,

            /// <summary>
            /// Unknown.
            /// </summary>
            Flag04 = 0b0000_0100,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag08 = 0b0000_1000,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag10 = 0b0001_0000,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag20 = 0b0010_0000,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag40 = 0b0100_0000,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag80 = 0b1000_0000,
        }
    }
}
