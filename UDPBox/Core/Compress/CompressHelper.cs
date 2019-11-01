using UnityEngine;
using System.Collections;
using System.IO;
using System.IO.Compression;
using Ionic.Zlib;

namespace Hont.UDPBoxPackage
{
    public static class CompressHelper
    {
        public const int ZIP_BUFFER_SIZE = 4096;


        public static byte[] ZipBytesCompress(byte[] source)
        {
            var inStream = new MemoryStream(source);
            var outStream = new MemoryStream();

            ZipStreamCompress(inStream, outStream);

            var result = outStream.ToArray();

            inStream.Close();
            inStream.Dispose();

            outStream.Close();
            outStream.Dispose();

            return result;
        }

        public static byte[] ZipBytesDecompress(byte[] source)
        {
            var inStream = new MemoryStream(source);
            var outStream = new MemoryStream();

            ZipStreamDecompress(inStream, outStream);

            inStream.Close();
            inStream.Dispose();

            var result = outStream.ToArray();
            outStream.Close();
            outStream.Dispose();

            return result;
        }

        public static void ZipStreamCompress(Stream source, Stream dest)
        {
            using (var stream = new Ionic.Zlib.GZipStream(
            dest,
            Ionic.Zlib.CompressionMode.Compress,
            Ionic.Zlib.CompressionLevel.BestCompression,
            true))
            {
                byte[] buf = new byte[ZIP_BUFFER_SIZE];
                int len;
                while ((len = source.Read(buf, 0, buf.Length)) > 0)
                {
                    stream.Write(buf, 0, len);
                }
            }
        }

        public static void ZipStreamDecompress(Stream source, Stream dest)
        {
            using (var stream = new Ionic.Zlib.GZipStream(source, Ionic.Zlib.CompressionMode.Decompress, true))
            {
                var buf = new byte[ZIP_BUFFER_SIZE];
                int len;
                while ((len = stream.Read(buf, 0, buf.Length)) > 0)
                {
                    dest.Write(buf, 0, len);
                }
            }
        }
    }
}
