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
      verticalSlider = new VerticalSliderWidget(0, (Element2D) this);
      horizonalSlider = new HorizontalSliderWidget(1, (Element2D) this);
      m_frameDrawableRegion = new Frame
      {
        Clipping = true
      };
      m_frameDrawableRegion.SetPosition(2, 2);
      ScollableChildframe = Activator.CreateInstance<T>();
      ScollableChildframe.ID = 2;
      ScollableChildframe.Parent = (Element2D) this;
      ScollableChildframe.always_contains_point = true;
      m_frameDrawableRegion.AddChildElement((Element2D)ScollableChildframe);
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
        {
          return;
        }

        if (verticalSlider.Visible)
        {
          var scrollbarWidth = scrollbar_width;
        }
        ScollableChildframe.Y = -(int) xparam;
      }
      else if (the_control.ID == 1)
      {
        if (msg != ControlMsg.SCROLL_MOVE)
        {
          return;
        }

        if (horizonalSlider.Visible)
        {
          var scrollbarWidth = scrollbar_width;
        }
        ScollableChildframe.X = -(int) xparam;
      }
      else
      {
        base.OnControlMsg(the_control, msg, xparam, yparam);
      }
    }

    public override bool OnMouseCommand(MouseEvent mouseevent)
    {
      if (mouseevent.type != MouseEventType.MouseWheel || !verticalSlider.Visible)
      {
        return base.OnMouseCommand(mouseevent);
      }

      verticalSlider.MoveSlider((float) -((double) mouseevent.delta / 120.0) * verticalSlider.PushButtonStep);
      return true;
    }

    public override void OnRender(GUIHost host)
    {
      base.OnRender(host);
    }

    public void Init(GUIHost host)
    {
      ScollableChildframe.WrapOnNegative = false;
      verticalSlider.InitTrack(host, "guicontrols", 1008f, 73f, 1016f, 95f, 2, 8);
      verticalSlider.InitButton(host, "guicontrols", 1000f, 0.0f, 1023f, 23f, 1000f, 24f, 1023f, 47f, 1000f, 48f, 1023f, 71f, 4, 4, 24);
      verticalSlider.InitMinus(host, "guicontrols", 928f, 48f, 951f, 71f, 952f, 48f, 975f, 71f, 976f, 48f, 999f, 71f);
      verticalSlider.InitPlus(host, "guicontrols", 928f, 72f, 951f, 95f, 952f, 72f, 975f, 95f, 976f, 72f, 999f, 95f);
      verticalSlider.SetButtonSize(24f);
      verticalSlider.ShowPushButtons = true;
      horizonalSlider.InitTrack(host, "guicontrols", 809f, 80f, 831f, 88f, 2, 8);
      horizonalSlider.InitButton(host, "guicontrols", 1000f, 0.0f, 1023f, 23f, 1000f, 24f, 1023f, 47f, 1000f, 48f, 1023f, 71f, 4, 4, 24);
      horizonalSlider.InitMinus(host, "guicontrols", 928f, 0.0f, 951f, 23f, 952f, 0.0f, 975f, 23f, 976f, 0.0f, 999f, 23f);
      horizonalSlider.InitPlus(host, "guicontrols", 928f, 24f, 951f, 47f, 952f, 24f, 975f, 47f, 976f, 24f, 999f, 47f);
      horizonalSlider.SetButtonSize(24f);
      horizonalSlider.ShowPushButtons = true;
      base.AddChildElement((Element2D)verticalSlider);
      base.AddChildElement((Element2D)horizonalSlider);
      base.AddChildElement((Element2D)m_frameDrawableRegion);
      scrollbar_width = 24;
      Refresh();
    }

    public override void AddChildElement(Element2D child)
    {
      ScollableChildframe.AddChildElement(child);
    }

    public override void RemoveAllChildElements()
    {
      ScollableChildframe.RemoveAllChildElements();
    }

    public override void RemoveChildElement(Element2D child)
    {
      ScollableChildframe.RemoveChildElement(child);
    }

    public override void RemoveChildElementAt(int index)
    {
      ScollableChildframe.RemoveChildElementAt(index);
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      Parent = parent;
      lock (ChildList)
      {
        foreach (Element2D child in (IEnumerable<Element2D>)ChildList)
        {
          ScollableChildframe.AddChildElement(child);
        }

        ChildList.Clear();
        ScollableChildframe.InitChildren((Element2D) this, host, MyButtonCallback);
      }
      Init(host);
      ScollableChildframe.Refresh();
    }

    public override void Refresh()
    {
      bool flag1;
      bool flag2;
      if (ShowScrollbar == GenericScrollFrame<T>.ScrollBarState.Off)
      {
        flag1 = false;
        flag2 = false;
      }
      else if (ShowScrollbar == GenericScrollFrame<T>.ScrollBarState.On)
      {
        flag1 = true;
        flag2 = true;
      }
      else
      {
        flag1 = ScollableChildframe.Width > Width;
        flag2 = ScollableChildframe.Height > Height;
      }
      m_frameDrawableRegion.Width = (flag2 ? Width - scrollbar_width : Width) - 4;
      m_frameDrawableRegion.Height = (flag1 ? Height - scrollbar_width : Height) - 4;
      if (flag2)
      {
        var height = flag1 ? Height - scrollbar_width : Height;
        verticalSlider.SetPosition(-scrollbar_width, 0);
        verticalSlider.SetSize(scrollbar_width, height);
        var num = ScollableChildframe.Height - m_frameDrawableRegion.Height;
        if (num < 0)
        {
          num = 0;
        }

        verticalSlider.Visible = true;
        verticalSlider.Enabled = true;
        verticalSlider.SetRange(0.0f, (float) (ScollableChildframe.Height - Height));
        verticalSlider.PushButtonStep = (float) (int) Math.Ceiling((double) num / 10.0);
      }
      else
      {
        if (verticalSlider.Visible)
        {
          verticalSlider.SetTrackPosition(0.0f);
        }

        verticalSlider.Visible = false;
        verticalSlider.Enabled = false;
      }
      if (flag1)
      {
        var width = flag2 ? Width - scrollbar_width : Width;
        horizonalSlider.SetPosition(0, -scrollbar_width);
        horizonalSlider.SetSize(width, scrollbar_width);
        var num = ScollableChildframe.Width - m_frameDrawableRegion.Width;
        if (num < 0)
        {
          num = 0;
        }

        horizonalSlider.Visible = true;
        horizonalSlider.Enabled = true;
        horizonalSlider.SetRange(0.0f, (float) (ScollableChildframe.Width - Width));
        horizonalSlider.PushButtonStep = 1f;
        horizonalSlider.PushButtonStep = (float) (int) Math.Ceiling((double) num / 10.0);
      }
      else
      {
        if (horizonalSlider.Visible)
        {
          horizonalSlider.SetTrackPosition(0.0f);
        }

        horizonalSlider.Visible = false;
        horizonalSlider.Enabled = false;
      }
    }

    public override void SetPosition(int x, int y)
    {
      base.SetPosition(x, y);
      Refresh();
    }

    public override void SetSize(int width, int height)
    {
      base.SetSize(width, height);
      Refresh();
    }

    public void VerticalMoveSlider(float fAmount)
    {
      verticalSlider.MoveSlider(fAmount);
    }

    public void HorizonalMoveSlider(float fAmount)
    {
      horizonalSlider.MoveSlider(fAmount);
    }

    [XmlIgnore]
    public Element2D MovableFrame
    {
      get
      {
        return (Element2D)ScollableChildframe;
      }
    }

    [XmlAttribute("ShowScrollbar")]
    public GenericScrollFrame<T>.ScrollBarState ShowScrollbar
    {
      set
      {
        _showScrollbar = value;
        Refresh();
      }
      get
      {
        return _showScrollbar;
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
