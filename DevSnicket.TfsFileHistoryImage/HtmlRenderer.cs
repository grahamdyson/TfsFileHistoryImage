using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;

namespace DevSnicket.TfsFileHistoryImage
{
	internal class HtmlRenderer
	{
		public static Bitmap[] Render(
			ICollection<String> documents)
		{
			return new HtmlRenderer(documents).Render();
		}

		private HtmlRenderer(
			ICollection<String> documents)
		{
			_documents = documents;
		}

		private readonly ICollection<String> _documents;

		private Bitmap[] Render()
		{
			Bitmap[] images = null;

			for (Int32 scale = 1; images == null; scale = scale == 1 ? 2 : scale * scale)
				images = TryRenderWithScale(scale);

			return images;
		}

		private Bitmap[] TryRenderWithScale(
			Int32 scale)
		{
			var images = new Bitmap[_documents.Count];

			using (IEnumerator<String> enumerator = _documents.GetEnumerator())
				for (Int32 index = 0; enumerator.MoveNext(); index++)
				{
					Bitmap image = TryRender(scale, enumerator.Current);

					if (image == null)
					{
						Dispose(images);

						return null;
					}
					else
						images[index] = image;
				}

			return images;
		}

		private static void Dispose(IEnumerable<Bitmap> images)
		{
			foreach (Bitmap image in images)
				if (image != null)
					image.Dispose();
		}

		private static Bitmap TryRender(
			Int32 scale,
			String document)
		{
			using (var webBrowser = new WebBrowser())
			{
				webBrowser.DocumentText = document;
				webBrowser.ScrollBarsEnabled = false;

				while (webBrowser.ReadyState != WebBrowserReadyState.Complete)
					Application.DoEvents();

				((SHDocVw.WebBrowser)webBrowser.ActiveXInstance).ExecWB(
					SHDocVw.OLECMDID.OLECMDID_OPTICAL_ZOOM,
					SHDocVw.OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER,
					100 / scale,
					IntPtr.Zero);

				webBrowser.Width = webBrowser.Document.Body.ScrollRectangle.Width / scale;
				webBrowser.Height = webBrowser.Document.Body.ScrollRectangle.Height / scale;

				try
				{
					var bitmap = new Bitmap(webBrowser.Width, webBrowser.Height, PixelFormat.Format24bppRgb);

					webBrowser.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));

					return bitmap;
				}
				catch (ArgumentException)
				{
					return null;
				}
			}
		}
	}
}
