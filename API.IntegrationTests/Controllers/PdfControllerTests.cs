using API.Controllers;
using API.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using API.Models.Requests;
using System.Net;

namespace API.Tests.Controllers.Integration;

public class PdfControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PdfControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "FillDocument returns Unauthorized when ApiKey is invalid")]
    public async Task FillDocument_ReturnsUnauthorized_WhenApiKeyIsInvalid()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/pdf/fill"); 
        request.Headers.Add("X-Api-Key", "invalid-api----------key");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.True(response.StatusCode.Equals(HttpStatusCode.Unauthorized));
    }

    [Fact(DisplayName = "FillDocument passes authentication when ApiKey is valid")]
    public async Task FillDocument_ReturnsOK_WhenApiKeyIsValid()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/pdf/fill");
        request.Headers.Add("X-Api-Key", "VALID_API_KEY"); // Use the valid key
        
        // Act
        var response = await _client.SendAsync(request);

        // Assert
        // it will return BadRequest because the request does not contain a file
        // but it means that the authentication was successful
        Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));        
    }

    [Fact(DisplayName = "ReadFields returns Unauthorized when ApiKey is invalid")]
    public async Task ReadFields_ReturnsUnauthorized_WhenApiKeyIsInvalid()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/pdf/fields"); 
        request.Headers.Add("X-Api-Key", "invalid-api----------key");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.True(response.StatusCode.Equals(HttpStatusCode.Unauthorized));
    }

    [Fact(DisplayName = "ReadFields passes authentication when ApiKey is valid")]
    public async Task ReadFields_ReturnsOK_WhenApiKeyIsValid()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/pdf/fields");
        request.Headers.Add("X-Api-Key", "VALID_API_KEY"); // Use the valid key
        
        // Act
        var response = await _client.SendAsync(request);

        // Assert
        // it will return BadRequest because the request does not contain a file
        // but it means that the authentication was successful
        Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));        
    }

}
