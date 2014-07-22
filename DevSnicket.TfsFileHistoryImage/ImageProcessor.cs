using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace DevSnicket.TfsFileHistoryImage
{
	internal partial class ImageProcessor
	{
		public static IEnumerable<Bitmap> Process(
			ICollection<Bitmap> images,
			Size maximum)
		{
			return
				Process(
					images,
					ParameterFactory.Create(
						maximum: maximum,
						sizes: images.Select(image => image.Size)));
		}

		private static IEnumerable<Bitmap> Process(
			ICollection<Bitmap> images,
			Parameter parameter)
		{
			var processor =
				new ImageProcessor(
					parameter: parameter,
					progressBarStepWidth: (Double)parameter.MaximumImageSize.Width / (images.Count - 1));

			return
				images
				.Select(
					processor.Process)
				.Concat(
					new [] { processor.CreateBlankImage(), });
		}
		
		private ImageProcessor(
			Parameter parameter,
			Double progressBarStepWidth)
		{
			_parameter = parameter;
			_progressBarStepWidth = progressBarStepWidth;
		}

		private readonly Parameter _parameter;
		private readonly Double _progressBarStepWidth;

		private Bitmap Process(
			Bitmap image,
			Int32 index)
		{
			var standardisedImage = CreateImageWithPixelFormat(image.PixelFormat);

			using (var graphics = Graphics.FromImage(standardisedImage))
			{
				DrawProgressBar(graphics, (Int32)(index * _progressBarStepWidth));
				DrawBackgroundAndImage(graphics, image, GetSize(image));
			}
			
			return standardisedImage;
		}

		private void DrawProgressBar(
			Graphics graphics,
			Int32 width)
		{
			graphics.FillRectangle(
				brush: Brushes.Gray,
				x: 0,
				y: 0,
				width: _parameter.MaximumImageSize.Width - width,
				height: _progressBarHeight);

			graphics.FillRectangle(
				brush: _backgroundBrush,
				x: _parameter.MaximumImageSize.Width - width,
				y: 0,
				width: width,
				height: _progressBarHeight);
		}

		private Size GetSize(
			Bitmap image)
		{
			return
				new Size(
					width: (Int32)(image.Width / _parameter.ShrinkFactor),
					height: (Int32)(image.Height / _parameter.ShrinkFactor) + _progressBarHeight);
		}	

		private void DrawBackgroundAndImage(
			Graphics graphics,
			Bitmap image,
			Size imageSize)
		{
			DrawBackground(
				graphics, 
				imageSize);

			graphics.DrawImage(
				image,
				new Rectangle(
					new Point(x: 0, y: _progressBarHeight),
					imageSize));
		}
		
		private const Int32 _progressBarHeight = 8;

		private void DrawBackground(
			Graphics graphics,
			Size imageSize)
		{
			graphics.FillRectangle(
				brush: _backgroundBrush,
				x: imageSize.Width,
				y: _progressBarHeight,
				width: _parameter.MaximumImageSize.Width - imageSize.Width,
				height: _parameter.MaximumImageSize.Height);

			graphics.FillRectangle(
				brush: _backgroundBrush,
				x: 0,
				y: imageSize.Height + _progressBarHeight,
				width: imageSize.Width,
				height: _parameter.MaximumImageSize.Height - imageSize.Height);
		}
		
		private Bitmap CreateBlankImage()
		{
			Bitmap image = CreateImageWithPixelFormat(PixelFormat.Format32bppRgb);

			using (var graphics = Graphics.FromImage(image))
				graphics.FillRectangle(
					_backgroundBrush,
					new Rectangle(Point.Empty, image.Size));

			return image;
		}

		private Bitmap CreateImageWithPixelFormat(
			PixelFormat pixelFormat)
		{
			return
				new Bitmap(
					_parameter.MaximumImageSize.Width,
					_parameter.MaximumImageSize.Height,
					pixelFormat);
		}

		private readonly Brush _backgroundBrush = Brushes.White;
	}
}
