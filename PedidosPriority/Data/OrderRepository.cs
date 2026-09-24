using System.Data;
using Microsoft.Data.SqlClient;
using PedidosPriority.Models;

namespace PedidosPriority.Data;

public sealed class OrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public OrderRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PedidosDb")
            ?? throw new InvalidOperationException("Connection string 'PedidosDb' is not configured.");
    }

    public int Insert(Order order)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("dbo.sp_InsertOrder", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int) { Value = order.CustomerId });
        command.Parameters.Add(new SqlParameter("@Subtotal", SqlDbType.Decimal) { Value = order.Subtotal });
        command.Parameters.Add(new SqlParameter("@ShippingCost", SqlDbType.Decimal) { Value = order.ShippingCost });
        var orderId = new SqlParameter("@OrderId", SqlDbType.Int) { Direction = ParameterDirection.Output };
        command.Parameters.Add(orderId);

        connection.Open();
        command.ExecuteNonQuery();

        return (int)orderId.Value;
    }

    public IEnumerable<Order> GetPackingQueue()
    {
        var orders = new List<Order>();

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("dbo.sp_GetPackingQueue", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            orders.Add(new Order
            {
                OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                CustomerFullName = reader.GetString(reader.GetOrdinal("CustomerFullName")),
                CustomerPhone = reader.GetString(reader.GetOrdinal("CustomerPhone")),
                DeliveryAddress = reader.GetString(reader.GetOrdinal("DeliveryAddress")),
                Subtotal = reader.GetDecimal(reader.GetOrdinal("Subtotal")),
                ShippingCost = reader.GetDecimal(reader.GetOrdinal("ShippingCost")),
                Total = reader.GetDecimal(reader.GetOrdinal("Total")),
                PriorityLevel = reader.GetByte(reader.GetOrdinal("PriorityLevel")),
                PriorityName = reader.GetString(reader.GetOrdinal("PriorityName")),
                RegistrationDate = reader.GetDateTime(reader.GetOrdinal("RegistrationDate"))
            });
        }

        return orders;
    }
}