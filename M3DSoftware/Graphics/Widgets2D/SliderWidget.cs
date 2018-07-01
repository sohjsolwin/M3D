// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Widgets2D.SliderWidget
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using System;

namespace M3D.Graphics.Widgets2D
{
  public class SliderWidget : Element2D
  {
    private int rounding_place = 2;
    protected ImageWidget m_trackSubControl;
    protected ButtonWidget m_buttonSubControl;
    protected ButtonWidget m_pushButtonPlus;
    protected ButtonWidget m_pushButtonMinus;
    protected float m_track_size;
    protected float m_track_position;
    protected float m_button_size;
    protected float m_button_size_percent;
    protected float m_range_start;
    protected float m_range;
    protected float m_button_range;
    private bool m_bShowPushButtons;
    private float m_pushbuttonstep;

    public SliderWidget()
      : this(0)
    {
    }

    public SliderWidget(int ID)
      : this(ID, (Element2D) null)
    {
    }

    public SliderWidget(int ID, Element2D parent)
      : base(ID, parent)
    {
      this.m_trackSubControl = new ImageWidget(0, (Element2D) this);
      this.m_buttonSubControl = new ButtonWidget(0, (Element2D) this);
      this.m_pushButtonPlus = new ButtonWidget(1, (Element2D) this);
      this.m_pushButtonMinus = new ButtonWidget(2, (Element2D) this);
      this.m_buttonSubControl.DontMove = true;
      this.m_pushButtonPlus.DontMove = true;
      this.m_pushButtonMinus.DontMove = true;
      this.m_trackSubControl.IgnoreMouse = true;
      this.ChildList = this.ChildList + (Element2D) this.m_trackSubControl;
      this.ChildList = this.ChildList + (Element2D) this.m_buttonSubControl;
      this.ChildList = this.ChildList + (Element2D) this.m_pushButtonPlus;
      this.ChildList = this.ChildList + (Element2D) this.m_pushButtonMinus;
      this.m_track_size = 0.0f;
      this.m_track_position = 0.0f;
      this.m_button_size = 0.0f;
      this.m_button_size_percent = 0.0f;
      this.m_range_start = 0.0f;
      this.m_range = 1f;
      this.m_button_range = 0.0f;
    }

    public override ElementType GetElementType()
    {
      return ElementType.SliderWidget;
    }

    public override void SetSize(int width, int height)
    {
      base.SetSize(width, height);
      this.OnSetTrackSize((float) width, (float) height);
    }

    public override void SetPosition(int x, int y)
    {
      base.SetPosition(x, y);
    }

    public override void SetEnabled(bool bEnabled)
    {
      base.SetEnabled(bEnabled);
      this.m_buttonSubControl.Enabled = bEnabled;
      this.m_pushButtonPlus.Enabled = bEnabled;
      this.m_pushButtonMinus.Enabled = bEnabled;
      this.m_buttonSubControl.Visible = bEnabled;
    }

    public override void OnControlMsg(Element2D the_control, ControlMsg msg, float xparam, float yparam)
    {
      if (msg == ControlMsg.MSG_HIT)
      {
        if (the_control.ID == 2)
          this.SetTrackPosition(this.m_track_position - this.m_pushbuttonstep);
        else if (the_control.ID == 1)
          this.SetTrackPosition(this.m_track_position + this.m_pushbuttonstep);
      }
      if (the_control.GetElementType() == ElementType.ButtonWidget && msg == ControlMsg.MSG_MOVE && the_control.ID == 0)
      {
        float fPosition = xparam / yparam * this.m_range + this.m_range_start;
        if (this.rounding_place >= 0)
          fPosition = (float) Math.Round((double) fPosition, this.rounding_place, MidpointRounding.AwayFromZero);
        this.SetTrackPosition(fPosition);
      }
      if (the_control != null)
        return;
      this.SetTrackPosition(xparam / yparam * this.m_range + this.m_range_start);
    }

    public void InitPlus(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, float over_u0, float over_v0, float over_u1, float over_v1, float down_u0, float down_v0, float down_u1, float down_v1)
    {
      this.m_pushButtonPlus.Init(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, over_u0, over_v0, over_u1, over_v1, down_u0, down_v0, down_u1, down_v1);
    }

    public void InitMinus(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, float over_u0, float over_v0, float over_u1, float over_v1, float down_u0, float down_v0, float down_u1, float down_v1)
    {
      this.m_pushButtonMinus.Init(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, over_u0, over_v0, over_u1, over_v1, down_u0, down_v0, down_u1, down_v1);
    }

    public virtual void InitButton(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, float over_u0, float over_v0, float over_u1, float over_v1, float down_u0, float down_v0, float down_u1, float down_v1, int topborder, int bottomborder, int minsize)
    {
      this.m_buttonSubControl.Init(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, over_u0, over_v0, over_u1, over_v1, down_u0, down_v0, down_u1, down_v1);
    }

    public void InitTrack(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, int growableborder, int min, bool isvertical)
    {
      this.InitTrack(host, texture, normal_u0, normal_v0, normal_u1, normal_v1);
      if (isvertical)
        this.m_trackSubControl.SetGrowableHeight(growableborder, growableborder, min);
      else
        this.m_trackSubControl.SetGrowableWidth(growableborder, growableborder, min);
    }

    public void InitTrack(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1)
    {
      this.m_trackSubControl.Init(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, normal_u0, normal_v0, normal_u1, normal_v1, normal_u0, normal_v0, normal_u1, normal_v1);
    }

    public void SetButtonSize(float size)
    {
      this.m_button_size = size;
      this.m_button_size_percent = -1f;
    }

    public void SetButtonSizeByPercent(float size)
    {
      this.m_button_size_percent = size;
      this.m_button_size = -1f;
    }

    public void SetRange(float start, float end)
    {
      this.SetRange(start, end, start);
    }

    public void SetRange(float start, float end, float position)
    {
      if ((double) this.m_range_start == (double) start && (double) this.m_range == (double) end - (double) start)
        return;
      this.m_range_start = start;
      this.m_range = end - start;
      this.SetTrackPosition(position);
      if ((double) this.m_pushbuttonstep != 0.0)
        return;
      this.m_pushbuttonstep = this.m_range * 0.1f;
    }

    public void MoveSlider(float fAmount)
    {
      this.m_track_position += fAmount;
      if ((double) this.m_track_position < (double) this.m_range_start)
        this.m_track_position = this.m_range_start;
      else if ((double) this.m_track_position > (double) this.m_range_start + (double) this.m_range)
        this.m_track_position = this.m_range_start + this.m_range;
      this.Refresh();
      base.OnControlMsg((Element2D) this, ControlMsg.SCROLL_MOVE, this.m_track_position, 0.0f);
    }

    public void SetTrackPosition(float fPosition)
    {
      this.SetTrackPositionNoCallBack(fPosition);
      base.OnControlMsg((Element2D) this, ControlMsg.SCROLL_MOVE, this.m_track_position, 0.0f);
    }

    public void SetTrackPositionNoCallBack(float fPosition)
    {
      this.m_track_position = fPosition;
      if ((double) this.m_track_position < (double) this.m_range_start)
        this.m_track_position = this.m_range_start;
      else if ((double) this.m_track_position > (double) this.m_range_start + (double) this.m_range)
        this.m_track_position = this.m_range_start + this.m_range;
      this.Refresh();
    }

    public void SetTrackPositionToEnd()
    {
      this.SetTrackPosition(this.m_range_start + this.m_range);
    }

    public int RoundingPlace
    {
      get
      {
        return this.rounding_place;
      }
      set
      {
        this.rounding_place = value;
        if (this.rounding_place < 0)
          return;
        this.m_track_position = (float) Math.Round((double) this.m_track_position, this.rounding_place, MidpointRounding.AwayFromZero);
      }
    }

    public float TrackPosition
    {
      get
      {
        return this.m_track_position;
      }
      set
      {
        this.SetTrackPosition(value);
      }
    }

    public float PushButtonStep
    {
      set
      {
        this.m_pushbuttonstep = value;
      }
      get
      {
        return this.m_pushbuttonstep;
      }
    }

    public bool ShowPushButtons
    {
      get
      {
        return this.m_bShowPushButtons;
      }
      set
      {
        this.m_bShowPushButtons = value;
      }
    }

    protected virtual void OnSetTrackSize(float width, float height)
    {
    }

    protected void CopySliderProperties(SliderWidget other)
    {
      this.m_trackSubControl.CopyImageData(other.m_trackSubControl);
      this.m_buttonSubControl.CopyImageData((ImageWidget) other.m_buttonSubControl);
      this.m_pushButtonPlus.CopyImageData((ImageWidget) other.m_pushButtonPlus);
      this.m_pushButtonMinus.CopyImageData((ImageWidget) other.m_pushButtonMinus);
      this.m_track_size = other.m_track_size;
      this.m_track_position = other.m_track_position;
      this.m_button_size = other.m_button_size;
      this.m_button_size_percent = other.m_button_size_percent;
      this.m_range_start = other.m_range_start;
      this.m_range = other.m_range;
      this.m_button_range = other.m_button_range;
      this.m_bShowPushButtons = other.m_bShowPushButtons;
      this.m_pushbuttonstep = other.m_pushbuttonstep;
    }
  }
}
