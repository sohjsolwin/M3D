namespace M3D.Spooling.StayAwakeAndShutdown_.Implementation
{
  internal interface IStayAwakeAndShutdown
  {
    void Shutdown();

    bool NeverSleep();

    void AllowSleep();

    void CreateShutdownMessage(string msg);

    void DestroyShutdownMessage();
  }
}
