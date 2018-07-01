// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.MessagePopUp
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      : base(ID, (Element2D) null)
    {
      this.threadsync_spooler = new object();
      this.mainLogicController = mainLogicController;
      this.message_queue = new Queue<string>();
      this.hasmessage = false;
      this.mytimer = new Stopwatch();
      this.mytimer.Reset();
      this.mytimer.Stop();
    }

    public void AddMessageToQueue(string message)
    {
      if (message.StartsWith("T_"))
        message = Locale.GlobalLocale.T(message);
      lock (this.threadsync_spooler)
        this.message_queue.Enqueue(message);
    }

    public void Init(GUIHost host)
    {
      Sprite.pixel_perfect = true;
      this.Init(host, "guicontrols", 768f, 384f, 895f, 511f, 14, 14, 64, 14, 14, 64);
      this.SetSize(300, 100);
      this.X = 36;
      this.Y = 0;
      this.text_information = new TextWidget(0);
      this.text_information.Text = "Job Started";
      this.text_information.Size = FontSize.Medium;
      this.text_information.Alignment = QFontAlignment.Centre;
      this.text_information.VAlignment = TextVerticalAlignment.Middle;
      this.text_information.SetPosition(14, 14);
      this.text_information.SetSize(272, 72);
      this.text_information.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      this.AddChildElement((Element2D) this.text_information);
      host.AddControlElement((Element2D) this);
      Sprite.pixel_perfect = false;
    }

    public void Reshow()
    {
      if (this.hasmessage || !this.hadmessage)
        return;
      this.hasmessage = true;
    }

    public override void OnRender(GUIHost host)
    {
      if (!this.hasmessage)
      {
        string spoolerMessage = this.GetSpoolerMessage();
        if (!string.IsNullOrEmpty(spoolerMessage))
        {
          this.text_information.Text = spoolerMessage;
          this.hasmessage = true;
          this.hadmessage = true;
        }
      }
      if (this.hasmessage)
      {
        this.mytimer.Stop();
        float elapsedMilliseconds = (float) this.mytimer.ElapsedMilliseconds;
        if ((double) elapsedMilliseconds < 500.0)
          this.Y = (int) (-100.0 * (double) (elapsedMilliseconds / 500f));
        else if ((double) elapsedMilliseconds < 3500.0)
          this.Y = -100;
        else if ((double) elapsedMilliseconds < 4000.0)
        {
          this.Y = (int) (-100.0 * (1.0 - ((double) elapsedMilliseconds - 3500.0) / 500.0));
        }
        else
        {
          this.Y = 0;
          this.hasmessage = false;
          this.mytimer.Reset();
        }
        if (this.hasmessage)
          this.mytimer.Start();
      }
      if (this.Y >= 0)
        return;
      base.OnRender(host);
    }

    public string GetSpoolerMessage()
    {
      string str = (string) null;
      try
      {
        lock (this.threadsync_spooler)
        {
          if (this.message_queue.Count != 0)
            str = this.message_queue.Dequeue();
        }
      }
      catch (Exception ex)
      {
      }
      return str;
    }
  }
}
