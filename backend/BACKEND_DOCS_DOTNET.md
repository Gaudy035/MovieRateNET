# MovieRate — Dokumentacja Backendu (.NET)

Dokumentacja techniczna REST API aplikacji **MovieRate**. Opisuje warstwę serwerową zbudowaną w **ASP.NET Core 8 + Entity Framework Core + PostgreSQL 16** na tyle szczegółowo, aby można było zbudować backend w pełni kompatybilny z istniejącym frontendem Next.js — bez żadnych zmian po stronie klienta.

---

## Spis treści

1. [Przegląd systemu](#1-przegląd-systemu)
2. [Stos technologiczny i uruchomienie](#2-stos-technologiczny-i-uruchomienie)
3. [Architektura backendu](#3-architektura-backendu)
4. [Baza danych PostgreSQL](#4-baza-danych-postgresql)
5. [Autentykacja i sesje](#5-autentykacja-i-sesje)
6. [Konwencje API](#6-konwencje-api)
7. [Endpointy — pełna specyfikacja](#7-endpointy--pełna-specyfikacja)
8. [Warstwa serwisów — logika biznesowa](#8-warstwa-serwisów--logika-biznesowa)
9. [Integracja z frontendem](#9-integracja-z-frontendem)
10. [Reguły biznesowe — checklista implementacji](#10-reguły-biznesowe--checklista-implementacji)
11. [Struktura projektu .NET](#11-struktura-projektu-net)
12. [Znane ograniczenia i uwagi produkcyjne](#12-znane-ograniczenia-i-uwagi-produkcyjne)

---

## 1. Przegląd systemu

**MovieRate** to aplikacja do oceniania filmów. Backend odpowiada za:

| Obszar | Opis |
|--------|------|
| **Filmy** | Odczyt listy filmów (z opcjonalnym wyszukiwaniem po tytule) oraz szczegółów pojedynczego filmu |
| **Recenzje** | Odczyt recenzji dla filmu; dodawanie recenzji przez zalogowanego użytkownika |
| **Użytkownicy** | Rejestracja, logowanie, zmiana e-maila i hasła |
| **Sesja** | JWT przechowywany w ciasteczku `httpOnly` o nazwie `access_token` |

**Czego backend NIE robi:**

- CRUD filmów (filmy są seedowane w SQL, brak endpointów tworzenia/edycji/usuwania)
- Edycja ani usuwanie recenzji
- Weryfikacja e-maila, reset hasła, refresh tokenów
- Paginacja, upload plików, panel administracyjny

**Port domyślny:** `3001`
**Dokumentacja Swagger:** `http://localhost:3001/swagger`

---

## 2. Stos technologiczny i uruchomienie

### 2.1 Technologie

| Warstwa | Technologia | Wersja / pakiet NuGet |
|---------|-------------|----------------------|
| Framework | ASP.NET Core | 8.0 |
| ORM | Entity Framework Core | 8.x |
| Provider EF | Npgsql.EntityFrameworkCore.PostgreSQL | 8.x |
| Baza danych | PostgreSQL | 16 |
| Haszowanie haseł | `BCrypt.Net-Next` | 4.x (koszt 10) |
| Tokeny JWT | `System.IdentityModel.Tokens.Jwt` + `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.x |
| Ciasteczka | Wbudowane w ASP.NET Core (`HttpContext.Response.Cookies`) | — |
| Walidacja | `FluentValidation.AspNetCore` lub `System.ComponentModel.DataAnnotations` | — |
| Dokumentacja API | `Swashbuckle.AspNetCore` (Swagger) | 6.x |
| Serializacja JSON | `System.Text.Json` (wbudowane) | — |

### 2.2 Zmienne środowiskowe / `appsettings.json`

Konfiguracja w `appsettings.json` (lub `appsettings.Development.json`):

```json
{
  "App": {
    "Port": 3001,
    "OriginUrl": "http://localhost:3000"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=movie_rate_db;Username=postgres;Password=secret"
  },
  "Jwt": {
    "Secret": "twoj-bardzo-dlugi-i-losowy-sekret-min-32-znaki",
    "ExpiresInDays": 1
  }
}
```

Odpowiedniki zmiennych środowiskowych (dla Dockera / produkcji):

| Zmienna env | Odpowiednik w `appsettings` | Opis |
|-------------|----------------------------|------|
| `PORT` | `App:Port` | Port nasłuchu API (domyślnie `3001`) |
| `ORIGIN_URL` | `App:OriginUrl` | Dozwolony origin CORS (URL frontendu) |
| `DB_HOST` | część `ConnectionStrings:DefaultConnection` | Host PostgreSQL |
| `DB_PORT` | część `ConnectionStrings:DefaultConnection` | Port PostgreSQL |
| `DB_USERNAME` | część `ConnectionStrings:DefaultConnection` | Użytkownik bazy |
| `DB_PASSWORD` | część `ConnectionStrings:DefaultConnection` | Hasło bazy |
| `DB_NAME` | część `ConnectionStrings:DefaultConnection` | Nazwa bazy |
| `JWT_SECRET` | `Jwt:Secret` | Sekret do podpisywania JWT |

> **Uwaga:** Zmienne środowiskowe w .NET nadpisują `appsettings.json` automatycznie. Separator w zmiennych env to podkreślenie podwójne: `Jwt__Secret`, `App__Port` itp.

### 2.3 Konfiguracja Entity Framework Core

Plik: `Program.cs` lub dedykowany extension method.

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

**Krytyczne:** EF Core **nie zarządza schematem** — tabele tworzone są przez skrypt `db/script.sql`. Nie używaj `Database.EnsureCreated()` ani migracji EF w tym projekcie:

```csharp
// NIE rob tego:
// app.Services.GetRequiredService<AppDbContext>().Database.Migrate();

// Schemat zarządzany ręcznie przez db/script.sql
```

### 2.4 Konfiguracja `Program.cs` — bootstrap aplikacji

```csharp
var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var originUrl = builder.Configuration["App:OriginUrl"] ?? "http://localhost:3000";
        policy.WithOrigins(originUrl)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // WYMAGANE dla ciasteczek cross-origin
    });
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
        // Odczyt tokena z ciasteczka, nie z nagłówka Authorization
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["access_token"];
                return Task.CompletedTask;
            }
        };
    });

// Serwisy, kontrolery, Swagger...
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serializacja: snake_case dla zgodności z frontendem
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

var port = builder.Configuration.GetValue<int>("App:Port", 3001);
app.Run($"http://0.0.0.0:{port}");
```

> **Ważne — snake_case:** Frontend oczekuje pól w formacie `snake_case` (np. `movie_id`, `average_rating`). Skonfiguruj `JsonNamingPolicy.SnakeCaseLower` lub użyj atrybutów `[JsonPropertyName("movie_id")]` na każdym DTO/modelu.

### 2.5 Uruchomienie

```bash
# Zbuduj i uruchom
dotnet build
dotnet run --project MovieRate.Api

# Lub z hot-reload podczas developmentu
dotnet watch --project MovieRate.Api

# Docker (cały stack: db + backend + frontend)
docker-compose up --build
```

Przykładowy `Dockerfile` dla backendu .NET:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 3001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MovieRate.Api/MovieRate.Api.csproj", "MovieRate.Api/"]
RUN dotnet restore "MovieRate.Api/MovieRate.Api.csproj"
COPY . .
WORKDIR "/src/MovieRate.Api"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MovieRate.Api.dll"]
```

---

## 3. Architektura backendu

### 3.1 Struktura warstw

W .NET projekt dzielimy na warstwy (można jako osobne projekty lub foldery):

```
MovieRate.Api/
├── Controllers/          → warstwa HTTP (odpowiednik NestJS Controllers)
│   ├── AuthController
│   ├── MoviesController
│   └── ReviewsController
├── Services/             → logika biznesowa (odpowiednik NestJS Services)
│   ├── IAuthService / AuthService
│   ├── IUsersService / UsersService
│   ├── IMoviesService / MoviesService
│   └── IReviewsService / ReviewsService
├── Data/                 → EF Core DbContext i encje (odpowiednik TypeORM Entities)
│   ├── AppDbContext
│   └── Entities/
│       ├── Movie.cs
│       ├── User.cs
│       └── Review.cs
├── DTOs/                 → obiekty transferu danych (odpowiednik NestJS DTOs)
│   ├── Auth/
│   ├── Movies/
│   └── Reviews/
├── Middleware/           → własne middleware / filtry
│   └── CookieJwtMiddleware (opcjonalnie, lub konfiguracja w Program.cs)
└── Program.cs            → bootstrap aplikacji
```

### 3.2 Rejestracja serwisów (DI)

```csharp
// Program.cs
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMoviesService, MoviesService>();
builder.Services.AddScoped<IReviewsService, ReviewsService>();
builder.Services.AddDbContext<AppDbContext>(...);
```

### 3.3 Przepływ danych

```
Frontend (Next.js :3000)
    │  fetch + credentials: 'include'
    │  Content-Type: application/json
    ▼
Backend (ASP.NET Core :3001)
    │  CORS Middleware
    │  Authentication Middleware (JWT z ciasteczka)
    │  Controller → Service → EF Core Repository (DbContext)
    ▼
PostgreSQL (:5432 / host :5433)
    tabele: t_movies, t_users, t_reviews
```

---

## 4. Baza danych PostgreSQL

Plik inicjalizacyjny: `db/script.sql` — **nie modyfikuj schematu przez EF Core.**
EF Core mapuje encje na tabele z prefiksem `t_`.

### 4.1 Tabela `t_movies`

**Przeznaczenie:** Katalog filmów (tylko odczyt przez API).

| Kolumna | Typ SQL | Ograniczenia | Opis |
|---------|---------|--------------|------|
| `movie_id` | `SERIAL` | PRIMARY KEY | Identyfikator filmu |
| `title` | `VARCHAR(255)` | NOT NULL | Tytuł filmu |
| `description` | `TEXT` | NOT NULL | Opis fabuły |
| `poster_url` | `TEXT` | NOT NULL | URL do plakatu |
| `release_year` | `INT` | NOT NULL | Rok premiery |
| `duration` | `INT` | NOT NULL | Czas trwania w minutach |
| `created_at` | `TIMESTAMPTZ` | DEFAULT `NOW()` | Data utworzenia rekordu |

**Encja EF Core (`Movie.cs`):**

```csharp
[Table("t_movies")]
public class Movie
{
    [Column("movie_id")]
    public int MovieId { get; set; }

    [Column("title")]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("poster_url")]
    public string PosterUrl { get; set; } = string.Empty;

    [Column("release_year")]
    public int ReleaseYear { get; set; }

    [Column("duration")]
    public int Duration { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // Relacja (nawigacja)
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
```

**DTO odpowiedzi (`MovieResponseDto`):**

```csharp
public class MovieResponseDto
{
    [JsonPropertyName("movie_id")]
    public int MovieId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("poster_url")]
    public string PosterUrl { get; set; } = string.Empty;

    [JsonPropertyName("release_year")]
    public int ReleaseYear { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("average_rating")]
    public double? AverageRating { get; set; }
}
```

`AverageRating` = `AVG(rating)` z `t_reviews` dla danego `movie_id`, lub `null` gdy brak recenzji.

**Dane seed (skrypt SQL, 2 filmy):**

1. *Sweeney Todd: Demoniczny Golibroda z Fleet Street* (2007, 116 min)
2. *Edward Nożycoręki* (1990, 103 min)

---

### 4.2 Tabela `t_users`

**Przeznaczenie:** Konta użytkowników.

| Kolumna | Typ SQL | Ograniczenia | Opis |
|---------|---------|--------------|------|
| `user_id` | `SERIAL` | PRIMARY KEY | Identyfikator użytkownika |
| `username` | `VARCHAR(50)` | NOT NULL, UNIQUE | Nazwa użytkownika |
| `email` | `VARCHAR(255)` | NOT NULL, UNIQUE | Adres e-mail |
| `password` | `VARCHAR(255)` | NOT NULL | Hash bcrypt (nigdy plaintext) |
| `created_at` | `TIMESTAMPTZ` | DEFAULT `NOW()` | Data rejestracji |

**Encja EF Core (`User.cs`):**

```csharp
[Table("t_users")]
public class User
{
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("username")]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Column("email")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Column("password")]
    [MaxLength(255)]
    public string Password { get; set; } = string.Empty; // Hash bcrypt!

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
```

> **Bezpieczeństwo:** Pole `Password` (hash) **nigdy** nie jest zwracane do frontendu. Nie twórz DTO zawierającego to pole.

**Operacje zapisu:**

| Operacja | Endpoint | Zapisywane pola |
|----------|----------|-----------------|
| Rejestracja | `POST /auth/register` | `username`, `email`, `password` (bcrypt hash) |
| Zmiana e-maila | `PATCH /auth/change-email` | `email` → nowy adres |
| Zmiana hasła | `PATCH /auth/change-password` | `password` → nowy hash bcrypt |

---

### 4.3 Tabela `t_reviews`

**Przeznaczenie:** Recenzje użytkowników do filmów.

| Kolumna | Typ SQL | Ograniczenia | Opis |
|---------|---------|--------------|------|
| `review_id` | `SERIAL` | PRIMARY KEY | Identyfikator recenzji |
| `user_id` | `INT` | NOT NULL, FK → `t_users(user_id)` ON DELETE CASCADE | Autor |
| `movie_id` | `INT` | NOT NULL, FK → `t_movies(movie_id)` ON DELETE CASCADE | Film |
| `title` | `VARCHAR(50)` | NULL dozwolony | Opcjonalny tytuł recenzji |
| `body` | `TEXT` | NULL dozwolony | Opcjonalna treść recenzji |
| `rating` | `INT` | NOT NULL, CHECK 1–10 | Ocena liczbowa |
| `created_at` | `TIMESTAMPTZ` | DEFAULT `NOW()` | Data dodania |

**Ograniczenie unikalności:**

```sql
CONSTRAINT unique_user_review UNIQUE (user_id, movie_id)
```

Jeden użytkownik może dodać **dokładnie jedną** recenzję do danego filmu. Ponowna próba → Postgres rzuca wyjątek naruszenia unikalności → API zwraca `409 Conflict`.

**Encja EF Core (`Review.cs`):**

```csharp
[Table("t_reviews")]
public class Review
{
    [Column("review_id")]
    public int ReviewId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("movie_id")]
    public int MovieId { get; set; }

    [Column("title")]
    [MaxLength(50)]
    public string? Title { get; set; }

    [Column("body")]
    public string? Body { get; set; }

    [Column("rating")]
    public int Rating { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // Właściwości nawigacyjne
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(MovieId))]
    public Movie Movie { get; set; } = null!;
}
```

**Konfiguracja unikalności w EF Core (`AppDbContext.cs`):**

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Review>()
        .HasIndex(r => new { r.UserId, r.MovieId })
        .IsUnique();

    modelBuilder.Entity<Review>()
        .HasOne(r => r.User)
        .WithMany(u => u.Reviews)
        .HasForeignKey(r => r.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<Review>()
        .HasOne(r => r.Movie)
        .WithMany(m => m.Reviews)
        .HasForeignKey(r => r.MovieId)
        .OnDelete(DeleteBehavior.Cascade);
}
```

---

### 4.4 AppDbContext

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Movie> Movies { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Konfiguracja jak wyżej
    }
}
```

### 4.5 Diagram relacji

```
t_users (1) ──────< (N) t_reviews (N) >────── (1) t_movies
         user_id FK              movie_id FK

UNIQUE (user_id, movie_id) na t_reviews
```

Usunięcie użytkownika lub filmu kaskadowo usuwa powiązane recenzje.

---

## 5. Autentykacja i sesje

### 5.1 Model sesji

Backend używa **JWT w ciasteczku HTTP**, nie nagłówka `Authorization: Bearer`.

| Parametr | Wartość |
|----------|---------|
| Nazwa ciasteczka | `access_token` |
| `HttpOnly` | `true` (niedostępne z JavaScript w przeglądarce) |
| `SameSite` | `Lax` |
| `Secure` | `false` (dev); `true` przy HTTPS produkcji |
| Ważność | 24 godziny |
| `Path` | `/` |

**Ustawianie ciasteczka w ASP.NET Core:**

```csharp
Response.Cookies.Append("access_token", token, new CookieOptions
{
    HttpOnly = true,
    SameSite = SameSiteMode.Lax,
    Secure = false,          // true w produkcji za HTTPS
    Path = "/",
    Expires = DateTimeOffset.UtcNow.AddDays(1)
});
```

**Czyszczenie ciasteczka (logout / zmiana danych):**

```csharp
Response.Cookies.Append("access_token", string.Empty, new CookieOptions
{
    HttpOnly = true,
    SameSite = SameSiteMode.Lax,
    Path = "/",
    Expires = DateTimeOffset.UnixEpoch  // 1 Jan 1970 — ciasteczko wygasa natychmiast
});
```

### 5.2 Payload JWT

```json
{
  "sub": "1",
  "email": "user@example.com",
  "iat": 1717776000,
  "exp": 1717862400
}
```

| Claim | Znaczenie |
|-------|-----------|
| `sub` | `user_id` z tabeli `t_users` (jako string) |
| `email` | Aktualny e-mail użytkownika |
| `exp` | Wygaśnięcie po 1 dniu |

**Generowanie JWT w .NET:**

```csharp
private string GenerateToken(int userId, string email)
{
    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, email)
    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.UtcNow.AddDays(1),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

### 5.3 Odczyt tokena — konfiguracja JwtBearer

Standardowy middleware JWT w ASP.NET Core szuka tokena w nagłówku `Authorization`. Musimy go skierować na ciasteczko:

```csharp
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Odczyt tokena z ciasteczka zamiast nagłówka
            context.Token = context.Request.Cookies["access_token"];
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // Zwróć 401 z komunikatem zgodnym z frontendem
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var body = context.Request.Cookies.ContainsKey("access_token")
                ? """{"statusCode":401,"message":"Incorrect token"}"""
                : """{"statusCode":401,"message":"Token not found"}""";
            return context.Response.WriteAsync(body);
        }
    };
});
```

### 5.4 Endpointy chronione vs publiczne

Dekoratory kontrolerów:

```csharp
[Authorize]     // wymaga ważnego JWT w ciasteczku
[AllowAnonymous]  // publiczny
```

| Endpoint | Autoryzacja |
|----------|-------------|
| `POST /auth/register` | `[AllowAnonymous]` |
| `POST /auth/login` | `[AllowAnonymous]` |
| `POST /auth/logout` | `[AllowAnonymous]` |
| `PATCH /auth/change-email` | `[Authorize]` |
| `PATCH /auth/change-password` | `[Authorize]` |
| `GET /movies` | `[AllowAnonymous]` |
| `GET /movies/{id}` | `[AllowAnonymous]` |
| `GET /reviews/{movieId}` | `[AllowAnonymous]` |
| `POST /reviews/add/{movieId}` | `[Authorize]` |

**Odczyt ID użytkownika z tokena w kontrolerze:**

```csharp
var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
// lub
var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
```

### 5.5 Cykl życia sesji

```
Rejestracja / Logowanie
    → backend generuje JWT
    → Set-Cookie: access_token=<JWT>; HttpOnly; SameSite=Lax
    → frontend przekierowuje na /

Wylogowanie (POST /auth/logout)
    → Set-Cookie: access_token=; Expires=1 Jan 1970

Zmiana e-maila / hasła (PATCH /auth/change-*)
    → operacja na bazie danych
    → ciasteczko wyczyszczone (wymuszone ponowne logowanie)
    → frontend po 2 s przekierowuje na /Login
```

### 5.6 Haszowanie haseł (BCrypt)

```csharp
// Instalacja: dotnet add package BCrypt.Net-Next

// Hashowanie (rejestracja, zmiana hasła)
string hash = BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 10);

// Weryfikacja (logowanie, zmiana e-maila/hasła)
bool isValid = BCrypt.Net.BCrypt.Verify(plainPassword, storedHash);
```

---

## 6. Konwencje API

### 6.1 Format żądań

| Element | Wartość |
|---------|---------|
| Protokół | HTTP/HTTPS |
| `Content-Type` | `application/json` (dla POST/PATCH z body) |
| Credentials | Frontend wysyła `credentials: 'include'` (ciasteczka cross-origin) |
| Base URL (lokalnie) | `http://localhost:3001` |
| Base URL (Docker) | `http://backend:3001` |

### 6.2 Format odpowiedzi sukcesu

- Body: JSON
- Rejestracja: `201 Created`
- Dodanie recenzji: `201 Created`
- Pozostałe sukcesy: `200 OK`

### 6.3 Format odpowiedzi błędu

Frontend parsuje pole `message` i rzuca `Error(message)`. Backend **musi** zwracać błędy w tym formacie:

```json
{
  "statusCode": 401,
  "message": "Incorrect password"
}
```

Pomocnicza klasa DTO błędu:

```csharp
public class ErrorResponseDto
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
```

Przykład zwracania błędu w kontrolerze:

```csharp
return StatusCode(401, new ErrorResponseDto
{
    StatusCode = 401,
    Message = "Incorrect password"
});
```

### 6.4 Kody HTTP używane w projekcie

| Kod | Kiedy |
|-----|-------|
| `200` | Sukces (GET, PATCH, POST logout/login) |
| `201` | Utworzono (register, add review) |
| `401` | Brak/nieprawidłowy token, złe hasło, nieznany e-mail |
| `409` | Konflikt unikalności (email, username, duplikat recenzji) |
| `500` | Nieobsłużony błąd serwera |

### 6.5 snake_case — krytyczne dla kompatybilności z frontendem

Frontend TypeScript oczekuje pól w `snake_case`. Masz dwa podejścia:

**Opcja A (globalna — zalecana):** Konfiguracja w `Program.cs`:

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
```

**Opcja B (per-pole):** Atrybuty na każdym DTO:

```csharp
[JsonPropertyName("movie_id")]
public int MovieId { get; set; }
```

---

## 7. Endpointy — pełna specyfikacja

### 7.1 Autentykacja — prefix `/auth`

Kontroler: `AuthController`

---

#### `POST /auth/register`

**Cel:** Utworzenie nowego konta i automatyczne zalogowanie.
**Autoryzacja:** Brak (`[AllowAnonymous]`)

**Request body:**

```json
{
  "username": "jan_kowalski",
  "email": "jan@example.com",
  "password": "haslo12345"
}
```

**DTO żądania:**

```csharp
public class RegisterDto
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;   // min 3 znaki, unikalny

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;      // poprawny format, unikalny

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;   // min 8 znaków
}
```

**Logika serwisu:**

1. Sprawdź czy `email` istnieje → `409` `"Email address is already taken"`
2. Sprawdź czy `username` istnieje → `409` `"Username is already taken"`
3. Zahashuj hasło (`BCrypt`, koszt 10)
4. `INSERT` do `t_users`
5. Wygeneruj JWT `{ sub: user_id, email }`
6. Ustaw ciasteczko `access_token`

**Response:**

```http
HTTP/1.1 201 Created
Set-Cookie: access_token=<JWT>; Path=/; HttpOnly; SameSite=Lax

{"message": "User registered successfully"}
```

**Implementacja kontrolera:**

```csharp
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterDto dto)
{
    var token = await _authService.RegisterAsync(dto);
    Response.Cookies.Append("access_token", token, GetCookieOptions());
    return StatusCode(201, new { message = "User registered successfully" });
}
```

---

#### `POST /auth/login`

**Cel:** Logowanie istniejącego użytkownika.
**Autoryzacja:** Brak (`[AllowAnonymous]`)

**Request body:**

```json
{
  "email": "jan@example.com",
  "password": "haslo12345"
}
```

**Logika serwisu:**

1. Znajdź użytkownika po `email`
2. Brak użytkownika → `401` `"Invalid credentials"`
3. `BCrypt.Verify` hasła → niezgodne → `401` `"Incorrect password"`
4. Wygeneruj JWT
5. Ustaw ciasteczko

**Response:**

```http
HTTP/1.1 200 OK
Set-Cookie: access_token=<JWT>; ...

{"message": "User logged in successfully"}
```

---

#### `POST /auth/logout`

**Cel:** Wylogowanie — usunięcie ciasteczka po stronie klienta.
**Autoryzacja:** Brak (działa nawet bez aktywnego tokena)

**Request body:** Brak

**Logika:** Wyczyść ciasteczko `access_token` (ustaw `Expires = UnixEpoch`).

**Response:**

```http
HTTP/1.1 200 OK
Set-Cookie: access_token=; Expires=Thu, 01 Jan 1970 00:00:00 GMT; Path=/; HttpOnly

{"message": "User logged out successfully"}
```

---

#### `PATCH /auth/change-email`

**Cel:** Zmiana adresu e-mail z weryfikacją aktualnego hasła.
**Autoryzacja:** `[Authorize]` — wymagane ciasteczko `access_token`

**Request body:**

```json
{
  "new_email": "nowy@example.com",
  "password": "aktualne_haslo"
}
```

**DTO żądania:**

```csharp
public class ChangeEmailDto
{
    [JsonPropertyName("new_email")]
    public string NewEmail { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}
```

**Logika serwisu:**

1. `userId` z JWT (`User.FindFirstValue(JwtRegisteredClaimNames.Sub)`)
2. Pobierz użytkownika → brak → `401` `"User not found"`
3. Zweryfikuj aktualne `password` (BCrypt) → błąd → `401` `"Incorrect password"`
4. Sprawdź czy `new_email` wolny → zajęty → `409` `"Email is already taken"`
5. `UPDATE t_users SET email = new_email WHERE user_id = sub`
6. **Wyczyść ciasteczko** (wymusza ponowne logowanie)

**Response:**

```http
HTTP/1.1 200 OK
Set-Cookie: access_token=; Expires=... (wyczyszczone)

{"message": "Email changed successfully"}
```

---

#### `PATCH /auth/change-password`

**Cel:** Zmiana hasła z weryfikacją aktualnego hasła.
**Autoryzacja:** `[Authorize]`

**Request body:**

```json
{
  "new_password": "nowe_haslo_123",
  "password": "aktualne_haslo"
}
```

**Logika serwisu:**

1. `userId` z JWT
2. Pobierz użytkownika → brak → `401` `"User not found"`
3. Zweryfikuj aktualne `password` → błąd → `401` `"Incorrect password"`
4. Zahashuj `new_password` (BCrypt, koszt 10)
5. `UPDATE t_users SET password = hash WHERE user_id = sub`
6. **Wyczyść ciasteczko**

**Response:**

```http
HTTP/1.1 200 OK
Set-Cookie: access_token=; ... (wyczyszczone)

{"message": "Password changed successfully"}
```

---

### 7.2 Filmy — prefix `/movies`

Kontroler: `MoviesController`

---

#### `GET /movies`

**Cel:** Lista wszystkich filmów lub wyszukiwanie po tytule.
**Autoryzacja:** Brak

**Query params:**

| Param | Typ | Wymagane | Opis |
|-------|-----|----------|------|
| `title` | string | Nie | Filtr case-insensitive (ILIKE `%title%`) |

**Zapytania EF Core:**

```csharp
// Bez filtra — wszystkie filmy, sort: created_at DESC
var movies = await _context.Movies
    .OrderByDescending(m => m.CreatedAt)
    .ToListAsync();

// Z filtrem — ILIKE w Npgsql
var movies = await _context.Movies
    .Where(m => EF.Functions.ILike(m.Title, $"%{title}%"))
    .OrderByDescending(m => m.Title)
    .ToListAsync();
```

**Obliczanie average_rating:**

```csharp
// Osobne zapytanie dla każdego filmu
var avgRating = await _context.Reviews
    .Where(r => r.MovieId == movie.MovieId)
    .AverageAsync(r => (double?)r.Rating);
// Zwraca null gdy brak recenzji
```

**Response:**

```json
[
  {
    "movie_id": 1,
    "title": "Sweeney Todd: Demoniczny Golibroda z Fleet Street",
    "description": "...",
    "poster_url": "https://...",
    "release_year": 2007,
    "duration": 116,
    "created_at": "2025-06-07T10:00:00.000Z",
    "average_rating": null
  }
]
```

---

#### `GET /movies/{movieId}`

**Cel:** Szczegóły pojedynczego filmu.
**Autoryzacja:** Brak

**Param URL:**

| Param | Typ | Opis |
|-------|-----|------|
| `movieId` | int | ID filmu, parsowany automatycznie |

**Logika:**

1. `SELECT` film po `movie_id`
2. Jeśli znaleziony → dołącz `average_rating`
3. Jeśli nie znaleziony → zwróć `null` ze statusem `200` (zachowaj oryginalną logikę dla kompatybilności z frontendem)

**Response — film istnieje:**

```json
{
  "movie_id": 1,
  "title": "...",
  "average_rating": 8.0
}
```

**Response — film nie istnieje:**

```http
HTTP/1.1 200 OK

null
```

> **Uwaga:** Oryginalna implementacja zwraca `null` zamiast `404`. Frontend nie obsługuje `null` — zachowaj to zachowanie lub popraw jednocześnie po stronie frontowej. Aby zwrócić `null` z `200 OK` w ASP.NET Core: `return Ok((object?)null);`

---

### 7.3 Recenzje — prefix `/reviews`

Kontroler: `ReviewsController`

---

#### `GET /reviews/{movieId}`

**Cel:** Lista wszystkich recenzji dla danego filmu.
**Autoryzacja:** Brak

**Logika:**

```csharp
var reviews = await _context.Reviews
    .Where(r => r.MovieId == movieId)
    .Include(r => r.User)
    .OrderByDescending(r => r.CreatedAt)
    .Select(r => new ReviewResponseDto
    {
        ReviewId = r.ReviewId,
        Username = r.User.Username,
        Title = r.Title,
        Body = r.Body,
        Rating = r.Rating,
        CreatedAt = r.CreatedAt
    })
    .ToListAsync();
```

**Response:**

```json
[
  {
    "review_id": 5,
    "username": "jan_kowalski",
    "title": "Arcydzieło",
    "body": "Najlepszy musical...",
    "rating": 10,
    "created_at": "2025-06-07T16:00:00.000Z"
  }
]
```

Pola `user_id` i `movie_id` **nie są** zwracane w tej odpowiedzi. Pusta lista `[]` gdy brak recenzji.

**DTO odpowiedzi:**

```csharp
public class ReviewResponseDto
{
    [JsonPropertyName("review_id")]
    public int ReviewId { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("rating")]
    public int Rating { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}
```

---

#### `POST /reviews/add/{movieId}`

**Cel:** Dodanie recenzji przez zalogowanego użytkownika.
**Autoryzacja:** `[Authorize]`

**Request:**

```http
POST /reviews/add/1 HTTP/1.1
Cookie: access_token=<JWT>
Content-Type: application/json

{
  "movie_id": 1,
  "title": "Moja opinia",
  "body": "Bardzo polecam.",
  "rating": 9
}
```

**DTO żądania:**

```csharp
public class AddReviewDto
{
    [JsonPropertyName("movie_id")]
    public int? MovieId { get; set; }       // Ignorowane — nadpisywane przez param URL

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("rating")]
    public int Rating { get; set; }          // Wymagane, 1-10
}
```

**Logika:**

```csharp
[HttpPost("add/{movieId:int}")]
[Authorize]
public async Task<IActionResult> AddReview(int movieId, [FromBody] AddReviewDto dto)
{
    var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    var review = new Review
    {
        UserId = userId,
        MovieId = movieId,      // Zawsze z URL, nie z body!
        Title = dto.Title,
        Body = dto.Body,
        Rating = dto.Rating,
        CreatedAt = DateTime.UtcNow
    };

    try
    {
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        return StatusCode(201, review);
    }
    catch (DbUpdateException ex)
        when (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
    {
        return StatusCode(409, new ErrorResponseDto
        {
            StatusCode = 409,
            Message = "You have already reviewed this movie"
        });
    }
}
```

**Response (sukces):**

```http
HTTP/1.1 201 Created

{
  "review_id": 6,
  "user_id": 2,
  "movie_id": 1,
  "title": "Moja opinia",
  "body": "Bardzo polecam.",
  "rating": 9,
  "created_at": "2025-06-07T17:00:00.000Z"
}
```

---

## 8. Warstwa serwisów — logika biznesowa

### 8.1 `IAuthService` / `AuthService`

```csharp
public interface IAuthService
{
    Task<string> RegisterAsync(RegisterDto dto);       // Zwraca token JWT
    Task<string> LoginAsync(LoginDto dto);             // Zwraca token JWT
    Task ChangeEmailAsync(int userId, ChangeEmailDto dto);
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
}
```

| Metoda | Wejście | Wyjście | Efekt w DB |
|--------|---------|---------|------------|
| `RegisterAsync` | `RegisterDto` | `string` (JWT) | INSERT `t_users` |
| `LoginAsync` | `LoginDto` | `string` (JWT) | SELECT `t_users` |
| `ChangeEmailAsync` | `userId`, `ChangeEmailDto` | `void` | UPDATE email |
| `ChangePasswordAsync` | `userId`, `ChangePasswordDto` | `void` | UPDATE password |

### 8.2 `IUsersService` / `UsersService`

Brak ekspozycji HTTP — używany wyłącznie przez `AuthService`.

```csharp
public interface IUsersService
{
    Task<User> CreateAsync(string username, string email, string passwordHash);
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByUsernameAsync(string username);
    Task<User?> FindByIdAsync(int userId);
    Task UpdateEmailAsync(int userId, string newEmail);
    Task UpdatePasswordAsync(int userId, string newPasswordHash);
}
```

### 8.3 `IMoviesService` / `MoviesService`

```csharp
public interface IMoviesService
{
    Task<List<MovieResponseDto>> GetAllAsync();
    Task<List<MovieResponseDto>> SearchByTitleAsync(string title);
    Task<MovieResponseDto?> GetByIdAsync(int movieId);
}
```

| Metoda | Zapytanie DB | Dodatkowa logika |
|--------|--------------|------------------|
| `GetAllAsync()` | Wszystkie filmy, `ORDER BY created_at DESC` | `average_rating` per film |
| `SearchByTitleAsync(title)` | `ILike('%title%')`, `ORDER BY title DESC` | `average_rating` per film |
| `GetByIdAsync(movieId)` | `FirstOrDefault({ MovieId })` | `average_rating` jeśli istnieje |

### 8.4 `IReviewsService` / `ReviewsService`

```csharp
public interface IReviewsService
{
    Task<double?> GetAverageRatingAsync(int movieId);
    Task<List<ReviewResponseDto>> GetReviewsAsync(int movieId);
    Task<Review> AddReviewAsync(int userId, int movieId, AddReviewDto dto);
}
```

| Metoda | Zapytanie DB | Uwagi |
|--------|--------------|-------|
| `GetAverageRatingAsync` | `AVG(rating) WHERE movie_id` | Zwraca `double?` lub `null` |
| `GetReviewsAsync` | Include User, `ORDER BY created_at DESC` | Mapuje do `ReviewResponseDto` |
| `AddReviewAsync` | INSERT | Łapie `23505` → `409 Conflict` |

---

## 9. Integracja z frontendem

### 9.1 Klient HTTP frontendu

```typescript
const BASE_URL = typeof window === 'undefined'
  ? process.env.API_URL_SERVER           // Server Components: http://backend:3001
  : process.env.NEXT_PUBLIC_API_URL_CLIENT;  // Browser: http://localhost:3001

fetch(`${BASE_URL}${endpoint}`, {
  credentials: 'include',   // WYMAGANE — wysyła ciasteczka
  headers: { 'Content-Type': 'application/json' },
});
```

### 9.2 Macierz: Frontend → Backend

| Lokalizacja UI | Metoda | Endpoint | Body wysyłane | Response używane |
|----------------|--------|----------|---------------|------------------|
| Strona główna (`/`) | GET | `/movies` lub `/movies?title=` | — | `Movie[]` |
| Login (`/Login`) | POST | `/auth/login` | `{ email, password }` | `{ message }` + cookie |
| Register (`/Register`) | POST | `/auth/register` | `{ username, email, password }` | `{ message }` + cookie |
| Opinie (`/Reviews`) | GET | `/movies/:id` | — | `Movie` (poster, tytuł, avg) |
| Opinie (`/Reviews`) | GET | `/reviews/:id` | — | `Review[]` |
| Dodaj opinię (`/Reviews/Rate`) | GET | `/movies/:id` | — | `Movie.title` |
| Formularz recenzji | POST | `/reviews/add/:id` | `{ movie_id, title?, body?, rating }` | ignorowane (redirect) |
| Wyloguj | POST | `/auth/logout` | — | `{ message }` |
| Zmiana e-maila | PATCH | `/auth/change-email` | `{ new_email, password }` | `{ message }` |
| Zmiana hasła | PATCH | `/auth/change-password` | `{ new_password, password }` | `{ message }` |

### 9.3 Typy oczekiwane przez frontend

**`Movie`** (`frontend/src/Interfaces/Movie.ts`):

```typescript
interface Movie {
  movie_id: number;
  title: string;
  description: string;
  poster_url: string;
  release_year: number;
  duration: number;
  created_at: Date;
  average_rating: number | null;
}
```

**`Review`** (`frontend/src/Interfaces/Review.ts`):

```typescript
interface Review {
  review_id: number;
  username: string;
  title?: string;
  body?: string;
  rating: number;
  created_at: Date;
}
```

Backend **musi** zwracać dokładnie te pola w formacie `snake_case`, aby frontend działał bez modyfikacji.

### 9.4 Wykrywanie stanu zalogowania na frontendzie

Server Components sprawdzają obecność ciasteczka:

```typescript
const cookieStorage = await cookies();
const isLoggedIn = cookieStorage.has('access_token');
```

> **Znany problem architektury rozdzielonej (frontend :3000, backend :3001):** Ciasteczko `access_token` jest ustawiane dla hosta backendu (`localhost:3001`). Next.js `cookies()` na `:3000` może nie widzieć tego ciasteczka w Server Components, mimo że przeglądarka wysyła je poprawnie do API przy żądaniach klienckich (`credentials: 'include'`). Przy przebudowie backendu w monolicie Next.js (ten sam origin) problem znika.

---

## 10. Reguły biznesowe — checklista implementacji

### Autentykacja

- [ ] Hasła hashowane BCrypt z kosztem 10 (`BCrypt.Net.BCrypt.HashPassword(..., 10)`)
- [ ] JWT payload: `{ sub: user_id, email }`, ważność 1 dzień
- [ ] Ciasteczko `access_token`: `HttpOnly`, `SameSite=Lax`, `Path=/`
- [ ] Token odczytywany z ciasteczka (`OnMessageReceived`), nie z nagłówka Bearer
- [ ] Po zmianie e-maila/hasła — wyczyść ciasteczko
- [ ] Rejestracja: sprawdź unikalność email i username przed INSERT
- [ ] Logowanie: osobne komunikaty dla braku użytkownika vs złego hasła

### Filmy

- [ ] Brak endpointów mutacji filmów (tylko GET)
- [ ] Lista domyślnie sortowana `created_at DESC`
- [ ] Wyszukiwanie: `EF.Functions.ILike` (case-insensitive), sort `title DESC`
- [ ] Każda odpowiedź filmu zawiera `average_rating` (nullable `double?`)

### Recenzje

- [ ] Jeden użytkownik = jedna recenzja na film (UNIQUE constraint + obsługa `23505` → 409)
- [ ] `rating` integer 1–10 (CHECK w DB)
- [ ] `user_id` z JWT (`User.FindFirstValue`), nigdy z body żądania
- [ ] `movie_id` z parametru URL ma pierwszeństwo nad body
- [ ] Lista recenzji zawiera `username`, nie `user_id`
- [ ] Lista recenzji sortowana `created_at DESC`
- [ ] `average_rating` = `AVG(rating)` lub `null`

### Baza danych

- [ ] EF Core bez migracji — schemat zarządzany przez `db/script.sql`
- [ ] Tabele: `t_movies`, `t_users`, `t_reviews` (atrybuty `[Table("t_...")]`)
- [ ] FK z `OnDelete(DeleteBehavior.Cascade)`
- [ ] Seed filmów w `db/script.sql`

### CORS i sieć

- [ ] `AllowCredentials()` w konfiguracji CORS (bez tego ciasteczka nie działają cross-origin)
- [ ] `WithOrigins(originUrl)` — adres frontendu (nie `*`)
- [ ] Frontend wysyła `credentials: 'include'` w każdym wywołaniu

### Serializacja JSON

- [ ] Wszystkie pola DTO w `snake_case` (globalna polityka lub atrybuty `[JsonPropertyName]`)
- [ ] Daty serializowane jako ISO 8601 UTC (domyślne w `System.Text.Json`)
- [ ] Pola `null` serializowane jako `null` (nie pomijane)

---

## 11. Struktura projektu .NET

Zalecana struktura projektu ASP.NET Core (jeden projekt):

```
MovieRate.Api/
├── Program.cs                          # Bootstrap, CORS, JWT, Swagger, DI
├── MovieRate.Api.csproj
│
├── Controllers/
│   ├── AuthController.cs               # POST/PATCH /auth/*
│   ├── MoviesController.cs             # GET /movies, GET /movies/{id}
│   └── ReviewsController.cs            # GET /reviews/{id}, POST /reviews/add/{id}
│
├── Services/
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IUsersService.cs
│   │   ├── IMoviesService.cs
│   │   └── IReviewsService.cs
│   ├── AuthService.cs
│   ├── UsersService.cs
│   ├── MoviesService.cs
│   └── ReviewsService.cs
│
├── Data/
│   ├── AppDbContext.cs                 # EF Core DbContext
│   └── Entities/
│       ├── Movie.cs                    # [Table("t_movies")]
│       ├── User.cs                     # [Table("t_users")]
│       └── Review.cs                   # [Table("t_reviews")]
│
├── DTOs/
│   ├── Auth/
│   │   ├── RegisterDto.cs
│   │   ├── LoginDto.cs
│   │   ├── ChangeEmailDto.cs
│   │   └── ChangePasswordDto.cs
│   ├── Movies/
│   │   └── MovieResponseDto.cs
│   └── Reviews/
│       ├── AddReviewDto.cs
│       └── ReviewResponseDto.cs
│
└── Common/
    └── ErrorResponseDto.cs

db/
└── script.sql                          # DDL + seed filmów
```

**Plik projektu (`MovieRate.Api.csproj`) — zależności NuGet:**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.*" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.*" />
    <PackageReference Include="BCrypt.Net-Next" Version="4.*" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.*" />
  </ItemGroup>
</Project>
```

---

## 12. Znane ograniczenia i uwagi produkcyjne

| # | Problem | Wpływ | Rekomendacja |
|---|---------|-------|--------------|
| 1 | `GET /movies/{id}` zwraca `null` zamiast 404 | Frontend może się wywrócić przy nieistniejącym ID | Zwróć `404 NotFound` i zaktualizuj obsługę po stronie frontowej |
| 2 | `Secure = false` na ciasteczku | Ryzyko przechwycenia tokena przy HTTP w produkcji | Ustaw `Secure = true` za HTTPS / reverse proxy |
| 3 | Cross-origin cookies (:3000 vs :3001) | SSR `isLoggedIn` może być fałszywie `false` | Rozważ reverse proxy (Nginx) lub monolit Next.js |
| 4 | Brak rate limiting | Podatność na brute-force i nadużycia | Dodaj `AspNetCoreRateLimit` lub middleware w produkcji |
| 5 | Brak nagłówków bezpieczeństwa (HSTS, CSP) | Standardowe luki web security | Dodaj `app.UseHsts()` i pakiet `NWebsec` lub `Helmet`-equivalent |
| 6 | Brak paginacji list filmów/recenzji | OK przy małym seedzie | Dodaj gdy dane urosną |
| 7 | `average_rating` obliczane N+1 zapytaniami | Wydajność przy dużej liczbie filmów | Zmień na JOIN lub podzapytanie SQL |
| 8 | Walidacja danych wejściowych | Bez walidacji niepoprawne dane trafiają do DB | Dodaj `DataAnnotations` lub `FluentValidation` |

---

## Podsumowanie — minimalna powierzchnia API

Zaimplementuj dokładnie te 9 tras, aby frontend działał bez zmian:

```
POST   /auth/register
POST   /auth/login
POST   /auth/logout
PATCH  /auth/change-email        [Authorize]
PATCH  /auth/change-password     [Authorize]
GET    /movies
GET    /movies/{movieId}
GET    /reviews/{movieId}
POST   /reviews/add/{movieId}    [Authorize]
```

Połącz je z PostgreSQL według schematu `db/script.sql`, zachowaj kontrakt JSON opisany w sekcji 7, zadbaj o `snake_case` w odpowiedziach i `AllowCredentials()` w CORS — a frontend Next.js będzie działał bez żadnych zmian w logice wywołań API.

---

*Dokumentacja oparta na oryginalnym backendzie NestJS 11 + TypeORM + PostgreSQL 16. Wersja docelowa: ASP.NET Core 8 + Entity Framework Core 8 + Npgsql.*
