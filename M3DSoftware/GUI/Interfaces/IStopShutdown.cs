namespace M3D.GUI.Interfaces
{
  internal interface IStopShutdown
  {
    void CreateShutdownMessage(string msg);

    void DestroyShutdownMessage();
  }
}
