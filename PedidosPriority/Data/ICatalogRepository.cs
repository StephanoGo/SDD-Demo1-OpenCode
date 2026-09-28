using PedidosPriority.Models;

namespace PedidosPriority.Data;

public interface ICatalogRepository
{
    IEnumerable<ActiveCustomerDto> GetActiveCustomers();
    IEnumerable<CategoryDto> GetCategories();
    IEnumerable<ProductDto> GetProductsByCategory(int? categoryId);
}