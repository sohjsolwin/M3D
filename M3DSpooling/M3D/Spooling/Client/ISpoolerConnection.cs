using M3D.Spooling.Core;
using System;

namespace M3D.Spooling.Client
{
  public abstract class ISpoolerConnection
  {
    public OnReceiveXMLFromSpooler XMLProcessor;

    public abstract void ShutdownConnection();

    public abstract SpoolerResult SendSpoolerMessageInternal(string message);

    public abstract bool UseNoSpoolerMode { get; set; }

    protected internal void OnRawMessage(string data)
    {
      try
      {
        var xml_message = !data.Contains("<EOF>") ? data : data.Substring(0, data.IndexOf("<EOF>"));
        if (!(xml_message != "<?>") || XMLProcessor == null)
        {
          return;
        }

        XMLProcessor(xml_message);
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in SpoolerClient.OnRawMessage " + ex.Message, ex);
      }
    }
  }
}
