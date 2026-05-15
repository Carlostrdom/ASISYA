namespace Asisya.Application.DTOs.Category;

public class CategoryDto
{
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PictureBase64 { get; set; }
}
