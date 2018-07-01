// Decompiled with JetBrains decompiler
// Type: M3D.GUI.AccessoriesDialog.CustomNozzleSizeDialog
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Dialogs;
using M3D.Properties;
using System;

namespace M3D.GUI.AccessoriesDialog
{
  internal class CustomNozzleSizeDialog : XMLFrame
  {
    private PopupMessageBox m_oMessagebox;
    private EventHandler<CustomNozzleSizeDialog.CustomNozzleSizeResult> m_OnDialogClose;
    private int m_iMaxNozzleSize;
    private int m_iMinNozzleSize;
    private PrinterObject m_oPrinter;

    private CustomNozzleSizeDialog(GUIHost guiHost, PopupMessageBox messagebox, EventHandler<CustomNozzleSizeDialog.CustomNozzleSizeResult> OnDialogClose, int iMinNozzleSize, int iMaxNozzleSize, PrinterObject printer)
    {
      string nozzleSizeDialog = Resources.customNozzleSizeDialog;
      this.Init(guiHost, nozzleSizeDialog, new ButtonCallback(this.ButtonCallback));
      this.SetSize(540, (int) byte.MaxValue);
      this.CenterHorizontallyInParent = true;
      this.CenterVerticallyInParent = true;
      this.m_OnDialogClose = OnDialogClose;
      this.m_oMessagebox = messagebox;
      this.m_iMinNozzleSize = iMinNozzleSize;
      this.m_iMaxNozzleSize = iMaxNozzleSize;
      this.m_oPrinter = printer;
      EditBoxWidget childElement = (EditBoxWidget) this.childFrame.FindChildElement(100);
      if (childElement == null)
        return;
      float num = (float) this.m_oPrinter.Info.extruder.iNozzleSizeMicrons / 1000f;
      childElement.Text = num.ToString();
    }

    public static void Show(GUIHost guiHost, PopupMessageBox messagebox, EventHandler<CustomNozzleSizeDialog.CustomNozzleSizeResult> OnDialogClose, int iMinNozzleSize, int iMaxNozzleSize, PrinterObject printer)
    {
      CustomNozzleSizeDialog nozzleSizeDialog = new CustomNozzleSizeDialog(guiHost, messagebox, OnDialogClose, iMinNozzleSize, iMaxNozzleSize, printer);
      guiHost.GlobalChildDialog += (Element2D) nozzleSizeDialog;
    }

    private void ButtonCallback(ButtonWidget button)
    {
      if (button.ID == 101)
      {
        EditBoxWidget childElement = (EditBoxWidget) this.childFrame.FindChildElement(100);
        if (childElement == null)
        {
          this.m_oMessagebox.AddMessageToQueue("There was an error setting the nozzle size. Please try again.");
        }
        else
        {
          float result;
          if (float.TryParse(childElement.Text, out result))
          {
            int iNozzleSizeMicrons = (int) ((double) result * 1000.0);
            if (iNozzleSizeMicrons < this.m_iMinNozzleSize || iNozzleSizeMicrons > this.m_iMaxNozzleSize)
              this.m_oMessagebox.AddMessageToQueue(string.Format("Sorry, but the nozzle size must be between {0} and {1} mm.", (object) (float) ((double) this.m_iMinNozzleSize / 1000.0), (object) (float) ((double) this.m_iMaxNozzleSize / 1000.0)));
            else
              this.Close(iNozzleSizeMicrons, false);
          }
          else
            this.m_oMessagebox.AddMessageToQueue("Sorry, but the nozzle size is not a number.");
        }
      }
      else
      {
        if (button.ID != 102)
          return;
        this.Close();
      }
    }

    private void Close(int iNozzleSizeMicrons, bool bCanceled)
    {
      if (this.m_OnDialogClose != null)
        this.m_OnDialogClose((object) this, new CustomNozzleSizeDialog.CustomNozzleSizeResult(iNozzleSizeMicrons, bCanceled, this.m_oPrinter));
      this.Host.GlobalChildDialog -= (Element2D) this;
    }

    public override void Close()
    {
      this.Close(0, true);
    }

    public class CustomNozzleSizeResult
    {
      public CustomNozzleSizeResult(int iCustomNozzleSizeMicrons, bool bCanceled, PrinterObject printer)
      {
        this.bCanceled = bCanceled;
        this.iCustomNozzleSizeMicrons = iCustomNozzleSizeMicrons;
        this.Printer = printer;
      }

      public bool bCanceled { get; private set; }

      public int iCustomNozzleSizeMicrons { get; private set; }

      public PrinterObject Printer { get; private set; }
    }

    private enum ControlIDs
    {
      NozzleSizeEditBox = 100, // 0x00000064
      OK = 101, // 0x00000065
      Cancel = 102, // 0x00000066
    }
  }
}
