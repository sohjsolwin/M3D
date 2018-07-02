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
      : base(ID, null)
    {
      this.main_controller = main_controller;
      this.messagebox = messagebox;
      this.spooler_connection = spooler_connection;
      printer_list = new List<PrinterInfo>();
      spooler_connection.OnPrintProcessChanged += new SpoolerClient.OnPrintProcessDel(OnPrintProcessChanged);
      Init(host);
    }

    private void Init(GUIHost host)
    {
      this.host = host;
      Init(host, "guicontrols", 640f, 320f, 704f, 383f, 41, 8, 64, 35, 8, 64);
      SetSize(792, 356);
      var textWidget = new TextWidget(100);
      textWidget.SetPosition(50, 2);
      textWidget.SetSize(500, 35);
      textWidget.Text = "Multi-Printer Options";
      textWidget.Alignment = QFontAlignment.Left;
      textWidget.Size = FontSize.Large;
      textWidget.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      AddChildElement(textWidget);
      var buttonWidget = new ButtonWidget(1000)
      {
        X = -40,
        Y = 4
      };
      buttonWidget.SetSize(32, 32);
      buttonWidget.Text = "";
      buttonWidget.TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f);
      buttonWidget.TextDownColor = new Color4(1f, 1f, 1f, 1f);
      buttonWidget.TextOverColor = new Color4(0.161f, 0.79f, 0.95f, 1f);
      buttonWidget.Alignment = QFontAlignment.Left;
      buttonWidget.Init(host, "guicontrols", 704f, 320f, 735f, 351f, 736f, 320f, 767f, 351f, 704f, 352f, 735f, 383f);
      buttonWidget.DontMove = true;
      buttonWidget.SetCallback(new ButtonCallback(MyButtonCallback));
      AddChildElement(buttonWidget);
      navigation = new Frame(0, null);
      navigation_left = new ButtonWidget(1005, null)
      {
        Text = "",
        X = 16,
        Y = 0,
        Width = 32,
        Height = 32
      };
      navigation_left.SetCallback(new ButtonCallback(MyButtonCallback));
      navigation_left.Init(host, "guicontrols", 608f, 0.0f, 639f, 31f, 640f, 0.0f, 671f, 31f, 672f, 0.0f, 703f, 31f, 704f, 0.0f, 735f, 31f);
      navigation_right = new ButtonWidget(1006, null)
      {
        Text = "",
        X = -48,
        Y = 0,
        Width = 32,
        Height = 32
      };
      navigation_right.SetCallback(new ButtonCallback(MyButtonCallback));
      navigation_right.Init(host, "guicontrols", 608f, 32f, 639f, 63f, 640f, 32f, 671f, 63f, 672f, 32f, 703f, 63f, 704f, 32f, 735f, 63f);
      pagebuttons = new ButtonWidget[31];
      for (var ID = 1032; ID < 1062; ++ID)
      {
        var index = ID - 1032;
        pagebuttons[index] = new ButtonWidget(ID, null)
        {
          Text = "",
          X = 48 + (ID - 1032) * 24,
          Y = 8,
          Width = 16,
          Height = 16
        };
        pagebuttons[index].SetCallback(new ButtonCallback(MyButtonCallback));
        pagebuttons[index].Init(host, "guicontrols", 448f, 192f, 463f, 208f, 480f, 192f, 495f, 208f, 464f, 192f, 479f, 208f);
        pagebuttons[index].DontMove = true;
        pagebuttons[index].GroupID = 1;
        pagebuttons[index].ClickType = ButtonType.Checkable;
        pagebuttons[index].Visible = false;
        navigation.AddChildElement(pagebuttons[index]);
      }
      navigation.AddChildElement(navigation_left);
      navigation.AddChildElement(navigation_right);
      navigation.RelativeWidth = 0.95f;
      navigation.Height = 32;
      navigation.SetPosition(0, -50);
      navigation.CenterHorizontallyInParent = true;
      AddChildElement(navigation);
      PrinterGrid = new GridLayout(1)
      {
        ColumnWidth = 130,
        RowHeight = 150,
        BorderWidth = 0,
        BorderHeight = 0,
        RelativeHeight = 0.8f,
        RelativeWidth = 0.8f
      };
      PrinterGrid.SetPosition(0, 48);
      PrinterGrid.CenterHorizontallyInParent = true;
      AddChildElement(PrinterGrid);
      Sprite.pixel_perfect = false;
      Visible = false;
    }

    public override void OnParentResize()
    {
      base.OnParentResize();
    }

    public override void OnUpdate()
    {
      base.OnUpdate();
      if (spooler_connection.CopyPrinterList(ref printer_list))
      {
        return;
      }

      PrinterGrid.Clear();
      var num = 0;
      foreach (PrinterInfo printer in printer_list)
      {
        var buttonWidget1 = new ButtonWidget(1064 + num, null);
        ImageResourceMapping.PixelCoordinate pixelCoordinate = ImageResourceMapping.PrinterColorPosition(printer.serial_number.ToString());
        buttonWidget1.Init(host, "extendedcontrols", pixelCoordinate.u0, pixelCoordinate.v0, pixelCoordinate.u1, pixelCoordinate.v1, pixelCoordinate.u0, pixelCoordinate.v0, pixelCoordinate.u1, pixelCoordinate.v1, pixelCoordinate.u0, pixelCoordinate.v0, pixelCoordinate.u1, pixelCoordinate.v1);
        buttonWidget1.DontMove = true;
        buttonWidget1.Text = GetPrinterLabelText(printer);
        buttonWidget1.Color = new Color4(0.0f, 0.5f, 1f, 1f);
        buttonWidget1.Size = FontSize.Small;
        buttonWidget1.VAlignment = TextVerticalAlignment.Bottom;
        buttonWidget1.Data = printer;
        buttonWidget1.SetCallback(new ButtonCallback(MyButtonCallback));
        buttonWidget1.ToolTipMessage = " " + printer.Status.ToString();
        ButtonWidget buttonWidget2 = buttonWidget1;
        buttonWidget2.ToolTipMessage = buttonWidget2.ToolTipMessage + "\n Gantry clips removed: " + (printer.persistantData.GantryClipsRemoved ? "yes" : "no") + " ";
        ButtonWidget buttonWidget3 = buttonWidget1;
        buttonWidget3.ToolTipMessage = buttonWidget3.ToolTipMessage + "\n Has Valid Z: " + (printer.extruder.Z_Valid ? "yes" : "no") + " ";
        ButtonWidget buttonWidget4 = buttonWidget1;
        buttonWidget4.ToolTipMessage = buttonWidget4.ToolTipMessage + "\n Has Valid Calibration: " + (printer.calibration.Calibration_Valid ? "yes" : "no") + " ";
        PrinterGrid.AddChildElement(buttonWidget1);
        ++num;
      }
    }

    private void OnPrintProcessChanged()
    {
      for (var index = 0; index < PrinterGrid.Count; ++index)
      {
        var elementAt = (ButtonWidget)PrinterGrid.GetElementAt(index);
        if (elementAt != null && elementAt.Data != null)
        {
          var data = elementAt.Data as PrinterInfo;
          elementAt.Text = GetPrinterLabelText(data);
        }
      }
    }

    private string GetPrinterLabelText(PrinterInfo info)
    {
      if (info.FirmwareIsInvalid)
      {
        return info.serial_number.ToString() + "\n(Invalid Firmware)";
      }

      if (info.current_job == null)
      {
        return info.serial_number.ToString() + "\n";
      }

      return info.serial_number.ToString() + "\n(Printing)";
    }

    public override void OnRender(GUIHost host)
    {
      base.OnRender(host);
      ResetPageButtons();
    }

    private void ResetPageButtons()
    {
      try
      {
        if (PrinterGrid == null)
        {
          return;
        }

        if (lastpage_count != PrinterGrid.PageCount)
        {
          lastpage_count = PrinterGrid.PageCount;
        }

        var curPage = PrinterGrid.CurPage;
        for (var index = 1032; index < 1062; ++index)
        {
          pagebuttons[index - 1032].Visible = false;
        }

        PrinterGrid.CurPage = curPage;
        if (PrinterGrid.Count < 1)
        {
          return;
        }

        var num1 = navigation.Width - 96;
        var num2 = num1 / 16;
        var num3 = lastpage_count;
        if (num3 > num2)
        {
          num3 = num2;
        }

        if (num3 > 31)
        {
          num3 = 31;
        }

        var num4 = (num1 - num3 * 16) / 2 + 48;
        for (var index = 0; index < num3; ++index)
        {
          pagebuttons[index].X = num4;
          pagebuttons[index].Visible = true;
          num4 += 16;
        }
        if (curPage >= num3)
        {
          return;
        }

        pagebuttons[curPage].SetChecked(true);
      }
      catch (Exception ex)
      {
        if (temp_exception_count < 100)
        {
          ++temp_exception_count;
        }
        else
        {
          ExceptionForm.ShowExceptionForm(ex);
        }
      }
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      PrinterObject selectedPrinter = spooler_connection.SelectedPrinter;
      if (button.ID >= 1032 && button.ID <= 1062)
      {
        PrinterGrid.CurPage = button.ID - 1032;
      }

      if (button.ID >= 1064)
      {
        var data = (PrinterInfo) button.Data;
        if (selectedPrinter != null && selectedPrinter.Info.serial_number != data.serial_number && (selectedPrinter.MarkedAsBusy && selectedPrinter.PrinterState != PrinterObject.State.IsPrinting))
        {
          messagebox.AddMessageToQueue("Warning: Switching printers will cause the current running action to stop.", PopupMessageBox.MessageBoxButtons.OKCANCEL, new PopupMessageBox.OnUserSelectionDel(OnUserSelection), data);
        }
        else
        {
          spooler_connection.SelectPrinterBySerialNumber(data.serial_number.ToString());
          Close();
        }
      }
      else
      {
        switch (button.ID)
        {
          case 1000:
            Close();
            break;
          case 1005:
            --PrinterGrid.CurPage;
            break;
          case 1006:
            ++PrinterGrid.CurPage;
            break;
        }
      }
    }

    public void OnUserSelection(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object user_data)
    {
      if (result != PopupMessageBox.PopupResult.Button1_YesOK || type != MessageType.UserDefined)
      {
        return;
      }

      PrinterObject selectedPrinter = spooler_connection.SelectedPrinter;
      spooler_connection.SelectPrinterBySerialNumber(((PrinterInfo) user_data).serial_number.ToString());
      Close();
    }

    public override ElementType GetElementType()
    {
      return ElementType.SettingsDialog;
    }

    public override void Close()
    {
      Visible = false;
      if (!host.HasChildDialog)
      {
        return;
      }

      host.GlobalChildDialog -= (this);
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
