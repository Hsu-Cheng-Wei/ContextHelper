# 動態Class產生器

## 用途
**以更視覺化的方式New 物件，方便單元測試時建構大量mock資料的可閱讀性**

## Demo
    public class Company
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public bool Disable { get; set; }

        public DateTime CreateDate { get; set; }
    }

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
    }

    void Main()
    {
        CreateObject<CompanySetting>.Build().Dump();
    }

## 結果相當於

![image](https://github.com/Hsu-Cheng-Wei/ContextHelper/blob/master/Result.PNG?raw=true)