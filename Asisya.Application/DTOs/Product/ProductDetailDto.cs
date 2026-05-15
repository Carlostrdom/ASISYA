namespace Asisya.Application.DTOs.Product;

public class ProductDetailDto
{
    public int ProductID { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int? CategoryID { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryPictureBase64 { get; set; }
    public int? SupplierID { get; set; }
    public string? SupplierName { get; set; }
    public string? QuantityPerUnit { get; set; }
    public decimal UnitPrice { get; set; }
    public short UnitsInStock { get; set; }
    public short UnitsOnOrder { get; set; }
    public short ReorderLevel { get; set; }
    public bool Discontinued { get; set; }
}
