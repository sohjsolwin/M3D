// Decompiled with JetBrains decompiler
// Type: M3D.MultiPrinterDialogWidget
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.GUI.Forms;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using OpenTK.Graphics;
using QuickFont;
using System;
using System.Collections.Generic;

namespace M3D
{
  public class MultiPrinterDialogWidget : BorderedImageFrame
  {
    private int temp_exception_count;
    private int lastpage_count;
    private List<PrinterInfo> printer_list;
    private SpoolerConnection spooler_connection;
    private SettingsManager main_controller;
    private PopupMessageBox messagebox;
    private Frame navigation;
    private ButtonWidget navigation_left;
    private ButtonWidget navigation_right;
    private ButtonWidget[] pagebuttons;
    private GridLayout PrinterGrid;
    private GUIHost host;
    public int window_width;
    public int window_height;

    public MultiPrinterDialogWidget(int ID, GUIHost host, SettingsManager main_controller, PopupMessageBox messagebox, SpoolerConnection spooler_connection)
      : base(ID, (Element2D) null)
    {
      this.main_controller = main_controller;
      this.messagebox = messagebox;
      this.spooler_connection = spooler_connection;
      this.printer_list = new List<PrinterInfo>();
      spooler_connection.OnPrintProcessChanged += new SpoolerClient.OnPrintProcessDel(this.OnPrintProcessChanged);
      this.Init(host);
    }

    private void Init(GUIHost host)
    {
      this.host = host;
      this.Init(host, "guicontrols", 640f, 320f, 704f, 383f, 41, 8, 64, 35, 8, 64);
      this.SetSize(792, 356);
      TextWidget textWidget = new TextWidget(100);
      textWidget.SetPosition(50, 2);
      textWidget.SetSize(500, 35);
      textWidget.Text = "Multi-Printer Options";
      textWidget.Alignment = QFontAlignment.Left;
      textWidget.Size = FontSize.Large;
      textWidget.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      this.AddChildElement((Element2D) textWidget);
      ButtonWidget buttonWidget = new ButtonWidget(1000);
      buttonWidget.X = -40;
      buttonWidget.Y = 4;
      buttonWidget.SetSize(32, 32);
      buttonWidget.Text = "";
      buttonWidget.TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f);
      buttonWidget.TextDownColor = new Color4(1f, 1f, 1f, 1f);
      buttonWidget.TextOverColor = new Color4(0.161f, 0.79f, 0.95f, 1f);
      buttonWidget.Alignment = QFontAlignment.Left;
      buttonWidget.Init(host, "guicontrols", 704f, 320f, 735f, 351f, 736f, 320f, 767f, 351f, 704f, 352f, 735f, 383f);
      buttonWidget.DontMove = true;
      buttonWidget.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.AddChildElement((Element2D) buttonWidget);
      this.navigation = new Frame(0, (Element2D) null);
      this.navigation_left = new ButtonWidget(1005, (Element2D) null);
      this.navigation_left.Text = "";
      this.navigation_left.X = 16;
      this.navigation_left.Y = 0;
      this.navigation_left.Width = 32;
      this.navigation_left.Height = 32;
      this.navigation_left.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.navigation_left.Init(host, "guicontrols", 608f, 0.0f, 639f, 31f, 640f, 0.0f, 671f, 31f, 672f, 0.0f, 703f, 31f, 704f, 0.0f, 735f, 31f);
      this.navigation_right = new ButtonWidget(1006, (Element2D) null);
      this.navigation_right.Text = "";
      this.navigation_right.X = -48;
      this.navigation_right.Y = 0;
      this.navigation_right.Width = 32;
      this.navigation_right.Height = 32;
      this.navigation_right.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.navigation_right.Init(host, "guicontrols", 608f, 32f, 639f, 63f, 640f, 32f, 671f, 63f, 672f, 32f, 703f, 63f, 704f, 32f, 735f, 63f);
      this.pagebuttons = new ButtonWidget[31];
      for (int ID = 1032; ID < 1062; ++ID)
      {
        int index = ID - 1032;
        this.pagebuttons[index] = new ButtonWidget(ID, (Element2D) null);
        this.pagebuttons[index].Text = "";
        this.pagebuttons[index].X = 48 + (ID - 1032) * 24;
        this.pagebuttons[index].Y = 8;
        this.pagebuttons[index].Width = 16;
        this.pagebuttons[index].Height = 16;
        this.pagebuttons[index].SetCallback(new ButtonCallback(this.MyButtonCallback));
        this.pagebuttons[index].Init(host, "guicontrols", 448f, 192f, 463f, 208f, 480f, 192f, 495f, 208f, 464f, 192f, 479f, 208f);
        this.pagebuttons[index].DontMove = true;
        this.pagebuttons[index].GroupID = 1;
        this.pagebuttons[index].ClickType = ButtonType.Checkable;
        this.pagebuttons[index].Visible = false;
        this.navigation.AddChildElement((Element2D) this.pagebuttons[index]);
      }
      this.navigation.AddChildElement((Element2D) this.navigation_left);
      this.navigation.AddChildElement((Element2D) this.navigation_right);
      this.navigation.RelativeWidth = 0.95f;
      this.navigation.Height = 32;
      this.navigation.SetPosition(0, -50);
      this.navigation.CenterHorizontallyInParent = true;
      this.AddChildElement((Element2D) this.navigation);
      this.PrinterGrid = new GridLayout(1);
      this.PrinterGrid.ColumnWidth = 130;
      this.PrinterGrid.RowHeight = 150;
      this.PrinterGrid.BorderWidth = 0;
      this.PrinterGrid.BorderHeight = 0;
      this.PrinterGrid.RelativeHeight = 0.8f;
      this.PrinterGrid.RelativeWidth = 0.8f;
      this.PrinterGrid.SetPosition(0, 48);
      this.PrinterGrid.CenterHorizontallyInParent = true;
      this.AddChildElement((Element2D) this.PrinterGrid);
      Sprite.pixel_perfect = false;
      this.Visible = false;
    }

    public override void OnParentResize()
    {
      base.OnParentResize();
    }

    public override void OnUpdate()
    {
      base.OnUpdate();
      if (this.spooler_connection.CopyPrinterList(ref this.printer_list))
        return;
      this.PrinterGrid.Clear();
      int num = 0;
      foreach (PrinterInfo printer in this.printer_list)
      {
        ButtonWidget buttonWidget1 = new ButtonWidget(1064 + num, (Element2D) null);
        ImageResourceMapping.PixelCoordinate pixelCoordinate = ImageResourceMapping.PrinterColorPosition(printer.serial_number.ToString());
        buttonWidget1.Init(this.host, "extendedcontrols", (float) pixelCoordinate.u0, (float) pixelCoordinate.v0, (float) pixelCoordinate.u1, (float) pixelCoordinate.v1, (float) pixelCoordinate.u0, (float) pixelCoordinate.v0, (float) pixelCoordinate.u1, (float) pixelCoordinate.v1, (float) pixelCoordinate.u0, (float) pixelCoordinate.v0, (float) pixelCoordinate.u1, (float) pixelCoordinate.v1);
        buttonWidget1.DontMove = true;
        buttonWidget1.Text = this.GetPrinterLabelText(printer);
        buttonWidget1.Color = new Color4(0.0f, 0.5f, 1f, 1f);
        buttonWidget1.Size = FontSize.Small;
        buttonWidget1.VAlignment = TextVerticalAlignment.Bottom;
        buttonWidget1.Data = (object) printer;
        buttonWidget1.SetCallback(new ButtonCallback(this.MyButtonCallback));
        buttonWidget1.ToolTipMessage = " " + printer.Status.ToString();
        ButtonWidget buttonWidget2 = buttonWidget1;
        buttonWidget2.ToolTipMessage = buttonWidget2.ToolTipMessage + "\n Gantry clips removed: " + (printer.persistantData.GantryClipsRemoved ? "yes" : "no") + " ";
        ButtonWidget buttonWidget3 = buttonWidget1;
        buttonWidget3.ToolTipMessage = buttonWidget3.ToolTipMessage + "\n Has Valid Z: " + (printer.extruder.Z_Valid ? "yes" : "no") + " ";
        ButtonWidget buttonWidget4 = buttonWidget1;
        buttonWidget4.ToolTipMessage = buttonWidget4.ToolTipMessage + "\n Has Valid Calibration: " + (printer.calibration.Calibration_Valid ? "yes" : "no") + " ";
        this.PrinterGrid.AddChildElement((Element2D) buttonWidget1);
        ++num;
      }
    }

    private void OnPrintProcessChanged()
    {
      for (int index = 0; index < this.PrinterGrid.Count; ++index)
      {
        ButtonWidget elementAt = (ButtonWidget) this.PrinterGrid.GetElementAt(index);
        if (elementAt != null && elementAt.Data != null)
        {
          PrinterInfo data = elementAt.Data as PrinterInfo;
          elementAt.Text = this.GetPrinterLabelText(data);
        }
      }
    }

    private string GetPrinterLabelText(PrinterInfo info)
    {
      if (info.FirmwareIsInvalid)
        return info.serial_number.ToString() + "\n(Invalid Firmware)";
      if (info.current_job == null)
        return info.serial_number.ToString() + "\n";
      return info.serial_number.ToString() + "\n(Printing)";
    }

    public override void OnRender(GUIHost host)
    {
      base.OnRender(host);
      this.ResetPageButtons();
    }

    private void ResetPageButtons()
    {
      try
      {
        if (this.PrinterGrid == null)
          return;
        if (this.lastpage_count != this.PrinterGrid.PageCount)
          this.lastpage_count = this.PrinterGrid.PageCount;
        int curPage = this.PrinterGrid.CurPage;
        for (int index = 1032; index < 1062; ++index)
          this.pagebuttons[index - 1032].Visible = false;
        this.PrinterGrid.CurPage = curPage;
        if (this.PrinterGrid.Count < 1)
          return;
        int num1 = this.navigation.Width - 96;
        int num2 = num1 / 16;
        int num3 = this.lastpage_count;
        if (num3 > num2)
          num3 = num2;
        if (num3 > 31)
          num3 = 31;
        int num4 = (num1 - num3 * 16) / 2 + 48;
        for (int index = 0; index < num3; ++index)
        {
          this.pagebuttons[index].X = num4;
          this.pagebuttons[index].Visible = true;
          num4 += 16;
        }
        if (curPage >= num3)
          return;
        this.pagebuttons[curPage].SetChecked(true);
      }
      catch (Exception ex)
      {
        if (this.temp_exception_count < 100)
          ++this.temp_exception_count;
        else
          ExceptionForm.ShowExceptionForm(ex);
      }
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      PrinterObject selectedPrinter = this.spooler_connection.SelectedPrinter;
      if (button.ID >= 1032 && button.ID <= 1062)
        this.PrinterGrid.CurPage = button.ID - 1032;
      if (button.ID >= 1064)
      {
        PrinterInfo data = (PrinterInfo) button.Data;
        if (selectedPrinter != null && selectedPrinter.Info.serial_number != data.serial_number && (selectedPrinter.MarkedAsBusy && selectedPrinter.PrinterState != PrinterObject.State.IsPrinting))
        {
          this.messagebox.AddMessageToQueue("Warning: Switching printers will cause the current running action to stop.", PopupMessageBox.MessageBoxButtons.OKCANCEL, new PopupMessageBox.OnUserSelectionDel(this.OnUserSelection), (object) data);
        }
        else
        {
          this.spooler_connection.SelectPrinterBySerialNumber(data.serial_number.ToString());
          this.Close();
        }
      }
      else
      {
        switch (button.ID)
        {
          case 1000:
            this.Close();
            break;
          case 1005:
            --this.PrinterGrid.CurPage;
            break;
          case 1006:
            ++this.PrinterGrid.CurPage;
            break;
        }
      }
    }

    public void OnUserSelection(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object user_data)
    {
      if (result != PopupMessageBox.PopupResult.Button1_YesOK || type != MessageType.UserDefined)
        return;
      PrinterObject selectedPrinter = this.spooler_connection.SelectedPrinter;
      this.spooler_connection.SelectPrinterBySerialNumber(((PrinterInfo) user_data).serial_number.ToString());
      this.Close();
    }

    public override ElementType GetElementType()
    {
      return ElementType.SettingsDialog;
    }

    public override void Close()
    {
      this.Visible = false;
      if (!this.host.HasChildDialog)
        return;
      this.host.GlobalChildDialog -= (Element2D) this;
    }

    private enum SettingsButtons
    {
      Static = 0,
      Title = 100, // 0x00000064
      Close = 1000, // 0x000003E8
      PrevPageButton = 1005, // 0x000003ED
      NextPageButton = 1006, // 0x000003EE
      FirstPageButton = 1032, // 0x00000408
      LastPageButton = 1062, // 0x00000426
      FirstModelCell = 1064, // 0x00000428
    }

    private enum ControlGroups
    {
      TabsGroup = 1,
      PageButtonsGroup = 2,
    }
  }
}
