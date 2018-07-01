// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Utils.GCodeBufferedFileReader
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
      this.m_oInternalReader = new GCodeFileReader(gcodefilename);
      if (!this.m_oInternalReader.IsOpen)
        return;
      for (ulong index = 0; index < ulFastForward; ++index)
      {
        GCode nextLine = this.m_oInternalReader.GetNextLine(true);
        if (nextLine == null)
        {
          this.m_oInternalReader.Close();
          return;
        }
        if (nextLine.hasE)
          fEStartingLocation = nextLine.E;
      }
      this.m_iCurPage = 0;
      this.m_iCurrentLineInPage = 0;
      this.m_agGcodeBuffer = new GCode[8, 1024];
      this.m_aiGcodeInBuffer = new int[8];
      for (int index = 0; index < 8; ++index)
        this.m_aiGcodeInBuffer[index] = 0;
      this.m_otFillThread = (Thread) null;
      this.m_aoFillThreadSync = new object[8];
      for (int index = 0; index < 8; ++index)
        this.m_aoFillThreadSync[index] = new object();
      this.StartRefillThread();
    }

    public void Close()
    {
      try
      {
        if (this.m_otFillThread != null)
          this.m_otFillThread.Abort();
      }
      catch (Exception ex)
      {
      }
      this.m_otFillThread = (Thread) null;
      this.m_oInternalReader.Close();
    }

    public GCode GetNextLine(bool excludeComments)
    {
      lock (this.m_aoFillThreadSync[this.m_iCurPage])
      {
        if (this.m_iCurrentLineInPage >= 1024)
        {
          this.m_iCurPage = (this.m_iCurPage + 1) % 8;
          this.m_iCurrentLineInPage = 0;
        }
      }
      int num1 = this.m_aiGcodeInBuffer[this.m_iCurPage];
      int currentLineInPage = this.m_iCurrentLineInPage;
      int num2 = 0;
      if (num1 <= num2)
        return (GCode) null;
      GCode gcode = this.m_agGcodeBuffer[this.m_iCurPage, currentLineInPage];
      ++this.m_iCurrentLineInPage;
      --this.m_aiGcodeInBuffer[this.m_iCurPage];
      return gcode;
    }

    public long MaxLines
    {
      get
      {
        return this.m_oInternalReader.MaxLines;
      }
    }

    public bool IsOpen
    {
      get
      {
        return this.m_oInternalReader.IsOpen;
      }
    }

    private void StartRefillThread()
    {
      this.m_iCurPage = 0;
      for (int pagenumber = 0; pagenumber < 8; ++pagenumber)
        this.FillBuffer(pagenumber);
      this.m_otFillThread = new Thread(new ThreadStart(this.FillBufferThread));
      this.m_otFillThread.Name = "Job Filler";
      this.m_otFillThread.IsBackground = true;
      this.m_otFillThread.Start();
    }

    private void FillBufferThread()
    {
      Thread.CurrentThread.CurrentCulture = PrinterCompatibleString.PRINTER_CULTURE;
      try
      {
        while (!this.EndOfFIle)
        {
          for (int index = 1; index < 8; ++index)
          {
            int pagenumber = (this.m_iCurPage + index) % 8;
            if (this.m_aiGcodeInBuffer[pagenumber] == 0)
            {
              this.FillBuffer(pagenumber);
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
      lock (this.m_aoFillThreadSync[pagenumber])
      {
        while (!this.EndOfFIle && this.m_aiGcodeInBuffer[pagenumber] < 1024)
        {
          GCode nextLine = this.m_oInternalReader.GetNextLine(true);
          if (nextLine == null)
            this.EndOfFIle = true;
          else
            this.AddGCodeToBuffer(nextLine, pagenumber);
        }
      }
    }

    private void AddGCodeToBuffer(GCode new_code, int pagenumber)
    {
      int index = this.m_aiGcodeInBuffer[pagenumber];
      this.m_agGcodeBuffer[pagenumber, index] = new_code;
      ++this.m_aiGcodeInBuffer[pagenumber];
    }

    public bool EndOfFIle
    {
      get
      {
        return this.m_bEndreached.Value;
      }
      set
      {
        this.m_bEndreached.Value = value;
      }
    }
  }
}
