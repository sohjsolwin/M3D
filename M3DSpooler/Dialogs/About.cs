// Decompiled with JetBrains decompiler
// Type: M3D.Spooler.Dialogs.About
// Assembly: M3DSpooler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 88304559-4DEB-43F9-B51D-EEDFDC1EF62E
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\Resources\Spooler\M3DSpooler.exe

using M3D.Spooling.Common;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace M3D.Spooler.Dialogs
{
  public class About : Form
  {
    private IContainer components;
    private Label label1;
    private Label label2;
    private Label label3;
    private ListBox listBox1;
    private Button button1;
    private Label versionText;

    public About(SpoolerInfo spoolerInfo)
    {
      this.InitializeComponent();
      if (spoolerInfo == null)
        return;
      this.versionText.Text = "Spooler Version: " + spoolerInfo.Version.ToString();
      foreach (EmbeddedFirmwareSummary supportPrinterProfile in spoolerInfo.SupportPrinterProfiles)
      {
        foreach (FirmwareBoardVersionKVP firmwareVersion in supportPrinterProfile.FirmwareVersions)
          this.listBox1.Items.Add((object) string.Format("{0} - {1}", (object) supportPrinterProfile.ToString(), (object) firmwareVersion.ToString()));
      }
    }

    private void button1_Click(object sender, EventArgs e)
    {
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
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (About));
      this.label1 = new Label();
      this.label2 = new Label();
      this.label3 = new Label();
      this.listBox1 = new ListBox();
      this.button1 = new Button();
      this.versionText = new Label();
      this.SuspendLayout();
      this.label1.AutoSize = true;
      this.label1.Font = new Font("Arial Narrow", 18f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.label1.Location = new Point(10, 9);
      this.label1.Name = "label1";
      this.label1.Size = new Size(203, 29);
      this.label1.TabIndex = 0;
      this.label1.Text = "M3D 3D Print Spooler";
      this.label2.AutoSize = true;
      this.label2.Location = new Point(11, 58);
      this.label2.Name = "label2";
      this.label2.Size = new Size(271, 12);
      this.label2.TabIndex = 1;
      this.label2.Text = "Copyright © 2016 M3D LLC. All rights Reserved";
      this.label3.AutoSize = true;
      this.label3.Location = new Point(11, 91);
      this.label3.Name = "label3";
      this.label3.Size = new Size(195, 12);
      this.label3.TabIndex = 2;
      this.label3.Text = "Supported Printers and Firmware:";
      this.listBox1.FormattingEnabled = true;
      this.listBox1.ItemHeight = 12;
      this.listBox1.Location = new Point(12, 110);
      this.listBox1.Name = "listBox1";
      this.listBox1.Size = new Size(393, 76);
      this.listBox1.TabIndex = 3;
      this.button1.Location = new Point(12, 209);
      this.button1.Name = "button1";
      this.button1.Size = new Size(91, 23);
      this.button1.TabIndex = 4;
      this.button1.Text = "OK";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new EventHandler(this.button1_Click);
      this.versionText.AutoSize = true;
      this.versionText.Location = new Point(12, 40);
      this.versionText.Name = "versionText";
      this.versionText.Size = new Size(46, 12);
      this.versionText.TabIndex = 5;
      this.versionText.Text = "version";
      this.AutoScaleDimensions = new SizeF(7f, 12f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(415, 244);
      this.Controls.Add((Control) this.versionText);
      this.Controls.Add((Control) this.button1);
      this.Controls.Add((Control) this.listBox1);
      this.Controls.Add((Control) this.label3);
      this.Controls.Add((Control) this.label2);
      this.Controls.Add((Control) this.label1);
      this.Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = nameof (About);
      this.Text = "About M3D Print Spooler";
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
