// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Forms.ExceptionForm
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace M3D.GUI.Forms
{
  public class ExceptionForm : Form
  {
    private string debugFile;
    public static Form1 form1;
    private Exception the_exception;
    private string extra_info;
    private IContainer components;
    private Button buttonClose;
    private Label label1;
    private TextBox textBox1;
    private PictureBox pictureBox1;
    private Label labelVersion;
    private Button button1;

    public ExceptionForm()
    {
      this.InitializeComponent();
    }

    private void buttonClose_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    protected override void OnLoad(EventArgs e)
    {
      this.debugFile = Paths.DebugLogPath(DateTime.Now.Ticks / 10000L);
      Form1.debugLogger.Print(this.debugFile);
      string str = ExceptionForm.ToMessageAndCompleteStacktrace(this.the_exception);
      if (!string.IsNullOrEmpty(this.extra_info))
        str = "DEBUG STRING = " + this.extra_info + "\r\n\r\n" + str;
      this.textBox1.Text = str;
      this.labelVersion.Text = M3D.Spooling.Version.VersionText;
      StreamWriter streamWriter = File.AppendText(this.debugFile);
      streamWriter.WriteLine();
      streamWriter.WriteLine("--------------------------------------------------");
      streamWriter.WriteLine("Exception Text: ");
      streamWriter.WriteLine("--------------------------------------------------");
      streamWriter.WriteLine();
      streamWriter.WriteLine(str);
      streamWriter.Close();
      base.OnLoad(e);
    }

    public static string ToMessageAndCompleteStacktrace(Exception exception)
    {
      Exception exception1 = exception;
      StringBuilder stringBuilder = new StringBuilder();
      for (; exception1 != null; exception1 = exception1.InnerException)
      {
        stringBuilder.AppendLine("System Version: " + M3D.Spooling.Version.VersionText);
        stringBuilder.AppendLine("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -");
        stringBuilder.AppendLine("Exception type: " + exception1.GetType().FullName);
        stringBuilder.AppendLine("Message       : " + exception1.Message);
        stringBuilder.AppendLine("Stacktrace:");
        stringBuilder.AppendLine(exception1.StackTrace);
        stringBuilder.AppendLine();
      }
      return stringBuilder.ToString();
    }

    public static void ShowExceptionForm(Exception e)
    {
      ExceptionForm.ShowExceptionForm(e, (string) null);
    }

    public static void ShowExceptionForm(Exception e, string extra_info)
    {
      if (ExceptionForm.form1 != null)
        ExceptionForm.form1.StopTimers();
      FileAssociationSingleInstance.UnRegisterAsSingleInstance();
      int num = (int) new ExceptionForm() { the_exception = e, extra_info = extra_info }.ShowDialog();
      Application.Exit();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      if (new SendDebugEmail(File.ReadAllText(this.debugFile)).ShowDialog() != DialogResult.OK)
        return;
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
      this.buttonClose = new Button();
      this.label1 = new Label();
      this.textBox1 = new TextBox();
      this.labelVersion = new Label();
      this.pictureBox1 = new PictureBox();
      this.button1 = new Button();
      ((ISupportInitialize) this.pictureBox1).BeginInit();
      this.SuspendLayout();
      this.buttonClose.Location = new Point(453, 217);
      this.buttonClose.Name = "buttonClose";
      this.buttonClose.Size = new Size(139, 30);
      this.buttonClose.TabIndex = 0;
      this.buttonClose.Text = "Close Application";
      this.buttonClose.UseVisualStyleBackColor = true;
      this.buttonClose.Click += new EventHandler(this.buttonClose_Click);
      this.label1.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.label1.Location = new Point(8, 5);
      this.label1.Name = "label1";
      this.label1.Size = new Size(582, 47);
      this.label1.TabIndex = 1;
      this.label1.Text = "Sorry, but there was a problem in the software. Please restart the program. If this problem persists, please send the message below to us and we will do all that we can to fix the problem. ";
      this.textBox1.Location = new Point(8, 54);
      this.textBox1.Multiline = true;
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new Size(583, 144);
      this.textBox1.TabIndex = 2;
      this.labelVersion.AutoSize = true;
      this.labelVersion.Location = new Point(14, 201);
      this.labelVersion.Name = "labelVersion";
      this.labelVersion.Size = new Size(130, 12);
      this.labelVersion.TabIndex = 4;
      this.labelVersion.Text = "Version 2015-02-11-01";
      this.pictureBox1.Image = (Image) Resources.m3dlogo;
      this.pictureBox1.InitialImage = (Image) Resources.m3dlogo;
      this.pictureBox1.Location = new Point(14, 217);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new Size(155, 35);
      this.pictureBox1.TabIndex = 3;
      this.pictureBox1.TabStop = false;
      this.button1.Location = new Point(359, 217);
      this.button1.Name = "button1";
      this.button1.Size = new Size(89, 30);
      this.button1.TabIndex = 9;
      this.button1.Text = "Send";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Visible = false;
      this.button1.Click += new EventHandler(this.button1_Click);
      this.AutoScaleDimensions = new SizeF(7f, 12f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.AutoSize = true;
      this.ClientSize = new Size(605, 258);
      this.ControlBox = false;
      this.Controls.Add((Control) this.button1);
      this.Controls.Add((Control) this.labelVersion);
      this.Controls.Add((Control) this.pictureBox1);
      this.Controls.Add((Control) this.textBox1);
      this.Controls.Add((Control) this.label1);
      this.Controls.Add((Control) this.buttonClose);
      this.FormBorderStyle = FormBorderStyle.Fixed3D;
      this.Name = nameof (ExceptionForm);
      this.Text = "M3D GUI";
      ((ISupportInitialize) this.pictureBox1).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
