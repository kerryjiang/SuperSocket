using Super.Engine;
using Volo.Abp.Modularity;

namespace Super.Command
{
    [DependsOn(
       typeof(OnlineEngineModule)
       )]
    public class OnlineCommandModule : AbpModule
    {

    }
}
