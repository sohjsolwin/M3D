using M3D.Graphics.Ext3D;
using M3D.Graphics.Frames_and_Layouts;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace M3D.Graphics.Widgets2D
{
  [XmlInclude(typeof (SpriteAnimationWidget))]
  public class Element2D : IElement
  {
    [XmlIgnore]
    public int tabIndex = -1;
    [XmlIgnore]
    private float relative_x = -1000f;
    [XmlIgnore]
    private float relative_y = -1000f;
    [XmlIgnore]
    private float relative_height = -1f;
    [XmlIgnore]
    private float relative_width = -1f;
    [XmlElement("BorderedImageFrame", Type = typeof (BorderedImageFrame))]
    [XmlElement("Frame3DView", Type = typeof (Frame3DView))]
    [XmlElement("GridLayout", Type = typeof (GridLayout))]
    [XmlElement("HorizontalLayout", Type = typeof (HorizontalLayout))]
    [XmlElement("VerticalLayout", Type = typeof (VerticalLayout))]
    [XmlElement("HorizontalLayoutScrollList", Type = typeof (HorizontalLayoutScrollList))]
    [XmlElement("XMLFrame", Type = typeof (XMLFrame))]
    [XmlElement("ScrollFrame", Type = typeof (ScrollFrame))]
    [XmlElement("ButtonWidget", Type = typeof (ButtonWidget))]
    [XmlElement("ComboBoxWidget", Type = typeof (ComboBoxWidget))]
    [XmlElement("EditBoxWidget", Type = typeof (EditBoxWidget))]
    [XmlElement("HorizontalSliderWidget", Type = typeof (HorizontalSliderWidget))]
    [XmlElement("ImageWidget", Type = typeof (ImageWidget))]
    [XmlElement("ListBoxWidget", Type = typeof (ListBoxWidget))]
    [XmlElement("ProgressBarWidget", Type = typeof (ProgressBarWidget))]
    [XmlElement("QuadWidget", Type = typeof (QuadWidget))]
    [XmlElement("SpriteAnimationWidget", Type = typeof (SpriteAnimationWidget))]
    [XmlElement("ScrollableVerticalLayout", Type = typeof (ScrollableVerticalLayout))]
    [XmlElement("TextWidget", Type = typeof (TextWidget))]
    [XmlElement("TreeNodeWidget", Type = typeof (TreeNodeWidget))]
    [XmlElement("TreeViewWidget", Type = typeof (TreeViewWidget))]
    [XmlElement("VerticalSliderWidget", Type = typeof (VerticalSliderWidget))]
    public Element2DList ChildList;
    [XmlAttribute("center-horizontally")]
    public bool CenterHorizontallyInParent;
    [XmlAttribute("center-vertically")]
    public bool CenterVerticallyInParent;
    [XmlAttribute("fade-when-disabled")]
    public bool FadeWhenDisabled;
    [XmlIgnore]
    public ElementStandardDelegate DoOnParentResize;
    [XmlIgnore]
    public ElementStandardDelegate DoOnParentMove;
    [XmlIgnore]
    public OnControlMsgDelegate OnControlMsgCallback;
    [XmlIgnore]
    private bool hasfocus;
    [XmlIgnore]
    private Point absolutelocation;
    [XmlIgnore]
    private Point location;
    [XmlIgnore]
    private Dimensions size;
    [XmlIgnore]
    private bool _bActive;
    [XmlIgnore]
    private Element2D parent;
    [XmlIgnore]
    private int __MinWidth;
    [XmlIgnore]
    private int __MinHeight;
    [XmlIgnore]
    private int __MaxWidth;
    [XmlIgnore]
    private int __MaxHeight;
    [XmlIgnore]
    private bool wraponnegativeX;
    [XmlIgnore]
    private bool wraponnegativeY;
    [XmlIgnore]
    private object data;
    [XmlIgnore]
    private bool ignoremouse;
    [XmlIgnore]
    private Element2D dropdownelement;
    [XmlIgnore]
    private bool relativeposition;
    [XmlIgnore]
    private int relative_x_adj;
    [XmlIgnore]
    private int relative_y_adj;
    [XmlIgnore]
    private int relative_height_adj;
    [XmlIgnore]
    private int relative_width_adj;
    [XmlIgnore]
    private int auto_center_x_offset;
    [XmlIgnore]
    private int auto_center_y_offset;
    [XmlIgnore]
    protected bool bUpdateWhenNotVisible;

    public Element2D()
      : this(0, null)
    {
    }

    public Element2D(int ID)
      : this(ID, null)
    {
    }

    public Element2D(int ID, Element2D parent)
      : base(ID, parent)
    {
      ChildList = new Element2DList(this);
      CenterHorizontallyInParent = false;
      CenterVerticallyInParent = false;
      this.parent = parent;
      _bActive = false;
      wraponnegativeX = true;
      wraponnegativeY = true;
      location.x = 0;
      location.y = 0;
      size.width = 0;
      size.height = 0;
      __MinWidth = 0;
      __MinHeight = 0;
      __MaxWidth = 0;
      __MaxHeight = 0;
      auto_center_x_offset = 0;
      auto_center_y_offset = 0;
      IgnoreMouse = false;
      hasfocus = false;
      relativeposition = false;
      dropdownelement = null;
    }

    public virtual void OnControlMsg(Element2D the_control, ControlMsg msg, float xparam, float yparam)
    {
      if (OnControlMsgCallback != null)
      {
        OnControlMsgCallback(the_control, msg, xparam, yparam);
      }
      else
      {
        if (parent == null)
        {
          return;
        }

        parent.OnControlMsg(the_control, msg, xparam, yparam);
      }
    }

    public void TurnOffGroup(int group, Element2D except)
    {
      if (group == -1)
      {
        return;
      }

      foreach (Element2D child in ChildList)
      {
        if (child.GroupID == group && child != except)
        {
          child.SetOff();
        }
      }
    }

    public void DisableGroup(int groupID)
    {
      if (GroupID == groupID)
      {
        Enabled = false;
      }

      if (groupID == -1)
      {
        return;
      }

      foreach (Element2D child in ChildList)
      {
        child.DisableGroup(groupID);
      }
    }

    public void EnableGroup(int groupID)
    {
      if (GroupID == groupID)
      {
        Enabled = true;
      }

      if (groupID == -1)
      {
        return;
      }

      foreach (Element2D child in ChildList)
      {
        child.EnableGroup(groupID);
      }
    }

    public override void Refresh()
    {
      OnParentResize();
    }

    public virtual void RemoveAllChildElements()
    {
      ChildList.Clear();
    }

    public virtual void RemoveChildElementAt(int index)
    {
      ChildList.RemoveAt(index);
    }

    public bool IsComboBoxElement()
    {
      if (GetElementType() == ElementType.ComboBoxWidget)
      {
        return true;
      }

      for (Element2D parent = Parent; parent != null; parent = parent.Parent)
      {
        if (parent.GetElementType() == ElementType.ComboBoxWidget)
        {
          return true;
        }
      }
      return false;
    }

    public bool IsListBoxElement()
    {
      if (GetElementType() == ElementType.ListBoxWidget)
      {
        return true;
      }

      for (Element2D parent = Parent; parent != null; parent = parent.Parent)
      {
        if (parent.GetElementType() == ElementType.ListBoxWidget)
        {
          return true;
        }
      }
      return false;
    }

    public Frame FindParentFrame()
    {
      if (IsScrollFrame())
      {
        return (Frame) this;
      }

      if (Parent != null)
      {
        return Parent.FindParentFrame();
      }

      return null;
    }

    public ComboBoxWidget GetComboBoxElement()
    {
      if (GetElementType() == ElementType.ComboBoxWidget)
      {
        return (ComboBoxWidget) this;
      }

      for (Element2D parent = Parent; parent != null; parent = parent.Parent)
      {
        if (parent.GetElementType() == ElementType.ComboBoxWidget)
        {
          return (ComboBoxWidget) parent;
        }
      }
      return null;
    }

    public ListBoxWidget GetListBoxElement()
    {
      if (GetElementType() == ElementType.ListBoxWidget)
      {
        return (ListBoxWidget) this;
      }

      for (Element2D parent = Parent; parent != null; parent = parent.Parent)
      {
        if (parent.GetElementType() == ElementType.ListBoxWidget)
        {
          return (ListBoxWidget) parent;
        }
      }
      return null;
    }

    public bool IsScrollFrame()
    {
      if (GetElementType() == ElementType.ScrollFrame)
      {
        return true;
      }

      for (Element2D parent = Parent; parent != null; parent = parent.Parent)
      {
        if (parent.GetElementType() == ElementType.ScrollFrame)
        {
          return true;
        }
      }
      return false;
    }

    public Frame GetScrollFrame()
    {
      if (GetElementType() == ElementType.ScrollFrame)
      {
        return (Frame) this;
      }

      for (Element2D parent = Parent; parent != null; parent = parent.Parent)
      {
        if (parent.GetElementType() == ElementType.ScrollFrame)
        {
          return (Frame) parent;
        }
      }
      return null;
    }

    public Element2D FindChildElement(int ID)
    {
      foreach (Element2D child in ChildList)
      {
        if (child.ID == ID)
        {
          return child;
        }

        Element2D childElement = child.FindChildElement(ID);
        if (childElement != null)
        {
          return childElement;
        }
      }
      return null;
    }

    public Element2D FindChildElement(string tag)
    {
      foreach (Element2D child in ChildList)
      {
        if (child.tag == tag)
        {
          return child;
        }

        Element2D childElement = child.FindChildElement(tag);
        if (childElement != null)
        {
          return childElement;
        }
      }
      return null;
    }

    public virtual void SetOff()
    {
    }

    public virtual ElementType GetElementType()
    {
      return ElementType.Element;
    }

    public virtual void Clear()
    {
      RemoveAllChildElements();
    }

    private int CalculateXFromRelative()
    {
      var num = (int)(Parent.Width * (double)relative_x) + relative_x_adj;
      if (num < 0)
      {
        num = 0;
      }

      return num;
    }

    private int CalculateYFromRelative()
    {
      var num = (int)(Parent.Height * (double)relative_y) + relative_y_adj;
      if (num < 0)
      {
        num = 0;
      }

      return num;
    }

    private int CalculateWidthFromRelative()
    {
      return (int)(Parent.Width * (double)relative_width) + relative_width_adj;
    }

    private int CalculateHeightFromRelative()
    {
      return (int)(Parent.Height * (double)relative_height) + relative_height_adj;
    }

    public virtual void OnParentResize()
    {
      if (Parent == null)
      {
        return;
      }

      if (relative_width > 0.0 || relative_height > 0.0)
      {
        var width = Width;
        var height = Height;
        if (relative_width > 0.0)
        {
          width = CalculateWidthFromRelative();
        }

        if (relative_height > 0.0)
        {
          height = CalculateHeightFromRelative();
        }

        SetSize(width, height);
      }
      var x = X;
      var y = Y;
      if (CenterHorizontallyInParent || CenterVerticallyInParent)
      {
        if (CenterHorizontallyInParent)
        {
          x = (Parent.Width - Width - auto_center_x_offset) / 2 + auto_center_x_offset;
          if (x < 0)
          {
            x = 0;
          }
        }
        if (CenterVerticallyInParent)
        {
          y = (Parent.Height - Height - auto_center_y_offset) / 2 + auto_center_y_offset;
          if (y < 0)
          {
            y = 0;
          }
        }
      }
      if (UseRelativePositions)
      {
        if (relative_y > 0.0)
        {
          y = CalculateYFromRelative();
        }

        if (relative_x > 0.0)
        {
          x = CalculateXFromRelative();
        }
      }
      SetPosition(x, y);
      DoOnParentResize?.Invoke();

      foreach (Element2D child in ChildList)
      {
        child.OnParentResize();
      }
    }

    public virtual void OnFocusChanged()
    {
    }

    public virtual bool OnKeyboardEvent(KeyboardEvent keyboardevent)
    {
      foreach (Element2D child in ChildList)
      {
        if (child.OnKeyboardEvent(keyboardevent))
        {
          return true;
        }
      }
      return false;
    }

    public virtual void OnMouseLeave()
    {
      foreach (Element2D child in ChildList)
      {
        child.OnMouseLeave();
      }
    }

    public virtual bool OnMouseCommand(MouseEvent mouseevent)
    {
      if (dropdownelement != null)
      {
        return dropdownelement.OnMouseCommand(mouseevent);
      }

      return false;
    }

    public virtual void OnMouseMove(int x, int y)
    {
      foreach (Element2D child in ChildList)
      {
        if (child.Enabled || IElement.HasEnabledOnlyItem && child.HasChildren)
        {
          child.OnMouseMove(x, y);
        }
      }
    }

    public bool Overlaps(Element2D other)
    {
      return Overlaps(other, 0, 0);
    }

    public bool Overlaps(Element2D other, int x_off, int y_off)
    {
      return X_Abs + Width + x_off >= other.X_Abs && X_Abs + x_off <= other.X_Abs + other.Width && (Y_Abs + Height + y_off >= other.Y_Abs && Y_Abs + y_off <= other.Y_Abs + other.Height);
    }

    public virtual bool ContainsPoint(int x, int y)
    {
      return x >= absolutelocation.x && y >= absolutelocation.y && (x <= absolutelocation.x + size.width && y <= absolutelocation.y + size.height);
    }

    public virtual Element2D GetSelfOrDependantAtPoint(int x, int y)
    {
      if (ContainsPoint(x, y) && Visible && (Enabled || ChildList.Count > 0))
      {
        foreach (Element2D element2D in ChildList.Reverse())
        {
          Element2D dependantAtPoint = element2D.GetSelfOrDependantAtPoint(x, y);
          if (dependantAtPoint != null && dependantAtPoint.Enabled && !dependantAtPoint.IgnoreMouse)
          {
            return dependantAtPoint;
          }
        }
        if (GetElementType() != ElementType.Frame)
        {
          return this;
        }
      }
      return null;
    }

    public void GetNextTabIndexElement(ref List<Element2D> tabIndexElements)
    {
      if (!Visible || !Enabled)
      {
        return;
      }

      foreach (Element2D child in ChildList)
      {
        if (child.Visible && child.Enabled)
        {
          child.GetNextTabIndexElement(ref tabIndexElements);
        }
      }
      if (GetElementType() == ElementType.Frame || GetElementType() == ElementType.ScrollFrame || tabIndex <= -1)
      {
        return;
      }

      tabIndexElements.Add(this);
    }

    public virtual void OnParentMove()
    {
      SetPosition(X, Y);
      if (DoOnParentMove == null)
      {
        return;
      }

      DoOnParentMove();
    }

    public virtual void OnRender(GUIHost host)
    {
      OnRender();
      foreach (Element2D child in ChildList)
      {
        if (child.Visible && dropdownelement != child && (!child.HasFocus || !child.FocusedAlwaysOnTop))
        {
          child.OnRender(host);
        }
      }
      if (dropdownelement == null)
      {
        return;
      }

      dropdownelement.OnRender(host);
    }

    public override void OnUpdate()
    {
      foreach (Element2D child in ChildList)
      {
        if ((child.Visible || child.bUpdateWhenNotVisible) && (child.Enabled || IElement.HasEnabledOnlyItem && child.HasChildren))
        {
          child.OnUpdate();
        }
      }
      base.OnUpdate();
    }

    public void AddFirstChild(Element2D child)
    {
      child.SetParent(this);
      ChildList.Insert(0, child);
    }

    public virtual void AddChildElement(Element2D child)
    {
      ChildList.Add(child);
    }

    public virtual void RemoveChildElement(Element2D child)
    {
      ChildList.Remove(child);
    }

    public virtual void SetPosition(int x, int y)
    {
      absolutelocation.x = location.x = x;
      absolutelocation.y = location.y = y;
      if (Parent != null)
      {
        if (location.x < 0 && WrapOnNegativeX)
        {
          absolutelocation.x = Parent.Width + x;
        }

        if (location.y < 0 && WrapOnNegativeY)
        {
          absolutelocation.y = Parent.Height + y;
        }

        absolutelocation.x += Parent.X_Abs;
        absolutelocation.y += Parent.Y_Abs;
      }
      foreach (Element2D child in ChildList)
      {
        child.OnParentMove();
      }
    }

    public void SetPositionRelative(float x, float y)
    {
      RelativeX = x;
      RelativeY = y;
    }

    public virtual void SetSize(int width, int height)
    {
      if (MinWidth > 0 && width < MinWidth)
      {
        width = MinWidth;
      }

      if (MaxWidth > 0 && width > MaxWidth)
      {
        width = MaxWidth;
      }

      if (MinHeight > 0 && height < MinHeight)
      {
        height = MinHeight;
      }

      if (MaxHeight > 0 && height > MaxHeight)
      {
        height = MaxHeight;
      }

      size.width = width;
      size.height = height;
      foreach (Element2D child in ChildList)
      {
        child.OnParentResize();
      }
    }

    public void SetSizeRelative(float width, float height)
    {
      RelativeWidth = width;
      RelativeHeight = height;
    }

    public virtual void SetActive(bool bActive)
    {
      _bActive = bActive;
    }

    public void SetAsFocused(GUIHost host)
    {
      host.SetFocus(this);
    }

    public virtual void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      SetParent(parent);
      if (HasFocus)
      {
        SetAsFocused(host);
      }

      foreach (Element2D child in ChildList)
      {
        child.SetParent(this);
        child.InitChildren(this, host, MyButtonCallback);
      }
      Refresh();
    }

    public void SetParent(Element2D parent)
    {
      this.parent = parent;
      IParent = parent;
      SetBaseParent(parent);
      if (parent == null)
      {
        return;
      }

      if (relative_x > 0.0)
      {
        RelativeX = RelativeX;
      }

      if (relative_y > 0.0)
      {
        RelativeY = RelativeY;
      }

      if (relative_width > 0.0)
      {
        RelativeWidth = RelativeWidth;
      }

      if (relative_height <= 0.0)
      {
        return;
      }

      RelativeHeight = RelativeHeight;
    }

    [XmlIgnore]
    public int X_Abs
    {
      get
      {
        return absolutelocation.x;
      }
    }

    [XmlIgnore]
    public int Y_Abs
    {
      get
      {
        return absolutelocation.y;
      }
    }

    [XmlAttribute("x")]
    public int X
    {
      get
      {
        return location.x;
      }
      set
      {
        SetPosition(value, location.y);
      }
    }

    [XmlAttribute("y")]
    public int Y
    {
      get
      {
        return location.y;
      }
      set
      {
        SetPosition(location.x, value);
      }
    }

    [XmlAttribute("width")]
    public int Width
    {
      get
      {
        return size.width;
      }
      set
      {
        SetSize(value, size.height);
      }
    }

    [XmlAttribute("height")]
    public int Height
    {
      get
      {
        return size.height;
      }
      set
      {
        SetSize(size.width, value);
      }
    }

    [XmlAttribute("active")]
    public bool Active
    {
      get
      {
        return _bActive;
      }
      set
      {
        SetActive(value);
      }
    }

    [XmlIgnore]
    public Element2D Parent
    {
      get
      {
        return parent;
      }
      set
      {
        SetParent(value);
      }
    }

    [XmlAttribute("use-relative-positions")]
    public bool UseRelativePositions
    {
      get
      {
        return relativeposition;
      }
      set
      {
        relativeposition = value;
      }
    }

    public void ForceAbsolute()
    {
      relative_width = -1f;
      relative_height = -1f;
      relativeposition = false;
    }

    [XmlAttribute("relative-x")]
    public float RelativeX
    {
      get
      {
        return relative_x;
      }
      set
      {
        UseRelativePositions = true;
        relative_x = value;
        if (Parent == null)
        {
          return;
        }

        X = CalculateXFromRelative();
      }
    }

    [XmlAttribute("relative-y")]
    public float RelativeY
    {
      get
      {
        return relative_y;
      }
      set
      {
        UseRelativePositions = true;
        relative_y = value;
        if (Parent == null)
        {
          return;
        }

        Y = CalculateYFromRelative();
      }
    }

    [XmlAttribute("relative-x-adj")]
    public int RelativeXAdj
    {
      get
      {
        return relative_x_adj;
      }
      set
      {
        relative_x_adj = value;
        if (!UseRelativePositions || Parent == null)
        {
          return;
        }

        X = CalculateXFromRelative();
      }
    }

    [XmlAttribute("relative-y-adj")]
    public int RelativeYAdj
    {
      get
      {
        return relative_y_adj;
      }
      set
      {
        relative_y_adj = value;
        if (!UseRelativePositions || Parent == null)
        {
          return;
        }

        Y = CalculateYFromRelative();
      }
    }

    [XmlAttribute("auto-center-x-offset")]
    public int AutoCenterXOffset
    {
      get
      {
        return auto_center_x_offset;
      }
      set
      {
        auto_center_x_offset = value;
      }
    }

    [XmlAttribute("auto-center-y-offset")]
    public int AutoCenterYOffset
    {
      get
      {
        return auto_center_y_offset;
      }
      set
      {
        auto_center_y_offset = value;
      }
    }

    [XmlAttribute("relative-width")]
    public float RelativeWidth
    {
      get
      {
        return relative_width;
      }
      set
      {
        relative_width = value;
        if (Parent == null || relative_width == -1.0)
        {
          return;
        }

        Width = CalculateWidthFromRelative();
      }
    }

    [XmlAttribute("relative-height")]
    public float RelativeHeight
    {
      get
      {
        return relative_height;
      }
      set
      {
        relative_height = value;
        if (Parent == null || relative_height == -1.0)
        {
          return;
        }

        Height = CalculateHeightFromRelative();
      }
    }

    [XmlAttribute("relative-width-adj")]
    public int RelativeWidthAdj
    {
      get
      {
        return relative_width_adj;
      }
      set
      {
        relative_width_adj = value;
        if (Parent == null || relative_width == -1.0)
        {
          return;
        }

        Width = CalculateWidthFromRelative();
      }
    }

    [XmlAttribute("relative-height-adj")]
    public int RelativeHeightAdj
    {
      get
      {
        return relative_height_adj;
      }
      set
      {
        relative_height_adj = value;
        if (Parent == null || relative_height == -1.0)
        {
          return;
        }

        Height = CalculateHeightFromRelative();
      }
    }

    [XmlAttribute("has_focus")]
    public virtual bool HasFocus
    {
      get
      {
        return hasfocus;
      }
      set
      {
        hasfocus = value;
        OnFocusChanged();
      }
    }

    [XmlIgnore]
    public object Data
    {
      get
      {
        return data;
      }
      set
      {
        data = value;
      }
    }

    [XmlAttribute("min_width")]
    public int MinWidth
    {
      get
      {
        return __MinWidth;
      }
      set
      {
        __MinWidth = value;
        if (__MinWidth <= 0 || Width >= __MinWidth)
        {
          return;
        }

        SetSize(__MinWidth, Height);
      }
    }

    [XmlAttribute("min_height")]
    public int MinHeight
    {
      get
      {
        return __MinHeight;
      }
      set
      {
        __MinHeight = value;
        if (__MinHeight <= 0 || Height >= __MinHeight)
        {
          return;
        }

        SetSize(Width, __MinHeight);
      }
    }

    [XmlAttribute("max_width")]
    public int MaxWidth
    {
      get
      {
        return __MaxWidth;
      }
      set
      {
        __MaxWidth = value;
        if (__MaxWidth <= 0 || Width <= __MaxWidth)
        {
          return;
        }

        SetSize(__MaxWidth, Height);
      }
    }

    [XmlAttribute("max_height")]
    public int MaxHeight
    {
      get
      {
        return __MaxHeight;
      }
      set
      {
        __MaxHeight = value;
        if (__MaxHeight <= 0 || Height <= __MaxHeight)
        {
          return;
        }

        SetSize(Width, __MaxHeight);
      }
    }

    [XmlAttribute("wrap_negative_x")]
    public bool WrapOnNegativeX
    {
      get
      {
        return wraponnegativeX;
      }
      set
      {
        wraponnegativeX = value;
      }
    }

    [XmlAttribute("wrap_negative_y")]
    public bool WrapOnNegativeY
    {
      get
      {
        return wraponnegativeY;
      }
      set
      {
        wraponnegativeY = value;
      }
    }

    [XmlAttribute("wrap_negative")]
    public bool WrapOnNegative
    {
      set
      {
        wraponnegativeY = value;
        wraponnegativeX = value;
      }
    }

    [XmlAttribute("ignore_mouse")]
    public bool IgnoreMouse
    {
      set
      {
        ignoremouse = value;
      }
      get
      {
        return ignoremouse;
      }
    }

    [XmlIgnore]
    public bool HasChildren
    {
      get
      {
        return ChildList.Count > 0;
      }
    }

    [XmlIgnore]
    public Element2D DropdownElement
    {
      set
      {
        dropdownelement = value;
      }
      get
      {
        return dropdownelement;
      }
    }

    [XmlIgnore]
    public virtual bool FocusedAlwaysOnTop
    {
      get
      {
        return false;
      }
    }
  }
}
