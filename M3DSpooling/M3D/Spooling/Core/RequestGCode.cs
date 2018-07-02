namespace M3D.Spooling.Core
{
  internal enum RequestGCode
  {
    NonRequestGCode,
    M117GetInternalState,
    M576GetFilamentInfo,
    G30_32_30_ZSaveGcode,
    HiddenType,
    M114GetExtruderLocation,
  }
}
