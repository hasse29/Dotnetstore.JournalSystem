using System.ComponentModel.DataAnnotations;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Pages.Account;

public sealed class LoginInput
{
    [Required(ErrorMessage = "Enter your username.")]
    [Display(Name = "Username")]
    [StringLength(256)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter your password.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    [StringLength(128)]
    public string Password { get; set; } = string.Empty;
}
