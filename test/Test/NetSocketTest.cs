namespace Tests
{
    public class NetSocketTest : TestBase
    {
        protected override void RegisterServices(IServiceCollection services)
        {
            services.UseNetSocketListener();
        }
    }
}