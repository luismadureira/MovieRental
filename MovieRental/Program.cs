using MovieRental.Customer;
using MovieRental.Data;
using MovieRental.Movie;
using MovieRental.PaymentProviders;
using MovieRental.Rental;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddEntityFrameworkSqlite().AddDbContext<MovieRentalDbContext>();

builder.Services.AddScoped<IMovieFeatures, MovieFeatures>();
builder.Services.AddScoped<IRentalFeatures, RentalFeatures>();
builder.Services.AddScoped<ICustomerFeatures, CustomerFeatures>();
builder.Services.AddScoped<PaymentProviderFactory>();
builder.Services.AddScoped<MbWayProvider>();
builder.Services.AddScoped<PayPalProvider>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

//Add Global Exception Handling BEFORE mapping controllers
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(GlobalExceptionHandler.HandleException);
});

app.MapControllers();

// Properly scope the DbContext for database initialization
using (IServiceScope scope = app.Services.CreateScope())
{
    MovieRentalDbContext context = scope.ServiceProvider.GetRequiredService<MovieRentalDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.Run();
