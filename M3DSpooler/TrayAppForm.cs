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
      trayMenu = new ContextMenu();
      trayMenu.MenuItems.Add("Exit", new EventHandler(OnExit));
      trayIcon = new NotifyIcon
      {
        Text = "M3D Spooler",
        Icon = new Icon(Resources.M3D, 40, 40),
        ContextMenu = trayMenu,
        Visible = true
      };
      TopLevel = true;
      trayIcon.Click += new EventHandler(notifyIcon1_Click);
    }

    protected override void OnLoad(EventArgs e)
    {
      if (TrayAppForm.start_invisible)
      {
        Visible = false;
        ShowInTaskbar = false;
      }
      base.OnLoad(e);
    }

    private void OnExit(object sender, EventArgs e)
    {
      m_bShutdownByUser = true;
      Close();
    }

    protected override void Dispose(bool bIsDisposing)
    {
      if (bIsDisposing)
      {
        trayIcon.Dispose();
      }

      base.Dispose(bIsDisposing);
    }

    private void notifyIcon1_Click(object sender, EventArgs e)
    {
      ShowSpooler();
    }
  }
}
