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
      this.settingsManager = settings;
      this.messagebox = message_box;
      this.Init(host);
    }

    public override void OnActivate(PrintJobDetails details)
    {
      this.SlicerConnection.SlicerSettingStack.PushSettings();
      this.globalErrorCount = 0;
      this.globalWarningCount = 0;
      this.tabsErrorWarningCount = new Dictionary<string, AdvancedPrintSettingsFrame.TabErrorStateCount>();
      this.settingErrorWarningState = new Dictionary<string, AdvancedPrintSettingsFrame.ErrorStateEnum>();
      this.buttonElements = new List<Element2D>();
      this.GenerateFromSlicerSettings(this.SlicerConnection.SlicerSettings);
      this.PrintDialogWindow.SetSize(750, 500);
      this.PrintDialogWindow.Refresh();
      this.CurrentJobDetails = details;
    }

    public override void OnDeactivate()
    {
      this.RemoveChildElement((Element2D) this.tab_frame);
      this.tab_frame = (Frame) null;
      foreach (Element2D buttonElement in this.buttonElements)
        this.RemoveChildElement(buttonElement);
      this.buttonElements = (List<Element2D>) null;
    }

    public void Init(GUIHost host)
    {
      this.host = host;
      this.CreateStandardElements();
      this.CreateErrorWarningMessage();
      Sprite.pixel_perfect = true;
      this.Visible = false;
    }

    private void CreateStandardElements()
    {
      this.SetSize(750, 500);
      BorderedImageFrame borderedImageFrame = new BorderedImageFrame(this.ID, (Element2D) null);
      borderedImageFrame.Init(this.host, "guicontrols", 640f, 320f, 704f, 383f, 41, 8, 64, 35, 8, 64);
      borderedImageFrame.SetSize(750, 500);
      borderedImageFrame.CenterHorizontallyInParent = true;
      borderedImageFrame.CenterVerticallyInParent = true;
      this.AddChildElement((Element2D) borderedImageFrame);
      this.AutoCenterYOffset = 0;
      this.CenterHorizontallyInParent = true;
      this.CenterVerticallyInParent = true;
      TextWidget textWidget = new TextWidget(600);
      textWidget.SetPosition(50, 2);
      textWidget.SetSize(500, 35);
      textWidget.Text = "Advanced Print Settings";
      textWidget.Alignment = QFontAlignment.Left;
      textWidget.Size = FontSize.Large;
      textWidget.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      this.AddChildElement((Element2D) textWidget);
      int x = 10;
      this.AddChildElement((Element2D) this.CreateUserOptionButton(this.host, "OK", AdvancedPrintSettingsFrame.AdvanceSettingsButtons.OK, x, -42, 100, 32, ""));
      int num;
      this.AddChildElement((Element2D) this.CreateUserOptionButton(this.host, "Cancel", AdvancedPrintSettingsFrame.AdvanceSettingsButtons.Cancel, num = x + 110, -42, 100, 32, ""));
      this.AddChildElement((Element2D) this.CreateUserOptionButton(this.host, "Reset", AdvancedPrintSettingsFrame.AdvanceSettingsButtons.Reset, -110, -42, 100, 32, ""));
      this.OK_Button = this.FindChildElement(613) as ButtonWidget;
    }

    private void GenerateFromSlicerSettings(SmartSlicerSettingsBase smartSlicerSettings)
    {
      this.smartSlicerSettings = smartSlicerSettings;
      this.tab_frame = new Frame(608, (Element2D) this);
      this.tab_frame.X = 180;
      this.tab_frame.Y = 35;
      this.tab_frame.HexBorderColor = "#0C0C0C0C";
      this.AddChildElement((Element2D) this.tab_frame);
      int YPos = 35;
      int num = 10000;
      List<XMLTabCollectionSettingsItem> visualSettings = smartSlicerSettings.VisualSettings;
      for (int index = 0; index < visualSettings.Count; ++index)
      {
        this.CreateTabButton(visualSettings[index].Header, ref num, YPos, 181, 64);
        YPos += 64;
      }
      for (int index1 = 0; index1 < visualSettings.Count; ++index1)
      {
        ScrollableVerticalLayout scrollableVerticalLayout = new ScrollableVerticalLayout(num++, (Element2D) this.tab_frame);
        scrollableVerticalLayout.Init(this.host);
        scrollableVerticalLayout.tag = this.GetTabFrameTag(visualSettings[index1].Header);
        scrollableVerticalLayout.Visible = false;
        scrollableVerticalLayout.Enabled = false;
        scrollableVerticalLayout.RelativeWidth = 1f;
        scrollableVerticalLayout.RelativeHeight = 1f;
        for (int index2 = 0; index2 < visualSettings[index1].Items.Count; ++index2)
        {
          XMLSettingsItem settings = visualSettings[index1].Items[index2];
          if (!settings.Name.StartsWith(XMLSetting.MagicInternalString + "Hidden", StringComparison.InvariantCultureIgnoreCase))
          {
            Element2D element;
            switch (settings.SlicerSettingsItem.GetItemType())
            {
              case SettingItemType.IntType:
              case SettingItemType.FloatMMType:
                element = this.CreateTextBox((Frame) scrollableVerticalLayout, settings, ref num);
                break;
              case SettingItemType.BoolType:
                element = this.CreateCheckBox((Frame) scrollableVerticalLayout, settings, ref num);
                break;
              case SettingItemType.FillPatternType:
              case SettingItemType.SupportPatternType:
                element = this.CreateComboBox((Frame) scrollableVerticalLayout, settings, ref num);
                break;
              default:
                throw new NotImplementedException("GenerateAdvancedSettings was given an unknown setting");
            }
            if (element != null)
              this.ProcessValidity(settings.Name, settings.SlicerSettingsItem, element);
          }
        }
        this.tab_frame.AddChildElement((Element2D) scrollableVerticalLayout);
      }
      this.Refresh();
      if (visualSettings.Count == 0)
        return;
      (this.FindChildElement(this.GetTabButtonTag(visualSettings[0].Header)) as ButtonWidget)?.SetChecked(true);
    }

    private void CreateErrorWarningMessage()
    {
      this.ErrorWarningMessage = new TextWidget();
      this.ErrorWarningMessage.SetPosition(300, -45);
      this.ErrorWarningMessage.SetSize(500, 35);
      this.ErrorWarningMessage.Text = "Testing one two three";
      this.ErrorWarningMessage.Alignment = QFontAlignment.Left;
      this.ErrorWarningMessage.Size = FontSize.Large;
      this.ErrorWarningMessage.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      this.ErrorWarningMessage.Visible = false;
      this.AddChildElement((Element2D) this.ErrorWarningMessage);
    }

    private void CreateTabButton(string Header, ref int tagIDBuilder, int YPos, int width, int height)
    {
      ButtonWidget buttonWidget = new ButtonWidget(tagIDBuilder++);
      buttonWidget.Init(this.host, "guicontrols", 448f, 256f, 628f, 319f, 448f, 256f, 628f, 319f, 448f, 384f, 628f, 447f);
      buttonWidget.tag = this.GetTabButtonTag(Header);
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
      buttonWidget.SetCallback(new ButtonCallback(this.MyTabButtonCallback));
      this.AddChildElement((Element2D) buttonWidget);
      this.buttonElements.Add((Element2D) buttonWidget);
      ImageWidget imageWidget1 = new ImageWidget(tagIDBuilder++);
      imageWidget1.tag = this.GetWarningImageTag(Header);
      imageWidget1.Init(this.host, "extendedcontrols", 0.0f, 0.0f, 37f, 30f);
      imageWidget1.SetSize(24, 20);
      imageWidget1.SetPosition(width - (imageWidget1.Width + 10), YPos + 3);
      imageWidget1.Visible = false;
      this.AddChildElement((Element2D) imageWidget1);
      this.buttonElements.Add((Element2D) imageWidget1);
      ImageWidget imageWidget2 = new ImageWidget(tagIDBuilder++);
      imageWidget2.tag = this.GetErrorImageTag(Header);
      imageWidget2.Init(this.host, "extendedcontrols", 38f, 0.0f, 72f, 30f);
      imageWidget2.SetSize(24, 20);
      imageWidget2.SetPosition(width - (imageWidget2.Width + 10), YPos + 3);
      imageWidget2.Visible = false;
      this.AddChildElement((Element2D) imageWidget2);
      this.buttonElements.Add((Element2D) imageWidget2);
    }

    private ButtonWidget CreateUserOptionButton(GUIHost host, string content, AdvancedPrintSettingsFrame.AdvanceSettingsButtons id, int x, int y, int width = 100, int height = 32, string tooltip = "")
    {
      ButtonWidget buttonWidget = new ButtonWidget((int) id);
      buttonWidget.Init(host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      buttonWidget.ID = (int) id;
      buttonWidget.SetPosition(x, y);
      buttonWidget.SetSize(width, height);
      buttonWidget.Text = content;
      buttonWidget.Alignment = QFontAlignment.Centre;
      buttonWidget.Size = FontSize.Medium;
      buttonWidget.ToolTipMessage = tooltip;
      buttonWidget.SetCallback(new ButtonCallback(this.MyTabButtonCallback));
      return buttonWidget;
    }

    private Element2D CreateCheckBox(Frame vertLayout, XMLSettingsItem settings, ref int ID_Builder)
    {
      Frame horzonalFrame = new Frame(ID_Builder++, (Element2D) vertLayout);
      horzonalFrame.RelativeWidth = 1f;
      horzonalFrame.Height = 28;
      this.CreateSettingsTitleElement(horzonalFrame, ID_Builder++, settings.Text);
      ButtonWidget buttonWidget = new ButtonWidget(ID_Builder++, (Element2D) horzonalFrame);
      buttonWidget.Init(this.host, ButtonTemplate.CheckBox);
      buttonWidget.ToolTipMessage = settings.Tooltip;
      buttonWidget.ClickType = ButtonType.Checkable;
      buttonWidget.DontMove = true;
      buttonWidget.CanClickOff = true;
      buttonWidget.Height = 28;
      buttonWidget.Width = 28;
      buttonWidget.X = 245;
      buttonWidget.tag = settings.Name;
      buttonWidget.Checked = bool.Parse(settings.SlicerSettingsItem.TranslateToUserValue());
      buttonWidget.SetCallback(new ButtonCallback(this.CheckBoxCallback));
      buttonWidget.FadeWhenDisabled = true;
      horzonalFrame.AddChildElement((Element2D) buttonWidget);
      vertLayout.AddChildElement((Element2D) horzonalFrame);
      return (Element2D) buttonWidget;
    }

    private Element2D CreateTextBox(Frame vertLayout, XMLSettingsItem settings, ref int ID_Builder)
    {
      IReportFormat slicerSettingsItem = settings.SlicerSettingsItem as IReportFormat;
      NumFormat numFormat = NumFormat.Thousands;
      if (slicerSettingsItem != null)
        numFormat = slicerSettingsItem.Format;
      Frame horzonalFrame = new Frame(ID_Builder++, (Element2D) vertLayout);
      horzonalFrame.RelativeWidth = 1f;
      horzonalFrame.Height = 28;
      this.CreateSettingsTitleElement(horzonalFrame, ID_Builder++, settings.Text);
      EditBoxWidget editBoxWidget = new EditBoxWidget(ID_Builder++, (Element2D) horzonalFrame);
      editBoxWidget.Init(this.host, "guicontrols", 898f, 104f, 941f, 135f);
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
      editBoxWidget.SetCallbackOnTextAdded(new EditBoxWidget.EditBoxCallback(this.EditBoxCallback));
      editBoxWidget.SetCallbackOnBackspace(new EditBoxWidget.EditBoxCallback(this.EditBoxCallback));
      horzonalFrame.AddChildElement((Element2D) editBoxWidget);
      TextWidget textWidget1 = new TextWidget(ID_Builder++, (Element2D) horzonalFrame);
      textWidget1.Width = 65;
      textWidget1.Height = 35;
      textWidget1.X = 325;
      textWidget1.Size = FontSize.Medium;
      textWidget1.HexColor = "#FF7f7f7f";
      textWidget1.Alignment = QFontAlignment.Left;
      textWidget1.Text = settings.Suffix;
      horzonalFrame.AddChildElement((Element2D) textWidget1);
      TextWidget textWidget2 = new TextWidget(ID_Builder++, (Element2D) horzonalFrame);
      textWidget2.Width = 500;
      textWidget2.Height = 35;
      textWidget2.X = 395;
      textWidget2.Size = FontSize.Medium;
      textWidget2.HexColor = "#FF7f7f7f";
      textWidget2.Alignment = QFontAlignment.Left;
      textWidget2.Visible = true;
      textWidget2.tag = this.GetRangeText(settings.Name);
      horzonalFrame.AddChildElement((Element2D) textWidget2);
      vertLayout.AddChildElement((Element2D) horzonalFrame);
      return (Element2D) editBoxWidget;
    }

    private Element2D CreateComboBox(Frame vertLayout, XMLSettingsItem settings, ref int ID_Builder)
    {
      SlicerSettingsEnumItem slicerSettingsItem = settings.SlicerSettingsItem as SlicerSettingsEnumItem;
      Frame horzonalFrame = new Frame(ID_Builder++, (Element2D) vertLayout);
      horzonalFrame.RelativeWidth = 1f;
      horzonalFrame.Height = 28;
      this.CreateSettingsTitleElement(horzonalFrame, ID_Builder++, settings.Text);
      ComboBoxWidget comboBoxWidget = new ComboBoxWidget(ID_Builder++, (Element2D) horzonalFrame);
      comboBoxWidget.Value = (object) slicerSettingsItem.ValueInt;
      comboBoxWidget.tag = settings.Name;
      comboBoxWidget.Init(this.host);
      comboBoxWidget.ItemsEnumString = slicerSettingsItem.EnumType.ToString();
      comboBoxWidget.Select = slicerSettingsItem.ValueInt;
      comboBoxWidget.ToolTipMessage = settings.Tooltip;
      comboBoxWidget.Height = 24;
      comboBoxWidget.Width = 240;
      comboBoxWidget.X = 245;
      comboBoxWidget.Size = FontSize.Medium;
      comboBoxWidget.HexColor = "#FF808080";
      comboBoxWidget.TextChangedCallback += new ComboBoxWidget.ComboBoxTextChangedCallback(this.comboBoxChangedCallBack);
      horzonalFrame.AddChildElement((Element2D) comboBoxWidget);
      vertLayout.AddChildElement((Element2D) horzonalFrame);
      return (Element2D) comboBoxWidget;
    }

    private void CreateSettingsTitleElement(Frame horzonalFrame, int id, string text)
    {
      TextWidget textWidget = new TextWidget(id, (Element2D) horzonalFrame);
      textWidget.Text = text;
      textWidget.Size = FontSize.Medium;
      textWidget.HexColor = "#FF7f7f7f";
      textWidget.Alignment = QFontAlignment.Left;
      textWidget.CenterVerticallyInParent = true;
      textWidget.Height = 35;
      textWidget.Width = 240;
      textWidget.FadeWhenDisabled = true;
      horzonalFrame.AddChildElement((Element2D) textWidget);
    }

    private string GetTabButtonTag(string tag)
    {
      tag = this.RemoveOtherElementNameFlag(tag);
      return tag + "_TabButton";
    }

    private string GetRangeText(string tag)
    {
      tag = this.RemoveOtherElementNameFlag(tag);
      return tag + "_RangeText";
    }

    private string GetTabFrameTag(string tag)
    {
      tag = this.RemoveOtherElementNameFlag(tag);
      return tag + "_Frame";
    }

    private string GetErrorImageTag(string tag)
    {
      tag = this.RemoveOtherElementNameFlag(tag);
      return tag + "_ErrorImage";
    }

    private string GetWarningImageTag(string tag)
    {
      tag = this.RemoveOtherElementNameFlag(tag);
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
      foreach (string str in strArray)
      {
        if (tag.EndsWith(str))
          return tag.Substring(0, tag.Length - str.Length);
      }
      return tag;
    }

    public void MyTabButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 611:
          this.SlicerSettings.SetToDefault();
          this.syncAllSettingWithGUI();
          break;
        case 612:
          this.SlicerConnection.SlicerSettingStack.PopSettings();
          this.PrintDialogWindow.ActivateFrame(PrintDialogWidgetFrames.PrintDialogFrame, this.CurrentJobDetails);
          break;
        case 613:
          this.SlicerConnection.SlicerSettingStack.SaveSettingsDown();
          this.PrintDialogWindow.ActivateFrame(PrintDialogWidgetFrames.PrintDialogFrame, this.CurrentJobDetails);
          break;
        default:
          this.ShowFrame(button.tag);
          break;
      }
    }

    private void ShowFrame(string tag)
    {
      Frame childElement = this.FindChildElement(this.GetTabFrameTag(tag)) as Frame;
      if (childElement == null)
        return;
      this.ActivateFrame(childElement);
    }

    private void ActivateFrame(Frame frame)
    {
      if (this.active_frame != null)
      {
        this.active_frame.Visible = false;
        this.active_frame.Enabled = false;
        this.active_frame = (Frame) null;
      }
      this.active_frame = frame;
      if (this.active_frame == null)
        return;
      this.active_frame.Enabled = true;
      this.active_frame.Visible = true;
    }

    private void CheckBoxCallback(ButtonWidget button)
    {
      string tag = button.tag;
      if (tag.StartsWith("InternalToGUI_"))
      {
        PrinterObject selectedPrinter = this.SelectedPrinter;
        FilamentSpool filament = (FilamentSpool) null;
        if (selectedPrinter != null)
          filament = selectedPrinter.GetCurrentFilament();
        this.SlicerConnection.SlicerSettings.SmartCheckBoxCallBack(tag, button.Checked, filament);
        this.syncAllSettingWithGUI();
      }
      else
      {
        SlicerSettingsItem smartSlicerSetting = this.smartSlicerSettings[tag];
        smartSlicerSetting.ParseUserValue(button.Value.ToString());
        this.ProcessValidity(tag, smartSlicerSetting, (Element2D) button);
      }
    }

    private void EditBoxCallback(EditBoxWidget edit)
    {
      string tag = edit.tag;
      SlicerSettingsItem smartSlicerSetting = this.smartSlicerSettings[tag];
      smartSlicerSetting.ParseUserValue(edit.Value.ToString());
      this.ProcessValidity(tag, smartSlicerSetting, (Element2D) edit);
    }

    private void comboBoxChangedCallBack(ComboBoxWidget combobox)
    {
      string tag = combobox.tag;
      SlicerSettingsItem smartSlicerSetting = this.smartSlicerSettings[tag];
      smartSlicerSetting.ParseUserValue(combobox.Value.ToString());
      this.ProcessValidity(tag, smartSlicerSetting, (Element2D) combobox);
    }

    private void syncAllSettingWithGUI()
    {
      int count = this.SlicerSettings.Count;
      foreach (System.Collections.Generic.KeyValuePair<string, SlicerSettingsItem> slicerSetting in this.SlicerSettings)
        this.UpdateGUIFromSetting(slicerSetting.Key, slicerSetting.Value);
    }

    private void UpdateGUIFromSetting(string name, SlicerSettingsItem setting)
    {
      if (this.tab_frame == null)
        return;
      Element2D childElement = this.tab_frame.FindChildElement(name);
      if (childElement == null)
        return;
      string userValue = setting.TranslateToUserValue();
      switch (setting.GetItemType())
      {
        case SettingItemType.IntType:
        case SettingItemType.FloatMMType:
          EditBoxWidget editBoxWidget = childElement as EditBoxWidget;
          if (editBoxWidget != null)
          {
            editBoxWidget.Text = userValue;
            break;
          }
          break;
        case SettingItemType.BoolType:
          ButtonWidget buttonWidget = childElement as ButtonWidget;
          if (buttonWidget != null)
          {
            buttonWidget.Checked = bool.Parse(userValue);
            break;
          }
          break;
        case SettingItemType.FillPatternType:
        case SettingItemType.SupportPatternType:
          ComboBoxWidget comboBoxWidget = childElement as ComboBoxWidget;
          SlicerSettingsEnumItem settingsEnumItem = setting as SlicerSettingsEnumItem;
          if (comboBoxWidget != null && settingsEnumItem != null)
          {
            comboBoxWidget.Select = settingsEnumItem.ValueInt;
            break;
          }
          break;
        default:
          throw new NotImplementedException("GenerateAdvancedSettings was given an unknown setting");
      }
      this.ProcessValidity(name, setting, childElement);
    }

    private void ProcessValidity(string settingName, SlicerSettingsItem setting, Element2D element)
    {
      Element2D parent = element.Parent;
      while (parent != null && string.IsNullOrEmpty(parent.tag))
        parent = parent.Parent;
      if (parent == null)
        return;
      string tag = parent.tag;
      AdvancedPrintSettingsFrame.ErrorStateEnum oldState;
      AdvancedPrintSettingsFrame.ErrorStateEnum currentState;
      int num = this.GetStateChangeInfo(element, settingName, setting, tag, out oldState, out currentState) ? 1 : 0;
      Element2D childElement = element.Parent.FindChildElement(this.GetRangeText(settingName));
      if (num != 0)
      {
        AdvancedPrintSettingsFrame.TabErrorStateCount tabErrorStateCount = this.tabsErrorWarningCount[tag];
        switch (oldState)
        {
          case AdvancedPrintSettingsFrame.ErrorStateEnum.Warning:
            --tabErrorStateCount.Warnings;
            --this.globalWarningCount;
            if (tabErrorStateCount.Warnings == 0)
            {
              this.image_Helper(this.GetWarningImageTag(tag), false);
              break;
            }
            break;
          case AdvancedPrintSettingsFrame.ErrorStateEnum.Error:
            --tabErrorStateCount.Errors;
            --this.globalErrorCount;
            if (tabErrorStateCount.Errors == 0)
            {
              this.image_Helper(this.GetErrorImageTag(tag), false);
              break;
            }
            break;
        }
        Color4 color4 = this.DEFAULT_COLOR;
        bool flag = true;
        switch (currentState)
        {
          case AdvancedPrintSettingsFrame.ErrorStateEnum.Warning:
            color4 = this.WARNING_COLOR;
            ++tabErrorStateCount.Warnings;
            ++this.globalWarningCount;
            if (tabErrorStateCount.Warnings == 1)
            {
              this.image_Helper(this.GetWarningImageTag(tag), true);
              break;
            }
            break;
          case AdvancedPrintSettingsFrame.ErrorStateEnum.Error:
            color4 = this.ERROR_COLOR;
            ++tabErrorStateCount.Errors;
            ++this.globalErrorCount;
            if (tabErrorStateCount.Errors == 1)
            {
              this.image_Helper(this.GetErrorImageTag(tag), true);
              break;
            }
            break;
          default:
            flag = false;
            break;
        }
        if (element is EditBoxWidget)
          (element as EditBoxWidget).Color = color4;
        if (childElement is TextWidget)
        {
          TextWidget textWidget = childElement as TextWidget;
          textWidget.Color = color4;
          textWidget.Visible = flag;
        }
        this.UpdateErrorWarningMessage();
      }
      if (!(childElement is TextWidget))
        return;
      (childElement as TextWidget).Text = setting.GetErrorMsg();
    }

    private void UpdateErrorWarningMessage()
    {
      bool flag = false;
      if (this.globalErrorCount == 1)
      {
        this.OK_Button.Enabled = false;
        this.ErrorWarningMessage.Visible = true;
        this.ErrorWarningMessage.Text = "Error on settings";
        this.ErrorWarningMessage.Color = this.ERROR_COLOR;
      }
      else if (this.globalErrorCount == 0)
      {
        this.ErrorWarningMessage.Visible = false;
        this.OK_Button.Enabled = true;
        flag = this.globalWarningCount > 1;
      }
      if (this.globalWarningCount == 1 | flag && this.globalErrorCount == 0)
      {
        this.ErrorWarningMessage.Visible = true;
        this.ErrorWarningMessage.Text = "Warning on settings";
        this.ErrorWarningMessage.Color = this.WARNING_COLOR;
      }
      if (this.globalWarningCount != 0 || this.globalErrorCount != 0)
        return;
      this.ErrorWarningMessage.Visible = false;
    }

    private bool GetStateChangeInfo(Element2D element, string settingName, SlicerSettingsItem setting, string parentTag, out AdvancedPrintSettingsFrame.ErrorStateEnum oldState, out AdvancedPrintSettingsFrame.ErrorStateEnum currentState)
    {
      if (!this.tabsErrorWarningCount.ContainsKey(parentTag))
        this.tabsErrorWarningCount[parentTag] = new AdvancedPrintSettingsFrame.TabErrorStateCount();
      oldState = AdvancedPrintSettingsFrame.ErrorStateEnum.AllGood;
      bool flag = false;
      if (this.settingErrorWarningState.ContainsKey(settingName))
        oldState = this.settingErrorWarningState[settingName];
      if (setting.HasError && setting.IsSettingOn)
      {
        currentState = AdvancedPrintSettingsFrame.ErrorStateEnum.Error;
        flag = true;
        if (!this.settingErrorWarningState.ContainsKey(settingName))
        {
          this.settingErrorWarningState.Add(settingName, AdvancedPrintSettingsFrame.ErrorStateEnum.Error);
        }
        else
        {
          flag = this.settingErrorWarningState[settingName] != AdvancedPrintSettingsFrame.ErrorStateEnum.Error;
          this.settingErrorWarningState[settingName] = AdvancedPrintSettingsFrame.ErrorStateEnum.Error;
        }
      }
      else if (setting.HasWarning && setting.IsSettingOn)
      {
        currentState = AdvancedPrintSettingsFrame.ErrorStateEnum.Warning;
        flag = true;
        if (!this.settingErrorWarningState.ContainsKey(settingName))
        {
          this.settingErrorWarningState.Add(settingName, AdvancedPrintSettingsFrame.ErrorStateEnum.Warning);
        }
        else
        {
          flag = this.settingErrorWarningState[settingName] != AdvancedPrintSettingsFrame.ErrorStateEnum.Warning;
          this.settingErrorWarningState[settingName] = AdvancedPrintSettingsFrame.ErrorStateEnum.Warning;
        }
      }
      else
      {
        currentState = AdvancedPrintSettingsFrame.ErrorStateEnum.AllGood;
        if (this.settingErrorWarningState.ContainsKey(settingName))
        {
          this.settingErrorWarningState.Remove(settingName);
          flag = true;
        }
        element.Enabled = setting.IsSettingOn;
        if (!element.Enabled)
          flag = true;
      }
      return flag;
    }

    private void image_Helper(string name, bool visibility)
    {
      ImageWidget childElement = this.FindChildElement(name) as ImageWidget;
      if (childElement == null)
        return;
      childElement.Visible = visibility;
    }

    public override void OnParentResize()
    {
      if (this.tab_frame != null)
        this.tab_frame.SetSize(this.Width - this.tab_frame.X, this.Height - this.tab_frame.Y - 60);
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
