// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.ModelAdjustmentsDialog
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      : this(ID, printerview, (Element2D) null)
    {
    }

    public ModelAdjustmentsDialog(int ID, PrinterView printerview, Element2D parent)
      : base(ID, parent)
    {
      this.printerview = printerview;
      this.editmode = ModelAdjustmentsDialog.EditMode.Undefined;
    }

    public void Init(GUIHost host)
    {
      Sprite.texture_height_pixels = 1024;
      Sprite.texture_width_pixels = 1024;
      Sprite.pixel_perfect = true;
      this.Init(host, "guicontrols", 896f, 384f, 1023f, 511f, 64, 32, 64, 16, 16, 64);
      this.WrapOnNegativeY = false;
      this.X_Text = new ImageWidget(0);
      this.X_Text.SetPosition(6, 7);
      this.X_Text.SetSize(56, 56);
      this.X_Text.Init(host, "guicontrols", 64f, 192f, 119f, 247f, 64f, 192f, 119f, 247f, 64f, 192f, 119f, 247f);
      this.X_Text.Text = "";
      this.AddChildElement((Element2D) this.X_Text);
      this.Y_Text = new ImageWidget(0);
      this.Y_Text.SetPosition(6, 63);
      this.Y_Text.SetSize(56, 56);
      this.Y_Text.Init(host, "guicontrols", 192f, 192f, 247f, 247f, 192f, 192f, 247f, 247f, 192f, 192f, 247f, 247f);
      this.Y_Text.Text = "";
      this.AddChildElement((Element2D) this.Y_Text);
      this.Z_Text = new ImageWidget(0);
      this.Z_Text.SetPosition(6, 119);
      this.Z_Text.SetSize(56, 56);
      this.Z_Text.Init(host, "guicontrols", 320f, 192f, 375f, 247f, 320f, 192f, 375f, 247f, 320f, 192f, 375f, 247f);
      this.Z_Text.Text = "";
      this.AddChildElement((Element2D) this.Z_Text);
      this.XPitch_Text = new ImageWidget(0);
      this.XPitch_Text.SetPosition(6, 7);
      this.XPitch_Text.SetSize(56, 56);
      this.XPitch_Text.Init(host, "guicontrols", 0.0f, 192f, 55f, 247f, 0.0f, 192f, 55f, 247f, 0.0f, 192f, 55f, 247f);
      this.XPitch_Text.Text = "";
      this.XPitch_Text.Visible = true;
      this.AddChildElement((Element2D) this.XPitch_Text);
      this.YRoll_Text = new ImageWidget(0);
      this.YRoll_Text.SetPosition(6, 63);
      this.YRoll_Text.SetSize(56, 56);
      this.YRoll_Text.Init(host, "guicontrols", 128f, 192f, 183f, 247f, 128f, 192f, 183f, 247f, 128f, 192f, 183f, 247f);
      this.YRoll_Text.Text = "";
      this.YRoll_Text.Visible = true;
      this.AddChildElement((Element2D) this.YRoll_Text);
      this.ZYaw_Text = new ImageWidget(0);
      this.ZYaw_Text.SetPosition(6, 119);
      this.ZYaw_Text.SetSize(56, 56);
      this.ZYaw_Text.Init(host, "guicontrols", 256f, 192f, 311f, 247f, 256f, 192f, 311f, 247f, 256f, 192f, 311f, 247f);
      this.ZYaw_Text.Text = "";
      this.ZYaw_Text.Visible = true;
      this.AddChildElement((Element2D) this.ZYaw_Text);
      this.X_Edit = new EditBoxWidget(8006);
      this.X_Edit.Init(host, "guicontrols", 384f, 192f, 447f, 247f);
      this.X_Edit.SetTextWindowBorders(6, 6, 19, 17);
      this.X_Edit.SetGrowableWidth(40, 16, 68);
      this.X_Edit.Size = FontSize.Medium;
      this.X_Edit.Color = new Color4(0.51f, 0.51f, 0.51f, 1f);
      this.X_Edit.Hint = "";
      this.X_Edit.Text = "0.0";
      this.X_Edit.SetPosition(51, 7);
      this.X_Edit.SetSize(68, 56);
      this.X_Edit.tabIndex = 1;
      this.AddChildElement((Element2D) this.X_Edit);
      this.Y_Edit = new EditBoxWidget(8007);
      this.Y_Edit.Init(host, "guicontrols", 384f, 192f, 447f, 247f);
      this.Y_Edit.SetTextWindowBorders(6, 6, 19, 17);
      this.Y_Edit.SetGrowableWidth(40, 16, 68);
      this.Y_Edit.Size = FontSize.Medium;
      this.Y_Edit.Color = new Color4(0.51f, 0.51f, 0.51f, 1f);
      this.Y_Edit.Hint = "";
      this.Y_Edit.Text = "0.0";
      this.Y_Edit.SetPosition(51, 63);
      this.Y_Edit.SetSize(68, 56);
      this.Y_Edit.tabIndex = 2;
      this.AddChildElement((Element2D) this.Y_Edit);
      this.Z_Edit = new EditBoxWidget(8008);
      this.Z_Edit.Init(host, "guicontrols", 384f, 192f, 447f, 247f);
      this.Z_Edit.SetTextWindowBorders(6, 6, 19, 17);
      this.Z_Edit.SetGrowableWidth(40, 16, 68);
      this.Z_Edit.Size = FontSize.Medium;
      this.Z_Edit.Color = new Color4(0.51f, 0.51f, 0.51f, 1f);
      this.Z_Edit.Hint = "";
      this.Z_Edit.Text = "0.0";
      this.Z_Edit.SetPosition(51, 119);
      this.Z_Edit.SetSize(68, 56);
      this.Z_Edit.tabIndex = 3;
      this.AddChildElement((Element2D) this.Z_Edit);
      Sprite.pixel_perfect = false;
      this.top_bracket_image = new ImageWidget(0);
      this.top_bracket_image.Init(host, "extendedcontrols3", 0.0f, 0.0f, 22f, 31f, 0.0f, 0.0f, 22f, 31f, 0.0f, 0.0f, 22f, 31f);
      this.top_bracket_image.SetSize(22, 30);
      this.top_bracket_image.SetPosition(131, 33);
      this.top_bracket_image.Visible = true;
      this.AddChildElement((Element2D) this.top_bracket_image);
      this.link_image = new ImageWidget(1);
      this.link_image.Init(host, "extendedcontrols3", 24f, 0.0f, 48f, 9f, 0.0f, 0.0f, 48f, 9f, 0.0f, 0.0f, 48f, 9f, 24f, 12f, 48f, 20f);
      this.link_image.SetSize(20, 10);
      this.link_image.SetPosition(124, 69);
      this.link_image.Visible = false;
      this.AddChildElement((Element2D) this.link_image);
      this.linkScaling_button = new ButtonWidget(8015);
      this.linkScaling_button.Init(host, "guicontrols", 640f, 448f, 671f, 479f, 672f, 448f, 703f, 479f, 640f, 480f, 671f, 511f, 672f, 480f, 703f, 511f);
      this.linkScaling_button.Size = FontSize.Small;
      this.linkScaling_button.Text = "";
      this.linkScaling_button.SetGrowableWidth(5, 5, 16);
      this.linkScaling_button.SetGrowableHeight(5, 5, 16);
      this.linkScaling_button.SetSize(24, 24);
      this.linkScaling_button.SetPosition(122, 82);
      this.linkScaling_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.linkScaling_button.DontMove = true;
      this.linkScaling_button.ClickType = ButtonType.Checkable;
      this.linkScaling_button.CanClickOff = true;
      this.linkScaling_button.Checked = true;
      this.linkScaling_button.Visible = false;
      this.linkScaling_button.ImageHasFocusColor = new Color4((byte) 100, (byte) 230, byte.MaxValue, byte.MaxValue);
      this.linkScaling_button.tabIndex = 4;
      this.AddChildElement((Element2D) this.linkScaling_button);
      this.middle_bracket_image = new ImageWidget(2);
      this.middle_bracket_image.Init(host, "extendedcontrols3", 5f, 0.0f, 22f, 5f, 0.0f, 0.0f, 22f, 31f, 0.0f, 0.0f, 22f, 31f);
      this.middle_bracket_image.SetSize(11, 5);
      this.middle_bracket_image.SetPosition(150, 88);
      this.middle_bracket_image.Visible = true;
      this.AddChildElement((Element2D) this.middle_bracket_image);
      this.bottom_bracket_image = new ImageWidget(3);
      this.bottom_bracket_image.Init(host, "extendedcontrols3", 0.0f, 85f, 22f, 115f, 0.0f, 85f, 22f, 115f, 0.0f, 85f, 22f, 115f);
      this.bottom_bracket_image.SetSize(22, 35);
      this.bottom_bracket_image.SetPosition(131, 113);
      this.bottom_bracket_image.Visible = true;
      this.AddChildElement((Element2D) this.bottom_bracket_image);
      this.X_Slider = new HorizontalSliderWidget(8009);
      this.X_Slider.InitTrack(host, "guicontrols", 809f, 72f, 831f, 95f, 4, 24);
      this.X_Slider.InitButton(host, "guicontrols", 808f, 0.0f, 831f, 23f, 808f, 24f, 831f, 47f, 808f, 48f, 831f, 71f, 4, 4, 24);
      this.X_Slider.InitMinus(host, "guicontrols", 736f, 0.0f, 759f, 23f, 760f, 0.0f, 783f, 23f, 784f, 0.0f, 808f, 23f);
      this.X_Slider.InitPlus(host, "guicontrols", 736f, 24f, 759f, 47f, 760f, 24f, 783f, 47f, 784f, 24f, 808f, 47f);
      this.X_Slider.SetButtonSize(24f);
      this.X_Slider.ShowPushButtons = true;
      this.X_Slider.SetSize(167, 24);
      this.X_Slider.SetPosition(155, 23);
      this.X_Slider.SetRange(-360f, 360f);
      this.X_Slider.PushButtonStep = 15f;
      this.X_Slider.SetTrackPosition(0.0f);
      this.AddChildElement((Element2D) this.X_Slider);
      this.Y_Slider = new HorizontalSliderWidget(8010);
      this.Y_Slider.InitTrack(host, "guicontrols", 809f, 72f, 831f, 95f, 4, 24);
      this.Y_Slider.InitButton(host, "guicontrols", 904f, 0.0f, 927f, 23f, 904f, 24f, 927f, 47f, 904f, 48f, 927f, 71f, 4, 4, 24);
      this.Y_Slider.InitMinus(host, "guicontrols", 832f, 0.0f, 855f, 23f, 856f, 0.0f, 879f, 23f, 880f, 0.0f, 904f, 23f);
      this.Y_Slider.InitPlus(host, "guicontrols", 832f, 24f, 855f, 47f, 856f, 24f, 879f, 47f, 880f, 24f, 904f, 47f);
      this.Y_Slider.SetButtonSize(24f);
      this.Y_Slider.ShowPushButtons = true;
      this.Y_Slider.SetSize(167, 24);
      this.Y_Slider.SetPosition(155, 78);
      this.Y_Slider.SetRange(-360f, 360f);
      this.Y_Slider.PushButtonStep = 15f;
      this.Y_Slider.SetTrackPosition(0.0f);
      this.AddChildElement((Element2D) this.Y_Slider);
      this.Z_Slider = new HorizontalSliderWidget(8011);
      this.Z_Slider.InitTrack(host, "guicontrols", 809f, 72f, 831f, 95f, 4, 24);
      this.Z_Slider.InitButton(host, "guicontrols", 1000f, 0.0f, 1023f, 23f, 1000f, 24f, 1023f, 47f, 1000f, 48f, 1023f, 71f, 4, 4, 24);
      this.Z_Slider.InitMinus(host, "guicontrols", 928f, 0.0f, 951f, 23f, 952f, 0.0f, 975f, 23f, 976f, 0.0f, 999f, 23f);
      this.Z_Slider.InitPlus(host, "guicontrols", 928f, 24f, 951f, 47f, 952f, 24f, 975f, 47f, 976f, 24f, 999f, 47f);
      this.Z_Slider.SetButtonSize(24f);
      this.Z_Slider.ShowPushButtons = true;
      this.Z_Slider.SetSize(167, 24);
      this.Z_Slider.SetPosition(155, 133);
      this.Z_Slider.SetRange(-360f, 360f);
      this.Z_Slider.PushButtonStep = 15f;
      this.Z_Slider.SetTrackPosition(0.0f);
      this.AddChildElement((Element2D) this.Z_Slider);
      Sprite.pixel_perfect = true;
      Sprite.texture_height_pixels = 1024;
      Sprite.texture_width_pixels = 1024;
      Sprite.pixel_perfect = false;
    }

    public override void OnControlMsg(Element2D the_control, ControlMsg msg, float xparam, float yparam)
    {
      if (this.__processingInput)
        return;
      this.__processingInput = true;
      if (msg == ControlMsg.ENTERHIT)
      {
        if (the_control.ID == 8006)
        {
          float result = 0.0f;
          if (float.TryParse(this.X_Edit.Text, out result))
          {
            if (this.linkScaling_button.Checked && this.editmode == ModelAdjustmentsDialog.EditMode.Scale)
            {
              this.LinkedScaleX(result);
              this.enter_hit = true;
            }
            else
              this.X_Slider.TrackPosition = result;
          }
        }
        else if (the_control.ID == 8007)
        {
          float result = 0.0f;
          if (float.TryParse(this.Y_Edit.Text, out result))
          {
            if (this.linkScaling_button.Checked && this.editmode == ModelAdjustmentsDialog.EditMode.Scale)
            {
              this.LinkedScaleY(result);
              this.enter_hit = true;
            }
            else
              this.Y_Slider.TrackPosition = result;
          }
        }
        else if (the_control.ID == 8008)
        {
          float result = 0.0f;
          if (float.TryParse(this.Z_Edit.Text, out result))
          {
            if (this.linkScaling_button.Checked && this.editmode == ModelAdjustmentsDialog.EditMode.Scale)
            {
              this.LinkedScaleZ(result);
              this.enter_hit = true;
            }
            else
              this.Z_Slider.TrackPosition = result;
          }
        }
        this.printerview.ObjectTransformed = true;
      }
      if (msg == ControlMsg.SCROLL_MOVE)
      {
        if (the_control.ID == 8009)
        {
          if (this.linkScaling_button.Checked && this.editmode == ModelAdjustmentsDialog.EditMode.Scale)
            this.LinkedScaleX(this.X_Slider.TrackPosition);
          else
            this.X_Edit.Text = this.X_Slider.TrackPosition.ToString("F2");
        }
        else if (the_control.ID == 8010)
        {
          if (this.linkScaling_button.Checked && this.editmode == ModelAdjustmentsDialog.EditMode.Scale)
            this.LinkedScaleY(this.Y_Slider.TrackPosition);
          else
            this.Y_Edit.Text = this.Y_Slider.TrackPosition.ToString("F2");
        }
        else if (the_control.ID == 8011)
        {
          if (this.linkScaling_button.Checked && this.editmode == ModelAdjustmentsDialog.EditMode.Scale)
            this.LinkedScaleZ(this.Z_Slider.TrackPosition);
          else
            this.Z_Edit.Text = this.Z_Slider.TrackPosition.ToString("F2");
        }
        this.printerview.ObjectTransformed = true;
      }
      else
        base.OnControlMsg(the_control, msg, xparam, yparam);
      this.__processingInput = false;
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      if (button.ID != 8015)
        return;
      if (this.link_image != null)
      {
        if (!this.linkScaling_button.Checked)
          this.link_image.Enabled = false;
        else
          this.link_image.Enabled = true;
      }
      this.SaveScalingRatio();
    }

    public void UseScaleSliders()
    {
      this.EnableLinkedScalingControls(true);
      this.Height = 182;
      this.editmode = ModelAdjustmentsDialog.EditMode.Scale;
      this.X_Slider.Visible = true;
      this.X_Text.Visible = true;
      this.XPitch_Text.Visible = false;
      this.X_Slider.SetRange(this.printerview.MinScale.x, this.printerview.MaxScale.x);
      this.X_Slider.PushButtonStep = (float) (((double) this.printerview.MaxScale.x - 0.100000001490116) / 20.0);
      if ((double) this.X_Slider.PushButtonStep > 10.0)
        this.X_Slider.PushButtonStep = 10f;
      this.X_Slider.RoundingPlace = 2;
      this.Y_Slider.Visible = true;
      this.Y_Text.Visible = true;
      this.YRoll_Text.Visible = false;
      this.Y_Slider.SetRange(this.printerview.MinScale.y, this.printerview.MaxScale.y);
      this.Y_Slider.PushButtonStep = (float) (((double) this.printerview.MaxScale.y - 0.100000001490116) / 20.0);
      if ((double) this.Y_Slider.PushButtonStep > 10.0)
        this.Y_Slider.PushButtonStep = 10f;
      this.Y_Slider.RoundingPlace = 2;
      this.Z_Slider.Visible = true;
      this.Z_Text.Visible = true;
      this.ZYaw_Text.Visible = false;
      this.Z_Slider.SetRange(this.printerview.MinScale.z, this.printerview.MaxScale.z);
      this.Z_Slider.PushButtonStep = (float) (((double) this.printerview.MaxScale.z - 0.100000001490116) / 20.0);
      if ((double) this.Z_Slider.PushButtonStep > 10.0)
        this.Z_Slider.PushButtonStep = 10f;
      this.Z_Edit.Visible = true;
      this.Z_Slider.RoundingPlace = 2;
      TransformationNode modelTransformation = this.printerview.ModelTransformation;
      if (modelTransformation == null)
        return;
      this.X_Slider.TrackPosition = modelTransformation.Scale.x;
      this.Y_Slider.TrackPosition = modelTransformation.Scale.y;
      this.Z_Slider.TrackPosition = modelTransformation.Scale.z;
      this.printerview.ObjectTransformed = false;
    }

    public void UseTranslationSliders()
    {
      this.Height = 126;
      this.EnableLinkedScalingControls(false);
      this.editmode = ModelAdjustmentsDialog.EditMode.Translation;
      this.__processingInput = true;
      this.X_Slider.Visible = true;
      this.X_Text.Visible = true;
      this.XPitch_Text.Visible = false;
      this.X_Slider.SetRange(-200f, 200f);
      this.X_Slider.PushButtonStep = 10f;
      this.X_Slider.RoundingPlace = 2;
      this.Y_Slider.Visible = true;
      this.Y_Text.Visible = true;
      this.YRoll_Text.Visible = false;
      this.Y_Slider.SetRange(-200f, 200f);
      this.Y_Slider.PushButtonStep = 10f;
      this.Y_Slider.RoundingPlace = 2;
      this.Z_Slider.Visible = false;
      this.Z_Text.Visible = false;
      this.ZYaw_Text.Visible = false;
      this.Z_Edit.Visible = false;
      this.Z_Slider.RoundingPlace = 2;
      this.__processingInput = false;
      TransformationNode modelTransformation = this.printerview.ModelTransformation;
      if (modelTransformation == null)
        return;
      this.X_Slider.TrackPosition = modelTransformation.Translation.x;
      this.Y_Slider.TrackPosition = modelTransformation.Translation.y;
      this.printerview.ObjectTransformed = false;
    }

    public void UseRotationSliders()
    {
      this.Height = 182;
      this.EnableLinkedScalingControls(false);
      this.editmode = ModelAdjustmentsDialog.EditMode.Rotation;
      this.X_Slider.Visible = true;
      this.X_Text.Visible = false;
      this.XPitch_Text.Visible = true;
      this.X_Slider.SetRange(-180f, 180f);
      this.X_Slider.PushButtonStep = 15f;
      this.X_Slider.RoundingPlace = 0;
      this.Y_Slider.Visible = true;
      this.Y_Text.Visible = false;
      this.YRoll_Text.Visible = true;
      this.Y_Slider.SetRange(-180f, 180f);
      this.Y_Slider.PushButtonStep = 15f;
      this.Y_Slider.RoundingPlace = 0;
      this.Z_Slider.Visible = true;
      this.Z_Text.Visible = false;
      this.ZYaw_Text.Visible = true;
      this.Z_Edit.Visible = true;
      this.Z_Slider.SetRange(-180f, 180f);
      this.Z_Slider.PushButtonStep = 15f;
      this.Z_Slider.RoundingPlace = 0;
      TransformationNode modelTransformation = this.printerview.ModelTransformation;
      if (modelTransformation == null)
        return;
      this.X_Slider.TrackPosition = modelTransformation.Rotation.x;
      this.Y_Slider.TrackPosition = modelTransformation.Rotation.y;
      this.Z_Slider.TrackPosition = modelTransformation.Rotation.z;
      this.printerview.ObjectTransformed = false;
    }

    public void Deactivate()
    {
      this.editmode = ModelAdjustmentsDialog.EditMode.Undefined;
    }

    public override void OnUpdate()
    {
      base.OnUpdate();
      if (!this.X_Edit.HasFocus || this.enter_hit)
      {
        float result = 0.0f;
        if (float.TryParse(this.X_Edit.Text, out result) && Math.Round((double) result, 2) != Math.Round((double) this.X_Slider.TrackPosition, 2))
        {
          this.X_Slider.TrackPosition = result;
          this.printerview.ObjectTransformed = true;
        }
      }
      if (!this.Y_Edit.HasFocus || this.enter_hit)
      {
        float result = 0.0f;
        if (float.TryParse(this.Y_Edit.Text, out result) && Math.Round((double) result, 2) != Math.Round((double) this.Y_Slider.TrackPosition, 2))
        {
          this.Y_Slider.TrackPosition = result;
          this.printerview.ObjectTransformed = true;
        }
      }
      if (!this.Z_Edit.HasFocus || this.enter_hit)
      {
        float result = 0.0f;
        if (float.TryParse(this.Z_Edit.Text, out result) && Math.Round((double) result, 2) != Math.Round((double) this.Z_Slider.TrackPosition, 2))
        {
          this.Z_Slider.TrackPosition = result;
          this.printerview.ObjectTransformed = true;
        }
      }
      this.enter_hit = false;
    }

    public void RefreshSliders()
    {
      if (this.editmode == ModelAdjustmentsDialog.EditMode.Rotation)
        this.UseRotationSliders();
      else if (this.editmode == ModelAdjustmentsDialog.EditMode.Scale)
      {
        this.UseScaleSliders();
      }
      else
      {
        if (this.editmode != ModelAdjustmentsDialog.EditMode.Translation)
          return;
        this.UseTranslationSliders();
      }
    }

    public void PushUpdatedInfomation(ModelTransformPair model)
    {
      if (this.editmode == ModelAdjustmentsDialog.EditMode.Rotation)
      {
        this.X_Slider.TrackPosition = model.transformNode.Rotation.x;
        this.Y_Slider.TrackPosition = model.transformNode.Rotation.y;
        this.Z_Slider.TrackPosition = model.transformNode.Rotation.z;
      }
      else if (this.editmode == ModelAdjustmentsDialog.EditMode.Scale)
      {
        this.X_Slider.TrackPosition = model.transformNode.Scale.x;
        this.Y_Slider.TrackPosition = model.transformNode.Scale.y;
        this.Z_Slider.TrackPosition = model.transformNode.Scale.z;
      }
      else
      {
        if (this.editmode != ModelAdjustmentsDialog.EditMode.Translation)
          return;
        this.X_Slider.TrackPosition = model.transformNode.Translation.x;
        this.Y_Slider.TrackPosition = model.transformNode.Translation.y;
      }
    }

    public override void Refresh()
    {
      this.SaveCurrentSliderInfo();
      this.RefreshSliders();
      base.Refresh();
    }

    internal void SaveCurrentSliderInfo()
    {
      TransformationNode modelTransformation = this.printerview.ModelTransformation;
      if (modelTransformation == null)
        return;
      if (this.editmode == ModelAdjustmentsDialog.EditMode.Rotation)
      {
        modelTransformation.Rotation.x = this.X_Slider.TrackPosition;
        modelTransformation.Rotation.y = this.Y_Slider.TrackPosition;
        modelTransformation.Rotation.z = this.Z_Slider.TrackPosition;
      }
      else if (this.editmode == ModelAdjustmentsDialog.EditMode.Scale)
      {
        modelTransformation.Scale.x = this.X_Slider.TrackPosition;
        modelTransformation.Scale.y = this.Y_Slider.TrackPosition;
        modelTransformation.Scale.z = this.Z_Slider.TrackPosition;
      }
      else
      {
        if (this.editmode != ModelAdjustmentsDialog.EditMode.Translation)
          return;
        modelTransformation.Translation.x = this.X_Slider.TrackPosition;
        modelTransformation.Translation.y = this.Y_Slider.TrackPosition;
      }
    }

    public void SetRotationValues(Vector3 rotation)
    {
      this.SetRotationValues(rotation.x, rotation.y, rotation.z);
    }

    public void SetRotationValues(float x, float y, float z)
    {
      if (this.printerview.SelectedModelTransformPair == null)
        return;
      if (this.editmode == ModelAdjustmentsDialog.EditMode.Rotation)
      {
        this.X_Slider.SetTrackPosition(x);
        this.Y_Slider.SetTrackPosition(y);
        this.Z_Slider.SetTrackPosition(z);
      }
      this.printerview.ObjectTransformed = true;
    }

    public void SetTranslationValues(Vector3 translation)
    {
      this.SetTranslationValues(translation.x, translation.y);
    }

    public void SetTranslationValues(float x, float y)
    {
      if (this.printerview.SelectedModelTransformPair == null)
        return;
      if (this.editmode == ModelAdjustmentsDialog.EditMode.Translation)
      {
        this.X_Slider.SetTrackPosition(x);
        this.Y_Slider.SetTrackPosition(y);
      }
      this.printerview.ObjectTransformed = true;
    }

    public void SetScaleValues(Vector3 scale)
    {
      this.SetScaleValues(scale.x, scale.y, scale.z);
    }

    public void SetScaleValues(float x, float y, float z)
    {
      if (this.printerview.SelectedModelTransformPair == null)
        return;
      if (this.editmode == ModelAdjustmentsDialog.EditMode.Scale)
      {
        this.X_Slider.SetTrackPosition(x);
        this.Y_Slider.SetTrackPosition(y);
        this.Z_Slider.SetTrackPosition(z);
      }
      this.printerview.ObjectTransformed = true;
    }

    public void AdjustTranslationValues(float x, float y, float z)
    {
      if (this.editmode != ModelAdjustmentsDialog.EditMode.Translation)
        return;
      this.X_Slider.TrackPosition += x;
      this.Y_Slider.TrackPosition += y;
    }

    public void AdjustScaleValues(float x, float y, float z)
    {
      if (this.editmode != ModelAdjustmentsDialog.EditMode.Scale)
        return;
      this.X_Slider.TrackPosition += x;
      this.Y_Slider.TrackPosition += y;
      this.Z_Slider.TrackPosition += z;
    }

    public void AdjustRotationValues(float x, float y, float z)
    {
      if (this.editmode != ModelAdjustmentsDialog.EditMode.Rotation)
        return;
      this.X_Slider.TrackPosition += x;
      this.Y_Slider.TrackPosition += y;
      this.Z_Slider.TrackPosition += z;
    }

    public ModelAdjustmentsDialog.EditMode Mode
    {
      get
      {
        return this.editmode;
      }
    }

    public void CheckLinkedRescalingCheckbox(bool action)
    {
      if (this.linkScaling_button == null || this.linkScaling_button.Checked == action)
        return;
      this.linkScaling_button.Checked = action;
    }

    private void EnableLinkedScalingControls(bool action)
    {
      this.top_bracket_image.Visible = action;
      this.link_image.Visible = action;
      this.linkScaling_button.Visible = action;
      this.middle_bracket_image.Visible = action;
      this.bottom_bracket_image.Visible = action;
      this.SaveScalingRatio();
    }

    private void SaveScalingRatio()
    {
      TransformationNode modelTransformation = this.printerview.ModelTransformation;
      if (modelTransformation != null)
      {
        this.initialscalevalues.x = modelTransformation.Scale.x;
        this.initialscalevalues.y = modelTransformation.Scale.y;
        this.initialscalevalues.z = modelTransformation.Scale.z;
      }
      else
      {
        this.initialscalevalues.x = 1f;
        this.initialscalevalues.y = 1f;
        this.initialscalevalues.z = 1f;
      }
    }

    private void LinkedScaleX(float value)
    {
      float fPosition1 = value;
      float fPosition2 = fPosition1 * (this.initialscalevalues.y / this.initialscalevalues.x);
      float fPosition3 = fPosition1 * (this.initialscalevalues.z / this.initialscalevalues.x);
      this.X_Slider.SetTrackPositionNoCallBack(fPosition1);
      this.Y_Slider.SetTrackPositionNoCallBack(fPosition2);
      this.Z_Slider.SetTrackPositionNoCallBack(fPosition3);
      this.X_Edit.Text = fPosition1.ToString("F2");
      this.Y_Edit.Text = fPosition2.ToString("F2");
      this.Z_Edit.Text = fPosition3.ToString("F2");
    }

    private void LinkedScaleY(float value)
    {
      float fPosition1 = value;
      float fPosition2 = fPosition1 * (this.initialscalevalues.x / this.initialscalevalues.y);
      float fPosition3 = fPosition1 * (this.initialscalevalues.z / this.initialscalevalues.y);
      this.X_Slider.SetTrackPositionNoCallBack(fPosition2);
      this.Y_Slider.SetTrackPositionNoCallBack(fPosition1);
      this.Z_Slider.SetTrackPositionNoCallBack(fPosition3);
      this.X_Edit.Text = fPosition2.ToString("F2");
      this.Y_Edit.Text = fPosition1.ToString("F2");
      this.Z_Edit.Text = fPosition3.ToString("F2");
    }

    private void LinkedScaleZ(float value)
    {
      float fPosition1 = value;
      float fPosition2 = fPosition1 * (this.initialscalevalues.x / this.initialscalevalues.z);
      float fPosition3 = fPosition1 * (this.initialscalevalues.y / this.initialscalevalues.z);
      this.X_Slider.SetTrackPositionNoCallBack(fPosition2);
      this.Y_Slider.SetTrackPositionNoCallBack(fPosition3);
      this.Z_Slider.SetTrackPositionNoCallBack(fPosition1);
      this.X_Edit.Text = fPosition2.ToString("F2");
      this.Y_Edit.Text = fPosition3.ToString("F2");
      this.Z_Edit.Text = fPosition1.ToString("F2");
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
