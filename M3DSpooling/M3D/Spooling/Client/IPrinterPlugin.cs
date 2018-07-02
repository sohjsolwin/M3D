namespace M3D.Spooling.Client
{
  public interface IPrinterPlugin
  {
    void OnReceivedPluginMessage(string gcode, string result);

    string[] GetGCodes();
  }
}
