using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Stacklash.iOS
{
    [Register ("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        static void Main(string[] args)
        {
            UIApplication.Main(args, null, "AppDelegate");
        }

        private UIWindow window;
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            window = new UIWindow(UIScreen.MainScreen.Bounds);
            Database.Init();
            Nav.Init(window);
            Nav.BuildDeck();
            window.MakeKeyAndVisible();
            return true;
        }
    }
}

