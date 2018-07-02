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
      components = new Container();
      timer1 = new Timer(components);
      SuspendLayout();
      timer1.Interval = 2000;
      timer1.Tick += new EventHandler(timer1_Tick);
      AutoScaleDimensions = new SizeF(6f, 13f);
      AutoScaleMode = AutoScaleMode.Font;
      BackColor = Color.White;
      BackgroundImageLayout = ImageLayout.Center;
      if (Program.IsDEBUGBUILD || VersionNumber.Stage.DEBUG == M3D.Spooling.Version.Client_Version.stage)
      {
        BackgroundImage = Resources.splashscreen_debug;
      }
      else if (VersionNumber.Stage.Alpha == M3D.Spooling.Version.Client_Version.stage)
      {
        BackgroundImage = Resources.splashscreenAlpha;
      }
      else if (VersionNumber.Stage.Beta == M3D.Spooling.Version.Client_Version.stage)
      {
        BackgroundImage = Resources.splashscreen_beta;
      }
      else if (M3D.Spooling.Version.Client_Version.stage == VersionNumber.Stage.Release || VersionNumber.Stage.ReleaseCandidate == M3D.Spooling.Version.Client_Version.stage)
      {
        BackgroundImage = Resources.splashscreen_real;
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
