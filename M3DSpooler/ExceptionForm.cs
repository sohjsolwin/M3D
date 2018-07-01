// Decompiled with JetBrains decompiler
// Type: M3D.Spooler.ExceptionForm
// Assembly: M3DSpooler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 88304559-4DEB-43F9-B51D-EEDFDC1EF62E
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\Resources\Spooler\M3DSpooler.exe

using M3D.Spooling.Core;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace M3D.Spooler
{
  public class ExceptionForm : Form
  {
    private string debugFile = Paths.LogPath;
    private static bool Singleton;
    public static Form form1;
    private Exception the_exception;
    private string extra_info;
    private IContainer components;
    private Button buttonClose;
    private Label label1;
    private RichTextBox textBox1;
    private PictureBox pictureBox1;
    private Label labelVersion;
    private Button button1;

    public ExceptionForm()
    {
      if (!ExceptionForm.Singleton)
        this.InitializeComponent(' ');
      ExceptionForm.Singleton = true;
    }

    private void buttonClose_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    protected override void OnLoad(EventArgs e)
    {
      string str = ExceptionForm.ToMessageAndCompleteStacktrace(this.the_exception);
      if (!string.IsNullOrEmpty(this.extra_info))
        str = "DEBUG STRING = " + this.extra_info + "\r\n\r\n" + str;
      this.textBox1.Text = str;
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
      Form form1 = ExceptionForm.form1;
      int num = (int) new ExceptionForm()
      {
        the_exception = e,
        extra_info = extra_info
      }.ShowDialog();
      Application.Exit();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void InitializeComponent(char e)
    {
      this.SuspendLayout();
      this.ClientSize = new Size(284, 261);
      this.Name = nameof (ExceptionForm);
      this.Load += new EventHandler(this.ExceptionForm_Load);
      this.ResumeLayout(false);
    }

    private void ExceptionForm_Load(object sender, EventArgs e)
    {
    }

    private void label1_Click(object sender, EventArgs e)
    {
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
      this.textBox1 = new RichTextBox();
      this.labelVersion = new Label();
      this.pictureBox1 = new PictureBox();
      this.button1 = new Button();
      ((ISupportInitialize) this.pictureBox1).BeginInit();
      this.SuspendLayout();
      this.buttonClose.Location = new Point(388, 235);
      this.buttonClose.Name = "buttonClose";
      this.buttonClose.Size = new Size(119, 32);
      this.buttonClose.TabIndex = 0;
      this.buttonClose.Text = "Close Application";
      this.buttonClose.UseVisualStyleBackColor = true;
      this.buttonClose.Click += new EventHandler(this.buttonClose_Click);
      this.label1.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.label1.Location = new Point(7, 5);
      this.label1.Name = "label1";
      this.label1.Size = new Size(499, 51);
      this.label1.TabIndex = 1;
      this.label1.Text = "Sorry, but there was a problem in the software. Please restart the program. If this problem persists, please send the message below to us and we will do all that we can to fix the problem. ";
      this.label1.Click += new EventHandler(this.label1_Click);
      this.textBox1.Location = new Point(7, 59);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new Size(500, 156);
      this.textBox1.TabIndex = 2;
      this.textBox1.Text = "";
      this.labelVersion.AutoSize = true;
      this.labelVersion.Location = new Point(12, 218);
      this.labelVersion.Name = "labelVersion";
      this.labelVersion.Size = new Size(114, 13);
      this.labelVersion.TabIndex = 4;
      this.labelVersion.Text = "Version 2015-02-11-01";
      this.pictureBox1.Location = new Point(12, 235);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new Size(133, 38);
      this.pictureBox1.TabIndex = 3;
      this.pictureBox1.TabStop = false;
      this.button1.Location = new Point(308, 235);
      this.button1.Name = "button1";
      this.button1.Size = new Size(76, 32);
      this.button1.TabIndex = 9;
      this.button1.Text = "Send";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Visible = false;
      this.button1.Click += new EventHandler(this.button1_Click);
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(519, 279);
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
