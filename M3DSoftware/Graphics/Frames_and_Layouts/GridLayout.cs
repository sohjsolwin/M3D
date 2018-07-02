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
      element_list = new List<Element2D>();
      border_width = 10;
      border_height = 10;
      column_width = 100;
      row_height = 100;
    }

    public override void OnParentResize()
    {
      RecalcChildSizes();
      base.OnParentResize();
    }

    public Element2D GetElementAt(int index)
    {
      if (index >= 0 && index < element_list.Count)
      {
        return element_list[index];
      }

      return (Element2D) null;
    }

    public override void Clear()
    {
      cur_page = 0;
      element_list.Clear();
      base.Clear();
    }

    private void RecalcChildSizes()
    {
      if (Count < 1)
      {
        return;
      }

      var numColumns1 = NumColumns;
      var numRows = NumRows;
      if (numColumns1 <= 0 || numRows <= 0 || (Width > 100000 || Width < 0))
      {
        return;
      }

      var num1 = (Width - numColumns1 * ColumnWidth) / (numColumns1 + 1);
      var num2 = (Height - numRows * RowHeight) / (numRows + 1);
      var num3 = CurPage * ElementsPerPage;
      var num4 = num3 + ElementsPerPage - 1;
      last_start = num3;
      if (num4 >= element_list.Count)
      {
        num4 = element_list.Count - 1;
      }

      for (var index = 0; index < num3; ++index)
      {
        if (element_list[index].Active)
        {
          element_list[index].Active = false;
        }

        if (element_list[index].Visible)
        {
          element_list[index].Visible = false;
        }
      }
      for (var index = num4 + 1; index < element_list.Count; ++index)
      {
        if (element_list[index].Active)
        {
          element_list[index].Active = false;
        }

        if (element_list[index].Visible)
        {
          element_list[index].Visible = false;
        }
      }
      var numColumns2 = NumColumns;
      var num5 = 0;
      var x = num1;
      var y = num2;
      for (var index = num3; index <= num4; ++index)
      {
        if (num5 >= numColumns2)
        {
          num5 = 0;
          x = num1;
          y += num2 + row_height;
        }
        element_list[index].SetPosition(x, y);
        element_list[index].SetSize(column_width, row_height);
        if (!element_list[index].Active)
        {
          element_list[index].Active = true;
        }

        if (!element_list[index].Visible)
        {
          element_list[index].Visible = true;
        }

        ++num5;
        x += num1 + column_width;
      }
    }

    public override void SetSize(int width, int height)
    {
      base.SetSize(width, height);
      if (ElementsPerPage <= 0)
      {
        return;
      }

      cur_page = last_start / ElementsPerPage;
    }

    public override void AddChildElement(Element2D child)
    {
      AddChildElement(child, element_list.Count);
    }

    public void AddChildElement(Element2D child, int index)
    {
      if (index > element_list.Count)
      {
        index = element_list.Count;
      }
      else if (index < 0)
      {
        index = 0;
      }

      element_list.Insert(index, child);
      base.AddChildElement(child);
    }

    public int ElementsPerPage
    {
      get
      {
        return NumColumns * NumRows;
      }
    }

    public int CurPage
    {
      get
      {
        return cur_page;
      }
      set
      {
        cur_page = value;
        if (cur_page < 0)
        {
          cur_page = 0;
        }
        else if (cur_page >= PageCount)
        {
          cur_page = PageCount - 1;
        }

        RecalcChildSizes();
      }
    }

    public int PageCount
    {
      get
      {
        if (element_list == null || element_list.Count == 0)
        {
          return 1;
        }

        return (int) Math.Ceiling((double)element_list.Count / (double)ElementsPerPage);
      }
    }

    public int NumColumns
    {
      get
      {
        return (Width - border_width) / (ColumnWidth + border_width);
      }
    }

    public int NumRows
    {
      get
      {
        return (Height - border_height) / (RowHeight + border_height);
      }
    }

    public int BorderWidth
    {
      get
      {
        return border_width;
      }
      set
      {
        border_width = value;
        if (border_width >= 0)
        {
          return;
        }

        border_width = 0;
      }
    }

    public int BorderHeight
    {
      get
      {
        return border_height;
      }
      set
      {
        border_height = value;
        if (border_height >= 0)
        {
          return;
        }

        border_height = 0;
      }
    }

    public int ColumnWidth
    {
      get
      {
        return column_width;
      }
      set
      {
        column_width = value;
        if (column_width >= 50)
        {
          return;
        }

        column_width = 50;
      }
    }

    public int RowHeight
    {
      get
      {
        return row_height;
      }
      set
      {
        row_height = value;
        if (row_height >= 50)
        {
          return;
        }

        row_height = 50;
      }
    }

    public int Count
    {
      get
      {
        return element_list.Count;
      }
    }
  }
}
