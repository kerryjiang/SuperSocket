using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerformanceTestAgent.ViewModel
{
    public class ContentAppendEventArgs : EventArgs
    {
        public string Content { get; private set; }

        public ContentAppendEventArgs(string content)
            : base()
        {
            Content = content;
        }
    }
}
