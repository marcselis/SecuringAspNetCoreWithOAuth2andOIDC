// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using IdentityModel;
using System.Security.Claims;
using System.Text.Json;
using Duende.IdentityServer;
using Duende.IdentityServer.Test;

namespace Marvin.IDP
{
  public static class TestUsers
  {
      public static List<TestUser> Users
      {
          get
          {
              var address = new
              {
                  street_address = "One Hacker Way",
                  locality = "Heidelberg",
                  postal_code = "69118",
                  country = "Germany"
              };
                
              return
              [
                new TestUser
                {
                  SubjectId = "d860efca-22d9-47fd-8249-791ba61b07c7",
                  Username = "David",
                  Password = "password",
                  Claims =
                  {
                    new Claim("role", "FreeUser"),
                    new Claim(JwtClaimTypes.GivenName, "David"),
                    new Claim(JwtClaimTypes.FamilyName, "Beckham"),
                    new Claim("country", "nl")
                  }
                },

                new TestUser
                {
                  SubjectId = "b7539694-97e7-4dfe-84da-b4256e1ff5c7",
                  Username = "Emma",
                  Password = "password",
                  Claims =
                  {
                    new Claim("role", "PayingUser"),
                    new Claim(JwtClaimTypes.GivenName, "Emma"),
                    new Claim(JwtClaimTypes.FamilyName, "Bale"),
                    new Claim("country", "be")
                  }
                }
              ];
          }
      }
  }
}