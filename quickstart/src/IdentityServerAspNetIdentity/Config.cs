using IdentityServer4.Models;
using IdentityServer4;
using System.Collections.Generic;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("api1", "My API")
        };


    public static IEnumerable<ApiResource> GetApis()
    {
        return new ApiResource[]
        {
             new ApiResource("apiCatalog", "Test API")
            {
                 Scopes = { "api1" },
                 ApiSecrets = new List<Secret>
                    {
                        new Secret("Secret".Sha256())
                    }
            }

        };
    }

    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            // machine to machine client
            new Client
            {
                ClientId = "client",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                // scopes that client has access to
                AllowedScopes = { "api1" }
            },

            // interactive ASP.NET Core MVC client
            new Client
            {
                ClientId = "mvc",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                // where to redirect to after login
                RedirectUris = { "https://localhost:5002/signin-oidc" },

                // where to redirect to after logout
                PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },
                 AllowOfflineAccess = true,

                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "api1"
                }
            },
              // MVC client using code flow + pkce
              new Client
                {
                    ClientId = "ro.client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AccessTokenLifetime = 3600 * 48,
                    AccessTokenType = AccessTokenType.Reference,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowOfflineAccess = true,
                    AllowedCorsOrigins = {"https://localhost:6001" },
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowedScopes = { "identity", "api1", "openid", "profile", "catalog", "webhooks", "price" }
                },
        };
}