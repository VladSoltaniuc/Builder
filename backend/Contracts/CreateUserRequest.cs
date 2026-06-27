// Application layer
using System.ComponentModel.DataAnnotations;
using ProductApi.Models;

namespace ProductApi.Contracts;

public class CreateUserRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; } = string.Empty;

    // E.164 phone, e.g. +14155552671. Required when ReportChannel is Sms.
    [Phone(ErrorMessage = "Invalid phone number.")]
    public string? PhoneNumber { get; set; }

    public PreferredReportChannel ReportChannel { get; set; }
}
