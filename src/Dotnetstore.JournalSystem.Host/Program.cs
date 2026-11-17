using Dotnetstore.JournalSystem.Host.Extensions;

var builder = WebApplication.CreateBuilder(args);

await builder
    .RegisterServices()
    .Build()
    .RegisterPipeline()
    .RunApplicationAsync()
    .ConfigureAwait(false);

namespace Dotnetstore.JournalSystem.Host
{
    internal sealed partial class Program;
}
