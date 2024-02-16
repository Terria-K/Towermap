using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using MoonWorks;
using NFD;

namespace Towermap;

public static class FileDialog 
{
    private static readonly Encoder utf8Encoder = Encoding.UTF8.GetEncoder();

    private static byte[] ToUTF8(string str) 
    {
        ReadOnlySpan<char> span = str;
        int byteCount = Encoding.UTF8.GetByteCount(span);
        Span<byte> bytes = stackalloc byte[byteCount];

        utf8Encoder.Convert(span, bytes, true, out _, out _, out bool completed);
        if (completed) 
        {
            return bytes.ToArray();
        }
        Logger.LogError("Bytes incompleted!");
        return null;
    }

    private static unsafe string FromUTF8(byte* nts) 
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

    public static unsafe NFDResult OpenFile(string path = null, string filters = null) 
    {
        byte[] utfPath = null;
        byte[] filterPath = null;
        if (path != null) 
        {
            utfPath = ToUTF8(path);
        }
        if (filters != null) 
        {
            filterPath = ToUTF8(filters);
        }

        string error = null;

        fixed (byte* filterPtr = filterPath) 
        fixed (byte* utfPtr = utfPath) 
        {
            nfdresult_t result = NativeFunctions.NFD_OpenDialog(filterPtr, utfPtr, out IntPtr pathPtr);
            string resultPath = null;

            if (result == nfdresult_t.NFD_OKAY) 
            {
                resultPath = FromUTF8((byte*)pathPtr);
                NativeFunctions.NFD_Free(pathPtr);
            }
            else if (result == nfdresult_t.NFD_ERROR) 
            {
                error = FromUTF8(NativeFunctions.NFD_GetError());
            }

            return new NFDResult(result, resultPath, [resultPath], error);
        }
    }

    public static unsafe NFDResult Save(string path = null, string filters = null) 
    {
        byte[] utfPath = null;
        byte[] filterPath = null;
        if (path != null) 
        {
            utfPath = ToUTF8(path);
        }
        if (filters != null) 
        {
            filterPath = ToUTF8(filters);
        }

        string error = null;

        fixed (byte* filterPtr = filterPath) 
        fixed (byte* utfPtr = utfPath) 
        {
            nfdresult_t result = NativeFunctions.NFD_SaveDialog(filterPtr, utfPtr, out IntPtr pathPtr);
            string resultPath = null;

            if (result == nfdresult_t.NFD_OKAY) 
            {
                resultPath = FromUTF8((byte*)pathPtr);
                NativeFunctions.NFD_Free(pathPtr);
            }
            else if (result == nfdresult_t.NFD_ERROR) 
            {
                error = FromUTF8(NativeFunctions.NFD_GetError());
            }

            return new NFDResult(result, resultPath, [resultPath], error);
        }
    }

    public static unsafe NFDResult OpenFolder(string path = null) 
    {
        byte[] utfPath = null;
        if (path != null) 
        {
            utfPath = ToUTF8(path);
        }
        string error = null;
        fixed (byte* utfPtr = utfPath) 
        {
            nfdresult_t result = NativeFunctions.NFD_PickFolder(utfPtr, out IntPtr pathPtr);
            string resultPath = null;

            if (result == nfdresult_t.NFD_OKAY) 
            {
                resultPath = FromUTF8((byte*)pathPtr);
                NativeFunctions.NFD_Free(pathPtr);
            }
            else if (result == nfdresult_t.NFD_ERROR) 
            {
                error = FromUTF8(NativeFunctions.NFD_GetError());
            }

            return new NFDResult(result, resultPath, [resultPath], error);
        }
    }
}

public struct NFDResult 
{
    private readonly nfdresult_t result;
    public string Path { get; }
    public bool IsCancelled => result == nfdresult_t.NFD_CANCEL;
    public bool IsOk => result == nfdresult_t.NFD_OKAY;
    public IReadOnlyList<string> Paths { get; }
    public bool IsError => result == nfdresult_t.NFD_ERROR;
    public string ErrorMessage { get; }



    internal NFDResult(nfdresult_t result, string path, IReadOnlyList<string> paths, string error) 
    {
        this.result = result;
        Path = path;
        Paths = paths;
        ErrorMessage = error;
    }
}