using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.TextLocalization;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller.Settings;
using OpenTK.Graphics;
using QuickFont;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace M3D.GUI.Views
{
  public class MessagePopUp : BorderedImageFrame
  {
    private Queue<string> message_queue;
    private bool hasmessage;
    private bool hadmessage;
    private TextWidget text_information;
    private SettingsManager mainLogicController;
    private object threadsync_spooler;
    private const float DisplayTimeMS = 3000f;
    private const float TransitionTimeMS = 500f;
    private Stopwatch mytimer;

    public MessagePopUp(int ID, SettingsManager mainLogicController)
      : base(ID, null)
    {
      threadsync_spooler = new object();
      this.mainLogicController = mainLogicController;
      message_queue = new Queue<string>();
      hasmessage = false;
      mytimer = new Stopwatch();
      mytimer.Reset();
      mytimer.Stop();
    }

    public void AddMessageToQueue(string message)
    {
      if (message.StartsWith("T_"))
      {
        message = Locale.GlobalLocale.T(message);
      }

      lock (threadsync_spooler)
      {
        message_queue.Enqueue(message);
      }
    }

    public void Init(GUIHost host)
    {
      Sprite.pixel_perfect = true;
      Init(host, "guicontrols", 768f, 384f, 895f, 511f, 14, 14, 64, 14, 14, 64);
      SetSize(300, 100);
      X = 36;
      Y = 0;
      text_information = new TextWidget(0)
      {
        Text = "Job Started",
        Size = FontSize.Medium,
        Alignment = QFontAlignment.Centre,
        VAlignment = TextVerticalAlignment.Middle
      };
      text_information.SetPosition(14, 14);
      text_information.SetSize(272, 72);
      text_information.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      AddChildElement(text_information);
      host.AddControlElement(this);
      Sprite.pixel_perfect = false;
    }

    public void Reshow()
    {
      if (hasmessage || !hadmessage)
      {
        return;
      }

      hasmessage = true;
    }

    public override void OnRender(GUIHost host)
    {
      if (!hasmessage)
      {
        var spoolerMessage = GetSpoolerMessage();
        if (!string.IsNullOrEmpty(spoolerMessage))
        {
          text_information.Text = spoolerMessage;
          hasmessage = true;
          hadmessage = true;
        }
      }
      if (hasmessage)
      {
        mytimer.Stop();
        var elapsedMilliseconds = (float)mytimer.ElapsedMilliseconds;
        if (elapsedMilliseconds < 500.0)
        {
          Y = (int) (-100.0 * (elapsedMilliseconds / 500f));
        }
        else if (elapsedMilliseconds < 3500.0)
        {
          Y = -100;
        }
        else if (elapsedMilliseconds < 4000.0)
        {
          Y = (int) (-100.0 * (1.0 - (elapsedMilliseconds - 3500.0) / 500.0));
        }
        else
        {
          Y = 0;
          hasmessage = false;
          mytimer.Reset();
        }
        if (hasmessage)
        {
          mytimer.Start();
        }
      }
      if (Y >= 0)
      {
        return;
      }

      base.OnRender(host);
    }

    public string GetSpoolerMessage()
    {
      var str = (string) null;
      try
      {
        lock (threadsync_spooler)
        {
          if (message_queue.Count != 0)
          {
            str = message_queue.Dequeue();
          }
        }
      }
      catch (Exception ex)
      {
      }
      return str;
    }
  }
}
