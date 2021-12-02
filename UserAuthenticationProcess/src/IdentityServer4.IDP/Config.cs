// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentityServer4.IDP
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                //These are mapped to claims of the user such as first name and last name.
                //These are subclaim and also known as user identifier.
                new IdentityResources.OpenId(),
                new IdentityResources.Profile() //Given Name and Family Name

            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            { };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                //Configuring Identity Server to login with Authorization Code Flow
                new Client
                {
                    ClientName = "Image Gallery",
                    ClientId = "imagegalleryclient",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequireConsent = true,
                    RequirePkce = true,
                    RedirectUris = new List<string>
                    {
                        //sigin-oidc is the default value. This can be configured at web client.
                        //URL of the imagegalleryclient
                        "https://localhost:44389/signin-oidc"
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile
                    },
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    #region Redirect URI after logging out
                    PostLogoutRedirectUris = new List<string>
                    {
                        //sigin-oidc is the default value. This can be configured at web client.
                        //URL of the imagegalleryclient
                        "https://localhost:44389/signout-callback-oidc"

                    }
                    #endregion
                    
                }

            };
    }
}