// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Widgets2D.Element2D
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      : this(0, (Element2D) null)
    {
    }

    public Element2D(int ID)
      : this(ID, (Element2D) null)
    {
    }

    public Element2D(int ID, Element2D parent)
      : base(ID, (IElement) parent)
    {
      this.ChildList = new Element2DList(this);
      this.CenterHorizontallyInParent = false;
      this.CenterVerticallyInParent = false;
      this.parent = parent;
      this._bActive = false;
      this.wraponnegativeX = true;
      this.wraponnegativeY = true;
      this.location.x = 0;
      this.location.y = 0;
      this.size.width = 0;
      this.size.height = 0;
      this.__MinWidth = 0;
      this.__MinHeight = 0;
      this.__MaxWidth = 0;
      this.__MaxHeight = 0;
      this.auto_center_x_offset = 0;
      this.auto_center_y_offset = 0;
      this.IgnoreMouse = false;
      this.hasfocus = false;
      this.relativeposition = false;
      this.dropdownelement = (Element2D) null;
    }

    public virtual void OnControlMsg(Element2D the_control, ControlMsg msg, float xparam, float yparam)
    {
      if (this.OnControlMsgCallback != null)
      {
        this.OnControlMsgCallback(the_control, msg, xparam, yparam);
      }
      else
      {
        if (this.parent == null)
          return;
        this.parent.OnControlMsg(the_control, msg, xparam, yparam);
      }
    }

    public void TurnOffGroup(int group, Element2D except)
    {
      if (group == -1)
        return;
      foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
      {
        if (child.GroupID == group && child != except)
          child.SetOff();
      }
    }

    public void DisableGroup(int groupID)
    {
      if (this.GroupID == groupID)
        this.Enabled = false;
      if (groupID == -1)
        return;
      foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
        child.DisableGroup(groupID);
    }

    public void EnableGroup(int groupID)
    {
      if (this.GroupID == groupID)
        this.Enabled = true;
      if (groupID == -1)
        return;
      foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
        child.EnableGroup(groupID);
    }

    public override void Refresh()
    {
      this.OnParentResize();
    }

    public virtual void RemoveAllChildElements()
    {
      this.ChildList.Clear();
    }

    public virtual void RemoveChildElementAt(int index)
    {
      this.ChildList.RemoveAt(index);
    }

    public bool IsComboBoxElement()
    {
      if (this.GetElementType() == ElementType.ComboBoxWidget)
        return true;
      for (Element2D parent = this.Parent; parent != null; parent = parent.Parent)
      {
        if (parent.GetElementType() == ElementType.ComboBoxWidget)
          return true;
      }
      return false;
    }

    public bool IsListBoxElement()
    {
      if (this.GetElementType() == ElementType.ListBoxWidget)
        return true;
      for (Element2D parent = this.Parent; parent != null; parent = parent.Parent)
      {
        if (parent.GetElementType() == ElementType.ListBoxWidget)
          return true;
      }
      return false;
    }

    public Frame FindParentFrame()
    {
      if (this.IsScrollFrame())
        return (Frame) this;
      if (this.Parent != null)
        return this.Parent.FindParentFrame();
      return (Frame) null;
    }

    public ComboBoxWidget GetComboBoxElement()
    {
      if (this.GetElementType() == ElementType.ComboBoxWidget)
        return (ComboBoxWidget) this;
      for (Element2D parent = this.Parent; parent != null; parent = parent.Parent)
      {
        if (parent.GetElementType() == ElementType.ComboBoxWidget)
          return (ComboBoxWidget) parent;
      }
      return (ComboBoxWidget) null;
    }

    public ListBoxWidget GetListBoxElement()
    {
      if (this.GetElementType() == ElementType.ListBoxWidget)
        return (ListBoxWidget) this;
      for (Element2D parent = this.Parent; parent != null; parent = parent.Parent)
      {
        if (parent.GetElementType() == ElementType.ListBoxWidget)
          return (ListBoxWidget) parent;
      }
      return (ListBoxWidget) null;
    }

    public bool IsScrollFrame()
    {
      if (this.GetElementType() == ElementType.ScrollFrame)
        return true;
      for (Element2D parent = this.Parent; parent != null; parent = parent.Parent)
      {
        if (parent.GetElementType() == ElementType.ScrollFrame)
          return true;
      }
      return false;
    }

    public Frame GetScrollFrame()
    {
      if (this.GetElementType() == ElementType.ScrollFrame)
        return (Frame) this;
      for (Element2D parent = this.Parent; parent != null; parent = parent.Parent)
      {
        if (parent.GetElementType() == ElementType.ScrollFrame)
          return (Frame) parent;
      }
      return (Frame) null;
    }

    public Element2D FindChildElement(int ID)
    {
      foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
      {
        if (child.ID == ID)
          return child;
        Element2D childElement = child.FindChildElement(ID);
        if (childElement != null)
          return childElement;
      }
      return (Element2D) null;
    }

    public Element2D FindChildElement(string tag)
    {
      foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
      {
        if (child.tag == tag)
          return child;
        Element2D childElement = child.FindChildElement(tag);
        if (childElement != null)
          return childElement;
      }
      return (Element2D) null;
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
      this.RemoveAllChildElements();
    }

    private int CalculateXFromRelative()
    {
      int num = (int) ((double) this.Parent.Width * (double) this.relative_x) + this.relative_x_adj;
      if (num < 0)
        num = 0;
      return num;
    }

    private int CalculateYFromRelative()
    {
      int num = (int) ((double) this.Parent.Height * (double) this.relative_y) + this.relative_y_adj;
      if (num < 0)
        num = 0;
      return num;
    }

    private int CalculateWidthFromRelative()
    {
      return (int) ((double) this.Parent.Width * (double) this.relative_width) + this.relative_width_adj;
    }

    private int CalculateHeightFromRelative()
    {
      return (int) ((double) this.Parent.Height * (double) this.relative_height) + this.relative_height_adj;
    }

    public virtual void OnParentResize()
    {
      if (this.Parent == null)
        return;
      if ((double) this.relative_width > 0.0 || (double) this.relative_height > 0.0)
      {
        int width = this.Width;
        int height = this.Height;
        if ((double) this.relative_width > 0.0)
          width = this.CalculateWidthFromRelative();
        if ((double) this.relative_height > 0.0)
          height = this.CalculateHeightFromRelative();
        this.SetSize(width, height);
      }
      int x = this.X;
      int y = this.Y;
      if (this.CenterHorizontallyInParent || this.CenterVerticallyInParent)
      {
        if (this.CenterHorizontallyInParent)
        {
          x = (this.Parent.Width - this.Width - this.auto_center_x_offset) / 2 + this.auto_center_x_offset;
          if (x < 0)
            x = 0;
        }
        if (this.CenterVerticallyInParent)
        {
          y = (this.Parent.Height - this.Height - this.auto_center_y_offset) / 2 + this.auto_center_y_offset;
          if (y < 0)
            y = 0;
        }
      }
      if (this.UseRelativePositions)
      {
        if ((double) this.relative_y > 0.0)
          y = this.CalculateYFromRelative();
        if ((double) this.relative_x > 0.0)
          x = this.CalculateXFromRelative();
      }
      this.SetPosition(x, y);
      if (this.DoOnParentResize != null)
        this.DoOnParentResize();
      foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
        child.OnParentResize();
    }

    public virtual void OnFocusChanged()
    {
    }

    public virtual bool OnKeyboardEvent(KeyboardEvent keyboardevent)
    {
      foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
      {
        if (child.OnKeyboardEvent(keyboardevent))
          return true;
      }
      return false;
    }

    public virtual void OnMouseLeave()
    {
      foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
        child.OnMouseLeave();
    }

    public virtual bool OnMouseCommand(MouseEvent mouseevent)
    {
      if (this.dropdownelement != null)
        return this.dropdownelement.OnMouseCommand(mouseevent);
      return false;
    }

    public virtual void OnMouseMove(int x, int y)
    {
      foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
      {
        if (child.Enabled || IElement.HasEnabledOnlyItem && child.HasChildren)
          child.OnMouseMove(x, y);
      }
    }

    public bool Overlaps(Element2D other)
    {
      return this.Overlaps(other, 0, 0);
    }

    public bool Overlaps(Element2D other, int x_off, int y_off)
    {
      return this.X_Abs + this.Width + x_off >= other.X_Abs && this.X_Abs + x_off <= other.X_Abs + other.Width && (this.Y_Abs + this.Height + y_off >= other.Y_Abs && this.Y_Abs + y_off <= other.Y_Abs + other.Height);
    }

    public virtual bool ContainsPoint(int x, int y)
    {
      return x >= this.absolutelocation.x && y >= this.absolutelocation.y && (x <= this.absolutelocation.x + this.size.width && y <= this.absolutelocation.y + this.size.height);
    }

    public virtual Element2D GetSelfOrDependantAtPoint(int x, int y)
    {
      if (this.ContainsPoint(x, y) && this.Visible && (this.Enabled || this.ChildList.Count > 0))
      {
        foreach (Element2D element2D in (IEnumerable<Element2D>) this.ChildList.Reverse())
        {
          Element2D dependantAtPoint = element2D.GetSelfOrDependantAtPoint(x, y);
          if (dependantAtPoint != null && dependantAtPoint.Enabled && !dependantAtPoint.IgnoreMouse)
            return dependantAtPoint;
        }
        if (this.GetElementType() != ElementType.Frame)
          return this;
      }
      return (Element2D) null;
    }

    public void GetNextTabIndexElement(ref List<Element2D> tabIndexElements)
    {
      if (!this.Visible || !this.Enabled)
        return;
      foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
      {
        if (child.Visible && child.Enabled)
          child.GetNextTabIndexElement(ref tabIndexElements);
      }
      if (this.GetElementType() == ElementType.Frame || this.GetElementType() == ElementType.ScrollFrame || this.tabIndex <= -1)
        return;
      tabIndexElements.Add(this);
    }

    public virtual void OnParentMove()
    {
      this.SetPosition(this.X, this.Y);
      if (this.DoOnParentMove == null)
        return;
      this.DoOnParentMove();
    }

    public virtual void OnRender(GUIHost host)
    {
      this.OnRender();
      foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
      {
        if (child.Visible && this.dropdownelement != child && (!child.HasFocus || !child.FocusedAlwaysOnTop))
          child.OnRender(host);
      }
      if (this.dropdownelement == null)
        return;
      this.dropdownelement.OnRender(host);
    }

    public override void OnUpdate()
    {
      foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
      {
        if ((child.Visible || child.bUpdateWhenNotVisible) && (child.Enabled || IElement.HasEnabledOnlyItem && child.HasChildren))
          child.OnUpdate();
      }
      base.OnUpdate();
    }

    public void AddFirstChild(Element2D child)
    {
      child.SetParent(this);
      this.ChildList.Insert(0, child);
    }

    public virtual void AddChildElement(Element2D child)
    {
      this.ChildList.Add(child);
    }

    public virtual void RemoveChildElement(Element2D child)
    {
      this.ChildList.Remove(child);
    }

    public virtual void SetPosition(int x, int y)
    {
      this.absolutelocation.x = this.location.x = x;
      this.absolutelocation.y = this.location.y = y;
      if (this.Parent != null)
      {
        if (this.location.x < 0 && this.WrapOnNegativeX)
          this.absolutelocation.x = this.Parent.Width + x;
        if (this.location.y < 0 && this.WrapOnNegativeY)
          this.absolutelocation.y = this.Parent.Height + y;
        this.absolutelocation.x += this.Parent.X_Abs;
        this.absolutelocation.y += this.Parent.Y_Abs;
      }
      foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
        child.OnParentMove();
    }

    public void SetPositionRelative(float x, float y)
    {
      this.RelativeX = x;
      this.RelativeY = y;
    }

    public virtual void SetSize(int width, int height)
    {
      if (this.MinWidth > 0 && width < this.MinWidth)
        width = this.MinWidth;
      if (this.MaxWidth > 0 && width > this.MaxWidth)
        width = this.MaxWidth;
      if (this.MinHeight > 0 && height < this.MinHeight)
        height = this.MinHeight;
      if (this.MaxHeight > 0 && height > this.MaxHeight)
        height = this.MaxHeight;
      this.size.width = width;
      this.size.height = height;
      foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
        child.OnParentResize();
    }

    public void SetSizeRelative(float width, float height)
    {
      this.RelativeWidth = width;
      this.RelativeHeight = height;
    }

    public virtual void SetActive(bool bActive)
    {
      this._bActive = bActive;
    }

    public void SetAsFocused(GUIHost host)
    {
      host.SetFocus(this);
    }

    public virtual void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      this.SetParent(parent);
      if (this.HasFocus)
        this.SetAsFocused(host);
      foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
      {
        child.SetParent(this);
        child.InitChildren(this, host, MyButtonCallback);
      }
      this.Refresh();
    }

    public void SetParent(Element2D parent)
    {
      this.parent = parent;
      this.IParent = (IElement) parent;
      this.SetBaseParent((IElement) parent);
      if (parent == null)
        return;
      if ((double) this.relative_x > 0.0)
        this.RelativeX = this.RelativeX;
      if ((double) this.relative_y > 0.0)
        this.RelativeY = this.RelativeY;
      if ((double) this.relative_width > 0.0)
        this.RelativeWidth = this.RelativeWidth;
      if ((double) this.relative_height <= 0.0)
        return;
      this.RelativeHeight = this.RelativeHeight;
    }

    [XmlIgnore]
    public int X_Abs
    {
      get
      {
        return this.absolutelocation.x;
      }
    }

    [XmlIgnore]
    public int Y_Abs
    {
      get
      {
        return this.absolutelocation.y;
      }
    }

    [XmlAttribute("x")]
    public int X
    {
      get
      {
        return this.location.x;
      }
      set
      {
        this.SetPosition(value, this.location.y);
      }
    }

    [XmlAttribute("y")]
    public int Y
    {
      get
      {
        return this.location.y;
      }
      set
      {
        this.SetPosition(this.location.x, value);
      }
    }

    [XmlAttribute("width")]
    public int Width
    {
      get
      {
        return this.size.width;
      }
      set
      {
        this.SetSize(value, this.size.height);
      }
    }

    [XmlAttribute("height")]
    public int Height
    {
      get
      {
        return this.size.height;
      }
      set
      {
        this.SetSize(this.size.width, value);
      }
    }

    [XmlAttribute("active")]
    public bool Active
    {
      get
      {
        return this._bActive;
      }
      set
      {
        this.SetActive(value);
      }
    }

    [XmlIgnore]
    public Element2D Parent
    {
      get
      {
        return this.parent;
      }
      set
      {
        this.SetParent(value);
      }
    }

    [XmlAttribute("use-relative-positions")]
    public bool UseRelativePositions
    {
      get
      {
        return this.relativeposition;
      }
      set
      {
        this.relativeposition = value;
      }
    }

    public void ForceAbsolute()
    {
      this.relative_width = -1f;
      this.relative_height = -1f;
      this.relativeposition = false;
    }

    [XmlAttribute("relative-x")]
    public float RelativeX
    {
      get
      {
        return this.relative_x;
      }
      set
      {
        this.UseRelativePositions = true;
        this.relative_x = value;
        if (this.Parent == null)
          return;
        this.X = this.CalculateXFromRelative();
      }
    }

    [XmlAttribute("relative-y")]
    public float RelativeY
    {
      get
      {
        return this.relative_y;
      }
      set
      {
        this.UseRelativePositions = true;
        this.relative_y = value;
        if (this.Parent == null)
          return;
        this.Y = this.CalculateYFromRelative();
      }
    }

    [XmlAttribute("relative-x-adj")]
    public int RelativeXAdj
    {
      get
      {
        return this.relative_x_adj;
      }
      set
      {
        this.relative_x_adj = value;
        if (!this.UseRelativePositions || this.Parent == null)
          return;
        this.X = this.CalculateXFromRelative();
      }
    }

    [XmlAttribute("relative-y-adj")]
    public int RelativeYAdj
    {
      get
      {
        return this.relative_y_adj;
      }
      set
      {
        this.relative_y_adj = value;
        if (!this.UseRelativePositions || this.Parent == null)
          return;
        this.Y = this.CalculateYFromRelative();
      }
    }

    [XmlAttribute("auto-center-x-offset")]
    public int AutoCenterXOffset
    {
      get
      {
        return this.auto_center_x_offset;
      }
      set
      {
        this.auto_center_x_offset = value;
      }
    }

    [XmlAttribute("auto-center-y-offset")]
    public int AutoCenterYOffset
    {
      get
      {
        return this.auto_center_y_offset;
      }
      set
      {
        this.auto_center_y_offset = value;
      }
    }

    [XmlAttribute("relative-width")]
    public float RelativeWidth
    {
      get
      {
        return this.relative_width;
      }
      set
      {
        this.relative_width = value;
        if (this.Parent == null || (double) this.relative_width == -1.0)
          return;
        this.Width = this.CalculateWidthFromRelative();
      }
    }

    [XmlAttribute("relative-height")]
    public float RelativeHeight
    {
      get
      {
        return this.relative_height;
      }
      set
      {
        this.relative_height = value;
        if (this.Parent == null || (double) this.relative_height == -1.0)
          return;
        this.Height = this.CalculateHeightFromRelative();
      }
    }

    [XmlAttribute("relative-width-adj")]
    public int RelativeWidthAdj
    {
      get
      {
        return this.relative_width_adj;
      }
      set
      {
        this.relative_width_adj = value;
        if (this.Parent == null || (double) this.relative_width == -1.0)
          return;
        this.Width = this.CalculateWidthFromRelative();
      }
    }

    [XmlAttribute("relative-height-adj")]
    public int RelativeHeightAdj
    {
      get
      {
        return this.relative_height_adj;
      }
      set
      {
        this.relative_height_adj = value;
        if (this.Parent == null || (double) this.relative_height == -1.0)
          return;
        this.Height = this.CalculateHeightFromRelative();
      }
    }

    [XmlAttribute("has_focus")]
    public virtual bool HasFocus
    {
      get
      {
        return this.hasfocus;
      }
      set
      {
        this.hasfocus = value;
        this.OnFocusChanged();
      }
    }

    [XmlIgnore]
    public object Data
    {
      get
      {
        return this.data;
      }
      set
      {
        this.data = value;
      }
    }

    [XmlAttribute("min_width")]
    public int MinWidth
    {
      get
      {
        return this.__MinWidth;
      }
      set
      {
        this.__MinWidth = value;
        if (this.__MinWidth <= 0 || this.Width >= this.__MinWidth)
          return;
        this.SetSize(this.__MinWidth, this.Height);
      }
    }

    [XmlAttribute("min_height")]
    public int MinHeight
    {
      get
      {
        return this.__MinHeight;
      }
      set
      {
        this.__MinHeight = value;
        if (this.__MinHeight <= 0 || this.Height >= this.__MinHeight)
          return;
        this.SetSize(this.Width, this.__MinHeight);
      }
    }

    [XmlAttribute("max_width")]
    public int MaxWidth
    {
      get
      {
        return this.__MaxWidth;
      }
      set
      {
        this.__MaxWidth = value;
        if (this.__MaxWidth <= 0 || this.Width <= this.__MaxWidth)
          return;
        this.SetSize(this.__MaxWidth, this.Height);
      }
    }

    [XmlAttribute("max_height")]
    public int MaxHeight
    {
      get
      {
        return this.__MaxHeight;
      }
      set
      {
        this.__MaxHeight = value;
        if (this.__MaxHeight <= 0 || this.Height <= this.__MaxHeight)
          return;
        this.SetSize(this.Width, this.__MaxHeight);
      }
    }

    [XmlAttribute("wrap_negative_x")]
    public bool WrapOnNegativeX
    {
      get
      {
        return this.wraponnegativeX;
      }
      set
      {
        this.wraponnegativeX = value;
      }
    }

    [XmlAttribute("wrap_negative_y")]
    public bool WrapOnNegativeY
    {
      get
      {
        return this.wraponnegativeY;
      }
      set
      {
        this.wraponnegativeY = value;
      }
    }

    [XmlAttribute("wrap_negative")]
    public bool WrapOnNegative
    {
      set
      {
        this.wraponnegativeY = value;
        this.wraponnegativeX = value;
      }
    }

    [XmlAttribute("ignore_mouse")]
    public bool IgnoreMouse
    {
      set
      {
        this.ignoremouse = value;
      }
      get
      {
        return this.ignoremouse;
      }
    }

    [XmlIgnore]
    public bool HasChildren
    {
      get
      {
        return this.ChildList.Count > 0;
      }
    }

    [XmlIgnore]
    public Element2D DropdownElement
    {
      set
      {
        this.dropdownelement = value;
      }
      get
      {
        return this.dropdownelement;
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
