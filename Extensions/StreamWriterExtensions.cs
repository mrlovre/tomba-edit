using System.IO;

namespace TombaEdit.Extensions
{
    public static class StreamWriterExtensions
    {
        public static void WriteAll(this Stream stream, params byte[] bytes)
        {
            var size = bytes.Length;
            stream.Write(bytes, 0, size);
        }

        public static long Tell(this Stream stream)
        {
            return stream.Seek(0, SeekOrigin.Current);
        }
    }
}
