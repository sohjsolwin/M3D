// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Controller.PrinterStatusDialogOrganizer
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.GUI.Forms;
using M3D.GUI.Views.Printer_View;
using M3D.Spooling.Client;
using System;
using System.Collections.Generic;

namespace M3D.GUI.Controller
{
  public class PrinterStatusDialogOrganizer
  {
    private GUIHost gui_host;
    private VerticalLayout layout;
    private ScrollFrame scroll_frame;
    private SpoolerConnection spooler_connection;
    private Form1 mainform;
    private ModelLoadingManager modelLoadingManager;
    private SettingsManager settingsManager;
    private PrinterView printerView;
    private PopupMessageBox messagebox;
    private List<PrinterStatusDialog> connected_printers;

    public PrinterStatusDialogOrganizer(SpoolerConnection spooler_connection, ModelLoadingManager modelLoadingManager, SettingsManager settingsManager, Form1 mainform, GUIHost gui_host, PrinterView printerView, PopupMessageBox messagebox)
    {
      this.spooler_connection = spooler_connection;
      this.modelLoadingManager = modelLoadingManager;
      this.settingsManager = settingsManager;
      this.mainform = mainform;
      this.messagebox = messagebox;
      this.printerView = printerView;
      this.gui_host = gui_host;
      this.spooler_connection.OnGotNewPrinter += new SpoolerClient.OnGotNewPrinterDel(this.OnGotNewPrinter);
      this.spooler_connection.OnPrinterDisconnected += new SpoolerClient.OnPrinterDisconnectedDel(this.OnPrinterDisconnected);
      this.connected_printers = new List<PrinterStatusDialog>();
      this.InitGUIElement(gui_host, (Element2D) printerView.GetEditFrame());
    }

    private void InitGUIElement(GUIHost gui_host, Element2D parent)
    {
      this.scroll_frame = new ScrollFrame(1234);
      this.scroll_frame.Init(gui_host);
      this.scroll_frame.Width = 450;
      this.scroll_frame.RelativeHeight = -1f;
      this.scroll_frame.Pane_Width = 400;
      this.scroll_frame.Pane_Height = 10;
      this.scroll_frame.X = -this.scroll_frame.Width;
      this.scroll_frame.Y = 64;
      ScrollFrame scrollFrame = this.scroll_frame;
      scrollFrame.OnControlMsgCallback = scrollFrame.OnControlMsgCallback + new OnControlMsgDelegate(this.OnControlMsg);
      this.layout = new VerticalLayout(12345);
      this.layout.SetSize(400, 10);
      this.layout.layoutMode = Layout.LayoutMode.ResizeLayoutToFitChildren;
      this.scroll_frame.AddChildElement((Element2D) this.layout);
      this.scroll_frame.Visible = false;
      parent.ChildList += (Element2D) this.scroll_frame;
    }

    private void OnControlMsg(Element2D the_control, ControlMsg msg, float xparam, float yparam)
    {
      if (the_control.ID != this.layout.ID || msg != ControlMsg.LAYOUT_RESIZED_BY_CHILDREN)
        return;
      this.scroll_frame.Pane_Height = this.layout.Height;
      int num = (int) ((double) this.scroll_frame.Parent.Height * 0.649999976158142);
      if (this.scroll_frame.Pane_Height > 100)
      {
        this.scroll_frame.Visible = true;
        if (this.scroll_frame.Pane_Height < num)
          this.scroll_frame.Height = this.scroll_frame.Pane_Height;
        else
          this.scroll_frame.Height = num;
      }
      else
        this.scroll_frame.Visible = false;
      this.scroll_frame.Refresh();
    }

    public void OnGotNewPrinter(Printer new_printer)
    {
      lock (this.connected_printers)
      {
        this.connected_printers.Add(new PrinterStatusDialog(new_printer, this.gui_host, (Element2D) this.layout, this.messagebox, this.mainform, this.settingsManager));
        if (new_printer.Info.current_job == null || this.printerView.IsModelLoaded() || (this.modelLoadingManager.LoadingNewModel || this.modelLoadingManager.OptimizingModel))
          return;
        this.spooler_connection.SelectPrinterBySerialNumber(new_printer.Info.serial_number.ToString());
        PrintDetails.PrintJobObjectViewDetails printerview_settings;
        if (!SettingsManager.LoadPrinterView(new_printer.Info.current_job.Params.jobGuid, out printerview_settings))
          return;
        this.modelLoadingManager.LoadPrinterView(printerview_settings);
      }
    }

    public void OnPrinterDisconnected(Printer new_printer)
    {
      lock (this.connected_printers)
      {
        try
        {
          for (int index = 0; index < this.connected_printers.Count; ++index)
          {
            if (this.connected_printers[index].Info.hardware.com_port == new_printer.Info.hardware.com_port)
            {
              this.connected_printers[index].OnDisconnect();
              this.connected_printers.RemoveAt(index);
              break;
            }
          }
        }
        catch (Exception ex)
        {
        }
      }
    }
  }
}
