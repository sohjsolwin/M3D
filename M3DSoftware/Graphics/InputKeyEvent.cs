// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.InputKeyEvent
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

namespace M3D.Graphics
{
  public class InputKeyEvent : KeyboardEvent
  {
    private char ch;

    public InputKeyEvent(char ch, bool shift, bool alt, bool ctrl)
      : base(KeyboardEventType.InputKey, shift, alt, ctrl)
    {
      this.ch = ch;
    }

    public InputKeyEvent(char ch, bool shift, bool alt, bool ctrl, bool tab)
      : base(KeyboardEventType.InputKey, shift, alt, ctrl, tab)
    {
      this.ch = ch;
    }

    public char Ch
    {
      get
      {
        return this.ch;
      }
    }
  }
}
