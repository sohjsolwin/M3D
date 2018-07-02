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
      m_sSaveFileName = filename;
      m_iPrimaryMaxSize = maxSize;
      m_SecondaryMaxSize = maxSize;
      Init();
      FileUtils.GrantAccess(m_sSaveFileName);
    }

    public void Init()
    {
      m_PrimaryLogs = new DebugLog[(int)m_iPrimaryMaxSize];
      m_SecondaryLogs = new DebugLog[(int)m_SecondaryMaxSize];
      Add("DebuggLogger Init()", "Initialize debug logger. Max size: " + m_iPrimaryMaxSize, DebugLogger.LogType.Primary);
    }

    public void Add(string functionName, string description, DebugLogger.LogType logType)
    {
      try
      {
        if (logType == DebugLogger.LogType.Primary)
        {
          m_PrimaryLogs[(int)m_iPrimaryIndex] = new DebugLog(functionName, description);
          m_iPrimaryIndex = (m_iPrimaryIndex + 1U) % m_iPrimaryMaxSize;
          if ((int)m_iPrimaryIndex == (int)m_iPrimaryStartIndex)
          {
            ++m_iPrimaryStartIndex;
            m_iPrimaryStartIndex %= m_iPrimaryMaxSize;
          }
        }
        else
        {
          m_SecondaryLogs[(int)m_iSecondaryIndex] = new DebugLog(functionName, description);
          m_iSecondaryIndex = (m_iSecondaryIndex + 1U) % m_SecondaryMaxSize;
          if ((int)m_iSecondaryIndex == (int)m_iSecondaryStartIndex)
          {
            ++m_iSecondaryStartIndex;
            m_iSecondaryStartIndex %= m_SecondaryMaxSize;
          }
        }
      }
      catch (Exception ex)
      {
        Trace.WriteLine(ex.Message);
      }
      Print(m_sSaveFileName);
    }

    public void Print(string fileName)
    {
      try
      {
        var text = (TextWriter) File.CreateText(fileName);
        text.WriteLine("Primary Debug Log");
        text.WriteLine();
        for (var index = m_iPrimaryStartIndex; (int) index != (int)m_iPrimaryIndex; index = (index + 1U) % m_iPrimaryMaxSize)
        {
          m_PrimaryLogs[(int) index].Print(text);
        }

        text.WriteLine();
        text.WriteLine("Secondary Debug Log");
        text.WriteLine();
        for (var index = m_iSecondaryStartIndex; (int) index != (int)m_iSecondaryIndex; index = (index + 1U) % m_SecondaryMaxSize)
        {
          m_SecondaryLogs[(int) index].Print(text);
        }

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
