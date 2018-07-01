// Decompiled with JetBrains decompiler
// Type: M3D.SendDebugEmail
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      this.InitializeComponent();
      this.debugText = debugText;
    }

    public void Send(string firstName, string lastName, string email, string subject, string body)
    {
      try
      {
        MailMessage message = new MailMessage();
        SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
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
      if (this.textBox1.Text == "" || this.textBox2.Text == "" || this.textBox3.Text == "")
      {
        int num = (int) MessageBox.Show("Please fill in all the fields.", "Send Debug Log Error", MessageBoxButtons.OK);
      }
      else
      {
        this.Send(this.textBox1.Text, this.textBox2.Text, this.textBox3.Text, "M3D user debug log", this.debugText);
        this.DialogResult = DialogResult.OK;
      }
    }

    private void buttonClose_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.label4 = new Label();
      this.label3 = new Label();
      this.label2 = new Label();
      this.label1 = new Label();
      this.textBox3 = new TextBox();
      this.textBox2 = new TextBox();
      this.textBox1 = new TextBox();
      this.button1 = new Button();
      this.buttonClose = new Button();
      this.pictureBox1 = new PictureBox();
      ((ISupportInitialize) this.pictureBox1).BeginInit();
      this.SuspendLayout();
      this.label4.AutoSize = true;
      this.label4.Location = new Point(12, 71);
      this.label4.Name = "label4";
      this.label4.Size = new Size(35, 13);
      this.label4.TabIndex = 27;
      this.label4.Text = "Email:";
      this.label3.AutoSize = true;
      this.label3.Location = new Point(227, 45);
      this.label3.Name = "label3";
      this.label3.Size = new Size(61, 13);
      this.label3.TabIndex = 26;
      this.label3.Text = "Last Name:";
      this.label2.AutoSize = true;
      this.label2.Location = new Point(12, 45);
      this.label2.Name = "label2";
      this.label2.Size = new Size(60, 13);
      this.label2.TabIndex = 25;
      this.label2.Text = "First Name:";
      this.label1.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.label1.Location = new Point(9, 10);
      this.label1.Name = "label1";
      this.label1.Size = new Size(449, 16);
      this.label1.TabIndex = 24;
      this.label1.Text = "Please enter your name and email address so that we can get back to you.";
      this.textBox3.Location = new Point(89, 68);
      this.textBox3.Name = "textBox3";
      this.textBox3.Size = new Size(121, 20);
      this.textBox3.TabIndex = 23;
      this.textBox2.Location = new Point(294, 42);
      this.textBox2.Name = "textBox2";
      this.textBox2.Size = new Size(125, 20);
      this.textBox2.TabIndex = 22;
      this.textBox1.Location = new Point(89, 42);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new Size(121, 20);
      this.textBox1.TabIndex = 21;
      this.button1.Location = new Point(302, 132);
      this.button1.Name = "button1";
      this.button1.Size = new Size(78, 28);
      this.button1.TabIndex = 20;
      this.button1.Text = "Send";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new EventHandler(this.button1_Click);
      this.buttonClose.Location = new Point(386, 132);
      this.buttonClose.Name = "buttonClose";
      this.buttonClose.Size = new Size(78, 28);
      this.buttonClose.TabIndex = 19;
      this.buttonClose.Text = "Cancel";
      this.buttonClose.UseVisualStyleBackColor = true;
      this.buttonClose.Click += new EventHandler(this.buttonClose_Click);
      this.pictureBox1.Image = (Image) Resources.m3dlogo;
      this.pictureBox1.InitialImage = (Image) Resources.m3dlogo;
      this.pictureBox1.Location = new Point(12, 126);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new Size(133, 34);
      this.pictureBox1.TabIndex = 28;
      this.pictureBox1.TabStop = false;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(477, 176);
      this.Controls.Add((Control) this.pictureBox1);
      this.Controls.Add((Control) this.label4);
      this.Controls.Add((Control) this.label3);
      this.Controls.Add((Control) this.label2);
      this.Controls.Add((Control) this.label1);
      this.Controls.Add((Control) this.textBox3);
      this.Controls.Add((Control) this.textBox2);
      this.Controls.Add((Control) this.textBox1);
      this.Controls.Add((Control) this.button1);
      this.Controls.Add((Control) this.buttonClose);
      this.Name = nameof (SendDebugEmail);
      this.Text = nameof (SendDebugEmail);
      ((ISupportInitialize) this.pictureBox1).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
