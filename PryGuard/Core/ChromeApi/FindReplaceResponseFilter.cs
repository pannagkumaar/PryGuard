using System;
using CefSharp;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace PryGuard.Core.ChromeApi;

/// <summary>
/// Finds a string in the response and replaces it with another string
/// </summary>
public class FindReplaceResponseFilter : IResponseFilter, IDisposable
{
    private static readonly Encoding encoding = Encoding.UTF8;
    private readonly List<byte> overflow = new List<byte>();
    private readonly string findString;
    private readonly string replacementString;
    private int findMatchOffset;

    public FindReplaceResponseFilter(string find, string replacement)
    {
        findString = find;
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
        if (overflow.Count > 0)
            WriteOverflow(dataOut, ref dataOutWritten);
        for (int index = 0; (long)index < dataInRead; ++index)
        {
            byte data = (byte)dataIn.ReadByte();
            if (Convert.ToChar(data) == findString[findMatchOffset])
            {
                ++findMatchOffset;
                if (findMatchOffset == findString.Length)
                {
                    WriteString(replacementString, replacementString.Length, dataOut, ref dataOutWritten);
                    findMatchOffset = 0;
                }
            }
            else
            {
                if (findMatchOffset > 0)
                {
                    WriteString(findString, findMatchOffset, dataOut, ref dataOutWritten);
                    findMatchOffset = 0;
                }
                WriteSingleByte(data, dataOut, ref dataOutWritten);
            }
        }
        return overflow.Count > 0 || findMatchOffset > 0 ? FilterStatus.NeedMoreData : FilterStatus.Done;
    }

    private void WriteOverflow(Stream dataOut, ref long dataOutWritten)
    {
        long num = Math.Min(overflow.Count, dataOut.Length - dataOutWritten);
        if (num > 0L)
        {
            dataOut.Write(overflow.ToArray(), 0, (int)num);
            dataOutWritten += num;
        }
        if (num < overflow.Count)
            overflow.RemoveRange(0, (int)(num - 1L));
        else
            overflow.Clear();
    }

    private void WriteString(string str, int stringSize, Stream dataOut, ref long dataOutWritten)
    {
        long val2 = dataOut.Length - dataOutWritten;
        long num = Math.Min(stringSize, val2);
        if (num > 0L)
        {
            byte[] bytes = encoding.GetBytes(str);
            dataOut.Write(bytes, 0, (int)num);
            dataOutWritten += num;
        }
        if (num >= stringSize)
            return;
        overflow.AddRange(encoding.GetBytes(str.Substring((int)num, (int)(stringSize - num))));
    }

    private void WriteSingleByte(byte data, Stream dataOut, ref long dataOutWritten)
    {
        if (dataOut.Length - dataOutWritten > 0L)
        {
            dataOut.WriteByte(data);
            ++dataOutWritten;
        }
        else
            overflow.Add(data);
    }

    public void Dispose()
    {
    }
}