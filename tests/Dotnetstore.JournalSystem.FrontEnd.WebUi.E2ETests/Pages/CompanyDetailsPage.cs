using Microsoft.Playwright;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Pages;

internal sealed class CompanyDetailsPage(IPage page)
{
    // The smallest valid PNG, so that the browser can really decode what it gets back.
    private static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");

    public ILocator Heading => page.GetByTestId("company-heading");

    public ILocator Message => page.GetByTestId("company-message");

    public ILocator Error => page.GetByTestId("company-error");

    public ILocator EditButton => page.GetByTestId("company-edit");

    public ILocator DeleteButton => page.GetByTestId("company-delete");

    public ILocator LogoImage => page.GetByTestId("logo-image");

    public ILocator LogoPlaceholder => page.GetByTestId("logo-placeholder");

    public ILocator LogoFile => page.GetByTestId("logo-file");

    public ILocator PhotoFile => page.GetByTestId("photo-file");

    public ILocator Photos => page.GetByTestId("photo");

    public ILocator AddressRows => page.GetByTestId("address-row");

    public ILocator Users => page.GetByTestId("users");

    public ILocator Audit => page.GetByTestId("audit");

    public ILocator CorporateNumber => page.GetByTestId("details-corporate-number");

    public ILocator Rows(string kind) => page.GetByTestId($"{kind}-row");

    public ILocator AddButton(string kind) => page.GetByTestId($"{kind}-add");

    public async Task SetCorporateNumberAsync(string value)
    {
        await EditButton.ClickAsync();
        var dialog = page.Locator("#company-edit-dialog");
        await dialog.GetByTestId("company-corporate-number").FillAsync(value);
        await dialog.GetByTestId("company-save").ClickAsync();
    }

    public async Task UploadLogoAsync()
    {
        await LogoFile.SetInputFilesAsync(Picture("logo.png"));
        await page.GetByTestId("logo-upload").ClickAsync();
    }

    public async Task UploadPhotoAsync(string fileName)
    {
        await PhotoFile.SetInputFilesAsync(Picture(fileName));
        await page.GetByTestId("photo-upload").ClickAsync();
    }

    public async Task RemoveLogoAsync() => await page.GetByTestId("logo-delete").ClickAsync();

    public async Task DeleteFirstPhotoAsync() => await Photos.First.GetByTestId("photo-delete").ClickAsync();

    public async Task AddAddressAsync(string street)
    {
        await AddButton("address").ClickAsync();
        var dialog = page.Locator("#address-new");
        await dialog.GetByTestId("address-street").FillAsync(street);
        await dialog.GetByTestId("address-postal-code").FillAsync("12345");
        await dialog.GetByTestId("address-city").FillAsync("Stockholm");
        await dialog.GetByTestId("address-country").FillAsync("Sweden");
        await dialog.GetByTestId("address-save").ClickAsync();
    }

    /// <param name="kind">email or phone.</param>
    public async Task AddValueAsync(string kind, string value)
    {
        await AddButton(kind).ClickAsync();
        var dialog = page.Locator($"#{kind}-new");
        await dialog.GetByTestId($"{kind}-value").FillAsync(value);
        await dialog.GetByTestId($"{kind}-save").ClickAsync();
    }

    public async Task DeleteFirstAsync(string kind) => await Rows(kind).First.GetByTestId($"{kind}-delete").ClickAsync();

    public void AcceptConfirmations() => page.Dialog += async (_, dialog) => await dialog.AcceptAsync();

    /// <summary>True once the browser has fetched and decoded the image.</summary>
    public static async Task WaitUntilShownAsync(ILocator image)
    {
        await image.ScrollIntoViewIfNeededAsync();
        await image.Page.WaitForFunctionAsync(
            "element => element.complete && element.naturalWidth > 0",
            await image.ElementHandleAsync());
    }

    private static FilePayload Picture(string name) => new() { Name = name, MimeType = "image/png", Buffer = Png };
}
