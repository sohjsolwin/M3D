using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Interfaces;
using M3D.Graphics.TextLocalization;
using M3D.Graphics.Widgets2D;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using System;
using System.Collections.Generic;

namespace M3D.GUI.Dialogs
{
  public class PopupMessageBox : Frame, IProcess
  {
    private bool result_processed = true;
    private PopupMessageBox.PopupResult popup_result = PopupMessageBox.PopupResult.Button2_NoCancel;
    public PopupMessageBox.OnUserSelectionDel OnUserSelection;
    private PopupMessageBox.OnUserSelectionDel MessageCallback;
    private PopupMessageBox.XMLButtonCallback XMLMessageCallback;
    private ElementStandardDelegate XMLOnUpdateCallback;
    public bool AllowMessages;
    private object threadsync_standardmsgs;
    private XMLFrame child_frame;
    private SpoolerMessage message;
    private object MessageUserData;
    private string MessageText;
    private GUIHost host;
    private bool hasmessage;
    private Queue<PopupMessageBox.MessageData> standard_queue;

    public PopupMessageBox(int ID)
      : base(ID)
    {
      tag = "PopupMessageBox.Frame";
      hasmessage = false;
      standard_queue = new Queue<PopupMessageBox.MessageData>();
      threadsync_standardmsgs = new object();
    }

    private PopupMessageBox.MessageData GetCriticalMessage()
    {
      if (!AllowMessages)
      {
        return null;
      }

      try
      {
        var messageData = (PopupMessageBox.MessageData) null;
        lock (threadsync_standardmsgs)
        {
          if (standard_queue.Count != 0)
          {
            messageData = standard_queue.Dequeue();
          }
        }
        return messageData;
      }
      catch (Exception ex)
      {
      }
      return null;
    }

    public void AddMessageToQueue(string message, string button1_text, string button2_text, PopupMessageBox.OnUserSelectionDel callback)
    {
      AddMessageToQueue(message, button1_text, button2_text, null, callback, null);
    }

    public void AddMessageToQueue(string message, string button1_text, string button2_text, string button3_text, PopupMessageBox.OnUserSelectionDel callback, object data)
    {
      AddMessageToQueue(new PopupMessageBox.MessageDataStandard(new SpoolerMessage(MessageType.UserDefined, message), PopupMessageBox.MessageBoxButtons.CUSTOM, callback, data)
      {
        custom_button1_text = button1_text,
        custom_button2_text = button2_text,
        custom_button3_text = button3_text
      });
    }

    public void AddMessageToQueue(PopupMessageBox.MessageDataStandard message_data)
    {
      lock (threadsync_standardmsgs)
      {
        standard_queue.Enqueue(message_data);
      }
    }

    public void AddMessageToQueue(string message)
    {
      AddMessageToQueue(message, PopupMessageBox.MessageBoxButtons.DEFAULT);
    }

    public void AddMessageToQueue(string message, PopupMessageBox.MessageBoxButtons buttons)
    {
      AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, message), buttons, null);
    }

    public void AddMessageToQueue(string message, PopupMessageBox.MessageBoxButtons buttons, PopupMessageBox.OnUserSelectionDel callback)
    {
      AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, message), buttons, callback);
    }

    public void AddMessageToQueue(string message, PopupMessageBox.MessageBoxButtons buttons, PopupMessageBox.OnUserSelectionDel callback, object data)
    {
      AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, message), buttons, callback, data);
    }

    public void AddMessageToQueue(SpoolerMessage message)
    {
      AddMessageToQueue(message, PopupMessageBox.MessageBoxButtons.DEFAULT, null);
    }

    public void AddMessageToQueue(SpoolerMessage message, PopupMessageBox.MessageBoxButtons buttons)
    {
      AddMessageToQueue(message, buttons, null);
    }

    public void AddMessageToQueue(SpoolerMessage message, PopupMessageBox.MessageBoxButtons buttons, PopupMessageBox.OnUserSelectionDel callback)
    {
      AddMessageToQueue(message, buttons, callback, null);
    }

    public void AddMessageToQueue(string message, string title, PopupMessageBox.MessageBoxButtons buttons, PopupMessageBox.OnUserSelectionDel callback)
    {
      lock (threadsync_standardmsgs)
      {
        standard_queue.Enqueue(new PopupMessageBox.MessageDataStandard(new SpoolerMessage(MessageType.UserDefined, message), title, buttons, callback, (object)null));
      }
    }

    public void AddMessageToQueue(SpoolerMessage message, PopupMessageBox.MessageBoxButtons buttons, PopupMessageBox.OnUserSelectionDel callback, object data)
    {
      lock (threadsync_standardmsgs)
      {
        standard_queue.Enqueue(new PopupMessageBox.MessageDataStandard(message, buttons, callback, data));
      }
    }

    public void AddXMLMessageToQueue(PopupMessageBox.MessageDataXML xmlMessageData)
    {
      lock (threadsync_standardmsgs)
      {
        standard_queue.Enqueue(xmlMessageData);
      }
    }

    public bool CriticalMessageShowing()
    {
      return !Visible;
    }

    public void Init(GUIHost host)
    {
      this.host = host;
      RelativeX = 0.0f;
      RelativeX = 0.0f;
      RelativeWidth = 1f;
      RelativeHeight = 1f;
      child_frame = new XMLFrame(0)
      {
        CenterHorizontallyInParent = true,
        CenterVerticallyInParent = true,
        RelativeWidth = 1f,
        RelativeHeight = 1f,
        AutoCenterYOffset = 50
      };
      AddChildElement(child_frame);
      Visible = false;
      host.AddProcess(this);
    }

    public void CheckCriticalMessages()
    {
      if (Visible || hasmessage)
      {
        return;
      }

      PopupMessageBox.MessageData criticalMessage = GetCriticalMessage();
      if (criticalMessage == null)
      {
        return;
      }

      if (criticalMessage.GetMessageType() == PopupMessageBox.MessageBoxType.Critical)
      {
        var message_details = (PopupMessageBox.MessageDataStandard) criticalMessage;
        MessageCallback = message_details.OnSelection;
        MessageUserData = message_details.data;
        MessageText = TranslateSpoolerMessage(message_details.message);
        SetMessageStandard(message_details);
        hasmessage = true;
        Visible = true;
      }
      else if (criticalMessage.GetMessageType() == PopupMessageBox.MessageBoxType.XML)
      {
        var messageDataXml = (PopupMessageBox.MessageDataXML) criticalMessage;
        MessageUserData = messageDataXml.data;
        XMLMessageCallback = messageDataXml.buttonCallback;
        XMLOnUpdateCallback = messageDataXml.onUpdateCallback;
        SetMessageXML(messageDataXml.message, messageDataXml.xmlsource);
        messageDataXml.onShowCallback?.Invoke(this, child_frame, host, messageDataXml.data);

        hasmessage = true;
        Visible = true;
      }
      Refresh();
    }

    public void Process()
    {
      if (!result_processed)
      {
        result_processed = true;
        if (MessageCallback != null)
        {
          MessageCallback(popup_result, message.Type, message.SerialNumber, MessageUserData);
        }
        else
        {
          if (OnUserSelection == null)
          {
            return;
          }

          OnUserSelection(popup_result, message.Type, message.SerialNumber, MessageUserData);
        }
      }
      else
      {
        if (Visible || !AllowMessages)
        {
          return;
        }

        CheckCriticalMessages();
      }
    }

    public void CloseCurrent()
    {
      if (!Visible)
      {
        return;
      }

      Visible = false;
      if (child_frame == null)
      {
        return;
      }

      child_frame.DoOnUpdate = null;
    }

    public override void OnRender(GUIHost host)
    {
      base.OnRender(host);
    }

    public void SetMessageXML(SpoolerMessage message, string message_box_xml)
    {
      Sprite.pixel_perfect = true;
      this.message = message;
      child_frame.Init(host, message_box_xml, new ButtonCallback(XMLFormButtonCallback));
      child_frame.DoOnUpdate = XMLOnUpdateCallback;
      child_frame.Refresh();
      host.SetFocus(child_frame);
      Sprite.pixel_perfect = false;
    }

    public string TranslateSpoolerMessage(SpoolerMessage incoming_messsage)
    {
      switch (incoming_messsage.Type)
      {
        case MessageType.FirmwareUpdateComplete:
        case MessageType.FirmwareMustBeUpdated:
          return Locale.GlobalLocale.T("T_FIRMWARE_UPDATE_COMPLETE");
        case MessageType.ResetPrinterConnection:
        case MessageType.FirmwareErrorCyclePower:
          return "Your printer is taking longer than usual. " + Locale.GlobalLocale.T("T_CYCLE_POWER");
        case MessageType.SDPrintIncompatibleFilament:
          return Locale.GlobalLocale.T("T_SDPrint_Incompatible_Filament");
        default:
          return incoming_messsage.ToString();
      }
    }

    private void SetMessageStandard(PopupMessageBox.MessageDataStandard message_details)
    {
      Sprite.pixel_perfect = true;
      child_frame.Init(host, "<?xml version=\"1.0\" encoding=\"utf-16\"?><XMLFrame id=\"1000\" width=\"400\" height=\"200\" center-vertically=\"1\" center-horizontally=\"1\"><ImageWidget id=\"1001\" x=\"0\" y=\"0\" relative-width=\"1.0\" relative-height=\"1.0\" src=\"guicontrols\" texture-u0=\"640\" texture-v0=\"320\" texture-u1=\"704\" texture-v1=\"383\" center-vertically=\"1\" center-horizontally=\"1\" leftbordersize-pixels=\"41\" rightbordersize-pixels=\"8\" minimumwidth=\"64\" topbordersize-pixels=\"35\" bottombordersize-pixels=\"8\" minimumheight=\"64\"></ImageWidget><TextWidget id=\"1002\" x=\"50\" y=\"2\" width=\"298\" height=\"35\" font-size=\"Large\" font-color=\"#FF808080\" alignment=\"Left\">Printer Message</TextWidget><TextWidget id=\"1003\" x=\"14\" y=\"14\" width=\"372\" height=\"150\" font-size=\"Medium\" font-color=\"#FF404040\" alignment=\"Centre\">Warning! Please make sure the print bed is clear. We wouldn't want any accidents.</TextWidget><ButtonWidget id=\"101\" x=\"92\" y=\"-50\" width=\"100\" height=\"32\" font-size=\"Medium\" alignment=\"Centre\" has_focus=\"1\">OK</ButtonWidget><ButtonWidget id=\"102\" x=\"208\" y=\"-50\" width=\"100\" height=\"32\" font-size=\"Medium\" alignment=\"Centre\">Cancel</ButtonWidget><ButtonWidget id=\"103\" x=\"208\" y=\"-50\" width=\"100\" height=\"32\" font-size=\"Medium\" alignment=\"Centre\" visible=\"0\" enabled=\"0\">Custom</ButtonWidget></XMLFrame>", new ButtonCallback(MyButtonCallback));
      child_frame.Refresh();
      Sprite.pixel_perfect = false;
      var childElement1 = (TextWidget)FindChildElement(1003);
      var childElement2 = (ButtonWidget)FindChildElement(101);
      var childElement3 = (ButtonWidget)FindChildElement(102);
      var childElement4 = (ButtonWidget)FindChildElement(103);
      ((TextWidget)FindChildElement(1002)).Text = message_details.title;
      var childElement5 = (Frame)FindChildElement(1000);
      message = message_details.message;
      if (message_details.buttons == PopupMessageBox.MessageBoxButtons.DEFAULT)
      {
        switch (message.Type)
        {
          case MessageType.PrinterConnected:
          case MessageType.JobComplete:
          case MessageType.JobCanceled:
          case MessageType.JobStarted:
          case MessageType.PrinterError:
          case MessageType.PrinterMessage:
          case MessageType.FirmwareUpdateComplete:
          case MessageType.FirmwareUpdateFailed:
          case MessageType.ResetPrinterConnection:
          case MessageType.UserDefined:
          case MessageType.RawData:
          case MessageType.PrinterNotConnected:
          case MessageType.MicroMotionControllerFailed:
          case MessageType.ModelOutOfPrintableBounds:
          case MessageType.IncompatibleSpooler:
          case MessageType.UnexpectedDisconnect:
          case MessageType.CantStartJobPrinterBusy:
          case MessageType.FirmwareMustBeUpdated:
          case MessageType.FirmwareErrorCyclePower:
            message_details.buttons = PopupMessageBox.MessageBoxButtons.OK;
            break;
          default:
            message_details.buttons = PopupMessageBox.MessageBoxButtons.OKCANCEL;
            break;
        }
      }
      if (message_details.buttons == PopupMessageBox.MessageBoxButtons.OK)
      {
        childElement4.Visible = false;
        childElement3.Visible = false;
        childElement2.SetPosition((childElement5.Width - childElement2.Width) / 2, -50);
        childElement2.Text = "T_OK";
        childElement2.Enabled = true;
      }
      else if (message_details.buttons == PopupMessageBox.MessageBoxButtons.OKCANCEL)
      {
        childElement2.Text = "T_OK";
        childElement3.Text = "T_Cancel";
        childElement3.Visible = true;
        childElement4.Visible = false;
        childElement2.SetPosition(childElement5.Width / 2 - 108, -50);
        childElement3.SetPosition(208, -50);
        childElement2.Enabled = true;
        childElement3.Enabled = true;
      }
      else if (message_details.buttons == PopupMessageBox.MessageBoxButtons.YESNO)
      {
        childElement2.Text = "T_Yes";
        childElement3.Text = "T_No";
        childElement3.Visible = true;
        childElement4.Visible = false;
        childElement2.SetPosition(childElement5.Width / 2 - 108, -50);
        childElement3.SetPosition(208, -50);
        childElement2.Enabled = true;
        childElement3.Enabled = true;
      }
      else if (message_details.buttons == PopupMessageBox.MessageBoxButtons.CUSTOM)
      {
        if (message_details.custom_button1_text != null && message_details.custom_button2_text != null && message_details.custom_button3_text != null)
        {
          childElement2.Text = message_details.custom_button1_text;
          childElement3.Text = message_details.custom_button2_text;
          childElement4.Text = message_details.custom_button3_text;
          childElement2.Visible = true;
          childElement3.Visible = true;
          childElement4.Visible = true;
          childElement4.Enabled = true;
          if (childElement2.Text.Length > 8)
          {
            childElement2.Width = 150;
            childElement2.Height = 72;
            childElement5.Width += 50;
            childElement5.Height = 250;
            childElement1.Width += 50;
          }
          if (childElement3.Text.Length > 8)
          {
            childElement3.Width = 150;
            childElement3.Height = 72;
            childElement5.Width += 70;
            childElement5.Height = 250;
            childElement1.Width += 50;
          }
          if (childElement4.Text.Length > 8)
          {
            childElement4.Width = 150;
            childElement4.Height = 72;
            childElement5.Width += 50;
            childElement5.Height = 250;
            childElement1.Width += 50;
          }
          childElement2.SetPosition(10, -(childElement2.Height + 18));
          childElement3.SetPosition(childElement2.X + childElement2.Width + 5, -(childElement3.Height + 18));
          childElement4.SetPosition(-(childElement4.Width + 10), -(childElement4.Height + 18));
          childElement2.Enabled = true;
          childElement3.Enabled = true;
        }
        else if (message_details.custom_button1_text != null && message_details.custom_button2_text != null)
        {
          childElement2.Text = message_details.custom_button1_text;
          childElement3.Text = message_details.custom_button2_text;
          childElement3.Visible = true;
          childElement2.SetPosition(childElement5.Width / 2 - 108, -50);
          childElement3.SetPosition(208, -50);
          childElement2.Enabled = true;
          childElement3.Enabled = true;
        }
        else if (message_details.custom_button1_text != null)
        {
          childElement3.Visible = false;
          childElement2.SetPosition((childElement5.Width - childElement2.Width) / 2, -50);
          childElement2.Text = "T_OK";
          childElement2.Enabled = true;
        }
      }
      host.SetFocus(childElement2);
      childElement1.Text = MessageText;
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 101:
          hasmessage = false;
          Visible = false;
          popup_result = PopupMessageBox.PopupResult.Button1_YesOK;
          result_processed = false;
          break;
        case 102:
          hasmessage = false;
          Visible = false;
          popup_result = PopupMessageBox.PopupResult.Button2_NoCancel;
          result_processed = false;
          break;
        case 103:
          hasmessage = false;
          Visible = false;
          popup_result = PopupMessageBox.PopupResult.Button3_Custom;
          result_processed = false;
          break;
      }
    }

    private void XMLFormButtonCallback(ButtonWidget button)
    {
      if (XMLMessageCallback == null)
      {
        return;
      }

      XMLMessageCallback(button, message, this, child_frame, MessageUserData);
    }

    public override void SetVisible(bool bVisible)
    {
      if (Visible)
      {
        if (bVisible)
        {
          return;
        }

        hasmessage = false;
        if (host.HasChildDialog)
        {
          host.GlobalChildDialog -= (this);
        }

        base.SetVisible(bVisible);
      }
      else
      {
        if (!bVisible)
        {
          return;
        }

        host.GlobalChildDialog += (this);
        base.SetVisible(bVisible);
      }
    }

    public override void OnParentMove()
    {
      if (Parent == null)
      {
        return;
      }

      X = (Parent.Width - Width) / 2;
      Y = (Parent.Height - Height) / 2;
      base.OnParentMove();
    }

    private enum MessageBoxControlID
    {
      OKButton = 101, // 0x00000065
      CancelButton = 102, // 0x00000066
      CustomButton = 103, // 0x00000067
      MainFrame = 1000, // 0x000003E8
      Title = 1002, // 0x000003EA
      TextInformation = 1003, // 0x000003EB
    }

    public enum MessageBoxButtons
    {
      DEFAULT,
      OK,
      OKCANCEL,
      YESNO,
      CUSTOM,
    }

    public enum PopupResult
    {
      Button1_YesOK,
      Button2_NoCancel,
      Button3_Custom,
    }

    public enum MessageBoxType
    {
      Critical,
      XML,
    }

    public abstract class MessageData
    {
      public abstract PopupMessageBox.MessageBoxType GetMessageType();
    }

    public class MessageDataStandard : PopupMessageBox.MessageData
    {
      public SpoolerMessage message;
      public PopupMessageBox.MessageBoxButtons buttons;
      public PopupMessageBox.OnUserSelectionDel OnSelection;
      public string custom_button1_text;
      public string custom_button2_text;
      public string custom_button3_text;
      public string title;
      public object data;

      public MessageDataStandard(SpoolerMessage message, PopupMessageBox.MessageBoxButtons buttons)
        : this(message, "Printer Message", buttons, null, null)
      {
      }

      public MessageDataStandard(SpoolerMessage message, PopupMessageBox.MessageBoxButtons buttons, PopupMessageBox.OnUserSelectionDel OnSelectionCallback, object data)
        : this(message, "Printer Message", buttons, OnSelectionCallback, data)
      {
      }

      public MessageDataStandard(SpoolerMessage message, string title, PopupMessageBox.MessageBoxButtons buttons, PopupMessageBox.OnUserSelectionDel OnSelectionCallback, object data)
      {
        this.message = message;
        this.buttons = buttons;
        OnSelection = OnSelectionCallback;
        this.data = data;
        this.title = title;
        custom_button1_text = null;
        custom_button2_text = null;
        custom_button3_text = null;
      }

      public override PopupMessageBox.MessageBoxType GetMessageType()
      {
        return PopupMessageBox.MessageBoxType.Critical;
      }
    }

    public class MessageDataXML : PopupMessageBox.MessageData
    {
      public SpoolerMessage message;
      public string xmlsource;
      public PopupMessageBox.XMLButtonCallback buttonCallback;
      public object data;
      public ElementStandardDelegate onUpdateCallback;
      public PopupMessageBox.XMLOnShow onShowCallback;

      public MessageDataXML(string xmlsource, PopupMessageBox.XMLButtonCallback buttonCallback, ElementStandardDelegate onUpdateCallback, PopupMessageBox.XMLOnShow onShowCallback)
        : this(xmlsource, buttonCallback, onUpdateCallback, onShowCallback, null)
      {
      }

      public MessageDataXML(string xmlsource, PopupMessageBox.XMLButtonCallback buttonCallback, ElementStandardDelegate onUpdateCallback, PopupMessageBox.XMLOnShow onShowCallback, object state)
        : this(new SpoolerMessage(), xmlsource, buttonCallback, state, onUpdateCallback, onShowCallback)
      {
      }

      public MessageDataXML(SpoolerMessage message, string xmlsource, PopupMessageBox.XMLButtonCallback buttonCallback, object data)
        : this(message, xmlsource, buttonCallback, data, null, null)
      {
      }

      public MessageDataXML(SpoolerMessage message, string xmlsource, PopupMessageBox.XMLButtonCallback buttonCallback, object data, ElementStandardDelegate onUpdateCallback)
        : this(message, xmlsource, buttonCallback, data, onUpdateCallback, null)
      {
      }

      public MessageDataXML(SpoolerMessage message, string xmlsource, PopupMessageBox.XMLButtonCallback buttonCallback, object data, ElementStandardDelegate onUpdateCallback, PopupMessageBox.XMLOnShow onShowCallback)
      {
        this.message = message;
        this.xmlsource = xmlsource;
        this.buttonCallback = buttonCallback;
        this.data = data;
        this.onUpdateCallback = onUpdateCallback;
        this.onShowCallback = onShowCallback;
      }

      public override PopupMessageBox.MessageBoxType GetMessageType()
      {
        return PopupMessageBox.MessageBoxType.XML;
      }
    }

    public delegate void XMLOnShow(PopupMessageBox parentFrame, XMLFrame childFrame, GUIHost host, object data);

    public delegate void XMLButtonCallback(ButtonWidget button, SpoolerMessage message, PopupMessageBox parentFrame, XMLFrame childFrame, object data);

    public delegate void OnUserSelectionDel(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object user_data);
  }
}
