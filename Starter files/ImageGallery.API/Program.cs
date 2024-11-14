using ImageGallery.API.Auhtorization;
using ImageGallery.API.DbContexts;
using ImageGallery.API.Services;
using ImageGallery.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
  .AddJsonOptions(configure => configure.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddDbContext<GalleryContext>(options =>
{
  options.UseSqlite(
    builder.Configuration["ConnectionStrings:ImageGalleryDBConnectionString"]);
});

// register the repository
builder.Services.AddScoped<IGalleryRepository, GalleryRepository>();
builder.Services.AddHttpContextAccessor(); //this service is required by the MustOwnImageHandler below
builder.Services.AddScoped<IAuthorizationHandler, MustOwnImageHandler>();

// register AutoMapper-related services
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  // Code for JWT Bearer tokens, read from configuration
  //.AddJwtBearer(); 
  // Code for JWT Bearer tokens, configured in code
  //.AddJwtBearer(options =>
  //{
  //  options.Authority = "https://localhost:5001";
  //  options.Audience = "imagegalleryapi";
  //  options.TokenValidationParameters = new TokenValidationParameters
  //  {
  //    NameClaimType = "given_name",
  //    RoleClaimType = "role",
  //    ValidTypes = new[] { "at+jwt" } //only allow at+jwt tokens, mitigation for the JWT Algorithm Confusion Attack.
  //  };
  //});
  // Code for Reference tokens
  .AddOAuth2Introspection(options =>
  {
    options.Authority = "https://localhost:5001";
    options.ClientId = "imagegalleryapi";
    options.ClientSecret = "apisecret";
    options.NameClaimType = "given_name";
    options.RoleClaimType = "role";
  });

// Add Attribute-based Authorization Policies
builder.Services.AddAuthorization(authorizationOptions =>
{
  //user-based ABAC policies
  authorizationOptions.AddPolicy(PolicyNames.UserCanAddImage, AuthorizationPolicies.CanAddImage());
  //client-based ABAC policies
  authorizationOptions.AddPolicy("ClientApplicationCanWrite", policyBuilder =>
  {
    // require the client to have the "imagegallery.write" scope claim, to pass this policy
    policyBuilder.RequireClaim("scope", "imagegalleryapi.write");
  });
  authorizationOptions.AddPolicy("MustOwnImage", policyBuilder =>
  {
    policyBuilder.RequireAuthenticatedUser();
    policyBuilder.AddRequirements(new MustOwnImageRequirement());
  });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();