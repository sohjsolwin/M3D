namespace M3D.Graphics.Widgets2D
{
  public class VerticalSliderWidget : SliderWidget
  {
    public VerticalSliderWidget()
      : this(0, (Element2D) null)
    {
    }

    public VerticalSliderWidget(int ID)
      : this(ID, (Element2D) null)
    {
    }

    public VerticalSliderWidget(int ID, Element2D parent)
      : base(ID, parent)
    {
    }

    public VerticalSliderWidget(VerticalSliderWidget source, int ID)
      : this(source, ID, (Element2D) null)
    {
    }

    public VerticalSliderWidget(VerticalSliderWidget source, int ID, Element2D parent)
      : base(ID, parent)
    {
      CopySliderProperties((SliderWidget) source);
    }

    public void InitTrack(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, int growableborder, int min)
    {
      InitTrack(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, growableborder, min, true);
    }

    public override ElementType GetElementType()
    {
      return ElementType.VerticalSliderWidget;
    }

    public override void SetSize(int width, int height)
    {
      base.SetSize(width, height);
      Refresh();
    }

    public Point GetScrollBarPos()
    {
      return new Point() { x = m_buttonSubControl.X_Abs, y = m_buttonSubControl.Y_Abs };
    }

    public override void Refresh()
    {
      var width = Width;
      m_track_size = (float)Height;
      if (ShowPushButtons)
      {
        m_track_size -= (float) (width * 2);
        m_pushButtonPlus.SetSize(width, width);
        m_pushButtonPlus.Y = -width;
        m_pushButtonMinus.SetSize(width, width);
        m_pushButtonMinus.Y = 0;
        m_pushButtonPlus.Visible = true;
        m_pushButtonPlus.Enabled = true;
        m_pushButtonMinus.Visible = true;
        m_pushButtonMinus.Enabled = true;
        m_trackSubControl.SetPosition(0, width);
        m_trackSubControl.SetSize(width, (int)m_track_size);
      }
      else
      {
        m_pushButtonPlus.Visible = false;
        m_pushButtonPlus.Enabled = false;
        m_pushButtonMinus.Visible = false;
        m_pushButtonMinus.Enabled = false;
        m_trackSubControl.SetPosition(0, 0);
        m_trackSubControl.SetSize(width, (int)m_track_size);
      }
      var num = (m_track_size - (float)m_buttonSubControl.Height) / m_range;
      m_button_size = 24f;
      if ((double)m_button_size_percent > 0.0)
      {
        m_buttonSubControl.SetSize(width, (int) ((double)m_button_size_percent * (double)m_track_size));
      }
      else
      {
        m_buttonSubControl.SetSize(width, (int)m_button_size);
      }

      m_buttonSubControl.SetPosition(m_trackSubControl.X, (int) ((double)m_trackSubControl.Y + ((double)m_track_position - (double)m_range_start) * (double) num));
      m_buttonSubControl.SetDraggable(m_trackSubControl.X, m_trackSubControl.X, m_trackSubControl.Y, m_trackSubControl.Y + m_trackSubControl.Height - m_buttonSubControl.Height);
    }

    public override void OnControlMsg(Element2D the_control, ControlMsg msg, float xparam, float yparam)
    {
      if (the_control.GetElementType() == ElementType.ButtonWidget && msg == ControlMsg.MSG_MOVE)
      {
        base.OnControlMsg(the_control, msg, yparam - (float)m_trackSubControl.Y, (float) (m_trackSubControl.Height - m_buttonSubControl.Height));
      }
      else
      {
        base.OnControlMsg(the_control, msg, xparam, yparam);
      }
    }

    public override bool OnMouseCommand(MouseEvent mouseevent)
    {
      if (Enabled && mouseevent.type == MouseEventType.Down && IsListBoxElement())
      {
        ListBoxWidget listBoxElement = GetListBoxElement();
        if (mouseevent.pos.y < listBoxElement.ScrollBar.GetScrollBarPos().y)
        {
          listBoxElement.ScrollBar.MoveSlider(-1f);
        }
        else if (mouseevent.pos.y > listBoxElement.ScrollBar.GetScrollBarPos().y)
        {
          listBoxElement.ScrollBar.MoveSlider(1f);
        }
      }
      return true;
    }

    public override void OnRender(GUIHost host)
    {
      base.OnRender(host);
    }
  }
}
