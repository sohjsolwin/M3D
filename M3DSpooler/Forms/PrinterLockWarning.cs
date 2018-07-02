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
      InitializeComponent();
    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
      DoNotShowAgain = checkBoxDoNotShowAgain.Checked;
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
      var componentResourceManager = new ComponentResourceManager(typeof (PrinterLockWarning));
      buttonOK = new Button();
      checkBoxDoNotShowAgain = new CheckBox();
      label1 = new Label();
      SuspendLayout();
      buttonOK.Location = new Point(273, 121);
      buttonOK.Name = "buttonOK";
      buttonOK.Size = new Size(82, 29);
      buttonOK.TabIndex = 0;
      buttonOK.Text = "OK";
      buttonOK.UseVisualStyleBackColor = true;
      buttonOK.Click += new EventHandler(buttonOK_Click);
      checkBoxDoNotShowAgain.AutoSize = true;
      checkBoxDoNotShowAgain.Location = new Point(10, 121);
      checkBoxDoNotShowAgain.Name = "checkBoxDoNotShowAgain";
      checkBoxDoNotShowAgain.Size = new Size(177, 17);
      checkBoxDoNotShowAgain.TabIndex = 1;
      checkBoxDoNotShowAgain.Text = "Do not show this warning again.";
      checkBoxDoNotShowAgain.UseVisualStyleBackColor = true;
      label1.Font = new Font("Arial Narrow", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      label1.Location = new Point(9, 10);
      label1.Name = "label1";
      label1.Size = new Size(339, 91);
      label1.TabIndex = 2;
      label1.Text = componentResourceManager.GetString("label1.Text");
      AutoScaleDimensions = new SizeF(6f, 13f);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(365, 163);
      Controls.Add((Control)label1);
      Controls.Add((Control)checkBoxDoNotShowAgain);
      Controls.Add((Control)buttonOK);
      FormBorderStyle = FormBorderStyle.FixedToolWindow;
      Name = nameof (PrinterLockWarning);
      Text = "M3D Printer Warning";
      ResumeLayout(false);
      PerformLayout();
    }
  }
}
