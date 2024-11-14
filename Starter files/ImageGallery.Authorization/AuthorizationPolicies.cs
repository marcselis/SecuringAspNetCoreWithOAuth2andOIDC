using Microsoft.AspNetCore.Authorization;

namespace ImageGallery.Authorization
{
  public static class PolicyNames
  {
    public const string UserCanAddImage = "UserCanAddImage";
  }

  public static class AuthorizationPolicies
  {
    public static AuthorizationPolicy CanAddImage()
    {
      return new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireClaim("country", "be")
        .RequireRole("PayingUser")
        .Build();
    }
  }
}
