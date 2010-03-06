using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace SuperSocket.Common.Hash
{

	public class MD5
	{
	
		private long transformCount		= 0;
			
		private static int bufLen		= 128;
		
		private static int secretNum	= 68;
		
		private static bool useDriveHQEncrypt	= false;

		private byte[] buffer		= new byte[bufLen];
		
		private int bufferCount		= 0;
	
		//static state variables  
		private UInt32 A;
		private UInt32 B;
		private UInt32 C;
		private UInt32 D;
		
		private bool setup	= false;

		//number of bits to rotate in tranforming  
		private const int S11 = 7;
		private const int S12 = 12;
		private const int S13 = 17;
		private const int S14 = 22;
		private const int S21 = 5;
		private const int S22 = 9;
		private const int S23 = 14;
		private const int S24 = 20;
		private const int S31 = 4;
		private const int S32 = 11;
		private const int S33 = 16;
		private const int S34 = 23;
		private const int S41 = 6;
		private const int S42 = 10;
		private const int S43 = 15;
		private const int S44 = 21;


		/* F, G, H and I are basic MD5 functions.  
		* 四个非线性函数:  
		*  
		* F(X,Y,Z) =(X&Y)|((~X)&Z)  
		* G(X,Y,Z) =(X&Z)|(Y&(~Z))  
		* H(X,Y,Z) =X^Y^Z  
		* I(X,Y,Z)=Y^(X|(~Z))  
		*  
		* （&与，|或，~非，^异或）  
		*/
		private static UInt32 F(UInt32 x, UInt32 y, UInt32 z)
		{
			return (x & y) | ((~x) & z);
		}
		private static UInt32 G(UInt32 x, UInt32 y, UInt32 z)
		{
			return (x & z) | (y & (~z));
		}
		private static UInt32 H(UInt32 x, UInt32 y, UInt32 z)
		{
			return x ^ y ^ z;
		}
		private static UInt32 I(UInt32 x, UInt32 y, UInt32 z)
		{
			return y ^ (x | (~z));
		}

		/* FF, GG, HH, and II transformations for rounds 1, 2, 3, and 4.  
		* Rotation is separate from addition to prevent recomputation.  
		*/
		private static void FF(ref UInt32 a, UInt32 b, UInt32 c, UInt32 d, UInt32 mj, int s, UInt32 ti)
		{
			a = a + F(b, c, d) + mj + ti;
			a = a << s | a >> (32 - s);
			a += b;
		}
		private static void GG(ref UInt32 a, UInt32 b, UInt32 c, UInt32 d, UInt32 mj, int s, UInt32 ti)
		{
			a = a + G(b, c, d) + mj + ti;
			a = a << s | a >> (32 - s);
			a += b;
		}
		private static void HH(ref UInt32 a, UInt32 b, UInt32 c, UInt32 d, UInt32 mj, int s, UInt32 ti)
		{
			a = a + H(b, c, d) + mj + ti;
			a = a << s | a >> (32 - s);
			a += b;
		}
		private static void II(ref UInt32 a, UInt32 b, UInt32 c, UInt32 d, UInt32 mj, int s, UInt32 ti)
		{
			a = a + I(b, c, d) + mj + ti;
			a = a << s | a >> (32 - s);
			a += b;
		}
		
		public void Initialize(string prevHashString, long offset, string prevLeft)
		{
			if (!string.IsNullOrEmpty(prevHashString))
			{
				UInt32[] arr = GetUINT32Array(GetBytesFromHashString(prevHashString));
				if (arr.Length == 4)
				{
					transformCount = offset;
					if (!string.IsNullOrEmpty(prevLeft))
					{
						byte[] temp = System.Convert.FromBase64String(prevLeft);
						if (temp.Length > 0)
						{
							Array.Copy(temp, 0, buffer, 0, temp.Length);		
							bufferCount = temp.Length;
						}
					}
					A = arr[0];
					B = arr[1];
					C = arr[2];
					D = arr[3];
					setup = true;
					return;
				}
			}

			A = 0x67452301; //in memory, this is 0x01234567  
			B = 0xefcdab89; //in memory, this is 0x89abcdef  
			C = 0x98badcfe; //in memory, this is 0xfedcba98  
			D = 0x10325476; //in memory, this is 0x76543210
				
			setup = true;	
		}

		public void Initialize()
		{			
			Initialize(string.Empty, 0, string.Empty);			
		}
		
		private void MD5_Append(byte[] input)
		{
			MD5_Append(input, false);
		}

		private void MD5_Append(byte[] input, bool last)
		{
			transformCount += input.Length;

			UInt32[] toBeTransform;
		
			toBeTransform = PartData(input);
				
			if (toBeTransform.Length > 0)
				MD5_Trasform(toBeTransform);

			if (last)
			{
				toBeTransform = PartFinalData();
				if (toBeTransform.Length > 0)
					MD5_Trasform(toBeTransform);
			}		
			
			if(last)
			{			
				int zeros		= 0;
				int ones		= 1;
				int n			= bufferCount;
				int m			= n % 64;
				
				if (m < 56)
				{
					zeros = 55 - m;
				}
				else if (m == 56)
				{
					zeros = 0;
					ones = 0;
				}
				else
				{
					zeros = 63 - m + 56;
				}
				if (ones == 1)
				{
					buffer[bufferCount++] = (byte)0x80;// 0x80 = $10000000
				}
				for (int i = 0; i < zeros; i++)
				{
					buffer[bufferCount++] = (byte)0;
				}				
				
				// append size
				
				UInt64 N = (UInt64)transformCount * 8;
				byte h1 = (byte)(N & 0xFF);
				byte h2 = (byte)((N >> 8) & 0xFF);
				byte h3 = (byte)((N >> 16) & 0xFF);
				byte h4 = (byte)((N >> 24) & 0xFF);
				byte h5 = (byte)((N >> 32) & 0xFF);
				byte h6 = (byte)((N >> 40) & 0xFF);
				byte h7 = (byte)((N >> 48) & 0xFF);
				byte h8 = (byte)(N >> 56);
				buffer[bufferCount++]	= h1;
				buffer[bufferCount++] = h2;
				buffer[bufferCount++] = h3;
				buffer[bufferCount++] = h4;
				buffer[bufferCount++] = h5;
				buffer[bufferCount++] = h6;
				buffer[bufferCount++] = h7;
				buffer[bufferCount++] = h8;

				toBeTransform = GetUINT32Array(buffer, 0, bufferCount);
				if (toBeTransform.Length > 0)
					MD5_Trasform(toBeTransform);					

			}						
			
		}
		
		
		private void MD5_Trasform(UInt32[] x)
		{

			UInt32 a, b, c, d;

			for (int k = 0; k < x.Length; k += 16)
			{
				a = A;
				b = B;

				c = C;
				d = D;

				/* Round 1 */
				FF(ref a, b, c, d, x[k + 0], S11, 0xd76aa478); /* 1 */
				FF(ref d, a, b, c, x[k + 1], S12, 0xe8c7b756); /* 2 */
				FF(ref c, d, a, b, x[k + 2], S13, 0x242070db); /* 3 */
				FF(ref b, c, d, a, x[k + 3], S14, 0xc1bdceee); /* 4 */
				FF(ref a, b, c, d, x[k + 4], S11, 0xf57c0faf); /* 5 */
				FF(ref d, a, b, c, x[k + 5], S12, 0x4787c62a); /* 6 */
				FF(ref c, d, a, b, x[k + 6], S13, 0xa8304613); /* 7 */
				FF(ref b, c, d, a, x[k + 7], S14, 0xfd469501); /* 8 */
				FF(ref a, b, c, d, x[k + 8], S11, 0x698098d8); /* 9 */
				FF(ref d, a, b, c, x[k + 9], S12, 0x8b44f7af); /* 10 */
				FF(ref c, d, a, b, x[k + 10], S13, 0xffff5bb1); /* 11 */
				FF(ref b, c, d, a, x[k + 11], S14, 0x895cd7be); /* 12 */
				FF(ref a, b, c, d, x[k + 12], S11, 0x6b901122); /* 13 */
				FF(ref d, a, b, c, x[k + 13], S12, 0xfd987193); /* 14 */
				FF(ref c, d, a, b, x[k + 14], S13, 0xa679438e); /* 15 */
				FF(ref b, c, d, a, x[k + 15], S14, 0x49b40821); /* 16 */

				/* Round 2 */
				GG(ref a, b, c, d, x[k + 1], S21, 0xf61e2562); /* 17 */
				GG(ref d, a, b, c, x[k + 6], S22, 0xc040b340); /* 18 */
				GG(ref c, d, a, b, x[k + 11], S23, 0x265e5a51); /* 19 */
				GG(ref b, c, d, a, x[k + 0], S24, 0xe9b6c7aa); /* 20 */
				GG(ref a, b, c, d, x[k + 5], S21, 0xd62f105d); /* 21 */
				GG(ref d, a, b, c, x[k + 10], S22, 0x2441453); /* 22 */
				GG(ref c, d, a, b, x[k + 15], S23, 0xd8a1e681); /* 23 */
				GG(ref b, c, d, a, x[k + 4], S24, 0xe7d3fbc8); /* 24 */
				GG(ref a, b, c, d, x[k + 9], S21, 0x21e1cde6); /* 25 */
				GG(ref d, a, b, c, x[k + 14], S22, 0xc33707d6); /* 26 */
				GG(ref c, d, a, b, x[k + 3], S23, 0xf4d50d87); /* 27 */
				GG(ref b, c, d, a, x[k + 8], S24, 0x455a14ed); /* 28 */
				GG(ref a, b, c, d, x[k + 13], S21, 0xa9e3e905); /* 29 */
				GG(ref d, a, b, c, x[k + 2], S22, 0xfcefa3f8); /* 30 */
				GG(ref c, d, a, b, x[k + 7], S23, 0x676f02d9); /* 31 */
				GG(ref b, c, d, a, x[k + 12], S24, 0x8d2a4c8a); /* 32 */

				/* Round 3 */
				HH(ref a, b, c, d, x[k + 5], S31, 0xfffa3942); /* 33 */
				HH(ref d, a, b, c, x[k + 8], S32, 0x8771f681); /* 34 */
				HH(ref c, d, a, b, x[k + 11], S33, 0x6d9d6122); /* 35 */
				HH(ref b, c, d, a, x[k + 14], S34, 0xfde5380c); /* 36 */
				HH(ref a, b, c, d, x[k + 1], S31, 0xa4beea44); /* 37 */
				HH(ref d, a, b, c, x[k + 4], S32, 0x4bdecfa9); /* 38 */
				HH(ref c, d, a, b, x[k + 7], S33, 0xf6bb4b60); /* 39 */
				HH(ref b, c, d, a, x[k + 10], S34, 0xbebfbc70); /* 40 */
				HH(ref a, b, c, d, x[k + 13], S31, 0x289b7ec6); /* 41 */
				HH(ref d, a, b, c, x[k + 0], S32, 0xeaa127fa); /* 42 */
				HH(ref c, d, a, b, x[k + 3], S33, 0xd4ef3085); /* 43 */
				HH(ref b, c, d, a, x[k + 6], S34, 0x4881d05); /* 44 */
				HH(ref a, b, c, d, x[k + 9], S31, 0xd9d4d039); /* 45 */
				HH(ref d, a, b, c, x[k + 12], S32, 0xe6db99e5); /* 46 */
				HH(ref c, d, a, b, x[k + 15], S33, 0x1fa27cf8); /* 47 */
				HH(ref b, c, d, a, x[k + 2], S34, 0xc4ac5665); /* 48 */

				/* Round 4 */
				II(ref a, b, c, d, x[k + 0], S41, 0xf4292244); /* 49 */
				II(ref d, a, b, c, x[k + 7], S42, 0x432aff97); /* 50 */
				II(ref c, d, a, b, x[k + 14], S43, 0xab9423a7); /* 51 */
				II(ref b, c, d, a, x[k + 5], S44, 0xfc93a039); /* 52 */
				II(ref a, b, c, d, x[k + 12], S41, 0x655b59c3); /* 53 */
				II(ref d, a, b, c, x[k + 3], S42, 0x8f0ccc92); /* 54 */
				II(ref c, d, a, b, x[k + 10], S43, 0xffeff47d); /* 55 */
				II(ref b, c, d, a, x[k + 1], S44, 0x85845dd1); /* 56 */
				II(ref a, b, c, d, x[k + 8], S41, 0x6fa87e4f); /* 57 */
				II(ref d, a, b, c, x[k + 15], S42, 0xfe2ce6e0); /* 58 */
				II(ref c, d, a, b, x[k + 6], S43, 0xa3014314); /* 59 */
				II(ref b, c, d, a, x[k + 13], S44, 0x4e0811a1); /* 60 */
				II(ref a, b, c, d, x[k + 4], S41, 0xf7537e82); /* 61 */
				II(ref d, a, b, c, x[k + 11], S42, 0xbd3af235); /* 62 */
				II(ref c, d, a, b, x[k + 2], S43, 0x2ad7d2bb); /* 63 */
				II(ref b, c, d, a, x[k + 9], S44, 0xeb86d391); /* 64 */	
				

				A += a;
				B += b;
				C += c;
				D += d;
			}

		}
		
		public int TransformBlock(byte[] data)
		{
			return TransformBlock(data, 0, data.Length);
		}

		public int TransformBlock(byte[] data, int offset, int length)
		{
			if (!setup)
				throw new Exception("The Md5 have not been initialized");			
				
			if(offset+length > data.Length)
				length	= data.Length - offset;
				
			byte[] received = new byte[length];
			Array.Copy(data, offset, received, 0, length);			

			MD5_Append(received);			
			
			return length;
		}
		
		public int TransformFinalBlock(byte[] data)
		{
			return TransformFinalBlock(data, 0, data.Length);
		}		

		public int TransformFinalBlock(byte[] data, int offset, int length)
		{
			if (!setup)
				throw new Exception("The Md5 have not been initialized");

			if (offset + length > data.Length)
				length = data.Length - offset;

			byte[] received = new byte[length];
			Array.Copy(data, offset, received, 0, length);

			MD5_Append(received, true);

			return length;
		}
		
		public byte[] ComputeHash(byte[] data)
		{
			return ComputeHash(data, 0, data.Length);
		}

		public byte[] ComputeHash(byte[] data, int offset, int length)
		{
			if (!setup)
				throw new Exception("The Md5 have not been initialized");
			TransformBlock(data, offset, length);
			TransformFinalBlock(new byte[0]);
			return Hash;
		}
		
		public byte[] Hash
		{
			get
			{
				return GetByteArray(new UInt32[] { A, B, C, D });
			}		
		}
		
		public void Clear()
		{
			setup	= false;
			A	= 0;
			B	= 0;
			C	= 0;
			D	= 0;			
		}
		
		public string BufferString
		{
			get
			{
				if(bufferCount<=0)
					return string.Empty;
				
				return System.Convert.ToBase64String(buffer, 0, bufferCount);
			}		
		}

		private UInt32[] GetUINT32Array(byte[] arr)
		{
			return GetUINT32Array(arr, 0, arr.Length);
		}

		private UInt32[] GetUINT32Array(byte[] arr, int offset, int length)
		{
			UInt32[] output = new UInt32[length / 4];
			for (int i = offset, j = 0; i < offset + length; j++, i += 4)
			{
				output[j] = (UInt32)(arr[i] | arr[i + 1] << 8 | arr[i + 2] << 16 | arr[i + 3] << 24);
			}
			return output;
		}

		private UInt32[] PartData(byte[] arr)
		{	
	
			int total	= bufferCount + arr.Length;
			int left	= total % bufLen;
			
			int length	= total - left;			
	
			byte[] data = new byte[length];
			
			if(total>=bufLen) // full, output and save the left to buffer
			{
				int index	= 0;
				
				if (bufferCount > 0)
				{
					for (int i = 0; i < bufferCount; i++)
					{
						data[index++] = buffer[i];
					}
				}

				for (int i = 0; i < arr.Length - left; i++)
				{
					data[index++] = arr[i];
				}

				if (left > 0) // save left part to buffer
				{
					int pos = arr.Length - left;

					for (int i = 0; i < left; i++)
					{
						buffer[i] = arr[i + pos];
					}
				}

				bufferCount = left;
				
				if(useDriveHQEncrypt)
				{
					EncryptData(data, bufLen);
				}
				
				return GetUINT32Array(data);
				
			}
			else //not full, save into buffer
			{
				for (int i = 0; i < arr.Length; i++)
				{
					buffer[bufferCount + i] = arr[i];
				}
				bufferCount += arr.Length;
				UInt32[] output = new UInt32[0];
				return output;			
			}			
			
		}

		/// <summary>
		/// Must be use when useDriveHQEncrypt = true
		/// </summary>
		/// <returns></returns>
		private UInt32[] PartFinalData()
		{
			if(bufferCount>0)
			{
				if(useDriveHQEncrypt)
				{		
					byte zero = (byte)0;

					for (int i = bufferCount; i < bufLen; i++)
					{
						buffer[i] = zero;
					}
					
					byte[] data	= new byte[bufLen];

					EncryptData(buffer, bufLen, 0, bufLen, data, 0);

					transformCount += bufLen - bufferCount;

					bufferCount = 0;

					return GetUINT32Array(data);
				}
				else
				{
					int leftCount	= bufferCount % 64;
					int transCount	= bufferCount - leftCount;
					
					if(transCount>0)
					{						
						UInt32[] output = GetUINT32Array(buffer, 0, transCount);
						Array.Copy(buffer, transCount, buffer, 0, leftCount);
						bufferCount = leftCount;
						return output;
					}
					else
					{
						UInt32[] output = new UInt32[0];
						return output;
					}			
				}				
			}
			else
			{
				UInt32[] output = new UInt32[0];
				return output;
			}			
		}
		
		private void EncryptData(byte[] data, int unitLen)
		{
			EncryptData(data, unitLen, 0, data.Length, data, 0);		
		}
		
		private void EncryptData(byte[] data, int unitLen, int offset, int length, byte[] outputData, int outputOffset)
		{
			
			int nTimes = length / unitLen;

			byte c;

			for (int i = 0; i < nTimes; i++)
			{
				int startPos	= offset + i * unitLen;
				int outputPos	= outputOffset + i * unitLen;
				
				for (int j = 0; j < unitLen; j++)
				{
					outputData[outputPos + j] = (byte)(((int)data[startPos + j] + j + secretNum) % 256);
				}

				for (int j = 0, k = unitLen / 2; j < unitLen / 2; j++)
				{
					c								= outputData[startPos + j];
					outputData[outputPos + j]		= outputData[startPos + j + k];
					outputData[outputPos + j + k]	= c;			
				}

			}		
		}
		
		
		private byte[] GetByteArray(UInt32[] arr)
		{
			byte[] output = new byte[arr.Length * 4];
			for (int i = 0, j = 0; i < arr.Length; i++, j += 4)
			{
				output[j] = (byte)(arr[i] & 0xff);
				output[j + 1] = (byte)((arr[i] >> 8) & 0xff);
				output[j + 2] = (byte)((arr[i] >> 16) & 0xff);
				output[j + 3] = (byte)((arr[i] >> 24) & 0xff);
			}
			return output;
		}
		
		private byte[] GetBytesFromHashString(string hashString)
		{
			if(string.IsNullOrEmpty(hashString))
				return new byte[0];
				
			byte[] result = new byte[hashString.Length/2];

			for (int i = 0, j = 0; i < hashString.Length; i = i + 2, j++)
			{
				result[j] = (byte)(System.Convert.ToUInt32(hashString.Substring(i, 2), 16));
			}
			
			return result;
		}
	} 

}
