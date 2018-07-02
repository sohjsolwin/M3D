namespace M3D.Spooling.Common
{
  public enum PrinterLockStatus
  {
    WeOwnLocked,
    OurLockPending,
    OurReleasePending,
    LockedByOther,
    Unlocked,
  }
}
