// Decompiled with JetBrains decompiler
// Type: M3D.Spooler.HandlerTaskDesc
// Assembly: M3DSpooler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 88304559-4DEB-43F9-B51D-EEDFDC1EF62E
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\Resources\Spooler\M3DSpooler.exe

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
