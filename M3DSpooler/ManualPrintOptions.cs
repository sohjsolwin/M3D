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
      InitializeComponent();
      comboBoxJobMode.Items.Add((object) "Print without SD Save");
      comboBoxJobMode.Items.Add((object) "Save to printer only.");
      comboBoxJobMode.Items.Add((object) "Save to printer then print.");
      comboBoxJobMode.SelectedIndex = 0;
      options.jobMode = JobParams.Mode.DirectPrinting;
    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
      var flag = false;
      var num1 = float.Parse(textBoxFilamentTemp.Text);
      FilamentConstants.Temperature.MaxMin maxMin = FilamentConstants.Temperature.MaxMinForFilamentType(options.type);
      if ((double) num1 >= (double) maxMin.Min && (double) num1 <= (double) maxMin.Max)
      {
        flag = true;
      }

      if (flag)
      {
        options.use_preprocessors = checkBoxUsePreprocessors.Checked;
        options.calibrateZ = checkBoxCalibrate.Checked;
        options.temperature = Convert.ToInt32(textBoxFilamentTemp.Text);
        ok = true;
        Close();
      }
      else
      {
        var num2 = (int) MessageBox.Show("Please enter a temperature from " + (object) maxMin.Min + " to " + (object) maxMin.Max, "Temperature Invalid");
        ok = false;
      }
      switch (comboBoxJobMode.SelectedIndex)
      {
        case 1:
          options.jobMode = JobParams.Mode.SavingToSDCard;
          break;
        case 2:
          options.jobMode = JobParams.Mode.SavingToSDCardAutoStartPrint;
          break;
        default:
          options.jobMode = JobParams.Mode.DirectPrinting;
          break;
      }
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      ok = false;
      Close();
    }

    private void SelectFilament(FilamentSpool.TypeEnum type)
    {
      switch (type)
      {
        case FilamentSpool.TypeEnum.ABS:
          radioButtonABS.Checked = true;
          break;
        case FilamentSpool.TypeEnum.PLA:
          radioButtonPLA.Checked = true;
          break;
        case FilamentSpool.TypeEnum.HIPS:
          radioButtonHIPS.Checked = true;
          break;
        case FilamentSpool.TypeEnum.TGH:
          radioButtonTGH.Checked = true;
          break;
        case FilamentSpool.TypeEnum.CAM:
          radioButtonCAM.Checked = true;
          break;
        case FilamentSpool.TypeEnum.ABS_R:
          radioButtonABSR.Checked = true;
          break;
      }
      options.type = type;
      textBoxFilamentTemp.Text = FilamentConstants.Temperature.Default(type).ToString();
    }

    public void SetUntetheredOptions(bool allow_untethered, JobParams.Mode defaultmode)
    {
      if (!allow_untethered)
      {
        groupBoxSDCard.Enabled = false;
      }
      else
      {
        groupBoxSDCard.Enabled = true;
        switch (defaultmode)
        {
          case JobParams.Mode.SavingToSDCard:
            comboBoxJobMode.SelectedIndex = 1;
            break;
          case JobParams.Mode.SavingToSDCardAutoStartPrint:
            comboBoxJobMode.SelectedIndex = 2;
            break;
          default:
            comboBoxJobMode.SelectedIndex = 0;
            break;
        }
      }
    }

    public static PrintOptions GetOptions(FilamentSpool info, bool allow_untethered, JobParams.Mode defaultmode)
    {
      var manualPrintOptions = new ManualPrintOptions();
      manualPrintOptions.SetUntetheredOptions(allow_untethered, defaultmode);
      if (info != (FilamentSpool) null)
      {
        manualPrintOptions.SelectFilament(info.filament_type);
        manualPrintOptions.textBoxFilamentTemp.Text = info.filament_temperature.ToString();
      }
      else
      {
        manualPrintOptions.SelectFilament(FilamentSpool.TypeEnum.PLA);
      }

      manualPrintOptions.groupBox1.Enabled = false;
      manualPrintOptions.textBoxFilamentTemp.Enabled = false;
      manualPrintOptions.groupBox3.Enabled = false;
      var num = (int) manualPrintOptions.ShowDialog();
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
      if (groupBox1.Enabled)
      {
        groupBox1.Enabled = false;
        groupBox3.Enabled = false;
        textBoxFilamentTemp.Enabled = false;
      }
      else
      {
        groupBox1.Enabled = true;
        groupBox3.Enabled = true;
        textBoxFilamentTemp.Enabled = true;
      }
    }

    private void radioButtonPLA_CheckedChanged(object sender, EventArgs e)
    {
      if (!radioButtonPLA.Checked)
      {
        return;
      }

      SelectFilament(FilamentSpool.TypeEnum.PLA);
    }

    private void radioButtonABS_CheckedChanged(object sender, EventArgs e)
    {
      if (!radioButtonABS.Checked)
      {
        return;
      }

      SelectFilament(FilamentSpool.TypeEnum.ABS);
    }

    private void radioButtonHIPS_CheckedChanged(object sender, EventArgs e)
    {
      if (!radioButtonHIPS.Checked)
      {
        return;
      }

      SelectFilament(FilamentSpool.TypeEnum.HIPS);
    }

    private void radioButtonTGH_CheckedChanged(object sender, EventArgs e)
    {
      if (!radioButtonTGH.Checked)
      {
        return;
      }

      SelectFilament(FilamentSpool.TypeEnum.TGH);
    }

    private void radioButtonCAM_CheckedChanged(object sender, EventArgs e)
    {
      if (!radioButtonCAM.Checked)
      {
        return;
      }

      SelectFilament(FilamentSpool.TypeEnum.CAM);
    }

    private void radioButtonABSR_CheckedChanged(object sender, EventArgs e)
    {
      if (!radioButtonABSR.Checked)
      {
        return;
      }

      SelectFilament(FilamentSpool.TypeEnum.ABS_R);
    }

    private void comboBoxJobMode_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (comboBoxJobMode.SelectedIndex == 1)
      {
        checkBoxCalibrate.Checked = false;
        checkBoxCalibrate.Enabled = false;
      }
      else
      {
        checkBoxCalibrate.Enabled = true;
      }
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
      radioButtonPLA = new RadioButton();
      radioButtonABS = new RadioButton();
      radioButtonHIPS = new RadioButton();
      buttonOK = new Button();
      buttonCancel = new Button();
      groupBox1 = new GroupBox();
      groupBox4 = new GroupBox();
      radioButtonTGH = new RadioButton();
      radioButtonABSR = new RadioButton();
      radioButtonCAM = new RadioButton();
      textBoxFilamentTemp = new TextBox();
      groupBox3 = new GroupBox();
      buttonUnlock = new Button();
      groupBox2 = new GroupBox();
      checkBoxCalibrate = new CheckBox();
      checkBoxUsePreprocessors = new CheckBox();
      groupBoxSDCard = new GroupBox();
      comboBoxJobMode = new ComboBox();
      groupBox1.SuspendLayout();
      groupBox4.SuspendLayout();
      groupBox3.SuspendLayout();
      groupBox2.SuspendLayout();
      groupBoxSDCard.SuspendLayout();
      SuspendLayout();
      radioButtonPLA.AutoSize = true;
      radioButtonPLA.Checked = true;
      radioButtonPLA.Location = new Point(15, 18);
      radioButtonPLA.Name = "radioButtonPLA";
      radioButtonPLA.Size = new Size(46, 16);
      radioButtonPLA.TabIndex = 0;
      radioButtonPLA.TabStop = true;
      radioButtonPLA.Text = "PLA";
      radioButtonPLA.UseVisualStyleBackColor = true;
      radioButtonPLA.CheckedChanged += new EventHandler(radioButtonPLA_CheckedChanged);
      radioButtonABS.AutoSize = true;
      radioButtonABS.Location = new Point(15, 40);
      radioButtonABS.Name = "radioButtonABS";
      radioButtonABS.Size = new Size(47, 16);
      radioButtonABS.TabIndex = 1;
      radioButtonABS.Text = "ABS";
      radioButtonABS.UseVisualStyleBackColor = true;
      radioButtonABS.CheckedChanged += new EventHandler(radioButtonABS_CheckedChanged);
      radioButtonHIPS.AutoSize = true;
      radioButtonHIPS.Location = new Point(15, 60);
      radioButtonHIPS.Name = "radioButtonHIPS";
      radioButtonHIPS.Size = new Size(50, 16);
      radioButtonHIPS.TabIndex = 2;
      radioButtonHIPS.Text = "HIPS";
      radioButtonHIPS.UseVisualStyleBackColor = true;
      radioButtonHIPS.CheckedChanged += new EventHandler(radioButtonHIPS_CheckedChanged);
      buttonOK.Location = new Point(149, 230);
      buttonOK.Name = "buttonOK";
      buttonOK.Size = new Size(106, 18);
      buttonOK.TabIndex = 3;
      buttonOK.Text = "OK";
      buttonOK.UseVisualStyleBackColor = true;
      buttonOK.Click += new EventHandler(buttonOK_Click);
      buttonCancel.Location = new Point(278, 230);
      buttonCancel.Name = "buttonCancel";
      buttonCancel.Size = new Size(106, 18);
      buttonCancel.TabIndex = 4;
      buttonCancel.Text = "Cancel";
      buttonCancel.UseVisualStyleBackColor = true;
      buttonCancel.Click += new EventHandler(buttonCancel_Click);
      groupBox1.Controls.Add((Control)groupBox4);
      groupBox1.Location = new Point(12, 19);
      groupBox1.Name = "groupBox1";
      groupBox1.Size = new Size(245, 191);
      groupBox1.TabIndex = 5;
      groupBox1.TabStop = false;
      groupBox1.Text = "Filament Options";
      groupBox4.Controls.Add((Control)radioButtonTGH);
      groupBox4.Controls.Add((Control)radioButtonPLA);
      groupBox4.Controls.Add((Control)radioButtonABSR);
      groupBox4.Controls.Add((Control)radioButtonABS);
      groupBox4.Controls.Add((Control)radioButtonCAM);
      groupBox4.Controls.Add((Control)radioButtonHIPS);
      groupBox4.Location = new Point(17, 18);
      groupBox4.Name = "groupBox4";
      groupBox4.Size = new Size(213, 84);
      groupBox4.TabIndex = 11;
      groupBox4.TabStop = false;
      groupBox4.Text = "Type";
      radioButtonTGH.AutoSize = true;
      radioButtonTGH.Location = new Point(118, 18);
      radioButtonTGH.Name = "radioButtonTGH";
      radioButtonTGH.Size = new Size(48, 16);
      radioButtonTGH.TabIndex = 3;
      radioButtonTGH.Text = "TGH";
      radioButtonTGH.UseVisualStyleBackColor = true;
      radioButtonTGH.CheckedChanged += new EventHandler(radioButtonTGH_CheckedChanged);
      radioButtonABSR.AutoSize = true;
      radioButtonABSR.Location = new Point(118, 60);
      radioButtonABSR.Name = "radioButtonABSR";
      radioButtonABSR.Size = new Size(61, 16);
      radioButtonABSR.TabIndex = 3;
      radioButtonABSR.Text = "ABS-R";
      radioButtonABSR.UseVisualStyleBackColor = true;
      radioButtonABSR.CheckedChanged += new EventHandler(radioButtonABSR_CheckedChanged);
      radioButtonCAM.Location = new Point(118, 36);
      radioButtonCAM.Name = "radioButtonCAM";
      radioButtonCAM.Size = new Size(121, 22);
      radioButtonCAM.TabIndex = 4;
      radioButtonCAM.Text = "CAM";
      radioButtonCAM.CheckedChanged += new EventHandler(radioButtonCAM_CheckedChanged);
      textBoxFilamentTemp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      textBoxFilamentTemp.Location = new Point(10, 17);
      textBoxFilamentTemp.Name = "textBoxFilamentTemp";
      textBoxFilamentTemp.Size = new Size(82, 21);
      textBoxFilamentTemp.TabIndex = 10;
      groupBox3.Controls.Add((Control)textBoxFilamentTemp);
      groupBox3.Location = new Point(27, 126);
      groupBox3.Name = "groupBox3";
      groupBox3.Size = new Size(213, 41);
      groupBox3.TabIndex = 7;
      groupBox3.TabStop = false;
      groupBox3.Text = "Temperature";
      buttonUnlock.Location = new Point(192, 180);
      buttonUnlock.Name = "buttonUnlock";
      buttonUnlock.Size = new Size(59, 18);
      buttonUnlock.TabIndex = 5;
      buttonUnlock.Text = "Edit";
      buttonUnlock.UseVisualStyleBackColor = true;
      buttonUnlock.Click += new EventHandler(buttonEdit_Click);
      groupBox2.Controls.Add((Control)checkBoxCalibrate);
      groupBox2.Controls.Add((Control)checkBoxUsePreprocessors);
      groupBox2.Location = new Point(271, 19);
      groupBox2.Name = "groupBox2";
      groupBox2.Size = new Size(245, 74);
      groupBox2.TabIndex = 6;
      groupBox2.TabStop = false;
      groupBox2.Text = "Print Options";
      checkBoxCalibrate.AutoSize = true;
      checkBoxCalibrate.Location = new Point(12, 49);
      checkBoxCalibrate.Name = "checkBoxCalibrate";
      checkBoxCalibrate.Size = new Size(158, 16);
      checkBoxCalibrate.TabIndex = 1;
      checkBoxCalibrate.Text = "Calibrate before printing";
      checkBoxCalibrate.UseVisualStyleBackColor = true;
      checkBoxUsePreprocessors.AutoSize = true;
      checkBoxUsePreprocessors.Checked = true;
      checkBoxUsePreprocessors.CheckState = CheckState.Checked;
      checkBoxUsePreprocessors.Location = new Point(12, 26);
      checkBoxUsePreprocessors.Name = "checkBoxUsePreprocessors";
      checkBoxUsePreprocessors.Size = new Size(162, 16);
      checkBoxUsePreprocessors.TabIndex = 0;
      checkBoxUsePreprocessors.Text = "Use M3D Preprocessors";
      checkBoxUsePreprocessors.UseVisualStyleBackColor = true;
      groupBoxSDCard.Controls.Add((Control)comboBoxJobMode);
      groupBoxSDCard.Location = new Point(275, 104);
      groupBoxSDCard.Name = "groupBoxSDCard";
      groupBoxSDCard.Size = new Size(240, 105);
      groupBoxSDCard.TabIndex = 8;
      groupBoxSDCard.TabStop = false;
      groupBoxSDCard.Text = "Untethered Printing";
      comboBoxJobMode.FormattingEnabled = true;
      comboBoxJobMode.Location = new Point(12, 25);
      comboBoxJobMode.Name = "comboBoxJobMode";
      comboBoxJobMode.Size = new Size(217, 20);
      comboBoxJobMode.TabIndex = 0;
      comboBoxJobMode.SelectedIndexChanged += new EventHandler(comboBoxJobMode_SelectedIndexChanged);
      AutoScaleDimensions = new SizeF(7f, 12f);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(530, 258);
      ControlBox = false;
      Controls.Add((Control)groupBoxSDCard);
      Controls.Add((Control)groupBox3);
      Controls.Add((Control)buttonUnlock);
      Controls.Add((Control)groupBox2);
      Controls.Add((Control)groupBox1);
      Controls.Add((Control)buttonCancel);
      Controls.Add((Control)buttonOK);
      Name = nameof (ManualPrintOptions);
      Text = "Print Options";
      groupBox1.ResumeLayout(false);
      groupBox4.ResumeLayout(false);
      groupBox4.PerformLayout();
      groupBox3.ResumeLayout(false);
      groupBox3.PerformLayout();
      groupBox2.ResumeLayout(false);
      groupBox2.PerformLayout();
      groupBoxSDCard.ResumeLayout(false);
      ResumeLayout(false);
    }
  }
}
