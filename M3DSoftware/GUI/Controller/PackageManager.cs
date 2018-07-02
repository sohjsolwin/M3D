using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace M3D.GUI.Controller
{
  public class PackageManager
  {
    [XmlArray("Packages")]
    [XmlArrayItem("Package")]
    public List<Package> items = new List<Package>();

    public static PackageManager Load(string filePath)
    {
      var packageManager = (PackageManager) null;
      try
      {
        using (var xmlReader = XmlReader.Create(filePath))
        {
          packageManager = (PackageManager) new XmlSerializer(typeof (PackageManager)).Deserialize(xmlReader);
        }
      }
      catch (Exception ex)
      {
        packageManager = null;
        if (ex is ThreadAbortException)
        {
          throw ex;
        }
      }
      return packageManager;
    }
  }
}
