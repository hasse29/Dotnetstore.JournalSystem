using Dotnetstore.JournalSystem.Shared.Clients.Contact;
using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Dotnetstore.JournalSystem.Shared.Clients;

public static class JournalApiClientsExtensions
{
    /// <summary>
    /// Registers a typed HttpClient for every API resource. Binds <see cref="JournalApiClientOptions"/> from the
    /// <c>JournalApi</c> section and validates it on start. Register your <see cref="IAccessTokenProvider"/> first
    /// to send a bearer token; without one, calls are anonymous.
    /// </summary>
    public static IServiceCollection AddJournalApiClients(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<JournalApiClientOptions>()
            .Bind(configuration.GetSection(JournalApiClientOptions.SectionName))
            .Validate(
                static options => options.BaseAddress is { IsAbsoluteUri: true },
                $"{JournalApiClientOptions.SectionName}:BaseAddress must be an absolute URI.")
            .ValidateOnStart();

        services.TryAddSingleton<IAccessTokenProvider, AnonymousAccessTokenProvider>();
        services.TryAddTransient<BearerTokenHandler>();

        AddClient<IAuthClient, AuthClient>(services);
        AddClient<IOwnCompanyClient, OwnCompanyClient>(services);
        AddClient<IRoleClient, RoleClient>(services);
        AddClient<IUserClient, UserClient>(services);
        AddClient<IWorkGroupClient, WorkGroupClient>(services);
        AddClient<IAddressTypeClient, AddressTypeClient>(services);
        AddClient<IPhoneNumberTypeClient, PhoneNumberTypeClient>(services);
        AddClient<IEmailAddressTypeClient, EmailAddressTypeClient>(services);
        AddClient<IAddressClient, AddressClient>(services);
        AddClient<IEmailAddressClient, EmailAddressClient>(services);
        AddClient<IPhoneNumberClient, PhoneNumberClient>(services);

        return services;
    }

    private static void AddClient<TClient, TImplementation>(IServiceCollection services)
        where TClient : class
        where TImplementation : class, TClient
    {
        services.AddHttpClient<TClient, TImplementation>()
            .ConfigureHttpClient(static (provider, client) =>
                client.BaseAddress = provider.GetRequiredService<IOptions<JournalApiClientOptions>>().Value.BaseAddress)
            .AddHttpMessageHandler<BearerTokenHandler>();
    }
}
