using M3D.Spooling.Core;
using RepetierHost.model;
using System;
using System.Threading;

namespace M3D.Spooling.Common.Utils
{
  internal class GCodeBufferedFileReader : IGCodeReader
  {
    private ThreadSafeVariable<bool> m_bEndreached = new ThreadSafeVariable<bool>(false);
    private GCodeFileReader m_oInternalReader;
    private Thread m_otFillThread;
    private object[] m_aoFillThreadSync;
    private GCode[,] m_agGcodeBuffer;
    private int[] m_aiGcodeInBuffer;
    private int m_iCurPage;
    private int m_iCurrentLineInPage;
    private const int BUFFER_SIZE = 1024;
    private const int NUM_PAGES = 8;

    public GCodeBufferedFileReader(string gcodefilename, ulong ulFastForward, out float fEStartingLocation)
    {
      fEStartingLocation = 0.0f;
      m_oInternalReader = new GCodeFileReader(gcodefilename);
      if (!m_oInternalReader.IsOpen)
      {
        return;
      }

      for (ulong index = 0; index < ulFastForward; ++index)
      {
        GCode nextLine = m_oInternalReader.GetNextLine(true);
        if (nextLine == null)
        {
          m_oInternalReader.Close();
          return;
        }
        if (nextLine.HasE)
        {
          fEStartingLocation = nextLine.E;
        }
      }
      m_iCurPage = 0;
      m_iCurrentLineInPage = 0;
      m_agGcodeBuffer = new GCode[8, 1024];
      m_aiGcodeInBuffer = new int[8];
      for (var index = 0; index < 8; ++index)
      {
        m_aiGcodeInBuffer[index] = 0;
      }

      m_otFillThread = null;
      m_aoFillThreadSync = new object[8];
      for (var index = 0; index < 8; ++index)
      {
        m_aoFillThreadSync[index] = new object();
      }

      StartRefillThread();
    }

    public void Close()
    {
      try
      {
        if (m_otFillThread != null)
        {
          m_otFillThread.Abort();
        }
      }
      catch (Exception ex)
      {
      }
      m_otFillThread = null;
      m_oInternalReader.Close();
    }

    public GCode GetNextLine(bool excludeComments)
    {
      lock (m_aoFillThreadSync[m_iCurPage])
      {
        if (m_iCurrentLineInPage >= 1024)
        {
          m_iCurPage = (m_iCurPage + 1) % 8;
          m_iCurrentLineInPage = 0;
        }
      }
      var num1 = m_aiGcodeInBuffer[m_iCurPage];
      var currentLineInPage = m_iCurrentLineInPage;
      var num2 = 0;
      if (num1 <= num2)
      {
        return null;
      }

      GCode gcode = m_agGcodeBuffer[m_iCurPage, currentLineInPage];
      ++m_iCurrentLineInPage;
      --m_aiGcodeInBuffer[m_iCurPage];
      return gcode;
    }

    public long MaxLines
    {
      get
      {
        return m_oInternalReader.MaxLines;
      }
    }

    public bool IsOpen
    {
      get
      {
        return m_oInternalReader.IsOpen;
      }
    }

    private void StartRefillThread()
    {
      m_iCurPage = 0;
      for (var pagenumber = 0; pagenumber < 8; ++pagenumber)
      {
        FillBuffer(pagenumber);
      }

      m_otFillThread = new Thread(new ThreadStart(FillBufferThread))
      {
        Name = "Job Filler",
        IsBackground = true
      };
      m_otFillThread.Start();
    }

    private void FillBufferThread()
    {
      Thread.CurrentThread.CurrentCulture = PrinterCompatibleString.PRINTER_CULTURE;
      try
      {
        while (!EndOfFIle)
        {
          for (var index = 1; index < 8; ++index)
          {
            var pagenumber = (m_iCurPage + index) % 8;
            if (m_aiGcodeInBuffer[pagenumber] == 0)
            {
              FillBuffer(pagenumber);
              Thread.Sleep(1);
            }
          }
          Thread.Sleep(1000);
        }
      }
      catch (ThreadAbortException ex)
      {
        throw ex;
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("PrinterJob::FillBufferThread", ex);
      }
    }

    private void FillBuffer(int pagenumber)
    {
      lock (m_aoFillThreadSync[pagenumber])
      {
        while (!EndOfFIle && m_aiGcodeInBuffer[pagenumber] < 1024)
        {
          GCode nextLine = m_oInternalReader.GetNextLine(true);
          if (nextLine == null)
          {
            EndOfFIle = true;
          }
          else
          {
            AddGCodeToBuffer(nextLine, pagenumber);
          }
        }
      }
    }

    private void AddGCodeToBuffer(GCode new_code, int pagenumber)
    {
      var index = m_aiGcodeInBuffer[pagenumber];
      m_agGcodeBuffer[pagenumber, index] = new_code;
      ++m_aiGcodeInBuffer[pagenumber];
    }

    public bool EndOfFIle
    {
      get
      {
        return m_bEndreached.Value;
      }
      set
      {
        m_bEndreached.Value = value;
      }
    }
  }
}
