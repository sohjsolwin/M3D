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
      InitializeComponent();
      DEFAULT_SPEED = default_speed;
      MAX_SPEED = max_speed;
      textBoxXBacklash.Text = initial_x.ToString();
      textBoxYBacklash.Text = initial_y.ToString();
      textBoxBacklashSpeed.Text = initial_speed.ToString();
    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
      if (!float.TryParse(textBoxXBacklash.Text, out X_BACKLASH))
      {
        var num1 = (int) MessageBox.Show("Sorry, but the X backlash is not a number. Please try again.");
      }
      else if ((double)X_BACKLASH < 0.0)
      {
        var num2 = (int) MessageBox.Show("The X backlash must be a positive number. Please try again.");
      }
      else if (!float.TryParse(textBoxYBacklash.Text, out Y_BACKLASH))
      {
        var num3 = (int) MessageBox.Show("Sorry, but the Y backlash is not a number. Please try again.");
      }
      else if ((double)Y_BACKLASH < 0.0)
      {
        var num4 = (int) MessageBox.Show("The Y backlash must be a positive number. Please try again.");
      }
      else if (!float.TryParse(textBoxBacklashSpeed.Text, out BACKLASH_SPEED))
      {
        var num5 = (int) MessageBox.Show("Sorry, but the backlash speed is not a number. Please try again.");
      }
      else
      {
        if ((double)BACKLASH_SPEED > (double)MAX_SPEED)
        {
          if (MessageBox.Show("The backlash speed has been set to a value faster than the max speed for this printer and may not be reached. Do you want to continue?", "M3D Spooler", MessageBoxButtons.YesNo) == DialogResult.No)
          {
            return;
          }
        }
        else if ((double)BACKLASH_SPEED < 100.0)
        {
          var num6 = (int) MessageBox.Show("The backlash speed must be a greater than 100. Please try again.");
          return;
        }
        ok = true;
        Close();
      }
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      ok = false;
      Close();
    }

    private void defaultSpeedbutton_Click(object sender, EventArgs e)
    {
      textBoxBacklashSpeed.Text = DEFAULT_SPEED.ToString();
    }

    private void MaxSpeedbutton_Click(object sender, EventArgs e)
    {
      textBoxBacklashSpeed.Text = MAX_SPEED.ToString();
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
      var componentResourceManager = new ComponentResourceManager(typeof (BacklashSettingsDialog));
      buttonCancel = new Button();
      buttonOK = new Button();
      label2 = new Label();
      textBoxYBacklash = new TextBox();
      label1 = new Label();
      textBoxXBacklash = new TextBox();
      groupBox1 = new GroupBox();
      defaultSpeedbutton = new Button();
      label4 = new Label();
      textBoxBacklashSpeed = new TextBox();
      label3 = new Label();
      MaxSpeedbutton = new Button();
      groupBox1.SuspendLayout();
      SuspendLayout();
      buttonCancel.Location = new Point(209, 212);
      buttonCancel.Name = "buttonCancel";
      buttonCancel.Size = new Size(121, 28);
      buttonCancel.TabIndex = 11;
      buttonCancel.Text = "Cancel";
      buttonCancel.UseVisualStyleBackColor = true;
      buttonCancel.Click += new EventHandler(buttonCancel_Click);
      buttonOK.Location = new Point(67, 212);
      buttonOK.Name = "buttonOK";
      buttonOK.Size = new Size(121, 28);
      buttonOK.TabIndex = 10;
      buttonOK.Text = "OK";
      buttonOK.UseVisualStyleBackColor = true;
      buttonOK.Click += new EventHandler(buttonOK_Click);
      label2.AutoSize = true;
      label2.Location = new Point(17, 64);
      label2.Name = "label2";
      label2.Size = new Size(83, 12);
      label2.TabIndex = 9;
      label2.Text = "Y_BACKLASH";
      textBoxYBacklash.Location = new Point(132, 61);
      textBoxYBacklash.Name = "textBoxYBacklash";
      textBoxYBacklash.Size = new Size(230, 21);
      textBoxYBacklash.TabIndex = 8;
      label1.AutoSize = true;
      label1.Location = new Point(17, 30);
      label1.Name = "label1";
      label1.Size = new Size(83, 12);
      label1.TabIndex = 7;
      label1.Text = "X_BACKLASH";
      textBoxXBacklash.Location = new Point(132, 27);
      textBoxXBacklash.Name = "textBoxXBacklash";
      textBoxXBacklash.Size = new Size(230, 21);
      textBoxXBacklash.TabIndex = 6;
      groupBox1.Controls.Add((Control)MaxSpeedbutton);
      groupBox1.Controls.Add((Control)defaultSpeedbutton);
      groupBox1.Controls.Add((Control)label4);
      groupBox1.Controls.Add((Control)textBoxBacklashSpeed);
      groupBox1.Controls.Add((Control)label3);
      groupBox1.Controls.Add((Control)textBoxXBacklash);
      groupBox1.Controls.Add((Control)label1);
      groupBox1.Controls.Add((Control)textBoxYBacklash);
      groupBox1.Controls.Add((Control)label2);
      groupBox1.Location = new Point(14, 11);
      groupBox1.Name = "groupBox1";
      groupBox1.Size = new Size(368, 190);
      groupBox1.TabIndex = 12;
      groupBox1.TabStop = false;
      groupBox1.Text = "Printer Backlash Settings";
      defaultSpeedbutton.Location = new Point(211, 91);
      defaultSpeedbutton.Name = "defaultSpeedbutton";
      defaultSpeedbutton.Size = new Size(73, 28);
      defaultSpeedbutton.TabIndex = 13;
      defaultSpeedbutton.Text = "Default";
      defaultSpeedbutton.UseVisualStyleBackColor = true;
      defaultSpeedbutton.Click += new EventHandler(defaultSpeedbutton_Click);
      label4.Location = new Point(18, 132);
      label4.Name = "label4";
      label4.Size = new Size(344, 46);
      label4.TabIndex = 13;
      label4.Text = "Note: Backlash speeds are not guaranteed and are further limitted by the hardware and settings built into the firmware.";
      textBoxBacklashSpeed.Location = new Point(131, 94);
      textBoxBacklashSpeed.Name = "textBoxBacklashSpeed";
      textBoxBacklashSpeed.Size = new Size(74, 21);
      textBoxBacklashSpeed.TabIndex = 10;
      label3.AutoSize = true;
      label3.Location = new Point(16, 97);
      label3.Name = "label3";
      label3.Size = new Size(97, 12);
      label3.TabIndex = 11;
      label3.Text = "Backlash Speed";
      MaxSpeedbutton.Location = new Point(289, 91);
      MaxSpeedbutton.Name = "MaxSpeedbutton";
      MaxSpeedbutton.Size = new Size(73, 28);
      MaxSpeedbutton.TabIndex = 14;
      MaxSpeedbutton.Text = "Max";
      MaxSpeedbutton.UseVisualStyleBackColor = true;
      MaxSpeedbutton.Click += new EventHandler(MaxSpeedbutton_Click);
      AutoScaleDimensions = new SizeF(7f, 12f);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(394, 246);
      ControlBox = false;
      Controls.Add((Control)groupBox1);
      Controls.Add((Control)buttonCancel);
      Controls.Add((Control)buttonOK);
      Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      Name = "BacklashSettings";
      Text = "Preprocessor Settings";
      groupBox1.ResumeLayout(false);
      groupBox1.PerformLayout();
      ResumeLayout(false);
    }
  }
}
