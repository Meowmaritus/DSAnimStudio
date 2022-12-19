using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoulsFormats
{
    public partial class PARAM_Hack
    {
        /// <summary>
        /// One row in a param file.
        /// </summary>
        public class Row
        {
            /// <summary>
            /// The ID number of this row.
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// A name given to this row; no functional significance, may be null.
            /// </summary>
            public string Name { get; set; }

            internal long DataOffset;

            public byte[] GhettoDataStorage = null;

            internal Row(BinaryReaderEx br, PARAM_Hack parent, ref long actualStringsOffset)
            {
                long nameOffset;
                if (parent.Format2D.HasFlag(FormatFlags1.LongDataOffset))
                {
                    ID = br.ReadInt32();
                    br.ReadInt32(); // I would like to assert 0, but some of the generatordbglocation params in DS2S have garbage here
                    DataOffset = br.ReadInt64();
                    nameOffset = br.ReadInt64();
                }
                else
                {
                    ID = br.ReadInt32();
                    DataOffset = br.ReadUInt32();
                    nameOffset = br.ReadUInt32();
                }

                if (nameOffset != 0)
                {
                    if (actualStringsOffset == 0 || nameOffset < actualStringsOffset)
                        actualStringsOffset = nameOffset;

                    if (parent.Format2E.HasFlag(FormatFlags2.UnicodeRowNames))
                        Name = br.GetUTF16(nameOffset);
                    else
                        Name = br.GetShiftJIS(nameOffset);
                }
            }

            internal void WriteHeader(BinaryWriterEx bw, PARAM_Hack parent, int i)
            {
                if (parent.Format2D.HasFlag(FormatFlags1.LongDataOffset))
                {
                    bw.WriteInt32(ID);
                    bw.WriteInt32(0);
                    bw.ReserveInt64($"RowOffset{i}");
                    bw.ReserveInt64($"NameOffset{i}");
                }
                else
                {
                    bw.WriteInt32(ID);
                    bw.ReserveUInt32($"RowOffset{i}");
                    bw.ReserveUInt32($"NameOffset{i}");
                }
            }

            internal void WriteName(BinaryWriterEx bw, PARAM_Hack parent, int i)
            {
                long nameOffset = 0;
                if (Name != null)
                {
                    nameOffset = bw.Position;
                    if (parent.Format2E.HasFlag(FormatFlags2.UnicodeRowNames))
                        bw.WriteUTF16(Name, true);
                    else
                        bw.WriteShiftJIS(Name, true);
                }

                if (parent.Format2D.HasFlag(FormatFlags1.LongDataOffset))
                    bw.FillInt64($"NameOffset{i}", nameOffset);
                else
                    bw.FillUInt32($"NameOffset{i}", (uint)nameOffset);
            }

            /// <summary>
            /// Returns a string representation of the row.
            /// </summary>
            public override string ToString()
            {
                return $"{ID} {Name}";
            }
        }
    }
}
