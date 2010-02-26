using System;
using System.Collections.Generic;
using System.Text;

namespace GiantFtpProvider.Model
{
	public partial class FileInfo
	{
		private bool tempHashResolved = false;
		
		private void ResolveTempHash()
		{
			if(!string.IsNullOrEmpty(tempHash))
			{
				string[] arrTempHash	= tempHash.Split('|');

				if (arrTempHash != null && arrTempHash.Length==3)
				{
					long.TryParse(arrTempHash[0], out offset);
					tempHashCode	= arrTempHash[1];
					prevHashBuffer	= arrTempHash[2];					
				}			
			}
			
			tempHashResolved = true;
		}
		
		private long offset = 0;
		
		public long Offset
		{
			get
			{
				if(!tempHashResolved)
					ResolveTempHash();
					
				return offset;
			}
		}		
		
		private string prevHashBuffer = string.Empty;

		public string PrevHashBuffer
		{
			get
			{
				if (!tempHashResolved)
					ResolveTempHash();
				
				return prevHashBuffer;
			}
		}
		
		private string tempHashCode = string.Empty;
		
		public string TempHashCode
		{
			get
			{
				if (!tempHashResolved)
					ResolveTempHash();

				return tempHashCode;
			}
		
		}
		
		public void SetTempHash(long offset, string hashCode, string prevBuffer)
		{
			TempHash = offset.ToString() + "|" + hashCode + "|" + prevBuffer;
		}		
	}
}
