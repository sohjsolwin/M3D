// Decompiled with JetBrains decompiler
// Type: M3D.ConfirmShutdownForm
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      this.InitializeComponent();
    }

    private void buttonYes_Click(object sender, EventArgs e)
    {
      this.shutdown = true;
      this.Close();
    }

    private void buttonNo_Click(object sender, EventArgs e)
    {
      this.shutdown = false;
      this.Close();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.label1 = new Label();
      this.label2 = new Label();
      this.buttonYes = new Button();
      this.buttonNo = new Button();
      this.SuspendLayout();
      this.label1.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.label1.Location = new Point(10, 12);
      this.label1.Name = "label1";
      this.label1.Size = new Size(411, 41);
      this.label1.TabIndex = 5;
      this.label1.Text = "There are active print jobs. Shutting down your computer will stop these jobs.";
      this.label1.TextAlign = ContentAlignment.TopCenter;
      this.label2.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.label2.ForeColor = Color.FromArgb((int) byte.MaxValue, 128, 0);
      this.label2.Location = new Point(12, 69);
      this.label2.Name = "label2";
      this.label2.Size = new Size(411, 29);
      this.label2.TabIndex = 6;
      this.label2.Text = "Are you sure you want to exit?";
      this.label2.TextAlign = ContentAlignment.TopCenter;
      this.buttonYes.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.buttonYes.Location = new Point(53, 113);
      this.buttonYes.Name = "buttonYes";
      this.buttonYes.Size = new Size(124, 33);
      this.buttonYes.TabIndex = 7;
      this.buttonYes.Text = "Yes";
      this.buttonYes.UseVisualStyleBackColor = true;
      this.buttonYes.Click += new EventHandler(this.buttonYes_Click);
      this.buttonNo.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.buttonNo.Location = new Point(264, 113);
      this.buttonNo.Name = "buttonNo";
      this.buttonNo.Size = new Size(124, 33);
      this.buttonNo.TabIndex = 8;
      this.buttonNo.Text = "No";
      this.buttonNo.UseVisualStyleBackColor = true;
      this.buttonNo.Click += new EventHandler(this.buttonNo_Click);
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(434, 166);
      this.ControlBox = false;
      this.Controls.Add((Control) this.buttonNo);
      this.Controls.Add((Control) this.buttonYes);
      this.Controls.Add((Control) this.label2);
      this.Controls.Add((Control) this.label1);
      this.Name = nameof (ConfirmShutdownForm);
      this.Text = "M3D";
      this.TopMost = true;
      this.ResumeLayout(false);
    }
  }
}
