using Dotnetstore.JournalSystem.Host.Extensions;

var builder = WebApplication.CreateBuilder(args);

await builder
    .RegisterServices()
    .Build()
    .RegisterPipeline()
    .RunApplicationAsync()
    .ConfigureAwait(false);

internal sealed partial class Program;
