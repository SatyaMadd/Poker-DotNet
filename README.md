# Poker .NET Project

## Overview
This project is a real-time poker game application developed using .NET technologies. It leverages SignalR for seamless communication betIen the server and clients, ensuring a responsive and interactive user experience. The application is structured around a hub-based architecture, with a focus on modularity, scalability, and security.

## Key Components

### SignalR and Hub-Based Architecture
I utilize **SignalR** for real-time communication betIen the server and clients. SignalR uses IbSockets as the preferred transport protocol, providing a low-latency, persistent connection. If IbSockets are not supported, SignalR falls back to other compatible techniques such as Server-Sent Events or Long Polling.

my application is structured around **SignalR hubs** instead of traditional controllers. A hub is a high-level pipeline that allows the client and server to call methods on each other, making it easier to manage real-time communication and data exchange.

#### Server-Side Configuration
In my `Program.cs`, I configure SignalR by adding the necessary services and mapping the hubs:

```csharp
var builder = IbApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapHub<GameHub>("/gamehub");
app.MapHub<JoinHub>("/joinhub");

app.Run();
```

#### Hub Implementation
The `GameHub` class handles various game-related operations and interactions. It inherits from `Hub`, which provides the necessary methods and properties to manage connections, groups, and messaging.

```csharp
[Authorize]
public class GameHub : Hub
{
    private readonly IHubInteractionService _hubInteractionService;
    private readonly IGameService _gameService;
    private readonly ILobbyService _lobbyService;

    public GameHub(IHubInteractionService hubInteractionService, IGameService gameService, ILobbyService lobbyService)
    {
        _hubInteractionService = hubInteractionService;
        _gameService = gameService;
        _lobbyService = lobbyService;
    }

    public override async Task OnConnectedAsync()
    {
        var username = Context.User?.Identity?.Name;
        if (username == null){
            return;
        }
        var game = await _gameService.GetGameAsync(username);
        if (game == null){
            return;
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, game.Id.ToString());
        await Clients.Caller.SendAsync("Refresh");
        await base.OnConnectedAsync();
    }

    // Other methods like Check, Bet, Fold, ShowCards, MuckCards, Leave, Refresh, and Kick
}
```

#### Client-Side Configuration
On the client side, I establish connections to the SignalR hubs and handle events:

```javascript
const gameConnection = new signalR.HubConnectionBuilder()
    .withUrl("/gamehub", { accessTokenFactory: () => token })
    .build();
const joinConnection = new signalR.HubConnectionBuilder()
    .withUrl("/joinhub", { accessTokenFactory: () => token })
    .build();

gameConnection.start().catch(function (err) {
    return console.error(err.toString());
});
joinConnection.start().catch(function (err) {
    return console.error(err.toString());
});

gameConnection.on("Send", function (action) {
    console.log("Received Send event");
    if(action.actionName == "DeleteGame"){
        window.location.href = '/join';
    }
});

gameConnection.on("Refresh", function () {
    queue.push(true);
    processQueue();
});
```

### Service Layer
I encapsulate business logic within **services**. This separation of concerns ensures that my business logic is modular, reusable, and easily testable. For example, my `GameService` handles game-related operations:4

```csharp
public class GameService : IGameService
{
    public Game GetGameById(int id)
    {
        // Business logic to retrieve game by id
    }
}
```

### Repository Pattern
I implement the **Repository Pattern** to abstract data access logic. This pattern provides a clean separation betIen the data access layer and the business logic layer, promoting maintainability and scalability. Here's an example of a repository:

```csharp
public class GameRepository : IGameRepository
{
    private readonly AppDbContext _context;

    public GameRepository(AppDbContext context)
    {
        _context = context;
    }

    public Game GetGameById(int id)
    {
        return _context.Games.Find(id);
    }
}
```

### Dependency Injection
I manage dependencies using **Dependency Injection**. This approach enhances the modularity and testability of my code. Additionally, I use `ServiceFactory` to create a scoped service in one specific instance:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<Func<IGameService>>(provider => () => provider.GetService<IGameService>());
```

### JWT Authentication
I secure user authentication and authorization using **JWT (JSON Ib Token)**. This approach ensures that my application is secure and that user data is protected. Here's a snippet of my JWT configuration:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
```

### Entity Framework Core
I manage data access and ORM using **Entity Framework Core**. This poIrful framework allows us to interact with the database using .NET objects, simplifying data manipulation and access. Below is an example of my `DbContext` configuration:

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Game> Games { get; set; }
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

## Getting Started

### Prerequisites
- .NET 6.0 SDK
- SQL Server
- Node.js (for client-side development)

### Installation
1. Clone the repository:

```bash
git clone https://github.com/SatyaMadd/Poker-DotNet.git
```
2. Navigate to the project directory:

```bash
cd Poker-DotNet
```
3. Restore the .NET dependencies:

```bash
dotnet restore
```
4. Build the project:

```bash
dotnet build
```
5. Run the application:

```bash
dotnet run
```

### Running the Client
1. Navigate to the client directory:

```bash
cd client
```
2. Install the dependencies:

```bash
npm install
```

3. Start the client:

```bash
npm start
```

## Contributing
Contributions are welcome! Please open an issue or submit a pull request for any improvements or bug fixes.