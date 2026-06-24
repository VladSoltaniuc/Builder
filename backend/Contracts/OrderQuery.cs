// Application layer — order query object
namespace ProductApi.Contracts;

// Kept separate from PageQuery to house order-specific filters (e.g. filter.status, filter.userId) when added
public class OrderQuery : PageQuery { }
