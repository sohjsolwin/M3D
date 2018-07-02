using M3D.Spooling.Client;

namespace M3D.Spooler
{
  internal class HandlerTaskDesc
  {
    public AfterLockTask task;
    public SpoolerMessageHandler handler;
    public Printer printer;
    public bool hadlockbeforecall;
    public AfterLockTask previous_task;
    public int attempts;

    public HandlerTaskDesc(AfterLockTask task, SpoolerMessageHandler handler, Printer printer)
      : this(task, handler, printer, AfterLockTask.None, 0)
    {
    }

    public HandlerTaskDesc(AfterLockTask task, SpoolerMessageHandler handler, Printer printer, AfterLockTask previous_task, int attempts)
    {
      this.task = task;
      this.handler = handler;
      this.printer = printer;
      this.previous_task = previous_task;
      hadlockbeforecall = false;
      attempts = 0;
    }
  }
}
