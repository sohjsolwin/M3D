// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Frames_and_Layouts.GenericScrollFrame`1
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.Widgets2D;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace M3D.Graphics.Frames_and_Layouts
{
  public class GenericScrollFrame<T> : Frame where T : Frame, new()
  {
    private GenericScrollFrame<T>.ScrollBarState _showScrollbar = GenericScrollFrame<T>.ScrollBarState.Auto;
    private const int BORDER_SIZE = 2;
    protected int scrollbar_width;
    private VerticalSliderWidget verticalSlider;
    private HorizontalSliderWidget horizonalSlider;
    protected T ScollableChildframe;
    private Frame m_frameDrawableRegion;

    public GenericScrollFrame()
      : this(0, (Element2D) null)
    {
    }

    public GenericScrollFrame(int ID)
      : this(ID, (Element2D) null)
    {
    }

    public GenericScrollFrame(int ID, Element2D parent)
      : base(ID, parent)
    {
      this.verticalSlider = new VerticalSliderWidget(0, (Element2D) this);
      this.horizonalSlider = new HorizontalSliderWidget(1, (Element2D) this);
      this.m_frameDrawableRegion = new Frame();
      this.m_frameDrawableRegion.Clipping = true;
      this.m_frameDrawableRegion.SetPosition(2, 2);
      this.ScollableChildframe = Activator.CreateInstance<T>();
      this.ScollableChildframe.ID = 2;
      this.ScollableChildframe.Parent = (Element2D) this;
      this.ScollableChildframe.always_contains_point = true;
      this.m_frameDrawableRegion.AddChildElement((Element2D) this.ScollableChildframe);
    }

    public override ElementType GetElementType()
    {
      return ElementType.ScrollFrame;
    }

    public override void OnControlMsg(Element2D the_control, ControlMsg msg, float xparam, float yparam)
    {
      if (the_control.ID == 0)
      {
        if (msg != ControlMsg.SCROLL_MOVE)
          return;
        if (this.verticalSlider.Visible)
        {
          int scrollbarWidth = this.scrollbar_width;
        }
        this.ScollableChildframe.Y = -(int) xparam;
      }
      else if (the_control.ID == 1)
      {
        if (msg != ControlMsg.SCROLL_MOVE)
          return;
        if (this.horizonalSlider.Visible)
        {
          int scrollbarWidth = this.scrollbar_width;
        }
        this.ScollableChildframe.X = -(int) xparam;
      }
      else
        base.OnControlMsg(the_control, msg, xparam, yparam);
    }

    public override bool OnMouseCommand(MouseEvent mouseevent)
    {
      if (mouseevent.type != MouseEventType.MouseWheel || !this.verticalSlider.Visible)
        return base.OnMouseCommand(mouseevent);
      this.verticalSlider.MoveSlider((float) -((double) mouseevent.delta / 120.0) * this.verticalSlider.PushButtonStep);
      return true;
    }

    public override void OnRender(GUIHost host)
    {
      base.OnRender(host);
    }

    public void Init(GUIHost host)
    {
      this.ScollableChildframe.WrapOnNegative = false;
      this.verticalSlider.InitTrack(host, "guicontrols", 1008f, 73f, 1016f, 95f, 2, 8);
      this.verticalSlider.InitButton(host, "guicontrols", 1000f, 0.0f, 1023f, 23f, 1000f, 24f, 1023f, 47f, 1000f, 48f, 1023f, 71f, 4, 4, 24);
      this.verticalSlider.InitMinus(host, "guicontrols", 928f, 48f, 951f, 71f, 952f, 48f, 975f, 71f, 976f, 48f, 999f, 71f);
      this.verticalSlider.InitPlus(host, "guicontrols", 928f, 72f, 951f, 95f, 952f, 72f, 975f, 95f, 976f, 72f, 999f, 95f);
      this.verticalSlider.SetButtonSize(24f);
      this.verticalSlider.ShowPushButtons = true;
      this.horizonalSlider.InitTrack(host, "guicontrols", 809f, 80f, 831f, 88f, 2, 8);
      this.horizonalSlider.InitButton(host, "guicontrols", 1000f, 0.0f, 1023f, 23f, 1000f, 24f, 1023f, 47f, 1000f, 48f, 1023f, 71f, 4, 4, 24);
      this.horizonalSlider.InitMinus(host, "guicontrols", 928f, 0.0f, 951f, 23f, 952f, 0.0f, 975f, 23f, 976f, 0.0f, 999f, 23f);
      this.horizonalSlider.InitPlus(host, "guicontrols", 928f, 24f, 951f, 47f, 952f, 24f, 975f, 47f, 976f, 24f, 999f, 47f);
      this.horizonalSlider.SetButtonSize(24f);
      this.horizonalSlider.ShowPushButtons = true;
      base.AddChildElement((Element2D) this.verticalSlider);
      base.AddChildElement((Element2D) this.horizonalSlider);
      base.AddChildElement((Element2D) this.m_frameDrawableRegion);
      this.scrollbar_width = 24;
      this.Refresh();
    }

    public override void AddChildElement(Element2D child)
    {
      this.ScollableChildframe.AddChildElement(child);
    }

    public override void RemoveAllChildElements()
    {
      this.ScollableChildframe.RemoveAllChildElements();
    }

    public override void RemoveChildElement(Element2D child)
    {
      this.ScollableChildframe.RemoveChildElement(child);
    }

    public override void RemoveChildElementAt(int index)
    {
      this.ScollableChildframe.RemoveChildElementAt(index);
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      this.Parent = parent;
      lock (this.ChildList)
      {
        foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
          this.ScollableChildframe.AddChildElement(child);
        this.ChildList.Clear();
        this.ScollableChildframe.InitChildren((Element2D) this, host, MyButtonCallback);
      }
      this.Init(host);
      this.ScollableChildframe.Refresh();
    }

    public override void Refresh()
    {
      bool flag1;
      bool flag2;
      if (this.ShowScrollbar == GenericScrollFrame<T>.ScrollBarState.Off)
      {
        flag1 = false;
        flag2 = false;
      }
      else if (this.ShowScrollbar == GenericScrollFrame<T>.ScrollBarState.On)
      {
        flag1 = true;
        flag2 = true;
      }
      else
      {
        flag1 = this.ScollableChildframe.Width > this.Width;
        flag2 = this.ScollableChildframe.Height > this.Height;
      }
      this.m_frameDrawableRegion.Width = (flag2 ? this.Width - this.scrollbar_width : this.Width) - 4;
      this.m_frameDrawableRegion.Height = (flag1 ? this.Height - this.scrollbar_width : this.Height) - 4;
      if (flag2)
      {
        int height = flag1 ? this.Height - this.scrollbar_width : this.Height;
        this.verticalSlider.SetPosition(-this.scrollbar_width, 0);
        this.verticalSlider.SetSize(this.scrollbar_width, height);
        int num = this.ScollableChildframe.Height - this.m_frameDrawableRegion.Height;
        if (num < 0)
          num = 0;
        this.verticalSlider.Visible = true;
        this.verticalSlider.Enabled = true;
        this.verticalSlider.SetRange(0.0f, (float) (this.ScollableChildframe.Height - this.Height));
        this.verticalSlider.PushButtonStep = (float) (int) Math.Ceiling((double) num / 10.0);
      }
      else
      {
        if (this.verticalSlider.Visible)
          this.verticalSlider.SetTrackPosition(0.0f);
        this.verticalSlider.Visible = false;
        this.verticalSlider.Enabled = false;
      }
      if (flag1)
      {
        int width = flag2 ? this.Width - this.scrollbar_width : this.Width;
        this.horizonalSlider.SetPosition(0, -this.scrollbar_width);
        this.horizonalSlider.SetSize(width, this.scrollbar_width);
        int num = this.ScollableChildframe.Width - this.m_frameDrawableRegion.Width;
        if (num < 0)
          num = 0;
        this.horizonalSlider.Visible = true;
        this.horizonalSlider.Enabled = true;
        this.horizonalSlider.SetRange(0.0f, (float) (this.ScollableChildframe.Width - this.Width));
        this.horizonalSlider.PushButtonStep = 1f;
        this.horizonalSlider.PushButtonStep = (float) (int) Math.Ceiling((double) num / 10.0);
      }
      else
      {
        if (this.horizonalSlider.Visible)
          this.horizonalSlider.SetTrackPosition(0.0f);
        this.horizonalSlider.Visible = false;
        this.horizonalSlider.Enabled = false;
      }
    }

    public override void SetPosition(int x, int y)
    {
      base.SetPosition(x, y);
      this.Refresh();
    }

    public override void SetSize(int width, int height)
    {
      base.SetSize(width, height);
      this.Refresh();
    }

    public void VerticalMoveSlider(float fAmount)
    {
      this.verticalSlider.MoveSlider(fAmount);
    }

    public void HorizonalMoveSlider(float fAmount)
    {
      this.horizonalSlider.MoveSlider(fAmount);
    }

    [XmlIgnore]
    public Element2D MovableFrame
    {
      get
      {
        return (Element2D) this.ScollableChildframe;
      }
    }

    [XmlAttribute("ShowScrollbar")]
    public GenericScrollFrame<T>.ScrollBarState ShowScrollbar
    {
      set
      {
        this._showScrollbar = value;
        this.Refresh();
      }
      get
      {
        return this._showScrollbar;
      }
    }

    private enum ScrollFrameControlID
    {
      vertical_scrollbar,
      horizontal_scrollbar,
      child_frame,
    }

    public enum ScrollBarState
    {
      On,
      Off,
      Auto,
    }
  }
}
