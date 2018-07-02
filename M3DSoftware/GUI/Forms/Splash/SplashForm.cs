// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Forms.Splash.SplashForm
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Properties;
using M3D.Spooling.Common;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace M3D.GUI.Forms.Splash
{
  public class SplashForm : Form
  {
    private IContainer components;
    private Timer timer1;

    public SplashForm()
    {
      InitializeComponent();
      CenterToScreen();
      timer1.Start();
    }

    private void pictureBox1_Click(object sender, EventArgs e)
    {
    }

    private void SplashForm_Load(object sender, EventArgs e)
    {
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      Hide();
      timer1.Stop();
      timer1.Enabled = false;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && components != null)
      {
        components.Dispose();
      }

      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      components = (IContainer) new Container();
      timer1 = new Timer(components);
      SuspendLayout();
      timer1.Interval = 2000;
      timer1.Tick += new EventHandler(timer1_Tick);
      AutoScaleDimensions = new SizeF(6f, 13f);
      AutoScaleMode = AutoScaleMode.Font;
      BackColor = Color.White;
      BackgroundImageLayout = ImageLayout.Center;
      if (Program.isDEBUGBUILD || VersionNumber.Stage.DEBUG == M3D.Spooling.Version.Client_Version.stage)
      {
        BackgroundImage = (Image) Resources.splashscreen_debug;
      }
      else if (VersionNumber.Stage.Alpha == M3D.Spooling.Version.Client_Version.stage)
      {
        BackgroundImage = (Image) Resources.splashscreenAlpha;
      }
      else if (VersionNumber.Stage.Beta == M3D.Spooling.Version.Client_Version.stage)
      {
        BackgroundImage = (Image) Resources.splashscreen_beta;
      }
      else if (M3D.Spooling.Version.Client_Version.stage == VersionNumber.Stage.Release || VersionNumber.Stage.ReleaseCandidate == M3D.Spooling.Version.Client_Version.stage)
      {
        BackgroundImage = (Image) Resources.splashscreen_real;
      }

      ClientSize = new Size(500, 375);
      ControlBox = false;
      FormBorderStyle = FormBorderStyle.None;
      Icon = Resources.Icon;
      Name = nameof (SplashForm);
      TransparencyKey = Color.White;
      ResumeLayout(false);
    }
  }
}
