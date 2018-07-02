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
      InitializeComponent();
      if (spoolerInfo == null)
      {
        return;
      }

      versionText.Text = "Spooler Version: " + spoolerInfo.Version.ToString();
      foreach (EmbeddedFirmwareSummary supportPrinterProfile in spoolerInfo.SupportPrinterProfiles)
      {
        foreach (FirmwareBoardVersionKVP firmwareVersion in supportPrinterProfile.FirmwareVersions)
        {
          listBox1.Items.Add(string.Format("{0} - {1}", (object)supportPrinterProfile.ToString(), (object)firmwareVersion.ToString()));
        }
      }
    }

    private void button1_Click(object sender, EventArgs e)
    {
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
      var componentResourceManager = new ComponentResourceManager(typeof (About));
      label1 = new Label();
      label2 = new Label();
      label3 = new Label();
      listBox1 = new ListBox();
      button1 = new Button();
      versionText = new Label();
      SuspendLayout();
      label1.AutoSize = true;
      label1.Font = new Font("Arial Narrow", 18f, FontStyle.Regular, GraphicsUnit.Point, 0);
      label1.Location = new Point(10, 9);
      label1.Name = "label1";
      label1.Size = new Size(203, 29);
      label1.TabIndex = 0;
      label1.Text = "M3D 3D Print Spooler";
      label2.AutoSize = true;
      label2.Location = new Point(11, 58);
      label2.Name = "label2";
      label2.Size = new Size(271, 12);
      label2.TabIndex = 1;
      label2.Text = "Copyright © 2016 M3D LLC. All rights Reserved";
      label3.AutoSize = true;
      label3.Location = new Point(11, 91);
      label3.Name = "label3";
      label3.Size = new Size(195, 12);
      label3.TabIndex = 2;
      label3.Text = "Supported Printers and Firmware:";
      listBox1.FormattingEnabled = true;
      listBox1.ItemHeight = 12;
      listBox1.Location = new Point(12, 110);
      listBox1.Name = "listBox1";
      listBox1.Size = new Size(393, 76);
      listBox1.TabIndex = 3;
      button1.Location = new Point(12, 209);
      button1.Name = "button1";
      button1.Size = new Size(91, 23);
      button1.TabIndex = 4;
      button1.Text = "OK";
      button1.UseVisualStyleBackColor = true;
      button1.Click += new EventHandler(button1_Click);
      versionText.AutoSize = true;
      versionText.Location = new Point(12, 40);
      versionText.Name = "versionText";
      versionText.Size = new Size(46, 12);
      versionText.TabIndex = 5;
      versionText.Text = "version";
      AutoScaleDimensions = new SizeF(7f, 12f);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(415, 244);
      Controls.Add(versionText);
      Controls.Add(button1);
      Controls.Add(listBox1);
      Controls.Add(label3);
      Controls.Add(label2);
      Controls.Add(label1);
      Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      MaximizeBox = false;
      MinimizeBox = false;
      Name = nameof (About);
      Text = "About M3D Print Spooler";
      ResumeLayout(false);
      PerformLayout();
    }
  }
}
