// Decompiled with JetBrains decompiler
// Type: M3D.Spooler.ManualPrintOptions
// Assembly: M3DSpooler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 88304559-4DEB-43F9-B51D-EEDFDC1EF62E
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\Resources\Spooler\M3DSpooler.exe

using M3D.Spooling.Common;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace M3D.Spooler
{
  public class ManualPrintOptions : Form
  {
    private bool ok;
    private PrintOptions options;
    private IContainer components;
    private RadioButton radioButtonPLA;
    private RadioButton radioButtonABS;
    private RadioButton radioButtonHIPS;
    private Button buttonOK;
    private Button buttonCancel;
    private GroupBox groupBox1;
    private GroupBox groupBox2;
    private CheckBox checkBoxUsePreprocessors;
    private RadioButton radioButtonABSR;
    private RadioButton radioButtonTGH;
    private RadioButton radioButtonCAM;
    private Button buttonUnlock;
    private TextBox textBoxFilamentTemp;
    private GroupBox groupBox3;
    private GroupBox groupBox4;
    private GroupBox groupBoxSDCard;
    private ComboBox comboBoxJobMode;
    private CheckBox checkBoxCalibrate;

    public ManualPrintOptions()
    {
      this.InitializeComponent();
      this.comboBoxJobMode.Items.Add((object) "Print without SD Save");
      this.comboBoxJobMode.Items.Add((object) "Save to printer only.");
      this.comboBoxJobMode.Items.Add((object) "Save to printer then print.");
      this.comboBoxJobMode.SelectedIndex = 0;
      this.options.jobMode = JobParams.Mode.DirectPrinting;
    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
      bool flag = false;
      float num1 = float.Parse(this.textBoxFilamentTemp.Text);
      FilamentConstants.Temperature.MaxMin maxMin = FilamentConstants.Temperature.MaxMinForFilamentType(this.options.type);
      if ((double) num1 >= (double) maxMin.Min && (double) num1 <= (double) maxMin.Max)
        flag = true;
      if (flag)
      {
        this.options.use_preprocessors = this.checkBoxUsePreprocessors.Checked;
        this.options.calibrateZ = this.checkBoxCalibrate.Checked;
        this.options.temperature = Convert.ToInt32(this.textBoxFilamentTemp.Text);
        this.ok = true;
        this.Close();
      }
      else
      {
        int num2 = (int) MessageBox.Show("Please enter a temperature from " + (object) maxMin.Min + " to " + (object) maxMin.Max, "Temperature Invalid");
        this.ok = false;
      }
      switch (this.comboBoxJobMode.SelectedIndex)
      {
        case 1:
          this.options.jobMode = JobParams.Mode.SavingToSDCard;
          break;
        case 2:
          this.options.jobMode = JobParams.Mode.SavingToSDCardAutoStartPrint;
          break;
        default:
          this.options.jobMode = JobParams.Mode.DirectPrinting;
          break;
      }
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      this.ok = false;
      this.Close();
    }

    private void SelectFilament(FilamentSpool.TypeEnum type)
    {
      switch (type)
      {
        case FilamentSpool.TypeEnum.ABS:
          this.radioButtonABS.Checked = true;
          break;
        case FilamentSpool.TypeEnum.PLA:
          this.radioButtonPLA.Checked = true;
          break;
        case FilamentSpool.TypeEnum.HIPS:
          this.radioButtonHIPS.Checked = true;
          break;
        case FilamentSpool.TypeEnum.TGH:
          this.radioButtonTGH.Checked = true;
          break;
        case FilamentSpool.TypeEnum.CAM:
          this.radioButtonCAM.Checked = true;
          break;
        case FilamentSpool.TypeEnum.ABS_R:
          this.radioButtonABSR.Checked = true;
          break;
      }
      this.options.type = type;
      this.textBoxFilamentTemp.Text = FilamentConstants.Temperature.Default(type).ToString();
    }

    public void SetUntetheredOptions(bool allow_untethered, JobParams.Mode defaultmode)
    {
      if (!allow_untethered)
      {
        this.groupBoxSDCard.Enabled = false;
      }
      else
      {
        this.groupBoxSDCard.Enabled = true;
        switch (defaultmode)
        {
          case JobParams.Mode.SavingToSDCard:
            this.comboBoxJobMode.SelectedIndex = 1;
            break;
          case JobParams.Mode.SavingToSDCardAutoStartPrint:
            this.comboBoxJobMode.SelectedIndex = 2;
            break;
          default:
            this.comboBoxJobMode.SelectedIndex = 0;
            break;
        }
      }
    }

    public static PrintOptions GetOptions(FilamentSpool info, bool allow_untethered, JobParams.Mode defaultmode)
    {
      ManualPrintOptions manualPrintOptions = new ManualPrintOptions();
      manualPrintOptions.SetUntetheredOptions(allow_untethered, defaultmode);
      if (info != (FilamentSpool) null)
      {
        manualPrintOptions.SelectFilament(info.filament_type);
        manualPrintOptions.textBoxFilamentTemp.Text = info.filament_temperature.ToString();
      }
      else
        manualPrintOptions.SelectFilament(FilamentSpool.TypeEnum.PLA);
      manualPrintOptions.groupBox1.Enabled = false;
      manualPrintOptions.textBoxFilamentTemp.Enabled = false;
      manualPrintOptions.groupBox3.Enabled = false;
      int num = (int) manualPrintOptions.ShowDialog();
      if (!manualPrintOptions.ok)
      {
        PrintOptions options = manualPrintOptions.options;
        options.type = FilamentSpool.TypeEnum.OtherOrUnknown;
        manualPrintOptions.options = options;
      }
      return manualPrintOptions.options;
    }

    private void buttonEdit_Click(object sender, EventArgs e)
    {
      if (this.groupBox1.Enabled)
      {
        this.groupBox1.Enabled = false;
        this.groupBox3.Enabled = false;
        this.textBoxFilamentTemp.Enabled = false;
      }
      else
      {
        this.groupBox1.Enabled = true;
        this.groupBox3.Enabled = true;
        this.textBoxFilamentTemp.Enabled = true;
      }
    }

    private void radioButtonPLA_CheckedChanged(object sender, EventArgs e)
    {
      if (!this.radioButtonPLA.Checked)
        return;
      this.SelectFilament(FilamentSpool.TypeEnum.PLA);
    }

    private void radioButtonABS_CheckedChanged(object sender, EventArgs e)
    {
      if (!this.radioButtonABS.Checked)
        return;
      this.SelectFilament(FilamentSpool.TypeEnum.ABS);
    }

    private void radioButtonHIPS_CheckedChanged(object sender, EventArgs e)
    {
      if (!this.radioButtonHIPS.Checked)
        return;
      this.SelectFilament(FilamentSpool.TypeEnum.HIPS);
    }

    private void radioButtonTGH_CheckedChanged(object sender, EventArgs e)
    {
      if (!this.radioButtonTGH.Checked)
        return;
      this.SelectFilament(FilamentSpool.TypeEnum.TGH);
    }

    private void radioButtonCAM_CheckedChanged(object sender, EventArgs e)
    {
      if (!this.radioButtonCAM.Checked)
        return;
      this.SelectFilament(FilamentSpool.TypeEnum.CAM);
    }

    private void radioButtonABSR_CheckedChanged(object sender, EventArgs e)
    {
      if (!this.radioButtonABSR.Checked)
        return;
      this.SelectFilament(FilamentSpool.TypeEnum.ABS_R);
    }

    private void comboBoxJobMode_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (this.comboBoxJobMode.SelectedIndex == 1)
      {
        this.checkBoxCalibrate.Checked = false;
        this.checkBoxCalibrate.Enabled = false;
      }
      else
        this.checkBoxCalibrate.Enabled = true;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.radioButtonPLA = new RadioButton();
      this.radioButtonABS = new RadioButton();
      this.radioButtonHIPS = new RadioButton();
      this.buttonOK = new Button();
      this.buttonCancel = new Button();
      this.groupBox1 = new GroupBox();
      this.groupBox4 = new GroupBox();
      this.radioButtonTGH = new RadioButton();
      this.radioButtonABSR = new RadioButton();
      this.radioButtonCAM = new RadioButton();
      this.textBoxFilamentTemp = new TextBox();
      this.groupBox3 = new GroupBox();
      this.buttonUnlock = new Button();
      this.groupBox2 = new GroupBox();
      this.checkBoxCalibrate = new CheckBox();
      this.checkBoxUsePreprocessors = new CheckBox();
      this.groupBoxSDCard = new GroupBox();
      this.comboBoxJobMode = new ComboBox();
      this.groupBox1.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBoxSDCard.SuspendLayout();
      this.SuspendLayout();
      this.radioButtonPLA.AutoSize = true;
      this.radioButtonPLA.Checked = true;
      this.radioButtonPLA.Location = new Point(15, 18);
      this.radioButtonPLA.Name = "radioButtonPLA";
      this.radioButtonPLA.Size = new Size(46, 16);
      this.radioButtonPLA.TabIndex = 0;
      this.radioButtonPLA.TabStop = true;
      this.radioButtonPLA.Text = "PLA";
      this.radioButtonPLA.UseVisualStyleBackColor = true;
      this.radioButtonPLA.CheckedChanged += new EventHandler(this.radioButtonPLA_CheckedChanged);
      this.radioButtonABS.AutoSize = true;
      this.radioButtonABS.Location = new Point(15, 40);
      this.radioButtonABS.Name = "radioButtonABS";
      this.radioButtonABS.Size = new Size(47, 16);
      this.radioButtonABS.TabIndex = 1;
      this.radioButtonABS.Text = "ABS";
      this.radioButtonABS.UseVisualStyleBackColor = true;
      this.radioButtonABS.CheckedChanged += new EventHandler(this.radioButtonABS_CheckedChanged);
      this.radioButtonHIPS.AutoSize = true;
      this.radioButtonHIPS.Location = new Point(15, 60);
      this.radioButtonHIPS.Name = "radioButtonHIPS";
      this.radioButtonHIPS.Size = new Size(50, 16);
      this.radioButtonHIPS.TabIndex = 2;
      this.radioButtonHIPS.Text = "HIPS";
      this.radioButtonHIPS.UseVisualStyleBackColor = true;
      this.radioButtonHIPS.CheckedChanged += new EventHandler(this.radioButtonHIPS_CheckedChanged);
      this.buttonOK.Location = new Point(149, 230);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new Size(106, 18);
      this.buttonOK.TabIndex = 3;
      this.buttonOK.Text = "OK";
      this.buttonOK.UseVisualStyleBackColor = true;
      this.buttonOK.Click += new EventHandler(this.buttonOK_Click);
      this.buttonCancel.Location = new Point(278, 230);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new Size(106, 18);
      this.buttonCancel.TabIndex = 4;
      this.buttonCancel.Text = "Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new EventHandler(this.buttonCancel_Click);
      this.groupBox1.Controls.Add((Control) this.groupBox4);
      this.groupBox1.Location = new Point(12, 19);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new Size(245, 191);
      this.groupBox1.TabIndex = 5;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Filament Options";
      this.groupBox4.Controls.Add((Control) this.radioButtonTGH);
      this.groupBox4.Controls.Add((Control) this.radioButtonPLA);
      this.groupBox4.Controls.Add((Control) this.radioButtonABSR);
      this.groupBox4.Controls.Add((Control) this.radioButtonABS);
      this.groupBox4.Controls.Add((Control) this.radioButtonCAM);
      this.groupBox4.Controls.Add((Control) this.radioButtonHIPS);
      this.groupBox4.Location = new Point(17, 18);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new Size(213, 84);
      this.groupBox4.TabIndex = 11;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Type";
      this.radioButtonTGH.AutoSize = true;
      this.radioButtonTGH.Location = new Point(118, 18);
      this.radioButtonTGH.Name = "radioButtonTGH";
      this.radioButtonTGH.Size = new Size(48, 16);
      this.radioButtonTGH.TabIndex = 3;
      this.radioButtonTGH.Text = "TGH";
      this.radioButtonTGH.UseVisualStyleBackColor = true;
      this.radioButtonTGH.CheckedChanged += new EventHandler(this.radioButtonTGH_CheckedChanged);
      this.radioButtonABSR.AutoSize = true;
      this.radioButtonABSR.Location = new Point(118, 60);
      this.radioButtonABSR.Name = "radioButtonABSR";
      this.radioButtonABSR.Size = new Size(61, 16);
      this.radioButtonABSR.TabIndex = 3;
      this.radioButtonABSR.Text = "ABS-R";
      this.radioButtonABSR.UseVisualStyleBackColor = true;
      this.radioButtonABSR.CheckedChanged += new EventHandler(this.radioButtonABSR_CheckedChanged);
      this.radioButtonCAM.Location = new Point(118, 36);
      this.radioButtonCAM.Name = "radioButtonCAM";
      this.radioButtonCAM.Size = new Size(121, 22);
      this.radioButtonCAM.TabIndex = 4;
      this.radioButtonCAM.Text = "CAM";
      this.radioButtonCAM.CheckedChanged += new EventHandler(this.radioButtonCAM_CheckedChanged);
      this.textBoxFilamentTemp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.textBoxFilamentTemp.Location = new Point(10, 17);
      this.textBoxFilamentTemp.Name = "textBoxFilamentTemp";
      this.textBoxFilamentTemp.Size = new Size(82, 21);
      this.textBoxFilamentTemp.TabIndex = 10;
      this.groupBox3.Controls.Add((Control) this.textBoxFilamentTemp);
      this.groupBox3.Location = new Point(27, 126);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new Size(213, 41);
      this.groupBox3.TabIndex = 7;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Temperature";
      this.buttonUnlock.Location = new Point(192, 180);
      this.buttonUnlock.Name = "buttonUnlock";
      this.buttonUnlock.Size = new Size(59, 18);
      this.buttonUnlock.TabIndex = 5;
      this.buttonUnlock.Text = "Edit";
      this.buttonUnlock.UseVisualStyleBackColor = true;
      this.buttonUnlock.Click += new EventHandler(this.buttonEdit_Click);
      this.groupBox2.Controls.Add((Control) this.checkBoxCalibrate);
      this.groupBox2.Controls.Add((Control) this.checkBoxUsePreprocessors);
      this.groupBox2.Location = new Point(271, 19);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new Size(245, 74);
      this.groupBox2.TabIndex = 6;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Print Options";
      this.checkBoxCalibrate.AutoSize = true;
      this.checkBoxCalibrate.Location = new Point(12, 49);
      this.checkBoxCalibrate.Name = "checkBoxCalibrate";
      this.checkBoxCalibrate.Size = new Size(158, 16);
      this.checkBoxCalibrate.TabIndex = 1;
      this.checkBoxCalibrate.Text = "Calibrate before printing";
      this.checkBoxCalibrate.UseVisualStyleBackColor = true;
      this.checkBoxUsePreprocessors.AutoSize = true;
      this.checkBoxUsePreprocessors.Checked = true;
      this.checkBoxUsePreprocessors.CheckState = CheckState.Checked;
      this.checkBoxUsePreprocessors.Location = new Point(12, 26);
      this.checkBoxUsePreprocessors.Name = "checkBoxUsePreprocessors";
      this.checkBoxUsePreprocessors.Size = new Size(162, 16);
      this.checkBoxUsePreprocessors.TabIndex = 0;
      this.checkBoxUsePreprocessors.Text = "Use M3D Preprocessors";
      this.checkBoxUsePreprocessors.UseVisualStyleBackColor = true;
      this.groupBoxSDCard.Controls.Add((Control) this.comboBoxJobMode);
      this.groupBoxSDCard.Location = new Point(275, 104);
      this.groupBoxSDCard.Name = "groupBoxSDCard";
      this.groupBoxSDCard.Size = new Size(240, 105);
      this.groupBoxSDCard.TabIndex = 8;
      this.groupBoxSDCard.TabStop = false;
      this.groupBoxSDCard.Text = "Untethered Printing";
      this.comboBoxJobMode.FormattingEnabled = true;
      this.comboBoxJobMode.Location = new Point(12, 25);
      this.comboBoxJobMode.Name = "comboBoxJobMode";
      this.comboBoxJobMode.Size = new Size(217, 20);
      this.comboBoxJobMode.TabIndex = 0;
      this.comboBoxJobMode.SelectedIndexChanged += new EventHandler(this.comboBoxJobMode_SelectedIndexChanged);
      this.AutoScaleDimensions = new SizeF(7f, 12f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(530, 258);
      this.ControlBox = false;
      this.Controls.Add((Control) this.groupBoxSDCard);
      this.Controls.Add((Control) this.groupBox3);
      this.Controls.Add((Control) this.buttonUnlock);
      this.Controls.Add((Control) this.groupBox2);
      this.Controls.Add((Control) this.groupBox1);
      this.Controls.Add((Control) this.buttonCancel);
      this.Controls.Add((Control) this.buttonOK);
      this.Name = nameof (ManualPrintOptions);
      this.Text = "Print Options";
      this.groupBox1.ResumeLayout(false);
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBoxSDCard.ResumeLayout(false);
      this.ResumeLayout(false);
    }
  }
}
