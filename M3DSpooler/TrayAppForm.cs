// Decompiled with JetBrains decompiler
// Type: M3D.Spooler.TrayAppForm
// Assembly: M3DSpooler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 88304559-4DEB-43F9-B51D-EEDFDC1EF62E
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\Resources\Spooler\M3DSpooler.exe

using M3D.Spooler.Properties;
using M3D.Spooling.Client;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace M3D.Spooler
{
  public class TrayAppForm : MainForm
  {
    private NotifyIcon trayIcon;
    private ContextMenu trayMenu;
    public static bool start_invisible;

    public TrayAppForm(SpoolerClientBuiltIn spooler_client)
      : base(spooler_client)
    {
      this.trayMenu = new ContextMenu();
      this.trayMenu.MenuItems.Add("Exit", new EventHandler(this.OnExit));
      this.trayIcon = new NotifyIcon();
      this.trayIcon.Text = "M3D Spooler";
      this.trayIcon.Icon = new Icon(Resources.M3D, 40, 40);
      this.trayIcon.ContextMenu = this.trayMenu;
      this.trayIcon.Visible = true;
      this.TopLevel = true;
      this.trayIcon.Click += new EventHandler(this.notifyIcon1_Click);
    }

    protected override void OnLoad(EventArgs e)
    {
      if (TrayAppForm.start_invisible)
      {
        this.Visible = false;
        this.ShowInTaskbar = false;
      }
      base.OnLoad(e);
    }

    private void OnExit(object sender, EventArgs e)
    {
      this.m_bShutdownByUser = true;
      this.Close();
    }

    protected override void Dispose(bool bIsDisposing)
    {
      if (bIsDisposing)
        this.trayIcon.Dispose();
      base.Dispose(bIsDisposing);
    }

    private void notifyIcon1_Click(object sender, EventArgs e)
    {
      this.ShowSpooler();
    }
  }
}
