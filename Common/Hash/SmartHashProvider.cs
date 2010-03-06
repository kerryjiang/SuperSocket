using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.Common.Hash
{
	public class SmartHashProvider : IHashProvider 
	{
		private MD5 md5Provider		= null;
		private string md5Result	= string.Empty;
		private long _offset		= 0;

		#region IHashProvider Members

		public void Setup()
		{
			Setup(string.Empty, 0, string.Empty);
		}

		public void Setup(string prevHashString, long offset, string prevLeft)
		{
			md5Provider = new MD5();
			md5Provider.Initialize(prevHashString, offset, prevLeft);
			_offset		= offset;
	
		}

		public void TransformBlock(byte[] data, int offset, int length)
		{
			_offset		= _offset + length;
			md5Provider.TransformBlock(data, offset, length);
		}

		public void TransformFinalBlock()
		{
			TransformFinalBlock(new byte[0], 0, 0);
		}

		public void TransformFinalBlock(byte[] data, int offset, int length)
		{
			md5Provider.TransformFinalBlock(data, offset, length);
			md5Result = "" + System.BitConverter.ToString(md5Provider.Hash);
			md5Result = md5Result.Replace("-", string.Empty) + "1";// Version1
			md5Provider.Clear();
		}

		public string GetHashString()
		{
			return md5Result;
		}


		public string GetTempHashString()
		{
			string tempHash = "" + System.BitConverter.ToString(md5Provider.Hash);
			tempHash = tempHash.Replace("-", string.Empty);//Do not add '1' if return middle hash string
			return tempHash;
		}

		public long Offset
		{
			get { return _offset; }
		}

		public void Clear()
		{
			if (md5Provider != null)
			{
				md5Provider.Clear();
				md5Provider = null;
			}
		}

		public string GetLeftBufferString()
		{
			return md5Provider.BufferString;
		}
		
		#endregion
	}
}
