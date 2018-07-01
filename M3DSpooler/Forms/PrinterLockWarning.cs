// Decompiled with JetBrains decompiler
// Type: M3D.Spooler.Forms.PrinterLockWarning
// Assembly: M3DSpooler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 88304559-4DEB-43F9-B51D-EEDFDC1EF62E
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\Resources\Spooler\M3DSpooler.exe

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace M3D.Spooler.Forms
{
  public class PrinterLockWarning : Form
  {
    public bool DoNotShowAgain;
    private IContainer components;
    private Button buttonOK;
    private CheckBox checkBoxDoNotShowAgain;
    private Label label1;

    public PrinterLockWarning()
    {
      this.InitializeComponent();
    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
      this.DoNotShowAgain = this.checkBoxDoNotShowAgain.Checked;
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
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (PrinterLockWarning));
      this.buttonOK = new Button();
      this.checkBoxDoNotShowAgain = new CheckBox();
      this.label1 = new Label();
      this.SuspendLayout();
      this.buttonOK.Location = new Point(273, 121);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new Size(82, 29);
      this.buttonOK.TabIndex = 0;
      this.buttonOK.Text = "OK";
      this.buttonOK.UseVisualStyleBackColor = true;
      this.buttonOK.Click += new EventHandler(this.buttonOK_Click);
      this.checkBoxDoNotShowAgain.AutoSize = true;
      this.checkBoxDoNotShowAgain.Location = new Point(10, 121);
      this.checkBoxDoNotShowAgain.Name = "checkBoxDoNotShowAgain";
      this.checkBoxDoNotShowAgain.Size = new Size(177, 17);
      this.checkBoxDoNotShowAgain.TabIndex = 1;
      this.checkBoxDoNotShowAgain.Text = "Do not show this warning again.";
      this.checkBoxDoNotShowAgain.UseVisualStyleBackColor = true;
      this.label1.Font = new Font("Arial Narrow", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.label1.Location = new Point(9, 10);
      this.label1.Name = "label1";
      this.label1.Size = new Size(339, 91);
      this.label1.TabIndex = 2;
      this.label1.Text = componentResourceManager.GetString("label1.Text");
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(365, 163);
      this.Controls.Add((Control) this.label1);
      this.Controls.Add((Control) this.checkBoxDoNotShowAgain);
      this.Controls.Add((Control) this.buttonOK);
      this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
      this.Name = nameof (PrinterLockWarning);
      this.Text = "M3D Printer Warning";
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
