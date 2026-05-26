using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StormAndStarfyApi.Data;

namespace StormAndStarfyApi.Tests;

public class AuthApiTests
{
    [Fact]
    public async Task Health_returns_ok_status()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/health");
        var json = await ReadJson(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("ok", json.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Register_creates_user_with_hashed_password_and_returns_token()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var response = await Register(client, login: "  CaptainStorm  ", name: "Storm", password: "secret12");
        var json = await ReadJson(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEqual(string.Empty, json.RootElement.GetProperty("token").GetString());

        var user = json.RootElement.GetProperty("user");
        Assert.Equal("CaptainStorm", user.GetProperty("login").GetString());
        Assert.Equal("Storm", user.GetProperty("name").GetString());
        Assert.False(user.TryGetProperty("totalScore", out _));
        Assert.False(user.TryGetProperty("password", out _));
        Assert.False(user.TryGetProperty("passwordHash", out _));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var savedUser = await db.Users.SingleAsync();
        var passwordHashProperty = savedUser.GetType().GetProperty("PasswordHash");

        Assert.NotNull(passwordHashProperty);
        Assert.NotEqual("secret12", passwordHashProperty!.GetValue(savedUser));
    }

    [Fact]
    public async Task Register_rejects_duplicate_login_case_insensitively()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        await Register(client, login: "Starfy", name: "Starfy", password: "secret12");

        var response = await Register(client, login: "  starfy  ", name: "Other", password: "secret12");
        var json = await ReadJson(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("login", json.RootElement.GetProperty("error").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_returns_token_for_correct_password_and_rejects_wrong_password()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        await Register(client, login: "storm", name: "Storm", password: "secret12");

        var goodResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            login = " storm ",
            password = "secret12"
        });
        var goodJson = await ReadJson(goodResponse);

        Assert.Equal(HttpStatusCode.OK, goodResponse.StatusCode);
        Assert.NotEqual(string.Empty, goodJson.RootElement.GetProperty("token").GetString());

        var badResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            login = "storm",
            password = "wrong12"
        });

        Assert.Equal(HttpStatusCode.BadRequest, badResponse.StatusCode);
    }

    [Fact]
    public async Task Me_requires_valid_jwt_and_returns_current_user()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var registerResponse = await Register(client, login: "starfy", name: "Starfy", password: "secret12");
        var registerJson = await ReadJson(registerResponse);
        var token = registerJson.RootElement.GetProperty("token").GetString();

        var anonymousResponse = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var meResponse = await client.GetAsync("/api/auth/me");
        var meJson = await ReadJson(meResponse);

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        Assert.Equal("starfy", meJson.RootElement.GetProperty("user").GetProperty("login").GetString());
    }

    [Fact]
    public async Task Jwt_contains_expected_user_claims()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var response = await Register(client, login: "storm", name: "Storm", password: "secret12");
        var json = await ReadJson(response);

        var token = new JwtSecurityTokenHandler()
            .ReadJwtToken(json.RootElement.GetProperty("token").GetString());

        Assert.Contains(token.Claims, claim => claim.Type == JwtRegisteredClaimNames.Sub);
        Assert.Contains(token.Claims, claim => claim.Type == "userId");
        Assert.Contains(token.Claims, claim => claim.Type == "login" && claim.Value == "storm");
        Assert.Contains(token.Claims, claim => claim.Type == "name" && claim.Value == "Storm");
    }

    [Fact]
    public async Task Old_game_and_user_routes_are_removed()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var gameResponse = await client.GetAsync("/api/game/getLeaderboard");
        var userResponse = await client.GetAsync("/api/user/getUser?id=1");

        Assert.Equal(HttpStatusCode.NotFound, gameResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, userResponse.StatusCode);
    }

    private static Task<HttpResponseMessage> Register(HttpClient client, string login, string name, string password)
    {
        return client.PostAsJsonAsync("/api/auth/register", new
        {
            login,
            name,
            password
        });
    }

    private static async Task<JsonDocument> ReadJson(HttpResponseMessage response)
    {
        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(stream);
    }
}

public sealed class ApiFactory : WebApplicationFactory<AppDbContext>
{
    private readonly string _databaseName = Guid.NewGuid().ToString("N");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "storm-and-starfy-test-key-with-enough-length",
                ["Jwt:Issuer"] = "StormAndStarfy.Tests",
                ["Jwt:Audience"] = "StormAndStarfy.Client",
                ["Jwt:ExpiresMinutes"] = "60"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }
}
