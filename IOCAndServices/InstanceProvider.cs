using Microsoft.Extensions.DependencyInjection;

namespace FeedFetcher.IOCAndServices
{
    public static class InstanceProvider
    {
        public static IServiceProvider? service { get; set; }
        public static T GetInstance<T>() => service.GetRequiredService<T>();
    }
}
