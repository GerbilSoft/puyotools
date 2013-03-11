﻿using System;
using System.IO;
using System.Collections.Generic;

namespace PuyoTools2.Archive
{
    public class U8 : ArchiveBase
    {
        public override ArchiveReader Open(Stream source, int length)
        {
            return new Read(source, length);
        }

        public override ArchiveWriter Create(Stream destination, ArchiveWriterSettings settings)
        {
            return new Write(destination, settings);
        }

        public override bool Is(Stream source, int length, string fname)
        {
            return (length > 32 && PTStream.Contains(source, 0, new byte[] { (byte)'U', 0xAA, (byte)'8', (byte)'-' }));
        }

        public class Read : ArchiveReader
        {
            public Read(Stream source, int length)
            {
                // The start of the archive
                offset = source.Position;

                // Read the archive header
                source.Position += 4;
                uint rootNodeOffset = PTStream.ReadUInt32BE(source);
                uint nodesLength = PTStream.ReadUInt32BE(source);
                uint dataOffset = PTStream.ReadUInt32BE(source);

                // Go the root node
                source.Position = offset + rootNodeOffset;
                Node rootNode = new Node();
                rootNode.Type = PTStream.ReadUInt16BE(source);
                rootNode.NameOffset = PTStream.ReadUInt16BE(source);
                rootNode.DataOffset = PTStream.ReadUInt32BE(source);
                rootNode.Length = PTStream.ReadUInt32BE(source);

                Files = new ArchiveEntry[rootNode.Length - 1];
                uint stringTableOffset = rootNodeOffset + (rootNode.Length * 12);

                // rootNode.Length is essentially how many files are contained in the archive, so we'll
                // do just that
                for (int i = 0; i < rootNode.Length - 1; i++)
                {
                    // Read in this node
                    Node node = new Node();
                    node.Type = PTStream.ReadUInt16BE(source);
                    node.NameOffset = PTStream.ReadUInt16BE(source);
                    node.DataOffset = PTStream.ReadUInt32BE(source);
                    node.Length = PTStream.ReadUInt32BE(source);

                    // If this is a directory node, just continue on with reading the file.
                    // Support will be added for it later. I promose ... maybe.
                    if (node.Type == 1)
                        continue;

                    // Now, let's create the archive entry
                    ArchiveEntry entry = new ArchiveEntry();
                    entry.Stream = source;
                    entry.Offset = offset + node.DataOffset;
                    entry.Length = (int)node.Length;

                    long oldPosition = source.Position;
                    source.Position = offset + stringTableOffset + node.NameOffset;
                    entry.Filename = PTStream.ReadCString(source);
                    source.Position = oldPosition;

                    // Add this entry to the file list
                    Files[i] = entry;
                }

                // Set the position of the stream to the end of the file
                source.Position = offset + length;
            }

            private struct Node
            {
                public ushort Type;
                public ushort NameOffset;
                public uint DataOffset;
                public uint Length;
            }
        }

        public class Write : ArchiveWriter
        {
            public Write(Stream destination)
            {
                Initalize(destination, new ArchiveWriterSettings());
            }

            public Write(Stream destination, ArchiveWriterSettings settings)
            {
                Initalize(destination, settings);
            }

            public override void Flush()
            {
                throw new NotImplementedException();
            }
        }
    }
}