// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.TextLocalization.Locale
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace M3D.Graphics.TextLocalization
{
  public class Locale
  {
    public static Locale GlobalLocale;
    private string language_name;
    private string characterset;
    private string fontfamily;
    private string fontfile;
    private Dictionary<string, string> stringtable;

    public static Dictionary<string, string> GetLanguageList(string path)
    {
      var dictionary = new Dictionary<string, string>();
      foreach (var file in Directory.GetFiles(path, "*.locale.xml"))
      {
        var langaugeFromFile = Locale.GetLangaugeFromFile(file);
        if (!string.IsNullOrEmpty(langaugeFromFile))
        {
          dictionary.Add(langaugeFromFile, file);
        }
      }
      return dictionary;
    }

    private static string GetLangaugeFromFile(string filename)
    {
      var str = "";
      try
      {
        using (var xmlReader = XmlReader.Create(filename))
        {
          while (xmlReader.Read())
          {
            if (xmlReader.IsStartElement() && xmlReader.Name == nameof (Locale))
            {
              str = xmlReader.GetAttribute("language");
            }
          }
        }
      }
      catch (FileNotFoundException ex)
      {
      }
      return str;
    }

    public Locale(string resource)
    {
      stringtable = new Dictionary<string, string>();
      try
      {
        using (var xmlReader = XmlReader.Create(resource))
        {
          while (xmlReader.Read())
          {
            if (xmlReader.IsStartElement())
            {
              var name = xmlReader.Name;
              if (!(name == "T"))
              {
                if (name == nameof (Locale))
                {
                  language_name = xmlReader.GetAttribute("language");
                  fontfamily = xmlReader.GetAttribute("font");
                  fontfile = xmlReader.GetAttribute("file");
                  if (fontfile != null)
                  {
                    fontfile = Path.Combine(Path.GetDirectoryName(resource), fontfile);
                  }

                  characterset = xmlReader.GetAttribute(nameof (characterset));
                  characterset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890.:,;'\"(!?)+-*/=_{}[]@~#\\<>|^%$£&" + characterset;
                }
              }
              else
              {
                stringtable.Add(xmlReader.GetAttribute("key"), xmlReader.GetAttribute("text"));
              }
            }
          }
        }
      }
      catch (FileNotFoundException ex)
      {
      }
    }

    public string T(string key)
    {
      try
      {
        return stringtable[key].Replace("\\n", "\n");
      }
      catch (KeyNotFoundException ex)
      {
        return key;
      }
    }

    public string GetLanguageName()
    {
      return language_name;
    }

    public string GetCharacterSet()
    {
      return characterset;
    }

    public string GetFontFamily()
    {
      return fontfamily;
    }

    public string GetFontFile()
    {
      return fontfile;
    }
  }
}
