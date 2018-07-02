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
