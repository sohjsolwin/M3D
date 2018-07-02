using M3D.Spooling.Preprocessors.Foundation;
using System.Collections.Generic;

namespace M3D.Spooling.Printer_Profiles
{
  internal class PrinterPreprocessorProfile
  {
    public readonly List<IPreprocessor> preprocessor_list;

    public PrinterPreprocessorProfile(params IPreprocessor[] preprocessors)
    {
      preprocessor_list = new List<IPreprocessor>((IEnumerable<IPreprocessor>) preprocessors);
    }
  }
}
