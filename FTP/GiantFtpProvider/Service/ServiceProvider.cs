using System;
using System.Collections.Generic;
using System.Text;

namespace GiantFtpProvider.Service
{
	static class ServiceProvider
	{
		private static object syncLock	= new object();
		private static IDataAccess da	= null;

		public static IDataAccess DAInstance
		{
			get
			{
				if(da==null)
				{
					lock(syncLock)
					{
						da	= new SQLServerDataAccess();
					}
				}
				
				return da;
			}
		}
	}
}
