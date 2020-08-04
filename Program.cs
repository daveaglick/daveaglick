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
                .DeployToGitHubPagesBranch("daveaglick", "daveaglick", Config.FromSetting<string>("GITHUB_TOKEN"), "main")
                .RunAsync();
    }
}
