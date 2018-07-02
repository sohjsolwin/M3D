// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.KeyboardEvent
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

namespace M3D.Graphics
{
  public class KeyboardEvent
  {
    public KeyboardEventType type;
    private bool shift;
    private bool alt;
    private bool ctrl;
    private bool tab;

    public KeyboardEvent(KeyboardEventType type, bool shift, bool alt, bool ctrl)
    {
      this.type = type;
      this.shift = shift;
      this.alt = alt;
      this.ctrl = ctrl;
    }

    public KeyboardEvent(KeyboardEventType type, bool shift, bool alt, bool ctrl, bool tab)
    {
      this.type = type;
      this.shift = shift;
      this.alt = alt;
      this.ctrl = ctrl;
      this.tab = tab;
    }

    public KeyboardEventType Type
    {
      get
      {
        return type;
      }
    }

    public bool Shift
    {
      get
      {
        return shift;
      }
    }

    public bool Alt
    {
      get
      {
        return alt;
      }
    }

    public bool Ctrl
    {
      get
      {
        return ctrl;
      }
    }

    public bool Tab
    {
      get
      {
        return tab;
      }
    }
  }
}
