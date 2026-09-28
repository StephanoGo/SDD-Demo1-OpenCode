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

    public int InsertWithDetails(Order order, IEnumerable<OrderDetailRequest> items)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();
        int orderId;

        try
        {
            using (var headerCommand = new SqlCommand("dbo.sp_InsertOrder", connection, transaction)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                headerCommand.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int) { Value = order.CustomerId });
                headerCommand.Parameters.Add(new SqlParameter("@Subtotal", SqlDbType.Decimal) { Value = order.Subtotal });
                headerCommand.Parameters.Add(new SqlParameter("@ShippingCost", SqlDbType.Decimal) { Value = order.ShippingCost });
                var orderIdParam = new SqlParameter("@OrderId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                headerCommand.Parameters.Add(orderIdParam);

                headerCommand.ExecuteNonQuery();
                orderId = (int)orderIdParam.Value;
            }

            foreach (var item in items)
            {
                using var detailCommand = new SqlCommand("dbo.sp_InsertOrderDetail", connection, transaction)
                {
                    CommandType = CommandType.StoredProcedure
                };

                detailCommand.Parameters.Add(new SqlParameter("@OrderId", SqlDbType.Int) { Value = orderId });
                detailCommand.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = item.ProductId });
                detailCommand.Parameters.Add(new SqlParameter("@Quantity", SqlDbType.Int) { Value = item.Quantity });
                detailCommand.Parameters.Add(new SqlParameter("@UnitPrice", SqlDbType.Decimal) { Value = item.UnitPrice });
                var detailIdParam = new SqlParameter("@OrderDetailId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                detailCommand.Parameters.Add(detailIdParam);

                detailCommand.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        return orderId;
    }

    public OrderDetailsViewModel GetOrderDetails(int orderId)
    {
        var details = new OrderDetailsViewModel();

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("dbo.sp_GetOrderDetailsByOrderId", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@OrderId", SqlDbType.Int) { Value = orderId });

        connection.Open();
        using var reader = command.ExecuteReader();

        if (reader.Read())
        {
            details.OrderId = ReadInt(reader, "OrderId") ?? 0;
            details.CustomerId = ReadInt(reader, "CustomerId") ?? 0;
            details.CustomerFullName = ReadString(reader, "FullName", "CustomerFullName");
            details.CustomerPhone = ReadString(reader, "Phone", "CustomerPhone");
            details.DeliveryAddress = ReadString(reader, "DeliveryAddress");
            details.Email = ReadString(reader, "Email");
            details.RegistrationDate = ReadDateTime(reader, "RegistrationDate") ?? DateTime.MinValue;
            details.PriorityLevel = ReadByte(reader, "PriorityLevel", "PriorityLevelId") ?? 0;
            details.PriorityName = ReadString(reader, "PriorityName");
            details.Subtotal = ReadDecimal(reader, "Subtotal") ?? 0m;
            details.ShippingCost = ReadDecimal(reader, "ShippingCost") ?? 0m;
            details.Total = ReadDecimal(reader, "Total") ?? 0m;
            details.HasHeader = true;
        }

        if (reader.NextResult())
        {
            while (reader.Read())
            {
                var line = new OrderDetailItemDto
                {
                    ProductName = ReadString(reader, "ProductName"),
                    Quantity = ReadInt(reader, "Quantity") ?? 0,
                    UnitPrice = ReadDecimal(reader, "UnitPrice") ?? 0m,
                    LineTotal = ReadDecimal(reader, "LineTotal") ?? 0m
                };

                details.Lines.Add(line);
            }
        }

        return details;
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

    private static int GetColumnIndex(IDataRecord reader, params string[] candidates)
    {
        for (var i = 0; i < reader.FieldCount; i++)
        {
            var name = reader.GetName(i);

            foreach (var candidate in candidates)
            {
                if (string.Equals(name, candidate, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private static int? ReadInt(IDataRecord reader, params string[] candidates)
    {
        var index = GetColumnIndex(reader, candidates);
        return index >= 0 && !reader.IsDBNull(index) ? reader.GetInt32(index) : null;
    }

    private static byte? ReadByte(IDataRecord reader, params string[] candidates)
    {
        var index = GetColumnIndex(reader, candidates);
        return index >= 0 && !reader.IsDBNull(index) ? (byte)reader.GetValue(index) : null;
    }

    private static string? ReadString(IDataRecord reader, params string[] candidates)
    {
        var index = GetColumnIndex(reader, candidates);
        return index >= 0 && !reader.IsDBNull(index) ? reader.GetString(index) : null;
    }

    private static decimal? ReadDecimal(IDataRecord reader, params string[] candidates)
    {
        var index = GetColumnIndex(reader, candidates);
        return index >= 0 && !reader.IsDBNull(index) ? reader.GetDecimal(index) : null;
    }

    private static DateTime? ReadDateTime(IDataRecord reader, params string[] candidates)
    {
        var index = GetColumnIndex(reader, candidates);
        return index >= 0 && !reader.IsDBNull(index) ? reader.GetDateTime(index) : null;
    }
}