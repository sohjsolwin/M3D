// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Widgets2D.ComboBoxWidget
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace M3D.Graphics.Widgets2D
{
  public class ComboBoxWidget : Element2D, IDataAccessable
  {
    [XmlIgnore]
    public ComboBoxWidget.ComboBoxTextChangedCallback TextChangedCallback;
    private int dropdown_height;
    private string hexColor;
    private FontSize fontSize;
    private Type itemsEnum;
    private string itemsEnumString;
    private ListBoxWidget listboxwidget;
    private ButtonWidget buttonwidget;
    private EditBoxWidget editboxwidget;
    private GUIHost host;

    public ComboBoxWidget()
      : this(0, (Element2D) null)
    {
      this.dropdown_height = 96;
    }

    public ComboBoxWidget(int ID)
      : this(ID, (Element2D) null)
    {
      this.dropdown_height = 96;
    }

    public ComboBoxWidget(int ID, Element2D parent)
      : base(ID, parent)
    {
      this.dropdown_height = 96;
      this.listboxwidget = new ListBoxWidget(2);
      this.buttonwidget = new ButtonWidget(1);
      this.editboxwidget = new EditBoxWidget(0);
      this.editboxwidget.SetCallbackOnClick(new EditBoxWidget.EditBoxCallback(this.MyEditBoxCallback));
      this.buttonwidget.DontMove = true;
      this.buttonwidget.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.listboxwidget.Visible = false;
      this.editboxwidget.Enabled = true;
      this.editboxwidget.Text = "";
      this.ShowDropDown = false;
      this.ChildList = this.ChildList + (Element2D) this.listboxwidget;
      this.ChildList = this.ChildList + (Element2D) this.buttonwidget;
      this.ChildList = this.ChildList + (Element2D) this.editboxwidget;
    }

    public override bool HasFocus
    {
      get
      {
        return base.HasFocus;
      }
      set
      {
        base.HasFocus = value;
        if (!this.HasFocus || this.host == null)
          return;
        this.host.SetFocus((Element2D) this);
      }
    }

    public override void OnFocusChanged()
    {
      if (this.HasFocus)
        return;
      this.ShowDropDown = false;
    }

    public override bool OnKeyboardEvent(KeyboardEvent keyboardevent)
    {
      if (!this.Enabled || !this.HasFocus)
        return false;
      if (this.ShowDropDown)
        this.listboxwidget.OnKeyboardEvent(keyboardevent);
      if (keyboardevent.type == KeyboardEventType.CommandKey && ((CommandKeyEvent) keyboardevent).Key == KeyboardCommandKey.Down && !this.ShowDropDown)
        this.ShowDropDown = true;
      return true;
    }

    public string Text
    {
      get
      {
        if (this.ListBox.Selected >= 0)
          return this.ListBox.Items[this.ListBox.Selected].ToString();
        return (string) null;
      }
    }

    public void Init(GUIHost host)
    {
      Sprite.pixel_perfect = false;
      this.host = host;
      this.listboxwidget.Init(host, "guicontrols", 944f, 96f, 960f, 144f, 944f, 96f, 960f, 144f, 4, 4, 16, 4, 4, 48, 24, 24);
      this.editboxwidget.Init(host, "guicontrols", 944f, 96f, 960f, 144f);
      this.editboxwidget.SetTextWindowBorders(10, 10, 3, 3);
      this.editboxwidget.SetGrowableWidth(3, 3, 12);
      this.buttonwidget.Init(host, "guicontrols", 928f, 48f, 951f, 71f, 952f, 48f, 975f, 71f, 976f, 48f, 999f, 71f);
      this.listboxwidget.ScrollBar.InitTrack(host, "guicontrols", 809f, 80f, 831f, 87f, 2, 8);
      this.listboxwidget.ScrollBar.InitButton(host, "guicontrols", 1000f, 0.0f, 1023f, 23f, 1000f, 24f, 1023f, 47f, 1000f, 48f, 1023f, 71f, 4, 4, 24);
      this.listboxwidget.ScrollBar.InitMinus(host, "guicontrols", 928f, 48f, 951f, 71f, 952f, 48f, 975f, 71f, 976f, 48f, 999f, 71f);
      this.listboxwidget.ScrollBar.InitPlus(host, "guicontrols", 928f, 72f, 951f, 95f, 952f, 72f, 975f, 95f, 976f, 72f, 999f, 95f);
      this.listboxwidget.ScrollBar.SetButtonSize(24f);
      this.listboxwidget.ScrollBar.ShowPushButtons = true;
      this.listboxwidget.ShowScrollbar = ListBoxWidget.ScrollBarState.On;
      this.editboxwidget.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      this.listboxwidget.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      this.Parent = parent;
      this.Init(host);
      lock (this.ChildList)
      {
        foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
          child.InitChildren((Element2D) this, host, MyButtonCallback);
      }
    }

    public override ElementType GetElementType()
    {
      return ElementType.ComboBoxWidget;
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 0:
        case 1:
          this.ShowDropDown = !this.listboxwidget.Visible;
          break;
      }
    }

    public void MyEditBoxCallback(EditBoxWidget edit)
    {
      if (edit.ID != 0)
        return;
      this.ShowDropDown = !this.listboxwidget.Visible;
    }

    public override void SetSize(int width, int height)
    {
      base.SetSize(width, height);
      this.Refresh();
    }

    public override void Refresh()
    {
      int num = this.Height - (this.ShowDropDown ? this.dropdown_height : 0);
      this.editboxwidget.SetPosition(0, 0);
      this.editboxwidget.SetSize(this.Width - num, num);
      this.buttonwidget.SetPosition(this.editboxwidget.Width, 0);
      this.buttonwidget.SetSize(num, num);
      this.listboxwidget.SetPosition(0, num);
      this.listboxwidget.SetSize(this.Width, this.dropdown_height);
      this.listboxwidget.Refresh();
    }

    public void AddItem(object new_item)
    {
      this.listboxwidget.Items.Add(new_item);
      this.Refresh();
    }

    public int GetItemIndex(string item)
    {
      for (int index = 0; index < this.listboxwidget.Items.Count; ++index)
      {
        if (item == this.listboxwidget.Items[index].ToString())
          return index;
      }
      return -1;
    }

    public void ClearItems()
    {
      this.listboxwidget.Items.Clear();
      this.editboxwidget.Text = "";
      this.Refresh();
    }

    [XmlIgnore]
    public object Value
    {
      set
      {
        if (value is int)
        {
          this.Select = (int) value;
        }
        else
        {
          if (!(value is string))
            return;
          int result = -1;
          if (int.TryParse((string) value, out result))
          {
            this.Select = result;
          }
          else
          {
            for (int index = 0; index < this.listboxwidget.Items.Count; ++index)
            {
              if (this.listboxwidget.Items[index].ToString() == (string) value)
                this.Select = index;
            }
          }
        }
      }
      get
      {
        if (this.Select < 0)
          return (object) "";
        return this.listboxwidget.Items[this.Select];
      }
    }

    public override void OnControlMsg(Element2D the_control, ControlMsg msg, float xparam, float yparam)
    {
      if (the_control.GetElementType() == ElementType.ListBoxWidget && msg == ControlMsg.SELECTCHANGED)
      {
        int index = (int) xparam;
        if (index < 0 || index >= this.listboxwidget.Items.Count)
          return;
        this.editboxwidget.Text = this.listboxwidget.Items[index].ToString();
        this.ShowDropDown = false;
      }
      else if (the_control.GetElementType() == ElementType.EditBoxWidget && msg == ControlMsg.TEXT_CHANGED)
      {
        if (this.TextChangedCallback == null)
          return;
        this.TextChangedCallback(this);
      }
      else
        base.OnControlMsg(the_control, msg, xparam, yparam);
    }

    [XmlIgnore]
    public int Select
    {
      set
      {
        if (value >= 0 && value < this.listboxwidget.Items.Count)
        {
          this.listboxwidget.Selected = value;
        }
        else
        {
          if (this.listboxwidget.Items.Count <= 0)
            return;
          this.listboxwidget.Selected = 0;
        }
      }
      get
      {
        return this.listboxwidget.Selected;
      }
    }

    public ListBoxWidget ListBox
    {
      get
      {
        return this.listboxwidget;
      }
      set
      {
        this.listboxwidget = value;
      }
    }

    public ButtonWidget Button
    {
      get
      {
        return this.buttonwidget;
      }
    }

    public EditBoxWidget EditBox
    {
      get
      {
        return this.editboxwidget;
      }
    }

    public bool ShowDropDown
    {
      get
      {
        return this.listboxwidget.Visible;
      }
      set
      {
        if (this.listboxwidget.Visible == value)
          return;
        if (value)
        {
          this.Height += this.dropdown_height;
          this.host.SetFocus((Element2D) this);
        }
        else
          this.Height -= this.dropdown_height;
        this.listboxwidget.Visible = value;
        this.Refresh();
        if (this.Parent == null)
          return;
        if (value)
          this.Parent.DropdownElement = (Element2D) this.listboxwidget;
        else
          this.Parent.DropdownElement = (Element2D) null;
      }
    }

    public int DropdownHeight
    {
      get
      {
        return this.dropdown_height;
      }
      set
      {
        this.dropdown_height = value;
        this.Refresh();
      }
    }

    [XmlAttribute("font-color")]
    public string HexColor
    {
      get
      {
        return this.hexColor;
      }
      set
      {
        this.hexColor = value;
      }
    }

    [XmlAttribute("font-size")]
    public FontSize Size
    {
      get
      {
        return this.fontSize;
      }
      set
      {
        this.fontSize = value;
      }
    }

    [XmlAttribute("items-enum")]
    public string ItemsEnumString
    {
      get
      {
        return this.itemsEnumString;
      }
      set
      {
        this.itemsEnumString = value;
        Type type = Type.GetType(this.itemsEnumString);
        if (!(type != (Type) null) || !type.IsEnum)
          return;
        foreach (string name in Enum.GetNames(type))
          this.listboxwidget.Items.Add((object) name.Replace("__", " "));
        this.itemsEnum = type;
      }
    }

    [XmlIgnore]
    public override bool FocusedAlwaysOnTop
    {
      get
      {
        return true;
      }
    }

    private enum ComboBoxControlID
    {
      edit_widget,
      button_widget,
      listbox_widget,
    }

    public delegate void ComboBoxTextChangedCallback(ComboBoxWidget combobox);
  }
}
