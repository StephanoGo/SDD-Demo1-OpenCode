using PedidosPriority.Models;

namespace PedidosPriority.Services;

public interface ICatalogService
{
    IEnumerable<ActiveCustomerDto> GetActiveCustomers();
    IEnumerable<CategoryDto> GetCategories();
    IEnumerable<ProductDto> GetProductsByCategory(int? categoryId);
}