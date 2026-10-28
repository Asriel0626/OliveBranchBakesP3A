namespace OliveBranchBakes.Models;

/// <summary>Order snapshot: the menu price is copied here so historical totals never change.</summary>
public sealed class OrderItem
{
    public string ProductId { get; set; } = "";
    public string ProductName { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;

    public OrderItem() { }
    public OrderItem(BakeryProduct product, int quantity)
    {
        if (quantity < 1) throw new ArgumentOutOfRangeException(nameof(quantity));
        ProductId = product.Id; ProductName = product.Name;
        UnitPrice = product.Price; Quantity = quantity;
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ProductId) || string.IsNullOrWhiteSpace(ProductName)
            || UnitPrice < 0 || Quantity < 1)
            throw new InvalidDataException("An order line has invalid product, quantity or price data.");
    }
}
