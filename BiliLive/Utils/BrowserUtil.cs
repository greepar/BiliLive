using System;

namespace BiliLive.Utils;

public static class BrowserUtil
{
   public static void OpenInBrowser(string url)
   {
       try
       {
           var ps = new System.Diagnostics.ProcessStartInfo(url)
           {
               UseShellExecute = true
           };
           System.Diagnostics.Process.Start(ps);
       }
       catch (Exception)
       {
           //Console.WriteLine(ex);
       }
   }
}