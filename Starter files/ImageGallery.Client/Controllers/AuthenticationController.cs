using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace ImageGallery.Client.Controllers
{
  public class AuthenticationController : Controller
  {
    private readonly IHttpClientFactory _httpClientFactory;
    public AuthenticationController(IHttpClientFactory httpClientFactory)
    {
      _httpClientFactory = httpClientFactory;
    }

    [Authorize]
    public async Task Logout()
    {
      var client = _httpClientFactory.CreateClient("IDPClient");
      // Discover the Revocation Endpoint address
      var discoveryDocumentResponse = await client.GetDiscoveryDocumentAsync().ConfigureAwait(false);
      if (discoveryDocumentResponse.IsError)
      {
        throw new Exception(discoveryDocumentResponse.Error);
      }
      // Revoke the Access Token
      var accessTokenRevocationResponse = await client.RevokeTokenAsync(new TokenRevocationRequest
      {
        Address = discoveryDocumentResponse.RevocationEndpoint,
        ClientId = "imagegalleryclient",
        ClientSecret = "secret",
        Token = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken)
      }).ConfigureAwait(false);
      if (accessTokenRevocationResponse.IsError)
      {
        throw new Exception(accessTokenRevocationResponse.Error);
      }
      // Revoke the Refresh Token
      var refreshTokenRevocationResponse = await client.RevokeTokenAsync(new TokenRevocationRequest
      {
        Address = discoveryDocumentResponse.RevocationEndpoint,
        ClientId = "imagegalleryclient",
        ClientSecret = "secret",
        Token = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken).ConfigureAwait(false)
      }).ConfigureAwait(false);
      if (refreshTokenRevocationResponse.IsError)
      {
        throw new Exception(refreshTokenRevocationResponse.Error);
      }

      // Clear local authentication cookie
      await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
      // Redirect to the IDP linked to the "OpenIdConnectDefaults.AuthenticationScheme" (oidc) so that it can clear its own session & cookie as well.
      await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme).ConfigureAwait(false);
    }

    public IActionResult AccessDenied()
    {
      return View();
    }
  }
}
