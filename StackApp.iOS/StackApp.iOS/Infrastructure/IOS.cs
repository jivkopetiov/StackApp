using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.ObjCRuntime;
using MonoTouch.SystemConfiguration;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.Globalization;

namespace Stacklash.iOS
{
    public static class IOS
    {
        public static DateTime? ToDateTime(this NSDate nsDate) {
            if (nsDate == null)
                return null;

            return new DateTime (2001, 1, 1, 0, 0, 0).AddSeconds (nsDate.SecondsSinceReferenceDate);
        }

        public static string DeviceFriendlyName {
            get 
            { 
                var device = UIDevice.CurrentDevice; 
                return string.Format("{0} ({1}, {2})", device.Name, GetDeviceVersion(), device.SystemVersion);
            }
        }

        public static string GetDeviceName() {
            return UIDevice.CurrentDevice.Name;
        }

        public static string GetIOSVersion() {
            return UIDevice.CurrentDevice.SystemVersion;
        }

        public static bool IsIOS5 {
            get {
                return UIDevice.CurrentDevice.SystemVersion.Split('.')[0] == "5";
            }
        }

        public static bool IsIOS6 {
            get {
                return UIDevice.CurrentDevice.SystemVersion.Split('.')[0] == "6";
            }
        }

        public static bool IsIOS7 {
            get {
                return UIDevice.CurrentDevice.SystemVersion.Split('.')[0] == "7";
            }
        }

        public static string GetBundleVersion() {
            NSString version = (NSString)NSBundle.MainBundle.InfoDictionary.ObjectForKey (new NSString ("CFBundleVersion"));
            return version.ToString ();
        }

        public static string GetBundleDisplayName() {
            NSString name = (NSString)NSBundle.MainBundle.InfoDictionary.ObjectForKey (new NSString ("CFBundleDisplayName"));
            return name.ToString ();
        }

        public static string GetBundleIdentifier() {
            return NSBundle.MainBundle.BundleIdentifier;
        }

        public static bool IsSimulator {
            get { return Runtime.Arch == Arch.SIMULATOR; }
        }

        public static bool IsDevice {
            get { return !IsSimulator; }
        }

        public static bool IsIpad {
            get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad; }
        }

        public static bool IsIphone {
            get { return !IsIpad; }
        }

        public static readonly bool IsHighRes = UIDevice.CurrentDevice.IsMultitaskingSupported && UIScreen.MainScreen.Scale > 1;

        public static bool CanCallPhones() {
            return UIApplication.SharedApplication.CanOpenUrl(new NSUrl("tel:+11111"));
        }

        public static void CallPhone(string phoneNumber) {

            if (!CanCallPhones())
            {
                Alert.ShowErrorBox("This device cannot call phones");
                return;
            }

            phoneNumber = phoneNumber.Trim().Replace("(", "").Replace(")", "").Replace(" ", "");

            var url = new NSUrl("tel:" + phoneNumber);
            if (!UIApplication.SharedApplication.OpenUrl(url)) {
                Alert.ShowErrorBox("Failed to call phone. Invalid phone format");
            }
        }

        public static UIInterfaceOrientation Orientation {
            get { return UIApplication.SharedApplication.StatusBarOrientation; }
        }

        public static bool IsPortrait ()
        {
            return UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.Portrait || UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.PortraitUpsideDown;
        }

        public static bool IsLandscape ()
        {
            return UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeLeft || UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeRight;
        }

        public static UIWindow Window {
            get { return UIApplication.SharedApplication.Windows[0]; }
        }

        public static float ScreenWidth { get {
                return IsPortrait () ? UIScreen.MainScreen.Bounds.Width : UIScreen.MainScreen.Bounds.Height;
            } }

        public static float ScreenHeight { get {
                return IsPortrait () ? UIScreen.MainScreen.Bounds.Height : UIScreen.MainScreen.Bounds.Width;
            } }

        public static string DocumentsFolder {
            get { return Environment.GetFolderPath (Environment.SpecialFolder.Personal); }
        }

        public static string ApplicationFolder {
            get { return Path.Combine (DocumentsFolder, ".."); }
        }

        public static string TempFolder {
            get { return Path.Combine (ApplicationFolder, "tmp"); }
        }

        public static string CacheFolder {
            get { return Path.Combine (ApplicationFolder, "Library/Caches"); }
        }

        private static bool IsReachableWithoutRequiringConnection (NetworkReachabilityFlags flags)
        {
            // Is it reachable with the current network configuration?
            bool isReachable = (flags & NetworkReachabilityFlags.Reachable) != 0;

            // Do we need a connection to reach it?
            bool noConnectionRequired = (flags & NetworkReachabilityFlags.ConnectionRequired) == 0;

            // Since the network stack will automatically try to get the WAN up, probe that
            if ((flags & NetworkReachabilityFlags.IsWWAN) != 0)
                noConnectionRequired = true;

            return isReachable && noConnectionRequired;
        }

        public static bool IsGoogleReachable() {
            return IsHostReachable ("www.google.com");
        }

        public static bool IsStackApiReachable() {
            return IsHostReachable ("stackexchange.com");
        }

        public static bool IsHostReachable (string host)
        {
            if (string.IsNullOrEmpty (host))
                return false;

            using (var r = new NetworkReachability (host)){
                NetworkReachabilityFlags flags;

                if (r.TryGetFlags (out flags)){
                    return IsReachableWithoutRequiringConnection (flags);
                }
            }
            return false;
        }

        [DllImport(MonoTouch.Constants.SystemLibrary)]
        static internal extern int sysctlbyname([MarshalAs(UnmanagedType.LPStr)] string property, IntPtr output, IntPtr oldLen, IntPtr newp, uint newlen);

        public static IOSHardware GetDeviceVersion() {
            const string HardwareProperty = "hw.machine";

            var pLen = Marshal.AllocHGlobal (sizeof(int));
            sysctlbyname (HardwareProperty, IntPtr.Zero, pLen, IntPtr.Zero, 0);

            var length = Marshal.ReadInt32 (pLen);

            if (length == 0) {
                Marshal.FreeHGlobal (pLen);

                return IOSHardware.Unknown;
            }

            var pStr = Marshal.AllocHGlobal (length);
            sysctlbyname (HardwareProperty, pStr, pLen, IntPtr.Zero, 0);

            var hardwareStr = Marshal.PtrToStringAnsi (pStr);

            Marshal.FreeHGlobal (pLen);
            Marshal.FreeHGlobal (pStr);

            if (hardwareStr == "iPhone1,1")
                return IOSHardware.iPhone;
            if (hardwareStr == "iPhone1,2")
                return IOSHardware.iPhone3G;
            if (hardwareStr == "iPhone2,1")
                return IOSHardware.iPhone3GS;
            if (hardwareStr == "iPhone3,1")
                return IOSHardware.iPhone4;
            if (hardwareStr == "iPhone3,2")
                return IOSHardware.iPhone4RevA;
            if (hardwareStr == "iPhone3,3")
                return IOSHardware.iPhone4CDMA;
            if (hardwareStr == "iPhone4,1")
                return IOSHardware.iPhone4S;
            if (hardwareStr == "iPhone5,1")
                return IOSHardware.iPhone5GSM;
            if (hardwareStr == "iPhone5,2")
                return IOSHardware.iPhone5CDMAGSM;

            if (hardwareStr == "iPad1,1")
                return IOSHardware.iPad;
            if (hardwareStr == "iPad1,2")
                return IOSHardware.iPad3G;
            if (hardwareStr == "iPad2,1")
                return IOSHardware.iPad2;
            if (hardwareStr == "iPad2,2")
                return IOSHardware.iPad2GSM;
            if (hardwareStr == "iPad2,3")
                return IOSHardware.iPad2CDMA;
            if (hardwareStr == "iPad2,4")
                return IOSHardware.iPad2RevA;
            if (hardwareStr == "iPad2,5")
                return IOSHardware.iPadMini;
            if (hardwareStr == "iPad2,6")
                return IOSHardware.iPadMiniGSM;
            if (hardwareStr == "iPad2,7")
                return IOSHardware.iPadMiniCDMAGSM;
            if (hardwareStr == "iPad3,1")
                return IOSHardware.iPad3;
            if (hardwareStr == "iPad3,2")
                return IOSHardware.iPad3CDMA;
            if (hardwareStr == "iPad3,3")
                return IOSHardware.iPad3GSM;
            if (hardwareStr == "iPad3,4")
                return IOSHardware.iPad4;
            if (hardwareStr == "iPad3,5")
                return IOSHardware.iPad4GSM;
            if (hardwareStr == "iPad3,6")
                return IOSHardware.iPad4CDMAGSM;

            if (hardwareStr == "iPod1,1")
                return IOSHardware.iPodTouch1G;
            if (hardwareStr == "iPod2,1")
                return IOSHardware.iPodTouch2G;
            if (hardwareStr == "iPod3,1")
                return IOSHardware.iPodTouch3G;
            if (hardwareStr == "iPod4,1")
                return IOSHardware.iPodTouch4G;
            if (hardwareStr == "iPod5,1")
                return IOSHardware.iPodTouch5G;

            if (hardwareStr == "i386" || hardwareStr == "x86_64") {
                if (UIDevice.CurrentDevice.Model.Contains ("iPhone")) {
                    if (UIScreen.MainScreen.Scale > 1.5f)
                        return IOSHardware.iPhoneRetinaSimulator;
                    else
                        return IOSHardware.iPhoneSimulator;
                } else {
                    if (UIScreen.MainScreen.Scale > 1.5f)
                        return IOSHardware.iPadRetinaSimulator;
                    else
                        return IOSHardware.iPadSimulator;
                }
            }

            return IOSHardware.Unknown;
        }
    }

    public static class DrawingEx {
        public static UIView MoveUp (this UIView view, float pixels)
        {
            var oldFrame = view.Frame;
            view.Frame = new RectangleF (oldFrame.X, oldFrame.Y - pixels, oldFrame.Width, oldFrame.Height);
            return view;
        }

        public static UIView MoveDown (this UIView view, float pixels)
        {
            var oldFrame = view.Frame;
            view.Frame = new RectangleF (oldFrame.X, oldFrame.Y + pixels, oldFrame.Width, oldFrame.Height);
            return view;
        }

        public static UIView MoveLeft (this UIView view, float pixels)
        {
            var oldFrame = view.Frame;
            view.Frame = new RectangleF (oldFrame.X - pixels, oldFrame.Y, oldFrame.Width, oldFrame.Height);
            return view;
        }

        public static UIView MoveRight (this UIView view, float pixels)
        {
            var oldFrame = view.Frame;
            view.Frame = new RectangleF (oldFrame.X + pixels, oldFrame.Y, oldFrame.Width, oldFrame.Height);
            return view;
        }

        public static UIView DecreaseWidth (this UIView view, int pixels)
        {
            var oldFrame = view.Frame;
            view.Frame = new RectangleF (oldFrame.X, oldFrame.Y, oldFrame.Width - pixels, oldFrame.Height);
            return view;
        }

        public static UIView SetWidth (this UIView view, float pixels)
        {
            var oldFrame = view.Frame;
            view.Frame = new RectangleF (oldFrame.X, oldFrame.Y, pixels, oldFrame.Height);
            return view;
        }

        public static UIView IncreaseWidth (this UIView view, int pixels)
        {
            var oldFrame = view.Frame;
            view.Frame = new RectangleF (oldFrame.X, oldFrame.Y, oldFrame.Width + pixels, oldFrame.Height);
            return view;
        }

        public static UIView IncreaseHeight (this UIView view, int pixels)
        {
            var oldFrame = view.Frame;
            view.Frame = new RectangleF (oldFrame.X, oldFrame.Y, oldFrame.Width, oldFrame.Height + pixels);
            return view;
        }

        public static UIView DecreaseHeight (this UIView view, int pixels)
        {
            var oldFrame = view.Frame;
            view.Frame = new RectangleF (oldFrame.X, oldFrame.Y, oldFrame.Width, oldFrame.Height - pixels);
            return view;
        }

        public static UIButton SetTitleWithResize (this UIButton button, UIView view, string title, UIControlState state)
        {
            var frame = button.Frame;
            var newSize = view.StringSize (title, button.Font);
            var newFrame = new RectangleF (frame.Left, frame.Top, newSize.Width + 6, frame.Height);
            button.Frame = newFrame;
            button.SetTitle (title, state);
            return button;
        }

        public static float DegreesToRadians (float degrees)
        {
            return degrees * 0.01745329f;
        }

        public static UIView AddRedBorder (this UIView view)
        {
            view.Layer.BorderColor = UIColor.Red.CGColor;
            view.Layer.BorderWidth = 1.0f;
            return view;
        }

        public static bool IsLandscape(this UIInterfaceOrientation orientation) {
            return orientation == UIInterfaceOrientation.LandscapeRight || 
                orientation == UIInterfaceOrientation.LandscapeLeft;
        }

        public static bool IsPortrait(this UIInterfaceOrientation orientation) {
            return orientation == UIInterfaceOrientation.Portrait ||
                orientation == UIInterfaceOrientation.PortraitUpsideDown;
        }
    }

    public enum IOSHardware {
        iPhone,
        iPhone3G,
        iPhone3GS,
        iPhone4,
        iPhone4RevA,
        iPhone4CDMA,
        iPhone4S,
        iPhone5GSM,
        iPhone5CDMAGSM,
        iPodTouch1G,
        iPodTouch2G,
        iPodTouch3G,
        iPodTouch4G,
        iPodTouch5G,
        iPad,
        iPad3G,
        iPad2,
        iPad2GSM,
        iPad2CDMA,
        iPad2RevA,
        iPadMini,
        iPadMiniGSM,
        iPadMiniCDMAGSM,
        iPad3,
        iPad3CDMA,
        iPad3GSM,
        iPad4,
        iPad4GSM,
        iPad4CDMAGSM,
        iPhoneSimulator,
        iPhoneRetinaSimulator,
        iPadSimulator,
        iPadRetinaSimulator,
        Unknown
    }

    public static class Colors
    {
        public static void HexToRgb (string hex, out byte red, out byte green, out byte blue)
        {
            hex = hex.TrimStart ('#');

            if (hex.Length == 6) {
                red = byte.Parse (hex.Substring (0, 2), NumberStyles.AllowHexSpecifier);
                green = byte.Parse (hex.Substring (2, 2), NumberStyles.AllowHexSpecifier);
                blue = byte.Parse (hex.Substring (4, 2), NumberStyles.AllowHexSpecifier);
            } else if (hex.Length == 3) {
                red = byte.Parse (hex[0].ToString () + hex[0], NumberStyles.AllowHexSpecifier);
                green = byte.Parse (hex[1].ToString () + hex[1], NumberStyles.AllowHexSpecifier);
                blue = byte.Parse (hex[2].ToString () + hex[2], NumberStyles.AllowHexSpecifier);
            } else {
                throw new ArgumentException ("Hex string is invalid: " + hex);
            }
        }

        public static string ToHex(this UIColor color) {
            float red;
            float green;
            float blue;
            float alpha;

            color.GetRGBA(out red, out green, out blue, out alpha);
            byte byteRed = (byte)(red * 255);
            byte byteGreen = (byte)(green * 255);
            byte byteBlue = (byte)(blue * 255);
            return RgbToHex(byteRed, byteGreen, byteBlue);
        }

        public static string ToHexAlternative(this UIColor color) {

            var components = color.CGColor.Components;

            float red = components[0];
            float green = components[1];
            float blue = components[2];

            byte byteRed = (byte)(red * 255);
            byte byteGreen = (byte)(green * 255);
            byte byteBlue = (byte)(blue * 255);
            return RgbToHex(byteRed, byteGreen, byteBlue);
        }

        public static string RgbToHex(byte red, byte green, byte blue) {
            return red.ToString("X2") + green.ToString("X2") + blue.ToString("X2");
        }

        public static UIColor FromHex (string hex)
        {
            byte red;
            byte green;
            byte blue;

            HexToRgb (hex, out red, out green, out blue);
            var color = UIColor.FromRGB (red, green, blue);

            return color;
        }
    }
}

