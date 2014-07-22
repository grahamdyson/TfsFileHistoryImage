using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DevSnicket.TfsFileHistoryImage
{
	partial class ImageProcessor
	{
		private class ParameterFactory
		{
			public static Parameter Create(
				   Size maximum,
				   IEnumerable<Size> sizes)
			{
				return
					new ParameterFactory(
						GetMaximum(sizes))
					.CreateForMaximum(
						maximum);
			}

			private static Size GetMaximum(
				IEnumerable<Size> sizes)
			{
				return
					new Size(
						sizes.Max(size => size.Width),
						sizes.Max(size => size.Height));
			}

			private ParameterFactory(
				Size size)
			{
				_size = size;
			}

			private readonly Size _size;

			private Parameter CreateForMaximum(
				Size maximum)
			{
				return
					CreateForShrinkFactor(
						GetShrinkFactorFromMaximum(maximum));
			}

			private Double GetShrinkFactorFromMaximum(
				Size maximum)
			{
				return
					Math.Max(
						1,
						Math.Max(
							(Double)_size.Height / maximum.Height,
							(Double)_size.Width / maximum.Width));
			}

			private Parameter CreateForShrinkFactor(
				Double shrinkFactor)
			{
				return
					new Parameter
					{
						MaximumImageSize = GetSizeFromShrinkFactor(shrinkFactor),
						ShrinkFactor = shrinkFactor,
					};
			}

			private Size GetSizeFromShrinkFactor(
				Double shrinkFactor)
			{
				return
					new Size(
						(Int32)(_size.Width / shrinkFactor),
						(Int32)(_size.Height / shrinkFactor));
			}
		}
	}
}