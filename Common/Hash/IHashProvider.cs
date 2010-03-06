using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.Common.Hash
{
	public interface IHashProvider
	{
		void Setup();

		/// <summary>
		/// Setup the hash provider by these parameters
		/// </summary>
		/// <param name="prevHashString">The prev hash string.</param>
		/// <param name="offset">The offset the hash provider want to start at</param>
		/// <param name="prevLeft">The prev left buffer string in md5 provider.</param>
		void Setup(string prevHashString, long offset, string prevLeft);
	
		void TransformBlock(byte[] data, int offset, int length);
		
		void TransformFinalBlock();

		void TransformFinalBlock(byte[] data, int offset, int length);

		/// <summary>
		/// Gets the hash string after finish
		/// </summary>
		/// <returns></returns>
		string GetHashString();

		/// <summary>
		/// Used to get hash string when the hash procedure have not finished
		/// </summary>
		/// <returns></returns>
		string GetTempHashString();
		
		long Offset { get;}
		
		void Clear();

		/// <summary>
		/// Gets the left buffer string in the md5 calculator.
		/// </summary>
		/// <returns></returns>
		string GetLeftBufferString();

	}
}
