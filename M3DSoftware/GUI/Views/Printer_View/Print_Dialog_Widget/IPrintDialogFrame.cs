// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.Print_Dialog_Widget.IPrintDialogFrame
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.Frames_and_Layouts;
using M3D.GUI.Controller;
using M3D.Slicer.General;

namespace M3D.GUI.Views.Printer_View.Print_Dialog_Widget
{
  internal abstract class IPrintDialogFrame : Frame
  {
    private PrintDialogMainWindow printDialogWindow;

    public IPrintDialogFrame(int ID, PrintDialogMainWindow printDialogWindow)
      : base(ID)
    {
      this.printDialogWindow = printDialogWindow;
    }

    public abstract void OnActivate(PrintJobDetails details);

    public abstract void OnDeactivate();

    public PrintDialogMainWindow PrintDialogWindow
    {
      get
      {
        return printDialogWindow;
      }
    }

    public SmartSlicerSettingsBase SlicerSettings
    {
      get
      {
        return printDialogWindow.SlicerSettings;
      }
    }

    public SlicerConnectionBase SlicerConnection
    {
      get
      {
        return printDialogWindow.SlicerConnection;
      }
    }

    public virtual PrinterObject SelectedPrinter
    {
      get
      {
        return printDialogWindow.GetSelectedPrinter();
      }
    }
  }
}
