// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Widgets2D.HorizontalLayoutScrollList
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.Frames_and_Layouts;
using System;
using System.Xml.Serialization;

namespace M3D.Graphics.Widgets2D
{
  public class HorizontalLayoutScrollList : HorizontalLayout
  {
    [XmlAttribute("visible-count")]
    public int visibleCount = 5;
    [XmlAttribute("start-index")]
    public int startIndex;

    public override void OnParentResize()
    {
      base.OnParentResize();
      this.RecalcChildSizes();
    }

    protected override void RecalcChildSizes()
    {
      if (this.info_list.Count <= 0)
        return;
      int num1 = this.Width - this.border_width;
      int num2 = num1 / this.visibleCount;
      float num3 = 1f;
      int num4 = 0;
      if (!this.use_fixed_column_width)
      {
        int num5 = 0;
        foreach (HorizontalLayout.ColumnInfo info in this.info_list)
        {
          if (num4 >= this.startIndex && num4 < this.startIndex + this.visibleCount)
          {
            if (info.prefered_size == 0)
              num5 += num2;
            else if (info.prefered_size < 0)
              num5 += num1 - num5;
            else if (info.ispercent)
              num5 += num1 * info.prefered_size;
            else
              num5 += info.prefered_size;
          }
          ++num4;
        }
        num3 = (float) num1 / (float) num5;
      }
      int borderWidth = this.border_width;
      int num6 = 0;
      foreach (HorizontalLayout.ColumnInfo info in this.info_list)
      {
        if (num6 >= this.startIndex && num6 < this.startIndex + this.visibleCount)
        {
          int width = this.use_fixed_column_width || info.prefered_size == 0 ? num2 - this.border_width : (info.prefered_size > 0 ? (!info.ispercent ? (int) ((double) num3 * (double) info.prefered_size) : (int) ((double) num3 * (double) info.prefered_size * (double) num1)) - this.border_width : num1 - borderWidth - this.border_height);
          info.element.SetPosition(borderWidth, this.border_height);
          info.element.SetSize(width, this.Height - this.border_height * 2);
          borderWidth += width + this.border_width;
          info.element.Visible = true;
          info.element.Enabled = true;
        }
        else
        {
          info.element.Visible = false;
          info.element.Enabled = false;
        }
        ++num6;
      }
    }

    public override void AddChildElement(Element2D child)
    {
      base.AddChildElement(child);
      ++this.StartIndex;
      this.RecalcChildSizes();
    }

    public override void RemoveChildElement(Element2D child)
    {
      base.RemoveChildElement(child);
      int index = this.info_list.FindIndex((Predicate<HorizontalLayout.ColumnInfo>) (ci => child == ci.element));
      if (index < 0)
        return;
      this.info_list.RemoveAt(index);
      if (index == this.info_list.Count)
        --this.StartIndex;
      else if (index == 0)
        ++this.StartIndex;
      else
        this.RecalcChildSizes();
    }

    public void GotoChild(int ID)
    {
      for (int index = 0; index < this.info_list.Count; ++index)
      {
        if (this.info_list[index].element.ID == ID)
        {
          if (index >= this.StartIndex && index < this.StartIndex + this.visibleCount)
            break;
          this.StartIndex = index;
          break;
        }
      }
    }

    public int StartIndex
    {
      get
      {
        return this.startIndex;
      }
      set
      {
        this.startIndex = value >= this.info_list.Count - this.visibleCount ? this.info_list.Count - this.visibleCount : value;
        if (this.startIndex < 0)
          this.startIndex = 0;
        this.RecalcChildSizes();
      }
    }

    public bool OnLastElement
    {
      get
      {
        return this.startIndex == this.info_list.Count - this.visibleCount;
      }
    }

    public bool OnFirstElement
    {
      get
      {
        return this.StartIndex == 0;
      }
    }
  }
}
