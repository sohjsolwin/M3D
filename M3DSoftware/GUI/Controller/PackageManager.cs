// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Controller.PackageManager
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      PackageManager packageManager = (PackageManager) null;
      try
      {
        using (XmlReader xmlReader = XmlReader.Create(filePath))
          packageManager = (PackageManager) new XmlSerializer(typeof (PackageManager)).Deserialize(xmlReader);
      }
      catch (Exception ex)
      {
        packageManager = (PackageManager) null;
        if (ex is ThreadAbortException)
          throw ex;
      }
      return packageManager;
    }
  }
}
