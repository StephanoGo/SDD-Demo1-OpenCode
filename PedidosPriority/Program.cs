var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<PedidosPriority.Data.IOrderRepository, PedidosPriority.Data.OrderRepository>();
builder.Services.AddScoped<PedidosPriority.Services.IOrderService, PedidosPriority.Services.OrderService>();
builder.Services.AddScoped<PedidosPriority.Data.ICatalogRepository, PedidosPriority.Data.CatalogRepository>();
builder.Services.AddScoped<PedidosPriority.Services.ICatalogService, PedidosPriority.Services.CatalogService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pedidos}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
