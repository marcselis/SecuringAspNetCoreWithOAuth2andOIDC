using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Marvin.IDP
{
  public static class Config
  {
    private const string ImageGalleryApiWriteScope = "imagegalleryapi.write";
    private const string ImageGalleryApiReadScope = "imagegalleryapi.read";
    private const string ImageGalleryApiFullAccessScope = "imagegalleryapi.fullaccess";

    public static IEnumerable<IdentityResource> IdentityResources =>
    [
      new IdentityResources.OpenId(),
      new IdentityResources.Profile(),
      //add roles as a possible scope, a description for the consent screen and the claims that are associated with it.
      new("roles", "Your role(s)", ["role"]),
      //add country as a possible scope, a description for the consent screen and the claims that are associated with it.
      new("country", "The country you're living in", ["country"])
    ];

    public static IEnumerable<ApiResource> ApiResources =>
    [
      //Add the resources we want to access, a description for the consent screen, and the claims we need for this resource
      new("imagegalleryapi", "Image Gallery API", ["role", "country"])
      {
        // also add the scopes that are available for this resource
        Scopes =
        {
          ImageGalleryApiFullAccessScope,
          ImageGalleryApiReadScope,
          ImageGalleryApiWriteScope
        },
        ApiSecrets = { new Secret("apisecret".Sha256())}
      }
    ];

    public static IEnumerable<ApiScope> ApiScopes =>
    [
      new(ImageGalleryApiFullAccessScope),
      new(ImageGalleryApiReadScope),
      new(ImageGalleryApiWriteScope)
    ];

    public static IEnumerable<Client> Clients =>
    [
      new()
      {
        ClientName = "Image Gallery",
        ClientId = "imagegalleryclient",
        AllowedGrantTypes = GrantTypes.Code,
        AccessTokenType=AccessTokenType.Reference, //This client needs to use reference tokens to access APIs
        // IdentityTokenLifetime = 300, // Default lifetime is 5 minutes
        // AuthorizationCodeLifetime = 300, // Default lifetime is 5 minutes
        AccessTokenLifetime = 20, // Default lifetime is 3600 seconds (1 hour)
        AllowOfflineAccess =
          true, // Allow this application to get tokens even if the user is no longer logged-in to the IDP, through refresh tokens.
        // AbsoluteRefreshTokenLifetime = 2592000, // Default absolute lifetime for refresh tokens is 30 days.
        // SlidingRefreshTokenLifetime = 1296000, //Sliding expiration, defaults to 15 days, but can never exceed the absolute refresh token lifetime.
        UpdateAccessTokenClaimsOnRefresh =
          true, //Updates the claims in the access token when a new token is refreshed (defaults to false,
        //which causes the new token to have the same claims (email address, etc...) in the new token,
        //and these might have changed
        RedirectUris =
        {
          "https://localhost:7184/signin-oidc"
        },
        PostLogoutRedirectUris =
        {
          "https://localhost:7184/signout-callback-oidc"
        },
        AllowedScopes = //list the scopes that this client is allowed to request
        {
          IdentityServerConstants.StandardScopes.OpenId,
          IdentityServerConstants.StandardScopes.Profile,
          "roles",
          //ImageGalleryApiFullAccessScope,
          ImageGalleryApiReadScope,
          ImageGalleryApiWriteScope,
          "country"
        },
        ClientSecrets =
        {
          new Secret("secret".Sha256())
        },
        RequireConsent = true
      }
    ];
  }
}