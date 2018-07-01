// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.Print_Dialog_Widget.PrintDialogMainWindow
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      spooler_connection.OnSelectedPrinterChanged += new SpoolerConnection.SelectedPrinterChangedCallback(this.OnSelectedPrinterChanged);
      this.SetSize(750, 500);
      this.CenterHorizontallyInParent = true;
      this.CenterVerticallyInParent = true;
      this.frames.Add((IPrintDialogFrame) new PrintDialogFrame(1000, host, printerview, spooler_connection, message_box, modelloadingmanager, controller, this));
      this.frames.Add((IPrintDialogFrame) new SlicingFrame(1001, host, printerview, message_box, controller, this));
      this.frames.Add((IPrintDialogFrame) new PreparingToStartFrame(1002, host, printerview, message_box, recentPrints, this));
      this.frames.Add((IPrintDialogFrame) new PreSlicingFrame(1003, host, this));
      this.frames.Add((IPrintDialogFrame) new AdvancedPrintSettingsFrame(1004, host, message_box, controller, this));
      this.frames.Add((IPrintDialogFrame) new PrintingToFileFrame(1005, host, message_box, this));
      foreach (IPrintDialogFrame frame in this.frames)
      {
        frame.Visible = false;
        this.AddChildElement((Element2D) frame);
      }
    }

    public override ElementType GetElementType()
    {
      return ElementType.Element;
    }

    public void Show(PrintDialogWidgetFrames frame, PrintJobDetails details)
    {
      this.Visible = true;
      this.Enabled = true;
      this.SlicerConnection.SlicerSettings.ConfigureFromPrinterData((IPrinter) details.printer);
      this.host.GlobalChildDialog += (Element2D) this;
      this.ActivateFrame(frame, details);
    }

    public void CloseWindow()
    {
      if (this.current_frame != null)
      {
        this.current_frame.Visible = false;
        this.current_frame.OnDeactivate();
        this.current_frame = (IPrintDialogFrame) null;
      }
      this.Visible = false;
      if (this.host == null)
        return;
      this.host.GlobalChildDialog -= (Element2D) this;
    }

    public void ActivateFrame(PrintDialogWidgetFrames frame, PrintJobDetails details)
    {
      if (this.current_frame != null)
      {
        this.previous = (PrintDialogWidgetFrames) this.current_frame.ID;
        this.current_frame.Visible = false;
        this.current_frame.OnDeactivate();
        this.current_frame = (IPrintDialogFrame) null;
      }
      this.current_frame = this.frames[(int) (frame - 1000)];
      this.current_frame.Visible = true;
      this.current_frame.OnActivate(details);
    }

    public void ActivatePrevious(PrintJobDetails details)
    {
      this.ActivateFrame(this.previous, details);
    }

    public PrinterObject GetSelectedPrinter()
    {
      PrinterObject printerObject = this.spooler_connection.SelectedPrinter;
      if (printerObject == null || !printerObject.isConnected())
      {
        this.CloseWindow();
        printerObject = (PrinterObject) null;
      }
      return printerObject;
    }

    public void OnSelectedPrinterChanged(PrinterSerialNumber serial_number)
    {
      if (!this.Visible)
        return;
      this.CloseWindow();
    }

    public SmartSlicerSettingsBase SlicerSettings
    {
      get
      {
        return this.slicer_connection.SlicerSettings;
      }
    }

    public SlicerConnectionBase SlicerConnection
    {
      get
      {
        return this.slicer_connection;
      }
    }
  }
}
