// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Frames_and_Layouts.GridLayout
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.Widgets2D;
using System;
using System.Collections.Generic;

namespace M3D.Graphics.Frames_and_Layouts
{
  public class GridLayout : Frame
  {
    private int border_width;
    private int border_height;
    private int column_width;
    private int row_height;
    private int cur_page;
    private int last_start;
    private List<Element2D> element_list;

    public GridLayout()
      : this(0)
    {
    }

    public GridLayout(int ID)
      : this(ID, (Element2D) null)
    {
    }

    public GridLayout(int ID, Element2D parent)
      : base(ID, parent)
    {
      this.element_list = new List<Element2D>();
      this.border_width = 10;
      this.border_height = 10;
      this.column_width = 100;
      this.row_height = 100;
    }

    public override void OnParentResize()
    {
      this.RecalcChildSizes();
      base.OnParentResize();
    }

    public Element2D GetElementAt(int index)
    {
      if (index >= 0 && index < this.element_list.Count)
        return this.element_list[index];
      return (Element2D) null;
    }

    public override void Clear()
    {
      this.cur_page = 0;
      this.element_list.Clear();
      base.Clear();
    }

    private void RecalcChildSizes()
    {
      if (this.Count < 1)
        return;
      int numColumns1 = this.NumColumns;
      int numRows = this.NumRows;
      if (numColumns1 <= 0 || numRows <= 0 || (this.Width > 100000 || this.Width < 0))
        return;
      int num1 = (this.Width - numColumns1 * this.ColumnWidth) / (numColumns1 + 1);
      int num2 = (this.Height - numRows * this.RowHeight) / (numRows + 1);
      int num3 = this.CurPage * this.ElementsPerPage;
      int num4 = num3 + this.ElementsPerPage - 1;
      this.last_start = num3;
      if (num4 >= this.element_list.Count)
        num4 = this.element_list.Count - 1;
      for (int index = 0; index < num3; ++index)
      {
        if (this.element_list[index].Active)
          this.element_list[index].Active = false;
        if (this.element_list[index].Visible)
          this.element_list[index].Visible = false;
      }
      for (int index = num4 + 1; index < this.element_list.Count; ++index)
      {
        if (this.element_list[index].Active)
          this.element_list[index].Active = false;
        if (this.element_list[index].Visible)
          this.element_list[index].Visible = false;
      }
      int numColumns2 = this.NumColumns;
      int num5 = 0;
      int x = num1;
      int y = num2;
      for (int index = num3; index <= num4; ++index)
      {
        if (num5 >= numColumns2)
        {
          num5 = 0;
          x = num1;
          y += num2 + this.row_height;
        }
        this.element_list[index].SetPosition(x, y);
        this.element_list[index].SetSize(this.column_width, this.row_height);
        if (!this.element_list[index].Active)
          this.element_list[index].Active = true;
        if (!this.element_list[index].Visible)
          this.element_list[index].Visible = true;
        ++num5;
        x += num1 + this.column_width;
      }
    }

    public override void SetSize(int width, int height)
    {
      base.SetSize(width, height);
      if (this.ElementsPerPage <= 0)
        return;
      this.cur_page = this.last_start / this.ElementsPerPage;
    }

    public override void AddChildElement(Element2D child)
    {
      this.AddChildElement(child, this.element_list.Count);
    }

    public void AddChildElement(Element2D child, int index)
    {
      if (index > this.element_list.Count)
        index = this.element_list.Count;
      else if (index < 0)
        index = 0;
      this.element_list.Insert(index, child);
      base.AddChildElement(child);
    }

    public int ElementsPerPage
    {
      get
      {
        return this.NumColumns * this.NumRows;
      }
    }

    public int CurPage
    {
      get
      {
        return this.cur_page;
      }
      set
      {
        this.cur_page = value;
        if (this.cur_page < 0)
          this.cur_page = 0;
        else if (this.cur_page >= this.PageCount)
          this.cur_page = this.PageCount - 1;
        this.RecalcChildSizes();
      }
    }

    public int PageCount
    {
      get
      {
        if (this.element_list == null || this.element_list.Count == 0)
          return 1;
        return (int) Math.Ceiling((double) this.element_list.Count / (double) this.ElementsPerPage);
      }
    }

    public int NumColumns
    {
      get
      {
        return (this.Width - this.border_width) / (this.ColumnWidth + this.border_width);
      }
    }

    public int NumRows
    {
      get
      {
        return (this.Height - this.border_height) / (this.RowHeight + this.border_height);
      }
    }

    public int BorderWidth
    {
      get
      {
        return this.border_width;
      }
      set
      {
        this.border_width = value;
        if (this.border_width >= 0)
          return;
        this.border_width = 0;
      }
    }

    public int BorderHeight
    {
      get
      {
        return this.border_height;
      }
      set
      {
        this.border_height = value;
        if (this.border_height >= 0)
          return;
        this.border_height = 0;
      }
    }

    public int ColumnWidth
    {
      get
      {
        return this.column_width;
      }
      set
      {
        this.column_width = value;
        if (this.column_width >= 50)
          return;
        this.column_width = 50;
      }
    }

    public int RowHeight
    {
      get
      {
        return this.row_height;
      }
      set
      {
        this.row_height = value;
        if (this.row_height >= 50)
          return;
        this.row_height = 50;
      }
    }

    public int Count
    {
      get
      {
        return this.element_list.Count;
      }
    }
  }
}
