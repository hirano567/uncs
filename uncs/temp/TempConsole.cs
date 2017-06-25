#if DEBUG
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace temp
{
    public static class CTempConsole
    {
        //------------------------------------------------------------
        // CTempOptions.Run
        //------------------------------------------------------------
        internal static void Run()
        {
            string str1 = "Microsoft アカウントとは何ですか? ";
            string str2 =
                "「Microsoft アカウント」とは、「Windows Live ID」と呼ばれていたものの新しい名称です。" +
                "お客様の Microsoft アカウントとは、電子メール アドレスに、" +
                "Hotmail、Messenger、OneDrive、Windows Phone、Xbox LIVE、または Outlook.com などの" +
                "サービスにサインインするためのパスワードを加えたものです。";

            int width, left;
            left = Console.WindowLeft;
            width = Console.WindowWidth;
            int newleft = 30;

            Console.Write(str1);
            Console.WindowWidth = width - newleft;
            Console.WindowLeft = newleft;
            left = Console.WindowLeft;
            Console.Write(str2);
        }
    }
}
#endif
