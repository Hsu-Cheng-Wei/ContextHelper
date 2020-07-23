using ContextHelper;
using ContextHelper.Contract;
using ContextHelperTests.Model;
using System;

namespace ContextHelperTests.Configure
{
    public class CompanySetting
    {
        [Configure, ConfigureData("68f1de41-8fd4-4fdc-afe7-3f74051ecfaf", "2020/07/23 16:00", "A", "Taiwan", false)]
        [Configure, ConfigureData("7edf30de-ce7c-493e-9f82-a4478f1b7089", "2020/07/22 16:00", "B", "Janpan", false)]
        [Configure, ConfigureData("6f39946d-7b1c-4603-8590-5a01d41413b6", "2020/07/21 16:00", "C", "China", true)]
        public ObjectConfigure CompanyConfigure = new ObjectConfigure<Company>()
        .SelectMemberConvert<string, Guid>(c => c.Id, s => Guid.Parse(s))
        .SelectMemberConvert<string, DateTime>(c => c.CreateDate, s => DateTime.Parse(s))
        .SelectMember(c => c.Name)
        .SelectMember(c => c.Address)
        .SelectMember(c => c.Disable);

        /*
            new Company[]
            {
                new Company
                {
                    Id = Guid.Parse("68f1de41-8fd4-4fdc-afe7-3f74051ecfaf"),
                    Name = "A",
                    Address = "Taiwan",
                    Disable = false,
                    CreateDate = DateTime.Parse("2020/07/23 16:00")
                },
                new Company
                {
                    Id = Guid.Parse("7edf30de-ce7c-493e-9f82-a4478f1b7089"),
                    Name = "B",
                    Address = "Janpan",
                    Disable = false,
                    CreateDate = DateTime.Parse("2020/07/22 16:00")
                },
                new Company
                {
                    Id = Guid.Parse("6f39946d-7b1c-4603-8590-5a01d41413b6"),
                    Name = "C",
                    Address = "China",
                    Disable = true,
                    CreateDate = DateTime.Parse("2020/07/21 16:00")
                },
            }
         */
    }
}
