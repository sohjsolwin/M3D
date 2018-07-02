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
      mytimer = new Stopwatch();
      mytimer.Reset();
      mytimer.Stop();
      this.host = host;
      SetSize(300, 100);
      X = 0;
      Y = 0;
      BGColor = new Color4(byte.MaxValue, byte.MaxValue, 225, byte.MaxValue);
      BorderColor = new Color4(0, 0, 0, byte.MaxValue);
      text_information = new TextWidget(0)
      {
        Text = "",
        Size = FontSize.Medium,
        Alignment = QFontAlignment.Left,
        VAlignment = TextVerticalAlignment.Middle
      };
      text_information.SetPosition(0, 0);
      text_information.SetSize(272, 72);
      text_information.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      AddChildElement(text_information);
    }

    public void SetMessage(string message)
    {
      this.message = message;
      QFont currentFont = host.GetCurrentFont();
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
      text_information.SetSize((int)(num1 * 1.0), (int)(num2 * 1.7));
      SetSize((int)(num1 * 1.0), (int)(num2 * 1.7));
    }

    public void Show(int x, int y)
    {
      if (show)
      {
        return;
      }

      mytimer.Reset();
      mytimer.Start();
      show = true;
      if (x + Width > host.GLWindowWidth())
      {
        X = host.GLWindowWidth() - Width;
      }
      else
      {
        X = x;
      }

      Cursor current = Cursor.Current;
      var num = 0;
      if (current != null)
      {
        num = current.Size.Height;
      }

      if (y - Height < 0)
      {
        Y = y + (int)(num * 0.75);
      }
      else
      {
        Y = y - Height - num / 20;
      }
    }

    public void Hide()
    {
      show = false;
    }

    public override void OnRender(GUIHost host)
    {
      text_information.Text = message;
      if (!show)
      {
        return;
      }

      var elapsedMilliseconds = (float)mytimer.ElapsedMilliseconds;
      if (elapsedMilliseconds <= 1000.0)
      {
        return;
      }

      if (elapsedMilliseconds > 10000.0)
      {
        show = false;
        mytimer.Reset();
      }
      base.OnRender(host);
    }
  }
}
