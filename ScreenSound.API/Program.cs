using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ScreenSound.API.Endpoints;
using ScreenSound.Banco;
using ScreenSound.Modelos;
using ScreenSound.Shared.Dados.Modelos;
using ScreenSound.Shared.Modelos.Modelos;
using System.Data.SqlTypes;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ScreenSoundContext>((options) => {
    options
            .UseSqlServer(builder.Configuration["ConnectionStrings:ScreenSoundDB"])
            .UseLazyLoadingProxies();
});

// Adiciona os serviços essenciais do ASP.NET Core Identity API para a classe customizada de usuário 'PessoaComAcesso'
builder.Services.AddIdentityApiEndpoints<PessoaComAcesso>()
    /* Configura o Entity Framework Core como mecanismo de armazenamento/persistência do Identity, 
     indicando que o 'ScreenSoundContext' é o banco de dados responsável por salvar as tabelas de usuários */
    .AddEntityFrameworkStores<ScreenSoundContext>();

builder.Services.AddAuthorization();

builder.Services.AddTransient<DAL<Artista>>();
builder.Services.AddTransient<DAL<Musica>>();
builder.Services.AddTransient<DAL<Genero>>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddCors(
    options => options.AddPolicy(
        "wasm",
        policy => policy.WithOrigins([builder.Configuration["BackendUrl"] ?? "https://localhost:7089",
            builder.Configuration["FrontendUrl"] ?? "https://localhost:7015"])
            .AllowAnyMethod()
            .SetIsOriginAllowed(pol => true)
            .AllowAnyHeader()
            .AllowCredentials()));


var app = builder.Build();

app.UseCors("wasm");

app.UseStaticFiles();

app.UseAuthorization();

app.AddEndPointsArtistas();
app.AddEndPointsMusicas();
app.AddEndPointGeneros();

// Mapeia o grupo de endpoints do Identity sob o prefixo "/auth" (ex: /auth/register, /auth/login)
app.MapGroup("auth")
    // Cria e expõe automaticamente as rotas nativas de autenticação (cadastro, login, etc.) para o tipo 'PessoaComAcesso'
    .MapIdentityApi<PessoaComAcesso>()
    // Agrupa visualmente todos esses endpoints na interface do Swagger sob a tag "Autorização"
    .WithTags("Autorização");

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
