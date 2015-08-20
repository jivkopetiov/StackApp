using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace Stacklash.iOS
{
    public class TagLabel : UILabel {

        private UIViewController _parent;

        public TagLabel(UIViewController parent)
        {
            UserInteractionEnabled = true;
            BackgroundColor = Colors.FromHex("c4dae9");
            Font = UIFont.SystemFontOfSize(12);
            TextAlignment = UITextAlignment.Center;
            TextColor = Colors.FromHex("4a6b82");

            _parent = parent;
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            var touch = (UITouch)touches.AnyObject;
            if (touch.TapCount != 1)
                return;

            var next = new QuestionsByTagController(Text);
            _parent.NavigationController.PushViewController(next, true);
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            return;
        }
    }
}

