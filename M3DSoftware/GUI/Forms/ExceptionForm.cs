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
      InitializeComponent();
    }

    private void buttonClose_Click(object sender, EventArgs e)
    {
      Close();
    }

    protected override void OnLoad(EventArgs e)
    {
      debugFile = Paths.DebugLogPath(DateTime.Now.Ticks / 10000L);
      Form1.debugLogger.Print(debugFile);
      var str = ExceptionForm.ToMessageAndCompleteStacktrace(the_exception);
      if (!string.IsNullOrEmpty(extra_info))
      {
        str = "DEBUG STRING = " + extra_info + "\r\n\r\n" + str;
      }

      textBox1.Text = str;
      labelVersion.Text = M3D.Spooling.Version.VersionText;
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
      {
        ExceptionForm.form1.StopTimers();
      }

      FileAssociationSingleInstance.UnRegisterAsSingleInstance();
      var num = (int) new ExceptionForm() { the_exception = e, extra_info = extra_info }.ShowDialog();
      Application.Exit();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      if (new SendDebugEmail(File.ReadAllText(debugFile)).ShowDialog() != DialogResult.OK)
      {
        return;
      }

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
      buttonClose = new Button();
      label1 = new Label();
      textBox1 = new TextBox();
      labelVersion = new Label();
      pictureBox1 = new PictureBox();
      button1 = new Button();
      ((ISupportInitialize)pictureBox1).BeginInit();
      SuspendLayout();
      buttonClose.Location = new Point(453, 217);
      buttonClose.Name = "buttonClose";
      buttonClose.Size = new Size(139, 30);
      buttonClose.TabIndex = 0;
      buttonClose.Text = "Close Application";
      buttonClose.UseVisualStyleBackColor = true;
      buttonClose.Click += new EventHandler(buttonClose_Click);
      label1.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      label1.Location = new Point(8, 5);
      label1.Name = "label1";
      label1.Size = new Size(582, 47);
      label1.TabIndex = 1;
      label1.Text = "Sorry, but there was a problem in the software. Please restart the program. If this problem persists, please send the message below to us and we will do all that we can to fix the problem. ";
      textBox1.Location = new Point(8, 54);
      textBox1.Multiline = true;
      textBox1.Name = "textBox1";
      textBox1.Size = new Size(583, 144);
      textBox1.TabIndex = 2;
      labelVersion.AutoSize = true;
      labelVersion.Location = new Point(14, 201);
      labelVersion.Name = "labelVersion";
      labelVersion.Size = new Size(130, 12);
      labelVersion.TabIndex = 4;
      labelVersion.Text = "Version 2015-02-11-01";
      pictureBox1.Image = (Image) Resources.m3dlogo;
      pictureBox1.InitialImage = (Image) Resources.m3dlogo;
      pictureBox1.Location = new Point(14, 217);
      pictureBox1.Name = "pictureBox1";
      pictureBox1.Size = new Size(155, 35);
      pictureBox1.TabIndex = 3;
      pictureBox1.TabStop = false;
      button1.Location = new Point(359, 217);
      button1.Name = "button1";
      button1.Size = new Size(89, 30);
      button1.TabIndex = 9;
      button1.Text = "Send";
      button1.UseVisualStyleBackColor = true;
      button1.Visible = false;
      button1.Click += new EventHandler(button1_Click);
      AutoScaleDimensions = new SizeF(7f, 12f);
      AutoScaleMode = AutoScaleMode.Font;
      AutoSize = true;
      ClientSize = new Size(605, 258);
      ControlBox = false;
      Controls.Add((Control)button1);
      Controls.Add((Control)labelVersion);
      Controls.Add((Control)pictureBox1);
      Controls.Add((Control)textBox1);
      Controls.Add((Control)label1);
      Controls.Add((Control)buttonClose);
      FormBorderStyle = FormBorderStyle.Fixed3D;
      Name = nameof (ExceptionForm);
      Text = "M3D GUI";
      ((ISupportInitialize)pictureBox1).EndInit();
      ResumeLayout(false);
      PerformLayout();
    }
  }
}
