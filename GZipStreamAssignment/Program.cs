using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace zip
{
    public class Program
    {
        private static string directoryPath = @".." + Path.DirectorySeparatorChar + "..";
        private static string fileName = @"Program.cs";
        public static void Main()
        {
            Compress(new FileInfo(directoryPath + Path.DirectorySeparatorChar + fileName));
            Decompress(new FileInfo(directoryPath + Path.DirectorySeparatorChar + fileName + ".gz"));
        }

        public static void Compress(FileInfo fileToCompress)
        {
            using (FileStream originalFileStream = fileToCompress.OpenRead())
            {
                if ((File.GetAttributes(fileToCompress.FullName) &
                   FileAttributes.Hidden) != FileAttributes.Hidden & fileToCompress.Extension != ".gz")
                {
                    using (FileStream compressedFileStream = File.Create(fileToCompress.FullName + ".gz"))
                    {
                        using (GZipStream compressionStream = new GZipStream(compressedFileStream,
                           CompressionMode.Compress))
                        {
                            originalFileStream.CopyTo(compressionStream);
                        }
                    }
                    FileInfo info = new FileInfo(directoryPath + Path.DirectorySeparatorChar + fileToCompress.Name + ".gz");
                    Console.WriteLine("Compressed {0} from {1} to {2} bytes.",
                    fileToCompress.Name, fileToCompress.Length.ToString(), info.Length.ToString());
                }

            }
        }

        public static void Decompress(FileInfo fileToDecompress)
        {
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                string currentFileName = fileToDecompress.FullName;
                string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length) + ".decompressed";

                using (FileStream decompressedFileStream = File.Create(newFileName))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                        Console.WriteLine("Decompressed: {0}", fileToDecompress.Name);
                    }
                }
            }
        }
    }

    public static class BytesEx
    {
        public static void CopyTo(this Stream source, Stream target)
        {
            target.Write(source.ReadBuffers());
        }

        public static void Write(this Stream stream, IEnumerable<byte[]> data)
        {
            data.Each(buffer => stream.Write(buffer, 0, buffer.Length));
        }

        public static void Each<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }

        public static IEnumerable<byte[]> ReadBuffers(this Stream stream, int bufferSize = 0x100000)
        {
            while (true)
            {
                var buffer = stream.Read(bufferSize);
                if (buffer.Length == 0)
                    break;
                yield return buffer;
                if (buffer.Length < bufferSize)
                    break;
            }
        }

        public static byte[] Read(this Stream stream, int count)
        {
            if (count <= 0)
                return new byte[0];
            var buffer = new byte[count];
            var offset = 0;
            while (true)
            {
                var length = stream.Read(buffer, offset, count);
                if (length == count)
                    return buffer;
                if (length == 0)
                {
                    var result = new byte[offset];
                    Buffer.BlockCopy(buffer, 0, result, 0, offset);
                    return result;
                }
                offset += length;
                count -= length;
            }
        }
    }
}