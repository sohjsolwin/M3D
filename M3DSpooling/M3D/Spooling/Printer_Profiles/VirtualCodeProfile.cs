// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Printer_Profiles.VirtualCodeProfile
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Core.Controllers;
using RepetierHost.model;
using System;
using System.Collections.Generic;

namespace M3D.Spooling.Printer_Profiles
{
  internal class VirtualCodeProfile
  {
    private Dictionary<int, VirtualCodeProfile.RunVirtualCode> code_dictionary;

    public VirtualCodeProfile()
    {
      this.code_dictionary = new Dictionary<int, VirtualCodeProfile.RunVirtualCode>();
      this.AddVirtualCode(576, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M576GetFilamentInformation));
      this.AddVirtualCode(578, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M578GetCurrentBedOffsets));
      this.AddVirtualCode(573, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M573GetBedLevelingValues));
      this.AddVirtualCode(572, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M572GetBackLash));
      this.AddVirtualCode(571, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M571SetBacklashConstants));
      this.AddVirtualCode(575, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M575SetFilament));
      this.AddVirtualCode(570, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M570SetFilamentUID));
      this.AddVirtualCode(577, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M577SetBedOffsets));
      this.AddVirtualCode(580, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M580SetBackLashSpeed));
      this.AddVirtualCode(581, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M581GetBackLashSpeed));
      this.AddVirtualCode(682, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M682SetLimitingSpeed));
      this.AddVirtualCode(683, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M683GetLimitingSpeed));
      this.AddVirtualCode(684, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M684PrintAllEepromValues));
      this.AddVirtualCode(303, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M303SetGantryClipsToOff));
      this.AddVirtualCode(304, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M304SetGantryClipsToOn));
      this.AddVirtualCode(1011, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M1011EnableBoundsChecking));
      this.AddVirtualCode(1012, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M1012DisableBoundsChecking));
      this.AddVirtualCode(1013, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M1013StopwatchStart));
      this.AddVirtualCode(1014, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M1014StopwatchStop));
      this.AddVirtualCode(5680, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M5680GetHoursUsed));
    }

    public bool ProcessVirtualCode(GCode gcode, FirmwareController connection)
    {
      if (!this.code_dictionary.ContainsKey((int) gcode.M))
        return false;
      this.code_dictionary[(int) gcode.M](gcode, connection);
      return true;
    }

    protected void AddVirtualCode(int m_code, VirtualCodeProfile.RunVirtualCode func)
    {
      if (this.code_dictionary.ContainsKey(m_code))
        throw new InvalidOperationException();
      this.code_dictionary.Add(m_code, func);
    }

    public delegate void RunVirtualCode(GCode gcode, FirmwareController connection);
  }
}
