// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Utils.GCodeFileReader
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
      FileStream readstream = (FileStream) null;
      try
      {
        readstream = new FileStream(gcodefilename, FileMode.Open, FileAccess.Read);
        if (new BinaryReader((Stream) readstream).PeekChar() > (int) sbyte.MaxValue)
          this.isbinary = true;
      }
      catch (Exception ex)
      {
        ErrorLogger.LogErrorMsg("Exception in GCodeReader.GCodeReader" + ex.Message, "Exception");
        this.m_bOpen.Value = false;
        return;
      }
      finally
      {
        if (readstream != null)
        {
          this.maxlines = !this.isbinary ? this.GetLineCountAscii(readstream, false) : this.GetLineCountBinary(readstream);
          readstream.Close();
        }
      }
      this.StartReading(gcodefilename);
      this.m_bOpen.Value = true;
    }

    public void Dispose()
    {
      if (this.readBinary != null)
        this.readBinary.Dispose();
      else if (this.readascii != null)
      {
        this.readascii.Dispose();
      }
      else
      {
        if (this.readstream == null)
          return;
        this.readstream.Dispose();
      }
    }

    public long GetFileSizeBytes()
    {
      if (this.readstream != null)
        return this.readstream.Length;
      return 0;
    }

    public long GetFilePositionBytes()
    {
      if (this.readstream != null)
        return this.readstream.Position;
      return 0;
    }

    public void Close()
    {
      if (this.readstream == null)
        return;
      this.readstream.Close();
    }

    public GCode GetNextLine(bool excludeComments)
    {
      if (this.endreached)
        return (GCode) null;
      GCode gcode = this.isbinary ? this.ReadGCodeLineBinary(this.readBinary) : this.ReadGCodeLineAscii(this.readascii, excludeComments);
      if (gcode != null)
        return gcode;
      this.endreached = true;
      return gcode;
    }

    public long MaxLines
    {
      get
      {
        return this.maxlines;
      }
    }

    public bool IsOpen
    {
      get
      {
        return this.m_bOpen.Value;
      }
    }

    private void StartReading(string gcodefilename)
    {
      try
      {
        this.readstream = new FileStream(gcodefilename, FileMode.Open, FileAccess.Read);
        if (this.isbinary)
          this.readBinary = new BinaryReader((Stream) this.readstream);
        else
          this.readascii = new StreamReader((Stream) this.readstream);
      }
      catch (Exception ex)
      {
        ErrorLogger.LogErrorMsg("Exception in GCodeReader.StartReading " + ex.Message, "Exception");
        this.endreached = true;
      }
    }

    private long GetLineCountBinary(FileStream readstream)
    {
      BinaryReader readBinary = new BinaryReader((Stream) readstream);
      int num = 0;
      while (this.ReadGCodeLineBinary(readBinary) != null)
        ++num;
      return (long) num;
    }

    private long GetLineCountAscii(FileStream readstream, bool excludeComments)
    {
      StreamReader readascii = new StreamReader((Stream) readstream);
      int num = 0;
      while (this.ReadGCodeLineAscii(readascii, excludeComments) != null)
        ++num;
      return (long) num;
    }

    private GCode ReadGCodeLineAscii(StreamReader readascii, bool excludeComments)
    {
      GCode gcode = new GCode();
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
          return (GCode) null;
        }
        if (line == null)
          return (GCode) null;
        gcode.Parse(line);
      }
      while (gcode.comment & excludeComments);
      return gcode;
    }

    private GCode ReadGCodeLineBinary(BinaryReader readBinary)
    {
      GCode gcode = new GCode();
      try
      {
        int num1 = (int) readBinary.ReadUInt16();
        ushort num2 = 0;
        bool flag1 = false;
        bool flag2 = (uint) (num1 & 32768) > 0U;
        int num3 = 0;
        if ((num1 & 4096) != 0)
        {
          flag1 = true;
          num2 = readBinary.ReadUInt16();
          if (flag2)
            num3 = (int) readBinary.ReadByte();
        }
        if ((num1 & 1) != 0)
          gcode.N = (int) readBinary.ReadUInt16();
        if ((num1 & 2) != 0)
          gcode.M = flag1 ? readBinary.ReadUInt16() : (ushort) readBinary.ReadByte();
        if ((num1 & 4) != 0)
          gcode.G = flag1 ? readBinary.ReadUInt16() : (ushort) readBinary.ReadByte();
        if ((num1 & 8) != 0)
          gcode.X = readBinary.ReadSingle();
        if ((num1 & 16) != 0)
          gcode.Y = readBinary.ReadSingle();
        if ((num1 & 32) != 0)
          gcode.Z = readBinary.ReadSingle();
        if ((num1 & 64) != 0)
          gcode.E = readBinary.ReadSingle();
        if ((num1 & 256) != 0)
          gcode.F = readBinary.ReadSingle();
        if ((num1 & 512) != 0)
          gcode.T = readBinary.ReadByte();
        if ((num1 & 1024) != 0)
          gcode.S = readBinary.ReadInt32();
        if ((num1 & 2048) != 0)
          gcode.P = readBinary.ReadInt32();
        if (flag1 && ((int) num2 & 1) != 0)
          gcode.I = readBinary.ReadSingle();
        if (flag1 && ((int) num2 & 2) != 0)
          gcode.J = readBinary.ReadSingle();
        if (flag1 && ((int) num2 & 4) != 0)
          gcode.R = readBinary.ReadSingle();
        if (flag2)
        {
          string str = "";
          int num4 = flag1 ? 1 : 0;
          for (int index = 0; index < num3; ++index)
            str += ((char) readBinary.ReadByte()).ToString();
        }
        int num5 = (int) readBinary.ReadByte();
        int num6 = (int) readBinary.ReadByte();
      }
      catch (EndOfStreamException ex)
      {
        return (GCode) null;
      }
      catch (Exception ex)
      {
        ErrorLogger.LogErrorMsg("Exception in GCodeReader.ReadGCodeLineBinary " + ex.Message, "Exception");
      }
      return gcode;
    }
  }
}
