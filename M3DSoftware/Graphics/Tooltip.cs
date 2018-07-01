// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Tooltip
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using OpenTK.Graphics;
using QuickFont;
using System.Diagnostics;
using System.Windows.Forms;

namespace M3D.Graphics
{
  public class Tooltip : Frame
  {
    private string message = "";
    private bool show;
    private GUIHost host;
    private TextWidget text_information;
    private Stopwatch mytimer;

    public Tooltip(GUIHost host)
      : base(0)
    {
      this.mytimer = new Stopwatch();
      this.mytimer.Reset();
      this.mytimer.Stop();
      this.host = host;
      this.SetSize(300, 100);
      this.X = 0;
      this.Y = 0;
      this.BGColor = new Color4(byte.MaxValue, byte.MaxValue, (byte) 225, byte.MaxValue);
      this.BorderColor = new Color4((byte) 0, (byte) 0, (byte) 0, byte.MaxValue);
      this.text_information = new TextWidget(0);
      this.text_information.Text = "";
      this.text_information.Size = FontSize.Medium;
      this.text_information.Alignment = QFontAlignment.Left;
      this.text_information.VAlignment = TextVerticalAlignment.Middle;
      this.text_information.SetPosition(0, 0);
      this.text_information.SetSize(272, 72);
      this.text_information.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      this.AddChildElement((Element2D) this.text_information);
    }

    public void SetMessage(string message)
    {
      this.message = message;
      QFont currentFont = this.host.GetCurrentFont();
      float num1;
      float num2;
      if (currentFont != null)
      {
        num1 = currentFont.Measure(message).Width;
        num2 = currentFont.Measure(message).Height;
      }
      else
      {
        num1 = 0.0f;
        num2 = 0.0f;
      }
      this.text_information.SetSize((int) ((double) num1 * 1.0), (int) ((double) num2 * 1.7));
      this.SetSize((int) ((double) num1 * 1.0), (int) ((double) num2 * 1.7));
    }

    public void Show(int x, int y)
    {
      if (this.show)
        return;
      this.mytimer.Reset();
      this.mytimer.Start();
      this.show = true;
      if (x + this.Width > this.host.GLWindowWidth())
        this.X = this.host.GLWindowWidth() - this.Width;
      else
        this.X = x;
      Cursor current = Cursor.Current;
      int num = 0;
      if (current != (Cursor) null)
        num = current.Size.Height;
      if (y - this.Height < 0)
        this.Y = y + (int) ((double) num * 0.75);
      else
        this.Y = y - this.Height - num / 20;
    }

    public void Hide()
    {
      this.show = false;
    }

    public override void OnRender(GUIHost host)
    {
      this.text_information.Text = this.message;
      if (!this.show)
        return;
      float elapsedMilliseconds = (float) this.mytimer.ElapsedMilliseconds;
      if ((double) elapsedMilliseconds <= 1000.0)
        return;
      if ((double) elapsedMilliseconds > 10000.0)
      {
        this.show = false;
        this.mytimer.Reset();
      }
      base.OnRender(host);
    }
  }
}
