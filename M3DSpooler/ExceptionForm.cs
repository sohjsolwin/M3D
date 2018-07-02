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
      {
        InitializeComponent(' ');
      }

      ExceptionForm.Singleton = true;
    }

    private void buttonClose_Click(object sender, EventArgs e)
    {
      Close();
    }

    protected override void OnLoad(EventArgs e)
    {
      var str = ExceptionForm.ToMessageAndCompleteStacktrace(the_exception);
      if (!string.IsNullOrEmpty(extra_info))
      {
        str = "DEBUG STRING = " + extra_info + "\r\n\r\n" + str;
      }

      textBox1.Text = str;
      StreamWriter streamWriter = File.AppendText(debugFile);
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
      var stringBuilder = new StringBuilder();
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
      ExceptionForm.ShowExceptionForm(e, null);
    }

    public static void ShowExceptionForm(Exception e, string extra_info)
    {
      Form form1 = ExceptionForm.form1;
      var num = (int) new ExceptionForm()
      {
        the_exception = e,
        extra_info = extra_info
      }.ShowDialog();
      Application.Exit();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void InitializeComponent(char e)
    {
      SuspendLayout();
      ClientSize = new Size(284, 261);
      Name = nameof (ExceptionForm);
      Load += new EventHandler(ExceptionForm_Load);
      ResumeLayout(false);
    }

    private void ExceptionForm_Load(object sender, EventArgs e)
    {
    }

    private void label1_Click(object sender, EventArgs e)
    {
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
      buttonClose = new Button();
      label1 = new Label();
      textBox1 = new RichTextBox();
      labelVersion = new Label();
      pictureBox1 = new PictureBox();
      button1 = new Button();
      ((ISupportInitialize)pictureBox1).BeginInit();
      SuspendLayout();
      buttonClose.Location = new Point(388, 235);
      buttonClose.Name = "buttonClose";
      buttonClose.Size = new Size(119, 32);
      buttonClose.TabIndex = 0;
      buttonClose.Text = "Close Application";
      buttonClose.UseVisualStyleBackColor = true;
      buttonClose.Click += new EventHandler(buttonClose_Click);
      label1.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
      label1.Location = new Point(7, 5);
      label1.Name = "label1";
      label1.Size = new Size(499, 51);
      label1.TabIndex = 1;
      label1.Text = "Sorry, but there was a problem in the software. Please restart the program. If this problem persists, please send the message below to us and we will do all that we can to fix the problem. ";
      label1.Click += new EventHandler(label1_Click);
      textBox1.Location = new Point(7, 59);
      textBox1.Name = "textBox1";
      textBox1.Size = new Size(500, 156);
      textBox1.TabIndex = 2;
      textBox1.Text = "";
      labelVersion.AutoSize = true;
      labelVersion.Location = new Point(12, 218);
      labelVersion.Name = "labelVersion";
      labelVersion.Size = new Size(114, 13);
      labelVersion.TabIndex = 4;
      labelVersion.Text = "Version 2015-02-11-01";
      pictureBox1.Location = new Point(12, 235);
      pictureBox1.Name = "pictureBox1";
      pictureBox1.Size = new Size(133, 38);
      pictureBox1.TabIndex = 3;
      pictureBox1.TabStop = false;
      button1.Location = new Point(308, 235);
      button1.Name = "button1";
      button1.Size = new Size(76, 32);
      button1.TabIndex = 9;
      button1.Text = "Send";
      button1.UseVisualStyleBackColor = true;
      button1.Visible = false;
      button1.Click += new EventHandler(button1_Click);
      AutoScaleDimensions = new SizeF(6f, 13f);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(519, 279);
      ControlBox = false;
      Controls.Add(button1);
      Controls.Add(labelVersion);
      Controls.Add(pictureBox1);
      Controls.Add(textBox1);
      Controls.Add(label1);
      Controls.Add(buttonClose);
      FormBorderStyle = FormBorderStyle.Fixed3D;
      Name = nameof (ExceptionForm);
      Text = "M3D GUI";
      ((ISupportInitialize)pictureBox1).EndInit();
      ResumeLayout(false);
      PerformLayout();
    }
  }
}
