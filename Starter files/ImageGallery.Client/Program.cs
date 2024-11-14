using ImageGallery.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
  .AddJsonOptions(configure =>
    configure.JsonSerializerOptions.PropertyNamingPolicy = null);
// Do not map incoming claim types to the old Microsoft WS-security claim names.  This ensures that the names of the incoming claims are identical to the ones that the IDP issued.
JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();
// Register the required services for access token management
builder.Services.AddAccessTokenManagement();
// create an HttpClient used for accessing the API
builder.Services.AddHttpClient("APIClient", client =>
{
  client.BaseAddress = new Uri(builder.Configuration["ImageGalleryAPIRoot"]);
  client.DefaultRequestHeaders.Clear();
  client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
}).AddUserAccessTokenHandler(); // ensures that the access token is passed to each call to the API
builder.Services.AddHttpClient("IDPClient", client =>
{
  client.BaseAddress = new Uri("https://localhost:5001/");
});
// Configure authentication
builder.Services.AddAuthentication(options =>
  {
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
  })
  .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
  {
    options.AccessDeniedPath = "/Authentication/AccessDenied";
  })
  .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme,
    options =>
    {
#pragma warning disable S125
      options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
      options.Authority = "https://localhost:5001/";
      options.ClientId = "imagegalleryclient";
      options.ClientSecret = "secret";
      options.ResponseType = "code";
      // List all scopes that this client will request from the IDP
      // These are the default scopes
      // options.Scope.Add("openid");
      // options.Scope.Add("profile");
      options.Scope.Add("roles");
      // options.Scope.Add("imagegalleryapi.fullaccess");
      options.Scope.Add("imagegalleryapi.read");
      options.Scope.Add("imagegalleryapi.write");
      options.Scope.Add("country");
      options.Scope.Add("offline_access");  //Makes this client request refresh tokens to have long-lived access
      // options.CallbackPath = new PathString("signin-oidc");  // Callback to receive token.  This is the default value.
      // options.SignedOutCallbackPath = new PathString("signout-callback-oidc"); // Callback after signing out, this is the default value.4
      options.SaveTokens=true; // Allow middleware to save tokens it receives, so they can be used afterward.
      //Configure that we want to call the IDP's UserInfo endpoint to get claims, and what to do with them
      options.GetClaimsFromUserInfoEndpoint = true; // Call the IDP's UserInfo endpoint to get the claims associated with the scopes that were requested (otherwise the profile claims will not be available in the client)
      options.ClaimActions.Remove("aud"); //Remove the claim filter for the 'aud' claim.  That filter removes the claim from the list of incoming claims, making it unavailable in our application.
      options.ClaimActions.DeleteClaim("sid");  // Register a new claim filter to delete the incoming sid claim, as we don't want to store it in our cookie
      options.ClaimActions.DeleteClaim("idp");  // Register a new claim filter to delete the incoming idp claim, as we don't want to store it in our cookie
      options.ClaimActions.MapJsonKey("role","role"); // Copy all role claims in our cookie
      options.ClaimActions.MapUniqueJsonKey("country", "country"); // Copy the first country claim to our cookie.
      options.TokenValidationParameters = new TokenValidationParameters
      {
        NameClaimType = "given_name",
        RoleClaimType = "role"
      };
#pragma warning restore S125
    });

// Add Attribute-based Authorization Policies
builder.Services.AddAuthorization(authorizationOptions =>
{
  authorizationOptions.AddPolicy(PolicyNames.UserCanAddImage, AuthorizationPolicies.CanAddImage());
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler();
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
//Add authentication to the request pipeline
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
  "default",
  "{controller=Gallery}/{action=Index}/{id?}");

app.Run();