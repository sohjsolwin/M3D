// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Utils.IGCodeReader
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using RepetierHost.model;

namespace M3D.Spooling.Common.Utils
{
  internal interface IGCodeReader
  {
    void Close();

    GCode GetNextLine(bool excludeComments);

    long MaxLines { get; }

    bool IsOpen { get; }
  }
}
