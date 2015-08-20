using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace Abilitics.iOS
{
	public enum CropPosition
	{
		Start, Center, End
	}

	public class ImageResizer
	{
		UIImage initialImage  = null;
		UIImage modifiedImage = null;

		public ImageResizer ( UIImage image )
		{
			this.initialImage         = image;
			this.modifiedImage = image;
		}


		/// <summary>
		/// strech resize
		/// </summary>
		public void Resize( float width, float height )
		{
			UIGraphics.BeginImageContext( new SizeF( width, height ) );
			//
			modifiedImage.Draw( new RectangleF( 0,0, width, height ) );
			modifiedImage = UIGraphics.GetImageFromCurrentImageContext();
			//
			UIGraphics.EndImageContext();
		}

		public void RatioResizeToWidth( float width )
		{
			var cur_width = modifiedImage.Size.Width;
			if( cur_width > width )
			{
				var ratio  = width / cur_width;
				var height = modifiedImage.Size.Height * ratio;
				//resize
				Resize( width, height );
			}
		}

		public void RatioResizeToHeight( float height )
		{
			var cur_height = modifiedImage.Size.Height;
			if( cur_height > height )
			{
				var ratio = height / cur_height;
				var width = modifiedImage.Size.Width * ratio;
				//
				Resize( width, height );
			}
		}

		/// <summary>
		/// resize maintaining ratio
		/// </summary>
		public void RatioResize( float max_width, float max_height )
		{
			if( max_width > max_height )
			{
				RatioResizeToWidth ( max_width  );
				RatioResizeToHeight( max_height );
			}
			else
			{
				RatioResizeToHeight( max_height );
				RatioResizeToWidth ( max_width  );
			}
		}

		/// <summary>
		/// scaling the image with a given procent value
		/// </summary>
		public void Scale( float procent )
		{
			var width  = modifiedImage.Size.Width  * (procent / 100);
			var height = modifiedImage.Size.Height * (procent / 100);
			Resize( width, height );
		}

		/// <summary>
		/// default crop resize, set on center
		/// </summary>
		public UIImage CropResize( float width, float height )
		{
			return CropResize(width, height, CropPosition.Center );
		}

		/// <summary>
		/// resize and crop to a specific location
		/// </summary>
		public UIImage CropResize( float width, float height, CropPosition position )
		{
			SizeF ImgSize = modifiedImage.Size;
			//
			if( ImgSize.Width  < width  ) width  = ImgSize.Width;
			if( ImgSize.Height < height ) height = ImgSize.Height;
			//
			float crop_x = 0;
			float crop_y = 0;
			if( ImgSize.Width / width < ImgSize.Height / height )
			{
				//scad din width
				RatioResizeToWidth( width );
				ImgSize = modifiedImage.Size;
				//compute crop_y
				if( position == CropPosition.Center ) crop_y = (ImgSize.Height / 2) - (height / 2);
				if( position == CropPosition.End    ) crop_y = ImgSize.Height - height;
			}
			else
			{
				//change height
				RatioResizeToHeight( height );
				ImgSize = modifiedImage.Size;
				//calculeaza crop_x
				if( position == CropPosition.Center ) crop_x = (ImgSize.Width / 2) - (width / 2);
				if( position == CropPosition.End    ) crop_x = ImgSize.Width - width;
			}
			//create new contect
			UIGraphics.BeginImageContext( new SizeF( width, height ) );
			CGContext context =  UIGraphics.GetCurrentContext();
			//crops the new context to the desired height and width
			RectangleF clippedRect = new RectangleF( 0, 0, width, height );
			context.ClipToRect(clippedRect);
			//draw my image on the context
			RectangleF drawRect = new RectangleF( -crop_x, -crop_y, ImgSize.Width, ImgSize.Height );
			modifiedImage.Draw( drawRect );
			//save the context in modifiedImage
			modifiedImage = UIGraphics.GetImageFromCurrentImageContext();
			//close the context
			UIGraphics.EndImageContext();

			return modifiedImage;
		}


		public UIImage InitialImage 
		{
			get 
			{
				return this.initialImage;
			}
		}

		public UIImage ModifiedImage 
		{
			get 
			{
				return this.modifiedImage;
			}
		}
	}
}
 