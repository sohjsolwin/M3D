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
