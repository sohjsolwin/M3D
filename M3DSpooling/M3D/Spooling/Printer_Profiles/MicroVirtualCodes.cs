namespace M3D.Spooling.Printer_Profiles
{
  internal class MicroVirtualCodes : VirtualCodeProfile
  {
    public MicroVirtualCodes()
    {
      AddVirtualCode(23975, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.SetExtruderCurrent500));
      AddVirtualCode(20904, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.SetExtruderCurrent660));
      AddVirtualCode(21914, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.SetExtruderCurrent660));
      AddVirtualCode(19007, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.SetFanConstantsHeineken));
      AddVirtualCode(18010, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.SetFanConstantsListener));
      AddVirtualCode(17013, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.SetFanConstantsShinZoo));
      AddVirtualCode(16007, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.SetFanConstantsXinyujie));
    }
  }
}
