// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.EditViewControlBar
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using OpenTK.Graphics;

namespace M3D.GUI.Views.Printer_View
{
  internal class EditViewControlBar : Frame
  {
    private ButtonWidget resetview_button;
    private ButtonWidget print_button;
    private ButtonWidget centermodel_button;
    private ButtonWidget backtolibrary_button;
    private PrinterView printerview;

    public EditViewControlBar(int ID, PrinterView printerview)
      : this(ID, printerview, (Element2D) null)
    {
    }

    public EditViewControlBar(int ID, PrinterView printerview, Element2D parent)
      : base(ID, parent)
    {
      this.printerview = printerview;
    }

    public void Init(GUIHost host)
    {
      this.resetview_button = new ButtonWidget(8002);
      this.resetview_button.Init(host, "guicontrols", (float) sbyte.MaxValue, 257f, 275f, 340f, (float) sbyte.MaxValue, 342f, 275f, 425f, (float) sbyte.MaxValue, 427f, 275f, 510f, (float) sbyte.MaxValue, 512f, 275f, 595f);
      this.resetview_button.Text = "";
      this.resetview_button.Size = FontSize.Medium;
      this.resetview_button.TextColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      this.resetview_button.TextDownColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      this.resetview_button.TextOverColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      this.resetview_button.Width = 149;
      this.resetview_button.Height = 84;
      this.resetview_button.Y = 6;
      this.resetview_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.resetview_button.DontMove = true;
      this.resetview_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_RESETVIEW");
      this.print_button = new ButtonWidget(8003);
      this.print_button.Init(host, "guicontrols", 1f, 597f, 102f, 694f, 104f, 597f, 205f, 694f, 207f, 597f, 308f, 694f, 310f, 597f, 411f, 694f);
      this.print_button.Text = "";
      this.print_button.Size = FontSize.Medium;
      this.print_button.TextColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      this.print_button.TextDownColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      this.print_button.TextOverColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      this.print_button.Width = 102;
      this.print_button.Height = 98;
      this.print_button.Y = 0;
      this.print_button.X = this.resetview_button.Width;
      this.print_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.print_button.DontMove = true;
      this.print_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_PRINT");
      this.centermodel_button = new ButtonWidget(8004);
      this.centermodel_button.Init(host, "guicontrols", 277f, 257f, 427f, 340f, 277f, 342f, 427f, 425f, 277f, 427f, 427f, 510f, 277f, 512f, 427f, 595f);
      this.centermodel_button.Text = "";
      this.centermodel_button.Size = FontSize.Medium;
      this.centermodel_button.TextColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      this.centermodel_button.TextDownColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      this.centermodel_button.TextOverColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      this.centermodel_button.Width = 151;
      this.centermodel_button.Height = 84;
      this.centermodel_button.Y = 6;
      this.centermodel_button.X = this.print_button.X + this.print_button.Width;
      this.centermodel_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.centermodel_button.DontMove = true;
      this.centermodel_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_CENTERMODEL");
      Frame frame = new Frame();
      frame.Width = this.resetview_button.Width + this.print_button.Width + this.centermodel_button.Width;
      frame.Height = this.print_button.Height;
      frame.AddChildElement((Element2D) this.resetview_button);
      frame.AddChildElement((Element2D) this.print_button);
      frame.AddChildElement((Element2D) this.centermodel_button);
      frame.Y = 0;
      frame.CenterHorizontallyInParent = true;
      this.backtolibrary_button = new ButtonWidget(8005);
      this.backtolibrary_button.Init(host, "guicontrols", 1f, 257f, 125f, 340f, 1f, 342f, 125f, 425f, 1f, 427f, 125f, 510f, 1f, 512f, 125f, 595f);
      this.backtolibrary_button.Text = "";
      this.backtolibrary_button.Size = FontSize.Medium;
      this.backtolibrary_button.TextColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      this.backtolibrary_button.TextDownColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      this.backtolibrary_button.TextOverColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      this.backtolibrary_button.Width = 125;
      this.backtolibrary_button.Height = 84;
      this.backtolibrary_button.X = -125;
      this.backtolibrary_button.Y = 6;
      this.backtolibrary_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.backtolibrary_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_BACKTOLIBRARY");
      Sprite.pixel_perfect = false;
      this.AddChildElement((Element2D) frame);
      this.AddChildElement((Element2D) this.backtolibrary_button);
      this.X = 0;
      this.Y = -121;
      this.Height = frame.Height;
      this.RelativeWidth = 1f;
      this.RelativeHeight = -1000f;
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 8002:
          this.printerview.ResetPrinterView();
          break;
        case 8003:
          this.printerview.Print();
          break;
        case 8004:
          this.printerview.CenterPrinterObject();
          break;
        case 8005:
          this.printerview.GotoLibraryView();
          break;
      }
    }

    public void EnableButtons(bool print, bool center)
    {
      if (this.print_button != null)
        this.print_button.Enabled = print;
      if (this.centermodel_button == null)
        return;
      this.centermodel_button.Enabled = center;
    }

    public void SetControlStateMaster(bool bShouldDisable)
    {
      if (this.centermodel_button == null)
        return;
      this.centermodel_button.Visible = !bShouldDisable;
    }

    public enum ControlIDs
    {
      Static = 0,
      resetview_button = 8002, // 0x00001F42
      print_button = 8003, // 0x00001F43
      centermodel_button = 8004, // 0x00001F44
      backtolibrary_button = 8005, // 0x00001F45
    }
  }
}
