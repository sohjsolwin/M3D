// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Sockets.IBroadcastServer
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System;

namespace M3D.Spooling.Sockets
{
  public interface IBroadcastServer
  {
    void BroadcastMessage(string message);

    void SendMessageToClient(Guid client_guid, string message);
  }
}
