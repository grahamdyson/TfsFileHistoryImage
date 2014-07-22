using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DevSnicket.TfsFileHistoryImage
{
	class Program
	{
		[STAThread]
		static void Main(String[] args)
		{
			if (args.Length == 0)
				Console.WriteLine("Either specify a local file path in a workspace or the address of a TFS server followed by a server file path.");
			else
			{
				AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

				try
				{
					Console.WriteLine("Rendering TFS file versions...");

					Bitmap[] images =
						RenderTfsFileVersions(
							args.Last(),
							args.Length > 1 ? new Uri(args[0]) : null);

					String tempFilePath = Path.GetTempFileName() + ".gif";

					try
					{
						Console.WriteLine("Writing animated GIF file...");

						using (var outputFileStream = File.OpenWrite(tempFilePath))
							WriteAnimatedGif(
								frameDelay: TimeSpan.FromMilliseconds(Math.Min(1000, 10000 / images.Length)),
								images: images.Reverse(),
								outputStream: outputFileStream);
					}
					finally
					{
						foreach (Bitmap image in images)
							image.Dispose();
					}

					Console.WriteLine("Opening animated GIF file...");

					Process.Start(tempFilePath);
				}
				finally
				{
					AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolve;
				}
			}
		}

		private static Assembly AssemblyResolve(Object sender, ResolveEventArgs args)
		{
			using (Stream manifestResourceStream = _assembly.GetManifestResourceStream(_embeddedPath + new AssemblyName(args.Name).Name + ".dll"))
				if (manifestResourceStream == null)
					return null;
				else
					using (var memoryStream = new MemoryStream())
					{
						manifestResourceStream.CopyTo(memoryStream);
						return Assembly.Load(memoryStream.ToArray());
					}
		}

		private static readonly Assembly _assembly = typeof(Program).Assembly;
		private static readonly string _embeddedPath = typeof(Program).Namespace + ".Assemblies.";

		private static Bitmap[] RenderTfsFileVersions(
			String path,
			Uri server)
		{
			Bitmap[] images =
				HtmlRenderer.Render(
					TfsFileVersionsWithHtmlFormatExtractor.Get(
						path,
						server));

			try
			{
				return
					ImageProcessor.Process(
						images,
						new Size(width: 1600, height: 900))
					.ToArray();
			}
			finally
			{
				foreach (Bitmap image in images)
					image.Dispose();
			}
		}

		private static void WriteAnimatedGif(
			TimeSpan frameDelay,
			IEnumerable<Bitmap> images,
			Stream outputStream)
		{
			using (BumpKit.GifEncoder gifEncoder = new BumpKit.GifEncoder(outputStream))
			{
				gifEncoder.FrameDelay = frameDelay;

				foreach (Bitmap image in images)
				{
					gifEncoder.AddFrame(image);

					image.Dispose();
				}
			}
		}
	}
}