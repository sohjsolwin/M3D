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
        return ch;
      }
    }
  }
}
