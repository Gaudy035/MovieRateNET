using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using backend.Data;
using backend.Services;
using backend.Jobs;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// DBContext
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DbConnection")));

builder.Services.AddControllers().AddJsonOptions(options =>
{
// Serializacja Jsonow
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

// Zmiennia z orginami z appsettings
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

// Cors
builder.Services.AddCors(options =>
{
   options.AddPolicy("FrontendPolicy", policy =>
   {
       policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
   }); 
});

// Jwt
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        // Pobierane z appsettings
        // ValidIssuer = builder.Configuration["Jwt:Issuer"],
        // ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        )
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("access_token"))
            {
                context.Token = context.Request.Cookies["access_token"];
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Quartz
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("TokenCleanupJob");

    q.AddJob<TokenCleanupJob>(options => options.WithIdentity(jobKey));

    q.AddTrigger(options => options
        .ForJob(jobKey)
        .WithIdentity("TokenCleanupJobTrigger")
        .WithCronSchedule("0 0 3 * * ?")
    );
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// Serwisy i interfejsy
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
