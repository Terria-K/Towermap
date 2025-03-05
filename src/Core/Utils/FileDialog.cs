using System;
using System.Text;
using Riateu;
using SDL3;

namespace Towermap;

public struct Filter(string name, string pattern)
{
    public string Name = name;
    public string Pattern = pattern;
}

public struct Property 
{
    public string Title;
    public Window Window;
    public Filter Filter;
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

    public static unsafe void OpenFile(Action<string> action, string path = null, Property property = default) 
    {
        ShowDialog(action, path, property, SDL.SDL_FileDialogType.SDL_FILEDIALOG_OPENFILE);
    }

    public static unsafe void Save(Action<string> action, string path = null, Property property = default) 
    {
        ShowDialog(action, path, property, SDL.SDL_FileDialogType.SDL_FILEDIALOG_SAVEFILE);
    }

    public static unsafe void OpenFolder(Action<string> action, string path = null) 
    {
        ShowDialog(action, path, default, SDL.SDL_FileDialogType.SDL_FILEDIALOG_OPENFOLDER);
    }

    private static unsafe void ShowDialog(Action<string> action, string path = null, Property property = default, SDL.SDL_FileDialogType access = SDL.SDL_FileDialogType.SDL_FILEDIALOG_OPENFILE) 
    {
        currentAction = action;

        byte *name = null;
        byte *pattern = null;
        
        if (property.Filter.Name != null)
        {
            name = EncodeAsUTF8(property.Filter.Name);
            pattern = EncodeAsUTF8(property.Filter.Pattern);
        }


        var properties = SDL.SDL_CreateProperties();
        if (property.Title != null)
        {
            SDL.SDL_SetStringProperty(properties, SDL.SDL_PROP_FILE_DIALOG_TITLE_STRING, property.Title);
        }

        if (property.Window != null)
        {
            SDL.SDL_SetPointerProperty(properties, SDL.SDL_PROP_FILE_DIALOG_WINDOW_POINTER, property.Window.Handle);
        }

        if (property.Filter.Name != null && access != SDL.SDL_FileDialogType.SDL_FILEDIALOG_OPENFOLDER)
        {
            var filterStruct = new SDL.SDL_DialogFileFilter();
            filterStruct.name = name;
            filterStruct.pattern = pattern;
            Span<SDL.SDL_DialogFileFilter> fileFilters = [filterStruct];
            fixed (SDL.SDL_DialogFileFilter *fi = fileFilters)
            {
                SDL.SDL_SetPointerProperty(properties, SDL.SDL_PROP_FILE_DIALOG_FILTERS_POINTER, (nint)fi);
            }
            SDL.SDL_SetNumberProperty(properties, SDL.SDL_PROP_FILE_DIALOG_NFILTERS_NUMBER, 1);
        }

        SDL.SDL_ShowFileDialogWithProperties(access, OnOpenActionDialog, IntPtr.Zero, properties);

        SDL.SDL_DestroyProperties(properties);
    }

    private static unsafe byte* EncodeAsUTF8(string str)
    {
        if (str == null)
        {
            return (byte*) 0;
        }

        var size = (str.Length * 4) + 1;
        var buffer = (byte*) SDL.SDL_malloc((UIntPtr) size);
        fixed (char* strPtr = str)
        {
            Encoding.UTF8.GetBytes(strPtr, str.Length + 1, buffer, size);
        }

        return buffer;
    }
}