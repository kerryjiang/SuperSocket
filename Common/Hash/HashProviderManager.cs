using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.Common.Hash
{
	public static class HashProviderManager
	{
		public static IHashProvider Create()
		{
			return new SmartHashProvider();
		}
	}
}
