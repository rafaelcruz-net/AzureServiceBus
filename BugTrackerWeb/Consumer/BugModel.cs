using System;
using System.Collections.Generic;
using System.Text;

namespace Consumer
{
    public class BugModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string User { get; set; }
        public Severity Severity { get; set; }
    }

    public enum Severity
    {
        Block = 1,
        Critical = 2,
        Low = 3
    }
}
