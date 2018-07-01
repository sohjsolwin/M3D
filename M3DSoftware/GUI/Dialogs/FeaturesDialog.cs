// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Dialogs.FeaturesDialog
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.SettingsPages;
using M3D.Properties;
using M3D.Spooling.Client;

namespace M3D.GUI.Dialogs
{
  public static class FeaturesDialog
  {
    private static FeaturePanel featurePanel;

    public static void Show(PopupMessageBox messagebox, SpoolerConnection spoolerConnection, PrinterObject printer)
    {
      messagebox.AddXMLMessageToQueue(new PopupMessageBox.MessageDataXML(Resources.FeaturesDialogFrame, new PopupMessageBox.XMLButtonCallback(FeaturesDialog.XMLButtonCallback), new ElementStandardDelegate(FeaturesDialog.XMLOnUpdateCallback), new PopupMessageBox.XMLOnShow(FeaturesDialog.XMLOnShow), (object) new FeaturesDialog.ProFeaturesDialogData(spoolerConnection, printer)));
    }

    private static void XMLOnShow(PopupMessageBox parentFrame, XMLFrame childFrame, GUIHost host, object data)
    {
      FeaturesDialog.ProFeaturesDialogData featuresDialogData = data as FeaturesDialog.ProFeaturesDialogData;
      ((TextWidget) childFrame.FindChildElement("FeaturePanel::Title")).Text = string.Format("{0} Features", (object) featuresDialogData.printer.MyPrinterProfile.ProfileName);
      ((TextWidget) childFrame.FindChildElement("FeaturePanel::Desc")).Text = string.Format("Here are the {0} features currently available for your printer.", (object) featuresDialogData.printer.MyPrinterProfile.ProfileName);
      FeaturesDialog.featurePanel = new FeaturePanel(1004, host, featuresDialogData.spoolerConnection, featuresDialogData.printer);
      FeaturesDialog.featurePanel.Visible = true;
      FeaturesDialog.featurePanel.Enabled = true;
      childFrame.FindChildElement(1003).AddChildElement((Element2D) FeaturesDialog.featurePanel);
    }

    private static void XMLButtonCallback(ButtonWidget button, SpoolerMessage message, PopupMessageBox parentFrame, XMLFrame childFrame, object data)
    {
      parentFrame.CloseCurrent();
    }

    private static void XMLOnUpdateCallback()
    {
    }

    private class ProFeaturesDialogData
    {
      public SpoolerConnection spoolerConnection;
      public PrinterObject printer;

      public ProFeaturesDialogData(SpoolerConnection spoolerConnection, PrinterObject printer)
      {
        this.spoolerConnection = spoolerConnection;
        this.printer = printer;
      }
    }
  }
}
