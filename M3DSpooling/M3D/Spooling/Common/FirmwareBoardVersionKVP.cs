namespace M3D.Spooling.Common
{
  public struct FirmwareBoardVersionKVP
  {
    public FirmwareBoardVersionKVP(char boardID, uint firmwareVersion)
    {
      BoardID = boardID;
      FirmwareVersion = firmwareVersion;
    }

    public char BoardID { get; set; }

    public uint FirmwareVersion { get; set; }

    public override string ToString()
    {
      var str = FirmwareVersion.ToString();
      return BoardID.ToString() + " - Firmware Version: " + str.Substring(0, 4) + "-" + str.Substring(4, 2) + "-" + str.Substring(6, 2) + "-" + str.Substring(8, 2);
    }
  }
}
