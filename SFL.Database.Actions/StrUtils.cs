using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace SFL.Database.Sql
{
    public class StrUtils
    {
        private StrUtils()
        {
            
        }

        public static byte[] Zip(string value)
        {
            var encoding = Encoding.UTF8;
            byte[] byteArray = encoding.GetBytes(value);

            MemoryStream ms = new MemoryStream();
            GZipStream zipStream = new GZipStream(ms, CompressionMode.Compress);

            // Compress
            zipStream.Write(byteArray, 0, byteArray.Length);
            zipStream.Close();

            return ms.ToArray();
        }

        public static string UnZip(byte[] value)
        {
            var encoding = Encoding.UTF8;
            // Decompress
            MemoryStream ms = new MemoryStream(value);
            GZipStream zipStream = new GZipStream(ms, CompressionMode.Decompress);
            StreamReader reader = new StreamReader(zipStream, encoding);

            string builder = reader.ReadToEnd();

            ms.Close();
            zipStream.Close();
            ms.Dispose();
            zipStream.Dispose();
            reader.Close();
            reader.Dispose();

            return builder;
        }
    }
}
