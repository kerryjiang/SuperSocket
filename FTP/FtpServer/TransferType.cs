using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.FtpService
{
	public enum TransferType
	{
		A,//ASCII text 
		E,//EBCDIC text 
		I,//Image (binary data) 
		L //local format
	}
}
