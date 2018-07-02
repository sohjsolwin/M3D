using System;

namespace M3D.Spooling.Sockets
{
  public interface IBroadcastServer
  {
    void BroadcastMessage(string message);

    void SendMessageToClient(Guid client_guid, string message);
  }
}
