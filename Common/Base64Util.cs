using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GiantSoft.Common
{
	public class Base64Util
	{
		public string Encode(byte[] data)
		{
			return Convert.ToBase64String(data);
		}
		
		public byte[] Decode(string strBase64)
		{
			return Convert.FromBase64String(strBase64);
		}

		public static void EncodeFile(string inputFile, string outputFile)
		{
			using(FileStream inputStream = File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using(StreamWriter outputWriter = new StreamWriter(outputFile, false, Encoding.ASCII))
				{
					byte[] data = new byte[57 * 1024]; //Chunk size is 57k
					int read	= inputStream.Read(data, 0, data.Length);
					
					while(read > 0)
					{
						outputWriter.WriteLine(Convert.ToBase64String(data, 0, read, Base64FormattingOptions.InsertLineBreaks));
						read = inputStream.Read(data, 0, data.Length);
					}
					
					outputWriter.Close();					
				}
				
				inputStream.Close();
			}
		}

		public static void DecodeFile(string inputFile, string outputFile)
		{
			using (StreamReader reader = new StreamReader(inputFile, Encoding.ASCII, true))
			{
				using (FileStream outputStream = File.Create(outputFile))
				{				
					string line = reader.ReadLine();

					while (!string.IsNullOrEmpty(line))
					{
						if (line.Length > 76)
							throw new InvalidDataException("Invalid mime-format base64 file");

						byte[] chunk = Convert.FromBase64String(line);
						outputStream.Write(chunk, 0, chunk.Length);
						line = reader.ReadLine();
					}

					outputStream.Close();
				}

				reader.Close();
			}
		}
	}
}
