// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Utils.GCodeFileWriter
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Core;
using RepetierHost.model;
using System;
using System.IO;
using System.Text;

namespace M3D.Spooling.Common.Utils
{
  internal class GCodeFileWriter : IDisposable
  {
    private const bool DEFAULT_MODE_BINARY = false;
    private bool m_bIsBinaryMode;
    private BinaryWriter writeBinary;
    private StreamWriter writeAscii;

    public GCodeFileWriter(string gcodefilename)
      : this(gcodefilename, false)
    {
    }

    public GCodeFileWriter(string gcodefilename, bool bIsBinaryMode)
    {
      this.m_bIsBinaryMode = bIsBinaryMode;
      FileStream fileStream;
      try
      {
        fileStream = new FileStream(gcodefilename, FileMode.Create);
      }
      catch (IOException ex)
      {
        return;
      }
      if (this.m_bIsBinaryMode)
        this.writeBinary = new BinaryWriter((Stream) fileStream, Encoding.ASCII);
      else
        this.writeAscii = new StreamWriter((Stream) fileStream, Encoding.ASCII);
    }

    public void Dispose()
    {
      this.Close();
    }

    public void Close()
    {
      if (this.writeBinary != null)
        this.writeBinary.Close();
      if (this.writeAscii == null)
        return;
      this.writeAscii.Close();
    }

    public bool Write(GCode code)
    {
      try
      {
        if (this.m_bIsBinaryMode)
          this.writeBinary.Write(code.getBinary(2));
        else
          this.writeAscii.WriteLine(code.getAscii(false, false));
      }
      catch (Exception ex)
      {
        ErrorLogger.LogErrorMsg("Exception in GCodeWriter.Write " + ex.Message, "Exception");
        return false;
      }
      return true;
    }
  }
}
