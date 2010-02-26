using System;
using System.Collections.Generic;
using System.Text;

namespace GiantSoft.Common
{
	public class SexFormatProvider : IFormatProvider, ICustomFormatter
	{
		private string m_Male;
		private string m_Female;
		
		public SexFormatProvider()
		{
			m_Male		= GlobalResources.GetString("SEX_MALE");
			m_Female	= GlobalResources.GetString("SEX_FEMALE");
		}
		
		#region IFormatProvider Members

		public object GetFormat(Type formatType)
		{
			if (formatType == typeof(ICustomFormatter))
				return this;
			else
				return null;
		}

		#endregion

		#region ICustomFormatter Members

		public string Format(string format, object arg, IFormatProvider formatProvider)
		{
			bool value = (bool)arg;
			
			return value ? m_Male : m_Female;
										
			//format = (format == null ? null : format.Trim().ToLower());
			//switch (format)
			//{
			//    case "yn":
			//        return value ? "Yes" : "No";
			//    default:
			//        if (arg is IFormattable)
			//            return ((IFormattable)arg).ToString(format, formatProvider);
			//        else
			//            return arg.ToString();
			//}
		}

		#endregion
	}
}
