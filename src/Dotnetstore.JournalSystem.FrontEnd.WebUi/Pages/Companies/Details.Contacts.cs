using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Pages.Companies;

public sealed class AddressInput
{
    public Guid TypeId { get; set; }

    public string? CO { get; set; }

    public string? Street1 { get; set; }

    public string? Street2 { get; set; }

    public string? PostalCode { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Country { get; set; }
}

public sealed class ContactValueInput
{
    public Guid TypeId { get; set; }

    public string? Value { get; set; }
}

public sealed partial class DetailsModel
{
    // A null list means that it could not be loaded.
    public IReadOnlyList<AddressResponse>? Addresses { get; private set; }

    public IReadOnlyList<EmailAddressResponse>? EmailAddresses { get; private set; }

    public IReadOnlyList<PhoneNumberResponse>? PhoneNumbers { get; private set; }

    public IReadOnlyList<ContactType> AddressTypes { get; private set; } = [];

    public IReadOnlyList<ContactType> EmailAddressTypes { get; private set; } = [];

    public IReadOnlyList<ContactType> PhoneNumberTypes { get; private set; } = [];

    public async Task<IActionResult> OnPostSaveAddressAsync(
        Guid id,
        Guid? contactId,
        AddressInput address,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(address);

        if (address.TypeId == Guid.Empty ||
            string.IsNullOrWhiteSpace(address.Street1) ||
            string.IsNullOrWhiteSpace(address.PostalCode) ||
            string.IsNullOrWhiteSpace(address.City) ||
            string.IsNullOrWhiteSpace(address.Country))
        {
            return Refuse(id, "An address needs a type, street, postal code, city and country.");
        }

        var request = new EditAddressRequest(
            address.TypeId,
            id,
            CompanyForm.NullIfBlank(address.CO),
            address.Street1.Trim(),
            CompanyForm.NullIfBlank(address.Street2),
            address.PostalCode.Trim(),
            address.City.Trim(),
            address.State?.Trim() ?? string.Empty,
            address.Country.Trim());

        return await DecideAsync(
                id,
                async () => contactId is { } existing
                    ? await addresses.UpdateAsync(existing, request, cancellationToken).ConfigureAwait(false)
                    : await addresses.CreateAsync(request, cancellationToken).ConfigureAwait(false),
                "Address saved.")
            .ConfigureAwait(false);
    }

    public async Task<IActionResult> OnPostDeleteAddressAsync(Guid id, Guid contactId, CancellationToken cancellationToken) =>
        await DecideAsync(id, () => addresses.DeleteAsync(contactId, cancellationToken), "Address deleted.")
            .ConfigureAwait(false);

    public async Task<IActionResult> OnPostSaveEmailAsync(
        Guid id,
        Guid? contactId,
        ContactValueInput email,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(email);

        if (email.TypeId == Guid.Empty || string.IsNullOrWhiteSpace(email.Value))
        {
            return Refuse(id, "An email address needs a type and an address.");
        }

        var request = new EditEmailAddressRequest(email.TypeId, id, email.Value.Trim());

        return await DecideAsync(
                id,
                async () => contactId is { } existing
                    ? await emailAddresses.UpdateAsync(existing, request, cancellationToken).ConfigureAwait(false)
                    : await emailAddresses.CreateAsync(request, cancellationToken).ConfigureAwait(false),
                "Email address saved.")
            .ConfigureAwait(false);
    }

    public async Task<IActionResult> OnPostDeleteEmailAsync(Guid id, Guid contactId, CancellationToken cancellationToken) =>
        await DecideAsync(id, () => emailAddresses.DeleteAsync(contactId, cancellationToken), "Email address deleted.")
            .ConfigureAwait(false);

    public async Task<IActionResult> OnPostSavePhoneAsync(
        Guid id,
        Guid? contactId,
        ContactValueInput phone,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(phone);

        if (phone.TypeId == Guid.Empty || string.IsNullOrWhiteSpace(phone.Value))
        {
            return Refuse(id, "A phone number needs a type and a number.");
        }

        var request = new EditPhoneNumberRequest(phone.TypeId, id, phone.Value.Trim());

        return await DecideAsync(
                id,
                async () => contactId is { } existing
                    ? await phoneNumbers.UpdateAsync(existing, request, cancellationToken).ConfigureAwait(false)
                    : await phoneNumbers.CreateAsync(request, cancellationToken).ConfigureAwait(false),
                "Phone number saved.")
            .ConfigureAwait(false);
    }

    public async Task<IActionResult> OnPostDeletePhoneAsync(Guid id, Guid contactId, CancellationToken cancellationToken) =>
        await DecideAsync(id, () => phoneNumbers.DeleteAsync(contactId, cancellationToken), "Phone number deleted.")
            .ConfigureAwait(false);

    private async Task LoadContactsAsync(Guid id, CancellationToken cancellationToken)
    {
        var addressTypeList = await LoadListAsync(() => addressTypes.GetAllAsync(cancellationToken)).ConfigureAwait(false);
        var emailTypeList = await LoadListAsync(() => emailAddressTypes.GetAllAsync(cancellationToken)).ConfigureAwait(false);
        var phoneTypeList = await LoadListAsync(() => phoneNumberTypes.GetAllAsync(cancellationToken)).ConfigureAwait(false);

        AddressTypes = ToContactTypes(addressTypeList?.Select(x => new ContactType(x.Id, x.Name)));
        EmailAddressTypes = ToContactTypes(emailTypeList?.Select(x => new ContactType(x.Id, x.Name)));
        PhoneNumberTypes = ToContactTypes(phoneTypeList?.Select(x => new ContactType(x.Id, x.Name)));

        Addresses = await LoadListAsync(() => addresses.GetByObjectIdAsync(id, cancellationToken)).ConfigureAwait(false);
        EmailAddresses = await LoadListAsync(() => emailAddresses.GetByObjectIdAsync(id, cancellationToken)).ConfigureAwait(false);
        PhoneNumbers = await LoadListAsync(() => phoneNumbers.GetByObjectIdAsync(id, cancellationToken)).ConfigureAwait(false);
    }

    private static List<ContactType> ToContactTypes(IEnumerable<ContactType>? types) =>
        types?.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList() ?? [];
}

public sealed record ContactType(Guid Id, string Name);
