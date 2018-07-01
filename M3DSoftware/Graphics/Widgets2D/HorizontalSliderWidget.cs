// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Widgets2D.HorizontalSliderWidget
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      this.CopySliderProperties((SliderWidget) source);
    }

    public void InitTrack(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, int growableborder, int min)
    {
      this.InitTrack(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, growableborder, min, false);
    }

    public override ElementType GetElementType()
    {
      return ElementType.HorizonalSliderWidget;
    }

    public override void SetSize(int width, int height)
    {
      base.SetSize(width, height);
      this.Refresh();
    }

    public override void Refresh()
    {
      int width = this.Width;
      int height = this.Height;
      this.m_track_size = (float) width;
      if (this.ShowPushButtons)
      {
        this.m_track_size -= (float) (height * 2);
        this.m_pushButtonPlus.SetSize(height, height);
        this.m_pushButtonPlus.X = -height;
        this.m_pushButtonMinus.SetSize(height, height);
        this.m_pushButtonMinus.X = 0;
        this.m_pushButtonPlus.Visible = true;
        this.m_pushButtonPlus.Enabled = true;
        this.m_pushButtonMinus.Visible = true;
        this.m_pushButtonMinus.Enabled = true;
        this.m_trackSubControl.SetPosition(height, 0);
        this.m_trackSubControl.SetSize((int) this.m_track_size, height);
      }
      else
      {
        this.m_pushButtonPlus.Visible = false;
        this.m_pushButtonPlus.Enabled = false;
        this.m_pushButtonMinus.Visible = false;
        this.m_pushButtonMinus.Enabled = false;
        this.m_trackSubControl.SetPosition(0, 0);
        this.m_trackSubControl.SetSize((int) this.m_track_size, height);
      }
      float num = (this.m_track_size - (float) this.m_buttonSubControl.Width) / this.m_range;
      if ((double) this.m_button_size_percent > 0.0)
        this.m_buttonSubControl.SetSize((int) ((double) this.m_button_size_percent * (double) this.m_track_size), height);
      else
        this.m_buttonSubControl.SetSize((int) this.m_button_size, height);
      this.m_buttonSubControl.SetPosition((int) ((double) this.m_trackSubControl.X + ((double) this.m_track_position - (double) this.m_range_start) * (double) num), this.m_trackSubControl.Y);
      this.m_buttonSubControl.SetDraggable(this.m_trackSubControl.X, this.m_trackSubControl.X + this.m_trackSubControl.Width - this.m_buttonSubControl.Width, this.m_trackSubControl.Y, this.m_trackSubControl.Y);
    }

    public override void OnControlMsg(Element2D the_control, ControlMsg msg, float xparam, float yparam)
    {
      if (the_control.GetElementType() == ElementType.ButtonWidget && msg == ControlMsg.MSG_MOVE)
        base.OnControlMsg(the_control, msg, xparam - (float) this.m_trackSubControl.X, (float) (this.m_trackSubControl.Width - this.m_buttonSubControl.Width));
      else
        base.OnControlMsg(the_control, msg, xparam, yparam);
    }
  }
}
