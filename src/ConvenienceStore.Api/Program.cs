using ConvenienceStore.Api.Infrastructure;
using ConvenienceStore.Api.Services;
using ConvenienceStore.Application;
using ConvenienceStore.Application.Common.Interfaces;
using ConvenienceStore.Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Convenience Store Chain API",
        Version = "v1",
        Description = "Enterprise Retail ERP & Web POS Backend Solution"
    });

    // Thêm cấu hình JWT Bearer Authorization
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập Access Token JWT theo định dạng: Bearer {token}"
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
            Array.Empty<string>()
        }
    });
});

// Đăng ký ProblemDetails & GlobalExceptionHandler của .NET 8
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddApplication();

// Đăng ký HttpContextAccessor và CurrentUserService
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Đăng ký tầng Infrastructure (EF Core, Interceptor, DbContext)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Sử dụng Exception Handler ở đầu HTTP Pipeline
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
