// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Frames_and_Layouts.VerticalLayout
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      : this(ID, (Element2D) null)
    {
    }

    public VerticalLayout(int ID, Element2D parent)
      : base(ID, parent)
    {
      this.use_fixed_row_height = false;
      this.info_list = new List<VerticalLayout.RowInfo>();
      this.border_width = 10;
      this.border_height = 10;
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      base.InitChildren(parent, host, MyButtonCallback);
      int index = 0;
      foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
      {
        VerticalLayout.RowInfo rowInfo;
        rowInfo.element = child;
        rowInfo.ispercent = false;
        rowInfo.prefered_size = child.Height;
        this.info_list.Insert(index, rowInfo);
        ++index;
      }
      this.Recalc();
    }

    public override void Refresh()
    {
      this.Recalc();
    }

    public override void OnParentResize()
    {
      base.OnParentResize();
      if (this.layoutMode == Layout.LayoutMode.ResizeLayoutToFitChildren)
        return;
      this.Recalc();
    }

    public override void SetSize(int width, int height)
    {
      base.SetSize(width, height);
      if (this.layoutMode == Layout.LayoutMode.ResizeLayoutToFitChildren)
        return;
      this.Recalc();
    }

    public override void AddChildElement(Element2D child)
    {
      this.AddChildElement(child, this.info_list.Count);
    }

    public void AddChildElement(Element2D child, int row_index)
    {
      this.AddChildElement(child, row_index, 0, false);
    }

    public void AddChildElement(Element2D child, int row_index, int size)
    {
      this.AddChildElement(child, row_index, size, false);
    }

    public void AddChildElement(Element2D child, int row_index, int size, bool sizeisprecent)
    {
      if (row_index > this.info_list.Count)
        row_index = this.info_list.Count;
      else if (row_index < 0)
        row_index = 0;
      VerticalLayout.RowInfo rowInfo;
      rowInfo.element = child;
      rowInfo.ispercent = sizeisprecent;
      rowInfo.prefered_size = size;
      this.info_list.Insert(row_index, rowInfo);
      base.AddChildElement(child);
      this.Recalc();
    }

    [XmlAttribute("fixed-row-height")]
    public bool FixedRowHeight
    {
      get
      {
        return this.use_fixed_row_height;
      }
      set
      {
        this.use_fixed_row_height = value;
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
          borderHeight += this.border_height + child.Height;
        }
      }
      this.Height = borderHeight;
      this.OnControlMsg((Element2D) this, ControlMsg.LAYOUT_RESIZED_BY_CHILDREN, 0.0f, 0.0f);
    }

    protected override void RecalcChildSizes()
    {
      if (this.info_list.Count <= 0)
        return;
      int num1 = this.Height - this.border_height;
      int num2 = num1 / this.info_list.Count;
      float num3 = 1f;
      if (!this.use_fixed_row_height)
      {
        int num4 = 0;
        foreach (VerticalLayout.RowInfo info in this.info_list)
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
      int borderHeight = this.border_height;
      foreach (VerticalLayout.RowInfo info in this.info_list)
      {
        int height = this.use_fixed_row_height || info.prefered_size == 0 ? num2 - this.border_height : (info.prefered_size > 0 ? (!info.ispercent ? (int) ((double) num3 * (double) info.prefered_size) : (int) ((double) num3 * (double) info.prefered_size * (double) num1)) - this.border_height : num1 - borderHeight - this.border_height);
        info.element.SetPosition(this.border_width, borderHeight);
        info.element.SetSize(this.Width - this.border_width * 2, height);
        borderHeight += height + this.border_height;
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
