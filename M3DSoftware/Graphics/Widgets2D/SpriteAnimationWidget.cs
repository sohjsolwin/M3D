// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Widgets2D.SpriteAnimationWidget
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      this.timer = new Stopwatch();
    }

    public SpriteAnimationWidget(int ID)
      : this(ID, (Element2D) null)
    {
      this.timer = new Stopwatch();
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
      this.frames = new Simple2DRenderer.TexturedQuad[num_frames];
      int num1 = 0;
      int num2 = 0;
      int num3 = (int) ((double) normal_u1 - (double) normal_u0) / num_columns + 1;
      int num4 = (int) ((double) normal_v1 - (double) normal_v0) / num_rows + 1;
      float num5 = 0.0f;
      float num6 = 0.0f;
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
      for (int index = 0; index < num_frames; ++index)
      {
        this.frames[index].color = new Color4(1f, 1f, 1f, 1f);
        this.frames[index].x0 = 0.0f;
        this.frames[index].y0 = 0.0f;
        this.frames[index].x1 = 1f;
        this.frames[index].y1 = 1f;
        this.frames[index].u0 = (normal_u0 + (float) (num1 * num3)) * num7;
        this.frames[index].v0 = (normal_v0 + (float) (num2 * num4)) * num8;
        this.frames[index].u1 = (normal_u0 + (float) ((num1 + 1) * num3)) * num7 + num5;
        this.frames[index].v1 = (normal_v0 + (float) ((num2 + 1) * num4)) * num8 + num6;
        ++num1;
        if (num1 >= num_columns)
        {
          num1 = 0;
          ++num2;
        }
      }
      this.texture = host.GetSimpleRenderer().LoadTextureFromFile(texture);
      this.curframe = 0U;
      this.frame_time = frame_time;
      this.timer.Reset();
      this.timer.Start();
    }

    public override void OnRender(GUIHost host)
    {
      if (this.timer.ElapsedMilliseconds > (long) this.frame_time)
      {
        this.timer.Reset();
        ++this.curframe;
        if ((long) this.curframe >= (long) this.frames.Length)
          this.curframe = 0U;
        this.timer.Start();
      }
      if (this.texture != uint.MaxValue)
      {
        Simple2DRenderer simpleRenderer = host.GetSimpleRenderer();
        simpleRenderer.SetCurrentTexture(this.texture);
        this.frames[(int) this.curframe].x0 = (float) this.X_Abs;
        this.frames[(int) this.curframe].y0 = (float) this.Y_Abs;
        this.frames[(int) this.curframe].x1 = (float) (this.X_Abs + this.Width);
        this.frames[(int) this.curframe].y1 = (float) (this.Y_Abs + this.Height);
        simpleRenderer.DrawQuad(this.frames[(int) this.curframe]);
      }
      base.OnRender(host);
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      this.Parent = parent;
      this.Init(host, "guicontrols", this.u0, this.v0, this.u1, this.v1, this.columns, this.rows, this.no_frames, this.frame_time);
      lock (this.ChildList)
      {
        foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
          child.InitChildren((Element2D) this, host, MyButtonCallback);
      }
    }
  }
}
