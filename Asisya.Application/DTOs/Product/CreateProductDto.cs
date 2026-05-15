using System.ComponentModel.DataAnnotations;

namespace Asisya.Application.DTOs.Product;

public class CreateProductDto
{
    [Required]
    [MaxLength(100)]
    public string ProductName { get; set; } = string.Empty;

    public int? SupplierID { get; set; }
    public int? CategoryID { get; set; }
    public string? QuantityPerUnit { get; set; }

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    public short UnitsInStock { get; set; }
    public short UnitsOnOrder { get; set; }
    public short ReorderLevel { get; set; }
    public bool Discontinued { get; set; }
}
