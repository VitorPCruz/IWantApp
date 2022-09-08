namespace IWantApp.Endpoints.Products;

public record ProductResponse(
    Guid Id,
    string Name,
    string CategoryName,
    string Description,
    decimal Price,
    bool HasStock,
    bool Active);