using System;  
using System.Globalization;  
using System.Speech.Recognition;  

namespace RecognizerSettings
{
  class Program
  {
    static readonly string[] settings = new string[] {
      "ResourceUsage",
      "ResponseSpeed",
      "ComplexResponseSpeed",
      "AdaptationOn",
      "PersistedBackgroundAdaptation",
    };

    static void Main(string[] args)
    {
      using (SpeechRecognitionEngine recognizer =
        new SpeechRecognitionEngine(new System.Globalization.CultureInfo("zh-CN")))
      {
        Console.WriteLine("Settings for recognizer {0}:",
          recognizer.RecognizerInfo.Name);
        Console.WriteLine();

        // List the current settings.  
        ListSettings(recognizer);

        // Change some of the settings.  
        recognizer.UpdateRecognizerSetting("ResponseSpeed", 200);
        recognizer.UpdateRecognizerSetting("ComplexResponseSpeed", 300);
        recognizer.UpdateRecognizerSetting("AdaptationOn", 1);
        recognizer.UpdateRecognizerSetting("PersistedBackgroundAdaptation", 0);

        Console.WriteLine("Updated settings:");
        Console.WriteLine();

        // List the updated settings.  
        ListSettings(recognizer);
      }

      Console.WriteLine("Press any key to exit...");
      Console.ReadKey();
    }

    private static void ListSettings(SpeechRecognitionEngine recognizer)
    {
      foreach (string setting in settings)
      {
        try
        {
          object value = recognizer.QueryRecognizerSetting(setting);
          Console.WriteLine("  {0,-30} = {1}", setting, value);
        }
        catch
        {
          Console.WriteLine("  {0,-30} is not supported by this recognizer.",
            setting);
        }
      }
      Console.WriteLine();
    }
  }
}
