using System.Net;
using System.Net.Http.Json;
using Asisya.Application.DTOs.Auth;
using Asisya.Application.DTOs.Category;
using Asisya.Application.DTOs.Common;
using Asisya.Application.DTOs.Product;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Asisya.Tests.Integration;

public class ProductApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetJwtTokenAsync()
    {
        var username = $"testuser_{Guid.NewGuid():N}";
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterDto { Username = username, Password = "Test1234!" });

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Username = username, Password = "Test1234!" });

        var token = await response.Content.ReadFromJsonAsync<TokenDto>();
        return token!.Token;
    }

    [Fact]
    public async Task GetProducts_Returns401_WithoutToken()
    {
        var response = await _client.GetAsync("/api/product");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegisterAndLogin_ReturnsValidToken()
    {
        var username = $"user_{Guid.NewGuid():N}";

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterDto { Username = username, Password = "Pass1234!" });
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Username = username, Password = "Pass1234!" });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var token = await loginResponse.Content.ReadFromJsonAsync<TokenDto>();
        Assert.NotNull(token);
        Assert.False(string.IsNullOrWhiteSpace(token.Token));
    }

    [Fact]
    public async Task CreateCategory_AndGetProducts_WithToken()
    {
        var token = await GetJwtTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var categoryResponse = await _client.PostAsJsonAsync("/api/category",
            new CreateCategoryDto { CategoryName = $"CAT_{Guid.NewGuid():N}", Description = "Integración" });
        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);

        var productsResponse = await _client.GetAsync("/api/product?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, productsResponse.StatusCode);

        var paged = await productsResponse.Content.ReadFromJsonAsync<PagedResultDto<ProductDto>>();
        Assert.NotNull(paged);
        Assert.True(paged.TotalCount >= 0);
    }

    [Fact]
    public async Task Login_Returns401_WithWrongPassword()
    {
        var username = $"user_{Guid.NewGuid():N}";
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterDto { Username = username, Password = "Correct1!" });

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Username = username, Password = "Wrong!" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
