// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Client.ISpoolerConnection
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
