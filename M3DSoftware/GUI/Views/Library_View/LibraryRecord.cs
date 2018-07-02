// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Library_View.LibraryRecord
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using System.Xml.Serialization;

namespace M3D.GUI.Views.Library_View
{
  public class LibraryRecord
  {
    [XmlAttribute("filename")]
    public string cachefilename;
    [XmlAttribute("icon")]
    public string iconfilename;
    public readonly ulong ID;
    private static ulong nextID;

    public LibraryRecord()
    {
      ID = ++LibraryRecord.nextID;
    }

    public LibraryRecord(LibraryRecord other)
    {
      ID = other.ID;
    }
  }
}
