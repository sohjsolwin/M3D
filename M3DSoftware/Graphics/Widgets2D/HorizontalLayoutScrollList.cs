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
      RecalcChildSizes();
    }

    protected override void RecalcChildSizes()
    {
      if (info_list.Count <= 0)
      {
        return;
      }

      var num1 = Width - border_width;
      var num2 = num1 / visibleCount;
      var num3 = 1f;
      var num4 = 0;
      if (!use_fixed_column_width)
      {
        var num5 = 0;
        foreach (HorizontalLayout.ColumnInfo info in info_list)
        {
          if (num4 >= startIndex && num4 < startIndex + visibleCount)
          {
            if (info.prefered_size == 0)
            {
              num5 += num2;
            }
            else if (info.prefered_size < 0)
            {
              num5 += num1 - num5;
            }
            else if (info.ispercent)
            {
              num5 += num1 * info.prefered_size;
            }
            else
            {
              num5 += info.prefered_size;
            }
          }
          ++num4;
        }
        num3 = (float) num1 / (float) num5;
      }
      var borderWidth = border_width;
      var num6 = 0;
      foreach (HorizontalLayout.ColumnInfo info in info_list)
      {
        if (num6 >= startIndex && num6 < startIndex + visibleCount)
        {
          var width = use_fixed_column_width || info.prefered_size == 0 ? num2 - border_width : (info.prefered_size > 0 ? (!info.ispercent ? (int) ((double) num3 * (double) info.prefered_size) : (int) ((double) num3 * (double) info.prefered_size * (double) num1)) - border_width : num1 - borderWidth - border_height);
          info.element.SetPosition(borderWidth, border_height);
          info.element.SetSize(width, Height - border_height * 2);
          borderWidth += width + border_width;
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
      ++StartIndex;
      RecalcChildSizes();
    }

    public override void RemoveChildElement(Element2D child)
    {
      base.RemoveChildElement(child);
      var index = info_list.FindIndex((Predicate<HorizontalLayout.ColumnInfo>) (ci => child == ci.element));
      if (index < 0)
      {
        return;
      }

      info_list.RemoveAt(index);
      if (index == info_list.Count)
      {
        --StartIndex;
      }
      else if (index == 0)
      {
        ++StartIndex;
      }
      else
      {
        RecalcChildSizes();
      }
    }

    public void GotoChild(int ID)
    {
      for (var index = 0; index < info_list.Count; ++index)
      {
        if (info_list[index].element.ID == ID)
        {
          if (index >= StartIndex && index < StartIndex + visibleCount)
          {
            break;
          }

          StartIndex = index;
          break;
        }
      }
    }

    public int StartIndex
    {
      get
      {
        return startIndex;
      }
      set
      {
        startIndex = value >= info_list.Count - visibleCount ? info_list.Count - visibleCount : value;
        if (startIndex < 0)
        {
          startIndex = 0;
        }

        RecalcChildSizes();
      }
    }

    public bool OnLastElement
    {
      get
      {
        return startIndex == info_list.Count - visibleCount;
      }
    }

    public bool OnFirstElement
    {
      get
      {
        return StartIndex == 0;
      }
    }
  }
}
