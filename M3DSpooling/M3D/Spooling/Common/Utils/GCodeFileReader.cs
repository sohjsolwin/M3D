using M3D.Spooling.Core;
using RepetierHost.model;
using System;
using System.IO;

namespace M3D.Spooling.Common.Utils
{
  internal class GCodeFileReader : IGCodeReader, IDisposable
  {
    private ThreadSafeVariable<bool> m_bOpen = new ThreadSafeVariable<bool>(false);
    private bool isbinary;
    private bool endreached;
    private FileStream readstream;
    private StreamReader readascii;
    private BinaryReader readBinary;
    private long maxlines;

    public GCodeFileReader(string gcodefilename)
    {
      var readstream = (FileStream) null;
      try
      {
        readstream = new FileStream(gcodefilename, FileMode.Open, FileAccess.Read);
        if (new BinaryReader(readstream).PeekChar() > sbyte.MaxValue)
        {
          isbinary = true;
        }
      }
      catch (Exception ex)
      {
        ErrorLogger.LogErrorMsg("Exception in GCodeReader.GCodeReader" + ex.Message, "Exception");
        m_bOpen.Value = false;
        return;
      }
      finally
      {
        if (readstream != null)
        {
          maxlines = !isbinary ? GetLineCountAscii(readstream, false) : GetLineCountBinary(readstream);
          readstream.Close();
        }
      }
      StartReading(gcodefilename);
      m_bOpen.Value = true;
    }

    public void Dispose()
    {
      if (readBinary != null)
      {
        readBinary.Dispose();
      }
      else if (readascii != null)
      {
        readascii.Dispose();
      }
      else
      {
        if (readstream == null)
        {
          return;
        }

        readstream.Dispose();
      }
    }

    public long GetFileSizeBytes()
    {
      if (readstream != null)
      {
        return readstream.Length;
      }

      return 0;
    }

    public long GetFilePositionBytes()
    {
      if (readstream != null)
      {
        return readstream.Position;
      }

      return 0;
    }

    public void Close()
    {
      if (readstream == null)
      {
        return;
      }

      readstream.Close();
    }

    public GCode GetNextLine(bool excludeComments)
    {
      if (endreached)
      {
        return null;
      }

      GCode gcode = isbinary ? ReadGCodeLineBinary(readBinary) : ReadGCodeLineAscii(readascii, excludeComments);
      if (gcode != null)
      {
        return gcode;
      }

      endreached = true;
      return gcode;
    }

    public long MaxLines
    {
      get
      {
        return maxlines;
      }
    }

    public bool IsOpen
    {
      get
      {
        return m_bOpen.Value;
      }
    }

    private void StartReading(string gcodefilename)
    {
      try
      {
        readstream = new FileStream(gcodefilename, FileMode.Open, FileAccess.Read);
        if (isbinary)
        {
          readBinary = new BinaryReader(readstream);
        }
        else
        {
          readascii = new StreamReader(readstream);
        }
      }
      catch (Exception ex)
      {
        ErrorLogger.LogErrorMsg("Exception in GCodeReader.StartReading " + ex.Message, "Exception");
        endreached = true;
      }
    }

    private long GetLineCountBinary(FileStream readstream)
    {
      var readBinary = new BinaryReader(readstream);
      var num = 0;
      while (ReadGCodeLineBinary(readBinary) != null)
      {
        ++num;
      }

      return num;
    }

    private long GetLineCountAscii(FileStream readstream, bool excludeComments)
    {
      var readascii = new StreamReader(readstream);
      var num = 0;
      while (ReadGCodeLineAscii(readascii, excludeComments) != null)
      {
        ++num;
      }

      return num;
    }

    private GCode ReadGCodeLineAscii(StreamReader readascii, bool excludeComments)
    {
      var gcode = new GCode();
      do
      {
        string line;
        try
        {
          line = readascii.ReadLine();
        }
        catch (Exception ex)
        {
          ErrorLogger.LogErrorMsg("Exception in GCodeReader.StartReading 1 " + ex.Message, "Exception");
          return null;
        }
        if (line == null)
        {
          return null;
        }

        gcode.Parse(line);
      }
      while (gcode.comment & excludeComments);
      return gcode;
    }

    private GCode ReadGCodeLineBinary(BinaryReader readBinary)
    {
      var gcode = new GCode();
      try
      {
        var num1 = (int) readBinary.ReadUInt16();
        ushort num2 = 0;
        var flag1 = false;
        var flag2 = (uint) (num1 & 32768) > 0U;
        var num3 = 0;
        if ((num1 & 4096) != 0)
        {
          flag1 = true;
          num2 = readBinary.ReadUInt16();
          if (flag2)
          {
            num3 = readBinary.ReadByte();
          }
        }
        if ((num1 & 1) != 0)
        {
          gcode.N = readBinary.ReadUInt16();
        }

        if ((num1 & 2) != 0)
        {
          gcode.M = flag1 ? readBinary.ReadUInt16() : readBinary.ReadByte();
        }

        if ((num1 & 4) != 0)
        {
          gcode.G = flag1 ? readBinary.ReadUInt16() : readBinary.ReadByte();
        }

        if ((num1 & 8) != 0)
        {
          gcode.X = readBinary.ReadSingle();
        }

        if ((num1 & 16) != 0)
        {
          gcode.Y = readBinary.ReadSingle();
        }

        if ((num1 & 32) != 0)
        {
          gcode.Z = readBinary.ReadSingle();
        }

        if ((num1 & 64) != 0)
        {
          gcode.E = readBinary.ReadSingle();
        }

        if ((num1 & 256) != 0)
        {
          gcode.F = readBinary.ReadSingle();
        }

        if ((num1 & 512) != 0)
        {
          gcode.T = readBinary.ReadByte();
        }

        if ((num1 & 1024) != 0)
        {
          gcode.S = readBinary.ReadInt32();
        }

        if ((num1 & 2048) != 0)
        {
          gcode.P = readBinary.ReadInt32();
        }

        if (flag1 && (num2 & 1) != 0)
        {
          gcode.I = readBinary.ReadSingle();
        }

        if (flag1 && (num2 & 2) != 0)
        {
          gcode.J = readBinary.ReadSingle();
        }

        if (flag1 && (num2 & 4) != 0)
        {
          gcode.R = readBinary.ReadSingle();
        }

        if (flag2)
        {
          var str = "";
          var num4 = flag1 ? 1 : 0;
          for (var index = 0; index < num3; ++index)
          {
            str += ((char) readBinary.ReadByte()).ToString();
          }
        }
        var num5 = (int) readBinary.ReadByte();
        var num6 = (int) readBinary.ReadByte();
      }
      catch (EndOfStreamException ex)
      {
        return null;
      }
      catch (Exception ex)
      {
        ErrorLogger.LogErrorMsg("Exception in GCodeReader.ReadGCodeLineBinary " + ex.Message, "Exception");
      }
      return gcode;
    }
  }
}
