using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace SuperSocket.Common
{
	public static class FileHelper
	{
		public static string ReadFileAsString(string filePath)
		{
			if (File.Exists(filePath))
			{
				string result = string.Empty;
				StreamReader sr = null;
				try
				{
					sr = new StreamReader(filePath, Encoding.UTF8);
					result = sr.ReadToEnd();
				}
				catch
				{
					result = string.Empty;
				}
				finally
				{
					sr.Close();
					sr.Dispose();
				}
				return result;
			}
			else
				return string.Empty;
		}


		public static bool SaveStringAsFile(string content, string filePath)
		{
			bool result = true;
			StreamWriter sw = null;
			try
			{
				sw = new StreamWriter(filePath, false, Encoding.UTF8);
				sw.Write(content);
				result = true;
			}
			catch
			{
				result = false;
			}
			finally
			{
				if (sw != null)
				{
					sw.Close();
					sw.Dispose();
				}
			}
			return result;
		}

		public static string GetParentFolder(string path, out string fileName)
		{
			string seperator	= "\\";
			
			int pos		= path.IndexOf("/");
			
			if(pos>=0)
				seperator	= "/";
		
			fileName	= string.Empty;

			if (path.EndsWith(seperator))
			{
				path = path.Substring(0, path.Length - 1);
			}

			pos	= path.LastIndexOf(seperator);

			if (pos > 0)
			{
				fileName	= path.Substring(pos + 1);
				path		= path.Substring(0, pos);				
			}
			else if(pos == 0)
			{
				fileName	= path.Substring(1);
				path		= seperator;
			}

			return path;
		}

		public static string GetParentFolder(string path)
		{
			string fileName	= string.Empty;
			
			return GetParentFolder(path, out fileName);
		}
		
		public static string GetExtention(string filename)
		{
			if(filename.EndsWith("."))
				return string.Empty;
			
			int pos	= filename.LastIndexOf(".");
			if(pos>=0)
				return filename.Substring(pos).ToLower();
			else
				return string.Empty;
		}

		public static string GetRandonFileName(string oldFilename)
		{
			return Guid.NewGuid().ToString() + GetExtention(oldFilename);
		}
	}
}
