using M3D.Graphics.Widgets2D;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace M3D.Graphics.Frames_and_Layouts
{
  public class HorizontalLayout : Layout
  {
    protected List<HorizontalLayout.ColumnInfo> info_list;
    public bool use_fixed_column_width;

    public HorizontalLayout()
      : this(0)
    {
    }

    public HorizontalLayout(int ID)
      : this(ID, (Element2D) null)
    {
    }

    public HorizontalLayout(int ID, Element2D parent)
      : base(ID, parent)
    {
      use_fixed_column_width = false;
      info_list = new List<HorizontalLayout.ColumnInfo>();
      border_width = 10;
      border_height = 10;
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      base.InitChildren(parent, host, MyButtonCallback);
      var index = 0;
      foreach (Element2D child in (IEnumerable<Element2D>)ChildList)
      {
        HorizontalLayout.ColumnInfo columnInfo;
        columnInfo.element = child;
        columnInfo.ispercent = false;
        columnInfo.prefered_size = child.Width;
        info_list.Insert(index, columnInfo);
        ++index;
      }
      RecalcChildSizes();
    }

    public override void OnParentResize()
    {
      base.OnParentResize();
    }

    public override void SetSize(int width, int height)
    {
      base.SetSize(width, height);
      RecalcChildSizes();
    }

    public override void RemoveChildElement(Element2D child)
    {
      for (var index = 0; index < info_list.Count; ++index)
      {
        if (info_list[index].element == child)
        {
          info_list.RemoveAt(index);
          base.RemoveChildElement(child);
          RecalcChildSizes();
          break;
        }
      }
    }

    public override void AddChildElement(Element2D child)
    {
      AddChildElement(child, info_list.Count);
    }

    public void AddChildElement(Element2D child, int column_index)
    {
      AddChildElement(child, column_index, 0, false);
    }

    public void AddChildElement(Element2D child, int column_index, int size)
    {
      AddChildElement(child, column_index, size, false);
    }

    public void AddChildElement(Element2D child, int column_index, int size, bool sizeisprecent)
    {
      if (column_index > info_list.Count)
      {
        column_index = info_list.Count;
      }
      else if (column_index < 0)
      {
        column_index = 0;
      }

      HorizontalLayout.ColumnInfo columnInfo;
      columnInfo.element = child;
      columnInfo.ispercent = sizeisprecent;
      columnInfo.prefered_size = size;
      info_list.Insert(column_index, columnInfo);
      base.AddChildElement(child);
      RecalcChildSizes();
    }

    [XmlAttribute("fixed-column-width")]
    public bool FixedColumnWidth
    {
      get
      {
        return use_fixed_column_width;
      }
      set
      {
        use_fixed_column_width = value;
      }
    }

    protected override void ResizeToFitChildren()
    {
      var borderHeight = border_height;
      var borderWidth = border_width;
      foreach (Element2D child in (IEnumerable<Element2D>)ChildList)
      {
        if (child.SelfIsVisible)
        {
          child.SetPosition(borderWidth, borderHeight);
          borderWidth += border_width + child.Width;
        }
      }
      Width = borderWidth;
      OnControlMsg((Element2D) this, ControlMsg.LAYOUT_RESIZED_BY_CHILDREN, 0.0f, 0.0f);
    }

    protected override void RecalcChildSizes()
    {
      if (info_list.Count <= 0)
      {
        return;
      }

      var num1 = Width - border_width;
      var num2 = num1 / info_list.Count;
      var num3 = 1f;
      if (!use_fixed_column_width)
      {
        var num4 = 0;
        foreach (HorizontalLayout.ColumnInfo info in info_list)
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
        num3 = (float) num1 / (float) num4;
      }
      var borderWidth = border_width;
      foreach (HorizontalLayout.ColumnInfo info in info_list)
      {
        var width = use_fixed_column_width || info.prefered_size == 0 ? num2 - border_width : (info.prefered_size > 0 ? (!info.ispercent ? (int) ((double) num3 * (double) info.prefered_size) : (int) ((double) num3 * (double) info.prefered_size * (double) num1)) - border_width : num1 - borderWidth - border_height);
        info.element.SetPosition(borderWidth, border_height);
        info.element.SetSize(width, Height - border_height * 2);
        borderWidth += width + border_width;
      }
    }

    protected struct ColumnInfo
    {
      public Element2D element;
      public int prefered_size;
      public bool ispercent;
    }
  }
}
