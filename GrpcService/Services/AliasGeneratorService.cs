using Grpc.Core;
using GrpcService.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenantAliasGenerator;
using TenantAliasGenerator.Services;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.DependencyInjection;

namespace GrpcService.Services
{
    public class AliasGeneratorService : AliasGenerator.AliasGeneratorBase
    {
        private readonly IFilmService filmService;
        private readonly Random random = new Random();

        [ActivatorUtilitiesConstructor]
        public AliasGeneratorService(IFilmService filmService)
        {
            this.filmService = filmService;
        }

        public AliasGeneratorService(IFilmService filmService, Random random) : this(filmService)
        {
            this.random = random;
        }

        public async override Task<CandidateCollection> ListFilmCandidates(AliasRequest request, ServerCallContext context)
        {
            var filmInfos = await filmService.GetFilmsThatStartWithLetterAsync(request.TenantName[0]);
            var collection = new CandidateCollection { Timestamp = Timestamp.FromDateTime(DateTime.UtcNow) };
            collection.Films.AddRange(Filter(filmInfos, request).Select(s => new Film { Title = s.Title, Year = s.Year }));
            collection.Count = collection.Films.Count;

            return collection;
        }

        public async override Task<Tenant> GenerateRandomAlias(AliasRequest request, ServerCallContext context)
        {
            var filmInfos = Filter(await filmService.GetFilmsThatStartWithLetterAsync(request.TenantName[0]), request).ToArray();
            return new Tenant { 
                Name = request.TenantName,
                Alias = filmInfos.Length == 0 ? "" : filmInfos.ElementAt(random.Next(0, filmInfos.Length)).Title
            };
        }

        public async override Task GetAliasStream(AliasRequest request, IServerStreamWriter<Tenant> responseStream, ServerCallContext context)
        {
            var filmInfos = Filter(await filmService.GetFilmsThatStartWithLetterAsync(request.TenantName[0]), request).ToList();
            while (filmInfos.Count > 0 && !context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000); // taking our time
                var film = filmInfos.ElementAt(random.Next(0, filmInfos.Count));

                await responseStream.WriteAsync(new Tenant
                {
                    Name = request.TenantName,
                    Alias = film.Title
                });

                filmInfos.Remove(film);
            }
        }

        private static IEnumerable<FilmInfo> Filter(IEnumerable<FilmInfo> filmInfos, AliasRequest request)
        {
            return request.Mode switch
            {
                Mode.SingleWord => filmInfos.Where(s => !s.Title.Trim().Contains(' ')),
                Mode.MatchWordCount => filmInfos.Where(s => s.Title.Trim().Count(x => x == ' ') == request.TenantName.Trim().Count(x => x == ' ')),
                _ => filmInfos,
            };
        }
    }
}
