using System;
using System.Threading.Tasks;
using CommandLine;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcService.Protos;

namespace GrpcClient
{
    class Program
    {
        public class Options
        {
            [Option('c', "client", Required = true, HelpText = "Client name")]
            public string ClientName { get; set; }

            [Option('m', "mode", Required = false, Default = Mode.SingleWord, HelpText = "Word count mode")]
            public Mode Mode { get; set; }

            [Option('l', "list", Required = false, Default = false, HelpText = "List films")]
            public bool List { get; set; }

            [Option('s', "stream", Required = false, Default = false, HelpText = "Stream aliases")]
            public bool Stream { get; set; }
        }

        static async Task Main(string[] args)
        {
            Options selectedOptions = new Options();
            Parser.Default.ParseArguments<Options>(args).WithParsed(o => { selectedOptions = o; });

            using var channel = GrpcChannel.ForAddress(" https://localhost:5001");
            var client = new AliasGenerator.AliasGeneratorClient(channel);
            var aliasRequest = new AliasRequest { TenantName = selectedOptions.ClientName, Mode = selectedOptions.Mode };

            if (selectedOptions.List)
            {
                var candidateCollection = await client.ListFilmCandidatesAsync(aliasRequest);
                Console.WriteLine($"Timestamp: {candidateCollection.Timestamp}, Title count: {candidateCollection.Count}");
                foreach (var film in candidateCollection.Films)
                    Console.WriteLine($"{film.Title} ({film.Year})");
            }
            else if (selectedOptions.Stream)
            {
                using var streamingCall = client.GetAliasStream(aliasRequest);
                await foreach (var tenant in streamingCall.ResponseStream.ReadAllAsync())
                    Console.WriteLine(tenant.Alias);
            }
            else
            {
                var tenant = await client.GenerateRandomAliasAsync(aliasRequest);
                Console.WriteLine(tenant.Alias);
            }
        }
    }
}
