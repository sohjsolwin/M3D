using M3D.Spooling.Common;
using M3D.Spooling.Printer_Profiles;
using System;

namespace M3D.Spooling.Core.Controllers.PrintJobs
{
  internal class CreationParameters
  {
    public PrinterInfo printerInfo;
    public InternalPrinterProfile printerProfile;
    public EventHandler<JobCreateResult> onProcessFinished;

    public CreationParameters(PrinterInfo printerInfo, InternalPrinterProfile printerProfile, EventHandler<JobCreateResult> onProcessFinished)
    {
      this.printerInfo = printerInfo;
      this.printerProfile = printerProfile;
      this.onProcessFinished = onProcessFinished;
    }
  }
}
