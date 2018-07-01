// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.SpoolerInfo
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Core;
using M3D.Spooling.Printer_Profiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public class SpoolerInfo
  {
    private static XmlWriterSettings settings = new XmlWriterSettings();
    private static XmlSerializer __class_serializer = (XmlSerializer) null;
    private XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
    [XmlElement("VersionNumber")]
    public VersionNumber Version;
    [XmlElement("PrinterProfileSummary", Type = typeof (EmbeddedFirmwareSummary))]
    public List<EmbeddedFirmwareSummary> SupportPrinterProfiles;
    [XmlElement("PrinterProfile", Type = typeof (PrinterProfile))]
    public List<PrinterProfile> PrinterProfileList;

    public SpoolerInfo()
    {
      this.SupportPrinterProfiles = new List<EmbeddedFirmwareSummary>();
      this.PrinterProfileList = new List<PrinterProfile>();
    }

    public SpoolerInfo(SpoolerInfo other)
    {
      this.CopyFrom(other);
    }

    public SpoolerInfo(string serialization)
      : this()
    {
      this.Deserialize(serialization);
    }

    public void Deserialize(string serialization)
    {
      using (TextReader textReader = (TextReader) new StringReader(serialization))
      {
        try
        {
          this.CopyFrom((SpoolerInfo) SpoolerInfo.ClassSerializer.Deserialize(textReader));
        }
        catch (Exception ex)
        {
          ErrorLogger.LogErrorMsg("Loading XML Exception: " + ex.Message + (ex.InnerException != null ? "\nInner Exception: " + ex.InnerException.Message : ""));
          this.CopyFrom(new SpoolerInfo());
        }
      }
    }

    public string Serialize()
    {
      SpoolerInfo.settings.OmitXmlDeclaration = true;
      StringWriter stringWriter = new StringWriter();
      XmlWriter xmlWriter = XmlWriter.Create((TextWriter) stringWriter, SpoolerInfo.settings);
      this.ns.Add("", "");
      try
      {
        SpoolerInfo.ClassSerializer.Serialize(xmlWriter, (object) this, this.ns);
      }
      catch (Exception ex)
      {
        if (Debugger.IsAttached)
          Debugger.Break();
        throw;
      }
      return stringWriter.ToString();
    }

    public void CopyFrom(SpoolerInfo other)
    {
      this.Version = other.Version;
      this.SupportPrinterProfiles = new List<EmbeddedFirmwareSummary>((IEnumerable<EmbeddedFirmwareSummary>) other.SupportPrinterProfiles);
      this.PrinterProfileList = new List<PrinterProfile>((IEnumerable<PrinterProfile>) other.PrinterProfileList);
    }

    public EmbeddedFirmwareSummary GetProfileByName(string name)
    {
      return this.SupportPrinterProfiles.Find((Predicate<EmbeddedFirmwareSummary>) (x => x.Name == name));
    }

    private static XmlSerializer ClassSerializer
    {
      get
      {
        if (SpoolerInfo.__class_serializer == null)
          SpoolerInfo.__class_serializer = new XmlSerializer(typeof (SpoolerInfo));
        return SpoolerInfo.__class_serializer;
      }
    }
  }
}
