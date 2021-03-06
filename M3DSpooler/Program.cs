﻿using M3D.Spooling.Client;
using M3D.Spooling.Common.Utils;
using M3D.Spooling.Core;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace M3D.Spooler
{
  internal static class Program
  {
    [STAThread]
    private static void Main(string[] args)
    {
      var filename = Path.Combine(Paths.SharedDataFolder, "spoolerclientlog.txt");
      Thread.CurrentThread.CurrentCulture = PrinterCompatibleString.PRINTER_CULTURE;
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.ThreadException += new ThreadExceptionEventHandler(Program.UIThreadException);
      Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Program.CurrentDomain_UnhandledException);
      var num = 100;
      var spooler_client = new SpoolerClientBuiltIn(new DebugLogger(filename, (uint)num))
      {
        IgnoreConnectingPrinters = false
      };
      if (spooler_client.StartInternalSpoolerSession() == SpoolerResult.Fail_Connect)
      {
        return;
      }

      for (var index = 0; index < args.Length; ++index)
      {
        if (args[index] == "A")
        {
          MainForm.AUTO_SHUTDOWN_IF_ALL_CLIENTS_DISCONNECT = true;
        }
        else if (args[index] == "H")
        {
          TrayAppForm.start_invisible = true;
        }
        else if (args[index] == "ZA91L")
        {
          spooler_client.StayInBootloader = true;
        }
        else if (args[index] == "NIF")
        {
          spooler_client.CHECK_INCOMPATIBLE_FIRMWARE = false;
        }
        else if (args[index] == "NOGAN")
        {
          spooler_client.CHECK_GANTRY_CLIPS = false;
        }
        else if (args[index] == "NOBED")
        {
          spooler_client.CHECK_BED_CALIBRATION = false;
        }
        else if (args[index] == "AUTO")
        {
          spooler_client.AUTO_UPDATE_FIRMWARE = true;
        }
      }
      spooler_client.CheckFirmware = true;
      Application.Run(new TrayAppForm(spooler_client));
    }

    private static void UIThreadException(object sender, ThreadExceptionEventArgs t)
    {
      try
      {
        Program.ShowExceptionHandler("Thread", t.Exception);
      }
      catch (Exception ex)
      {
        Program.ShowExceptionHandler("Thread exception handler", ex);
      }
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      try
      {
        Program.ShowExceptionHandler("Current Domain", (Exception) e.ExceptionObject);
      }
      catch (Exception ex)
      {
        Program.ShowExceptionHandler("Current Domain exception handler", ex);
      }
    }

    private static void ShowExceptionHandler(string source, Exception exp)
    {
      var str = "An application error occurred in " + source + ". Please contact the administrator with the following information.\n\nException: " + exp.Message + "\n\nStack Trace:\n" + exp.StackTrace;
      if (exp.InnerException != null)
      {
        str = str + "Inner Exception: " + exp.InnerException.Message + "\n\nStack Trace:\n" + exp.InnerException.StackTrace;
      }

      try
      {
        var num = (int) MessageBox.Show("Fatal Non-UI Error. Could not write the error to the event log. Reason: " + str, "Fatal Non-UI Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
      }
      finally
      {
        Application.Exit();
      }
    }
  }
}
