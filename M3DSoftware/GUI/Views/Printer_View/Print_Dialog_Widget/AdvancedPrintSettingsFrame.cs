// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.Print_Dialog_Widget.AdvancedPrintSettingsFrame
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.Slicer.General;
using M3D.SlicerConnectionCura.SlicerSettingsItems;
using M3D.Spooling.Common;
using OpenTK.Graphics;
using QuickFont;
using System;
using System.Collections.Generic;

namespace M3D.GUI.Views.Printer_View.Print_Dialog_Widget
{
  internal class AdvancedPrintSettingsFrame : IPrintDialogFrame
  {
    private Color4 WARNING_COLOR = new Color4(0.87f, 0.71f, 0.0f, 1f);
    private Color4 ERROR_COLOR = Color4.DarkRed;
    private Color4 DEFAULT_COLOR = new Color4(0.45f, 0.45f, 0.45f, 1f);
    private const int ColumnMargin = 5;
    private const int TitleWidth = 240;
    private const int UserInputColumn = 245;
    private const int EditBoxWidth = 75;
    private const int EditBoxMaxText = 7;
    private const int EditBoxSuffix = 325;
    private const int EditBoxSuffixWidth = 65;
    private const int EditBoxRangeX = 395;
    private PrintJobDetails CurrentJobDetails;
    private GUIHost host;
    private SmartSlicerSettingsBase smartSlicerSettings;
    private PopupMessageBox messagebox;
    private Frame tab_frame;
    private Frame active_frame;
    private List<Element2D> buttonElements;
    private int globalErrorCount;
    private int globalWarningCount;
    private Dictionary<string, AdvancedPrintSettingsFrame.TabErrorStateCount> tabsErrorWarningCount;
    private Dictionary<string, AdvancedPrintSettingsFrame.ErrorStateEnum> settingErrorWarningState;
    private ButtonWidget OK_Button;
    private TextWidget ErrorWarningMessage;
    private SettingsManager settingsManager;

    public AdvancedPrintSettingsFrame(int ID, GUIHost host, PopupMessageBox message_box, SettingsManager settings, PrintDialogMainWindow printDialogWindow)
      : base(ID, printDialogWindow)
    {
      settingsManager = settings;
      messagebox = message_box;
      Init(host);
    }

    public override void OnActivate(PrintJobDetails details)
    {
      SlicerConnection.SlicerSettingStack.PushSettings();
      globalErrorCount = 0;
      globalWarningCount = 0;
      tabsErrorWarningCount = new Dictionary<string, AdvancedPrintSettingsFrame.TabErrorStateCount>();
      settingErrorWarningState = new Dictionary<string, AdvancedPrintSettingsFrame.ErrorStateEnum>();
      buttonElements = new List<Element2D>();
      GenerateFromSlicerSettings(SlicerConnection.SlicerSettings);
      PrintDialogWindow.SetSize(750, 500);
      PrintDialogWindow.Refresh();
      CurrentJobDetails = details;
    }

    public override void OnDeactivate()
    {
      RemoveChildElement((Element2D)tab_frame);
      tab_frame = (Frame) null;
      foreach (Element2D buttonElement in buttonElements)
      {
        RemoveChildElement(buttonElement);
      }

      buttonElements = (List<Element2D>) null;
    }

    public void Init(GUIHost host)
    {
      this.host = host;
      CreateStandardElements();
      CreateErrorWarningMessage();
      Sprite.pixel_perfect = true;
      Visible = false;
    }

    private void CreateStandardElements()
    {
      SetSize(750, 500);
      var borderedImageFrame = new BorderedImageFrame(ID, (Element2D) null);
      borderedImageFrame.Init(host, "guicontrols", 640f, 320f, 704f, 383f, 41, 8, 64, 35, 8, 64);
      borderedImageFrame.SetSize(750, 500);
      borderedImageFrame.CenterHorizontallyInParent = true;
      borderedImageFrame.CenterVerticallyInParent = true;
      AddChildElement((Element2D) borderedImageFrame);
      AutoCenterYOffset = 0;
      CenterHorizontallyInParent = true;
      CenterVerticallyInParent = true;
      var textWidget = new TextWidget(600);
      textWidget.SetPosition(50, 2);
      textWidget.SetSize(500, 35);
      textWidget.Text = "Advanced Print Settings";
      textWidget.Alignment = QFontAlignment.Left;
      textWidget.Size = FontSize.Large;
      textWidget.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      AddChildElement((Element2D) textWidget);
      var x = 10;
      AddChildElement((Element2D)CreateUserOptionButton(host, "OK", AdvancedPrintSettingsFrame.AdvanceSettingsButtons.OK, x, -42, 100, 32, ""));
      int num;
      AddChildElement((Element2D)CreateUserOptionButton(host, "Cancel", AdvancedPrintSettingsFrame.AdvanceSettingsButtons.Cancel, num = x + 110, -42, 100, 32, ""));
      AddChildElement((Element2D)CreateUserOptionButton(host, "Reset", AdvancedPrintSettingsFrame.AdvanceSettingsButtons.Reset, -110, -42, 100, 32, ""));
      OK_Button = FindChildElement(613) as ButtonWidget;
    }

    private void GenerateFromSlicerSettings(SmartSlicerSettingsBase smartSlicerSettings)
    {
      this.smartSlicerSettings = smartSlicerSettings;
      tab_frame = new Frame(608, (Element2D)this)
      {
        X = 180,
        Y = 35,
        HexBorderColor = "#0C0C0C0C"
      };
      AddChildElement((Element2D)tab_frame);
      var YPos = 35;
      var num = 10000;
      List<XMLTabCollectionSettingsItem> visualSettings = smartSlicerSettings.VisualSettings;
      for (var index = 0; index < visualSettings.Count; ++index)
      {
        CreateTabButton(visualSettings[index].Header, ref num, YPos, 181, 64);
        YPos += 64;
      }
      for (var index1 = 0; index1 < visualSettings.Count; ++index1)
      {
        var scrollableVerticalLayout = new ScrollableVerticalLayout(num++, (Element2D)tab_frame);
        scrollableVerticalLayout.Init(host);
        scrollableVerticalLayout.tag = GetTabFrameTag(visualSettings[index1].Header);
        scrollableVerticalLayout.Visible = false;
        scrollableVerticalLayout.Enabled = false;
        scrollableVerticalLayout.RelativeWidth = 1f;
        scrollableVerticalLayout.RelativeHeight = 1f;
        for (var index2 = 0; index2 < visualSettings[index1].Items.Count; ++index2)
        {
          XMLSettingsItem settings = visualSettings[index1].Items[index2];
          if (!settings.Name.StartsWith(XMLSetting.MagicInternalString + "Hidden", StringComparison.InvariantCultureIgnoreCase))
          {
            Element2D element;
            switch (settings.SlicerSettingsItem.GetItemType())
            {
              case SettingItemType.IntType:
              case SettingItemType.FloatMMType:
                element = CreateTextBox((Frame) scrollableVerticalLayout, settings, ref num);
                break;
              case SettingItemType.BoolType:
                element = CreateCheckBox((Frame) scrollableVerticalLayout, settings, ref num);
                break;
              case SettingItemType.FillPatternType:
              case SettingItemType.SupportPatternType:
                element = CreateComboBox((Frame) scrollableVerticalLayout, settings, ref num);
                break;
              default:
                throw new NotImplementedException("GenerateAdvancedSettings was given an unknown setting");
            }
            if (element != null)
            {
              ProcessValidity(settings.Name, settings.SlicerSettingsItem, element);
            }
          }
        }
        tab_frame.AddChildElement((Element2D) scrollableVerticalLayout);
      }
      Refresh();
      if (visualSettings.Count == 0)
      {
        return;
      } (FindChildElement(GetTabButtonTag(visualSettings[0].Header)) as ButtonWidget)?.SetChecked(true);
    }

    private void CreateErrorWarningMessage()
    {
      ErrorWarningMessage = new TextWidget();
      ErrorWarningMessage.SetPosition(300, -45);
      ErrorWarningMessage.SetSize(500, 35);
      ErrorWarningMessage.Text = "Testing one two three";
      ErrorWarningMessage.Alignment = QFontAlignment.Left;
      ErrorWarningMessage.Size = FontSize.Large;
      ErrorWarningMessage.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      ErrorWarningMessage.Visible = false;
      AddChildElement((Element2D)ErrorWarningMessage);
    }

    private void CreateTabButton(string Header, ref int tagIDBuilder, int YPos, int width, int height)
    {
      var buttonWidget = new ButtonWidget(tagIDBuilder++);
      buttonWidget.Init(host, "guicontrols", 448f, 256f, 628f, 319f, 448f, 256f, 628f, 319f, 448f, 384f, 628f, 447f);
      buttonWidget.tag = GetTabButtonTag(Header);
      buttonWidget.Text = "  " + Header;
      buttonWidget.SetPosition(0, YPos);
      buttonWidget.SetSize(width, height);
      buttonWidget.TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f);
      buttonWidget.TextDownColor = new Color4(1f, 1f, 1f, 1f);
      buttonWidget.TextOverColor = new Color4(0.161f, 0.79f, 0.95f, 1f);
      buttonWidget.Alignment = QFontAlignment.Left;
      buttonWidget.DontMove = true;
      buttonWidget.ClickType = ButtonType.Checkable;
      buttonWidget.GroupID = 89405;
      buttonWidget.SetCallback(new ButtonCallback(MyTabButtonCallback));
      AddChildElement((Element2D) buttonWidget);
      buttonElements.Add((Element2D) buttonWidget);
      var imageWidget1 = new ImageWidget(tagIDBuilder++)
      {
        tag = GetWarningImageTag(Header)
      };
      imageWidget1.Init(host, "extendedcontrols", 0.0f, 0.0f, 37f, 30f);
      imageWidget1.SetSize(24, 20);
      imageWidget1.SetPosition(width - (imageWidget1.Width + 10), YPos + 3);
      imageWidget1.Visible = false;
      AddChildElement((Element2D) imageWidget1);
      buttonElements.Add((Element2D) imageWidget1);
      var imageWidget2 = new ImageWidget(tagIDBuilder++)
      {
        tag = GetErrorImageTag(Header)
      };
      imageWidget2.Init(host, "extendedcontrols", 38f, 0.0f, 72f, 30f);
      imageWidget2.SetSize(24, 20);
      imageWidget2.SetPosition(width - (imageWidget2.Width + 10), YPos + 3);
      imageWidget2.Visible = false;
      AddChildElement((Element2D) imageWidget2);
      buttonElements.Add((Element2D) imageWidget2);
    }

    private ButtonWidget CreateUserOptionButton(GUIHost host, string content, AdvancedPrintSettingsFrame.AdvanceSettingsButtons id, int x, int y, int width = 100, int height = 32, string tooltip = "")
    {
      var buttonWidget = new ButtonWidget((int) id);
      buttonWidget.Init(host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      buttonWidget.ID = (int) id;
      buttonWidget.SetPosition(x, y);
      buttonWidget.SetSize(width, height);
      buttonWidget.Text = content;
      buttonWidget.Alignment = QFontAlignment.Centre;
      buttonWidget.Size = FontSize.Medium;
      buttonWidget.ToolTipMessage = tooltip;
      buttonWidget.SetCallback(new ButtonCallback(MyTabButtonCallback));
      return buttonWidget;
    }

    private Element2D CreateCheckBox(Frame vertLayout, XMLSettingsItem settings, ref int ID_Builder)
    {
      var horzonalFrame = new Frame(ID_Builder++, (Element2D)vertLayout)
      {
        RelativeWidth = 1f,
        Height = 28
      };
      CreateSettingsTitleElement(horzonalFrame, ID_Builder++, settings.Text);
      var buttonWidget = new ButtonWidget(ID_Builder++, (Element2D) horzonalFrame);
      buttonWidget.Init(host, ButtonTemplate.CheckBox);
      buttonWidget.ToolTipMessage = settings.Tooltip;
      buttonWidget.ClickType = ButtonType.Checkable;
      buttonWidget.DontMove = true;
      buttonWidget.CanClickOff = true;
      buttonWidget.Height = 28;
      buttonWidget.Width = 28;
      buttonWidget.X = 245;
      buttonWidget.tag = settings.Name;
      buttonWidget.Checked = bool.Parse(settings.SlicerSettingsItem.TranslateToUserValue());
      buttonWidget.SetCallback(new ButtonCallback(CheckBoxCallback));
      buttonWidget.FadeWhenDisabled = true;
      horzonalFrame.AddChildElement((Element2D) buttonWidget);
      vertLayout.AddChildElement((Element2D) horzonalFrame);
      return (Element2D) buttonWidget;
    }

    private Element2D CreateTextBox(Frame vertLayout, XMLSettingsItem settings, ref int ID_Builder)
    {
      var slicerSettingsItem = settings.SlicerSettingsItem as IReportFormat;
      NumFormat numFormat = NumFormat.Thousands;
      if (slicerSettingsItem != null)
      {
        numFormat = slicerSettingsItem.Format;
      }

      var horzonalFrame = new Frame(ID_Builder++, (Element2D)vertLayout)
      {
        RelativeWidth = 1f,
        Height = 28
      };
      CreateSettingsTitleElement(horzonalFrame, ID_Builder++, settings.Text);
      var editBoxWidget = new EditBoxWidget(ID_Builder++, (Element2D) horzonalFrame);
      editBoxWidget.Init(host, "guicontrols", 898f, 104f, 941f, 135f);
      editBoxWidget.Height = 25;
      editBoxWidget.Width = 75;
      editBoxWidget.MAX_CHARS = 7;
      editBoxWidget.X = 245;
      editBoxWidget.Size = FontSize.Medium;
      editBoxWidget.HexColor = "#FF808080";
      editBoxWidget.ToolTipMessage = settings.Tooltip;
      editBoxWidget.FadeWhenDisabled = true;
      editBoxWidget.Text = settings.SlicerSettingsItem.TranslateToUserValue();
      editBoxWidget.tag = settings.Name;
      editBoxWidget.NumFormat = numFormat;
      editBoxWidget.SetCallbackOnTextAdded(new EditBoxWidget.EditBoxCallback(EditBoxCallback));
      editBoxWidget.SetCallbackOnBackspace(new EditBoxWidget.EditBoxCallback(EditBoxCallback));
      horzonalFrame.AddChildElement((Element2D) editBoxWidget);
      var textWidget1 = new TextWidget(ID_Builder++, (Element2D)horzonalFrame)
      {
        Width = 65,
        Height = 35,
        X = 325,
        Size = FontSize.Medium,
        HexColor = "#FF7f7f7f",
        Alignment = QFontAlignment.Left,
        Text = settings.Suffix
      };
      horzonalFrame.AddChildElement((Element2D) textWidget1);
      var textWidget2 = new TextWidget(ID_Builder++, (Element2D)horzonalFrame)
      {
        Width = 500,
        Height = 35,
        X = 395,
        Size = FontSize.Medium,
        HexColor = "#FF7f7f7f",
        Alignment = QFontAlignment.Left,
        Visible = true,
        tag = GetRangeText(settings.Name)
      };
      horzonalFrame.AddChildElement((Element2D) textWidget2);
      vertLayout.AddChildElement((Element2D) horzonalFrame);
      return (Element2D) editBoxWidget;
    }

    private Element2D CreateComboBox(Frame vertLayout, XMLSettingsItem settings, ref int ID_Builder)
    {
      var slicerSettingsItem = settings.SlicerSettingsItem as SlicerSettingsEnumItem;
      var horzonalFrame = new Frame(ID_Builder++, (Element2D)vertLayout)
      {
        RelativeWidth = 1f,
        Height = 28
      };
      CreateSettingsTitleElement(horzonalFrame, ID_Builder++, settings.Text);
      var comboBoxWidget = new ComboBoxWidget(ID_Builder++, (Element2D)horzonalFrame)
      {
        Value = (object)slicerSettingsItem.ValueInt,
        tag = settings.Name
      };
      comboBoxWidget.Init(host);
      comboBoxWidget.ItemsEnumString = slicerSettingsItem.EnumType.ToString();
      comboBoxWidget.Select = slicerSettingsItem.ValueInt;
      comboBoxWidget.ToolTipMessage = settings.Tooltip;
      comboBoxWidget.Height = 24;
      comboBoxWidget.Width = 240;
      comboBoxWidget.X = 245;
      comboBoxWidget.Size = FontSize.Medium;
      comboBoxWidget.HexColor = "#FF808080";
      comboBoxWidget.TextChangedCallback += new ComboBoxWidget.ComboBoxTextChangedCallback(comboBoxChangedCallBack);
      horzonalFrame.AddChildElement((Element2D) comboBoxWidget);
      vertLayout.AddChildElement((Element2D) horzonalFrame);
      return (Element2D) comboBoxWidget;
    }

    private void CreateSettingsTitleElement(Frame horzonalFrame, int id, string text)
    {
      var textWidget = new TextWidget(id, (Element2D)horzonalFrame)
      {
        Text = text,
        Size = FontSize.Medium,
        HexColor = "#FF7f7f7f",
        Alignment = QFontAlignment.Left,
        CenterVerticallyInParent = true,
        Height = 35,
        Width = 240,
        FadeWhenDisabled = true
      };
      horzonalFrame.AddChildElement((Element2D) textWidget);
    }

    private string GetTabButtonTag(string tag)
    {
      tag = RemoveOtherElementNameFlag(tag);
      return tag + "_TabButton";
    }

    private string GetRangeText(string tag)
    {
      tag = RemoveOtherElementNameFlag(tag);
      return tag + "_RangeText";
    }

    private string GetTabFrameTag(string tag)
    {
      tag = RemoveOtherElementNameFlag(tag);
      return tag + "_Frame";
    }

    private string GetErrorImageTag(string tag)
    {
      tag = RemoveOtherElementNameFlag(tag);
      return tag + "_ErrorImage";
    }

    private string GetWarningImageTag(string tag)
    {
      tag = RemoveOtherElementNameFlag(tag);
      return tag + "_WarningImage";
    }

    private string RemoveOtherElementNameFlag(string tag)
    {
      string[] strArray = new string[5]
      {
        "_TabButton",
        "_RangeText",
        "_Frame",
        "_ErrorImage",
        "_WarningImage"
      };
      foreach (var str in strArray)
      {
        if (tag.EndsWith(str))
        {
          return tag.Substring(0, tag.Length - str.Length);
        }
      }
      return tag;
    }

    public void MyTabButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 611:
          SlicerSettings.SetToDefault();
          syncAllSettingWithGUI();
          break;
        case 612:
          SlicerConnection.SlicerSettingStack.PopSettings();
          PrintDialogWindow.ActivateFrame(PrintDialogWidgetFrames.PrintDialogFrame, CurrentJobDetails);
          break;
        case 613:
          SlicerConnection.SlicerSettingStack.SaveSettingsDown();
          PrintDialogWindow.ActivateFrame(PrintDialogWidgetFrames.PrintDialogFrame, CurrentJobDetails);
          break;
        default:
          ShowFrame(button.tag);
          break;
      }
    }

    private void ShowFrame(string tag)
    {
      var childElement = FindChildElement(GetTabFrameTag(tag)) as Frame;
      if (childElement == null)
      {
        return;
      }

      ActivateFrame(childElement);
    }

    private void ActivateFrame(Frame frame)
    {
      if (active_frame != null)
      {
        active_frame.Visible = false;
        active_frame.Enabled = false;
        active_frame = (Frame) null;
      }
      active_frame = frame;
      if (active_frame == null)
      {
        return;
      }

      active_frame.Enabled = true;
      active_frame.Visible = true;
    }

    private void CheckBoxCallback(ButtonWidget button)
    {
      var tag = button.tag;
      if (tag.StartsWith("InternalToGUI_"))
      {
        PrinterObject selectedPrinter = SelectedPrinter;
        var filament = (FilamentSpool) null;
        if (selectedPrinter != null)
        {
          filament = selectedPrinter.GetCurrentFilament();
        }

        SlicerConnection.SlicerSettings.SmartCheckBoxCallBack(tag, button.Checked, filament);
        syncAllSettingWithGUI();
      }
      else
      {
        SlicerSettingsItem smartSlicerSetting = smartSlicerSettings[tag];
        smartSlicerSetting.ParseUserValue(button.Value.ToString());
        ProcessValidity(tag, smartSlicerSetting, (Element2D) button);
      }
    }

    private void EditBoxCallback(EditBoxWidget edit)
    {
      var tag = edit.tag;
      SlicerSettingsItem smartSlicerSetting = smartSlicerSettings[tag];
      smartSlicerSetting.ParseUserValue(edit.Value.ToString());
      ProcessValidity(tag, smartSlicerSetting, (Element2D) edit);
    }

    private void comboBoxChangedCallBack(ComboBoxWidget combobox)
    {
      var tag = combobox.tag;
      SlicerSettingsItem smartSlicerSetting = smartSlicerSettings[tag];
      smartSlicerSetting.ParseUserValue(combobox.Value.ToString());
      ProcessValidity(tag, smartSlicerSetting, (Element2D) combobox);
    }

    private void syncAllSettingWithGUI()
    {
      var count = SlicerSettings.Count;
      foreach (System.Collections.Generic.KeyValuePair<string, SlicerSettingsItem> slicerSetting in SlicerSettings)
      {
        UpdateGUIFromSetting(slicerSetting.Key, slicerSetting.Value);
      }
    }

    private void UpdateGUIFromSetting(string name, SlicerSettingsItem setting)
    {
      if (tab_frame == null)
      {
        return;
      }

      Element2D childElement = tab_frame.FindChildElement(name);
      if (childElement == null)
      {
        return;
      }

      var userValue = setting.TranslateToUserValue();
      switch (setting.GetItemType())
      {
        case SettingItemType.IntType:
        case SettingItemType.FloatMMType:
          var editBoxWidget = childElement as EditBoxWidget;
          if (editBoxWidget != null)
          {
            editBoxWidget.Text = userValue;
            break;
          }
          break;
        case SettingItemType.BoolType:
          var buttonWidget = childElement as ButtonWidget;
          if (buttonWidget != null)
          {
            buttonWidget.Checked = bool.Parse(userValue);
            break;
          }
          break;
        case SettingItemType.FillPatternType:
        case SettingItemType.SupportPatternType:
          var comboBoxWidget = childElement as ComboBoxWidget;
          var settingsEnumItem = setting as SlicerSettingsEnumItem;
          if (comboBoxWidget != null && settingsEnumItem != null)
          {
            comboBoxWidget.Select = settingsEnumItem.ValueInt;
            break;
          }
          break;
        default:
          throw new NotImplementedException("GenerateAdvancedSettings was given an unknown setting");
      }
      ProcessValidity(name, setting, childElement);
    }

    private void ProcessValidity(string settingName, SlicerSettingsItem setting, Element2D element)
    {
      Element2D parent = element.Parent;
      while (parent != null && string.IsNullOrEmpty(parent.tag))
      {
        parent = parent.Parent;
      }

      if (parent == null)
      {
        return;
      }

      var tag = parent.tag;
      var num = GetStateChangeInfo(element, settingName, setting, tag, out ErrorStateEnum oldState, out ErrorStateEnum currentState) ? 1 : 0;
      Element2D childElement = element.Parent.FindChildElement(GetRangeText(settingName));
      if (num != 0)
      {
        AdvancedPrintSettingsFrame.TabErrorStateCount tabErrorStateCount = tabsErrorWarningCount[tag];
        switch (oldState)
        {
          case AdvancedPrintSettingsFrame.ErrorStateEnum.Warning:
            --tabErrorStateCount.Warnings;
            --globalWarningCount;
            if (tabErrorStateCount.Warnings == 0)
            {
              image_Helper(GetWarningImageTag(tag), false);
              break;
            }
            break;
          case AdvancedPrintSettingsFrame.ErrorStateEnum.Error:
            --tabErrorStateCount.Errors;
            --globalErrorCount;
            if (tabErrorStateCount.Errors == 0)
            {
              image_Helper(GetErrorImageTag(tag), false);
              break;
            }
            break;
        }
        Color4 color4 = DEFAULT_COLOR;
        var flag = true;
        switch (currentState)
        {
          case AdvancedPrintSettingsFrame.ErrorStateEnum.Warning:
            color4 = WARNING_COLOR;
            ++tabErrorStateCount.Warnings;
            ++globalWarningCount;
            if (tabErrorStateCount.Warnings == 1)
            {
              image_Helper(GetWarningImageTag(tag), true);
              break;
            }
            break;
          case AdvancedPrintSettingsFrame.ErrorStateEnum.Error:
            color4 = ERROR_COLOR;
            ++tabErrorStateCount.Errors;
            ++globalErrorCount;
            if (tabErrorStateCount.Errors == 1)
            {
              image_Helper(GetErrorImageTag(tag), true);
              break;
            }
            break;
          default:
            flag = false;
            break;
        }
        if (element is EditBoxWidget)
        {
          (element as EditBoxWidget).Color = color4;
        }

        if (childElement is TextWidget)
        {
          var textWidget = childElement as TextWidget;
          textWidget.Color = color4;
          textWidget.Visible = flag;
        }
        UpdateErrorWarningMessage();
      }
      if (!(childElement is TextWidget))
      {
        return;
      } (childElement as TextWidget).Text = setting.GetErrorMsg();
    }

    private void UpdateErrorWarningMessage()
    {
      var flag = false;
      if (globalErrorCount == 1)
      {
        OK_Button.Enabled = false;
        ErrorWarningMessage.Visible = true;
        ErrorWarningMessage.Text = "Error on settings";
        ErrorWarningMessage.Color = ERROR_COLOR;
      }
      else if (globalErrorCount == 0)
      {
        ErrorWarningMessage.Visible = false;
        OK_Button.Enabled = true;
        flag = globalWarningCount > 1;
      }
      if (globalWarningCount == 1 | flag && globalErrorCount == 0)
      {
        ErrorWarningMessage.Visible = true;
        ErrorWarningMessage.Text = "Warning on settings";
        ErrorWarningMessage.Color = WARNING_COLOR;
      }
      if (globalWarningCount != 0 || globalErrorCount != 0)
      {
        return;
      }

      ErrorWarningMessage.Visible = false;
    }

    private bool GetStateChangeInfo(Element2D element, string settingName, SlicerSettingsItem setting, string parentTag, out AdvancedPrintSettingsFrame.ErrorStateEnum oldState, out AdvancedPrintSettingsFrame.ErrorStateEnum currentState)
    {
      if (!tabsErrorWarningCount.ContainsKey(parentTag))
      {
        tabsErrorWarningCount[parentTag] = new AdvancedPrintSettingsFrame.TabErrorStateCount();
      }

      oldState = AdvancedPrintSettingsFrame.ErrorStateEnum.AllGood;
      var flag = false;
      if (settingErrorWarningState.ContainsKey(settingName))
      {
        oldState = settingErrorWarningState[settingName];
      }

      if (setting.HasError && setting.IsSettingOn)
      {
        currentState = AdvancedPrintSettingsFrame.ErrorStateEnum.Error;
        flag = true;
        if (!settingErrorWarningState.ContainsKey(settingName))
        {
          settingErrorWarningState.Add(settingName, AdvancedPrintSettingsFrame.ErrorStateEnum.Error);
        }
        else
        {
          flag = settingErrorWarningState[settingName] != AdvancedPrintSettingsFrame.ErrorStateEnum.Error;
          settingErrorWarningState[settingName] = AdvancedPrintSettingsFrame.ErrorStateEnum.Error;
        }
      }
      else if (setting.HasWarning && setting.IsSettingOn)
      {
        currentState = AdvancedPrintSettingsFrame.ErrorStateEnum.Warning;
        flag = true;
        if (!settingErrorWarningState.ContainsKey(settingName))
        {
          settingErrorWarningState.Add(settingName, AdvancedPrintSettingsFrame.ErrorStateEnum.Warning);
        }
        else
        {
          flag = settingErrorWarningState[settingName] != AdvancedPrintSettingsFrame.ErrorStateEnum.Warning;
          settingErrorWarningState[settingName] = AdvancedPrintSettingsFrame.ErrorStateEnum.Warning;
        }
      }
      else
      {
        currentState = AdvancedPrintSettingsFrame.ErrorStateEnum.AllGood;
        if (settingErrorWarningState.ContainsKey(settingName))
        {
          settingErrorWarningState.Remove(settingName);
          flag = true;
        }
        element.Enabled = setting.IsSettingOn;
        if (!element.Enabled)
        {
          flag = true;
        }
      }
      return flag;
    }

    private void image_Helper(string name, bool visibility)
    {
      var childElement = FindChildElement(name) as ImageWidget;
      if (childElement == null)
      {
        return;
      }

      childElement.Visible = visibility;
    }

    public override void OnParentResize()
    {
      if (tab_frame != null)
      {
        tab_frame.SetSize(Width - tab_frame.X, Height - tab_frame.Y - 60);
      }

      base.OnParentResize();
    }

    public override void OnRender(GUIHost host)
    {
      base.OnRender(host);
    }

    public override ElementType GetElementType()
    {
      return ElementType.SettingsDialog;
    }

    public override void OnUpdate()
    {
      base.OnUpdate();
    }

    public enum AdvanceSettingsButtons
    {
      Title = 600, // 0x00000258
      Close = 607, // 0x0000025F
      TabFrame = 608, // 0x00000260
      Reset = 611, // 0x00000263
      Cancel = 612, // 0x00000264
      OK = 613, // 0x00000265
    }

    private enum ErrorStateEnum
    {
      AllGood,
      Warning,
      Error,
    }

    private class TabErrorStateCount
    {
      public int Warnings;
      public int Errors;
    }
  }
}
