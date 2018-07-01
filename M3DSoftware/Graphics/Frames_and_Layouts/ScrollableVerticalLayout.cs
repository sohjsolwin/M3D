// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Frames_and_Layouts.ScrollableVerticalLayout
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.Widgets2D;
using System.Xml.Serialization;

namespace M3D.Graphics.Frames_and_Layouts
{
  public class ScrollableVerticalLayout : GenericScrollFrame<VerticalLayout>
  {
    private const int ChildWidthDiff = 32;

    public ScrollableVerticalLayout()
      : this(0, (Element2D) null)
    {
    }

    public ScrollableVerticalLayout(int ID)
      : this(ID, (Element2D) null)
    {
    }

    public ScrollableVerticalLayout(int ID, Element2D parent)
      : base(ID, parent)
    {
      this.ScollableChildframe.layoutMode = Layout.LayoutMode.ResizeLayoutToFitChildren;
    }

    public override void OnControlMsg(Element2D the_control, ControlMsg msg, float xparam, float yparam)
    {
      if (msg == ControlMsg.LAYOUT_RESIZED_BY_CHILDREN)
        this.Refresh();
      else
        base.OnControlMsg(the_control, msg, xparam, yparam);
    }

    public new void Init(GUIHost host)
    {
      this.InitChildren(this.Parent, host, (ButtonCallback) null);
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      base.InitChildren(parent, host, MyButtonCallback);
      this.ScollableChildframe.RelativeWidth = -1f;
      this.ScollableChildframe.RelativeHeight = -1f;
    }

    [XmlAttribute("border-width")]
    public int BorderWidth
    {
      get
      {
        return this.ScollableChildframe.BorderWidth;
      }
      set
      {
        this.ScollableChildframe.BorderWidth = value;
      }
    }

    [XmlAttribute("border-height")]
    public int BorderHeight
    {
      get
      {
        return this.ScollableChildframe.BorderHeight;
      }
      set
      {
        this.ScollableChildframe.BorderHeight = value;
      }
    }

    [XmlAttribute("layout-mode")]
    public Layout.LayoutMode layoutMode
    {
      get
      {
        return this.ScollableChildframe.layoutMode;
      }
      set
      {
        this.ScollableChildframe.layoutMode = value;
      }
    }

    public override void OnParentResize()
    {
      this.ScollableChildframe.Width = this.Width - 32;
      base.OnParentResize();
    }
  }
}
