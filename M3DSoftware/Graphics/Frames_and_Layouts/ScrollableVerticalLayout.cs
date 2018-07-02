using M3D.Graphics.Widgets2D;
using System.Xml.Serialization;

namespace M3D.Graphics.Frames_and_Layouts
{
  public class ScrollableVerticalLayout : GenericScrollFrame<VerticalLayout>
  {
    private const int ChildWidthDiff = 32;

    public ScrollableVerticalLayout()
      : this(0, null)
    {
    }

    public ScrollableVerticalLayout(int ID)
      : this(ID, null)
    {
    }

    public ScrollableVerticalLayout(int ID, Element2D parent)
      : base(ID, parent)
    {
      ScollableChildframe.layoutMode = Layout.LayoutMode.ResizeLayoutToFitChildren;
    }

    public override void OnControlMsg(Element2D the_control, ControlMsg msg, float xparam, float yparam)
    {
      if (msg == ControlMsg.LAYOUT_RESIZED_BY_CHILDREN)
      {
        Refresh();
      }
      else
      {
        base.OnControlMsg(the_control, msg, xparam, yparam);
      }
    }

    public new void Init(GUIHost host)
    {
      InitChildren(Parent, host, null);
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      base.InitChildren(parent, host, MyButtonCallback);
      ScollableChildframe.RelativeWidth = -1f;
      ScollableChildframe.RelativeHeight = -1f;
    }

    [XmlAttribute("border-width")]
    public int BorderWidth
    {
      get
      {
        return ScollableChildframe.BorderWidth;
      }
      set
      {
        ScollableChildframe.BorderWidth = value;
      }
    }

    [XmlAttribute("border-height")]
    public int BorderHeight
    {
      get
      {
        return ScollableChildframe.BorderHeight;
      }
      set
      {
        ScollableChildframe.BorderHeight = value;
      }
    }

    [XmlAttribute("layout-mode")]
    public Layout.LayoutMode layoutMode
    {
      get
      {
        return ScollableChildframe.layoutMode;
      }
      set
      {
        ScollableChildframe.layoutMode = value;
      }
    }

    public override void OnParentResize()
    {
      ScollableChildframe.Width = Width - 32;
      base.OnParentResize();
    }
  }
}
