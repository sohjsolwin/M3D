// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Frames_and_Layouts.ScrollFrame
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.Widgets2D;
using System.Xml.Serialization;

namespace M3D.Graphics.Frames_and_Layouts
{
  public class ScrollFrame : GenericScrollFrame<Frame>
  {
    private int scrollframe_width = -1;
    private int scrollframe_height = -1;

    public ScrollFrame()
      : this(0, (Element2D) null)
    {
    }

    public ScrollFrame(int ID)
      : this(ID, (Element2D) null)
    {
    }

    public ScrollFrame(int ID, Element2D parent)
      : base(ID, parent)
    {
    }

    public override void OnParentResize()
    {
      base.OnParentResize();
      UpdatePaneSize();
    }

    [XmlAttribute("Pane-Width")]
    public int Pane_Width
    {
      get
      {
        return scrollframe_width;
      }
      set
      {
        scrollframe_width = value;
        if (ScollableChildframe == null)
        {
          return;
        }

        ScollableChildframe.RelativeWidth = -1f;
        UpdatePaneSize();
      }
    }

    [XmlAttribute("Pane-Height")]
    public int Pane_Height
    {
      get
      {
        return scrollframe_height;
      }
      set
      {
        scrollframe_height = value;
        if (ScollableChildframe == null)
        {
          return;
        }

        ScollableChildframe.RelativeHeight = -1f;
        UpdatePaneSize();
      }
    }

    public void UpdatePaneSize()
    {
      if (scrollframe_width < 1)
      {
        ScollableChildframe.Width = Width - 32;
      }
      else
      {
        ScollableChildframe.Width = scrollframe_width;
      }

      if (scrollframe_height < 1)
      {
        ScollableChildframe.Height = Height - 32;
      }
      else
      {
        ScollableChildframe.Height = scrollframe_height;
      }

      Refresh();
    }

    public void SetPaneSize(int width, int height)
    {
      scrollframe_width = width;
      scrollframe_height = height;
      if (ScollableChildframe == null)
      {
        return;
      }

      UpdatePaneSize();
    }
  }
}
