namespace M3D.Spooling.Printer_Profiles
{
  internal class ProVirtualCodes : VirtualCodeProfile
  {
    public ProVirtualCodes()
    {
      AddVirtualCode(582, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M582SetNozzleWidth));
      AddVirtualCode(583, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M583GetNozzleWidth));
    }
  }
}
