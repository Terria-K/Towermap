using System.Runtime.CompilerServices;
using System.Xml;

namespace Towermap;

public static class XmlUtils 
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Attr(this XmlElement xmlElement, string key) 
    {
        return Attr(xmlElement, key, "");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Attr(this XmlElement xmlElement, string key, string defaultValue) 
    {
        XmlAttribute attribute = xmlElement.GetAttributeNode(key);
        if (attribute == null)
        {
            return defaultValue;
        }
        return attribute.Value.Trim();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AttrInt(this XmlElement xmlElement, string key) 
    {
        return AttrInt(xmlElement, key, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AttrInt(this XmlElement xmlElement, string key, int defaultValue) 
    {
        XmlAttribute attribute = xmlElement.GetAttributeNode(key);
        if (attribute == null)
        {
            return defaultValue;
        }

        return int.Parse(attribute.Value.Trim());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long AttrLong(this XmlElement xmlElement, string key) 
    {
        return AttrLong(xmlElement, key, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long AttrLong(this XmlElement xmlElement, string key, long defaultValue) 
    {
        XmlAttribute attribute = xmlElement.GetAttributeNode(key);
        if (attribute == null)
        {
            return defaultValue;
        }

        return long.Parse(attribute.Value.Trim());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong AttrULong(this XmlElement xmlElement, string key) 
    {
        return AttrULong(xmlElement, key, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong AttrULong(this XmlElement xmlElement, string key, ulong defaultValue) 
    {
        XmlAttribute attribute = xmlElement.GetAttributeNode(key);
        if (attribute == null)
        {
            return defaultValue;
        }

        return ulong.Parse(attribute.Value.Trim());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float AttrFloat(this XmlElement xmlElement, string key) 
    {
        return AttrFloat(xmlElement, key, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float AttrFloat(this XmlElement xmlElement, string key, float defaultValue) 
    {
        XmlAttribute attribute = xmlElement.GetAttributeNode(key);
        if (attribute == null)
        {
            return defaultValue;
        }

        return float.Parse(attribute.Value.Trim());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AttrBool(this XmlElement xmlElement, string key) 
    {
        return AttrBool(xmlElement, key, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AttrBool(this XmlElement xmlElement, string key, bool defaultValue) 
    {
        XmlAttribute attribute = xmlElement.GetAttributeNode(key);
        if (attribute == null)
        {
            return defaultValue;
        }

        return bool.Parse(attribute.Value.Trim());
    }

    public static bool TryGetElement(this XmlElement xmlElement, string name, out XmlElement child)
    {
        child = xmlElement[name];
        return child != null;
    }
}