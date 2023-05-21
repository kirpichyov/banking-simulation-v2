using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Banking.Simulation.Api.Middleware;
using Banking.Simulation.Api.Swagger;
using Banking.Simulation.Application;
using Banking.Simulation.Application.Validators;
using Banking.Simulation.Core.Options;
using Banking.Simulation.DataAccess;
using FluentValidation;
using Kirpichyov.FriendlyJwt.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

namespace Banking.Simulation.Api;

public class Startup
{
	private readonly IConfiguration _configuration;
	private readonly IWebHostEnvironment _environment;

	public Startup(IConfiguration configuration, IWebHostEnvironment environment)
	{
		_configuration = configuration;
		_environment = environment;
	}

	public void ConfigureServices(IServiceCollection services)
	{
		services.AddHttpClient();
		services.AddHttpContextAccessor();

		services.AddFriendlyJwt();
		services.Configure<AuthOptions>(_configuration.GetSection(nameof(AuthOptions)));

		services.AddDataAccessServices(_configuration, _environment);
		services.AddApplicationServices(_configuration);
		services.AddJobs(_configuration);
		services.AddBroker();

		services.AddRouting(options => options.LowercaseUrls = true);

		services.AddApiVersioning(options =>
		{
			options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
			options.AssumeDefaultVersionWhenUnspecified = true;
			options.ReportApiVersions = true;

			options.ApiVersionReader = ApiVersionReader.Combine(
				new UrlSegmentApiVersionReader(),
				new HeaderApiVersionReader("x-api-version"),
				new MediaTypeApiVersionReader("x-api-version")
			);
		});

		services.AddCors(options =>
		{
			options.AddDefaultPolicy(policy =>
			{
				policy.AllowAnyOrigin();
				policy.AllowAnyHeader();
				policy.AllowAnyMethod();
			});
		});

		services.AddControllers()
			.AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
			})
			.AddFriendlyJwtAuthentication(configuration =>
			{
				var authOptions = _configuration.GetSection(nameof(AuthOptions)).Get<AuthOptions>();
				configuration.Bind(authOptions);
			})
			.AddMvcOptions(options =>
			{
				options.Filters.Add<ExceptionFilter>();
			});

		ValidatorOptions.Global.LanguageManager.Enabled = false;
		services.AddValidatorsFromAssemblyContaining<SignInRequestValidator>();

		services.AddVersionedApiExplorer(options =>
		{
			options.GroupNameFormat = "'v'VVV";
			options.SubstituteApiVersionInUrl = true;
		});
		
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen(options =>
		{
			options.AddSecurityDefinition("Bearer",
				new OpenApiSecurityScheme
				{
					Name = HeaderNames.Authorization,
					Type = SecuritySchemeType.ApiKey,
					In = ParameterLocation.Header,
					Description = "Obtained JWT."
				});

			options.MapType<DateOnly>(() => new OpenApiSchema()
			{
				Type = "string",
				Format = "date",
			});
			
			options.OperationFilter<AuthOperationFilter>();
		});

		services.ConfigureOptions<ConfigureSwaggerOptions>();
	}

	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		app.UseSwagger();
		app.UseSwaggerUI(options =>
		{
			var apiVersionDescriptionProvider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
				
			foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.Reverse())
			{
				options.SwaggerEndpoint(
					$"/swagger/{description.GroupName}/swagger.json", 
					description.GroupName.ToUpperInvariant()
				);
			}
		});

		// app.UseHttpsRedirection();
		app.UseRouting();

		app.UseCors();
		
		app.UseAuthentication();
		app.UseAuthorization();

		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllers();

			endpoints.MapGet("/ping",
				async context => { await context.Response.WriteAsync($"Pong! [{DateTime.UtcNow}]"); }
			);
		});
	}
}
