version=$1
nuget delete SuperSocket.Client.Proxy "$version" -source nuget.org -NonInteractive
nuget delete SuperSocket.Client "$version" -source nuget.org -NonInteractive
nuget delete SuperSocket.WebSocket.Server "$version" -source nuget.org -NonInteractive
nuget delete SuperSocket.WebSocket "$version" -source nuget.org -NonInteractive
nuget delete SuperSocket.Server "$version" -source nuget.org -NonInteractive
nuget delete SuperSocket.SessionContainer "$version" -source nuget.org -NonInteractive
nuget delete SuperSocket.Command "$version" -source nuget.org -NonInteractive
nuget delete SuperSocket.Connection "$version" -source nuget.org -NonInteractive
nuget delete SuperSocket.Primitives "$version" -source nuget.org -NonInteractive
nuget delete SuperSocket.ProtoBase "$version" -source nuget.org -NonInteractive