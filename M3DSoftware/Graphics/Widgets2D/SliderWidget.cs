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
      m_trackSubControl = new ImageWidget(0, (Element2D) this);
      m_buttonSubControl = new ButtonWidget(0, (Element2D) this);
      m_pushButtonPlus = new ButtonWidget(1, (Element2D) this);
      m_pushButtonMinus = new ButtonWidget(2, (Element2D) this);
      m_buttonSubControl.DontMove = true;
      m_pushButtonPlus.DontMove = true;
      m_pushButtonMinus.DontMove = true;
      m_trackSubControl.IgnoreMouse = true;
      ChildList = ChildList + (Element2D)m_trackSubControl;
      ChildList = ChildList + (Element2D)m_buttonSubControl;
      ChildList = ChildList + (Element2D)m_pushButtonPlus;
      ChildList = ChildList + (Element2D)m_pushButtonMinus;
      m_track_size = 0.0f;
      m_track_position = 0.0f;
      m_button_size = 0.0f;
      m_button_size_percent = 0.0f;
      m_range_start = 0.0f;
      m_range = 1f;
      m_button_range = 0.0f;
    }

    public override ElementType GetElementType()
    {
      return ElementType.SliderWidget;
    }

    public override void SetSize(int width, int height)
    {
      base.SetSize(width, height);
      OnSetTrackSize((float) width, (float) height);
    }

    public override void SetPosition(int x, int y)
    {
      base.SetPosition(x, y);
    }

    public override void SetEnabled(bool bEnabled)
    {
      base.SetEnabled(bEnabled);
      m_buttonSubControl.Enabled = bEnabled;
      m_pushButtonPlus.Enabled = bEnabled;
      m_pushButtonMinus.Enabled = bEnabled;
      m_buttonSubControl.Visible = bEnabled;
    }

    public override void OnControlMsg(Element2D the_control, ControlMsg msg, float xparam, float yparam)
    {
      if (msg == ControlMsg.MSG_HIT)
      {
        if (the_control.ID == 2)
        {
          SetTrackPosition(m_track_position - m_pushbuttonstep);
        }
        else if (the_control.ID == 1)
        {
          SetTrackPosition(m_track_position + m_pushbuttonstep);
        }
      }
      if (the_control.GetElementType() == ElementType.ButtonWidget && msg == ControlMsg.MSG_MOVE && the_control.ID == 0)
      {
        var fPosition = xparam / yparam * m_range + m_range_start;
        if (rounding_place >= 0)
        {
          fPosition = (float) Math.Round((double) fPosition, rounding_place, MidpointRounding.AwayFromZero);
        }

        SetTrackPosition(fPosition);
      }
      if (the_control != null)
      {
        return;
      }

      SetTrackPosition(xparam / yparam * m_range + m_range_start);
    }

    public void InitPlus(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, float over_u0, float over_v0, float over_u1, float over_v1, float down_u0, float down_v0, float down_u1, float down_v1)
    {
      m_pushButtonPlus.Init(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, over_u0, over_v0, over_u1, over_v1, down_u0, down_v0, down_u1, down_v1);
    }

    public void InitMinus(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, float over_u0, float over_v0, float over_u1, float over_v1, float down_u0, float down_v0, float down_u1, float down_v1)
    {
      m_pushButtonMinus.Init(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, over_u0, over_v0, over_u1, over_v1, down_u0, down_v0, down_u1, down_v1);
    }

    public virtual void InitButton(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, float over_u0, float over_v0, float over_u1, float over_v1, float down_u0, float down_v0, float down_u1, float down_v1, int topborder, int bottomborder, int minsize)
    {
      m_buttonSubControl.Init(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, over_u0, over_v0, over_u1, over_v1, down_u0, down_v0, down_u1, down_v1);
    }

    public void InitTrack(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, int growableborder, int min, bool isvertical)
    {
      InitTrack(host, texture, normal_u0, normal_v0, normal_u1, normal_v1);
      if (isvertical)
      {
        m_trackSubControl.SetGrowableHeight(growableborder, growableborder, min);
      }
      else
      {
        m_trackSubControl.SetGrowableWidth(growableborder, growableborder, min);
      }
    }

    public void InitTrack(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1)
    {
      m_trackSubControl.Init(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, normal_u0, normal_v0, normal_u1, normal_v1, normal_u0, normal_v0, normal_u1, normal_v1);
    }

    public void SetButtonSize(float size)
    {
      m_button_size = size;
      m_button_size_percent = -1f;
    }

    public void SetButtonSizeByPercent(float size)
    {
      m_button_size_percent = size;
      m_button_size = -1f;
    }

    public void SetRange(float start, float end)
    {
      SetRange(start, end, start);
    }

    public void SetRange(float start, float end, float position)
    {
      if ((double)m_range_start == (double) start && (double)m_range == (double) end - (double) start)
      {
        return;
      }

      m_range_start = start;
      m_range = end - start;
      SetTrackPosition(position);
      if ((double)m_pushbuttonstep != 0.0)
      {
        return;
      }

      m_pushbuttonstep = m_range * 0.1f;
    }

    public void MoveSlider(float fAmount)
    {
      m_track_position += fAmount;
      if ((double)m_track_position < (double)m_range_start)
      {
        m_track_position = m_range_start;
      }
      else if ((double)m_track_position > (double)m_range_start + (double)m_range)
      {
        m_track_position = m_range_start + m_range;
      }

      Refresh();
      base.OnControlMsg((Element2D) this, ControlMsg.SCROLL_MOVE, m_track_position, 0.0f);
    }

    public void SetTrackPosition(float fPosition)
    {
      SetTrackPositionNoCallBack(fPosition);
      base.OnControlMsg((Element2D) this, ControlMsg.SCROLL_MOVE, m_track_position, 0.0f);
    }

    public void SetTrackPositionNoCallBack(float fPosition)
    {
      m_track_position = fPosition;
      if ((double)m_track_position < (double)m_range_start)
      {
        m_track_position = m_range_start;
      }
      else if ((double)m_track_position > (double)m_range_start + (double)m_range)
      {
        m_track_position = m_range_start + m_range;
      }

      Refresh();
    }

    public void SetTrackPositionToEnd()
    {
      SetTrackPosition(m_range_start + m_range);
    }

    public int RoundingPlace
    {
      get
      {
        return rounding_place;
      }
      set
      {
        rounding_place = value;
        if (rounding_place < 0)
        {
          return;
        }

        m_track_position = (float) Math.Round((double)m_track_position, rounding_place, MidpointRounding.AwayFromZero);
      }
    }

    public float TrackPosition
    {
      get
      {
        return m_track_position;
      }
      set
      {
        SetTrackPosition(value);
      }
    }

    public float PushButtonStep
    {
      set
      {
        m_pushbuttonstep = value;
      }
      get
      {
        return m_pushbuttonstep;
      }
    }

    public bool ShowPushButtons
    {
      get
      {
        return m_bShowPushButtons;
      }
      set
      {
        m_bShowPushButtons = value;
      }
    }

    protected virtual void OnSetTrackSize(float width, float height)
    {
    }

    protected void CopySliderProperties(SliderWidget other)
    {
      m_trackSubControl.CopyImageData(other.m_trackSubControl);
      m_buttonSubControl.CopyImageData((ImageWidget) other.m_buttonSubControl);
      m_pushButtonPlus.CopyImageData((ImageWidget) other.m_pushButtonPlus);
      m_pushButtonMinus.CopyImageData((ImageWidget) other.m_pushButtonMinus);
      m_track_size = other.m_track_size;
      m_track_position = other.m_track_position;
      m_button_size = other.m_button_size;
      m_button_size_percent = other.m_button_size_percent;
      m_range_start = other.m_range_start;
      m_range = other.m_range;
      m_button_range = other.m_button_range;
      m_bShowPushButtons = other.m_bShowPushButtons;
      m_pushbuttonstep = other.m_pushbuttonstep;
    }
  }
}
