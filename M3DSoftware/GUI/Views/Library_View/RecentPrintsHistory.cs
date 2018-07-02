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
      LoadStartedPrints();
      CleanupStartedPrints();
    }

    public void RefreshListChanged()
    {
      if (OnStartedPrintListChanged == null)
      {
        return;
      }

      OnStartedPrintListChanged(new List<RecentPrintsHistory.PrintHistory>((IEnumerable<RecentPrintsHistory.PrintHistory>)startedPrintList));
    }

    public QueryResults<RecentPrintsHistory.PrintHistory> QuereyRecords(string filter)
    {
      if (startedPrintList.Count < 1)
      {
        return (QueryResults<RecentPrintsHistory.PrintHistory>) null;
      }

      var queryResults = new QueryResults<RecentPrintsHistory.PrintHistory>();
      foreach (RecentPrintsHistory.PrintHistory startedPrint in startedPrintList)
      {
        if (string.IsNullOrEmpty(filter) || Matches(startedPrint.cachefilename, filter))
        {
          var printHistory = new RecentPrintsHistory.PrintHistory(startedPrint);
          queryResults.records.Add(printHistory);
        }
      }
      return queryResults;
    }

    private bool Matches(string word, string filter)
    {
      var str1 = filter;
      char[] chArray = new char[1]{ ' ' };
      foreach (var str2 in str1.Split(chArray))
      {
        if (word.IndexOf(str2, 0, StringComparison.OrdinalIgnoreCase) >= 0)
        {
          return true;
        }
      }
      return false;
    }

    public void AddRecentPrintHistory(JobParams printerJob, PrinterObject printer, string slicerProfileName, List<Slicer.General.KeyValuePair<string, string>> complete_slicer_settings, List<PrintDetails.ObjectDetails> original_objectList)
    {
      RecentPrintsHistory.CreatePrintHistoryFolder(printerJob, printer, slicerProfileName, complete_slicer_settings, original_objectList, out PrintHistory cph);
      startedPrintList.Insert(0, cph);
      RefreshListChanged();
      SaveStartedPrints();
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
      var flag = true;
      try
      {
        zipFile.BeginUpdate();
        foreach (var file in Directory.GetFiles(printHistory.folder))
        {
          zipFile.Add(file, Path.GetFileName(file));
        }

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
      var printerViewFile = Path.Combine(cph.folder, "printerview.xml");
      var printerSettingsFile = Path.Combine(cph.folder, "printersettings.xml");
      try
      {
        Directory.CreateDirectory(cph.folder);
        var objectList = new List<PrintDetails.ObjectDetails>();
        foreach (PrintDetails.ObjectDetails originalObject in original_objectList)
        {
          var filename = originalObject.filename;
          var objectDetails = new PrintDetails.ObjectDetails(originalObject)
          {
            printerSettingsXMLFile = "printersettings.xml",
            printerViewXMLFile = "printerview.xml",
            zipFileName = "",
            filename = Path.GetFileName(filename)
          };
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
        using (var streamReader = new StreamReader(Paths.PrintHistoryPath))
        {
          using (var xmlReader = XmlReader.Create((TextReader) streamReader))
          {
            startedPrintList = (List<RecentPrintsHistory.PrintHistory>) new XmlSerializer(typeof (List<RecentPrintsHistory.PrintHistory>), new XmlRootAttribute(nameof (RecentPrintsHistory))).Deserialize(xmlReader);
          }
        }
      }
      catch (Exception ex)
      {
        startedPrintList = new List<RecentPrintsHistory.PrintHistory>();
      }
    }

    private void SaveStartedPrints()
    {
      try
      {
        var xmlSerializer = new XmlSerializer(typeof (List<RecentPrintsHistory.PrintHistory>), new XmlRootAttribute(nameof (RecentPrintsHistory)));
        var serializerNamespaces = new XmlSerializerNamespaces();
        serializerNamespaces.Add("", "");
        var streamWriter1 = new StreamWriter(Paths.PrintHistoryPath);
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
      foreach (RecentPrintsHistory.PrintHistory record in new List<RecentPrintsHistory.PrintHistory>((IEnumerable<RecentPrintsHistory.PrintHistory>)startedPrintList))
      {
        if (DateTime.Compare(record.begin, t2) < 0)
        {
          RemoveRecord(record);
        }
      }
      SaveStartedPrints();
      var stringList = new List<string>();
      try
      {
        stringList.AddRange((IEnumerable<string>) Directory.GetDirectories(Path.Combine(Paths.PublicDataFolder, "MyLibrary", "Prints")));
      }
      catch (Exception ex)
      {
      }
      if (stringList.Count == 0)
      {
        return;
      }

      foreach (var path in stringList)
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
      for (var index = 0; index < startedPrintList.Count; ++index)
      {
        if (record.folder == startedPrintList[index].folder)
        {
          RemoveRecord(index);
          break;
        }
      }
    }

    private void RemoveRecord(int index)
    {
      RecentPrintsHistory.PrintHistory startedPrint = startedPrintList[index];
      try
      {
        Directory.Delete(startedPrint.folder, true);
      }
      catch (Exception ex)
      {
      }
      startedPrintList.Remove(startedPrint);
      SaveStartedPrints();
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
        cachefilename = other.cachefilename;
        jobGuid = other.jobGuid;
        folder = other.folder;
        begin = other.begin;
        iconfilename = other.iconfilename;
      }

      public PrintHistory(string filename, string guid, DateTime time)
      {
        cachefilename = filename;
        jobGuid = guid;
        begin = time;
        folder = Path.Combine(Paths.PublicDataFolder, "MyLibrary", "Prints", Path.GetFileNameWithoutExtension(cachefilename) + (begin.Ticks / 10000L).ToString());
      }

      public override string ToString()
      {
        return Path.GetFileNameWithoutExtension(cachefilename) + begin.ToString(" MM\\/dd\\/yyyy h\\:mm tt", (IFormatProvider) new CultureInfo("en-US"));
      }
    }
  }
}
