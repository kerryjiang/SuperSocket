import clr
clr.AddReference("System")
clr.AddReference("SuperSocket.Common")
clr.AddReference("SuperSocket.SocketBase")

from System import *
from SuperSocket.Common import *
from SuperSocket.SocketBase import *

def execute(session, command):
	session.SendResponse((Convert.ToInt32(command[0]) * Convert.ToInt32(command[1])).ToString())