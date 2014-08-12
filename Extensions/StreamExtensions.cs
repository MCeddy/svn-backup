using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnBackup.Extensions
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Quelle: http://dotnet-snippets.de/dns/stream-in-datei-umleiten-SID718.aspx
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="outputFile"></param>
        /// <param name="fileMode"></param>
        public static void ToFile(this Stream inputStream, string outputFile, int bufferLength = 4096, FileMode fileMode = FileMode.CreateNew)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException("inputStream");
            }

            if (String.IsNullOrEmpty(outputFile))
            {
                throw new ArgumentException("Argument null or empty.", "outputFile");
            }

            using (FileStream outputStream = new FileStream(outputFile, fileMode, FileAccess.Write))
            {
                int cnt = 0;
                byte[] buffer = new byte[bufferLength];

                while ((cnt = inputStream.Read(buffer, 0, bufferLength)) != 0)
                {
                    outputStream.Write(buffer, 0, cnt);
                }
            }
        }
    }
}
