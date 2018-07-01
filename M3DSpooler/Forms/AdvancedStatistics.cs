// Decompiled with JetBrains decompiler
// Type: M3D.Spooler.Forms.AdvancedStatistics
// Assembly: M3DSpooler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 88304559-4DEB-43F9-B51D-EEDFDC1EF62E
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\Resources\Spooler\M3DSpooler.exe

using M3D.Spooling.Common;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace M3D.Spooler.Forms
{
  public class AdvancedStatistics : Form
  {
    private IContainer components;
    private ListView listViewPrinterInfo;
    private ColumnHeader columnHeader0;
    private ColumnHeader columnHeader1;
    private ColumnHeader columnHeader2;
    private ColumnHeader columnHeader3;
    private ColumnHeader columnHeader4;
    private ColumnHeader columnHeader5;

    public AdvancedStatistics()
    {
      this.InitializeComponent();
    }

    public void ClearList()
    {
    }

    public void RefreshList(List<PrinterInfo> connected_printers)
    {
      foreach (PrinterInfo connectedPrinter in connected_printers)
      {
        if (!(connectedPrinter.serial_number == PrinterSerialNumber.Undefined) && connectedPrinter != null)
        {
          ListViewItem listViewItem = this.FindItem(this.listViewPrinterInfo, connectedPrinter.serial_number.ToString());
          if (listViewItem == null)
          {
            listViewItem = this.listViewPrinterInfo.Items.Add(connectedPrinter.serial_number.ToString());
            listViewItem.SubItems.Add("?");
            listViewItem.SubItems.Add("?");
            listViewItem.SubItems.Add("?");
            listViewItem.SubItems.Add("?");
            listViewItem.SubItems.Add("?");
          }
          string str1 = connectedPrinter.Status.ToString();
          if (listViewItem.SubItems[1].Text != str1)
            listViewItem.SubItems[1].Text = str1;
          string str2 = (double) connectedPrinter.extruder.Temperature != -1.0 ? ((double) connectedPrinter.extruder.Temperature >= 1.0 ? connectedPrinter.extruder.Temperature.ToString() : "OFF") : "ON";
          if (str2 != listViewItem.SubItems[2].Text)
            listViewItem.SubItems[2].Text = str2;
          if (connectedPrinter.statistics.isMetering)
          {
            listViewItem.SubItems[3].Text = connectedPrinter.statistics.AvgCMDsBeforeRS.ToString();
            listViewItem.SubItems[4].Text = connectedPrinter.statistics.AvgRSDelay.ToString();
            listViewItem.SubItems[5].Text = connectedPrinter.statistics.RSDelayStandardDeviation.ToString();
          }
          else
          {
            listViewItem.SubItems[3].Text = "?";
            listViewItem.SubItems[4].Text = "?";
            listViewItem.SubItems[5].Text = "?";
          }
        }
      }
    }

    private ListViewItem FindItem(ListView view, string text)
    {
      for (int index = 0; index < view.Items.Count; ++index)
      {
        if (view.Items[index].Text == text)
          return view.Items[index];
      }
      return (ListViewItem) null;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.listViewPrinterInfo = new ListView();
      this.columnHeader0 = new ColumnHeader();
      this.columnHeader1 = new ColumnHeader();
      this.columnHeader2 = new ColumnHeader();
      this.columnHeader3 = new ColumnHeader();
      this.columnHeader4 = new ColumnHeader();
      this.columnHeader5 = new ColumnHeader();
      this.SuspendLayout();
      this.listViewPrinterInfo.Activation = ItemActivation.OneClick;
      this.listViewPrinterInfo.Columns.AddRange(new ColumnHeader[6]
      {
        this.columnHeader0,
        this.columnHeader1,
        this.columnHeader2,
        this.columnHeader3,
        this.columnHeader4,
        this.columnHeader5
      });
      this.listViewPrinterInfo.FullRowSelect = true;
      this.listViewPrinterInfo.HideSelection = false;
      this.listViewPrinterInfo.LabelWrap = false;
      this.listViewPrinterInfo.Location = new Point(12, 14);
      this.listViewPrinterInfo.MultiSelect = false;
      this.listViewPrinterInfo.Name = "listViewPrinterInfo";
      this.listViewPrinterInfo.Size = new Size(855, 120);
      this.listViewPrinterInfo.TabIndex = 50;
      this.listViewPrinterInfo.UseCompatibleStateImageBehavior = false;
      this.listViewPrinterInfo.View = View.Details;
      this.columnHeader0.Text = "Serial Number";
      this.columnHeader0.Width = 140;
      this.columnHeader1.Text = "Status";
      this.columnHeader1.Width = 151;
      this.columnHeader2.Text = "Temp. (C)";
      this.columnHeader2.Width = 82;
      this.columnHeader3.Text = "Avg Cmds Before RS";
      this.columnHeader3.Width = 145;
      this.columnHeader4.Text = "Avg RS Delay";
      this.columnHeader4.Width = 100;
      this.columnHeader5.Text = "RS Delay SDeviation";
      this.columnHeader5.Width = 140;
      this.AutoScaleDimensions = new SizeF(7f, 12f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(879, 146);
      this.ControlBox = false;
      this.Controls.Add((Control) this.listViewPrinterInfo);
      this.Name = nameof (AdvancedStatistics);
      this.Text = nameof (AdvancedStatistics);
      this.ResumeLayout(false);
    }
  }
}
