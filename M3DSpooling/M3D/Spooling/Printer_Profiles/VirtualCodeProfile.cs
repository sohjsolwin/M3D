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
      code_dictionary = new Dictionary<int, VirtualCodeProfile.RunVirtualCode>();
      AddVirtualCode(576, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M576GetFilamentInformation));
      AddVirtualCode(578, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M578GetCurrentBedOffsets));
      AddVirtualCode(573, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M573GetBedLevelingValues));
      AddVirtualCode(572, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M572GetBackLash));
      AddVirtualCode(571, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M571SetBacklashConstants));
      AddVirtualCode(575, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M575SetFilament));
      AddVirtualCode(570, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M570SetFilamentUID));
      AddVirtualCode(577, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M577SetBedOffsets));
      AddVirtualCode(580, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M580SetBackLashSpeed));
      AddVirtualCode(581, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M581GetBackLashSpeed));
      AddVirtualCode(682, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M682SetLimitingSpeed));
      AddVirtualCode(683, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M683GetLimitingSpeed));
      AddVirtualCode(684, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M684PrintAllEepromValues));
      AddVirtualCode(303, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M303SetGantryClipsToOff));
      AddVirtualCode(304, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M304SetGantryClipsToOn));
      AddVirtualCode(1011, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M1011EnableBoundsChecking));
      AddVirtualCode(1012, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M1012DisableBoundsChecking));
      AddVirtualCode(1013, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M1013StopwatchStart));
      AddVirtualCode(1014, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M1014StopwatchStop));
      AddVirtualCode(5680, new VirtualCodeProfile.RunVirtualCode(StandardVirtualCodes.M5680GetHoursUsed));
    }

    public bool ProcessVirtualCode(GCode gcode, FirmwareController connection)
    {
      if (!code_dictionary.ContainsKey(gcode.M))
      {
        return false;
      }

      code_dictionary[gcode.M](gcode, connection);
      return true;
    }

    protected void AddVirtualCode(int m_code, VirtualCodeProfile.RunVirtualCode func)
    {
      if (code_dictionary.ContainsKey(m_code))
      {
        throw new InvalidOperationException();
      }

      code_dictionary.Add(m_code, func);
    }

    public delegate void RunVirtualCode(GCode gcode, FirmwareController connection);
  }
}
