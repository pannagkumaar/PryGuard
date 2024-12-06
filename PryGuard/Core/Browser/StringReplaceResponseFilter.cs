using System;
using CefSharp;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace PryGuard.Core.Browser
{
    /// <summary>
    /// Filters the response to find a specific string and replace it with another string.
    /// </summary>
    public class StringReplaceResponseFilter : IResponseFilter, IDisposable
    {
        private static readonly Encoding DefaultEncoding = Encoding.UTF8;
        private readonly List<byte> overflowBuffer = new List<byte>();
        private readonly string targetString;
        private readonly string replacementString;
        private int matchOffset;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringReplaceResponseFilter"/> class.
        /// </summary>
        /// <param name="target">The string to find in the response.</param>
        /// <param name="replacement">The string to replace the target with.</param>
        public StringReplaceResponseFilter(string target, string replacement)
        {
            targetString = target;
            replacementString = replacement;
        }

        bool IResponseFilter.InitFilter()
        {
            return true;
        }

        FilterStatus IResponseFilter.Filter(
            Stream dataIn,
            out long dataInRead,
            Stream dataOut,
            out long dataOutWritten)
        {
            dataInRead = dataIn != null ? dataIn.Length : 0L;
            dataOutWritten = 0L;

            if (overflowBuffer.Count > 0)
                WriteOverflowToStream(dataOut, ref dataOutWritten);

            for (int i = 0; i < dataInRead; ++i)
            {
                byte data = (byte)dataIn.ReadByte();
                if (Convert.ToChar(data) == targetString[matchOffset])
                {
                    ++matchOffset;
                    if (matchOffset == targetString.Length)
                    {
                        WriteStringToStream(replacementString, replacementString.Length, dataOut, ref dataOutWritten);
                        matchOffset = 0;
                    }
                }
                else
                {
                    if (matchOffset > 0)
                    {
                        WriteStringToStream(targetString, matchOffset, dataOut, ref dataOutWritten);
                        matchOffset = 0;
                    }
                    WriteSingleByteToStream(data, dataOut, ref dataOutWritten);
                }
            }

            return overflowBuffer.Count > 0 || matchOffset > 0 ? FilterStatus.NeedMoreData : FilterStatus.Done;
        }

        private void WriteOverflowToStream(Stream dataOut, ref long dataOutWritten)
        {
            long bytesToWrite = Math.Min(overflowBuffer.Count, dataOut.Length - dataOutWritten);
            if (bytesToWrite > 0L)
            {
                dataOut.Write(overflowBuffer.ToArray(), 0, (int)bytesToWrite);
                dataOutWritten += bytesToWrite;
            }

            if (bytesToWrite < overflowBuffer.Count)
                overflowBuffer.RemoveRange(0, (int)bytesToWrite);
            else
                overflowBuffer.Clear();
        }

        private void WriteStringToStream(string str, int stringSize, Stream dataOut, ref long dataOutWritten)
        {
            long availableSpace = dataOut.Length - dataOutWritten;
            long bytesToWrite = Math.Min(stringSize, availableSpace);

            if (bytesToWrite > 0L)
            {
                byte[] bytes = DefaultEncoding.GetBytes(str);
                dataOut.Write(bytes, 0, (int)bytesToWrite);
                dataOutWritten += bytesToWrite;
            }

            if (bytesToWrite < stringSize)
            {
                overflowBuffer.AddRange(DefaultEncoding.GetBytes(str.Substring((int)bytesToWrite, stringSize - (int)bytesToWrite)));
            }
        }

        private void WriteSingleByteToStream(byte data, Stream dataOut, ref long dataOutWritten)
        {
            if (dataOut.Length - dataOutWritten > 0L)
            {
                dataOut.WriteByte(data);
                ++dataOutWritten;
            }
            else
            {
                overflowBuffer.Add(data);
            }
        }

        public void Dispose()
        {
            //TODO add 
        }
    }
}
