using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.Properties;
using M3D.Spooling.Common;
using QuickFont;
using System.Collections.Generic;

namespace M3D.GUI.SettingsPages
{
  internal class FeaturePanel : SettingsPage
  {
    private PrinterObject currentPrinter;
    private GUIHost mHost;
    private Frame mProFeaturesFrame;
    private Frame mProUnavailableFrame;
    private ScrollableVerticalLayout featureListBox;
    private SpoolerConnection spooler_connection;
    private PrinterObject mExclusivePrinter;

    public FeaturePanel(int ID, GUIHost host, SpoolerConnection spooler_connection)
      : this(ID, host, spooler_connection, null)
    {
    }

    public FeaturePanel(int ID, GUIHost host, SpoolerConnection spooler_connection, PrinterObject exclusive)
      : base(ID)
    {
      this.spooler_connection = spooler_connection;
      mExclusivePrinter = exclusive;
      Init(host);
      mProFeaturesFrame = (Frame)FindChildElement(2000);
      mProUnavailableFrame = (Frame)FindChildElement(3000);
      mProUnavailableFrame.Visible = true;
      mProFeaturesFrame.Visible = false;
      Visible = false;
      Enabled = false;
    }

    private void Init(GUIHost host)
    {
      mHost = host;
      var featurePanel = Resources.FeaturePanel;
      Init(host, featurePanel, new ButtonCallback(MyButtonCallback));
      featureListBox = (ScrollableVerticalLayout)FindChildElement(2101);
      RelativeWidth = 1f;
      RelativeHeight = 1f;
    }

    public override void SetVisible(bool bVisible)
    {
      if (bVisible && !Visible)
      {
        CheckProAvailability();
      }
      else
      {
        currentPrinter = null;
      }

      base.SetVisible(bVisible);
    }

    private void CheckProAvailability()
    {
      PrinterObject printer = spooler_connection.SelectedPrinter;
      if (mExclusivePrinter != null)
      {
        printer = !mExclusivePrinter.IsConnected() ? null : mExclusivePrinter;
      }

      if (currentPrinter == printer)
      {
        return;
      }

      if (printer == null || !printer.Info.supportedFeatures.UsesSupportedFeatures)
      {
        if (!mProUnavailableFrame.Visible)
        {
          mProUnavailableFrame.Visible = true;
          mProFeaturesFrame.Visible = false;
          featureListBox.RemoveAllChildElements();
        }
      }
      else if (!mProFeaturesFrame.Visible)
      {
        mProUnavailableFrame.Visible = false;
        mProFeaturesFrame.Visible = true;
        AddFeatureButtons(printer);
      }
      else
      {
        featureListBox.RemoveAllChildElements();
        AddFeatureButtons(printer);
      }
      currentPrinter = printer;
    }

    private void AddFeatureButtons(PrinterObject printer)
    {
      featureListBox.RemoveAllChildElements();
      foreach (KeyValuePair<string, int> enumerate in printer.MyPrinterProfile.SupportedFeaturesConstants.EnumerateList())
      {
        var key = enumerate.Key;
        var feature_slot = enumerate.Value;
        SupportedFeatures.Status status = printer.Info.supportedFeatures.GetStatus(feature_slot);
        featureListBox.AddChildElement(CreateFeatureButton(mHost, enumerate, status));
      }
    }

    public override void OnUpdate()
    {
      CheckProAvailability();
      base.OnUpdate();
    }

    private ButtonWidget CreateFeatureButton(GUIHost host, KeyValuePair<string, int> featureNameSlotPair, SupportedFeatures.Status status)
    {
      var buttonWidget = new ButtonWidget(featureNameSlotPair.Value);
      buttonWidget.SetSize(300, 29);
      buttonWidget.ImageAreaWidth = 29;
      buttonWidget.Alignment = QFontAlignment.Justify;
      buttonWidget.Size = FontSize.Medium;
      buttonWidget.Text = "  " + featureNameSlotPair.Key;
      buttonWidget.SetCallback(new ButtonCallback(MyButtonCallback));
      var u0 = 0.0f;
      var v0 = 0.0f;
      var u1 = 0.0f;
      var v1 = 0.0f;
      switch (status)
      {
        case SupportedFeatures.Status.Unavailable:
          u0 = 63f;
          v0 = 65f;
          u1 = 91f;
          v1 = 94f;
          break;
        case SupportedFeatures.Status.DevelopmentalFeature:
          u0 = 32f;
          v0 = 65f;
          u1 = 60f;
          v1 = 94f;
          break;
        case SupportedFeatures.Status.Available:
          u0 = 1f;
          v0 = 65f;
          u1 = 29f;
          v1 = 94f;
          break;
      }
      buttonWidget.Init(host, "extendedcontrols", u0, v0, u1, v1);
      return buttonWidget;
    }

    private void MyButtonCallback(ButtonWidget button)
    {
    }

    private enum ControlIDs
    {
      FeaturesFrame = 2000, // 0x000007D0
      FeatureListBox = 2101, // 0x00000835
      UnavailableFrame = 3000, // 0x00000BB8
    }
  }
}
