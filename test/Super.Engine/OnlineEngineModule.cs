using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace Super.Engine
{
    [DependsOn(
      typeof(AbpAutoMapperModule)
      )]
    public class OnlineEngineModule : AbpModule
    {        
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            /////配置Mapping
            //Configure<AbpAutoMapperOptions>(options =>
            //{
            //    options.AddMaps<OnlineEngineModule>(validate: true);
            //});
        }
    }
}
