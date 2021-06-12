using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EditorConfig;

namespace EditorConfigGenerator
{
    internal static class Program
    {
        internal static async Task Main()
        {
            await CopySchemaAsync();

            await GenerateEditorConfigAsync(".editorconfig");
        }

        private static async Task CopySchemaAsync()
        {
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync("https://raw.githubusercontent.com/madskristensen/EditorConfigLanguage/master/src/Schema/EditorConfig.json");

            // This is where the EditorConfig project expects to find this file
            var directory = Directory.CreateDirectory("Schema");
            using var file = File.Create($"{directory.Name}/EditorConfig.json");
            await response.Content.CopyToAsync(file);
        }

        private static async Task GenerateEditorConfigAsync(string path)
        {
            using var writer = new StreamWriter(path);
            await GenerateContentsAsync(writer);
        }

        private static async Task GenerateContentsAsync(StreamWriter writer)
        {
            await writer.WriteLineAsync("is_global = true");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("global_level = -99");

            foreach (var group in GetKeywords().GroupBy(k => k.Category))
            {
                await writer.WriteLineAsync();

                foreach (var keyword in group)
                {
                    await writer.WriteLineAsync(GetRule(keyword));
                }
            }
        }

        private static string GetRule(Keyword keyword)
        {
            string defaultValue = string.Join(',', keyword.DefaultValue.Select(v => v.Name));
            string severity = (keyword.RequiresSeverity ? $": {keyword.DefaultSeverity}" : string.Empty);

            return $"{keyword.Name} = {defaultValue} {severity}";
        }

        private static IEnumerable<Keyword> GetKeywords()
        {
            foreach (var keyword in SchemaCatalog.VisibleKeywords.Where(k => k.IsSupported))
            {
                if (keyword.Category == Category.Standard)
                    continue;

                if (keyword.Name.Contains('<'))
                    continue;

                if (keyword.Name == "dotnet_remove_unnecessary_suppression_exclusions")
                    continue;

                if (keyword.DefaultValue.First().Name == "unset")
                    continue;

                yield return keyword;
            }
        }
    }
}
