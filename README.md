# MovieRental

This repository contains a modern movie rental system developed as an exercise project. It simulates a comprehensive movie rental service with customer management, movie catalog, and rental tracking features, implemented using C# with both Web API and WPF interfaces.

---

## 🧩 Project Overview

MovieRental is a full-stack application that allows users to:

- **Browse and manage** a catalog of movies
- **Handle customer** registration and information management
- **Process movie rentals** and returns with payment integration
- **Track rental history** and customer activity
- **RESTful API** endpoints for programmatic access
- **Desktop application** interface for direct user interaction

The system supports multiple payment providers (MbWay and PayPal) and uses Entity Framework Core with SQLite for data persistence.

---

## 🛠️ Technologies Used

- **Framework:** .NET 8.0
- **Backend:** ASP.NET Core Web API
- **Frontend:** Windows Presentation Foundation (WPF)
- **Database:** SQLite with Entity Framework Core
- **Testing:** xUnit with Moq for mocking
- **API Documentation:** Swagger/OpenAPI
- **Payment Integration:** Multiple payment providers (MbWay, PayPal)
- **IDE:** Visual Studio  

---

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or later
- SQLite (automatically managed by Entity Framework Core)

### Clone the repository

```bash
git clone https://github.com/luismadureira/MovieRental.git
cd MovieRental
```

### Build and run the Web API

1. Open `MovieRental.sln` in Visual Studio
2. Restore NuGet packages (automatically done on build)
3. Set `MovieRental` as the startup project
4. Build the solution (`Ctrl + Shift + B`)
5. Run the API (`F5` or `Ctrl + F5`)

The API will be available at:
- **HTTPS:** `https://localhost:7xxx`
- **HTTP:** `http://localhost:5xxx`
- **Swagger UI:** `https://localhost:7xxx/swagger`

### Run the WPF Application

1. Set `MovieRental.WPF` as the startup project
2. Build and run (`F5`)

### API Endpoints

The Web API provides the following endpoints:
- **Movies:** `/api/movie` - CRUD operations for movie management
- **Customers:** `/api/customer` - Customer registration and management
- **Rentals:** `/api/rental` - Rental processing and tracking

---

## 🧪 Running Tests

### Using Visual Studio
- Open Test Explorer (`Test` → `Test Explorer`)
- Build the solution to discover tests
- Click "Run All Tests" to execute all unit tests

### Using Command Line
```bash
dotnet test MovieRental.Test.Unit/MovieRental.Test.Unit.csproj
```

### Test Coverage
The project includes comprehensive unit tests for:
- **Controllers:** API endpoint testing with mocked dependencies
- **Business Logic:** Core rental, customer, and movie operations
- **Payment Providers:** Payment processing functionality

---

## 📋 Development Notes & Future Improvements

### Current Status
- ✅ Web API with Swagger documentation
- ✅ Entity Framework Core with SQLite
- ✅ Multiple payment provider support
- ✅ Global exception handling
- ✅ Comprehensive unit testing
- ✅ WPF desktop interface

### Planned Improvements
- [ ] Fix any remaining startup errors
- [ ] Convert rental `Save` method to asynchronous operations
- [ ] Implement advanced filtering (rentals by customer name, date ranges)
- [ ] Add authentication and authorization
- [ ] Implement caching for better performance
- [ ] Add logging and monitoring
- [ ] Create API rate limiting
- [ ] Enhance WPF UI with modern design patterns

### Technical Debt
- Consider implementing CQRS pattern for complex operations
- Add integration tests for API endpoints
- Implement database migrations for production scenarios

---

## 📂 Project Structure

```
MovieRental.sln                 # Solution file
├── MovieRental/                # Web API Project (.NET 8)
│   ├── Controllers/            # API Controllers (Movie, Customer, Rental)
│   ├── Customer/               # Customer domain logic and interfaces
│   ├── Data/                   # Entity Framework DbContext
│   ├── Middleware/             # Global exception handling
│   ├── Movie/                  # Movie domain logic and interfaces
│   ├── PaymentProviders/       # Payment integration (MbWay, PayPal)
│   ├── Rental/                 # Rental domain logic and interfaces
│   └── Program.cs              # Application entry point and DI configuration
├── MovieRental.WPF/            # WPF Desktop Application
│   ├── Models/                 # Data models for UI
│   ├── Services/               # Service layer for API communication
│   ├── ViewModels/             # MVVM ViewModels
│   └── Views/                  # WPF Views and Windows
└── MovieRental.Test.Unit/      # Unit Tests
    └── *ControllerTests.cs     # Controller unit tests with Moq
```

### Key Features
- **Domain-Driven Design:** Organized by business domains (Movie, Customer, Rental)
- **Dependency Injection:** Configured in Program.cs with scoped lifetimes
- **Global Exception Handling:** Centralized error management
- **Payment Provider Factory:** Extensible payment system
- **Entity Framework Core:** Code-first database approach with SQLite
- **MVVM Pattern:** Clean separation in WPF application

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 📞 Contact & Support

Feel free to:
- 🐛 **Report bugs** by opening an issue
- 💡 **Suggest features** through feature requests
- 🤝 **Contribute** by submitting pull requests
- ⭐ **Star** the project if you find it useful

---

*Developed with ❤️ by Luis Madureira*
