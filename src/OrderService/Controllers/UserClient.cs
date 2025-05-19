using PrivateUtilities.Models;

public class UserClient
{
    private readonly HttpClient _httpClient;

    public UserClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    string _baseUrl = "http://userservice/api/user";
    public async Task<User> GetUserByIdAsync(int id)
    {
        var fullUrl = _baseUrl +  $"/{id}";
        var response = await _httpClient.GetFromJsonAsync<User>(fullUrl);
        return response ?? new User();
    }

    public async Task<List<User>> GetUsersAsync()
    {
        var fullUrl = _baseUrl;
        var response = await _httpClient.GetFromJsonAsync<List<User>>(fullUrl);
        return response ?? new List<User>();
    }

    public async Task<bool> HasEnoughBalanceAsync(int userId, decimal totalPrice)
    {
        var fullUrl = _baseUrl + $"/hasEnoughBalance/{userId}/{totalPrice}";
        var response = await _httpClient.GetFromJsonAsync<bool>(fullUrl);
        return response;
    }
    public async Task<bool> UpdateBalanceAsync(int userId, decimal amount)
    {
        var fullUrl = _baseUrl + $"/{userId}";
        var body = new StringContent(amount.ToString(), System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PatchAsync(fullUrl, body);
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error updating balance: {errorMessage}");
        }
        return response.IsSuccessStatusCode;
    }
}