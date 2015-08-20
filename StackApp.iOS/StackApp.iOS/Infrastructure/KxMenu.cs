using MonoTouch.UIKit;
using System;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;

namespace Stacklash.iOS
{
    public class KxMenuItem
    {
        public string Title;
        public UIImage Image;
        public Action Action;
        public UITextAlignment Alignment;
        public UIColor ForeColor;
    }

    public class KxMenuView : UIView
    {
        private ArrowDirection _arrowDirection;
        private float _arrowPosition;
        private UIView _contentView;
        private List<KxMenuItem> _menuItems;
        private const float kArrowSize = 12f;

        private static KxMenuView _sharedMenu;

        public static void Show(UIView view, RectangleF rect, List<KxMenuItem> menuItems)
        {
            if (_sharedMenu != null)
                _sharedMenu.dismissMenu(false);

            _sharedMenu = new KxMenuView();
            _sharedMenu.showMenuInView(view, rect, menuItems);
        }

        public static void Hide() {
            if (_sharedMenu != null)
                _sharedMenu.dismissMenu(false);
        }

        public static void HideAnimated() {
            if (_sharedMenu != null)
                _sharedMenu.dismissMenu(true);
        }

        private KxMenuView()
        {
            this.BackgroundColor = UIColor.Clear;
            this.Opaque = true;
            this.Alpha = 0;
        
            this.Layer.ShadowOpacity = 0.5f;
            this.Layer.ShadowOffset = new SizeF(2, 2);
            this.Layer.ShadowRadius = 2;
        }

        private enum ArrowDirection
        {
            None
            ,
            Up
            ,
            Down
            ,
            Left
            ,
            Right
            ,
        }

        private class KxMenuOverlay : UIView
        {
            public KxMenuOverlay()
            {
                this.BackgroundColor = UIColor.Clear;
                this.Opaque = false;
            }

            public override void TouchesEnded(NSSet touches, UIEvent evt)
            {
                foreach (var view in Subviews)
                {
                    if (view is KxMenuView)
                    {
                        ((KxMenuView)view).dismissMenu(true);
                        break;
                    }
                }
            }
        }

        private void setupFrameInView(UIView view, RectangleF fromRect)
        {

            SizeF contentSize = _contentView.Frame.Size;
    
            float outerWidth = view.Bounds.Width;
            float outerHeight = view.Bounds.Height;
    
            float rectX0 = fromRect.X;
            float rectX1 = fromRect.X + fromRect.Width;
            float rectXM = fromRect.X + fromRect.Width * 0.5f;
            float rectY0 = fromRect.Y;
            float rectY1 = fromRect.Y + fromRect.Height;
            float rectYM = fromRect.Y + fromRect.Height * 0.5f;
            ;
    
            float widthPlusArrow = contentSize.Width + kArrowSize;
            float heightPlusArrow = contentSize.Height + kArrowSize;
            float widthHalf = contentSize.Width * 0.5f;
            float heightHalf = contentSize.Height * 0.5f;
    
            float kMargin = 5f;
    
            if (heightPlusArrow < (outerHeight - rectY1))
            {
    
                _arrowDirection = ArrowDirection.Up;
                PointF point = new PointF(
                    rectXM - widthHalf,
                    rectY1
                );
        
                if (point.X < kMargin)
                    point.X = kMargin;
        
                if ((point.X + contentSize.Width + kMargin) > outerWidth)
                    point.X = outerWidth - contentSize.Width - kMargin;
        
                _arrowPosition = rectXM - point.X;
                //_arrowPosition = Math.Max(16, Math.Min(_arrowPosition, contentSize.Width - 16));        
                _contentView.Frame = new RectangleF(0, kArrowSize, contentSize.Width, contentSize.Height);
                
                this.Frame = new RectangleF(
            
                    point.X,
                    point.Y,
                    contentSize.Width,
                    contentSize.Height + kArrowSize
                );
        
            }
            else if (heightPlusArrow < rectY0)
            {
        
                _arrowDirection = ArrowDirection.Down;
                PointF point = new PointF(
                    rectXM - widthHalf,
                    rectY0 - heightPlusArrow
                );
        
                if (point.X < kMargin)
                    point.X = kMargin;
        
                if ((point.X + contentSize.Width + kMargin) > outerWidth)
                    point.X = outerWidth - contentSize.Width - kMargin;
        
                _arrowPosition = rectXM - point.X;
                _contentView.Frame = new RectangleF(PointF.Empty, contentSize);
        
                this.Frame = new RectangleF(
                    point.X,
                    point.Y,
                    contentSize.Width,
                    contentSize.Height + kArrowSize
                );
        
            }
            else if (widthPlusArrow < (outerWidth - rectX1))
            {
        
                _arrowDirection = ArrowDirection.Left;
                PointF point = new PointF(
                    rectX1,
                    rectYM - heightHalf
                );
        
                if (point.Y < kMargin)
                    point.Y = kMargin;
        
                if ((point.Y + contentSize.Height + kMargin) > outerHeight)
                    point.Y = outerHeight - contentSize.Height - kMargin;
        
                _arrowPosition = rectYM - point.Y;
                _contentView.Frame = new RectangleF(kArrowSize, 0, contentSize.Width, contentSize.Height);
        
                this.Frame = new RectangleF(
            
                    point.X,
                    point.Y,
                    contentSize.Width + kArrowSize,
                    contentSize.Height
                );
        
            }
            else if (widthPlusArrow < rectX0)
            {
        
                _arrowDirection = ArrowDirection.Right;
                PointF point = new PointF(
                    rectX0 - widthPlusArrow,
                    rectYM - heightHalf
                );
        
                if (point.Y < kMargin)
                    point.Y = kMargin;
        
                if ((point.Y + contentSize.Height + 5) > outerHeight)
                    point.Y = outerHeight - contentSize.Height - kMargin;
        
                _arrowPosition = rectYM - point.Y;
                _contentView.Frame = new RectangleF(PointF.Empty, contentSize);
        
                this.Frame = new RectangleF(
            
                    point.X, point.Y,
                    contentSize.Width + kArrowSize,
                    contentSize.Height
                );
        
            }
            else
            {
        
                _arrowDirection = ArrowDirection.None;
        
                this.Frame = new RectangleF(
            
                    (outerWidth - contentSize.Width) * 0.5f,
                    (outerHeight - contentSize.Height) * 0.5f,
                    contentSize.Width, contentSize.Height
                );
            }    
        }

        private void showMenuInView(UIView view, RectangleF rect, List<KxMenuItem> menuItems)
        {
            _menuItems = menuItems;
    
            _contentView = this.mkContentView();
            this.AddSubview(_contentView);
    
            this.setupFrameInView(view, rect);
        
            var overlay = new KxMenuOverlay { Frame = view.Bounds };
            overlay.AddSubview(this);
            view.Add(overlay);
    
            _contentView.Hidden = true;
            RectangleF toFrame = this.Frame;
            this.Frame = new RectangleF(this.arrowPoint(), new SizeF(1, 1));
    
            UIView.Animate(0.2, 
            delegate
            {
                this.Alpha = 1.0f;
                this.Frame = toFrame;
            },
            delegate
            {
                _contentView.Hidden = false;
            });
        }

        private void dismissMenu(bool animated)
        {
            if (animated)
            {
            
                _contentView.Hidden = true;            
                RectangleF toFrame = new RectangleF(this.arrowPoint(), new SizeF(1, 1));
            
                UIView.Animate(0.2, delegate
                {
                                 
                    this.Alpha = 0;
                    this.Frame = toFrame;
                                 
                }, delegate
                {
                    if (Superview is KxMenuOverlay)
                        this.Superview.RemoveFromSuperview();
                                 
                    this.RemoveFromSuperview();
                });
            }
            else
            {
            
                if (Superview is KxMenuOverlay)
                    this.Superview.RemoveFromSuperview();

                this.RemoveFromSuperview();
            }
        }

        private UIView mkContentView()
        {
            foreach (var view in Subviews)
            {
                view.RemoveFromSuperview();
            }
    
            if (_menuItems.Count == 0)
                return null;
 
            const float kMinMenuItemHeight = 32f;
            const float kMinMenuItemWidth = 32f;
            const float kMarginX = 10f;
            const float kMarginY = 5f;
    
            UIFont titleFont = UIFont.BoldSystemFontOfSize(16);
    
            float maxImageWidth = 0;    
            float maxItemHeight = 0;
            float maxItemWidth = 0;
    
            foreach (KxMenuItem menuItem in _menuItems)
            {
        
                if (menuItem.Image != null)
                {
                    SizeF imageSize = menuItem.Image.Size;        
                    if (imageSize.Width > maxImageWidth)
                        maxImageWidth = imageSize.Width; 
                }
            }
    
            foreach (KxMenuItem menuItem in _menuItems)
            {

                SizeF titleSize = this.StringSize(menuItem.Title, titleFont);

                SizeF imageSize = SizeF.Empty;
                if (menuItem.Image != null)
                    imageSize = menuItem.Image.Size;

                float itemHeight = Math.Max(titleSize.Height, imageSize.Height) + kMarginY * 2;
                float itemWidth = (menuItem.Image != null ? maxImageWidth + kMarginX : 0) + titleSize.Width + kMarginX * 4;
        
                if (itemHeight > maxItemHeight)
                    maxItemHeight = itemHeight;
        
                if (itemWidth > maxItemWidth)
                    maxItemWidth = itemWidth;
            }
       
            maxItemWidth = Math.Max(maxItemWidth, kMinMenuItemWidth);
            maxItemHeight = Math.Max(maxItemHeight, kMinMenuItemHeight);

            float titleX = kMarginX * 2 + (maxImageWidth > 0 ? maxImageWidth + kMarginX : 0);
            float titleWidth = maxItemWidth - titleX - kMarginX;
    
            UIImage selectedImage = getSelectedImage(new SizeF(maxItemWidth, maxItemHeight + 2));
            UIImage gradientLine = getGradientLine(new SizeF(maxItemWidth - kMarginX * 4, 1));
    
            var contentView = new UIView(RectangleF.Empty);
            contentView.AutoresizingMask = UIViewAutoresizing.None;
            contentView.BackgroundColor = UIColor.Clear;
            contentView.Opaque = false;
    
            float itemY = kMarginY * 2;
            int itemNum = 0;
        
            foreach (KxMenuItem menuItem in _menuItems)
            {
                RectangleF itemFrame = new RectangleF(0, itemY, maxItemWidth, maxItemHeight);
                UIView itemView = new UIView(itemFrame);
                itemView.AutoresizingMask = UIViewAutoresizing.None;
                itemView.BackgroundColor = UIColor.Clear;        
                itemView.Opaque = false;
                
                contentView.AddSubview(itemView);
        
                UIButton button = UIButton.FromType(UIButtonType.Custom);
                button.Tag = itemNum;
                button.Frame = itemView.Bounds;
                button.BackgroundColor = UIColor.Clear;
                button.Opaque = false;
                button.AutoresizingMask = UIViewAutoresizing.None;
                button.SetBackgroundImage(selectedImage, UIControlState.Highlighted);

                button.TouchDown += delegate
                {
                    NSTimer.CreateScheduledTimer(0.2d, delegate
                    {
                        dismissMenu(true);
                    });

                    if (menuItem.Action != null)
                        menuItem.Action();
                };


            
                itemView.AddSubview(button);
        
                if (menuItem.Title.Length > 0)
                {
            
                    RectangleF titleFrame;
            
                    if (menuItem.Image == null)
                    {
                
                        titleFrame = new RectangleF(
                            kMarginX * 2,
                            kMarginY,
                            maxItemWidth - kMarginX * 4,
                            maxItemHeight - kMarginY * 2
                        );
                
                    }
                    else
                    {
                
                        titleFrame = new RectangleF(
                            titleX,
                            kMarginY,
                            titleWidth,
                            maxItemHeight - kMarginY * 2
                        );
                    }
            
                    UILabel titleLabel = new UILabel(titleFrame);
                    titleLabel.Text = menuItem.Title;
                    titleLabel.Font = titleFont;
                    titleLabel.TextAlignment = menuItem.Alignment;
                    titleLabel.TextColor = menuItem.ForeColor ?? UIColor.White;
                    titleLabel.BackgroundColor = UIColor.Clear;
                    titleLabel.AutoresizingMask = UIViewAutoresizing.None;
                    itemView.AddSubview(titleLabel);
                }
        
                if (menuItem.Image != null)
                {
            
                    RectangleF imageFrame = new RectangleF(kMarginX * 2, kMarginY, maxImageWidth, maxItemHeight - kMarginY * 2);
                    UIImageView imageView = new UIImageView(imageFrame);
                    imageView.Image = menuItem.Image;
                    imageView.ClipsToBounds = true;
                    imageView.ContentMode = UIViewContentMode.Center;
                    imageView.AutoresizingMask = UIViewAutoresizing.None;
                    itemView.AddSubview(imageView);
                }
        
                if (itemNum < _menuItems.Count - 1)
                {
            
                    UIImageView gradientView = new UIImageView(gradientLine);
                    gradientView.Frame = new RectangleF(kMarginX * 2, maxItemHeight + 1, gradientLine.Size.Width, gradientLine.Size.Height);
                    gradientView.ContentMode = UIViewContentMode.Left;
                    itemView.AddSubview(gradientView);
            
                    itemY += 2;
                }
        
                itemY += maxItemHeight;
                ++itemNum;
            }    
    
            contentView.Frame = new RectangleF(0, 0, maxItemWidth, itemY + kMarginY * 2);
    
            return contentView;
        }

        private PointF arrowPoint()
        {
            PointF point;
    
            if (_arrowDirection == ArrowDirection.Up)
            {
        
                point = new PointF(this.Frame.GetMinX() + _arrowPosition, this.Frame.GetMinY());
        
            }
            else if (_arrowDirection == ArrowDirection.Down)
            {
        
                point = new PointF(this.Frame.GetMinX() + _arrowPosition, this.Frame.GetMaxY());
        
            }
            else if (_arrowDirection == ArrowDirection.Left)
            {
        
                point = new PointF(this.Frame.GetMinX(), this.Frame.GetMinY() + _arrowPosition);
        
            }
            else if (_arrowDirection == ArrowDirection.Right)
            {
        
                point = new PointF(this.Frame.GetMaxX(), this.Frame.GetMinY() + _arrowPosition);
        
            }
            else
            {
        
                point = this.Center;
            }
    
            return point;
        }

        private UIImage getSelectedImage(SizeF size)
        {
            float[] locations = new[] { 0f, 1f };
            float[] components = new[]
            {
                0.216f, 0.471f, 0.871f, 1f,
                0.059f, 0.353f, 0.839f, 1f,
            };
    
            return this.gradientImageWithSize(size, locations, components, 2);
        }

        private UIImage getGradientLine(SizeF size)
        {
            float[] locations = new float[] { 0f, 0.2f, 0.5f, 0.8f, 1f };
    
            float R = 0.44f, G = 0.44f, B = 0.44f;
        
            float[] components = new[]
            {
                R, G, B, 0.1f,
                R, G, B, 0.4f,
                R, G, B, 0.7f,
                R, G, B, 0.4f,
                R, G, B, 0.1f
            };
    
            return gradientImageWithSize(size, locations, components, 5);
        }

        private UIImage gradientImageWithSize(SizeF size, float[] locations, float[] components, int count)
        {
            UIGraphics.BeginImageContextWithOptions(size, false, 0);
            CGContext context = UIGraphics.GetCurrentContext();
    
            CGColorSpace colorSpace = CGColorSpace.CreateDeviceRGB();
            CGGradient colorGradient = new CGGradient(colorSpace, components, locations);
            colorSpace.Dispose();
            context.DrawLinearGradient(colorGradient, new PointF(0, 0), new PointF(size.Width, 0), 0);
            colorGradient.Dispose();
    
            UIImage image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return image;
        }

        public override void Draw(RectangleF rect)
        {
            this.drawBackground(Bounds, UIGraphics.GetCurrentContext());
        }

        private void drawBackground(RectangleF frame, CGContext context)
        {
            float R0 = 0.267f, G0 = 0.303f, B0 = 0.335f;
            float R1 = 0.040f, G1 = 0.040f, B1 = 0.040f;
    
            //UIColor tintColor = KxMenu.tintColor;
            float X0 = frame.X;
            float X1 = frame.X + frame.Width;
            float Y0 = frame.Y;
            float Y1 = frame.Y + frame.Height;
    
            // render arrow
    
            UIBezierPath arrowPath = new UIBezierPath();
    
            // fix the issue with gap of arrow's base if on the edge
            const float kEmbedFix = 3f;
    
            if (_arrowDirection == ArrowDirection.Up)
            {
        
                float arrowXM = _arrowPosition;
                float arrowX0 = arrowXM - kArrowSize;
                float arrowX1 = arrowXM + kArrowSize;
                float arrowY0 = Y0;
                float arrowY1 = Y0 + kArrowSize + kEmbedFix;
        
                arrowPath.MoveTo(new PointF(arrowXM, arrowY0));
                arrowPath.AddLineTo(new PointF(arrowX1, arrowY1));
                arrowPath.AddLineTo(new PointF(arrowX0, arrowY1));
                arrowPath.AddLineTo(new PointF(arrowXM, arrowY0));
        
                UIColor.FromRGBA(R0, G0, B0, 1).SetFill();
        
                Y0 += kArrowSize;
        
            }
            else if (_arrowDirection == ArrowDirection.Down)
            {
        
                float arrowXM = _arrowPosition;
                float arrowX0 = arrowXM - kArrowSize;
                float arrowX1 = arrowXM + kArrowSize;
                float arrowY0 = Y1 - kArrowSize - kEmbedFix;
                float arrowY1 = Y1;
        
                arrowPath.MoveTo(new PointF(arrowXM, arrowY1));
                arrowPath.AddLineTo(new PointF(arrowX1, arrowY0));
                arrowPath.AddLineTo(new PointF(arrowX0, arrowY0));
                arrowPath.AddLineTo(new PointF(arrowXM, arrowY1));
        
                UIColor.FromRGBA(R1, G1, B1, 1).SetFill();
        
                Y1 -= kArrowSize;
        
            }
            else if (_arrowDirection == ArrowDirection.Left)
            {
        
                float arrowYM = _arrowPosition;        
                float arrowX0 = X0;
                float arrowX1 = X0 + kArrowSize + kEmbedFix;
                float arrowY0 = arrowYM - kArrowSize;
                ;
                float arrowY1 = arrowYM + kArrowSize;
        
                arrowPath.MoveTo(new PointF(arrowX0, arrowYM));
                arrowPath.AddLineTo(new PointF(arrowX1, arrowY0));
                arrowPath.AddLineTo(new PointF(arrowX1, arrowY1));
                arrowPath.AddLineTo(new PointF(arrowX0, arrowYM));
        
                UIColor.FromRGBA(R0, G0, B0, 1).SetFill();
        
                X0 += kArrowSize;
        
            }
            else if (_arrowDirection == ArrowDirection.Right)
            {
        
                float arrowYM = _arrowPosition;        
                float arrowX0 = X1;
                float arrowX1 = X1 - kArrowSize - kEmbedFix;
                float arrowY0 = arrowYM - kArrowSize;
                ;
                float arrowY1 = arrowYM + kArrowSize;
        
                arrowPath.MoveTo(new PointF(arrowX0, arrowYM));
                arrowPath.AddLineTo(new PointF(arrowX1, arrowY0));
                arrowPath.AddLineTo(new PointF(arrowX1, arrowY1));
                arrowPath.AddLineTo(new PointF(arrowX0, arrowYM));
        
                UIColor.FromRGBA(R1, G1, B1, 1).SetFill();
        
                X1 -= kArrowSize;
            }
    
            arrowPath.Fill();
            // render body
    
            RectangleF bodyFrame = new RectangleF(X0, Y0, X1 - X0, Y1 - Y0);
    
            UIBezierPath borderPath = UIBezierPath.FromRoundedRect(bodyFrame, 8);
        
            float[] locations = new[] { 0f, 1f };
            float[] components = new[]
            {
                R0, G0, B0, 1,
                R1, G1, B1, 1,
            };
    
            CGColorSpace colorSpace = CGColorSpace.CreateDeviceRGB();
            CGGradient gradient = new CGGradient(colorSpace, components, locations);
    
            borderPath.AddClip();
    
            PointF start, end;
    
            if (_arrowDirection == ArrowDirection.Left ||
                _arrowDirection == ArrowDirection.Right)
            {
                
                start = new PointF(X0, Y0);
                end = new PointF(X1, Y0);
        
            }
            else
            {
        
                start = new PointF(X0, Y0);
                end = new PointF(X0, Y1);
            }
    
            context.DrawLinearGradient(gradient, start, end, 0);
        }
    }
}
