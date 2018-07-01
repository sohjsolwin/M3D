// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.GUIHost
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Interfaces;
using M3D.Graphics.TextLocalization;
using M3D.Graphics.Widgets2D;
using M3D.Properties;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using QuickFont;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace M3D.Graphics
{
  public class GUIHost
  {
    private object QFont_GDI_LOCK = new object();
    private List<IProcess> m_olProcessList = new List<IProcess>();
    private List<IProcess> m_olRemoveProcessList = new List<IProcess>();
    private Tooltip tooltipFrame;
    private Dictionary<FontSize, FontInfo> font_mapping;
    private Locale m_olocaleCurrent;
    private QFont m_ofntCurFont;
    private float m_fDefaultFontSize;
    private Simple2DRenderer m_renderer;
    private Frame m_oframeRootElement;
    private Frame m_oframeRootControlElement;
    private Frame m_oframeRootMasterElement;
    private DialogRootWidget m_odialogRootElement;
    private Element2D m_oeMouseOverElement;
    private Element2D m_oeFocusElement;
    private MouseEvent m_smePrevMouseEvent;
    private Element2D m_oeSelectedElement;
    private ComboBoxWidget m_ocbComboboxSelected;

    public GUIHost(Locale default_locale, float default_size, string PublicDataFolder, GLControl glControl)
    {
      this.m_olocaleCurrent = default_locale;
      if (this.m_olocaleCurrent == null)
        throw new ArgumentException("default_locale can not be null");
      this.font_mapping = new Dictionary<FontSize, FontInfo>();
      this.m_ofntCurFont = (QFont) null;
      this.m_fDefaultFontSize = default_size;
      this.m_oframeRootElement = new Frame(0);
      this.m_oframeRootControlElement = new Frame(0);
      this.m_oframeRootMasterElement = new Frame(0);
      this.m_odialogRootElement = new DialogRootWidget();
      this.m_oframeRootElement.RelativeX = 0.0f;
      this.m_oframeRootElement.RelativeY = 0.0f;
      this.m_oframeRootElement.RelativeWidth = 1f;
      this.m_oframeRootElement.RelativeHeight = 1f;
      this.m_oframeRootElement.IgnoreMouse = true;
      this.m_odialogRootElement.RelativeX = 0.0f;
      this.m_odialogRootElement.RelativeY = 0.0f;
      this.m_odialogRootElement.RelativeWidth = 1f;
      this.m_odialogRootElement.RelativeHeight = 1f;
      this.m_odialogRootElement.IgnoreMouse = false;
      this.m_oframeRootControlElement.RelativeX = 0.0f;
      this.m_oframeRootControlElement.RelativeY = 0.0f;
      this.m_oframeRootControlElement.RelativeWidth = 1f;
      this.m_oframeRootControlElement.RelativeHeight = 1f;
      this.m_oframeRootControlElement.IgnoreMouse = true;
      this.m_oframeRootMasterElement.AddChildElement((Element2D) this.m_oframeRootElement);
      this.m_oframeRootMasterElement.AddChildElement((Element2D) this.m_odialogRootElement);
      this.m_oframeRootMasterElement.AddChildElement((Element2D) this.m_oframeRootControlElement);
      this.m_renderer = new Simple2DRenderer(glControl.Width, glControl.Height);
      this.m_renderer.SetLineWidth(2f);
      this.m_renderer.SetTexturePath(Path.Combine(Path.Combine(PublicDataFolder, "Data"), "GUIImages"));
      int num1 = (int) this.m_renderer.LoadTextureFromBitmap(Resources.controls, "guicontrols");
      int num2 = (int) this.m_renderer.LoadTextureFromBitmap(Resources.extended_working, "extendedcontrols");
      int num3 = (int) this.m_renderer.LoadTextureFromBitmap(Resources.extended_working2, "extendedcontrols2");
      int num4 = (int) this.m_renderer.LoadTextureFromBitmap(Resources.extended_working3, "extendedcontrols3");
      this.m_renderer.SetCurrentTexture(this.m_renderer.GetTextureByName("guicontrols"));
      this.tooltipFrame = new Tooltip(this);
    }

    public int GLWindowWidth()
    {
      return this.m_renderer.GLWindowWidth();
    }

    public int GLWindowHeight()
    {
      return this.m_renderer.GLWindowHeight();
    }

    public void OnKeyboardEvent(KeyboardEvent keyboardevent)
    {
      if (keyboardevent.Tab)
      {
        List<Element2D> tabIndexElements = new List<Element2D>();
        this.m_oframeRootMasterElement.GetNextTabIndexElement(ref tabIndexElements);
        int num1 = -1;
        foreach (Element2D element2D in tabIndexElements)
        {
          if (element2D.tabIndex > num1)
            num1 = element2D.tabIndex;
        }
        int num2 = -1;
        Element2D element2D1 = (Element2D) null;
        if (this.m_oeFocusElement != null)
        {
          element2D1 = this.m_oeFocusElement;
          num2 = this.m_oeFocusElement.tabIndex;
        }
        int num3 = -1;
        foreach (Element2D element2D2 in tabIndexElements)
        {
          if (element2D2.tabIndex != num2)
          {
            if (element2D2.tabIndex < num2)
            {
              if (num1 - num2 + element2D2.tabIndex < num3 || num3 == -1)
              {
                this.m_oeFocusElement = element2D2;
                num3 = num1 - num2 + element2D2.tabIndex;
              }
            }
            else if (element2D2.tabIndex - num2 < num3 || num3 == -1)
            {
              this.m_oeFocusElement = element2D2;
              num3 = element2D2.tabIndex - num2;
            }
          }
        }
        if (element2D1 != null)
          element2D1.HasFocus = false;
        if (this.m_ocbComboboxSelected != null)
          this.m_ocbComboboxSelected.ShowDropDown = false;
        this.m_oeFocusElement.HasFocus = true;
        if (this.m_oeFocusElement.GetElementType() == ElementType.ComboBoxWidget)
          this.m_ocbComboboxSelected = (ComboBoxWidget) this.m_oeFocusElement;
        else
          this.m_ocbComboboxSelected = (ComboBoxWidget) null;
      }
      else if (keyboardevent.Type == KeyboardEventType.CommandKey && ((CommandKeyEvent) keyboardevent).Key == KeyboardCommandKey.Escape)
        this.m_oframeRootMasterElement.OnKeyboardEvent(keyboardevent);
      else if (this.m_oeFocusElement != null)
      {
        if (this.m_ocbComboboxSelected != null)
          this.m_ocbComboboxSelected.OnKeyboardEvent(keyboardevent);
        else if (this.m_oeFocusElement.IsListBoxElement())
        {
          ListBoxWidget listBoxElement = this.m_oeFocusElement.GetListBoxElement();
          if (listBoxElement == null)
            return;
          listBoxElement.HasFocus = true;
          listBoxElement.OnKeyboardEvent(keyboardevent);
          listBoxElement.HasFocus = false;
        }
        else
          this.m_oeFocusElement.OnKeyboardEvent(keyboardevent);
      }
      else
        this.m_oframeRootMasterElement.OnKeyboardEvent(keyboardevent);
    }

    public void SetFocus(int ID)
    {
      if (this.m_oeFocusElement != null)
        this.m_oeFocusElement.HasFocus = false;
      this.m_oeFocusElement = this.m_oframeRootMasterElement.FindChildElement(ID);
      if (this.m_oeFocusElement == null)
        return;
      this.m_oeFocusElement.HasFocus = true;
    }

    public void SetFocus(Element2D element)
    {
      if (this.m_oeFocusElement == element)
        return;
      if (this.m_oeFocusElement != null)
        this.m_oeFocusElement.HasFocus = false;
      this.m_oeFocusElement = element;
      if (this.m_oeFocusElement == null)
        return;
      this.m_oeFocusElement.HasFocus = true;
    }

    public void OnUpdate()
    {
      if (this.m_odialogRootElement.ChildList.Count > 0)
        this.m_oframeRootElement.Enabled = false;
      else if (!this.m_oframeRootElement.Enabled)
        this.m_oframeRootElement.Enabled = true;
      lock (this.m_olProcessList)
      {
        foreach (IProcess olProcess in this.m_olProcessList)
          olProcess.Process();
      }
      lock (this.m_olRemoveProcessList)
      {
        foreach (IProcess olRemoveProcess in this.m_olRemoveProcessList)
        {
          lock (this.m_olProcessList)
          {
            if (this.m_olProcessList.Contains(olRemoveProcess))
              this.m_olProcessList.Remove(olRemoveProcess);
          }
        }
        this.m_olRemoveProcessList.Clear();
      }
      this.m_oframeRootMasterElement.OnUpdate();
      if (this.m_oeMouseOverElement != null && !string.IsNullOrEmpty(this.m_oeMouseOverElement.ToolTipMessage))
      {
        this.tooltipFrame.SetMessage(this.m_oeMouseOverElement.ToolTipMessage);
        this.tooltipFrame.Show(this.m_smePrevMouseEvent.pos.x, this.m_smePrevMouseEvent.pos.y);
      }
      this.tooltipFrame.OnUpdate();
    }

    public void Render()
    {
      try
      {
        this.RefreshViews();
        this.GetSimpleRenderer().Begin2D();
        this.m_oframeRootMasterElement.OnRender(this);
        if (this.m_oeFocusElement != null && this.m_oeFocusElement.FocusedAlwaysOnTop)
          this.m_oeFocusElement.OnRender(this);
        this.tooltipFrame.OnRender(this);
        this.GetSimpleRenderer().End2D();
        GL.Color4(1f, 1f, 1f, 1f);
      }
      catch (Exception ex)
      {
        if (ex is InvalidOperationException || ex is NullReferenceException)
          return;
        throw;
      }
    }

    public Simple2DRenderer GetSimpleRenderer()
    {
      return this.m_renderer;
    }

    public void OnResize(int width, int height)
    {
      this.m_renderer.OnGLWindowResize(width, height);
      this.m_oframeRootMasterElement.SetSize(width, height);
      this.Refresh();
      this.RefreshViews();
    }

    public void AddElement(Element2D child)
    {
      this.m_oframeRootElement.AddChildElement(child);
    }

    public void AddControlElement(Element2D child)
    {
      this.m_oframeRootControlElement.AddChildElement(child);
    }

    public void AddProcess(IProcess process)
    {
      lock (this.m_olProcessList)
        this.m_olProcessList.Add(process);
    }

    public void RemoveProcess(IProcess process)
    {
      lock (this.m_olRemoveProcessList)
        this.m_olRemoveProcessList.Add(process);
    }

    public Element2DList GlobalChildDialog
    {
      set
      {
      }
      get
      {
        return this.m_odialogRootElement.ChildList;
      }
    }

    public bool HasChildDialog
    {
      get
      {
        return this.m_odialogRootElement.ChildList.Count > 0;
      }
    }

    public Element2D RootElement
    {
      get
      {
        return (Element2D) this.m_oframeRootElement;
      }
    }

    public void Refresh()
    {
      this.m_oframeRootMasterElement.SetSize(this.m_oframeRootMasterElement.Width, this.m_oframeRootMasterElement.Height);
      this.m_oframeRootMasterElement.SetPosition(this.m_oframeRootMasterElement.X, this.m_oframeRootMasterElement.Y);
      this.m_oframeRootMasterElement.SetSize(this.m_oframeRootMasterElement.Width, this.m_oframeRootMasterElement.Height);
    }

    public bool OnMouseCommand(MouseEvent mouseevent)
    {
      bool flag = false;
      if (mouseevent.type == MouseEventType.Leave)
      {
        this.m_oframeRootMasterElement.OnMouseLeave();
        return false;
      }
      this.tooltipFrame.Hide();
      this.m_oeMouseOverElement = this.m_oframeRootControlElement.GetSelfOrDependantAtPoint(mouseevent.pos.x, mouseevent.pos.y);
      if (this.m_oeMouseOverElement == null)
      {
        if (this.m_oeFocusElement != null && this.m_oeFocusElement.Visible && this.m_oeFocusElement.Enabled)
          this.m_oeMouseOverElement = this.m_oeFocusElement.GetSelfOrDependantAtPoint(mouseevent.pos.x, mouseevent.pos.y);
        if (this.m_oeMouseOverElement == null)
          this.m_oeMouseOverElement = this.m_oframeRootMasterElement.GetSelfOrDependantAtPoint(mouseevent.pos.x, mouseevent.pos.y);
      }
      if (this.m_oeSelectedElement != null && this.m_oeSelectedElement != this.m_oeMouseOverElement)
      {
        flag = this.m_oeSelectedElement.OnMouseCommand(mouseevent);
        this.m_oeSelectedElement = (Element2D) null;
      }
      Element2D mouseOverElement = this.m_oeMouseOverElement;
      if (mouseOverElement != null)
      {
        this.m_oeSelectedElement = mouseOverElement;
        flag = mouseOverElement.OnMouseCommand(mouseevent);
        if (mouseOverElement.GetElementType() != ElementType.Frame && mouseOverElement.GetElementType() != ElementType.ScrollFrame)
        {
          if (mouseevent.type == MouseEventType.Down)
          {
            if (this.m_oeSelectedElement.IsComboBoxElement())
            {
              if (this.m_ocbComboboxSelected != null && this.m_ocbComboboxSelected != this.m_oeSelectedElement.GetComboBoxElement())
              {
                this.m_ocbComboboxSelected.ShowDropDown = false;
                this.m_ocbComboboxSelected.HasFocus = false;
              }
              this.m_ocbComboboxSelected = this.m_oeSelectedElement.GetComboBoxElement();
            }
            else if (this.m_ocbComboboxSelected != null)
            {
              this.m_ocbComboboxSelected.ShowDropDown = false;
              this.m_ocbComboboxSelected = (ComboBoxWidget) null;
            }
            if (this.m_oeFocusElement != null && this.m_oeFocusElement != mouseOverElement && this.m_oeFocusElement != this.m_ocbComboboxSelected)
              this.m_oeFocusElement.HasFocus = false;
            this.m_oeFocusElement = mouseOverElement;
            this.m_oeFocusElement.HasFocus = true;
            if (this.m_ocbComboboxSelected != null)
              this.m_ocbComboboxSelected.HasFocus = true;
          }
          else if (mouseevent.type == MouseEventType.Up && mouseevent.button == MouseButton.Right)
          {
            int elementType = (int) this.m_oeMouseOverElement.GetElementType();
          }
        }
        if (mouseevent.type == MouseEventType.MouseWheel)
        {
          if (this.m_oeSelectedElement.IsListBoxElement())
          {
            ListBoxWidget listBoxElement = this.m_oeSelectedElement.GetListBoxElement();
            if (listBoxElement != null)
            {
              if (mouseevent.delta > 0)
                listBoxElement.ScrollBar.MoveSlider(-1f);
              else
                listBoxElement.ScrollBar.MoveSlider(1f);
            }
          }
          else if (this.m_oeSelectedElement.IsScrollFrame())
            this.m_oeSelectedElement.GetScrollFrame()?.OnMouseCommand(mouseevent);
        }
      }
      else if (this.GlobalChildDialog.Count > 0 && mouseevent.type == MouseEventType.Down)
      {
        Element2D element2D = this.GlobalChildDialog.Last();
        if (element2D is Frame)
          ((Frame) element2D).Close();
      }
      if (mouseevent.type == MouseEventType.Up)
      {
        this.m_oframeRootMasterElement.OnMouseLeave();
        if (this.m_ocbComboboxSelected != null && this.m_oeSelectedElement == null)
          this.m_ocbComboboxSelected.ShowDropDown = false;
      }
      this.m_oframeRootMasterElement.OnMouseMove(mouseevent.pos.x, mouseevent.pos.y);
      this.m_smePrevMouseEvent = mouseevent;
      return flag;
    }

    public void PrintWithBounds(string text, RectangleF bounds, QFontAlignment alignment)
    {
      if (this.m_ofntCurFont == null)
        return;
      GL.Disable(EnableCap.Texture2D);
      float width = bounds.Width;
      QFont.Begin();
      this.m_ofntCurFont.Options.Colour = new Color4(1f, 0.0f, 0.0f, 1f);
      this.m_ofntCurFont.Print(text, width, alignment, new Vector2(bounds.X, bounds.Y));
      QFont.End();
      GL.Color4(1f, 1f, 1f, 1f);
    }

    public void RefreshViews()
    {
      QFont.ForceViewportRefresh();
    }

    public void SetLocale(Locale new_locale)
    {
      this.m_olocaleCurrent = new_locale;
      foreach (KeyValuePair<FontSize, FontInfo> keyValuePair in this.font_mapping)
        keyValuePair.Value.font = (QFont) null;
      foreach (KeyValuePair<FontSize, FontInfo> keyValuePair in this.font_mapping)
        this.SetFontProperty(keyValuePair.Key, keyValuePair.Value.realsize);
    }

    public void SetFontProperty(FontSize size, float ptsize)
    {
      lock (this.QFont_GDI_LOCK)
      {
        FontSize fontWithPtSize = this.GetFontWithPtSize(ptsize);
        QFont qfont;
        if (fontWithPtSize != FontSize.Undefined)
          qfont = this.font_mapping[fontWithPtSize].font;
        else if (this.m_olocaleCurrent.GetFontFile() != null)
        {
          try
          {
            qfont = new QFont(this.m_olocaleCurrent.GetFontFile(), ptsize);
          }
          catch
          {
            qfont = new QFont(new Font(this.m_olocaleCurrent.GetFontFamily(), ptsize));
          }
        }
        else
          qfont = new QFont(new Font(this.m_olocaleCurrent.GetFontFamily(), ptsize));
        if (!this.font_mapping.ContainsKey(size))
          this.font_mapping[size] = new FontInfo();
        this.font_mapping[size].font = qfont;
        this.font_mapping[size].realsize = ptsize;
        if (this.m_ofntCurFont != null)
          return;
        this.SetCurFontSize(size);
      }
    }

    public void SetCurFontSize(FontSize size)
    {
      if (!this.font_mapping.ContainsKey(size))
        this.SetFontProperty(size, this.m_fDefaultFontSize);
      this.m_ofntCurFont = this.font_mapping[size].font;
    }

    public Locale Locale
    {
      get
      {
        return this.m_olocaleCurrent;
      }
    }

    private FontSize GetFontWithPtSize(float ptsize)
    {
      foreach (KeyValuePair<FontSize, FontInfo> keyValuePair in this.font_mapping)
      {
        if ((double) keyValuePair.Value.realsize == (double) ptsize && keyValuePair.Value.font != null)
          return keyValuePair.Key;
      }
      return FontSize.Undefined;
    }

    public QFont GetCurrentFont()
    {
      return this.m_ofntCurFont;
    }
  }
}
