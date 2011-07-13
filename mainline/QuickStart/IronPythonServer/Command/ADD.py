import clr
clr.AddReference("SuperSocket.Common")
clr.AddReference("SuperSocket.SocketBase")

from SuperSocket.Common import *
from SuperSocket.SocketBase import *

def execute(session, command):
	session.SendResponse(command.Data)