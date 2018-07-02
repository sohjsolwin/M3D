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
      m_olocaleCurrent = default_locale;
      if (m_olocaleCurrent == null)
      {
        throw new ArgumentException("default_locale can not be null");
      }

      font_mapping = new Dictionary<FontSize, FontInfo>();
      m_ofntCurFont = (QFont) null;
      m_fDefaultFontSize = default_size;
      m_oframeRootElement = new Frame(0);
      m_oframeRootControlElement = new Frame(0);
      m_oframeRootMasterElement = new Frame(0);
      m_odialogRootElement = new DialogRootWidget();
      m_oframeRootElement.RelativeX = 0.0f;
      m_oframeRootElement.RelativeY = 0.0f;
      m_oframeRootElement.RelativeWidth = 1f;
      m_oframeRootElement.RelativeHeight = 1f;
      m_oframeRootElement.IgnoreMouse = true;
      m_odialogRootElement.RelativeX = 0.0f;
      m_odialogRootElement.RelativeY = 0.0f;
      m_odialogRootElement.RelativeWidth = 1f;
      m_odialogRootElement.RelativeHeight = 1f;
      m_odialogRootElement.IgnoreMouse = false;
      m_oframeRootControlElement.RelativeX = 0.0f;
      m_oframeRootControlElement.RelativeY = 0.0f;
      m_oframeRootControlElement.RelativeWidth = 1f;
      m_oframeRootControlElement.RelativeHeight = 1f;
      m_oframeRootControlElement.IgnoreMouse = true;
      m_oframeRootMasterElement.AddChildElement((Element2D)m_oframeRootElement);
      m_oframeRootMasterElement.AddChildElement((Element2D)m_odialogRootElement);
      m_oframeRootMasterElement.AddChildElement((Element2D)m_oframeRootControlElement);
      m_renderer = new Simple2DRenderer(glControl.Width, glControl.Height);
      m_renderer.SetLineWidth(2f);
      m_renderer.SetTexturePath(Path.Combine(Path.Combine(PublicDataFolder, "Data"), "GUIImages"));
      var num1 = (int)m_renderer.LoadTextureFromBitmap(Resources.controls, "guicontrols");
      var num2 = (int)m_renderer.LoadTextureFromBitmap(Resources.extended_working, "extendedcontrols");
      var num3 = (int)m_renderer.LoadTextureFromBitmap(Resources.extended_working2, "extendedcontrols2");
      var num4 = (int)m_renderer.LoadTextureFromBitmap(Resources.extended_working3, "extendedcontrols3");
      m_renderer.SetCurrentTexture(m_renderer.GetTextureByName("guicontrols"));
      tooltipFrame = new Tooltip(this);
    }

    public int GLWindowWidth()
    {
      return m_renderer.GLWindowWidth();
    }

    public int GLWindowHeight()
    {
      return m_renderer.GLWindowHeight();
    }

    public void OnKeyboardEvent(KeyboardEvent keyboardevent)
    {
      if (keyboardevent.Tab)
      {
        var tabIndexElements = new List<Element2D>();
        m_oframeRootMasterElement.GetNextTabIndexElement(ref tabIndexElements);
        var num1 = -1;
        foreach (Element2D element2D in tabIndexElements)
        {
          if (element2D.tabIndex > num1)
          {
            num1 = element2D.tabIndex;
          }
        }
        var num2 = -1;
        var element2D1 = (Element2D) null;
        if (m_oeFocusElement != null)
        {
          element2D1 = m_oeFocusElement;
          num2 = m_oeFocusElement.tabIndex;
        }
        var num3 = -1;
        foreach (Element2D element2D2 in tabIndexElements)
        {
          if (element2D2.tabIndex != num2)
          {
            if (element2D2.tabIndex < num2)
            {
              if (num1 - num2 + element2D2.tabIndex < num3 || num3 == -1)
              {
                m_oeFocusElement = element2D2;
                num3 = num1 - num2 + element2D2.tabIndex;
              }
            }
            else if (element2D2.tabIndex - num2 < num3 || num3 == -1)
            {
              m_oeFocusElement = element2D2;
              num3 = element2D2.tabIndex - num2;
            }
          }
        }
        if (element2D1 != null)
        {
          element2D1.HasFocus = false;
        }

        if (m_ocbComboboxSelected != null)
        {
          m_ocbComboboxSelected.ShowDropDown = false;
        }

        m_oeFocusElement.HasFocus = true;
        if (m_oeFocusElement.GetElementType() == ElementType.ComboBoxWidget)
        {
          m_ocbComboboxSelected = (ComboBoxWidget)m_oeFocusElement;
        }
        else
        {
          m_ocbComboboxSelected = (ComboBoxWidget) null;
        }
      }
      else if (keyboardevent.Type == KeyboardEventType.CommandKey && ((CommandKeyEvent) keyboardevent).Key == KeyboardCommandKey.Escape)
      {
        m_oframeRootMasterElement.OnKeyboardEvent(keyboardevent);
      }
      else if (m_oeFocusElement != null)
      {
        if (m_ocbComboboxSelected != null)
        {
          m_ocbComboboxSelected.OnKeyboardEvent(keyboardevent);
        }
        else if (m_oeFocusElement.IsListBoxElement())
        {
          ListBoxWidget listBoxElement = m_oeFocusElement.GetListBoxElement();
          if (listBoxElement == null)
          {
            return;
          }

          listBoxElement.HasFocus = true;
          listBoxElement.OnKeyboardEvent(keyboardevent);
          listBoxElement.HasFocus = false;
        }
        else
        {
          m_oeFocusElement.OnKeyboardEvent(keyboardevent);
        }
      }
      else
      {
        m_oframeRootMasterElement.OnKeyboardEvent(keyboardevent);
      }
    }

    public void SetFocus(int ID)
    {
      if (m_oeFocusElement != null)
      {
        m_oeFocusElement.HasFocus = false;
      }

      m_oeFocusElement = m_oframeRootMasterElement.FindChildElement(ID);
      if (m_oeFocusElement == null)
      {
        return;
      }

      m_oeFocusElement.HasFocus = true;
    }

    public void SetFocus(Element2D element)
    {
      if (m_oeFocusElement == element)
      {
        return;
      }

      if (m_oeFocusElement != null)
      {
        m_oeFocusElement.HasFocus = false;
      }

      m_oeFocusElement = element;
      if (m_oeFocusElement == null)
      {
        return;
      }

      m_oeFocusElement.HasFocus = true;
    }

    public void OnUpdate()
    {
      if (m_odialogRootElement.ChildList.Count > 0)
      {
        m_oframeRootElement.Enabled = false;
      }
      else if (!m_oframeRootElement.Enabled)
      {
        m_oframeRootElement.Enabled = true;
      }

      lock (m_olProcessList)
      {
        foreach (IProcess olProcess in m_olProcessList)
        {
          olProcess.Process();
        }
      }
      lock (m_olRemoveProcessList)
      {
        foreach (IProcess olRemoveProcess in m_olRemoveProcessList)
        {
          lock (m_olProcessList)
          {
            if (m_olProcessList.Contains(olRemoveProcess))
            {
              m_olProcessList.Remove(olRemoveProcess);
            }
          }
        }
        m_olRemoveProcessList.Clear();
      }
      m_oframeRootMasterElement.OnUpdate();
      if (m_oeMouseOverElement != null && !string.IsNullOrEmpty(m_oeMouseOverElement.ToolTipMessage))
      {
        tooltipFrame.SetMessage(m_oeMouseOverElement.ToolTipMessage);
        tooltipFrame.Show(m_smePrevMouseEvent.pos.x, m_smePrevMouseEvent.pos.y);
      }
      tooltipFrame.OnUpdate();
    }

    public void Render()
    {
      try
      {
        RefreshViews();
        GetSimpleRenderer().Begin2D();
        m_oframeRootMasterElement.OnRender(this);
        if (m_oeFocusElement != null && m_oeFocusElement.FocusedAlwaysOnTop)
        {
          m_oeFocusElement.OnRender(this);
        }

        tooltipFrame.OnRender(this);
        GetSimpleRenderer().End2D();
        GL.Color4(1f, 1f, 1f, 1f);
      }
      catch (Exception ex)
      {
        if (ex is InvalidOperationException || ex is NullReferenceException)
        {
          return;
        }

        throw;
      }
    }

    public Simple2DRenderer GetSimpleRenderer()
    {
      return m_renderer;
    }

    public void OnResize(int width, int height)
    {
      m_renderer.OnGLWindowResize(width, height);
      m_oframeRootMasterElement.SetSize(width, height);
      Refresh();
      RefreshViews();
    }

    public void AddElement(Element2D child)
    {
      m_oframeRootElement.AddChildElement(child);
    }

    public void AddControlElement(Element2D child)
    {
      m_oframeRootControlElement.AddChildElement(child);
    }

    public void AddProcess(IProcess process)
    {
      lock (m_olProcessList)
      {
        m_olProcessList.Add(process);
      }
    }

    public void RemoveProcess(IProcess process)
    {
      lock (m_olRemoveProcessList)
      {
        m_olRemoveProcessList.Add(process);
      }
    }

    public Element2DList GlobalChildDialog
    {
      set
      {
      }
      get
      {
        return m_odialogRootElement.ChildList;
      }
    }

    public bool HasChildDialog
    {
      get
      {
        return m_odialogRootElement.ChildList.Count > 0;
      }
    }

    public Element2D RootElement
    {
      get
      {
        return (Element2D)m_oframeRootElement;
      }
    }

    public void Refresh()
    {
      m_oframeRootMasterElement.SetSize(m_oframeRootMasterElement.Width, m_oframeRootMasterElement.Height);
      m_oframeRootMasterElement.SetPosition(m_oframeRootMasterElement.X, m_oframeRootMasterElement.Y);
      m_oframeRootMasterElement.SetSize(m_oframeRootMasterElement.Width, m_oframeRootMasterElement.Height);
    }

    public bool OnMouseCommand(MouseEvent mouseevent)
    {
      var flag = false;
      if (mouseevent.type == MouseEventType.Leave)
      {
        m_oframeRootMasterElement.OnMouseLeave();
        return false;
      }
      tooltipFrame.Hide();
      m_oeMouseOverElement = m_oframeRootControlElement.GetSelfOrDependantAtPoint(mouseevent.pos.x, mouseevent.pos.y);
      if (m_oeMouseOverElement == null)
      {
        if (m_oeFocusElement != null && m_oeFocusElement.Visible && m_oeFocusElement.Enabled)
        {
          m_oeMouseOverElement = m_oeFocusElement.GetSelfOrDependantAtPoint(mouseevent.pos.x, mouseevent.pos.y);
        }

        if (m_oeMouseOverElement == null)
        {
          m_oeMouseOverElement = m_oframeRootMasterElement.GetSelfOrDependantAtPoint(mouseevent.pos.x, mouseevent.pos.y);
        }
      }
      if (m_oeSelectedElement != null && m_oeSelectedElement != m_oeMouseOverElement)
      {
        flag = m_oeSelectedElement.OnMouseCommand(mouseevent);
        m_oeSelectedElement = (Element2D) null;
      }
      Element2D mouseOverElement = m_oeMouseOverElement;
      if (mouseOverElement != null)
      {
        m_oeSelectedElement = mouseOverElement;
        flag = mouseOverElement.OnMouseCommand(mouseevent);
        if (mouseOverElement.GetElementType() != ElementType.Frame && mouseOverElement.GetElementType() != ElementType.ScrollFrame)
        {
          if (mouseevent.type == MouseEventType.Down)
          {
            if (m_oeSelectedElement.IsComboBoxElement())
            {
              if (m_ocbComboboxSelected != null && m_ocbComboboxSelected != m_oeSelectedElement.GetComboBoxElement())
              {
                m_ocbComboboxSelected.ShowDropDown = false;
                m_ocbComboboxSelected.HasFocus = false;
              }
              m_ocbComboboxSelected = m_oeSelectedElement.GetComboBoxElement();
            }
            else if (m_ocbComboboxSelected != null)
            {
              m_ocbComboboxSelected.ShowDropDown = false;
              m_ocbComboboxSelected = (ComboBoxWidget) null;
            }
            if (m_oeFocusElement != null && m_oeFocusElement != mouseOverElement && m_oeFocusElement != m_ocbComboboxSelected)
            {
              m_oeFocusElement.HasFocus = false;
            }

            m_oeFocusElement = mouseOverElement;
            m_oeFocusElement.HasFocus = true;
            if (m_ocbComboboxSelected != null)
            {
              m_ocbComboboxSelected.HasFocus = true;
            }
          }
          else if (mouseevent.type == MouseEventType.Up && mouseevent.button == MouseButton.Right)
          {
            var elementType = (int)m_oeMouseOverElement.GetElementType();
          }
        }
        if (mouseevent.type == MouseEventType.MouseWheel)
        {
          if (m_oeSelectedElement.IsListBoxElement())
          {
            ListBoxWidget listBoxElement = m_oeSelectedElement.GetListBoxElement();
            if (listBoxElement != null)
            {
              if (mouseevent.delta > 0)
              {
                listBoxElement.ScrollBar.MoveSlider(-1f);
              }
              else
              {
                listBoxElement.ScrollBar.MoveSlider(1f);
              }
            }
          }
          else if (m_oeSelectedElement.IsScrollFrame())
          {
            m_oeSelectedElement.GetScrollFrame()?.OnMouseCommand(mouseevent);
          }
        }
      }
      else if (GlobalChildDialog.Count > 0 && mouseevent.type == MouseEventType.Down)
      {
        Element2D element2D = GlobalChildDialog.Last();
        if (element2D is Frame)
        {
          ((Frame) element2D).Close();
        }
      }
      if (mouseevent.type == MouseEventType.Up)
      {
        m_oframeRootMasterElement.OnMouseLeave();
        if (m_ocbComboboxSelected != null && m_oeSelectedElement == null)
        {
          m_ocbComboboxSelected.ShowDropDown = false;
        }
      }
      m_oframeRootMasterElement.OnMouseMove(mouseevent.pos.x, mouseevent.pos.y);
      m_smePrevMouseEvent = mouseevent;
      return flag;
    }

    public void PrintWithBounds(string text, RectangleF bounds, QFontAlignment alignment)
    {
      if (m_ofntCurFont == null)
      {
        return;
      }

      GL.Disable(EnableCap.Texture2D);
      var width = bounds.Width;
      QFont.Begin();
      m_ofntCurFont.Options.Colour = new Color4(1f, 0.0f, 0.0f, 1f);
      m_ofntCurFont.Print(text, width, alignment, new Vector2(bounds.X, bounds.Y));
      QFont.End();
      GL.Color4(1f, 1f, 1f, 1f);
    }

    public void RefreshViews()
    {
      QFont.ForceViewportRefresh();
    }

    public void SetLocale(Locale new_locale)
    {
      m_olocaleCurrent = new_locale;
      foreach (KeyValuePair<FontSize, FontInfo> keyValuePair in font_mapping)
      {
        keyValuePair.Value.font = (QFont) null;
      }

      foreach (KeyValuePair<FontSize, FontInfo> keyValuePair in font_mapping)
      {
        SetFontProperty(keyValuePair.Key, keyValuePair.Value.realsize);
      }
    }

    public void SetFontProperty(FontSize size, float ptsize)
    {
      lock (QFont_GDI_LOCK)
      {
        FontSize fontWithPtSize = GetFontWithPtSize(ptsize);
        QFont qfont;
        if (fontWithPtSize != FontSize.Undefined)
        {
          qfont = font_mapping[fontWithPtSize].font;
        }
        else if (m_olocaleCurrent.GetFontFile() != null)
        {
          try
          {
            qfont = new QFont(m_olocaleCurrent.GetFontFile(), ptsize);
          }
          catch
          {
            qfont = new QFont(new Font(m_olocaleCurrent.GetFontFamily(), ptsize));
          }
        }
        else
        {
          qfont = new QFont(new Font(m_olocaleCurrent.GetFontFamily(), ptsize));
        }

        if (!font_mapping.ContainsKey(size))
        {
          font_mapping[size] = new FontInfo();
        }

        font_mapping[size].font = qfont;
        font_mapping[size].realsize = ptsize;
        if (m_ofntCurFont != null)
        {
          return;
        }

        SetCurFontSize(size);
      }
    }

    public void SetCurFontSize(FontSize size)
    {
      if (!font_mapping.ContainsKey(size))
      {
        SetFontProperty(size, m_fDefaultFontSize);
      }

      m_ofntCurFont = font_mapping[size].font;
    }

    public Locale Locale
    {
      get
      {
        return m_olocaleCurrent;
      }
    }

    private FontSize GetFontWithPtSize(float ptsize)
    {
      foreach (KeyValuePair<FontSize, FontInfo> keyValuePair in font_mapping)
      {
        if ((double) keyValuePair.Value.realsize == (double) ptsize && keyValuePair.Value.font != null)
        {
          return keyValuePair.Key;
        }
      }
      return FontSize.Undefined;
    }

    public QFont GetCurrentFont()
    {
      return m_ofntCurFont;
    }
  }
}
