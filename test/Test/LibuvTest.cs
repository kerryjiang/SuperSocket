namespace Tests
{
    public class LibuvTest : TestBase
    {
        protected override void RegisterServices(IServiceCollection services)
        {
            services.UseLibuvListener();
        }
    }
}