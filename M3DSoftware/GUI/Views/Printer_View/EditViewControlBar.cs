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
      : this(ID, printerview, null)
    {
    }

    public EditViewControlBar(int ID, PrinterView printerview, Element2D parent)
      : base(ID, parent)
    {
      this.printerview = printerview;
    }

    public void Init(GUIHost host)
    {
      resetview_button = new ButtonWidget(8002);
      resetview_button.Init(host, "guicontrols", sbyte.MaxValue, 257f, 275f, 340f, sbyte.MaxValue, 342f, 275f, 425f, sbyte.MaxValue, 427f, 275f, 510f, sbyte.MaxValue, 512f, 275f, 595f);
      resetview_button.Text = "";
      resetview_button.Size = FontSize.Medium;
      resetview_button.TextColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      resetview_button.TextDownColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      resetview_button.TextOverColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      resetview_button.Width = 149;
      resetview_button.Height = 84;
      resetview_button.Y = 6;
      resetview_button.SetCallback(new ButtonCallback(MyButtonCallback));
      resetview_button.DontMove = true;
      resetview_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_RESETVIEW");
      print_button = new ButtonWidget(8003);
      print_button.Init(host, "guicontrols", 1f, 597f, 102f, 694f, 104f, 597f, 205f, 694f, 207f, 597f, 308f, 694f, 310f, 597f, 411f, 694f);
      print_button.Text = "";
      print_button.Size = FontSize.Medium;
      print_button.TextColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      print_button.TextDownColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      print_button.TextOverColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      print_button.Width = 102;
      print_button.Height = 98;
      print_button.Y = 0;
      print_button.X = resetview_button.Width;
      print_button.SetCallback(new ButtonCallback(MyButtonCallback));
      print_button.DontMove = true;
      print_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_PRINT");
      centermodel_button = new ButtonWidget(8004);
      centermodel_button.Init(host, "guicontrols", 277f, 257f, 427f, 340f, 277f, 342f, 427f, 425f, 277f, 427f, 427f, 510f, 277f, 512f, 427f, 595f);
      centermodel_button.Text = "";
      centermodel_button.Size = FontSize.Medium;
      centermodel_button.TextColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      centermodel_button.TextDownColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      centermodel_button.TextOverColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      centermodel_button.Width = 151;
      centermodel_button.Height = 84;
      centermodel_button.Y = 6;
      centermodel_button.X = print_button.X + print_button.Width;
      centermodel_button.SetCallback(new ButtonCallback(MyButtonCallback));
      centermodel_button.DontMove = true;
      centermodel_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_CENTERMODEL");
      var frame = new Frame
      {
        Width = resetview_button.Width + print_button.Width + centermodel_button.Width,
        Height = print_button.Height
      };
      frame.AddChildElement(resetview_button);
      frame.AddChildElement(print_button);
      frame.AddChildElement(centermodel_button);
      frame.Y = 0;
      frame.CenterHorizontallyInParent = true;
      backtolibrary_button = new ButtonWidget(8005);
      backtolibrary_button.Init(host, "guicontrols", 1f, 257f, 125f, 340f, 1f, 342f, 125f, 425f, 1f, 427f, 125f, 510f, 1f, 512f, 125f, 595f);
      backtolibrary_button.Text = "";
      backtolibrary_button.Size = FontSize.Medium;
      backtolibrary_button.TextColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      backtolibrary_button.TextDownColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      backtolibrary_button.TextOverColor = new Color4(0.0f, 0.0f, 0.0f, 1f);
      backtolibrary_button.Width = 125;
      backtolibrary_button.Height = 84;
      backtolibrary_button.X = -125;
      backtolibrary_button.Y = 6;
      backtolibrary_button.SetCallback(new ButtonCallback(MyButtonCallback));
      backtolibrary_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_BACKTOLIBRARY");
      Sprite.pixel_perfect = false;
      AddChildElement(frame);
      AddChildElement(backtolibrary_button);
      X = 0;
      Y = -121;
      Height = frame.Height;
      RelativeWidth = 1f;
      RelativeHeight = -1000f;
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 8002:
          printerview.ResetPrinterView();
          break;
        case 8003:
          printerview.Print();
          break;
        case 8004:
          printerview.CenterPrinterObject();
          break;
        case 8005:
          printerview.GotoLibraryView();
          break;
      }
    }

    public void EnableButtons(bool print, bool center)
    {
      if (print_button != null)
      {
        print_button.Enabled = print;
      }

      if (centermodel_button == null)
      {
        return;
      }

      centermodel_button.Enabled = center;
    }

    public void SetControlStateMaster(bool bShouldDisable)
    {
      if (centermodel_button == null)
      {
        return;
      }

      centermodel_button.Visible = !bShouldDisable;
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
