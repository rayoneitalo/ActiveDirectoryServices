using Microsoft.OpenApi.Models;

using api_ldap.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

var API_VERSION = builder.Configuration.GetSection("API_VERSION").Value;
var arrAPI_VERSION = API_VERSION.Split('.');

String appsettingsEnvironment = builder.Configuration.GetSection("Parametros:Local").Value;

String typeServer = Environment.GetEnvironmentVariable("TYPE_SERVER");


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "_cors",
                      policy =>
                      {
                          policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
                      });
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(actions =>
{
    actions.SwaggerDoc(
        $"v{API_VERSION.Split('.')[0]}",
        new OpenApiInfo
        {
            Title = "LDAP API",
            Version = $"v{API_VERSION}"
        }
    );

    actions.EnableAnnotations();
    actions.OperationFilter<CustomHeaderSwaggerAttribute>();
    
});

var app = builder.Build();

//Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

if (appsettingsEnvironment != "Production")
{
    app.UsePathBase(new PathString("/h"));
}

if (typeServer == "KUBERNETES")
{
    app.UseSwagger();

    app.UseSwaggerUI();
}
else
{
    app.UseSwagger(options =>
{
    options.RouteTemplate = "ldap/swagger/{documentname}/swagger.json";
});

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint($"/ldap/swagger/v{arrAPI_VERSION[0]}/swagger.json", $"LDAP API{API_VERSION}");
        options.RoutePrefix = "ldap/swagger";
    });

}

app.UseCors("_cors");

app.UseAuthorization();

app.MapControllers();

app.Run();


public class PathPrefixInsertDocumentFilter : IDocumentFilter
{
    private readonly string _pathPrefix;

    public PathPrefixInsertDocumentFilter(string prefix)
    {
        this._pathPrefix = prefix;
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var paths = swaggerDoc.Paths.Keys.ToList();
        foreach (var path in paths)
        {
            var pathToChange = swaggerDoc.Paths[path];
            swaggerDoc.Paths.Remove(path);
            swaggerDoc.Paths.Add($"{_pathPrefix}{path}", pathToChange);
        }
    }

}
