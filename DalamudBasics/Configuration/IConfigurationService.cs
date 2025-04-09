namespace DalamudBasics.Configuration
{
    public interface IConfigurationService<T> where T : IConfiguration, new()
    {
        T GetConfiguration();

        void SaveConfiguration();
    }
}
