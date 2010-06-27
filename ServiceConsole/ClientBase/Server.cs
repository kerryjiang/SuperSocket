using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ServiceConsole.ClientBase
{
    public class Server
    {
        public Type ServerType { get; set; }

        public List<Task> Menus { get; private set; }

        public Server()
        {
            var menus = new List<Task>();

            menus.Add(new Task { Name = "Status" });
            menus.Add(new Task { Name = "Users" });
            menus.Add(new Task { Name = "Connections" });

            Menus = menus;
        }
    }

    public class Task
    {
        public string Name { get; set; }
    }
}
