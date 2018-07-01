// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Preprocessors.Foundation.IPreprocessor
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using M3D.Spooling.Printer_Profiles;

namespace M3D.Spooling.Preprocessors.Foundation
{
  public abstract class IPreprocessor
  {
    internal abstract bool ProcessGCode(GCodeFileReader input_reader, GCodeFileWriter output_writer, Calibration calibration, JobDetails jobdetails, InternalPrinterProfile printerProfile);

    public abstract string Name { get; }
  }
}
