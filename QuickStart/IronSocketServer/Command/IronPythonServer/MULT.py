def execute(session, request):
	session.Send(str(int(request[0]) * int(request[1])))