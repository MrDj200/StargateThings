using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using MuddyStargateWasm.Services;

namespace MuddyStargateWasm
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Logging.SetMinimumLevel(LogLevel.Information);

#if DEBUG
            builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif


            builder.Services.AddMudServices();
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddScoped<AppBarState>();
            builder.Services.AddScoped<StargateConnection>();
            builder.Services.AddSingleton<WebSocketService>();

            await builder.Build().RunAsync();
        }
    }
}
