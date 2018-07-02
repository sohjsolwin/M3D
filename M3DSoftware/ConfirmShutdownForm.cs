using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace M3D
{
  public class ConfirmShutdownForm : Form
  {
    public bool shutdown;
    private IContainer components;
    private Label label1;
    private Label label2;
    private Button buttonYes;
    private Button buttonNo;

    public ConfirmShutdownForm()
    {
      InitializeComponent();
    }

    private void ButtonYes_Click(object sender, EventArgs e)
    {
      shutdown = true;
      Close();
    }

    private void ButtonNo_Click(object sender, EventArgs e)
    {
      shutdown = false;
      Close();
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
      label1 = new Label();
      label2 = new Label();
      buttonYes = new Button();
      buttonNo = new Button();
      SuspendLayout();
      label1.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
      label1.Location = new Point(10, 12);
      label1.Name = "label1";
      label1.Size = new Size(411, 41);
      label1.TabIndex = 5;
      label1.Text = "There are active print jobs. Shutting down your computer will stop these jobs.";
      label1.TextAlign = ContentAlignment.TopCenter;
      label2.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
      label2.ForeColor = Color.FromArgb(byte.MaxValue, 128, 0);
      label2.Location = new Point(12, 69);
      label2.Name = "label2";
      label2.Size = new Size(411, 29);
      label2.TabIndex = 6;
      label2.Text = "Are you sure you want to exit?";
      label2.TextAlign = ContentAlignment.TopCenter;
      buttonYes.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
      buttonYes.Location = new Point(53, 113);
      buttonYes.Name = "buttonYes";
      buttonYes.Size = new Size(124, 33);
      buttonYes.TabIndex = 7;
      buttonYes.Text = "Yes";
      buttonYes.UseVisualStyleBackColor = true;
      buttonYes.Click += new EventHandler(ButtonYes_Click);
      buttonNo.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
      buttonNo.Location = new Point(264, 113);
      buttonNo.Name = "buttonNo";
      buttonNo.Size = new Size(124, 33);
      buttonNo.TabIndex = 8;
      buttonNo.Text = "No";
      buttonNo.UseVisualStyleBackColor = true;
      buttonNo.Click += new EventHandler(ButtonNo_Click);
      AutoScaleDimensions = new SizeF(6f, 13f);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(434, 166);
      ControlBox = false;
      Controls.Add(buttonNo);
      Controls.Add(buttonYes);
      Controls.Add(label2);
      Controls.Add(label1);
      Name = nameof (ConfirmShutdownForm);
      Text = "M3D";
      TopMost = true;
      ResumeLayout(false);
    }
  }
}
