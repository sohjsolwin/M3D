using M3D.Graphics.Widgets2D;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace M3D.Graphics.Frames_and_Layouts
{
  public class VerticalLayout : Layout
  {
    private List<VerticalLayout.RowInfo> info_list;
    private bool use_fixed_row_height;

    public VerticalLayout()
      : this(0)
    {
    }

    public VerticalLayout(int ID)
      : this(ID, null)
    {
    }

    public VerticalLayout(int ID, Element2D parent)
      : base(ID, parent)
    {
      use_fixed_row_height = false;
      info_list = new List<VerticalLayout.RowInfo>();
      border_width = 10;
      border_height = 10;
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      base.InitChildren(parent, host, MyButtonCallback);
      var index = 0;
      foreach (Element2D child in ChildList)
      {
        VerticalLayout.RowInfo rowInfo;
        rowInfo.element = child;
        rowInfo.ispercent = false;
        rowInfo.prefered_size = child.Height;
        info_list.Insert(index, rowInfo);
        ++index;
      }
      Recalc();
    }

    public override void Refresh()
    {
      Recalc();
    }

    public override void OnParentResize()
    {
      base.OnParentResize();
      if (layoutMode == Layout.LayoutMode.ResizeLayoutToFitChildren)
      {
        return;
      }

      Recalc();
    }

    public override void SetSize(int width, int height)
    {
      base.SetSize(width, height);
      if (layoutMode == Layout.LayoutMode.ResizeLayoutToFitChildren)
      {
        return;
      }

      Recalc();
    }

    public override void AddChildElement(Element2D child)
    {
      AddChildElement(child, info_list.Count);
    }

    public void AddChildElement(Element2D child, int row_index)
    {
      AddChildElement(child, row_index, 0, false);
    }

    public void AddChildElement(Element2D child, int row_index, int size)
    {
      AddChildElement(child, row_index, size, false);
    }

    public void AddChildElement(Element2D child, int row_index, int size, bool sizeisprecent)
    {
      if (row_index > info_list.Count)
      {
        row_index = info_list.Count;
      }
      else if (row_index < 0)
      {
        row_index = 0;
      }

      VerticalLayout.RowInfo rowInfo;
      rowInfo.element = child;
      rowInfo.ispercent = sizeisprecent;
      rowInfo.prefered_size = size;
      info_list.Insert(row_index, rowInfo);
      base.AddChildElement(child);
      Recalc();
    }

    [XmlAttribute("fixed-row-height")]
    public bool FixedRowHeight
    {
      get
      {
        return use_fixed_row_height;
      }
      set
      {
        use_fixed_row_height = value;
      }
    }

    protected override void ResizeToFitChildren()
    {
      var borderHeight = border_height;
      var borderWidth = border_width;
      foreach (Element2D child in ChildList)
      {
        if (child.SelfIsVisible)
        {
          child.SetPosition(borderWidth, borderHeight);
          borderHeight += border_height + child.Height;
        }
      }
      Height = borderHeight;
      OnControlMsg(this, ControlMsg.LAYOUT_RESIZED_BY_CHILDREN, 0.0f, 0.0f);
    }

    protected override void RecalcChildSizes()
    {
      if (info_list.Count <= 0)
      {
        return;
      }

      var num1 = Height - border_height;
      var num2 = num1 / info_list.Count;
      var num3 = 1f;
      if (!use_fixed_row_height)
      {
        var num4 = 0;
        foreach (VerticalLayout.RowInfo info in info_list)
        {
          if (info.prefered_size == 0)
          {
            num4 += num2;
          }
          else if (info.prefered_size < 0)
          {
            num4 += num1 - num4;
          }
          else if (info.ispercent)
          {
            num4 += num1 * info.prefered_size;
          }
          else
          {
            num4 += info.prefered_size;
          }
        }
        num3 = num1 / (float)num4;
      }
      var borderHeight = border_height;
      foreach (VerticalLayout.RowInfo info in info_list)
      {
        var height = use_fixed_row_height || info.prefered_size == 0 ? num2 - border_height : (info.prefered_size > 0 ? (!info.ispercent ? (int)(num3 * (double)info.prefered_size) : (int)(num3 * (double)info.prefered_size * num1)) - border_height : num1 - borderHeight - border_height);
        info.element.SetPosition(border_width, borderHeight);
        info.element.SetSize(Width - border_width * 2, height);
        borderHeight += height + border_height;
      }
    }

    private struct RowInfo
    {
      public Element2D element;
      public int prefered_size;
      public bool ispercent;
    }
  }
}
