namespace M3D.Graphics.Widgets2D
{
  public class HorizontalSliderWidget : SliderWidget
  {
    public HorizontalSliderWidget()
      : this(0)
    {
    }

    public HorizontalSliderWidget(int ID)
      : this(ID, (Element2D) null)
    {
    }

    public HorizontalSliderWidget(int ID, Element2D parent)
      : base(ID, parent)
    {
    }

    public HorizontalSliderWidget(HorizontalSliderWidget source, int ID)
      : this(source, ID, (Element2D) null)
    {
    }

    public HorizontalSliderWidget(HorizontalSliderWidget source, int ID, Element2D parent)
      : base(ID, parent)
    {
      CopySliderProperties((SliderWidget) source);
    }

    public void InitTrack(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, int growableborder, int min)
    {
      InitTrack(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, growableborder, min, false);
    }

    public override ElementType GetElementType()
    {
      return ElementType.HorizonalSliderWidget;
    }

    public override void SetSize(int width, int height)
    {
      base.SetSize(width, height);
      Refresh();
    }

    public override void Refresh()
    {
      var width = Width;
      var height = Height;
      m_track_size = (float) width;
      if (ShowPushButtons)
      {
        m_track_size -= (float) (height * 2);
        m_pushButtonPlus.SetSize(height, height);
        m_pushButtonPlus.X = -height;
        m_pushButtonMinus.SetSize(height, height);
        m_pushButtonMinus.X = 0;
        m_pushButtonPlus.Visible = true;
        m_pushButtonPlus.Enabled = true;
        m_pushButtonMinus.Visible = true;
        m_pushButtonMinus.Enabled = true;
        m_trackSubControl.SetPosition(height, 0);
        m_trackSubControl.SetSize((int)m_track_size, height);
      }
      else
      {
        m_pushButtonPlus.Visible = false;
        m_pushButtonPlus.Enabled = false;
        m_pushButtonMinus.Visible = false;
        m_pushButtonMinus.Enabled = false;
        m_trackSubControl.SetPosition(0, 0);
        m_trackSubControl.SetSize((int)m_track_size, height);
      }
      var num = (m_track_size - (float)m_buttonSubControl.Width) / m_range;
      if ((double)m_button_size_percent > 0.0)
      {
        m_buttonSubControl.SetSize((int) ((double)m_button_size_percent * (double)m_track_size), height);
      }
      else
      {
        m_buttonSubControl.SetSize((int)m_button_size, height);
      }

      m_buttonSubControl.SetPosition((int) ((double)m_trackSubControl.X + ((double)m_track_position - (double)m_range_start) * (double) num), m_trackSubControl.Y);
      m_buttonSubControl.SetDraggable(m_trackSubControl.X, m_trackSubControl.X + m_trackSubControl.Width - m_buttonSubControl.Width, m_trackSubControl.Y, m_trackSubControl.Y);
    }

    public override void OnControlMsg(Element2D the_control, ControlMsg msg, float xparam, float yparam)
    {
      if (the_control.GetElementType() == ElementType.ButtonWidget && msg == ControlMsg.MSG_MOVE)
      {
        base.OnControlMsg(the_control, msg, xparam - (float)m_trackSubControl.X, (float) (m_trackSubControl.Width - m_buttonSubControl.Width));
      }
      else
      {
        base.OnControlMsg(the_control, msg, xparam, yparam);
      }
    }
  }
}
