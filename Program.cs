using System;
using System.Threading.Tasks;
using Statiq.App;
using Statiq.Common;
using Statiq.Web;

namespace daveaglick
{
    public class Program
    {
        public static async Task<int> Main(string[] args) =>
            await Bootstrapper
                .Factory
                .CreateWeb(args)
                .DeployToNetlify("9bef82cf-1731-4cf0-ab3d-693ab0754810", Config.FromSetting<string>("NETLIFY_TOKEN"))
                .RunAsync();
    }
}
