using M3D.Graphics;
using M3D.Graphics.Ext3D;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.Model.Utils;
using OpenTK.Graphics;
using System;

namespace M3D.GUI.Views.Printer_View
{
  internal class ModelAdjustmentsDialog : BorderedImageFrame
  {
    private Vector3 initialscalevalues = new Vector3();
    private bool enter_hit;
    private ButtonWidget linkScaling_button;
    private ImageWidget link_image;
    private ImageWidget top_bracket_image;
    private ImageWidget middle_bracket_image;
    private ImageWidget bottom_bracket_image;
    private ImageWidget X_Text;
    private ImageWidget Y_Text;
    private ImageWidget Z_Text;
    private ImageWidget ZYaw_Text;
    private ImageWidget XPitch_Text;
    private ImageWidget YRoll_Text;
    private EditBoxWidget X_Edit;
    private EditBoxWidget Y_Edit;
    private EditBoxWidget Z_Edit;
    private HorizontalSliderWidget X_Slider;
    private HorizontalSliderWidget Y_Slider;
    private HorizontalSliderWidget Z_Slider;
    private bool __processingInput;
    private PrinterView printerview;
    private ModelAdjustmentsDialog.EditMode editmode;

    public ModelAdjustmentsDialog(int ID, PrinterView printerview)
      : this(ID, printerview, null)
    {
    }

    public ModelAdjustmentsDialog(int ID, PrinterView printerview, Element2D parent)
      : base(ID, parent)
    {
      this.printerview = printerview;
      editmode = ModelAdjustmentsDialog.EditMode.Undefined;
    }

    public void Init(GUIHost host)
    {
      Sprite.texture_height_pixels = 1024;
      Sprite.texture_width_pixels = 1024;
      Sprite.pixel_perfect = true;
      Init(host, "guicontrols", 896f, 384f, 1023f, 511f, 64, 32, 64, 16, 16, 64);
      WrapOnNegativeY = false;
      X_Text = new ImageWidget(0);
      X_Text.SetPosition(6, 7);
      X_Text.SetSize(56, 56);
      X_Text.Init(host, "guicontrols", 64f, 192f, 119f, 247f, 64f, 192f, 119f, 247f, 64f, 192f, 119f, 247f);
      X_Text.Text = "";
      AddChildElement(X_Text);
      Y_Text = new ImageWidget(0);
      Y_Text.SetPosition(6, 63);
      Y_Text.SetSize(56, 56);
      Y_Text.Init(host, "guicontrols", 192f, 192f, 247f, 247f, 192f, 192f, 247f, 247f, 192f, 192f, 247f, 247f);
      Y_Text.Text = "";
      AddChildElement(Y_Text);
      Z_Text = new ImageWidget(0);
      Z_Text.SetPosition(6, 119);
      Z_Text.SetSize(56, 56);
      Z_Text.Init(host, "guicontrols", 320f, 192f, 375f, 247f, 320f, 192f, 375f, 247f, 320f, 192f, 375f, 247f);
      Z_Text.Text = "";
      AddChildElement(Z_Text);
      XPitch_Text = new ImageWidget(0);
      XPitch_Text.SetPosition(6, 7);
      XPitch_Text.SetSize(56, 56);
      XPitch_Text.Init(host, "guicontrols", 0.0f, 192f, 55f, 247f, 0.0f, 192f, 55f, 247f, 0.0f, 192f, 55f, 247f);
      XPitch_Text.Text = "";
      XPitch_Text.Visible = true;
      AddChildElement(XPitch_Text);
      YRoll_Text = new ImageWidget(0);
      YRoll_Text.SetPosition(6, 63);
      YRoll_Text.SetSize(56, 56);
      YRoll_Text.Init(host, "guicontrols", 128f, 192f, 183f, 247f, 128f, 192f, 183f, 247f, 128f, 192f, 183f, 247f);
      YRoll_Text.Text = "";
      YRoll_Text.Visible = true;
      AddChildElement(YRoll_Text);
      ZYaw_Text = new ImageWidget(0);
      ZYaw_Text.SetPosition(6, 119);
      ZYaw_Text.SetSize(56, 56);
      ZYaw_Text.Init(host, "guicontrols", 256f, 192f, 311f, 247f, 256f, 192f, 311f, 247f, 256f, 192f, 311f, 247f);
      ZYaw_Text.Text = "";
      ZYaw_Text.Visible = true;
      AddChildElement(ZYaw_Text);
      X_Edit = new EditBoxWidget(8006);
      X_Edit.Init(host, "guicontrols", 384f, 192f, 447f, 247f);
      X_Edit.SetTextWindowBorders(6, 6, 19, 17);
      X_Edit.SetGrowableWidth(40, 16, 68);
      X_Edit.Size = FontSize.Medium;
      X_Edit.Color = new Color4(0.51f, 0.51f, 0.51f, 1f);
      X_Edit.Hint = "";
      X_Edit.Text = "0.0";
      X_Edit.SetPosition(51, 7);
      X_Edit.SetSize(68, 56);
      X_Edit.tabIndex = 1;
      AddChildElement(X_Edit);
      Y_Edit = new EditBoxWidget(8007);
      Y_Edit.Init(host, "guicontrols", 384f, 192f, 447f, 247f);
      Y_Edit.SetTextWindowBorders(6, 6, 19, 17);
      Y_Edit.SetGrowableWidth(40, 16, 68);
      Y_Edit.Size = FontSize.Medium;
      Y_Edit.Color = new Color4(0.51f, 0.51f, 0.51f, 1f);
      Y_Edit.Hint = "";
      Y_Edit.Text = "0.0";
      Y_Edit.SetPosition(51, 63);
      Y_Edit.SetSize(68, 56);
      Y_Edit.tabIndex = 2;
      AddChildElement(Y_Edit);
      Z_Edit = new EditBoxWidget(8008);
      Z_Edit.Init(host, "guicontrols", 384f, 192f, 447f, 247f);
      Z_Edit.SetTextWindowBorders(6, 6, 19, 17);
      Z_Edit.SetGrowableWidth(40, 16, 68);
      Z_Edit.Size = FontSize.Medium;
      Z_Edit.Color = new Color4(0.51f, 0.51f, 0.51f, 1f);
      Z_Edit.Hint = "";
      Z_Edit.Text = "0.0";
      Z_Edit.SetPosition(51, 119);
      Z_Edit.SetSize(68, 56);
      Z_Edit.tabIndex = 3;
      AddChildElement(Z_Edit);
      Sprite.pixel_perfect = false;
      top_bracket_image = new ImageWidget(0);
      top_bracket_image.Init(host, "extendedcontrols3", 0.0f, 0.0f, 22f, 31f, 0.0f, 0.0f, 22f, 31f, 0.0f, 0.0f, 22f, 31f);
      top_bracket_image.SetSize(22, 30);
      top_bracket_image.SetPosition(131, 33);
      top_bracket_image.Visible = true;
      AddChildElement(top_bracket_image);
      link_image = new ImageWidget(1);
      link_image.Init(host, "extendedcontrols3", 24f, 0.0f, 48f, 9f, 0.0f, 0.0f, 48f, 9f, 0.0f, 0.0f, 48f, 9f, 24f, 12f, 48f, 20f);
      link_image.SetSize(20, 10);
      link_image.SetPosition(124, 69);
      link_image.Visible = false;
      AddChildElement(link_image);
      linkScaling_button = new ButtonWidget(8015);
      linkScaling_button.Init(host, "guicontrols", 640f, 448f, 671f, 479f, 672f, 448f, 703f, 479f, 640f, 480f, 671f, 511f, 672f, 480f, 703f, 511f);
      linkScaling_button.Size = FontSize.Small;
      linkScaling_button.Text = "";
      linkScaling_button.SetGrowableWidth(5, 5, 16);
      linkScaling_button.SetGrowableHeight(5, 5, 16);
      linkScaling_button.SetSize(24, 24);
      linkScaling_button.SetPosition(122, 82);
      linkScaling_button.SetCallback(new ButtonCallback(MyButtonCallback));
      linkScaling_button.DontMove = true;
      linkScaling_button.ClickType = ButtonType.Checkable;
      linkScaling_button.CanClickOff = true;
      linkScaling_button.Checked = true;
      linkScaling_button.Visible = false;
      linkScaling_button.ImageHasFocusColor = new Color4(100, 230, byte.MaxValue, byte.MaxValue);
      linkScaling_button.tabIndex = 4;
      AddChildElement(linkScaling_button);
      middle_bracket_image = new ImageWidget(2);
      middle_bracket_image.Init(host, "extendedcontrols3", 5f, 0.0f, 22f, 5f, 0.0f, 0.0f, 22f, 31f, 0.0f, 0.0f, 22f, 31f);
      middle_bracket_image.SetSize(11, 5);
      middle_bracket_image.SetPosition(150, 88);
      middle_bracket_image.Visible = true;
      AddChildElement(middle_bracket_image);
      bottom_bracket_image = new ImageWidget(3);
      bottom_bracket_image.Init(host, "extendedcontrols3", 0.0f, 85f, 22f, 115f, 0.0f, 85f, 22f, 115f, 0.0f, 85f, 22f, 115f);
      bottom_bracket_image.SetSize(22, 35);
      bottom_bracket_image.SetPosition(131, 113);
      bottom_bracket_image.Visible = true;
      AddChildElement(bottom_bracket_image);
      X_Slider = new HorizontalSliderWidget(8009);
      X_Slider.InitTrack(host, "guicontrols", 809f, 72f, 831f, 95f, 4, 24);
      X_Slider.InitButton(host, "guicontrols", 808f, 0.0f, 831f, 23f, 808f, 24f, 831f, 47f, 808f, 48f, 831f, 71f, 4, 4, 24);
      X_Slider.InitMinus(host, "guicontrols", 736f, 0.0f, 759f, 23f, 760f, 0.0f, 783f, 23f, 784f, 0.0f, 808f, 23f);
      X_Slider.InitPlus(host, "guicontrols", 736f, 24f, 759f, 47f, 760f, 24f, 783f, 47f, 784f, 24f, 808f, 47f);
      X_Slider.SetButtonSize(24f);
      X_Slider.ShowPushButtons = true;
      X_Slider.SetSize(167, 24);
      X_Slider.SetPosition(155, 23);
      X_Slider.SetRange(-360f, 360f);
      X_Slider.PushButtonStep = 15f;
      X_Slider.SetTrackPosition(0.0f);
      AddChildElement(X_Slider);
      Y_Slider = new HorizontalSliderWidget(8010);
      Y_Slider.InitTrack(host, "guicontrols", 809f, 72f, 831f, 95f, 4, 24);
      Y_Slider.InitButton(host, "guicontrols", 904f, 0.0f, 927f, 23f, 904f, 24f, 927f, 47f, 904f, 48f, 927f, 71f, 4, 4, 24);
      Y_Slider.InitMinus(host, "guicontrols", 832f, 0.0f, 855f, 23f, 856f, 0.0f, 879f, 23f, 880f, 0.0f, 904f, 23f);
      Y_Slider.InitPlus(host, "guicontrols", 832f, 24f, 855f, 47f, 856f, 24f, 879f, 47f, 880f, 24f, 904f, 47f);
      Y_Slider.SetButtonSize(24f);
      Y_Slider.ShowPushButtons = true;
      Y_Slider.SetSize(167, 24);
      Y_Slider.SetPosition(155, 78);
      Y_Slider.SetRange(-360f, 360f);
      Y_Slider.PushButtonStep = 15f;
      Y_Slider.SetTrackPosition(0.0f);
      AddChildElement(Y_Slider);
      Z_Slider = new HorizontalSliderWidget(8011);
      Z_Slider.InitTrack(host, "guicontrols", 809f, 72f, 831f, 95f, 4, 24);
      Z_Slider.InitButton(host, "guicontrols", 1000f, 0.0f, 1023f, 23f, 1000f, 24f, 1023f, 47f, 1000f, 48f, 1023f, 71f, 4, 4, 24);
      Z_Slider.InitMinus(host, "guicontrols", 928f, 0.0f, 951f, 23f, 952f, 0.0f, 975f, 23f, 976f, 0.0f, 999f, 23f);
      Z_Slider.InitPlus(host, "guicontrols", 928f, 24f, 951f, 47f, 952f, 24f, 975f, 47f, 976f, 24f, 999f, 47f);
      Z_Slider.SetButtonSize(24f);
      Z_Slider.ShowPushButtons = true;
      Z_Slider.SetSize(167, 24);
      Z_Slider.SetPosition(155, 133);
      Z_Slider.SetRange(-360f, 360f);
      Z_Slider.PushButtonStep = 15f;
      Z_Slider.SetTrackPosition(0.0f);
      AddChildElement(Z_Slider);
      Sprite.pixel_perfect = true;
      Sprite.texture_height_pixels = 1024;
      Sprite.texture_width_pixels = 1024;
      Sprite.pixel_perfect = false;
    }

    public override void OnControlMsg(Element2D the_control, ControlMsg msg, float xparam, float yparam)
    {
      if (__processingInput)
      {
        return;
      }

      __processingInput = true;
      if (msg == ControlMsg.ENTERHIT)
      {
        if (the_control.ID == 8006)
        {
          if (float.TryParse(X_Edit.Text, out var result))
          {
            if (linkScaling_button.Checked && editmode == ModelAdjustmentsDialog.EditMode.Scale)
            {
              LinkedScaleX(result);
              enter_hit = true;
            }
            else
            {
              X_Slider.TrackPosition = result;
            }
          }
        }
        else if (the_control.ID == 8007)
        {
          if (float.TryParse(Y_Edit.Text, out var result))
          {
            if (linkScaling_button.Checked && editmode == ModelAdjustmentsDialog.EditMode.Scale)
            {
              LinkedScaleY(result);
              enter_hit = true;
            }
            else
            {
              Y_Slider.TrackPosition = result;
            }
          }
        }
        else if (the_control.ID == 8008)
        {
          if (float.TryParse(Z_Edit.Text, out var result))
          {
            if (linkScaling_button.Checked && editmode == ModelAdjustmentsDialog.EditMode.Scale)
            {
              LinkedScaleZ(result);
              enter_hit = true;
            }
            else
            {
              Z_Slider.TrackPosition = result;
            }
          }
        }
        printerview.ObjectTransformed = true;
      }
      if (msg == ControlMsg.SCROLL_MOVE)
      {
        if (the_control.ID == 8009)
        {
          if (linkScaling_button.Checked && editmode == ModelAdjustmentsDialog.EditMode.Scale)
          {
            LinkedScaleX(X_Slider.TrackPosition);
          }
          else
          {
            X_Edit.Text = X_Slider.TrackPosition.ToString("F2");
          }
        }
        else if (the_control.ID == 8010)
        {
          if (linkScaling_button.Checked && editmode == ModelAdjustmentsDialog.EditMode.Scale)
          {
            LinkedScaleY(Y_Slider.TrackPosition);
          }
          else
          {
            Y_Edit.Text = Y_Slider.TrackPosition.ToString("F2");
          }
        }
        else if (the_control.ID == 8011)
        {
          if (linkScaling_button.Checked && editmode == ModelAdjustmentsDialog.EditMode.Scale)
          {
            LinkedScaleZ(Z_Slider.TrackPosition);
          }
          else
          {
            Z_Edit.Text = Z_Slider.TrackPosition.ToString("F2");
          }
        }
        printerview.ObjectTransformed = true;
      }
      else
      {
        base.OnControlMsg(the_control, msg, xparam, yparam);
      }

      __processingInput = false;
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      if (button.ID != 8015)
      {
        return;
      }

      if (link_image != null)
      {
        if (!linkScaling_button.Checked)
        {
          link_image.Enabled = false;
        }
        else
        {
          link_image.Enabled = true;
        }
      }
      SaveScalingRatio();
    }

    public void UseScaleSliders()
    {
      EnableLinkedScalingControls(true);
      Height = 182;
      editmode = ModelAdjustmentsDialog.EditMode.Scale;
      X_Slider.Visible = true;
      X_Text.Visible = true;
      XPitch_Text.Visible = false;
      X_Slider.SetRange(printerview.MinScale.X, printerview.MaxScale.X);
      X_Slider.PushButtonStep = (float)((printerview.MaxScale.X - 0.100000001490116) / 20.0);
      if (X_Slider.PushButtonStep > 10.0)
      {
        X_Slider.PushButtonStep = 10f;
      }

      X_Slider.RoundingPlace = 2;
      Y_Slider.Visible = true;
      Y_Text.Visible = true;
      YRoll_Text.Visible = false;
      Y_Slider.SetRange(printerview.MinScale.Y, printerview.MaxScale.Y);
      Y_Slider.PushButtonStep = (float)((printerview.MaxScale.Y - 0.100000001490116) / 20.0);
      if (Y_Slider.PushButtonStep > 10.0)
      {
        Y_Slider.PushButtonStep = 10f;
      }

      Y_Slider.RoundingPlace = 2;
      Z_Slider.Visible = true;
      Z_Text.Visible = true;
      ZYaw_Text.Visible = false;
      Z_Slider.SetRange(printerview.MinScale.Z, printerview.MaxScale.Z);
      Z_Slider.PushButtonStep = (float)((printerview.MaxScale.Z - 0.100000001490116) / 20.0);
      if (Z_Slider.PushButtonStep > 10.0)
      {
        Z_Slider.PushButtonStep = 10f;
      }

      Z_Edit.Visible = true;
      Z_Slider.RoundingPlace = 2;
      TransformationNode modelTransformation = printerview.ModelTransformation;
      if (modelTransformation == null)
      {
        return;
      }

      X_Slider.TrackPosition = modelTransformation.Scale.X;
      Y_Slider.TrackPosition = modelTransformation.Scale.Y;
      Z_Slider.TrackPosition = modelTransformation.Scale.Z;
      printerview.ObjectTransformed = false;
    }

    public void UseTranslationSliders()
    {
      Height = 126;
      EnableLinkedScalingControls(false);
      editmode = ModelAdjustmentsDialog.EditMode.Translation;
      __processingInput = true;
      X_Slider.Visible = true;
      X_Text.Visible = true;
      XPitch_Text.Visible = false;
      X_Slider.SetRange(-200f, 200f);
      X_Slider.PushButtonStep = 10f;
      X_Slider.RoundingPlace = 2;
      Y_Slider.Visible = true;
      Y_Text.Visible = true;
      YRoll_Text.Visible = false;
      Y_Slider.SetRange(-200f, 200f);
      Y_Slider.PushButtonStep = 10f;
      Y_Slider.RoundingPlace = 2;
      Z_Slider.Visible = false;
      Z_Text.Visible = false;
      ZYaw_Text.Visible = false;
      Z_Edit.Visible = false;
      Z_Slider.RoundingPlace = 2;
      __processingInput = false;
      TransformationNode modelTransformation = printerview.ModelTransformation;
      if (modelTransformation == null)
      {
        return;
      }

      X_Slider.TrackPosition = modelTransformation.Translation.X;
      Y_Slider.TrackPosition = modelTransformation.Translation.Y;
      printerview.ObjectTransformed = false;
    }

    public void UseRotationSliders()
    {
      Height = 182;
      EnableLinkedScalingControls(false);
      editmode = ModelAdjustmentsDialog.EditMode.Rotation;
      X_Slider.Visible = true;
      X_Text.Visible = false;
      XPitch_Text.Visible = true;
      X_Slider.SetRange(-180f, 180f);
      X_Slider.PushButtonStep = 15f;
      X_Slider.RoundingPlace = 0;
      Y_Slider.Visible = true;
      Y_Text.Visible = false;
      YRoll_Text.Visible = true;
      Y_Slider.SetRange(-180f, 180f);
      Y_Slider.PushButtonStep = 15f;
      Y_Slider.RoundingPlace = 0;
      Z_Slider.Visible = true;
      Z_Text.Visible = false;
      ZYaw_Text.Visible = true;
      Z_Edit.Visible = true;
      Z_Slider.SetRange(-180f, 180f);
      Z_Slider.PushButtonStep = 15f;
      Z_Slider.RoundingPlace = 0;
      TransformationNode modelTransformation = printerview.ModelTransformation;
      if (modelTransformation == null)
      {
        return;
      }

      X_Slider.TrackPosition = modelTransformation.Rotation.X;
      Y_Slider.TrackPosition = modelTransformation.Rotation.Y;
      Z_Slider.TrackPosition = modelTransformation.Rotation.Z;
      printerview.ObjectTransformed = false;
    }

    public void Deactivate()
    {
      editmode = ModelAdjustmentsDialog.EditMode.Undefined;
    }

    public override void OnUpdate()
    {
      base.OnUpdate();
      if (!X_Edit.HasFocus || enter_hit)
      {
        if (float.TryParse(X_Edit.Text, out var result) && Math.Round(result, 2) != Math.Round(X_Slider.TrackPosition, 2))
        {
          X_Slider.TrackPosition = result;
          printerview.ObjectTransformed = true;
        }
      }
      if (!Y_Edit.HasFocus || enter_hit)
      {
        if (float.TryParse(Y_Edit.Text, out var result) && Math.Round(result, 2) != Math.Round(Y_Slider.TrackPosition, 2))
        {
          Y_Slider.TrackPosition = result;
          printerview.ObjectTransformed = true;
        }
      }
      if (!Z_Edit.HasFocus || enter_hit)
      {
        if (float.TryParse(Z_Edit.Text, out var result) && Math.Round(result, 2) != Math.Round(Z_Slider.TrackPosition, 2))
        {
          Z_Slider.TrackPosition = result;
          printerview.ObjectTransformed = true;
        }
      }
      enter_hit = false;
    }

    public void RefreshSliders()
    {
      if (editmode == ModelAdjustmentsDialog.EditMode.Rotation)
      {
        UseRotationSliders();
      }
      else if (editmode == ModelAdjustmentsDialog.EditMode.Scale)
      {
        UseScaleSliders();
      }
      else
      {
        if (editmode != ModelAdjustmentsDialog.EditMode.Translation)
        {
          return;
        }

        UseTranslationSliders();
      }
    }

    public void PushUpdatedInfomation(ModelTransformPair model)
    {
      if (editmode == ModelAdjustmentsDialog.EditMode.Rotation)
      {
        X_Slider.TrackPosition = model.transformNode.Rotation.X;
        Y_Slider.TrackPosition = model.transformNode.Rotation.Y;
        Z_Slider.TrackPosition = model.transformNode.Rotation.Z;
      }
      else if (editmode == ModelAdjustmentsDialog.EditMode.Scale)
      {
        X_Slider.TrackPosition = model.transformNode.Scale.X;
        Y_Slider.TrackPosition = model.transformNode.Scale.Y;
        Z_Slider.TrackPosition = model.transformNode.Scale.Z;
      }
      else
      {
        if (editmode != ModelAdjustmentsDialog.EditMode.Translation)
        {
          return;
        }

        X_Slider.TrackPosition = model.transformNode.Translation.X;
        Y_Slider.TrackPosition = model.transformNode.Translation.Y;
      }
    }

    public override void Refresh()
    {
      SaveCurrentSliderInfo();
      RefreshSliders();
      base.Refresh();
    }

    internal void SaveCurrentSliderInfo()
    {
      TransformationNode modelTransformation = printerview.ModelTransformation;
      if (modelTransformation == null)
      {
        return;
      }

      if (editmode == ModelAdjustmentsDialog.EditMode.Rotation)
      {
        modelTransformation.Rotation.X = X_Slider.TrackPosition;
        modelTransformation.Rotation.Y = Y_Slider.TrackPosition;
        modelTransformation.Rotation.Z = Z_Slider.TrackPosition;
      }
      else if (editmode == ModelAdjustmentsDialog.EditMode.Scale)
      {
        modelTransformation.Scale.X = X_Slider.TrackPosition;
        modelTransformation.Scale.Y = Y_Slider.TrackPosition;
        modelTransformation.Scale.Z = Z_Slider.TrackPosition;
      }
      else
      {
        if (editmode != ModelAdjustmentsDialog.EditMode.Translation)
        {
          return;
        }

        modelTransformation.Translation.X = X_Slider.TrackPosition;
        modelTransformation.Translation.Y = Y_Slider.TrackPosition;
      }
    }

    public void SetRotationValues(Vector3 rotation)
    {
      SetRotationValues(rotation.X, rotation.Y, rotation.Z);
    }

    public void SetRotationValues(float x, float y, float z)
    {
      if (printerview.SelectedModelTransformPair == null)
      {
        return;
      }

      if (editmode == ModelAdjustmentsDialog.EditMode.Rotation)
      {
        X_Slider.SetTrackPosition(x);
        Y_Slider.SetTrackPosition(y);
        Z_Slider.SetTrackPosition(z);
      }
      printerview.ObjectTransformed = true;
    }

    public void SetTranslationValues(Vector3 translation)
    {
      SetTranslationValues(translation.X, translation.Y);
    }

    public void SetTranslationValues(float x, float y)
    {
      if (printerview.SelectedModelTransformPair == null)
      {
        return;
      }

      if (editmode == ModelAdjustmentsDialog.EditMode.Translation)
      {
        X_Slider.SetTrackPosition(x);
        Y_Slider.SetTrackPosition(y);
      }
      printerview.ObjectTransformed = true;
    }

    public void SetScaleValues(Vector3 scale)
    {
      SetScaleValues(scale.X, scale.Y, scale.Z);
    }

    public void SetScaleValues(float x, float y, float z)
    {
      if (printerview.SelectedModelTransformPair == null)
      {
        return;
      }

      if (editmode == ModelAdjustmentsDialog.EditMode.Scale)
      {
        X_Slider.SetTrackPosition(x);
        Y_Slider.SetTrackPosition(y);
        Z_Slider.SetTrackPosition(z);
      }
      printerview.ObjectTransformed = true;
    }

    public void AdjustTranslationValues(float x, float y, float z)
    {
      if (editmode != ModelAdjustmentsDialog.EditMode.Translation)
      {
        return;
      }

      X_Slider.TrackPosition += x;
      Y_Slider.TrackPosition += y;
    }

    public void AdjustScaleValues(float x, float y, float z)
    {
      if (editmode != ModelAdjustmentsDialog.EditMode.Scale)
      {
        return;
      }

      X_Slider.TrackPosition += x;
      Y_Slider.TrackPosition += y;
      Z_Slider.TrackPosition += z;
    }

    public void AdjustRotationValues(float x, float y, float z)
    {
      if (editmode != ModelAdjustmentsDialog.EditMode.Rotation)
      {
        return;
      }

      X_Slider.TrackPosition += x;
      Y_Slider.TrackPosition += y;
      Z_Slider.TrackPosition += z;
    }

    public ModelAdjustmentsDialog.EditMode Mode
    {
      get
      {
        return editmode;
      }
    }

    public void CheckLinkedRescalingCheckbox(bool action)
    {
      if (linkScaling_button == null || linkScaling_button.Checked == action)
      {
        return;
      }

      linkScaling_button.Checked = action;
    }

    private void EnableLinkedScalingControls(bool action)
    {
      top_bracket_image.Visible = action;
      link_image.Visible = action;
      linkScaling_button.Visible = action;
      middle_bracket_image.Visible = action;
      bottom_bracket_image.Visible = action;
      SaveScalingRatio();
    }

    private void SaveScalingRatio()
    {
      TransformationNode modelTransformation = printerview.ModelTransformation;
      if (modelTransformation != null)
      {
        initialscalevalues.X = modelTransformation.Scale.X;
        initialscalevalues.Y = modelTransformation.Scale.Y;
        initialscalevalues.Z = modelTransformation.Scale.Z;
      }
      else
      {
        initialscalevalues.X = 1f;
        initialscalevalues.Y = 1f;
        initialscalevalues.Z = 1f;
      }
    }

    private void LinkedScaleX(float value)
    {
      var fPosition1 = value;
      var fPosition2 = fPosition1 * (initialscalevalues.Y / initialscalevalues.X);
      var fPosition3 = fPosition1 * (initialscalevalues.Z / initialscalevalues.X);
      X_Slider.SetTrackPositionNoCallBack(fPosition1);
      Y_Slider.SetTrackPositionNoCallBack(fPosition2);
      Z_Slider.SetTrackPositionNoCallBack(fPosition3);
      X_Edit.Text = fPosition1.ToString("F2");
      Y_Edit.Text = fPosition2.ToString("F2");
      Z_Edit.Text = fPosition3.ToString("F2");
    }

    private void LinkedScaleY(float value)
    {
      var fPosition1 = value;
      var fPosition2 = fPosition1 * (initialscalevalues.X / initialscalevalues.Y);
      var fPosition3 = fPosition1 * (initialscalevalues.Z / initialscalevalues.Y);
      X_Slider.SetTrackPositionNoCallBack(fPosition2);
      Y_Slider.SetTrackPositionNoCallBack(fPosition1);
      Z_Slider.SetTrackPositionNoCallBack(fPosition3);
      X_Edit.Text = fPosition2.ToString("F2");
      Y_Edit.Text = fPosition1.ToString("F2");
      Z_Edit.Text = fPosition3.ToString("F2");
    }

    private void LinkedScaleZ(float value)
    {
      var fPosition1 = value;
      var fPosition2 = fPosition1 * (initialscalevalues.X / initialscalevalues.Z);
      var fPosition3 = fPosition1 * (initialscalevalues.Y / initialscalevalues.Z);
      X_Slider.SetTrackPositionNoCallBack(fPosition2);
      Y_Slider.SetTrackPositionNoCallBack(fPosition3);
      Z_Slider.SetTrackPositionNoCallBack(fPosition1);
      X_Edit.Text = fPosition2.ToString("F2");
      Y_Edit.Text = fPosition3.ToString("F2");
      Z_Edit.Text = fPosition1.ToString("F2");
    }

    public enum ControlIDs
    {
      Static = 0,
      X_Edit = 8006, // 0x00001F46
      Y_Edit = 8007, // 0x00001F47
      Z_Edit = 8008, // 0x00001F48
      X_Slider = 8009, // 0x00001F49
      Y_Slider = 8010, // 0x00001F4A
      Z_Slider = 8011, // 0x00001F4B
      translation_button = 8012, // 0x00001F4C
      rotation_button = 8013, // 0x00001F4D
      scalingbutton = 8014, // 0x00001F4E
      LinkScaling_Button = 8015, // 0x00001F4F
    }

    public enum EditMode
    {
      Undefined,
      Translation,
      Scale,
      Rotation,
    }
  }
}
