// Decompiled with JetBrains decompiler
// Type: M3D.GUI.SettingsPages.FeaturePanel
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      : this(ID, host, spooler_connection, (PrinterObject) null)
    {
    }

    public FeaturePanel(int ID, GUIHost host, SpoolerConnection spooler_connection, PrinterObject exclusive)
      : base(ID)
    {
      this.spooler_connection = spooler_connection;
      this.mExclusivePrinter = exclusive;
      this.Init(host);
      this.mProFeaturesFrame = (Frame) this.FindChildElement(2000);
      this.mProUnavailableFrame = (Frame) this.FindChildElement(3000);
      this.mProUnavailableFrame.Visible = true;
      this.mProFeaturesFrame.Visible = false;
      this.Visible = false;
      this.Enabled = false;
    }

    private void Init(GUIHost host)
    {
      this.mHost = host;
      string featurePanel = Resources.FeaturePanel;
      this.Init(host, featurePanel, new ButtonCallback(this.MyButtonCallback));
      this.featureListBox = (ScrollableVerticalLayout) this.FindChildElement(2101);
      this.RelativeWidth = 1f;
      this.RelativeHeight = 1f;
    }

    public override void SetVisible(bool bVisible)
    {
      if (bVisible && !this.Visible)
        this.CheckProAvailability();
      else
        this.currentPrinter = (PrinterObject) null;
      base.SetVisible(bVisible);
    }

    private void CheckProAvailability()
    {
      PrinterObject printer = this.spooler_connection.SelectedPrinter;
      if (this.mExclusivePrinter != null)
        printer = !this.mExclusivePrinter.isConnected() ? (PrinterObject) null : this.mExclusivePrinter;
      if (this.currentPrinter == printer)
        return;
      if (printer == null || !printer.Info.supportedFeatures.UsesSupportedFeatures)
      {
        if (!this.mProUnavailableFrame.Visible)
        {
          this.mProUnavailableFrame.Visible = true;
          this.mProFeaturesFrame.Visible = false;
          this.featureListBox.RemoveAllChildElements();
        }
      }
      else if (!this.mProFeaturesFrame.Visible)
      {
        this.mProUnavailableFrame.Visible = false;
        this.mProFeaturesFrame.Visible = true;
        this.AddFeatureButtons(printer);
      }
      else
      {
        this.featureListBox.RemoveAllChildElements();
        this.AddFeatureButtons(printer);
      }
      this.currentPrinter = printer;
    }

    private void AddFeatureButtons(PrinterObject printer)
    {
      this.featureListBox.RemoveAllChildElements();
      foreach (KeyValuePair<string, int> enumerate in printer.MyPrinterProfile.SupportedFeaturesConstants.EnumerateList())
      {
        string key = enumerate.Key;
        int feature_slot = enumerate.Value;
        SupportedFeatures.Status status = printer.Info.supportedFeatures.GetStatus(feature_slot);
        this.featureListBox.AddChildElement((Element2D) this.CreateFeatureButton(this.mHost, enumerate, status));
      }
    }

    public override void OnUpdate()
    {
      this.CheckProAvailability();
      base.OnUpdate();
    }

    private ButtonWidget CreateFeatureButton(GUIHost host, KeyValuePair<string, int> featureNameSlotPair, SupportedFeatures.Status status)
    {
      ButtonWidget buttonWidget = new ButtonWidget(featureNameSlotPair.Value);
      buttonWidget.SetSize(300, 29);
      buttonWidget.ImageAreaWidth = 29;
      buttonWidget.Alignment = QFontAlignment.Justify;
      buttonWidget.Size = FontSize.Medium;
      buttonWidget.Text = "  " + featureNameSlotPair.Key;
      buttonWidget.SetCallback(new ButtonCallback(this.MyButtonCallback));
      float u0 = 0.0f;
      float v0 = 0.0f;
      float u1 = 0.0f;
      float v1 = 0.0f;
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
