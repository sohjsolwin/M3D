// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Library_View.RecentModelHistory
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.GUI.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace M3D.GUI.Views.Library_View
{
  public class RecentModelHistory
  {
    private List<RecentModelHistory.RecentRecord> m_thedatabaseRecent;

    public RecentModelHistory()
    {
      this.LoadRecentData();
      this.SaveRecentData();
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

    public QueryResults<RecentModelHistory.RecentRecord> QuereyRecords(string filter)
    {
      if (this.m_thedatabaseRecent.Count < 1)
        return (QueryResults<RecentModelHistory.RecentRecord>) null;
      QueryResults<RecentModelHistory.RecentRecord> queryResults = new QueryResults<RecentModelHistory.RecentRecord>();
      foreach (RecentModelHistory.RecentRecord other in this.m_thedatabaseRecent)
      {
        if (string.IsNullOrEmpty(filter) || this.Matches(other._3dmodelfilename, filter))
        {
          RecentModelHistory.RecentRecord recentRecord = new RecentModelHistory.RecentRecord(other);
          queryResults.records.Add(recentRecord);
        }
      }
      return queryResults;
    }

    public RecentModelHistory.RecentRecord GetRecord(string filename)
    {
      for (int index = 0; index < this.m_thedatabaseRecent.Count; ++index)
      {
        if (this.m_thedatabaseRecent[index].cachefilename == filename)
          return new RecentModelHistory.RecentRecord(this.m_thedatabaseRecent[index]);
      }
      return (RecentModelHistory.RecentRecord) null;
    }

    public bool AddModelToRecent(string fileName, string icon)
    {
      SplitFileName splitFileName = new SplitFileName(fileName);
      RecentModelHistory.RecentRecord recentRecord = new RecentModelHistory.RecentRecord(fileName, splitFileName.name + "." + splitFileName.ext, icon);
      for (int index = 0; index < this.m_thedatabaseRecent.Count; ++index)
      {
        if (this.m_thedatabaseRecent[index].cachefilename != null && this.m_thedatabaseRecent[index].cachefilename.Equals(recentRecord.cachefilename, StringComparison.OrdinalIgnoreCase))
        {
          if (string.IsNullOrEmpty(icon) && !string.IsNullOrEmpty(this.m_thedatabaseRecent[index].iconfilename))
            recentRecord.iconfilename = this.m_thedatabaseRecent[index].iconfilename;
          else if (!string.IsNullOrEmpty(this.m_thedatabaseRecent[index].iconfilename) && !this.m_thedatabaseRecent[index].iconfilename.Equals(recentRecord.iconfilename))
          {
            if (File.Exists(this.m_thedatabaseRecent[index].iconfilename))
            {
              try
              {
                File.Delete(this.m_thedatabaseRecent[index].iconfilename);
              }
              catch (IOException ex)
              {
              }
            }
          }
          this.m_thedatabaseRecent.RemoveAt(index);
          break;
        }
      }
      this.m_thedatabaseRecent.Insert(0, recentRecord);
      this.SaveRecentData();
      return true;
    }

    public void RemoveFileFromRecent(ulong ID)
    {
      for (int index = 0; index < this.m_thedatabaseRecent.Count; ++index)
      {
        if ((long) this.m_thedatabaseRecent[index].ID == (long) ID)
        {
          this.m_thedatabaseRecent.RemoveAt(index);
          break;
        }
      }
      this.SaveRecentData();
    }

    public void RemoveFileFromRecent(string filename)
    {
      for (int index = 0; index < this.m_thedatabaseRecent.Count; ++index)
      {
        if (this.m_thedatabaseRecent[index].cachefilename.Equals(filename, StringComparison.OrdinalIgnoreCase))
        {
          this.m_thedatabaseRecent.RemoveAt(index);
          break;
        }
      }
      this.SaveRecentData();
    }

    private void LoadRecentData()
    {
      try
      {
        using (StreamReader streamReader = new StreamReader(Paths.RecentDBPath))
        {
          using (XmlReader xmlReader = XmlReader.Create((TextReader) streamReader))
            this.m_thedatabaseRecent = (List<RecentModelHistory.RecentRecord>) new XmlSerializer(typeof (List<RecentModelHistory.RecentRecord>), new XmlRootAttribute("Recent")).Deserialize(xmlReader);
        }
      }
      catch (Exception ex)
      {
        this.m_thedatabaseRecent = new List<RecentModelHistory.RecentRecord>();
      }
    }

    private void SaveRecentData()
    {
      try
      {
        XmlSerializer xmlSerializer = new XmlSerializer(typeof (List<RecentModelHistory.RecentRecord>), new XmlRootAttribute("Recent"));
        XmlSerializerNamespaces serializerNamespaces = new XmlSerializerNamespaces();
        serializerNamespaces.Add("", "");
        StreamWriter streamWriter1 = new StreamWriter(Paths.RecentDBPath);
        StreamWriter streamWriter2 = streamWriter1;
        List<RecentModelHistory.RecentRecord> thedatabaseRecent = this.m_thedatabaseRecent;
        XmlSerializerNamespaces namespaces = serializerNamespaces;
        xmlSerializer.Serialize((TextWriter) streamWriter2, (object) thedatabaseRecent, namespaces);
        streamWriter1.Close();
      }
      catch (Exception ex)
      {
      }
    }

    private string GenerateCacheFileName(string filename)
    {
      SplitFileName splitFileName = new SplitFileName(filename);
      Guid guid = Guid.NewGuid();
      return splitFileName.name + guid.ToString().Substring(0, 18) + splitFileName.ext;
    }

    public enum LibraryRecordLocation
    {
      MyLibrary,
      Cache,
      MyComputer,
    }

    public class RecentRecord : LibraryRecord
    {
      [XmlAttribute("modelfilename")]
      public string _3dmodelfilename;

      public RecentRecord()
      {
      }

      public RecentRecord(string cachefilename, string _3dmodelfilename, string iconfilename)
      {
        this.cachefilename = cachefilename;
        this._3dmodelfilename = _3dmodelfilename;
        this.iconfilename = iconfilename;
      }

      public RecentRecord(RecentModelHistory.RecentRecord other)
        : base((LibraryRecord) other)
      {
        this.cachefilename = other.cachefilename;
        this._3dmodelfilename = other._3dmodelfilename;
        this.iconfilename = other.iconfilename;
      }

      [XmlAttribute("location")]
      public RecentModelHistory.LibraryRecordLocation location
      {
        get
        {
          return RecentModelHistory.LibraryRecordLocation.MyComputer;
        }
        set
        {
        }
      }

      [XmlAttribute("path")]
      public string path
      {
        get
        {
          string directoryName = Path.GetDirectoryName(this.cachefilename);
          if (!string.IsNullOrEmpty(directoryName))
            return directoryName + (!directoryName.EndsWith(Path.DirectorySeparatorChar.ToString()) ? Path.DirectorySeparatorChar.ToString() : "");
          return "";
        }
        set
        {
        }
      }
    }
  }
}
