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
      InitializeComponent();
    }

    private void buttonYes_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Yes;
    }

    private void buttonNo_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.No;
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
      checkBoxShowAgain = new CheckBox();
      buttonNo = new Button();
      buttonYes = new Button();
      label1 = new Label();
      SuspendLayout();
      checkBoxShowAgain.AutoSize = true;
      checkBoxShowAgain.Location = new Point(152, 217);
      checkBoxShowAgain.Name = "checkBoxShowAgain";
      checkBoxShowAgain.Size = new Size(215, 16);
      checkBoxShowAgain.TabIndex = 8;
      checkBoxShowAgain.Text = "Do not show this message again.";
      checkBoxShowAgain.UseVisualStyleBackColor = true;
      buttonNo.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      buttonNo.Location = new Point(281, 71);
      buttonNo.Name = "buttonNo";
      buttonNo.Size = new Size(148, 54);
      buttonNo.TabIndex = 7;
      buttonNo.Text = "No";
      buttonNo.UseVisualStyleBackColor = true;
      buttonNo.Click += new EventHandler(buttonNo_Click);
      buttonYes.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      buttonYes.ForeColor = Color.Green;
      buttonYes.Location = new Point(90, 71);
      buttonYes.Name = "buttonYes";
      buttonYes.Size = new Size(148, 54);
      buttonYes.TabIndex = 6;
      buttonYes.Text = "Yes";
      buttonYes.UseVisualStyleBackColor = true;
      buttonYes.Click += new EventHandler(buttonYes_Click);
      label1.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      label1.Location = new Point(12, 14);
      label1.Name = "label1";
      label1.Size = new Size(499, 45);
      label1.TabIndex = 5;
      label1.Text = "Would you like to replace the already loaded model\r\nwith the new model?";
      label1.TextAlign = ContentAlignment.MiddleCenter;
      AutoScaleDimensions = new SizeF(7f, 12f);
      AutoScaleMode = AutoScaleMode.Font;
      AutoSize = true;
      ClientSize = new Size(533, 148);
      Controls.Add((Control)checkBoxShowAgain);
      Controls.Add((Control)buttonNo);
      Controls.Add((Control)buttonYes);
      Controls.Add((Control)label1);
      Name = nameof (LoadNewModelForm);
      Text = "Load New Model";
      ResumeLayout(false);
      PerformLayout();
    }
  }
}
