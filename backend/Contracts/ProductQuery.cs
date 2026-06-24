// Application layer — product query object
namespace ProductApi.Contracts;

// Kept separate from PageQuery to house product-specific filters (e.g. filter.category) when added
public class ProductQuery : PageQuery { }
