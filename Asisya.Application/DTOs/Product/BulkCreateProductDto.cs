using System.ComponentModel.DataAnnotations;

namespace Asisya.Application.DTOs.Product;

public class BulkCreateProductDto
{
    [Required]
    [Range(1, 500000)]
    public int Count { get; set; }

    [Required]
    public int CategoryID { get; set; }
}
