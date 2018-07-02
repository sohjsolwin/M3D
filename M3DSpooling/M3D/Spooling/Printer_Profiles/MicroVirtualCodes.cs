// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Printer_Profiles.MicroVirtualCodes
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
