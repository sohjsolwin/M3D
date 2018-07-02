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
      var nozzleSizeDialog = Resources.customNozzleSizeDialog;
      Init(guiHost, nozzleSizeDialog, new ButtonCallback(ButtonCallback));
      SetSize(540, byte.MaxValue);
      CenterHorizontallyInParent = true;
      CenterVerticallyInParent = true;
      m_OnDialogClose = OnDialogClose;
      m_oMessagebox = messagebox;
      m_iMinNozzleSize = iMinNozzleSize;
      m_iMaxNozzleSize = iMaxNozzleSize;
      m_oPrinter = printer;
      var childElement = (EditBoxWidget)childFrame.FindChildElement(100);
      if (childElement == null)
      {
        return;
      }

      var num = m_oPrinter.Info.extruder.iNozzleSizeMicrons / 1000f;
      childElement.Text = num.ToString();
    }

    public static void Show(GUIHost guiHost, PopupMessageBox messagebox, EventHandler<CustomNozzleSizeDialog.CustomNozzleSizeResult> OnDialogClose, int iMinNozzleSize, int iMaxNozzleSize, PrinterObject printer)
    {
      var nozzleSizeDialog = new CustomNozzleSizeDialog(guiHost, messagebox, OnDialogClose, iMinNozzleSize, iMaxNozzleSize, printer);
      guiHost.GlobalChildDialog += nozzleSizeDialog;
    }

    private void ButtonCallback(ButtonWidget button)
    {
      if (button.ID == 101)
      {
        var childElement = (EditBoxWidget)childFrame.FindChildElement(100);
        if (childElement == null)
        {
          m_oMessagebox.AddMessageToQueue("There was an error setting the nozzle size. Please try again.");
        }
        else
        {
          if (float.TryParse(childElement.Text, out var result))
          {
            var iNozzleSizeMicrons = (int)(result * 1000.0);
            if (iNozzleSizeMicrons < m_iMinNozzleSize || iNozzleSizeMicrons > m_iMaxNozzleSize)
            {
              m_oMessagebox.AddMessageToQueue(string.Format("Sorry, but the nozzle size must be between {0} and {1} mm.", (float)((double)m_iMinNozzleSize / 1000.0), (float)((double)m_iMaxNozzleSize / 1000.0)));
            }
            else
            {
              Close(iNozzleSizeMicrons, false);
            }
          }
          else
          {
            m_oMessagebox.AddMessageToQueue("Sorry, but the nozzle size is not a number.");
          }
        }
      }
      else
      {
        if (button.ID != 102)
        {
          return;
        }

        Close();
      }
    }

    private void Close(int iNozzleSizeMicrons, bool bCanceled)
    {
      m_OnDialogClose?.Invoke(this, new CustomNozzleSizeDialog.CustomNozzleSizeResult(iNozzleSizeMicrons, bCanceled, m_oPrinter));

      Host.GlobalChildDialog -= (this);
    }

    public override void Close()
    {
      Close(0, true);
    }

    public class CustomNozzleSizeResult
    {
      public CustomNozzleSizeResult(int iCustomNozzleSizeMicrons, bool bCanceled, PrinterObject printer)
      {
        this.bCanceled = bCanceled;
        this.iCustomNozzleSizeMicrons = iCustomNozzleSizeMicrons;
        Printer = printer;
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
