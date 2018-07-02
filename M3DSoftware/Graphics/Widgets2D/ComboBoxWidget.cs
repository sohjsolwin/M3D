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
      dropdown_height = 96;
    }

    public ComboBoxWidget(int ID)
      : this(ID, (Element2D) null)
    {
      dropdown_height = 96;
    }

    public ComboBoxWidget(int ID, Element2D parent)
      : base(ID, parent)
    {
      dropdown_height = 96;
      listboxwidget = new ListBoxWidget(2);
      buttonwidget = new ButtonWidget(1);
      editboxwidget = new EditBoxWidget(0);
      editboxwidget.SetCallbackOnClick(new EditBoxWidget.EditBoxCallback(MyEditBoxCallback));
      buttonwidget.DontMove = true;
      buttonwidget.SetCallback(new ButtonCallback(MyButtonCallback));
      listboxwidget.Visible = false;
      editboxwidget.Enabled = true;
      editboxwidget.Text = "";
      ShowDropDown = false;
      ChildList = ChildList + (Element2D)listboxwidget;
      ChildList = ChildList + (Element2D)buttonwidget;
      ChildList = ChildList + (Element2D)editboxwidget;
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
        if (!HasFocus || host == null)
        {
          return;
        }

        host.SetFocus((Element2D) this);
      }
    }

    public override void OnFocusChanged()
    {
      if (HasFocus)
      {
        return;
      }

      ShowDropDown = false;
    }

    public override bool OnKeyboardEvent(KeyboardEvent keyboardevent)
    {
      if (!Enabled || !HasFocus)
      {
        return false;
      }

      if (ShowDropDown)
      {
        listboxwidget.OnKeyboardEvent(keyboardevent);
      }

      if (keyboardevent.type == KeyboardEventType.CommandKey && ((CommandKeyEvent) keyboardevent).Key == KeyboardCommandKey.Down && !ShowDropDown)
      {
        ShowDropDown = true;
      }

      return true;
    }

    public string Text
    {
      get
      {
        if (ListBox.Selected >= 0)
        {
          return ListBox.Items[ListBox.Selected].ToString();
        }

        return (string) null;
      }
    }

    public void Init(GUIHost host)
    {
      Sprite.pixel_perfect = false;
      this.host = host;
      listboxwidget.Init(host, "guicontrols", 944f, 96f, 960f, 144f, 944f, 96f, 960f, 144f, 4, 4, 16, 4, 4, 48, 24, 24);
      editboxwidget.Init(host, "guicontrols", 944f, 96f, 960f, 144f);
      editboxwidget.SetTextWindowBorders(10, 10, 3, 3);
      editboxwidget.SetGrowableWidth(3, 3, 12);
      buttonwidget.Init(host, "guicontrols", 928f, 48f, 951f, 71f, 952f, 48f, 975f, 71f, 976f, 48f, 999f, 71f);
      listboxwidget.ScrollBar.InitTrack(host, "guicontrols", 809f, 80f, 831f, 87f, 2, 8);
      listboxwidget.ScrollBar.InitButton(host, "guicontrols", 1000f, 0.0f, 1023f, 23f, 1000f, 24f, 1023f, 47f, 1000f, 48f, 1023f, 71f, 4, 4, 24);
      listboxwidget.ScrollBar.InitMinus(host, "guicontrols", 928f, 48f, 951f, 71f, 952f, 48f, 975f, 71f, 976f, 48f, 999f, 71f);
      listboxwidget.ScrollBar.InitPlus(host, "guicontrols", 928f, 72f, 951f, 95f, 952f, 72f, 975f, 95f, 976f, 72f, 999f, 95f);
      listboxwidget.ScrollBar.SetButtonSize(24f);
      listboxwidget.ScrollBar.ShowPushButtons = true;
      listboxwidget.ShowScrollbar = ListBoxWidget.ScrollBarState.On;
      editboxwidget.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      listboxwidget.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      Parent = parent;
      Init(host);
      lock (ChildList)
      {
        foreach (Element2D child in (IEnumerable<Element2D>)ChildList)
        {
          child.InitChildren((Element2D) this, host, MyButtonCallback);
        }
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
          ShowDropDown = !listboxwidget.Visible;
          break;
      }
    }

    public void MyEditBoxCallback(EditBoxWidget edit)
    {
      if (edit.ID != 0)
      {
        return;
      }

      ShowDropDown = !listboxwidget.Visible;
    }

    public override void SetSize(int width, int height)
    {
      base.SetSize(width, height);
      Refresh();
    }

    public override void Refresh()
    {
      var num = Height - (ShowDropDown ? dropdown_height : 0);
      editboxwidget.SetPosition(0, 0);
      editboxwidget.SetSize(Width - num, num);
      buttonwidget.SetPosition(editboxwidget.Width, 0);
      buttonwidget.SetSize(num, num);
      listboxwidget.SetPosition(0, num);
      listboxwidget.SetSize(Width, dropdown_height);
      listboxwidget.Refresh();
    }

    public void AddItem(object new_item)
    {
      listboxwidget.Items.Add(new_item);
      Refresh();
    }

    public int GetItemIndex(string item)
    {
      for (var index = 0; index < listboxwidget.Items.Count; ++index)
      {
        if (item == listboxwidget.Items[index].ToString())
        {
          return index;
        }
      }
      return -1;
    }

    public void ClearItems()
    {
      listboxwidget.Items.Clear();
      editboxwidget.Text = "";
      Refresh();
    }

    [XmlIgnore]
    public object Value
    {
      set
      {
        if (value is int)
        {
          Select = (int) value;
        }
        else
        {
          if (!(value is string))
          {
            return;
          }

          var result = -1;
          if (int.TryParse((string) value, out result))
          {
            Select = result;
          }
          else
          {
            for (var index = 0; index < listboxwidget.Items.Count; ++index)
            {
              if (listboxwidget.Items[index].ToString() == (string) value)
              {
                Select = index;
              }
            }
          }
        }
      }
      get
      {
        if (Select < 0)
        {
          return (object) "";
        }

        return listboxwidget.Items[Select];
      }
    }

    public override void OnControlMsg(Element2D the_control, ControlMsg msg, float xparam, float yparam)
    {
      if (the_control.GetElementType() == ElementType.ListBoxWidget && msg == ControlMsg.SELECTCHANGED)
      {
        var index = (int) xparam;
        if (index < 0 || index >= listboxwidget.Items.Count)
        {
          return;
        }

        editboxwidget.Text = listboxwidget.Items[index].ToString();
        ShowDropDown = false;
      }
      else if (the_control.GetElementType() == ElementType.EditBoxWidget && msg == ControlMsg.TEXT_CHANGED)
      {
        if (TextChangedCallback == null)
        {
          return;
        }

        TextChangedCallback(this);
      }
      else
      {
        base.OnControlMsg(the_control, msg, xparam, yparam);
      }
    }

    [XmlIgnore]
    public int Select
    {
      set
      {
        if (value >= 0 && value < listboxwidget.Items.Count)
        {
          listboxwidget.Selected = value;
        }
        else
        {
          if (listboxwidget.Items.Count <= 0)
          {
            return;
          }

          listboxwidget.Selected = 0;
        }
      }
      get
      {
        return listboxwidget.Selected;
      }
    }

    public ListBoxWidget ListBox
    {
      get
      {
        return listboxwidget;
      }
      set
      {
        listboxwidget = value;
      }
    }

    public ButtonWidget Button
    {
      get
      {
        return buttonwidget;
      }
    }

    public EditBoxWidget EditBox
    {
      get
      {
        return editboxwidget;
      }
    }

    public bool ShowDropDown
    {
      get
      {
        return listboxwidget.Visible;
      }
      set
      {
        if (listboxwidget.Visible == value)
        {
          return;
        }

        if (value)
        {
          Height += dropdown_height;
          host.SetFocus((Element2D) this);
        }
        else
        {
          Height -= dropdown_height;
        }

        listboxwidget.Visible = value;
        Refresh();
        if (Parent == null)
        {
          return;
        }

        if (value)
        {
          Parent.DropdownElement = (Element2D)listboxwidget;
        }
        else
        {
          Parent.DropdownElement = (Element2D) null;
        }
      }
    }

    public int DropdownHeight
    {
      get
      {
        return dropdown_height;
      }
      set
      {
        dropdown_height = value;
        Refresh();
      }
    }

    [XmlAttribute("font-color")]
    public string HexColor
    {
      get
      {
        return hexColor;
      }
      set
      {
        hexColor = value;
      }
    }

    [XmlAttribute("font-size")]
    public FontSize Size
    {
      get
      {
        return fontSize;
      }
      set
      {
        fontSize = value;
      }
    }

    [XmlAttribute("items-enum")]
    public string ItemsEnumString
    {
      get
      {
        return itemsEnumString;
      }
      set
      {
        itemsEnumString = value;
        var type = Type.GetType(itemsEnumString);
        if (!(type != (Type) null) || !type.IsEnum)
        {
          return;
        }

        foreach (var name in Enum.GetNames(type))
        {
          listboxwidget.Items.Add((object) name.Replace("__", " "));
        }

        itemsEnum = type;
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
