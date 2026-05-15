using System.ComponentModel.DataAnnotations;

namespace Asisya.Application.DTOs.Category;

public class CreateCategoryDto
{
    [Required]
    [MaxLength(50)]
    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? PictureBase64 { get; set; }
}
