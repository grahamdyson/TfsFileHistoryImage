using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Manoli.Utils.CSharpFormat;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace DevSnicket.TfsFileHistoryImage
{
	internal class TfsFileVersionsWithHtmlFormatExtractor
	{
		public static String[] Get(
			String path,
			Uri server)
		{
			var extractor = new TfsFileVersionsWithHtmlFormatExtractor(path: path);
			
			return
				extractor
				.FormatInHtml(
					extractor.GetVersionsFromServer(server ?? GetServerFromPath(path)))
				.ToArray();
		}

		private static Uri GetServerFromPath(String path)
		{
			return Workstation.Current.GetLocalWorkspaceInfo(path).ServerUri;
		}

		private TfsFileVersionsWithHtmlFormatExtractor(String path)
		{
			_path = path;
		}

		private readonly String _path;

		private String[] GetVersionsFromServer(
			Uri server)
		{
			String tempPath = Path.GetTempFileName();

			using (var collection = new TfsTeamProjectCollection(server))
				try
				{
					return
						GetChanges(collection)
						.Select(
							change =>
							{
								change.Item.DownloadFile(tempPath);
								return File.ReadAllText(tempPath);
							})
						.ToArray();
				}
				finally
				{
					if (File.Exists(tempPath))
						File.Delete(tempPath);
				}
		}

		private IEnumerable<Change> GetChanges(
			TfsTeamProjectCollection collection)
		{
			var versionControl = collection.GetService<VersionControlServer>();

			return
				versionControl.QueryHistory(
					path: _path,
					version: LatestVersionSpec.Instance,
					deletionId: 0,
					recursion: RecursionType.Full,
					user: null,
					versionFrom: null,
					versionTo: null,
					maxCount: Int32.MaxValue,
					includeChanges: false,
					slotMode: false)
				.Cast<Changeset>()
				.SelectMany(
					changeSet =>
						changeSet.Changes.Any()
						?
						new[] { changeSet.Changes.Single(), }
						:
						versionControl.QueryMergesExtended(
							new ItemSpec(_path, RecursionType.Full),
							new ChangesetVersionSpec(changeSet.ChangesetId),
							null,
							null)
						.Select(merge => merge.SourceItem))
				.OrderByDescending(
					change => change.Item.ChangesetId);
		}

		private IEnumerable<String> FormatInHtml(
			IEnumerable<String> versions)
		{
			return
				_path.EndsWith(".cs")
				?
				versions.Select(FormatCSharpInHtml)
				:
				versions;
		}

		private static String FormatCSharpInHtml(
			String content)
		{
			return
				String.Format(
					_cSharpFormat,
					new CSharpFormat().FormatCode(content));
		}

		private readonly static String _cSharpFormat = GetCSharpHtmlFormat();

		private static String GetCSharpHtmlFormat()
		{
			using (var cssReader = new StreamReader(typeof(CSharpFormat).Assembly.GetManifestResourceStream("csharp.css")))
				return
					String.Format(
						"<html><head><style media=\"screen\" type=\"text/css\">{0}</style></head><body>{{0}}</body></html>",
						cssReader.ReadToEnd().Replace("{", "{{").Replace("}", "}}"));
		}
	}
}
