using System;
using System.Text;
using Riateu;
namespace Towermap;

public static class UTF8Utils 
{
    private static readonly Encoder utf8Encoder = Encoding.UTF8.GetEncoder();

    public static byte[] ToUTF8(string str) 
    {
        ReadOnlySpan<char> span = str;
        int byteCount = Encoding.UTF8.GetByteCount(span);
        Span<byte> bytes = stackalloc byte[byteCount];

        utf8Encoder.Convert(span, bytes, true, out _, out _, out bool completed);
        if (completed) 
        {
            return bytes.ToArray();
        }
        Logger.Error("Bytes incompleted!");
        return null;
    }

    public static unsafe string FromUTF8(byte* nts) 
    {
        int count = 0;
        var ptr = nts;
        while (*ptr != 0) 
        {
            ptr++;
            count++;
        }

        return Encoding.UTF8.GetString(nts, count);
    }
}