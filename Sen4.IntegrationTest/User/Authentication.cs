using System.Net;
using System.Net.Http.Json;
using Infrastructure.DTO;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Sen4.IntegrationTests;
using Shouldly;

namespace Sen4.IntegrationTest.User;

public class Authentication(IntegrationTestWebAppFactory integrationTestWebAppFactory)
    : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _httpClient = integrationTestWebAppFactory.CreateClient();
    
    [Fact(DisplayName = "Success registration with valid data")]
    public async Task ValidData_SuccessRegistration()
    {
        //act 
        var response = await _httpClient.PostAsJsonAsync("user/:register", new UserRegistrationDTO()
        {
            Email = "validDataSuccessRegistration@gmail.com",
            Name = "Test",
            Surname = "Test",
            MiddleName = "Test",
            PostId = new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),
            Password = "JohnStringPassword_Kj8_Dn3456_ty5&"
        });
        var identityResultFromResponse = await response.Content.ReadFromJsonAsync<IdentityResult?>();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        identityResultFromResponse.ShouldNotBeNull();
    }
    
    [Fact(DisplayName = "Fail registration with already exist email")]
    public async Task WithAlreadyExistEmail_FailRegistration()
    {
        var body = new UserRegistrationDTO()
        {
            Email = "alreadyExistedEmail@gmail.com",
            Name = "Test",
            Surname = "Test",
            MiddleName = "Test",
            PostId = new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),
            Password = "JohnStringPassword_Kj8_Dn3456_ty5&"
        };
        await _httpClient.PostAsJsonAsync("user/:register",body );
        var response = await _httpClient.PostAsJsonAsync("user/:register",body );
        
        var identityResultFromResponse = await response.Content.ReadFromJsonAsync<IdentityResult?>();
        
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        identityResultFromResponse.ShouldNotBeNull();
        identityResultFromResponse.Succeeded.ShouldBe(false);
    }
    
    [Theory(DisplayName = "Fail registration with invalid data")]
    [MemberData(nameof(AuthTestData.InvalidDataForRegistration), MemberType = typeof(AuthTestData))]
    public async Task InvalidData_FailRegistration(UserRegistrationDTO body)
    {
        //act 
        var response = await _httpClient.PostAsJsonAsync("user/:register", body);
        var identityResultFromResponse = await response.Content.ReadFromJsonAsync<IdentityResult?>();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        identityResultFromResponse.ShouldNotBeNull();
        identityResultFromResponse.Succeeded.ShouldBe(false);
    }
    
    [Fact(DisplayName = "Success login with valid data")]
    public async Task ValidData_SuccessLogin()
    {
        //act
        await _httpClient.PostAsJsonAsync("user/:register", new UserRegistrationDTO()
        {
            Email = "validDataSuccessLogin@gmail.com",
            Name = "Test",
            Surname = "Test",
            MiddleName = "Test",
            PostId = new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),
            Password = "JohnStringPassword_Kj8_Dn3456_ty5&"
        });
        
        var response = await _httpClient.PostAsJsonAsync("user/:login", new LoginDTO()
        {
            Email = "validDataSuccessLogin@gmail.com",
            Password = "JohnStringPassword_Kj8_Dn3456_ty5&"
        });
        
        var tokensFromResponse = await response.Content.ReadFromJsonAsync<RefreshAccessToken>();
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        tokensFromResponse.ShouldNotBeNull();
        tokensFromResponse.AccessToken.ShouldNotBeNullOrEmpty();
        tokensFromResponse.RefreshToken.ShouldNotBeNullOrEmpty();
    }
    
    [Theory(DisplayName = "Fail login with invalid data")]
    [MemberData(nameof(AuthTestData.InvalidDataForLogin), MemberType = typeof(AuthTestData))]
    public async Task InvalidData_FailLogin(LoginDTO loginBody)
    {
        //act
        var response = await _httpClient.PostAsJsonAsync("user/:login", loginBody);
        
        //assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    
    [Fact(DisplayName = "Success update refresh token with valid data")]
    public async Task SuccessUpdateRefreshTokenWithValidData()
    {
        //arrange
        await _httpClient.PostAsJsonAsync("user/:register", new UserRegistrationDTO()
        {
            Email = "successUpdateRefresh@gmail.com",
            Name = "John",
            Surname = "Doe",
            MiddleName = "Gallager",
            PostId = new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),
            Password = "JohnStringPassword_Kj8_Dn3456_ty5&"
        });
        
        var loginResponse = await _httpClient.PostAsJsonAsync("user/:login", new LoginDTO()
        {
            Email = "successUpdateRefresh@gmail.com",
            Password = "JohnStringPassword_Kj8_Dn3456_ty5&"
        });
        
        var tokensFromLoginResponse = await loginResponse.Content.ReadFromJsonAsync<RefreshAccessToken>();
        
        //act
        var refreshResponse = await _httpClient.PostAsJsonAsync("user/:refresh", tokensFromLoginResponse);
        var accessTokenFromRefreshResponse = await refreshResponse.Content.ReadAsStringAsync();
    
        //assert
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        accessTokenFromRefreshResponse.ShouldNotBeNullOrEmpty();
    }
    
    [Fact(DisplayName = "Fail refresh token with invalid access token")]
    public async Task FailRefreshTokenWithInvalidAccessToken()
    {
        //arrange
        var a = await _httpClient.PostAsJsonAsync("user/:register", new UserRegistrationDTO()
        {
            Email = "failRefreshToken-invalidAccess@gmail.com",
            Name = "John",
            Surname = "Doe",
            MiddleName = "Gallager",
            PostId = new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),
            Password = "JohnStringPassword_Kj8_Dn3456_ty5&"
        });
        
        var loginResponse = await _httpClient.PostAsJsonAsync("user/:login", new LoginDTO()
        {
            Email = "failRefreshToken-invalidAccess@gmail.com",
            Password = "JohnStringPassword_Kj8_Dn3456_ty5&"
        });
        
        var tokensFromLoginResponse = await loginResponse.Content.ReadFromJsonAsync<RefreshAccessToken>();;
        tokensFromLoginResponse.AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
    
        //act
        var refreshResponse = await _httpClient.PostAsJsonAsync("user/:refresh", tokensFromLoginResponse);
    
        //assert
        refreshResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
    
    [Fact(DisplayName = "Fail refresh token with invalid refresh token")]
    public async Task FailRefreshTokenWithInvalidRefreshToken()
    {
        //arrange
        var a = await _httpClient.PostAsJsonAsync("user/:register", new UserRegistrationDTO()
        {
            Email = "failRefreshToken-invalidRefresh@gmail.com",
            Name = "John",
            Surname = "Doe",
            MiddleName = "Gallager",
            PostId = new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),
            Password = "JohnStringPassword_Kj8_Dn3456_ty5&"
        });
        
        var loginResponse = await _httpClient.PostAsJsonAsync("user/:login", new LoginDTO()
        {
            Email = "failRefreshToken-invalidRefresh@gmail.com",
            Password = "JohnStringPassword_Kj8_Dn3456_ty5&"
        });
        
        var tokensFromLoginResponse = await loginResponse.Content.ReadFromJsonAsync<RefreshAccessToken>();;
        tokensFromLoginResponse.RefreshToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
    
        //act
        var refreshResponse = await _httpClient.PostAsJsonAsync("user/:refresh", tokensFromLoginResponse);
    
        //assert
        refreshResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}