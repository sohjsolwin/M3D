// Decompiled with JetBrains decompiler
// Type: M3D.Spooler.BacklashSettingsDialog
// Assembly: M3DSpooler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 88304559-4DEB-43F9-B51D-EEDFDC1EF62E
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\Resources\Spooler\M3DSpooler.exe

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace M3D.Spooler
{
  public class BacklashSettingsDialog : Form
  {
    public float X_BACKLASH;
    public float Y_BACKLASH;
    public float BACKLASH_SPEED;
    private float DEFAULT_SPEED;
    private float MAX_SPEED;
    public bool ok;
    private IContainer components;
    private Button buttonCancel;
    private Button buttonOK;
    private Label label2;
    private TextBox textBoxYBacklash;
    private Label label1;
    private TextBox textBoxXBacklash;
    private GroupBox groupBox1;
    private TextBox textBoxBacklashSpeed;
    private Label label3;
    private Label label4;
    private Button defaultSpeedbutton;
    private Button MaxSpeedbutton;

    public BacklashSettingsDialog(float initial_x, float initial_y, float initial_speed, float default_speed, float max_speed)
    {
      this.InitializeComponent();
      this.DEFAULT_SPEED = default_speed;
      this.MAX_SPEED = max_speed;
      this.textBoxXBacklash.Text = initial_x.ToString();
      this.textBoxYBacklash.Text = initial_y.ToString();
      this.textBoxBacklashSpeed.Text = initial_speed.ToString();
    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
      if (!float.TryParse(this.textBoxXBacklash.Text, out this.X_BACKLASH))
      {
        int num1 = (int) MessageBox.Show("Sorry, but the X backlash is not a number. Please try again.");
      }
      else if ((double) this.X_BACKLASH < 0.0)
      {
        int num2 = (int) MessageBox.Show("The X backlash must be a positive number. Please try again.");
      }
      else if (!float.TryParse(this.textBoxYBacklash.Text, out this.Y_BACKLASH))
      {
        int num3 = (int) MessageBox.Show("Sorry, but the Y backlash is not a number. Please try again.");
      }
      else if ((double) this.Y_BACKLASH < 0.0)
      {
        int num4 = (int) MessageBox.Show("The Y backlash must be a positive number. Please try again.");
      }
      else if (!float.TryParse(this.textBoxBacklashSpeed.Text, out this.BACKLASH_SPEED))
      {
        int num5 = (int) MessageBox.Show("Sorry, but the backlash speed is not a number. Please try again.");
      }
      else
      {
        if ((double) this.BACKLASH_SPEED > (double) this.MAX_SPEED)
        {
          if (MessageBox.Show("The backlash speed has been set to a value faster than the max speed for this printer and may not be reached. Do you want to continue?", "M3D Spooler", MessageBoxButtons.YesNo) == DialogResult.No)
            return;
        }
        else if ((double) this.BACKLASH_SPEED < 100.0)
        {
          int num6 = (int) MessageBox.Show("The backlash speed must be a greater than 100. Please try again.");
          return;
        }
        this.ok = true;
        this.Close();
      }
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      this.ok = false;
      this.Close();
    }

    private void defaultSpeedbutton_Click(object sender, EventArgs e)
    {
      this.textBoxBacklashSpeed.Text = this.DEFAULT_SPEED.ToString();
    }

    private void MaxSpeedbutton_Click(object sender, EventArgs e)
    {
      this.textBoxBacklashSpeed.Text = this.MAX_SPEED.ToString();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (BacklashSettingsDialog));
      this.buttonCancel = new Button();
      this.buttonOK = new Button();
      this.label2 = new Label();
      this.textBoxYBacklash = new TextBox();
      this.label1 = new Label();
      this.textBoxXBacklash = new TextBox();
      this.groupBox1 = new GroupBox();
      this.defaultSpeedbutton = new Button();
      this.label4 = new Label();
      this.textBoxBacklashSpeed = new TextBox();
      this.label3 = new Label();
      this.MaxSpeedbutton = new Button();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      this.buttonCancel.Location = new Point(209, 212);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new Size(121, 28);
      this.buttonCancel.TabIndex = 11;
      this.buttonCancel.Text = "Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new EventHandler(this.buttonCancel_Click);
      this.buttonOK.Location = new Point(67, 212);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new Size(121, 28);
      this.buttonOK.TabIndex = 10;
      this.buttonOK.Text = "OK";
      this.buttonOK.UseVisualStyleBackColor = true;
      this.buttonOK.Click += new EventHandler(this.buttonOK_Click);
      this.label2.AutoSize = true;
      this.label2.Location = new Point(17, 64);
      this.label2.Name = "label2";
      this.label2.Size = new Size(83, 12);
      this.label2.TabIndex = 9;
      this.label2.Text = "Y_BACKLASH";
      this.textBoxYBacklash.Location = new Point(132, 61);
      this.textBoxYBacklash.Name = "textBoxYBacklash";
      this.textBoxYBacklash.Size = new Size(230, 21);
      this.textBoxYBacklash.TabIndex = 8;
      this.label1.AutoSize = true;
      this.label1.Location = new Point(17, 30);
      this.label1.Name = "label1";
      this.label1.Size = new Size(83, 12);
      this.label1.TabIndex = 7;
      this.label1.Text = "X_BACKLASH";
      this.textBoxXBacklash.Location = new Point(132, 27);
      this.textBoxXBacklash.Name = "textBoxXBacklash";
      this.textBoxXBacklash.Size = new Size(230, 21);
      this.textBoxXBacklash.TabIndex = 6;
      this.groupBox1.Controls.Add((Control) this.MaxSpeedbutton);
      this.groupBox1.Controls.Add((Control) this.defaultSpeedbutton);
      this.groupBox1.Controls.Add((Control) this.label4);
      this.groupBox1.Controls.Add((Control) this.textBoxBacklashSpeed);
      this.groupBox1.Controls.Add((Control) this.label3);
      this.groupBox1.Controls.Add((Control) this.textBoxXBacklash);
      this.groupBox1.Controls.Add((Control) this.label1);
      this.groupBox1.Controls.Add((Control) this.textBoxYBacklash);
      this.groupBox1.Controls.Add((Control) this.label2);
      this.groupBox1.Location = new Point(14, 11);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new Size(368, 190);
      this.groupBox1.TabIndex = 12;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Printer Backlash Settings";
      this.defaultSpeedbutton.Location = new Point(211, 91);
      this.defaultSpeedbutton.Name = "defaultSpeedbutton";
      this.defaultSpeedbutton.Size = new Size(73, 28);
      this.defaultSpeedbutton.TabIndex = 13;
      this.defaultSpeedbutton.Text = "Default";
      this.defaultSpeedbutton.UseVisualStyleBackColor = true;
      this.defaultSpeedbutton.Click += new EventHandler(this.defaultSpeedbutton_Click);
      this.label4.Location = new Point(18, 132);
      this.label4.Name = "label4";
      this.label4.Size = new Size(344, 46);
      this.label4.TabIndex = 13;
      this.label4.Text = "Note: Backlash speeds are not guaranteed and are further limitted by the hardware and settings built into the firmware.";
      this.textBoxBacklashSpeed.Location = new Point(131, 94);
      this.textBoxBacklashSpeed.Name = "textBoxBacklashSpeed";
      this.textBoxBacklashSpeed.Size = new Size(74, 21);
      this.textBoxBacklashSpeed.TabIndex = 10;
      this.label3.AutoSize = true;
      this.label3.Location = new Point(16, 97);
      this.label3.Name = "label3";
      this.label3.Size = new Size(97, 12);
      this.label3.TabIndex = 11;
      this.label3.Text = "Backlash Speed";
      this.MaxSpeedbutton.Location = new Point(289, 91);
      this.MaxSpeedbutton.Name = "MaxSpeedbutton";
      this.MaxSpeedbutton.Size = new Size(73, 28);
      this.MaxSpeedbutton.TabIndex = 14;
      this.MaxSpeedbutton.Text = "Max";
      this.MaxSpeedbutton.UseVisualStyleBackColor = true;
      this.MaxSpeedbutton.Click += new EventHandler(this.MaxSpeedbutton_Click);
      this.AutoScaleDimensions = new SizeF(7f, 12f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(394, 246);
      this.ControlBox = false;
      this.Controls.Add((Control) this.groupBox1);
      this.Controls.Add((Control) this.buttonCancel);
      this.Controls.Add((Control) this.buttonOK);
      this.Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      this.Name = "BacklashSettings";
      this.Text = "Preprocessor Settings";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);
    }
  }
}
