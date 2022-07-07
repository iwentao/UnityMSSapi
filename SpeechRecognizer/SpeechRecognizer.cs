using System;
using System.Globalization;
using System.Speech.Recognition;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace PITool
{
  public delegate void SpeechRecognized(string text);

  //https://blog.csdn.net/weixin_43542114/article/details/107341018
  namespace VoiceSpeeakHelper
  {
    public class SpeechHelper
    {

      #region var
      /// <summary>
      /// 命令字符串数组
      /// </summary>
      private string[] ChoicesCmdArray = null;
      /// <summary>
      /// 监听说话对象
      /// </summary>
      private SpeechRecognitionEngine SpeechREG = null;
      /// <summary>
      /// 委托函数
      /// </summary>
      public Action<string> ReciveSpeech = null;
      #endregion

      public SpeechHelper(string[] cmdArray, Action<string> reciveMethod) : this(cmdArray)
      {
        ReciveSpeech = reciveMethod;
      }

      private SpeechHelper()
      {
        var myCIintl = new CultureInfo("zh-CN");
        var rs = SpeechRecognitionEngine.InstalledRecognizers();
        Console.WriteLine("Found " + rs.Count + " SRE");
        if (rs.Count > 0)
        {
          foreach (var config in rs)//获取所有语音引擎  
          {
            if (config.Culture.Equals(myCIintl) && config.Id == "MS-2052-80-DESK")
            {
              SpeechREG = new SpeechRecognitionEngine(config);
              Console.WriteLine("Successful to choose zh-CN SRE");
              break;
            }
          }
          if (SpeechREG == null) // Choose first SRE
            SpeechREG = new SpeechRecognitionEngine(rs[0]);
        }
      }

      public SpeechHelper(Action<string> reciveMethod) : this()
      {
        ReciveSpeech = reciveMethod;
      }

      public SpeechHelper(string[] cmdArray) : this()
      {
        ChoicesCmdArray = cmdArray;
      }

      /// <summary>
      /// 初始化Speech所用参数信息 如果没有给规定范围字符串 则可以任意接收输入
      /// </summary>
      /// <returns></returns>
      public bool LoadGramma()
      {
        try
        {
          if (ChoicesCmdArray != null && ChoicesCmdArray.Length > 0)
          {
            //ChoicesCmdArray = ChoicesCmdArray.Where(ret => !string.IsNullOrEmpty(ret)).ToArray();
            Choices choices = new Choices();
            choices.Add(ChoicesCmdArray);
            GrammarBuilder gb = new GrammarBuilder(choices);
            gb.Culture = new CultureInfo("zh-CN");
            Grammar gr = new Grammar(gb);
            SpeechREG.LoadGrammar(gr);
          }
          else
          {
            SpeechREG.LoadGrammar(new DictationGrammar());
          }
          SpeechREG.SetInputToDefaultAudioDevice();
          SpeechREG.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(SpeechREG_SpeechRecognized);
          return true;
        }
        catch (Exception ex)
        {
          Debug.Assert(false, "加载语音模块失败！[提示：电脑可能没有开启 语音识别功能 ]" + ex.Message);
          return false;
        }
      }

      /// <summary>
      /// 开始语音监听
      /// </summary>
      /// <returns></returns>
      public bool StartSpeech()
      {
        bool stBool = false;
        if (SpeechREG != null)
        {
          try
          {
            SpeechREG.RecognizeAsync(RecognizeMode.Multiple);
            stBool = true;
          }
          catch (Exception)
          {
            stBool = false;
          }
        }
        return stBool;
      }

      /// <summary>
      /// 停止语音监听
      /// </summary>
      public void StopSpeech()
      {
        if (SpeechREG != null)
        {
          SpeechREG.RecognizeAsyncStop();
        }
      }

      public void CloseSpeech()
      {
        if (SpeechREG != null)
        {
          SpeechREG.Dispose();
          SpeechREG = null;
        }
      }

      void SpeechREG_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
      {
        ReciveSpeech(e.Result.Text);
      }
    }
  }

  public class Recognizer
  {
    private readonly SpeechRecognitionEngine m_recognizer;//语音识别引擎  
    private readonly DictationGrammar m_grammar; //自然语法  

    public Recognizer()
    {
      //var myCIintl = new CultureInfo("en-US");
      var myCIintl = new CultureInfo("zh-CN");
      var rs = SpeechRecognitionEngine.InstalledRecognizers();
      if (rs.Count > 0)
      {
        foreach (var config in rs)
        {
          if (config.Culture.Equals(myCIintl) && config.Id == "MS-2052-80-DESK")
          {
            m_recognizer = new SpeechRecognitionEngine(config);
            break;
          }//选择美国英语的识别引擎  
        }
        if (m_recognizer == null)//如果没有适合的语音引擎，则选用第一个
          m_recognizer = new SpeechRecognitionEngine(rs[0]);
      }
      if (m_recognizer != null)
      {
        //var kws = Properties.Settings.Default.Keywords;
        var kws = System.Configuration.ConfigurationManager.AppSettings["VoiceCommands"].Split('/');
        var fg = new string[kws.Length];
        kws.CopyTo(fg, 0);
        InitializeSpeechRecognitionEngine(fg);//初始化语音识别引擎  
        //m_grammar = new DictationGrammar();
      }
      else
      {
        Console.WriteLine("创建语音识别失败");
      }
    }

    private void InitializeSpeechRecognitionEngine(string[] ChoicesCmdArray)
    {
      m_recognizer.SetInputToDefaultAudioDevice();//选择默认的音频输入设备  

      //var customGrammar = CreateCustomGrammar(fg);
      ////根据关键字数组建立语法  
      //m_recognizer.UnloadAllGrammars();
      //m_recognizer.LoadGrammar(customGrammar);

      Choices choices = new Choices();
      choices.Add(ChoicesCmdArray);
      GrammarBuilder gb = new GrammarBuilder(choices);
      gb.Culture = new CultureInfo("zh-CN");
      Grammar gr = new Grammar(gb);
      m_recognizer.LoadGrammar(gr);

      //加载语法  
      m_recognizer.SpeechRecognized += recognizer_SpeechRecognized;
      //m_recognizer.SpeechHypothesized += recognizer_SpeechHypothesized;  
    }

    public SpeechRecognized OnRecognized;

    private void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
    {
      Console.WriteLine("SpeechRecognized: " + e.Result.Text);
      OnRecognized?.Invoke(e.Result.Text);
    }

    private void recognizer_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
    {
    }

    /// <summary>
    /// 录音并识别
    /// </summary>
    public void BeginRec()
    {
      Console.WriteLine("BeginRec");
      TurnSpeechRecognitionOn();
      //TurnDictationOn();
    }

    private void TurnDictationOn()
    {
      if (m_recognizer != null)
      {
        m_recognizer.LoadGrammar(m_grammar);
        //加载自然语法  
      }
      else
      {
        Console.WriteLine("创建语音识别失败");
      }
    }

    private void TurnSpeechRecognitionOn()//启动语音识别函数  
    {
      if (m_recognizer != null)
      {
        m_recognizer.RecognizeAsync(RecognizeMode.Multiple);
        //识别模式为连续识别  
      }
      else
      {
        Console.WriteLine("创建语音识别失败");
      }
    }

    public void EndRec()//停止语音识别引擎  
    {
      Console.WriteLine("EndRec");
      TurnSpeechRecognitionOff();
    }

    private void TurnSpeechRecognitionOff()//关闭语音识别函数  
    {
      if (m_recognizer != null)
      {
        m_recognizer.RecognizeAsyncStop();
        //TurnDictationOff();
      }
      else
      {
        Console.WriteLine("创建语音识别失败");
      }
    }

    private void TurnDictationOff()
    {
      if (m_grammar != null)
      {
        m_recognizer.UnloadGrammar(m_grammar);
      }
      else
      {
        Console.WriteLine("创建语音识别失败");
      }
    }

    private Grammar CreateCustomGrammar(string[] fg) //创造自定义语法  
    {
      var grammarBuilder = new GrammarBuilder();
      grammarBuilder.Append(new Choices(fg));
      return new Grammar(grammarBuilder);
    }
  }

  /// <summary>
  /// NetServer - Network server.
  /// </summary>
  public class NetServer : INetComponent
  {
    private Recognizer m_recognizer;
    private readonly SocketServer m_socket;

    public NetServer()
    {
      m_socket = new SocketServer(this);
    }

    public void Init()
    {
      try
      {
        m_recognizer = new Recognizer
        {
          OnRecognized = OnRecognized
        };
        Console.WriteLine("初始化完成");
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
      }
    }

    private void OnRecognized(string text)
    {
      var arg = new ByteInArg();
      arg.Write(text);
      NetSendMsg(arg.GetBuffer());
    }

    public void StartServer()
    {
      m_socket.OnConnecte = OnClientConnected;
      m_socket.Bind("127.0.0.1", Properties.Settings.Default.Port);
    }

    private int m_clientid;
    private void OnClientConnected(int cid)
    {
      m_clientid = cid;
      Console.WriteLine("Client connected:" + cid);
    }

    public bool NetSendMsg(byte[] sendbuffer)
    {
      return m_socket.SendMsg(sendbuffer, m_clientid);
    }

    public bool NetReceiveMsg(byte[] recivebuffer, int netID)
    {

      var arg = new ByteOutArg(recivebuffer);
      var cmd = arg.ReadInt32();
      switch ((EmCmd)cmd)
      {
        case EmCmd.Init:
          Init();
          break;
        case EmCmd.Recognize:
          var scmd = arg.ReadInt32();
          if (scmd == 1)
            m_recognizer.BeginRec();
          if (scmd == 0)
            m_recognizer.EndRec();
          return true;
        default:
          throw new ArgumentOutOfRangeException();
      }
      return false;
    }

    public bool Connected
    {
      get { return m_socket.Connected; }
    }
  }

}
