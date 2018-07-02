namespace M3D.Spooling.Embedded_Firmware
{
  public class FirmwareDetails
  {
    public readonly uint firmware_version;
    public readonly uint firmware_crc;
    public readonly string embedded_firmware;

    public FirmwareDetails(uint firmware_version, uint firmware_crc, string embedded_firmware)
    {
      this.firmware_version = firmware_version;
      this.firmware_crc = firmware_crc;
      this.embedded_firmware = embedded_firmware;
    }
  }
}
