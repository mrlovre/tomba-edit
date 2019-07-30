using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MiscUtil.Conversion;
using TombaEdit.Extensions;

namespace TombaEdit.Logic
{
    public static class Gam
    {
        private static readonly ASCIIEncoding AsciiEncoding = new ASCIIEncoding();
        private static readonly LittleEndianBitConverter LittleEndianBitConverter = new LittleEndianBitConverter();

        private class GamFile
        {
            public byte[] Infile;
            public byte[] Outfile;
        }

        private static IEnumerable<double> Pack(GamFile file)
        {
            var stream = new MemoryStream();

            // Write GAM header and file size.
            var header = AsciiEncoding.GetBytes("GAM\0");
            stream.WriteAll(header);
            var size = file.Infile.Length;
            stream.WriteAll(LittleEndianBitConverter.GetBytes(size));

            // Loop over file and compress it.
            var read = 0;
            var bitmask = new List<int>();
            var bitmaskPosition = -1;
            while (read < size)
            {
                yield return (float) read / size * 100;
                if (bitmask.Count == 0)
                {
                    // Create 2-byte placeholder for bitmask, remember its position.
                    bitmaskPosition = (int) stream.Tell();
                    stream.WriteAll(0, 0);
                }


                // Find best position in buffer to copy from, if such position exists.
                var (bestPosition, bestLength) = Find(file.Infile, read);

                if (bestPosition != null)
                {
                    // Position found: write position and length to output.
                    var seek = (byte) (read - bestPosition.Value);
                    stream.WriteAll(seek, bestLength);
                    read += bestLength;
                    bitmask.Add(1);
                }
                else
                {
                    // Can't copy from buffer, copy one byte from input to output.
                    stream.WriteAll(file.Infile[read]);
                    read += 1;
                    bitmask.Add(0);
                }

                if (bitmask.Count == 16 || read >= size)
                {
                    // Bitmask is full: update placeholder for bitmask.
                    bitmask.Reverse();
                    var bitmaskShort = (short) bitmask.Aggregate((b, acc) => acc * 2 + b);
                    stream.Seek(bitmaskPosition, SeekOrigin.Begin);
                    stream.WriteAll(LittleEndianBitConverter.GetBytes(bitmaskShort));
                    stream.Seek(0, SeekOrigin.End);
                    bitmask.Clear();
                }
            }

            file.Outfile = stream.ToArray();
            stream.Close();
            yield return 100.0;
        }

        private static (int?, byte) Find(IReadOnlyList<byte> infile, int read)
        {
            var i = Math.Max(0, read - 0xff);
            var size = infile.Count;
            var bestPosition = (int?) null;
            var bestLength = (byte) 1;

            while (i < read)
            {
                if (i + bestLength >= read) break;

                if (infile[i] == infile[read])
                {
                    int? position = i;
                    byte length = 1;

                    while (i + length < read && read + length < size &&
                           infile[i + length] == infile[read + length]) length++;

                    if (length > bestLength)
                    {
                        bestLength = length;
                        bestPosition = position;
                    }

                    i += length;
                }
                else
                {
                    i += 1;
                }
            }

            return (bestPosition, bestLength);
        }

        public static IEnumerable<double> Pack(string infilePath, string outfilePath)
        {
            var file = new GamFile {Infile = File.ReadAllBytes(infilePath)};
            foreach (var i in Pack(file)) yield return i;
            File.WriteAllBytes(outfilePath, file.Outfile);
        }
    }
}