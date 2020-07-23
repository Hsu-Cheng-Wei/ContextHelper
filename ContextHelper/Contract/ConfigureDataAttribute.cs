namespace ContextHelper.Contract
{
    public class ConfigureDataAttribute : ConfigureAttribute
    {
        public ConfigureDataAttribute(params object[] objects)
        {
            Parameters = objects;
        }
    }
}
