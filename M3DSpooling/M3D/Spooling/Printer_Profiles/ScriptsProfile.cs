﻿using M3D.Spooling.Common;
using M3D.Spooling.Core.Controllers;

namespace M3D.Spooling.Printer_Profiles
{
  internal abstract class ScriptsProfile
  {
    public abstract void StartupScript(IPublicFirmwareController connection, PrinterInfo info);
  }
}
