using OpenTK.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace M3D.Graphics.Widgets2D
{
  public class SpriteAnimationWidget : ImageWidget
  {
    private Simple2DRenderer.TexturedQuad[] frames;
    private uint texture;
    private uint curframe;
    [XmlAttribute("frame-time")]
    public uint frame_time;
    [XmlAttribute("frames")]
    public int no_frames;
    [XmlAttribute("columns")]
    public int columns;
    [XmlAttribute("rows")]
    public int rows;
    private Stopwatch timer;

    public SpriteAnimationWidget()
      : this(0, (Element2D) null)
    {
      timer = new Stopwatch();
    }

    public SpriteAnimationWidget(int ID)
      : this(ID, (Element2D) null)
    {
      timer = new Stopwatch();
    }

    public SpriteAnimationWidget(int ID, Element2D parent)
      : base(ID, parent)
    {
    }

    public override ElementType GetElementType()
    {
      return ElementType.SpriteAnimationWidget;
    }

    public void Init(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, int num_columns, int num_rows, int num_frames, uint frame_time)
    {
      frames = new Simple2DRenderer.TexturedQuad[num_frames];
      var num1 = 0;
      var num2 = 0;
      var num3 = (int) ((double) normal_u1 - (double) normal_u0) / num_columns + 1;
      var num4 = (int) ((double) normal_v1 - (double) normal_v0) / num_rows + 1;
      var num5 = 0.0f;
      var num6 = 0.0f;
      float num7;
      float num8;
      if (Sprite.pixel_perfect)
      {
        num5 = 1f / (float) Sprite.texture_width_pixels;
        num6 = 1f / (float) Sprite.texture_height_pixels;
        num7 = 1f / (float) Sprite.texture_width_pixels;
        num8 = 1f / (float) Sprite.texture_height_pixels;
      }
      else
      {
        num7 = 1f / (float) Sprite.texture_width_pixels;
        num8 = 1f / (float) Sprite.texture_height_pixels;
      }
      for (var index = 0; index < num_frames; ++index)
      {
        frames[index].color = new Color4(1f, 1f, 1f, 1f);
        frames[index].x0 = 0.0f;
        frames[index].y0 = 0.0f;
        frames[index].x1 = 1f;
        frames[index].y1 = 1f;
        frames[index].u0 = (normal_u0 + (float) (num1 * num3)) * num7;
        frames[index].v0 = (normal_v0 + (float) (num2 * num4)) * num8;
        frames[index].u1 = (normal_u0 + (float) ((num1 + 1) * num3)) * num7 + num5;
        frames[index].v1 = (normal_v0 + (float) ((num2 + 1) * num4)) * num8 + num6;
        ++num1;
        if (num1 >= num_columns)
        {
          num1 = 0;
          ++num2;
        }
      }
      this.texture = host.GetSimpleRenderer().LoadTextureFromFile(texture);
      curframe = 0U;
      this.frame_time = frame_time;
      timer.Reset();
      timer.Start();
    }

    public override void OnRender(GUIHost host)
    {
      if (timer.ElapsedMilliseconds > (long)frame_time)
      {
        timer.Reset();
        ++curframe;
        if ((long)curframe >= (long)frames.Length)
        {
          curframe = 0U;
        }

        timer.Start();
      }
      if (texture != uint.MaxValue)
      {
        Simple2DRenderer simpleRenderer = host.GetSimpleRenderer();
        simpleRenderer.SetCurrentTexture(texture);
        frames[(int)curframe].x0 = (float)X_Abs;
        frames[(int)curframe].y0 = (float)Y_Abs;
        frames[(int)curframe].x1 = (float) (X_Abs + Width);
        frames[(int)curframe].y1 = (float) (Y_Abs + Height);
        simpleRenderer.DrawQuad(frames[(int)curframe]);
      }
      base.OnRender(host);
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      Parent = parent;
      Init(host, "guicontrols", u0, v0, u1, v1, columns, rows, no_frames, frame_time);
      lock (ChildList)
      {
        foreach (Element2D child in (IEnumerable<Element2D>)ChildList)
        {
          child.InitChildren((Element2D) this, host, MyButtonCallback);
        }
      }
    }
  }
}
