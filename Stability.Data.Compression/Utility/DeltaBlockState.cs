﻿#region License

// Namespace : Stability.Data.Compression.Utility
// FileName  : DeltaBlockState.cs
// Created   : 2015-5-21
// Author    : Bennett R. Stabile 
// Copyright : Stability Systems LLC, 2015
// License   : GPL v3
// Website   : http://DeltaCodec.CodePlex.com

#endregion // License
using System;
using System.Collections.Generic;
using System.IO.Compression;
using Stability.Data.Compression.Finishers;

namespace Stability.Data.Compression.Utility
{
    public interface IDeltaBlockState
    {
        IFinisher Finisher { get; }
        int BlockIndex { get; }
        int ListCount { get; }
        byte[] Bytes { get; }

        DeltaFlags Flags { get; }
    }

    public class DeltaBlockState<T> : IDeltaBlockState 
        where T : struct
    {
        /// <summary>
        /// Resharper warnings can safely be ignored here.
        /// We'll get a different instance of the Finisher for each type.
        /// 
        /// Resharper disable StaticFieldInGenericType
        /// </summary>
        private static readonly IFinisher DefaultFinisher = new DeflateFinisher();
        // Resharper restore StaticFieldInGenericType

        public DeltaBlockState(IList<T> list, 
            CompressionLevel level = CompressionLevel.Fastest,
            T? granularity = default(T?), 
            Monotonicity monotonicity = Monotonicity.None, 
            IFinisher finisher = null,
            int blockIndex = 0)
        {
            List = list;
            ListCount = list.Count;
            Anchor = list[0];
            Flags = new DeltaFlags(typeof(T), level, monotonicity);
            Factor = granularity;
            Finisher = finisher ?? DefaultFinisher;
            BlockIndex = blockIndex;
        }

        public DeltaBlockState(byte[] bytes, IFinisher finisher = null, int blockIndex = 0)
        {
            Bytes = bytes;
            Flags = new DeltaFlags(typeof(T));
            Finisher = finisher ?? DefaultFinisher;
            BlockIndex = blockIndex;
        }

        /// <summary>
        /// This is used to maintain order within a Frame.
        /// </summary>
        public int BlockIndex { get; set; }

        public IFinisher Finisher { get; set; }

        /// <summary>
        /// Encoding: This is the series that will be encoded.
        /// Decoding: This is the series that has been decoded.
        /// </summary>
        public IList<T> List { get; set; }

        /// <summary>
        /// Encoding: This value isn't really needed.
        /// Decoding: This value is needed to properly reconstitute the series.
        /// This value must always be serialized with other header information.
        /// </summary>
        public int ListCount { get; set; }

        /// <summary>
        /// Encoding: Initially null, encoded bytes after processing.
        /// Decoding: The encoded bytes that will be processed.
        /// This value must always be serialized along with header information.
        /// </summary>
        public byte[] Bytes { get; set; }

        /// <summary>
        /// This is used when writing and reading header information with Streams.
        /// The reason we need it is to know how many bytes were serialized, and thus
        /// how many need to be read (before we actually have the Bytes property available).
        /// It is particularly important when working with blocks, so we know the position
        /// of each successive block.
        /// </summary>
        public int ByteCount { get; set; }

        /// <summary>
        /// Encoding: This will be set by the encoding method. Initial value doesn't matter.
        /// Decoding: Must be set before calling the decoding method.
        /// This value must always be serialized with other header information.
        /// </summary>
        public T Anchor { get; set; }

        /// <summary>
        /// Usually, this value is determined during encoding and set here.
        /// But it is also possible to pass in a factor (if known in advance),
        /// and that will then result in a substantial performance gain.
        /// If the value is initially set to default(T), the encoding method will
        /// calculate the factor internally, and save the result here.
        /// 
        /// This value must always be serialized with other header information.
        /// </summary>
        public T? Factor { get; set; }

        public DeltaFlags Flags { get; private set; }

    }
}
