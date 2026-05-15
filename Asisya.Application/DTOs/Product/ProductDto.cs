namespace Asisya.Application.DTOs.Product;

public class ProductDto
{
    public int ProductID { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int? CategoryID { get; set; }
    public string? CategoryName { get; set; }
    public decimal UnitPrice { get; set; }
    public short UnitsInStock { get; set; }
    public bool Discontinued { get; set; }
}
