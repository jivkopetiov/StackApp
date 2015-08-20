using System;
using Abilitics.iOS;
using Stacklash.Core;
using SDWebImage;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Stacklash.iOS
{
    public class UserListRow : Value1FixedRow
    {
        private static UIImage placeholder = UIImage.FromFile("noimage.png");

        public UserListRow(ShallowUser user)
        {
            Text = user.NameDecoded;
            Details = user.reputation.ToString();
            NavController = () => new UserProfileController(user);
            AfterGetCellInit = cell => {
                cell.ImageView.SetImage(new NSUrl(user.profile_image), placeholder, 
                                        delegate(UIImage image, NSError error, SDImageCacheType cacheType)
                {
                    cell.ImageView.Image = image;
                });
            };
        }
    }
}

