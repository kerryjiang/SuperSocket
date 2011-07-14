import clr
clr.AddReference("System")

from System import *

def execute(session, command):
	session.SendResponse((Convert.ToInt32(command[0]) * Convert.ToInt32(command[1])).ToString())