public class ProductQueryParameters
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Name { get; set; }

    public string? SKU { get; set; }

    public string? SortBy { get; set; }

    public bool Descending { get; set; }
}