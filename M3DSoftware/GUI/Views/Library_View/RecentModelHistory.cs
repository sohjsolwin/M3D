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
      LoadRecentData();
      SaveRecentData();
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

    public QueryResults<RecentModelHistory.RecentRecord> QuereyRecords(string filter)
    {
      if (m_thedatabaseRecent.Count < 1)
      {
        return (QueryResults<RecentModelHistory.RecentRecord>) null;
      }

      var queryResults = new QueryResults<RecentModelHistory.RecentRecord>();
      foreach (RecentModelHistory.RecentRecord other in m_thedatabaseRecent)
      {
        if (string.IsNullOrEmpty(filter) || Matches(other._3dmodelfilename, filter))
        {
          var recentRecord = new RecentModelHistory.RecentRecord(other);
          queryResults.records.Add(recentRecord);
        }
      }
      return queryResults;
    }

    public RecentModelHistory.RecentRecord GetRecord(string filename)
    {
      for (var index = 0; index < m_thedatabaseRecent.Count; ++index)
      {
        if (m_thedatabaseRecent[index].cachefilename == filename)
        {
          return new RecentModelHistory.RecentRecord(m_thedatabaseRecent[index]);
        }
      }
      return (RecentModelHistory.RecentRecord) null;
    }

    public bool AddModelToRecent(string fileName, string icon)
    {
      var splitFileName = new SplitFileName(fileName);
      var recentRecord = new RecentModelHistory.RecentRecord(fileName, splitFileName.name + "." + splitFileName.ext, icon);
      for (var index = 0; index < m_thedatabaseRecent.Count; ++index)
      {
        if (m_thedatabaseRecent[index].cachefilename != null && m_thedatabaseRecent[index].cachefilename.Equals(recentRecord.cachefilename, StringComparison.OrdinalIgnoreCase))
        {
          if (string.IsNullOrEmpty(icon) && !string.IsNullOrEmpty(m_thedatabaseRecent[index].iconfilename))
          {
            recentRecord.iconfilename = m_thedatabaseRecent[index].iconfilename;
          }
          else if (!string.IsNullOrEmpty(m_thedatabaseRecent[index].iconfilename) && !m_thedatabaseRecent[index].iconfilename.Equals(recentRecord.iconfilename))
          {
            if (File.Exists(m_thedatabaseRecent[index].iconfilename))
            {
              try
              {
                File.Delete(m_thedatabaseRecent[index].iconfilename);
              }
              catch (IOException ex)
              {
              }
            }
          }
          m_thedatabaseRecent.RemoveAt(index);
          break;
        }
      }
      m_thedatabaseRecent.Insert(0, recentRecord);
      SaveRecentData();
      return true;
    }

    public void RemoveFileFromRecent(ulong ID)
    {
      for (var index = 0; index < m_thedatabaseRecent.Count; ++index)
      {
        if ((long)m_thedatabaseRecent[index].ID == (long) ID)
        {
          m_thedatabaseRecent.RemoveAt(index);
          break;
        }
      }
      SaveRecentData();
    }

    public void RemoveFileFromRecent(string filename)
    {
      for (var index = 0; index < m_thedatabaseRecent.Count; ++index)
      {
        if (m_thedatabaseRecent[index].cachefilename.Equals(filename, StringComparison.OrdinalIgnoreCase))
        {
          m_thedatabaseRecent.RemoveAt(index);
          break;
        }
      }
      SaveRecentData();
    }

    private void LoadRecentData()
    {
      try
      {
        using (var streamReader = new StreamReader(Paths.RecentDBPath))
        {
          using (var xmlReader = XmlReader.Create((TextReader) streamReader))
          {
            m_thedatabaseRecent = (List<RecentModelHistory.RecentRecord>) new XmlSerializer(typeof (List<RecentModelHistory.RecentRecord>), new XmlRootAttribute("Recent")).Deserialize(xmlReader);
          }
        }
      }
      catch (Exception ex)
      {
        m_thedatabaseRecent = new List<RecentModelHistory.RecentRecord>();
      }
    }

    private void SaveRecentData()
    {
      try
      {
        var xmlSerializer = new XmlSerializer(typeof (List<RecentModelHistory.RecentRecord>), new XmlRootAttribute("Recent"));
        var serializerNamespaces = new XmlSerializerNamespaces();
        serializerNamespaces.Add("", "");
        var streamWriter1 = new StreamWriter(Paths.RecentDBPath);
        StreamWriter streamWriter2 = streamWriter1;
        List<RecentModelHistory.RecentRecord> thedatabaseRecent = m_thedatabaseRecent;
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
      var splitFileName = new SplitFileName(filename);
      var guid = Guid.NewGuid();
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
        cachefilename = other.cachefilename;
        _3dmodelfilename = other._3dmodelfilename;
        iconfilename = other.iconfilename;
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
          var directoryName = Path.GetDirectoryName(cachefilename);
          if (!string.IsNullOrEmpty(directoryName))
          {
            return directoryName + (!directoryName.EndsWith(Path.DirectorySeparatorChar.ToString()) ? Path.DirectorySeparatorChar.ToString() : "");
          }

          return "";
        }
        set
        {
        }
      }
    }
  }
}
