// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Library_View.RecentPrintsHistory
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using ICSharpCode.SharpZipLib.Zip;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.Slicer.General;
using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace M3D.GUI.Views.Library_View
{
  public class RecentPrintsHistory
  {
    public RecentPrintsHistory.StartedPrintListChanged OnStartedPrintListChanged;
    public List<RecentPrintsHistory.PrintHistory> startedPrintList;

    public RecentPrintsHistory()
    {
      this.LoadStartedPrints();
      this.CleanupStartedPrints();
    }

    public void RefreshListChanged()
    {
      if (this.OnStartedPrintListChanged == null)
        return;
      this.OnStartedPrintListChanged(new List<RecentPrintsHistory.PrintHistory>((IEnumerable<RecentPrintsHistory.PrintHistory>) this.startedPrintList));
    }

    public QueryResults<RecentPrintsHistory.PrintHistory> QuereyRecords(string filter)
    {
      if (this.startedPrintList.Count < 1)
        return (QueryResults<RecentPrintsHistory.PrintHistory>) null;
      QueryResults<RecentPrintsHistory.PrintHistory> queryResults = new QueryResults<RecentPrintsHistory.PrintHistory>();
      foreach (RecentPrintsHistory.PrintHistory startedPrint in this.startedPrintList)
      {
        if (string.IsNullOrEmpty(filter) || this.Matches(startedPrint.cachefilename, filter))
        {
          RecentPrintsHistory.PrintHistory printHistory = new RecentPrintsHistory.PrintHistory(startedPrint);
          queryResults.records.Add(printHistory);
        }
      }
      return queryResults;
    }

    private bool Matches(string word, string filter)
    {
      string str1 = filter;
      char[] chArray = new char[1]{ ' ' };
      foreach (string str2 in str1.Split(chArray))
      {
        if (word.IndexOf(str2, 0, StringComparison.OrdinalIgnoreCase) >= 0)
          return true;
      }
      return false;
    }

    public void AddRecentPrintHistory(JobParams printerJob, PrinterObject printer, string slicerProfileName, List<Slicer.General.KeyValuePair<string, string>> complete_slicer_settings, List<PrintDetails.ObjectDetails> original_objectList)
    {
      RecentPrintsHistory.PrintHistory cph;
      RecentPrintsHistory.CreatePrintHistoryFolder(printerJob, printer, slicerProfileName, complete_slicer_settings, original_objectList, out cph);
      this.startedPrintList.Insert(0, cph);
      this.RefreshListChanged();
      this.SaveStartedPrints();
    }

    public static bool SavePrintHistoryToZip(string filename, RecentPrintsHistory.PrintHistory printHistory)
    {
      ZipFile zipFile;
      try
      {
        zipFile = ZipFile.Create(filename);
      }
      catch (Exception ex)
      {
        return false;
      }
      bool flag = true;
      try
      {
        zipFile.BeginUpdate();
        foreach (string file in Directory.GetFiles(printHistory.folder))
          zipFile.Add(file, Path.GetFileName(file));
        zipFile.CommitUpdate();
      }
      catch (Exception ex)
      {
        flag = false;
      }
      try
      {
        zipFile.Close();
      }
      catch (Exception ex)
      {
        flag = false;
      }
      return flag;
    }

    public static void CreatePrintHistoryFolder(JobParams printerJob, PrinterObject printer, string slicerProfileName, List<Slicer.General.KeyValuePair<string, string>> complete_slicer_settings, List<PrintDetails.ObjectDetails> original_objectList, out RecentPrintsHistory.PrintHistory cph)
    {
      cph = new RecentPrintsHistory.PrintHistory(printerJob.jobname, printerJob.jobGuid, DateTime.Now);
      cph.iconfilename = Path.Combine(cph.folder, "previewimage.jpg");
      string printerViewFile = Path.Combine(cph.folder, "printerview.xml");
      string printerSettingsFile = Path.Combine(cph.folder, "printersettings.xml");
      try
      {
        Directory.CreateDirectory(cph.folder);
        List<PrintDetails.ObjectDetails> objectList = new List<PrintDetails.ObjectDetails>();
        foreach (PrintDetails.ObjectDetails originalObject in original_objectList)
        {
          string filename = originalObject.filename;
          PrintDetails.ObjectDetails objectDetails = new PrintDetails.ObjectDetails(originalObject);
          objectDetails.printerSettingsXMLFile = "printersettings.xml";
          objectDetails.printerViewXMLFile = "printerview.xml";
          objectDetails.zipFileName = "";
          objectDetails.filename = Path.GetFileName(filename);
          objectList.Add(objectDetails);
          File.Copy(filename, Path.Combine(cph.folder, objectDetails.filename), true);
        }
        File.Copy(printerJob.preview_image_file_name, cph.iconfilename, true);
        SettingsManager.SavePrintingObjectsDetails(printerViewFile, objectList);
        SettingsManager.SavePrintJobInfo(printerSettingsFile, printerJob, printer, slicerProfileName, complete_slicer_settings);
        FileUtils.GrantAccess(cph.folder);
      }
      catch (Exception ex)
      {
      }
    }

    private void LoadStartedPrints()
    {
      try
      {
        using (StreamReader streamReader = new StreamReader(Paths.PrintHistoryPath))
        {
          using (XmlReader xmlReader = XmlReader.Create((TextReader) streamReader))
            this.startedPrintList = (List<RecentPrintsHistory.PrintHistory>) new XmlSerializer(typeof (List<RecentPrintsHistory.PrintHistory>), new XmlRootAttribute(nameof (RecentPrintsHistory))).Deserialize(xmlReader);
        }
      }
      catch (Exception ex)
      {
        this.startedPrintList = new List<RecentPrintsHistory.PrintHistory>();
      }
    }

    private void SaveStartedPrints()
    {
      try
      {
        XmlSerializer xmlSerializer = new XmlSerializer(typeof (List<RecentPrintsHistory.PrintHistory>), new XmlRootAttribute(nameof (RecentPrintsHistory)));
        XmlSerializerNamespaces serializerNamespaces = new XmlSerializerNamespaces();
        serializerNamespaces.Add("", "");
        StreamWriter streamWriter1 = new StreamWriter(Paths.PrintHistoryPath);
        StreamWriter streamWriter2 = streamWriter1;
        List<RecentPrintsHistory.PrintHistory> startedPrintList = this.startedPrintList;
        XmlSerializerNamespaces namespaces = serializerNamespaces;
        xmlSerializer.Serialize((TextWriter) streamWriter2, (object) startedPrintList, namespaces);
        streamWriter1.Close();
      }
      catch (Exception ex)
      {
      }
    }

    private int CompareDateTime(Slicer.General.KeyValuePair<string, DateTime> a, Slicer.General.KeyValuePair<string, DateTime> b)
    {
      return -a.Value.CompareTo(b.Value);
    }

    private void CleanupStartedPrints()
    {
      DateTime t2 = DateTime.Now.AddDays(-7.0);
      foreach (RecentPrintsHistory.PrintHistory record in new List<RecentPrintsHistory.PrintHistory>((IEnumerable<RecentPrintsHistory.PrintHistory>) this.startedPrintList))
      {
        if (DateTime.Compare(record.begin, t2) < 0)
          this.RemoveRecord(record);
      }
      this.SaveStartedPrints();
      List<string> stringList = new List<string>();
      try
      {
        stringList.AddRange((IEnumerable<string>) Directory.GetDirectories(Path.Combine(Paths.PublicDataFolder, "MyLibrary", "Prints")));
      }
      catch (Exception ex)
      {
      }
      if (stringList.Count == 0)
        return;
      foreach (string path in stringList)
      {
        if (DateTime.Compare(File.GetLastAccessTime(path), t2) < 0)
        {
          try
          {
            Directory.Delete(path, true);
          }
          catch (Exception ex)
          {
          }
        }
      }
    }

    public void RemoveRecord(RecentPrintsHistory.PrintHistory record)
    {
      for (int index = 0; index < this.startedPrintList.Count; ++index)
      {
        if (record.folder == this.startedPrintList[index].folder)
        {
          this.RemoveRecord(index);
          break;
        }
      }
    }

    private void RemoveRecord(int index)
    {
      RecentPrintsHistory.PrintHistory startedPrint = this.startedPrintList[index];
      try
      {
        Directory.Delete(startedPrint.folder, true);
      }
      catch (Exception ex)
      {
      }
      this.startedPrintList.Remove(startedPrint);
      this.SaveStartedPrints();
    }

    public delegate void StartedPrintListChanged(List<RecentPrintsHistory.PrintHistory> StartedPrints);

    public class PrintHistory : LibraryRecord
    {
      [XmlElement("GUID")]
      public string jobGuid;
      [XmlElement("Folder")]
      public string folder;
      [XmlElement("PrintStarted")]
      public DateTime begin;

      public PrintHistory()
      {
      }

      public PrintHistory(RecentPrintsHistory.PrintHistory other)
        : base((LibraryRecord) other)
      {
        this.cachefilename = other.cachefilename;
        this.jobGuid = other.jobGuid;
        this.folder = other.folder;
        this.begin = other.begin;
        this.iconfilename = other.iconfilename;
      }

      public PrintHistory(string filename, string guid, DateTime time)
      {
        this.cachefilename = filename;
        this.jobGuid = guid;
        this.begin = time;
        this.folder = Path.Combine(Paths.PublicDataFolder, "MyLibrary", "Prints", Path.GetFileNameWithoutExtension(this.cachefilename) + (this.begin.Ticks / 10000L).ToString());
      }

      public override string ToString()
      {
        return Path.GetFileNameWithoutExtension(this.cachefilename) + this.begin.ToString(" MM\\/dd\\/yyyy h\\:mm tt", (IFormatProvider) new CultureInfo("en-US"));
      }
    }
  }
}
