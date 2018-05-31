using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;


namespace PROCAS2.Services.Utility
{
    // Decompressor class provided by Volpara
    public class GzipCompressor
    {


        public static byte[] Compress(string inString)
        {
            using (var myStringStream = new MemoryStream(Encoding.UTF8.GetBytes(inString)))
            {
                using (var myMs = new MemoryStream())
                {
                    using (var myZs = new GZipStream(myMs, CompressionMode.Compress))
                    {
                        myStringStream.CopyTo(myZs);
                    }
                    return myMs.ToArray();
                }
            }
        }

        public static async Task<byte[]> CompressAsync(string inString)
        {
            using (var myStringStream = new MemoryStream(Encoding.UTF8.GetBytes(inString)))
            {
                using (var myMs = new MemoryStream())
                {
                    using (var myZs = new GZipStream(myMs, CompressionMode.Compress))
                    {
                        await myStringStream.CopyToAsync(myZs);
                    }
                    return myMs.ToArray();
                }
            }
        }

        public static string Decompress(byte[] inBytes)
        {
            using (var myMs = new MemoryStream(inBytes))
            {
                using (var myStringStream = new MemoryStream())
                {
                    using (var myZs = new GZipStream(myMs, CompressionMode.Decompress))
                    {
                        myZs.CopyTo(myStringStream);
                    }
                    return Encoding.UTF8.GetString(myStringStream.ToArray());
                }
            }
        }

        public static async Task<string> DecompressAsync(byte[] inBytes)
        {
            using (var myMs = new MemoryStream(inBytes))
            {
                using (var myStringStream = new MemoryStream())
                {
                    using (var myZs = new GZipStream(myMs, CompressionMode.Decompress))
                    {
                        await myZs.CopyToAsync(myStringStream);
                    }
                    return Encoding.UTF8.GetString(myStringStream.ToArray());
                }
            }
        }
    }

}
