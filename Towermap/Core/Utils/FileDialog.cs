using System;
using System.Text;
using SDL3;

namespace Towermap;

public struct Filter(string name, string pattern)
{
    public string Name = name;
    public string Pattern = pattern;
}

public static class FileDialog 
{
    private static Action<string> currentAction;
    private static unsafe void OnOpenActionDialog(IntPtr userdata, IntPtr filelist, int filter) 
    {
        if (filelist == IntPtr.Zero)
        {
            return;
        }
        if (*(byte*)filelist == IntPtr.Zero) 
        {
            return;
        }
        byte **files = (byte**)filelist;
        byte *ptr = files[0];
        int count = 0;
        while (*ptr != 0)
        {
            ptr++;
            count++;
        }

        if (count <= 0)
        {
            return;
        }

        string file = Encoding.UTF8.GetString(files[0], count);
        currentAction(file);
    }

    public static unsafe void OpenFile(Action<string> action, string path = null, Filter filter = default) 
    {
        ShowDialog(action, path, filter, AccessType.OpenFile);
    }

    public static unsafe void Save(Action<string> action, string path = null, Filter filter = default) 
    {
        ShowDialog(action, path, filter, AccessType.Save);
    }

    public static unsafe void OpenFolder(Action<string> action, string path = null) 
    {
        ShowDialog(action, path, default, AccessType.OpenFolder);
    }

    private static unsafe void ShowDialog(Action<string> action, string path = null, Filter filter = default, AccessType access = AccessType.OpenFile) 
    {
        currentAction = action;
        var name = filter.Name.AsSpan();
        var pattern = filter.Pattern.AsSpan();

        Span<byte> bName = stackalloc byte[name.Length];
        Span<byte> bPattern = stackalloc byte[pattern.Length];
        Encoding.UTF8.GetBytes(name, bName);
        Encoding.UTF8.GetBytes(pattern, bPattern);
        fixed (byte *nPtr = bName) 
        fixed (byte *pPtr = bPattern)
        {
            switch (access) 
            {
            case AccessType.OpenFile: {
                var filterStruct = new SDL.SDL_DialogFileFilter();
                filterStruct.name = (byte*)nPtr;
                filterStruct.pattern = (byte*)pPtr;
                SDL.SDL_ShowOpenFileDialog(OnOpenActionDialog, IntPtr.Zero, IntPtr.Zero, [filterStruct], 1, path, false);
            }

                break;
            case AccessType.OpenFolder:
                SDL.SDL_ShowOpenFolderDialog(OnOpenActionDialog, IntPtr.Zero, IntPtr.Zero, path, false);
                break;
            case AccessType.Save: {
                var filterStruct = new SDL.SDL_DialogFileFilter();
                filterStruct.name = (byte*)nPtr;
                filterStruct.pattern = (byte*)pPtr;
                SDL.SDL_ShowSaveFileDialog(OnOpenActionDialog, IntPtr.Zero, IntPtr.Zero, [filterStruct], 1, path);
            }
                break;
            }
        }
    }

    private enum AccessType 
    {
        OpenFile,
        OpenFolder,
        Save
    }
}