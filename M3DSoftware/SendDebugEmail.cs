using M3D.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;

namespace M3D
{
  public class SendDebugEmail : Form
  {
    private static string m3dSupportEmailAddress = "m3ddebug@gmail.com";
    private static string userName = "m3ddebug@gmail.com";
    private static string password = "debuglogger";
    private string debugText;
    private IContainer components;
    private PictureBox pictureBox1;
    private Label label4;
    private Label label3;
    private Label label2;
    private Label label1;
    private TextBox textBox3;
    private TextBox textBox2;
    private TextBox textBox1;
    private Button button1;
    private Button buttonClose;

    public SendDebugEmail(string debugText)
    {
      InitializeComponent();
      this.debugText = debugText;
    }

    public void Send(string firstName, string lastName, string email, string subject, string body)
    {
      try
      {
        var message = new MailMessage();
        var smtpClient = new SmtpClient("smtp.gmail.com");
        message.From = new MailAddress(email);
        message.To.Add(SendDebugEmail.m3dSupportEmailAddress);
        message.Subject = subject;
        message.Body = "Debug log from: " + firstName + " " + lastName + " " + email + Environment.NewLine + body;
        smtpClient.Port = 587;
        smtpClient.Credentials = (ICredentialsByHost) new NetworkCredential(SendDebugEmail.userName, SendDebugEmail.password);
        smtpClient.EnableSsl = true;
        smtpClient.Send(message);
      }
      catch (Exception ex)
      {
      }
    }

    private void button1_Click(object sender, EventArgs e)
    {
      if (textBox1.Text == "" || textBox2.Text == "" || textBox3.Text == "")
      {
        var num = (int) MessageBox.Show("Please fill in all the fields.", "Send Debug Log Error", MessageBoxButtons.OK);
      }
      else
      {
        Send(textBox1.Text, textBox2.Text, textBox3.Text, "M3D user debug log", debugText);
        DialogResult = DialogResult.OK;
      }
    }

    private void buttonClose_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
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
      label4 = new Label();
      label3 = new Label();
      label2 = new Label();
      label1 = new Label();
      textBox3 = new TextBox();
      textBox2 = new TextBox();
      textBox1 = new TextBox();
      button1 = new Button();
      buttonClose = new Button();
      pictureBox1 = new PictureBox();
      ((ISupportInitialize)pictureBox1).BeginInit();
      SuspendLayout();
      label4.AutoSize = true;
      label4.Location = new Point(12, 71);
      label4.Name = "label4";
      label4.Size = new Size(35, 13);
      label4.TabIndex = 27;
      label4.Text = "Email:";
      label3.AutoSize = true;
      label3.Location = new Point(227, 45);
      label3.Name = "label3";
      label3.Size = new Size(61, 13);
      label3.TabIndex = 26;
      label3.Text = "Last Name:";
      label2.AutoSize = true;
      label2.Location = new Point(12, 45);
      label2.Name = "label2";
      label2.Size = new Size(60, 13);
      label2.TabIndex = 25;
      label2.Text = "First Name:";
      label1.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      label1.Location = new Point(9, 10);
      label1.Name = "label1";
      label1.Size = new Size(449, 16);
      label1.TabIndex = 24;
      label1.Text = "Please enter your name and email address so that we can get back to you.";
      textBox3.Location = new Point(89, 68);
      textBox3.Name = "textBox3";
      textBox3.Size = new Size(121, 20);
      textBox3.TabIndex = 23;
      textBox2.Location = new Point(294, 42);
      textBox2.Name = "textBox2";
      textBox2.Size = new Size(125, 20);
      textBox2.TabIndex = 22;
      textBox1.Location = new Point(89, 42);
      textBox1.Name = "textBox1";
      textBox1.Size = new Size(121, 20);
      textBox1.TabIndex = 21;
      button1.Location = new Point(302, 132);
      button1.Name = "button1";
      button1.Size = new Size(78, 28);
      button1.TabIndex = 20;
      button1.Text = "Send";
      button1.UseVisualStyleBackColor = true;
      button1.Click += new EventHandler(button1_Click);
      buttonClose.Location = new Point(386, 132);
      buttonClose.Name = "buttonClose";
      buttonClose.Size = new Size(78, 28);
      buttonClose.TabIndex = 19;
      buttonClose.Text = "Cancel";
      buttonClose.UseVisualStyleBackColor = true;
      buttonClose.Click += new EventHandler(buttonClose_Click);
      pictureBox1.Image = (Image) Resources.m3dlogo;
      pictureBox1.InitialImage = (Image) Resources.m3dlogo;
      pictureBox1.Location = new Point(12, 126);
      pictureBox1.Name = "pictureBox1";
      pictureBox1.Size = new Size(133, 34);
      pictureBox1.TabIndex = 28;
      pictureBox1.TabStop = false;
      AutoScaleDimensions = new SizeF(6f, 13f);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(477, 176);
      Controls.Add((Control)pictureBox1);
      Controls.Add((Control)label4);
      Controls.Add((Control)label3);
      Controls.Add((Control)label2);
      Controls.Add((Control)label1);
      Controls.Add((Control)textBox3);
      Controls.Add((Control)textBox2);
      Controls.Add((Control)textBox1);
      Controls.Add((Control)button1);
      Controls.Add((Control)buttonClose);
      Name = nameof (SendDebugEmail);
      Text = nameof (SendDebugEmail);
      ((ISupportInitialize)pictureBox1).EndInit();
      ResumeLayout(false);
      PerformLayout();
    }
  }
}
