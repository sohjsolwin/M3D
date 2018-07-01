// Decompiled with JetBrains decompiler
// Type: M3D.GUI.LoadNewModelForm
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace M3D.GUI
{
  public class LoadNewModelForm : Form
  {
    private IContainer components;
    private CheckBox checkBoxShowAgain;
    private Button buttonNo;
    private Button buttonYes;
    private Label label1;

    public LoadNewModelForm()
    {
      this.InitializeComponent();
    }

    private void buttonYes_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Yes;
    }

    private void buttonNo_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.No;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.checkBoxShowAgain = new CheckBox();
      this.buttonNo = new Button();
      this.buttonYes = new Button();
      this.label1 = new Label();
      this.SuspendLayout();
      this.checkBoxShowAgain.AutoSize = true;
      this.checkBoxShowAgain.Location = new Point(152, 217);
      this.checkBoxShowAgain.Name = "checkBoxShowAgain";
      this.checkBoxShowAgain.Size = new Size(215, 16);
      this.checkBoxShowAgain.TabIndex = 8;
      this.checkBoxShowAgain.Text = "Do not show this message again.";
      this.checkBoxShowAgain.UseVisualStyleBackColor = true;
      this.buttonNo.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.buttonNo.Location = new Point(281, 71);
      this.buttonNo.Name = "buttonNo";
      this.buttonNo.Size = new Size(148, 54);
      this.buttonNo.TabIndex = 7;
      this.buttonNo.Text = "No";
      this.buttonNo.UseVisualStyleBackColor = true;
      this.buttonNo.Click += new EventHandler(this.buttonNo_Click);
      this.buttonYes.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.buttonYes.ForeColor = Color.Green;
      this.buttonYes.Location = new Point(90, 71);
      this.buttonYes.Name = "buttonYes";
      this.buttonYes.Size = new Size(148, 54);
      this.buttonYes.TabIndex = 6;
      this.buttonYes.Text = "Yes";
      this.buttonYes.UseVisualStyleBackColor = true;
      this.buttonYes.Click += new EventHandler(this.buttonYes_Click);
      this.label1.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.label1.Location = new Point(12, 14);
      this.label1.Name = "label1";
      this.label1.Size = new Size(499, 45);
      this.label1.TabIndex = 5;
      this.label1.Text = "Would you like to replace the already loaded model\r\nwith the new model?";
      this.label1.TextAlign = ContentAlignment.MiddleCenter;
      this.AutoScaleDimensions = new SizeF(7f, 12f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.AutoSize = true;
      this.ClientSize = new Size(533, 148);
      this.Controls.Add((Control) this.checkBoxShowAgain);
      this.Controls.Add((Control) this.buttonNo);
      this.Controls.Add((Control) this.buttonYes);
      this.Controls.Add((Control) this.label1);
      this.Name = nameof (LoadNewModelForm);
      this.Text = "Load New Model";
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
