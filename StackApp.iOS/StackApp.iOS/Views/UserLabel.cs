using System;
using MonoTouch.UIKit;
using Stacklash.Core;
using MonoTouch.Foundation;
using System.Web;

namespace Stacklash.iOS
{
    public class UserLabel : UILabel {
        private UIViewController _parent;
        private ShallowUser _user;

        public UserLabel()
        {
            UserInteractionEnabled = true;
            BackgroundColor = Colors.FromHex("F5D49A");
            TextColor = UIColor.Gray;
        }

        public void SetData(UIViewController parent, ShallowUser user)
        {
            _user = user;
            _parent = parent;

            if (user != null && user.display_name.IsNotNullOrEmpty())
                Text = user.NameDecoded + " (" + user.reputation + ")";
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            var touch = (UITouch)touches.AnyObject;
            if (touch.TapCount != 1)
                return;

            var next = new UserProfileController(_user);
            _parent.NavigationController.PushViewController(next, true);
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            return;
        }
    }
}

