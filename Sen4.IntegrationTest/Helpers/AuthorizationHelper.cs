using System.Net.Http.Headers;
using System.Net.Http.Json;
using Infrastructure.DTO;
using Infrastructure.Models;

namespace Sen4.IntegrationTests.Helpers;

public class AuthorizationHelper(HttpClient httpClient)
{
    private const string registerUrl = "user/:register";
    private const string loginUrl = "user/:login";

    public HttpResponseMessage RegisterUserAsync(string email, string name, string surname, string middleName, Guid postId, string password)
    {
        return  httpClient.PostAsJsonAsync(registerUrl, new UserRegistrationDTO()
        {
            Email = email,
            Name = name,
            Surname = surname,
            MiddleName = middleName,
            PostId = postId,
            Password = password
        }).Result;
    }

    public async Task<HttpResponseMessage?> LoginUserAsync(string email, string password)
    {
        var response = await httpClient.PostAsJsonAsync(loginUrl, new LoginDTO()
        {
            Email = email,
            Password = password
        });
        var tokensFromResponse =  response.Content.ReadFromJsonAsync<RefreshAccessToken>().Result;
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensFromResponse?.AccessToken);

        return response;
    }
}