﻿using PuyoTools.Modules.Compression;
using System;
using System.Diagnostics;
using System.IO;

namespace PuyoTools.Modules.Archive
{
    public class OneStorybookArchive : ArchiveBase
    {
        /// <summary>
        /// Name of the format.
        /// </summary>
        public override string Name
        {
            get { return "ONE (Sonic and the Secret Rings)"; }
        }

        /// <summary>
        /// The primary file extension for this archive format.
        /// </summary>
        public override string FileExtension
        {
            get { return ".one"; }
        }

        /// <summary>
        /// Returns if data can be written to this format.
        /// </summary>
        public override bool CanWrite
        {
            get { return true; }
        }

        public override ArchiveReader Open(Stream source)
        {
            return new OneStorybookArchiveReader(source);
        }

        public override ArchiveWriter Create(Stream destination)
        {
            return new OneStorybookArchiveWriter(destination);
        }

        public override bool Is(Stream source, int length, string fname)
        {
            if (PTStream.ReadUInt32BEAt(source, 0x4) != 0x10)
                return false;

            var i = PTStream.ReadUInt32BEAt(source, 0xC);

            if (i != 0xFFFFFFFF && i != 0x00000000)
                return false;

            source.Seek(0, SeekOrigin.Begin);
            return true;
        }
    }

    #region Archive Reader
    public class OneStorybookArchiveReader : ArchiveReader
    {
        private PrsCompression _prsCompression;

        public OneStorybookArchiveReader(Stream source) : base(source)
        {
            _prsCompression = new PrsCompression();

            // Get the number of entries in the archive
            int numEntries = PTStream.ReadInt32BE(source);
            entries = new ArchiveEntryCollection(this, numEntries);

            // Read in all the entries
            for (int i = 0; i < numEntries; i++)
            {
                string entryFilename = PTStream.ReadCStringAt(source, 0x10 + (i * 0x30), 0x20);

                source.Position = 0x34 + (i * 0x30);
                int entryOffset = PTStream.ReadInt32BE(source);
                int entryLength = PTStream.ReadInt32BE(source);

                // Add this entry to the collection
                entries.Add(startOffset + entryOffset, entryLength, entryFilename);
            }

            // Set the position of the stream to the end of the file
            source.Seek(0, SeekOrigin.End);
        }

        public override Stream OpenEntry(ArchiveEntry entry)
        {
            archiveData.Seek(entry.Offset, SeekOrigin.Begin);
            var memoryStream = new MemoryStream();
            _prsCompression.Decompress(archiveData, memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }
    }
    #endregion

    public class OneStorybookArchiveWriter : ArchiveWriter
    {
        private PrsCompression _prsCompression;

        public OneStorybookArchiveWriter(Stream dest) : base(dest)
        {
            _prsCompression = new PrsCompression();
        }

        public override void Flush()
        {
            const int tableStartPtr = 0x10;
            int dataStartPtr = PTMethods.RoundUp(tableStartPtr + (entries.Count * 0x30), 0x10); // just to be safe

            PTStream.WriteInt32BE(destination, entries.Count);
            PTStream.WriteInt32BE(destination, tableStartPtr); // pointer to table start
            PTStream.WriteInt32BE(destination, dataStartPtr); // pointer to data start
            PTStream.WriteInt32BE(destination, 0); // not 100% sure on this one

            int offset = dataStartPtr;

            using (var compressedStream = new MemoryStream())
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    var entryStream = entry.Open();
                    _prsCompression.Compress(entryStream, compressedStream);

                    compressedStream.Position = 0;

                    PTStream.WriteCString(destination, entry.Name, 0x20); // name
                    PTStream.WriteInt32BE(destination, i); // index
                    PTStream.WriteInt32BE(destination, offset); // offset
                    PTStream.WriteInt32BE(destination, (int)compressedStream.Length); // compressed length
                    PTStream.WriteInt32BE(destination, (int)entryStream.Length); // uncompressed length

                    Debug.Assert(destination.Position <= dataStartPtr, "Table overrun!");

                    var currentPos = destination.Position;
                    destination.Position = offset;
                    PTStream.CopyTo(compressedStream, destination);
                    destination.Position = currentPos;

                    offset += (int)compressedStream.Length;
                    compressedStream.Position = 0;
                    compressedStream.SetLength(0);

                    OnFileAdded(EventArgs.Empty);
                }
            }
        }
    }
}