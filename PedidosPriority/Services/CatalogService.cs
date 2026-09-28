using PedidosPriority.Data;
using PedidosPriority.Models;

namespace PedidosPriority.Services;

public sealed class CatalogService : ICatalogService
{
    private readonly ICatalogRepository _repository;

    public CatalogService(ICatalogRepository repository)
    {
        _repository = repository;
    }

    public IEnumerable<ActiveCustomerDto> GetActiveCustomers()
    {
        return _repository.GetActiveCustomers();
    }

    public IEnumerable<CategoryDto> GetCategories()
    {
        return _repository.GetCategories();
    }

    public IEnumerable<ProductDto> GetProductsByCategory(int? categoryId)
    {
        return _repository.GetProductsByCategory(categoryId);
    }
}