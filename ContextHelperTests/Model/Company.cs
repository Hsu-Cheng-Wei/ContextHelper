using System;

namespace ContextHelperTests.Model
{
    public class Company
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public bool Disable { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
