using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using System;
using System.Reflection;
using System.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API",
        Version = "v1",
        Description = $"The Api Service"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows()
        {
            Password = new OpenApiOAuthFlow()
            {
                AuthorizationUrl = new Uri($"{builder.Configuration["identityUrl"]}/connect/authorize"),
                TokenUrl = new Uri($"{builder.Configuration["identityUrlExternal"]}/connect/token"),
                Scopes = new Dictionary<string, string>()
                {
                    { "api1", "api1 scrope" }
                }
            }
        }
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
        }
    });


    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});


//builder.Services.AddAuthentication("Bearer")
//                .AddJwtBearer("Bearer", options =>
//                {
//                    // Use our IS4 implementation as the authentication source.
//                    options.Authority = "https://localhost:5001";
//                    options.RequireHttpsMetadata = false;
//                    options.Audience = "api";
//                });

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddIdentityServerAuthentication(options =>
{
    options.Authority = "https://localhost:5001";
    //options.Authority = "https://apijjph.hqsoft.vn/identityserver";
    options.ApiName = "apiCatalog";
    options.ApiSecret = "Secret";
    options.RequireHttpsMetadata = false;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        //policy.RequireClaim("scope", "api1");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        //c.SwaggerEndpoint($"{(!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty)}/swagger/v1/swagger.json", "Catalog.API V1.01");
        c.OAuthClientId("ro.client");
        c.OAuthClientSecret("secret");
        c.OAuthAppName("Api Swagger UI");
        c.OAuthScopes("api1");
    });
}

#region Middleware
app.Use(async (context, next) =>
{
    if (context.Request.QueryString.HasValue)
    {
        if (string.IsNullOrWhiteSpace(context.Request.Headers["Authorization"]))
        {
            var queryString = HttpUtility.ParseQueryString(context.Request.QueryString.Value);
            string token = queryString.Get("access_token");

            if (!string.IsNullOrWhiteSpace(token))
            {
                context.Request.Headers.Add("Authorization", new[] { string.Format("Bearer {0}", token) });
            }
        }
    }
    await next.Invoke();
});
#endregion

app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers().RequireAuthorization("ApiScope"); ;
});
app.MapDefaultControllerRoute();
app.MapControllers();
app.Run();
