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
      this.spooler_connection.OnGotNewPrinter += new SpoolerClient.OnGotNewPrinterDel(OnGotNewPrinter);
      this.spooler_connection.OnPrinterDisconnected += new SpoolerClient.OnPrinterDisconnectedDel(OnPrinterDisconnected);
      connected_printers = new List<PrinterStatusDialog>();
      InitGUIElement(gui_host, printerView.GetEditFrame());
    }

    private void InitGUIElement(GUIHost gui_host, Element2D parent)
    {
      scroll_frame = new ScrollFrame(1234);
      scroll_frame.Init(gui_host);
      scroll_frame.Width = 450;
      scroll_frame.RelativeHeight = -1f;
      scroll_frame.Pane_Width = 400;
      scroll_frame.Pane_Height = 10;
      scroll_frame.X = -scroll_frame.Width;
      scroll_frame.Y = 64;
      ScrollFrame scrollFrame = scroll_frame;
      scrollFrame.OnControlMsgCallback = scrollFrame.OnControlMsgCallback + new OnControlMsgDelegate(OnControlMsg);
      layout = new VerticalLayout(12345);
      layout.SetSize(400, 10);
      layout.layoutMode = Layout.LayoutMode.ResizeLayoutToFitChildren;
      scroll_frame.AddChildElement(layout);
      scroll_frame.Visible = false;
      parent.ChildList += scroll_frame;
    }

    private void OnControlMsg(Element2D the_control, ControlMsg msg, float xparam, float yparam)
    {
      if (the_control.ID != layout.ID || msg != ControlMsg.LAYOUT_RESIZED_BY_CHILDREN)
      {
        return;
      }

      scroll_frame.Pane_Height = layout.Height;
      var num = (int)(scroll_frame.Parent.Height * 0.649999976158142);
      if (scroll_frame.Pane_Height > 100)
      {
        scroll_frame.Visible = true;
        if (scroll_frame.Pane_Height < num)
        {
          scroll_frame.Height = scroll_frame.Pane_Height;
        }
        else
        {
          scroll_frame.Height = num;
        }
      }
      else
      {
        scroll_frame.Visible = false;
      }

      scroll_frame.Refresh();
    }

    public void OnGotNewPrinter(Printer new_printer)
    {
      lock (connected_printers)
      {
        connected_printers.Add(new PrinterStatusDialog(new_printer, gui_host, layout, messagebox, mainform, settingsManager));
        if (new_printer.Info.current_job == null || printerView.IsModelLoaded() || (modelLoadingManager.LoadingNewModel || modelLoadingManager.OptimizingModel))
        {
          return;
        }

        spooler_connection.SelectPrinterBySerialNumber(new_printer.Info.serial_number.ToString());
        if (!SettingsManager.LoadPrinterView(new_printer.Info.current_job.Params.jobGuid, out PrintDetails.PrintJobObjectViewDetails printerview_settings))
        {
          return;
        }

        modelLoadingManager.LoadPrinterView(printerview_settings);
      }
    }

    public void OnPrinterDisconnected(Printer new_printer)
    {
      lock (connected_printers)
      {
        try
        {
          for (var index = 0; index < connected_printers.Count; ++index)
          {
            if (connected_printers[index].Info.hardware.com_port == new_printer.Info.hardware.com_port)
            {
              connected_printers[index].OnDisconnect();
              connected_printers.RemoveAt(index);
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
