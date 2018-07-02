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
