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
