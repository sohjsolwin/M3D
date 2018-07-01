// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Utils.DebugLogger
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System;
using System.Diagnostics;
using System.IO;

namespace M3D.Spooling.Common.Utils
{
  public class DebugLogger
  {
    private uint m_iPrimaryMaxSize = 100;
    private uint m_SecondaryMaxSize = 100;
    private DebugLog[] m_PrimaryLogs;
    private DebugLog[] m_SecondaryLogs;
    private uint m_iPrimaryStartIndex;
    private uint m_iPrimaryIndex;
    private uint m_iSecondaryStartIndex;
    private uint m_iSecondaryIndex;
    private string m_sSaveFileName;

    public DebugLogger(string filename, uint maxSize)
    {
      this.m_sSaveFileName = filename;
      this.m_iPrimaryMaxSize = maxSize;
      this.m_SecondaryMaxSize = maxSize;
      this.Init();
      FileUtils.GrantAccess(this.m_sSaveFileName);
    }

    public void Init()
    {
      this.m_PrimaryLogs = new DebugLog[(int) this.m_iPrimaryMaxSize];
      this.m_SecondaryLogs = new DebugLog[(int) this.m_SecondaryMaxSize];
      this.Add("DebuggLogger Init()", "Initialize debug logger. Max size: " + (object) this.m_iPrimaryMaxSize, DebugLogger.LogType.Primary);
    }

    public void Add(string functionName, string description, DebugLogger.LogType logType)
    {
      try
      {
        if (logType == DebugLogger.LogType.Primary)
        {
          this.m_PrimaryLogs[(int) this.m_iPrimaryIndex] = new DebugLog(functionName, description);
          this.m_iPrimaryIndex = (this.m_iPrimaryIndex + 1U) % this.m_iPrimaryMaxSize;
          if ((int) this.m_iPrimaryIndex == (int) this.m_iPrimaryStartIndex)
          {
            ++this.m_iPrimaryStartIndex;
            this.m_iPrimaryStartIndex %= this.m_iPrimaryMaxSize;
          }
        }
        else
        {
          this.m_SecondaryLogs[(int) this.m_iSecondaryIndex] = new DebugLog(functionName, description);
          this.m_iSecondaryIndex = (this.m_iSecondaryIndex + 1U) % this.m_SecondaryMaxSize;
          if ((int) this.m_iSecondaryIndex == (int) this.m_iSecondaryStartIndex)
          {
            ++this.m_iSecondaryStartIndex;
            this.m_iSecondaryStartIndex %= this.m_SecondaryMaxSize;
          }
        }
      }
      catch (Exception ex)
      {
        Trace.WriteLine(ex.Message);
      }
      this.Print(this.m_sSaveFileName);
    }

    public void Print(string fileName)
    {
      try
      {
        TextWriter text = (TextWriter) File.CreateText(fileName);
        text.WriteLine("Primary Debug Log");
        text.WriteLine();
        for (uint index = this.m_iPrimaryStartIndex; (int) index != (int) this.m_iPrimaryIndex; index = (index + 1U) % this.m_iPrimaryMaxSize)
          this.m_PrimaryLogs[(int) index].Print(text);
        text.WriteLine();
        text.WriteLine("Secondary Debug Log");
        text.WriteLine();
        for (uint index = this.m_iSecondaryStartIndex; (int) index != (int) this.m_iSecondaryIndex; index = (index + 1U) % this.m_SecondaryMaxSize)
          this.m_SecondaryLogs[(int) index].Print(text);
        text.Close();
      }
      catch (Exception ex)
      {
        Trace.WriteLine(ex.Message);
      }
    }

    public enum LogType
    {
      Primary,
      Secondary,
    }
  }
}
