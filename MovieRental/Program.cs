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



//problem: The app is throwing an error when we start, please help us.
//      Also, tell us what caused the issue.
//solution: You are registering RentalFeatures as a singleton, but its constructor requires 
//      a MovieRentalDbContext, which is registered as a scoped service which means a new
//      instance of MovieRentalDbContext is created per HTTP request(in web apps).
//      A singleton would hold onto a single instance of the scoped service, breaking the 
//      intended lifetime management.

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

app.MapControllers();

//Add Global Exception Handling
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(GlobalExceptionHandler.HandleException);
});

using (MovieRentalDbContext client = new MovieRentalDbContext())
{
    client.Database.EnsureCreated();
}

app.Run();
