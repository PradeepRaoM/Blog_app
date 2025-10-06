using Blog_app_Frontend;
using Blog_app_Frontend.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using System;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient points to backend API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7247/") });

// MudBlazor services
builder.Services.AddMudServices();
builder.Services.AddScoped<IDialogService, DialogService>();

// Application services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<TagService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<LikeService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<SavedPostService>();
builder.Services.AddScoped<UserFollowService>();
builder.Services.AddScoped<NotificationService>();




// Authentication
builder.Services.AddOptions();
builder.Services.AddAuthorizationCore(); // ? Required for [Authorize]
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

await builder.Build().RunAsync();
