/*
 * (c) 2008 The Managed.X86 Project
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Alex Lyman (<mailto:mail.alex.lyman@gmail.com>)
 */

using System;
using System.Collections.Generic;

namespace Managed.X86
{
    public class X86Label {
		readonly X86Writer writer;
		IntPtr? mark;

		List<IntPtr> patches = new List<IntPtr>();

		internal X86Label(X86Writer writer) {
			this.writer = writer;
		}
		internal X86Label(X86Writer writer, IntPtr position)
			: this(writer) {
			mark = position;
		}

		public void Mark() {
			if (!this.mark.HasValue) {
				IntPtr currentPosition;
				this.mark = currentPosition = this.writer.Position;
				if (patches.Count > 0) {
					foreach (IntPtr patch in patches) {
						writer.Position = patch;
						writer.Patch(currentPosition);
					}
					writer.Position = currentPosition;
				}
			}
		}

		public bool IsMarked { get { return this.mark.HasValue; } }

		public IntPtr Position {
			get {
				if (!mark.HasValue) { throw new NotSupportedException(); }
				return mark.Value;
			}
		}

		internal void AddPatchRequired() {
			patches.Add(writer.Position);
		}

		public override string ToString() {
			if (this.mark.HasValue) {
				return String.Format("label:0x{0:X8}", this.mark.Value.ToInt32());
			} else 
			{
				return String.Format("label:0x????????");
			}
		}
	}
}
