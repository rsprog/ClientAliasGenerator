using System;
using System.Threading.Tasks;

namespace TenantAliasGenerator.Services
{
    public interface IFilmService
    {
        Task<FilmInfo[]> GetFilmsThatStartWithLetterAsync(char letter);
    }
}
