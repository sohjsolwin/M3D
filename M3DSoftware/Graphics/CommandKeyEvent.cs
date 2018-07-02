namespace M3D.Graphics
{
  public class CommandKeyEvent : KeyboardEvent
  {
    private KeyboardCommandKey key;

    public CommandKeyEvent(KeyboardCommandKey key, bool shift, bool alt, bool ctrl)
      : base(KeyboardEventType.CommandKey, shift, alt, ctrl)
    {
      this.key = key;
    }

    public KeyboardCommandKey Key
    {
      get
      {
        return key;
      }
    }
  }
}
