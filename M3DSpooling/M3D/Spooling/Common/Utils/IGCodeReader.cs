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
