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
                //Given Name and Family Name
                new IdentityResources.Profile(),
                //Getting ready for calling the UserInfo endpoint
                new IdentityResources.Address(),
                //Role scope is not defined as one of the standard in the OpenIDConnect scopes.
                new IdentityResource(
                    "roles",            //Scope
                    "Your role(s)",     //Display name
                    new List<string>() { "role"}),//List of claims that application should return when application asks for this role scope.
                new IdentityResource(
                    "country",
                    "The country you're living in",
                    new List<string>() { "country"}),
                new IdentityResource(
                    "subscriptionlevel",
                    "Your subscription level",
                    new List<string>() { "subscriptionlevel"})                    
            };

        public static IEnumerable<ApiScope> ApiScopes =>
           new ApiScope[]
           {
               new ApiScope("imagegalleryapi", 
                            "Image Gallery API",
                            new List<string>() { "role" })
           };

        public static IEnumerable<ApiResource> Apis =>
            new ApiResource[]
            {
                new ApiResource("imagegalleryapi", "Image Gallery API")
                {
                    Scopes = new []{ "imagegalleryapi" },
                    ApiSecrets = { new Secret("apisecret".Sha256()) }
                }
            };
        
        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                //Configuring Identity Server to login with Authorization Code Flow
                new Client
                {
                    //IdentityTokenLifetime
                    //AuthorizationCodeLifetime
                    //AbsoluteRefreshTokenLifetime
                    //RefreshTokenExpiration


                    #region Reference Tokens

                    AccessTokenType = AccessTokenType.Reference,

                    #endregion

                    #region Supporting refresh tokens

                    AccessTokenLifetime = 120,
                    AllowOfflineAccess = true,
                    UpdateAccessTokenClaimsOnRefresh = true,

                    #endregion

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
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Address,                        
                        "roles",
                        "imagegalleryapi",
                        "country",
                        "subscriptionlevel"
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