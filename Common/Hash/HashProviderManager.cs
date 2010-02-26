using System;
using System.Collections.Generic;
using System.Text;

namespace GiantSoft.Common.Hash
{
	public static class HashProviderManager
	{
		public static IHashProvider Create()
		{
			return new SmartHashProvider();
		}
	}
}
