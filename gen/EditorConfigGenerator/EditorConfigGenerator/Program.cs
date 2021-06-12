using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace EditorConfigGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await CopySchema();
        }

        private static async Task CopySchema()
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://raw.githubusercontent.com/madskristensen/EditorConfigLanguage/master/src/Schema/EditorConfig.json");

            var directory = Directory.CreateDirectory("Schema");
            using var file = File.Create($"{directory.Name}/EditorConfig.json");
            await response.Content.CopyToAsync(file);
        }
    }
}
