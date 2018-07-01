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
      this.UpdatePaneSize();
    }

    [XmlAttribute("Pane-Width")]
    public int Pane_Width
    {
      get
      {
        return this.scrollframe_width;
      }
      set
      {
        this.scrollframe_width = value;
        if (this.ScollableChildframe == null)
          return;
        this.ScollableChildframe.RelativeWidth = -1f;
        this.UpdatePaneSize();
      }
    }

    [XmlAttribute("Pane-Height")]
    public int Pane_Height
    {
      get
      {
        return this.scrollframe_height;
      }
      set
      {
        this.scrollframe_height = value;
        if (this.ScollableChildframe == null)
          return;
        this.ScollableChildframe.RelativeHeight = -1f;
        this.UpdatePaneSize();
      }
    }

    public void UpdatePaneSize()
    {
      if (this.scrollframe_width < 1)
        this.ScollableChildframe.Width = this.Width - 32;
      else
        this.ScollableChildframe.Width = this.scrollframe_width;
      if (this.scrollframe_height < 1)
        this.ScollableChildframe.Height = this.Height - 32;
      else
        this.ScollableChildframe.Height = this.scrollframe_height;
      this.Refresh();
    }

    public void SetPaneSize(int width, int height)
    {
      this.scrollframe_width = width;
      this.scrollframe_height = height;
      if (this.ScollableChildframe == null)
        return;
      this.UpdatePaneSize();
    }
  }
}
