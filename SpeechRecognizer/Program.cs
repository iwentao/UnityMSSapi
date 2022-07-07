//////////////////////////////////////////////////////////////////////
// PITool
// 2022-6-29
//////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace PITool
{
  class Program
  {
    static void Main(string[] args)
    {
      var server = new NetServer();
      server.StartServer();
      while (true)
      {
        var res = Console.ReadLine();
        if (res == "exit")
          break;
      }
    }

    private static void OnRecognized(string text)
    {
      Console.WriteLine(text);
    }
  }
}
