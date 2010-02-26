using System;
using System.Collections.Generic;
using System.Text;

namespace GiantSoft.Common.IO
{
	public class FileContentAppendEventArgs : EventArgs
	{
		public string AppendedContent { get; private set; }

		public FileContentAppendEventArgs()
		{
			AppendedContent = string.Empty;
		}

		public FileContentAppendEventArgs(string content)
		{
			AppendedContent = content;
		}
	}

	public delegate void FileContentAppendEventHandler(object sender, FileContentAppendEventArgs args);
}
