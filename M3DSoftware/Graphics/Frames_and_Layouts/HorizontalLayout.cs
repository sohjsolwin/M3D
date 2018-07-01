// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Frames_and_Layouts.HorizontalLayout
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      this.use_fixed_column_width = false;
      this.info_list = new List<HorizontalLayout.ColumnInfo>();
      this.border_width = 10;
      this.border_height = 10;
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      base.InitChildren(parent, host, MyButtonCallback);
      int index = 0;
      foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
      {
        HorizontalLayout.ColumnInfo columnInfo;
        columnInfo.element = child;
        columnInfo.ispercent = false;
        columnInfo.prefered_size = child.Width;
        this.info_list.Insert(index, columnInfo);
        ++index;
      }
      this.RecalcChildSizes();
    }

    public override void OnParentResize()
    {
      base.OnParentResize();
    }

    public override void SetSize(int width, int height)
    {
      base.SetSize(width, height);
      this.RecalcChildSizes();
    }

    public override void RemoveChildElement(Element2D child)
    {
      for (int index = 0; index < this.info_list.Count; ++index)
      {
        if (this.info_list[index].element == child)
        {
          this.info_list.RemoveAt(index);
          base.RemoveChildElement(child);
          this.RecalcChildSizes();
          break;
        }
      }
    }

    public override void AddChildElement(Element2D child)
    {
      this.AddChildElement(child, this.info_list.Count);
    }

    public void AddChildElement(Element2D child, int column_index)
    {
      this.AddChildElement(child, column_index, 0, false);
    }

    public void AddChildElement(Element2D child, int column_index, int size)
    {
      this.AddChildElement(child, column_index, size, false);
    }

    public void AddChildElement(Element2D child, int column_index, int size, bool sizeisprecent)
    {
      if (column_index > this.info_list.Count)
        column_index = this.info_list.Count;
      else if (column_index < 0)
        column_index = 0;
      HorizontalLayout.ColumnInfo columnInfo;
      columnInfo.element = child;
      columnInfo.ispercent = sizeisprecent;
      columnInfo.prefered_size = size;
      this.info_list.Insert(column_index, columnInfo);
      base.AddChildElement(child);
      this.RecalcChildSizes();
    }

    [XmlAttribute("fixed-column-width")]
    public bool FixedColumnWidth
    {
      get
      {
        return this.use_fixed_column_width;
      }
      set
      {
        this.use_fixed_column_width = value;
      }
    }

    protected override void ResizeToFitChildren()
    {
      int borderHeight = this.border_height;
      int borderWidth = this.border_width;
      foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
      {
        if (child.SelfIsVisible)
        {
          child.SetPosition(borderWidth, borderHeight);
          borderWidth += this.border_width + child.Width;
        }
      }
      this.Width = borderWidth;
      this.OnControlMsg((Element2D) this, ControlMsg.LAYOUT_RESIZED_BY_CHILDREN, 0.0f, 0.0f);
    }

    protected override void RecalcChildSizes()
    {
      if (this.info_list.Count <= 0)
        return;
      int num1 = this.Width - this.border_width;
      int num2 = num1 / this.info_list.Count;
      float num3 = 1f;
      if (!this.use_fixed_column_width)
      {
        int num4 = 0;
        foreach (HorizontalLayout.ColumnInfo info in this.info_list)
        {
          if (info.prefered_size == 0)
            num4 += num2;
          else if (info.prefered_size < 0)
            num4 += num1 - num4;
          else if (info.ispercent)
            num4 += num1 * info.prefered_size;
          else
            num4 += info.prefered_size;
        }
        num3 = (float) num1 / (float) num4;
      }
      int borderWidth = this.border_width;
      foreach (HorizontalLayout.ColumnInfo info in this.info_list)
      {
        int width = this.use_fixed_column_width || info.prefered_size == 0 ? num2 - this.border_width : (info.prefered_size > 0 ? (!info.ispercent ? (int) ((double) num3 * (double) info.prefered_size) : (int) ((double) num3 * (double) info.prefered_size * (double) num1)) - this.border_width : num1 - borderWidth - this.border_height);
        info.element.SetPosition(borderWidth, this.border_height);
        info.element.SetSize(width, this.Height - this.border_height * 2);
        borderWidth += width + this.border_width;
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
