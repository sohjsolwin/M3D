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
      m_bIsBinaryMode = bIsBinaryMode;
      FileStream fileStream;
      try
      {
        fileStream = new FileStream(gcodefilename, FileMode.Create);
      }
      catch (IOException ex)
      {
        return;
      }
      if (m_bIsBinaryMode)
      {
        writeBinary = new BinaryWriter((Stream) fileStream, Encoding.ASCII);
      }
      else
      {
        writeAscii = new StreamWriter((Stream) fileStream, Encoding.ASCII);
      }
    }

    public void Dispose()
    {
      Close();
    }

    public void Close()
    {
      if (writeBinary != null)
      {
        writeBinary.Close();
      }

      if (writeAscii == null)
      {
        return;
      }

      writeAscii.Close();
    }

    public bool Write(GCode code)
    {
      try
      {
        if (m_bIsBinaryMode)
        {
          writeBinary.Write(code.getBinary(2));
        }
        else
        {
          writeAscii.WriteLine(code.getAscii(false, false));
        }
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
