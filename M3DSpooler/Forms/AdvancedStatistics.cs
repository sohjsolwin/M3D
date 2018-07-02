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
      InitializeComponent();
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
          ListViewItem listViewItem = FindItem(listViewPrinterInfo, connectedPrinter.serial_number.ToString());
          if (listViewItem == null)
          {
            listViewItem = listViewPrinterInfo.Items.Add(connectedPrinter.serial_number.ToString());
            listViewItem.SubItems.Add("?");
            listViewItem.SubItems.Add("?");
            listViewItem.SubItems.Add("?");
            listViewItem.SubItems.Add("?");
            listViewItem.SubItems.Add("?");
          }
          var str1 = connectedPrinter.Status.ToString();
          if (listViewItem.SubItems[1].Text != str1)
          {
            listViewItem.SubItems[1].Text = str1;
          }

          var str2 = (double) connectedPrinter.extruder.Temperature != -1.0 ? ((double) connectedPrinter.extruder.Temperature >= 1.0 ? connectedPrinter.extruder.Temperature.ToString() : "OFF") : "ON";
          if (str2 != listViewItem.SubItems[2].Text)
          {
            listViewItem.SubItems[2].Text = str2;
          }

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
      for (var index = 0; index < view.Items.Count; ++index)
      {
        if (view.Items[index].Text == text)
        {
          return view.Items[index];
        }
      }
      return (ListViewItem) null;
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
      listViewPrinterInfo = new ListView();
      columnHeader0 = new ColumnHeader();
      columnHeader1 = new ColumnHeader();
      columnHeader2 = new ColumnHeader();
      columnHeader3 = new ColumnHeader();
      columnHeader4 = new ColumnHeader();
      columnHeader5 = new ColumnHeader();
      SuspendLayout();
      listViewPrinterInfo.Activation = ItemActivation.OneClick;
      listViewPrinterInfo.Columns.AddRange(new ColumnHeader[6]
      {
        columnHeader0,
        columnHeader1,
        columnHeader2,
        columnHeader3,
        columnHeader4,
        columnHeader5
      });
      listViewPrinterInfo.FullRowSelect = true;
      listViewPrinterInfo.HideSelection = false;
      listViewPrinterInfo.LabelWrap = false;
      listViewPrinterInfo.Location = new Point(12, 14);
      listViewPrinterInfo.MultiSelect = false;
      listViewPrinterInfo.Name = "listViewPrinterInfo";
      listViewPrinterInfo.Size = new Size(855, 120);
      listViewPrinterInfo.TabIndex = 50;
      listViewPrinterInfo.UseCompatibleStateImageBehavior = false;
      listViewPrinterInfo.View = View.Details;
      columnHeader0.Text = "Serial Number";
      columnHeader0.Width = 140;
      columnHeader1.Text = "Status";
      columnHeader1.Width = 151;
      columnHeader2.Text = "Temp. (C)";
      columnHeader2.Width = 82;
      columnHeader3.Text = "Avg Cmds Before RS";
      columnHeader3.Width = 145;
      columnHeader4.Text = "Avg RS Delay";
      columnHeader4.Width = 100;
      columnHeader5.Text = "RS Delay SDeviation";
      columnHeader5.Width = 140;
      AutoScaleDimensions = new SizeF(7f, 12f);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(879, 146);
      ControlBox = false;
      Controls.Add((Control)listViewPrinterInfo);
      Name = nameof (AdvancedStatistics);
      Text = nameof (AdvancedStatistics);
      ResumeLayout(false);
    }
  }
}
