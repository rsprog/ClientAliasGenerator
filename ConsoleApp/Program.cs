using System;
using System.Linq;
using System.Threading.Tasks;
using TenantAliasGenerator.Services;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            WikipediaFilmService service = new WikipediaFilmService();
            var infos = await service.GetFilmsThatStartWithLetterAsync(args[0][0]);
            foreach (var info in infos.Where(s => !s.Title.Contains(' ')))
                Console.WriteLine($"{info.Title} ({info.Year})");
        }
    }
}
