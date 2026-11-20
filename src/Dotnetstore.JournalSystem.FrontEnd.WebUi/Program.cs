using Dotnetstore.JournalSystem.FrontEnd.WebUi.Extensions;

var builder = WebApplication.CreateBuilder(args);

await builder
    .RegisterServices()
    .Build()
    .RegisterMiddlewares()
    .RunApplicationAsync()
    .ConfigureAwait(false);

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi
{
    internal sealed partial class Program;
}
