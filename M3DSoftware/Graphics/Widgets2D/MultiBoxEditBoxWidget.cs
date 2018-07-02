// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Widgets2D.MultiBoxEditBoxWidget
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using OpenTK.Graphics;
using System;
using System.Xml.Serialization;

namespace M3D.Graphics.Widgets2D
{
  public class MultiBoxEditBoxWidget : AbstractEditBoxWidget
  {
    public int MaxCharacterPerBox = 1;
    private int num_boxes = 3;
    private FontSize size = FontSize.Medium;
    private Color4 color = new Color4(0.0f, 0.0f, 0.0f, 1f);
    protected MultiBoxEditBoxWidget.EditBoxCallback enterkeycallback;
    protected MultiBoxEditBoxWidget.EditBoxCallback clickkeycallback;
    protected MultiBoxEditBoxWidget.EditBoxCallback onnewtext;
    private bool initialized;
    private GUIHost host;
    private EditBoxWidget[] editboxes;

    public MultiBoxEditBoxWidget(int ID = 0, Element2D parent = null)
      : base(ID, parent)
    {
    }

    public void Init(GUIHost host, int num_boxes, int maxCharacterPerBox)
    {
      this.host = host;
      MaxCharacterPerBox = maxCharacterPerBox;
      NumBoxes = num_boxes;
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      base.InitChildren(parent, host, MyButtonCallback);
      this.host = host;
      NumBoxes = NumBoxes;
      Color = Color;
      Size = Size;
      Refresh();
    }

    private void Initialize(int num)
    {
      var num1 = 1.0 - ((double) num - 1.0) * 0.100000001490116;
      var num2 = 0.1f;
      var num3 = (double) num;
      var num4 = (float) (num1 / num3);
      var num5 = 0.0f;
      editboxes = new EditBoxWidget[num];
      for (var index = 0; index < num; ++index)
      {
        var editBoxWidget = new EditBoxWidget(index + 1000, (Element2D) this);
        editBoxWidget.Init(host, "guicontrols", 640f, 448f, 672f, 480f);
        editBoxWidget.SetGrowableWidth(4, 4, 32);
        editBoxWidget.Text = "";
        editBoxWidget.OnKeyboardEvent(new KeyboardEvent(KeyboardEventType.InputKey, false, false, false));
        editBoxWidget.Enabled = true;
        editBoxWidget.SetSize(100, 24);
        editBoxWidget.Color = Color;
        editBoxWidget.SetTextWindowBorders(7, 7, 7, 7);
        editBoxWidget.RelativeX = num5;
        editBoxWidget.RelativeY = 0.0f;
        editBoxWidget.RelativeWidth = num4;
        editBoxWidget.RelativeHeight = 1f;
        editBoxWidget.Size = FontSize.Large;
        editBoxWidget.CAPS = true;
        editBoxWidget.SetCallbackEnterKey(new EditBoxWidget.EditBoxCallback(EditBoxCallbackEnterKey));
        editBoxWidget.SetCallbackOnClick(new EditBoxWidget.EditBoxCallback(EditBoxCallbackOnClick));
        editBoxWidget.SetCallbackOnTextAdded(new EditBoxWidget.EditBoxCallback(EditBoxCallbackEnterKeyOnTextAdded));
        editBoxWidget.SetCallbackOnBackspace(new EditBoxWidget.EditBoxCallback(EditBoxCallbackOnBackSpace));
        editBoxWidget.MAX_CHARS = MaxCharacterPerBox;
        num5 += num2 + num4;
        AddChildElement((Element2D) editBoxWidget);
        editboxes[index] = editBoxWidget;
      }
      initialized = true;
    }

    public override void OnFocusChanged()
    {
      if (!HasFocus || !initialized || editboxes.Length == 0)
      {
        return;
      }

      host.SetFocus((Element2D)editboxes[0]);
    }

    public Color4 Color
    {
      get
      {
        return color;
      }
      set
      {
        color = value;
        if (!initialized)
        {
          return;
        }

        foreach (EditBoxWidget editbox in editboxes)
        {
          editbox.Color = value;
        }
      }
    }

    [XmlAttribute("font-size")]
    public FontSize Size
    {
      get
      {
        return size;
      }
      set
      {
        size = value;
        if (!initialized)
        {
          return;
        }

        foreach (EditBoxWidget editbox in editboxes)
        {
          editbox.Size = value;
        }
      }
    }

    public int NumBoxes
    {
      get
      {
        return num_boxes;
      }
      set
      {
        if (initialized)
        {
          throw new InvalidOperationException("MultiBoxEditBoxWidget can only be initialized once.");
        }

        if (host != null)
        {
          Initialize(value);
        }
        else
        {
          num_boxes = value;
        }
      }
    }

    public override string Text
    {
      get
      {
        if (!initialized)
        {
          return "";
        }

        var str = "";
        foreach (EditBoxWidget editbox in editboxes)
        {
          str += editbox.Text;
        }

        return str;
      }
      set
      {
        if (!initialized)
        {
          throw new InvalidOperationException("MultiBoxEditBoxWidget has not been initialized.");
        }

        foreach (AbstractEditBoxWidget editbox in editboxes)
        {
          editbox.Text = "";
        }

        var startIndex = 0;
        for (var index = 0; startIndex < value.Length && index < editboxes.Length; ++index)
        {
          var length = editboxes[index].MAX_CHARS;
          if (startIndex + length > value.Length)
          {
            length = value.Length - startIndex;
          }

          editboxes[index].Text = value.Substring(startIndex, length);
          startIndex += length;
        }
      }
    }

    private void EditBoxCallbackOnBackSpace(EditBoxWidget edit)
    {
      var cursor = edit.Cursor;
      if (!string.IsNullOrEmpty(edit.Text) || edit.Cursor != 0)
      {
        return;
      }

      var index = edit.ID - 1000 - 1;
      if (index < 0)
      {
        return;
      }

      host.SetFocus((Element2D)editboxes[index]);
    }

    private void EditBoxCallbackEnterKey(EditBoxWidget edit)
    {
      MultiBoxEditBoxWidget.EditBoxCallback enterkeycallback = this.enterkeycallback;
      if (enterkeycallback == null)
      {
        return;
      }

      enterkeycallback(this);
    }

    private void EditBoxCallbackOnClick(EditBoxWidget edit)
    {
      edit.Text = "";
      MultiBoxEditBoxWidget.EditBoxCallback clickkeycallback = this.clickkeycallback;
      if (clickkeycallback == null)
      {
        return;
      }

      clickkeycallback(this);
    }

    private void EditBoxCallbackEnterKeyOnTextAdded(EditBoxWidget edit)
    {
      if (edit.Text.Length == edit.MAX_CHARS)
      {
        var num = edit.ID - 1000;
        if (num < editboxes.Length - 1)
        {
          host.SetFocus((Element2D)editboxes[num + 1]);
        }
      }
      MultiBoxEditBoxWidget.EditBoxCallback onnewtext = this.onnewtext;
      if (onnewtext == null)
      {
        return;
      }

      onnewtext(this);
    }

    public void SetCallbackEnterKey(MultiBoxEditBoxWidget.EditBoxCallback func)
    {
      enterkeycallback = func;
    }

    public void SetCallbackOnClick(MultiBoxEditBoxWidget.EditBoxCallback func)
    {
      clickkeycallback = func;
    }

    public void SetCallbackOnTextAdded(MultiBoxEditBoxWidget.EditBoxCallback func)
    {
      onnewtext = func;
    }

    public delegate void EditBoxCallback(MultiBoxEditBoxWidget edit);
  }
}
