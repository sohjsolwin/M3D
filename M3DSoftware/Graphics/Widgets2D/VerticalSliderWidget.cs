// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Widgets2D.VerticalSliderWidget
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      this.CopySliderProperties((SliderWidget) source);
    }

    public void InitTrack(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, int growableborder, int min)
    {
      this.InitTrack(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, growableborder, min, true);
    }

    public override ElementType GetElementType()
    {
      return ElementType.VerticalSliderWidget;
    }

    public override void SetSize(int width, int height)
    {
      base.SetSize(width, height);
      this.Refresh();
    }

    public Point GetScrollBarPos()
    {
      return new Point() { x = this.m_buttonSubControl.X_Abs, y = this.m_buttonSubControl.Y_Abs };
    }

    public override void Refresh()
    {
      int width = this.Width;
      this.m_track_size = (float) this.Height;
      if (this.ShowPushButtons)
      {
        this.m_track_size -= (float) (width * 2);
        this.m_pushButtonPlus.SetSize(width, width);
        this.m_pushButtonPlus.Y = -width;
        this.m_pushButtonMinus.SetSize(width, width);
        this.m_pushButtonMinus.Y = 0;
        this.m_pushButtonPlus.Visible = true;
        this.m_pushButtonPlus.Enabled = true;
        this.m_pushButtonMinus.Visible = true;
        this.m_pushButtonMinus.Enabled = true;
        this.m_trackSubControl.SetPosition(0, width);
        this.m_trackSubControl.SetSize(width, (int) this.m_track_size);
      }
      else
      {
        this.m_pushButtonPlus.Visible = false;
        this.m_pushButtonPlus.Enabled = false;
        this.m_pushButtonMinus.Visible = false;
        this.m_pushButtonMinus.Enabled = false;
        this.m_trackSubControl.SetPosition(0, 0);
        this.m_trackSubControl.SetSize(width, (int) this.m_track_size);
      }
      float num = (this.m_track_size - (float) this.m_buttonSubControl.Height) / this.m_range;
      this.m_button_size = 24f;
      if ((double) this.m_button_size_percent > 0.0)
        this.m_buttonSubControl.SetSize(width, (int) ((double) this.m_button_size_percent * (double) this.m_track_size));
      else
        this.m_buttonSubControl.SetSize(width, (int) this.m_button_size);
      this.m_buttonSubControl.SetPosition(this.m_trackSubControl.X, (int) ((double) this.m_trackSubControl.Y + ((double) this.m_track_position - (double) this.m_range_start) * (double) num));
      this.m_buttonSubControl.SetDraggable(this.m_trackSubControl.X, this.m_trackSubControl.X, this.m_trackSubControl.Y, this.m_trackSubControl.Y + this.m_trackSubControl.Height - this.m_buttonSubControl.Height);
    }

    public override void OnControlMsg(Element2D the_control, ControlMsg msg, float xparam, float yparam)
    {
      if (the_control.GetElementType() == ElementType.ButtonWidget && msg == ControlMsg.MSG_MOVE)
        base.OnControlMsg(the_control, msg, yparam - (float) this.m_trackSubControl.Y, (float) (this.m_trackSubControl.Height - this.m_buttonSubControl.Height));
      else
        base.OnControlMsg(the_control, msg, xparam, yparam);
    }

    public override bool OnMouseCommand(MouseEvent mouseevent)
    {
      if (this.Enabled && mouseevent.type == MouseEventType.Down && this.IsListBoxElement())
      {
        ListBoxWidget listBoxElement = this.GetListBoxElement();
        if (mouseevent.pos.y < listBoxElement.ScrollBar.GetScrollBarPos().y)
          listBoxElement.ScrollBar.MoveSlider(-1f);
        else if (mouseevent.pos.y > listBoxElement.ScrollBar.GetScrollBarPos().y)
          listBoxElement.ScrollBar.MoveSlider(1f);
      }
      return true;
    }

    public override void OnRender(GUIHost host)
    {
      base.OnRender(host);
    }
  }
}
