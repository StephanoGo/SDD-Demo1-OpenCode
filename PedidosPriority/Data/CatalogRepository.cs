using System.Data;
using Microsoft.Data.SqlClient;
using PedidosPriority.Models;

namespace PedidosPriority.Data;

public sealed class CatalogRepository : ICatalogRepository
{
    private readonly string _connectionString;

    public CatalogRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PedidosDb")
            ?? throw new InvalidOperationException("Connection string 'PedidosDb' is not configured.");
    }

    public IEnumerable<ActiveCustomerDto> GetActiveCustomers()
    {
        var customers = new List<ActiveCustomerDto>();

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("dbo.sp_GetActiveCustomers", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            customers.Add(new ActiveCustomerDto
            {
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                DeliveryAddress = reader.GetString(reader.GetOrdinal("DeliveryAddress")),
                Email = reader.GetString(reader.GetOrdinal("Email"))
            });
        }

        return customers;
    }

    public IEnumerable<CategoryDto> GetCategories()
    {
        var categories = new List<CategoryDto>();

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("dbo.sp_GetCategories", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            categories.Add(new CategoryDto
            {
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Description"))
            });
        }

        return categories;
    }

    public IEnumerable<ProductDto> GetProductsByCategory(int? categoryId)
    {
        var products = new List<ProductDto>();

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("dbo.sp_GetProductsByCategory", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int)
        {
            Value = categoryId.HasValue ? categoryId.Value : DBNull.Value
        });

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            products.Add(new ProductDto
            {
                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                Stock = reader.GetInt32(reader.GetOrdinal("Stock"))
            });
        }

        return products;
    }
}