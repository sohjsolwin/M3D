namespace M3D.Spooling.Client
{
  public enum SpoolerResult
  {
    OK,
    Error,
    Fail,
    Fail_Connect,
    Fail_InvalidData,
    Fail_NotAvailable,
    Fail_PreviousCommandNotComplete,
    Fail_DoesNotHaveLock,
  }
}
