using Inventory_System.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<InventoryDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Auth config
builder.Services.AddAuthentication(options =>
{
    // default JWT bearer scheme for endpoints
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // config parameters for token validation
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true, // verify token is signed with secret key
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!)),

        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        ValidateLifetime = true,  // check whether token is expired
        ClockSkew = TimeSpan.Zero // no tolerance for expired tokens
    };

    // allow to read JWT token from cookies as well
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // if there is no auth header check for token in cookie
            if (string.IsNullOrEmpty(context.Token) && context.Request.Cookies.ContainsKey("AccessToken"))
            {
                context.Token = context.Request.Cookies["AccessToken"];
            }
            return Task.CompletedTask;
        }
    };
})
.AddCookie("Cookies", options =>
{
    // cookie config for web login
    options.Cookie.Name = "InventoryAuth";
    options.LoginPath = "/login"; // redirect if not authenticated
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied"; // redirect if access is denied
    options.ExpireTimeSpan = TimeSpan.FromHours(1); // cookie expire time is 1 hr
    options.SlidingExpiration = true; // refresh expire time on each request

    // security settings for cookies
    options.Cookie.HttpOnly = true; // XSS protection
    options.Cookie.SameSite = SameSiteMode.Strict; // CSRF protection
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS

    // return 401 without redirecting to login page
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = 401; // unauthorized
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});


builder.Services.AddAuthorization(options =>
{
    // for API endpoints, token is required
    options.AddPolicy("ApiPolicy", policy =>
    {
        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });

    // web needs cookie authentication
    options.AddPolicy("WebPolicy", policy =>
    {
        policy.AuthenticationSchemes.Add("Cookies");
        policy.RequireAuthenticatedUser();
    });

    // for managers only
    options.AddPolicy("ManagerOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("StoreManager");
    });

});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // checks jwt, cookies
app.UseAuthorization(); // check whether user has permission

app.MapControllers();


// seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    DbInitializer.Initialize(context);
}

app.Run();
