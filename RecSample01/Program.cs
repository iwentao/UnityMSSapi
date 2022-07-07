using System;
using System.Collections.Generic;
using System.Text;
using System.Speech.Recognition;
using System.Speech;
using System.Diagnostics;
using System.Globalization;

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

    public SpeechHelper(string[] cmdArray, Action<string> reciveMethod)
        : this(cmdArray)
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
          }//选择美国英语的识别引擎  
        }
        if (SpeechREG == null)//如果没有适合的语音引擎，则选用第一个
          SpeechREG = new SpeechRecognitionEngine(rs[0]);
      }
      //SpeechREG = new SpeechRecognitionEngine();
    }

    public SpeechHelper(Action<string> reciveMethod)
        : this()
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

namespace RecognizerSettings
{
  class Program
  {
    static void Main(string[] args)
    {
      string[] commands = { "下一步", "开始", "结束", "上一步" };
      VoiceSpeeakHelper.SpeechHelper sh = new VoiceSpeeakHelper.SpeechHelper(commands);
      sh.LoadGramma();
      sh.ReciveSpeech += (result) =>
      {
        Console.WriteLine("Recognized " + result);
      };

      sh.StartSpeech();

      Console.WriteLine("Press any key to exit...");
      Console.ReadKey();
    }
  }
}
