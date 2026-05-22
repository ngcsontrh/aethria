namespace Aethria.Api.Endpoints.Mentors;

/// <summary>
/// Mentor update request.
/// </summary>
public sealed record UpdateMentorRequest
{
    /// <summary>
    /// Updated mentor display name.
    /// </summary>
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
    public string Name { get; init; } = null!;

    /// <summary>
    /// Updated mentor description.
    /// </summary>
    [Required(ErrorMessage = "Description is required.")]
    [StringLength(1000, MinimumLength = 1, ErrorMessage = "Description must be between 1 and 1000 characters.")]
    public string Description { get; init; } = null!;

    /// <summary>
    /// Updated system instruction that guides mentor behavior.
    /// </summary>
    [Required(ErrorMessage = "Instruction is required.")]
    [StringLength(4000, MinimumLength = 1, ErrorMessage = "Instruction must be between 1 and 4000 characters.")]
    public string Instruction { get; init; } = null!;

    /// <summary>
    /// Updated tool identifiers available to the mentor.
    /// </summary>
    [MaxLength(20, ErrorMessage = "Tools must contain 20 items or fewer.")]
    public List<string> Tools { get; init; } = [];
}
