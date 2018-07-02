using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.GUI.Views.Library_View;
using M3D.Slicer.General;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using System.Collections.Generic;

namespace M3D.GUI.Views.Printer_View.Print_Dialog_Widget
{
  internal class PrintDialogMainWindow : Frame
  {
    private List<IPrintDialogFrame> frames = new List<IPrintDialogFrame>();
    private PrintDialogWidgetFrames previous = PrintDialogWidgetFrames.PrintDialogFrame;
    private IPrintDialogFrame current_frame;
    private GUIHost host;
    private SpoolerConnection spooler_connection;
    private SlicerConnectionBase slicer_connection;

    public PrintDialogMainWindow(int ID, GUIHost host, PrinterView printerview, SpoolerConnection spooler_connection, PopupMessageBox message_box, ModelLoadingManager modelloadingmanager, SlicerConnectionBase slicer_connection, RecentPrintsTab recentPrints, SettingsManager controller)
      : base(ID)
    {
      this.host = host;
      this.spooler_connection = spooler_connection;
      this.slicer_connection = slicer_connection;
      spooler_connection.OnSelectedPrinterChanged += new SpoolerConnection.SelectedPrinterChangedCallback(OnSelectedPrinterChanged);
      SetSize(750, 500);
      CenterHorizontallyInParent = true;
      CenterVerticallyInParent = true;
      frames.Add(new PrintDialogFrame(1000, host, printerview, spooler_connection, message_box, modelloadingmanager, controller, this));
      frames.Add(new SlicingFrame(1001, host, printerview, message_box, controller, this));
      frames.Add(new PreparingToStartFrame(1002, host, printerview, message_box, recentPrints, this));
      frames.Add(new PreSlicingFrame(1003, host, this));
      frames.Add(new AdvancedPrintSettingsFrame(1004, host, message_box, controller, this));
      frames.Add(new PrintingToFileFrame(1005, host, message_box, this));
      foreach (IPrintDialogFrame frame in frames)
      {
        frame.Visible = false;
        AddChildElement(frame);
      }
    }

    public override ElementType GetElementType()
    {
      return ElementType.Element;
    }

    public void Show(PrintDialogWidgetFrames frame, PrintJobDetails details)
    {
      Visible = true;
      Enabled = true;
      SlicerConnection.SlicerSettings.ConfigureFromPrinterData(details.printer);
      host.GlobalChildDialog += (this);
      ActivateFrame(frame, details);
    }

    public void CloseWindow()
    {
      if (current_frame != null)
      {
        current_frame.Visible = false;
        current_frame.OnDeactivate();
        current_frame = null;
      }
      Visible = false;
      if (host == null)
      {
        return;
      }

      host.GlobalChildDialog -= (this);
    }

    public void ActivateFrame(PrintDialogWidgetFrames frame, PrintJobDetails details)
    {
      if (current_frame != null)
      {
        previous = (PrintDialogWidgetFrames)current_frame.ID;
        current_frame.Visible = false;
        current_frame.OnDeactivate();
        current_frame = null;
      }
      current_frame = frames[(int) (frame - 1000)];
      current_frame.Visible = true;
      current_frame.OnActivate(details);
    }

    public void ActivatePrevious(PrintJobDetails details)
    {
      ActivateFrame(previous, details);
    }

    public PrinterObject GetSelectedPrinter()
    {
      PrinterObject printerObject = spooler_connection.SelectedPrinter;
      if (printerObject == null || !printerObject.IsConnected())
      {
        CloseWindow();
        printerObject = null;
      }
      return printerObject;
    }

    public void OnSelectedPrinterChanged(PrinterSerialNumber serial_number)
    {
      if (!Visible)
      {
        return;
      }

      CloseWindow();
    }

    public SmartSlicerSettingsBase SlicerSettings
    {
      get
      {
        return slicer_connection.SlicerSettings;
      }
    }

    public SlicerConnectionBase SlicerConnection
    {
      get
      {
        return slicer_connection;
      }
    }
  }
}
