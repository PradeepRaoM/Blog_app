using Blog_app_backend.Models;
using Blog_app_backend.Services;
using Blog_app_backend.Supabase;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// CORS policy name
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("https://localhost:7040", "http://localhost:8080") // Add Swagger UI origin if needed
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// Configure Supabase settings
builder.Services.Configure<SupabaseSettings>(builder.Configuration.GetSection("Supabase"));

// Register Supabase client provider
builder.Services.AddSingleton<SupabaseClientProvider>();

// Register Supabase.Client from provider
builder.Services.AddSingleton(sp =>
{
    var provider = sp.GetRequiredService<SupabaseClientProvider>();
    return provider.GetClient(); // Returns Supabase.Client
});

// Register application services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProfileService>(); // ProfileService now gets Supabase.Client injected
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<TagService>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<PostTagService>();
builder.Services.AddScoped<LikeService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<SavedPostService>();
builder.Services.AddScoped<UserFollowService>();
builder.Services.AddScoped<NotificationService>();


builder.Services.AddControllers();

// Configure Swagger with JWT Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Blog App Backend",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Insert 'Bearer ' followed by JWT"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// Supabase JWT secret for HS256
var supabaseJwtSecret = "fr0cUAohzTcVkhnAScwyGKbLT8xyn9tfFLbfjOF8cjLgAVK/MaW0796Obp2hvC4KO3rFXGAKd2wRFgnNzRUPQw==";

// Configure JWT authentication for Supabase tokens
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // Preserve original claim types (e.g., "sub")
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://kiqomnbdtdgxlzreimvq.supabase.co/auth/v1",
            ValidateAudience = true,
            ValidAudience = "authenticated",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(supabaseJwtSecret)),
            NameClaimType = "sub",
            RoleClaimType = "role"
        };
        options.RequireHttpsMetadata = true; // Set false for local testing if SSL issues
    });

var app = builder.Build();

// Swagger for Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

// Debugging middleware to log authentication state
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        Console.WriteLine("User is authenticated. Claims: " +
            string.Join(", ", context.User.Claims.Select(c => $"{c.Type}: {c.Value}")));
    }
    else
    {
        Console.WriteLine("User is not authenticated.");
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
