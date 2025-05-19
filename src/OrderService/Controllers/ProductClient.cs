using PrivateUtilities.Models;
public class ProductClient
{
    private readonly HttpClient _httpClient;

    public ProductClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ProductDto>> GetProductsByIdsAsync(List<int> ids)
    {
        var idsQuery = string.Join(",", ids);
        var fullUrl = $"http://productservice/api/product?ids={idsQuery}";
        var response = await _httpClient.GetFromJsonAsync<List<ProductDto>>(fullUrl);
        return response ?? new List<ProductDto>();
    }

    public async Task<List<ProductDto>> GetProductsAsync()
    {
        var fullUrl = $"http://productservice/api/product";
        var response = await _httpClient.GetFromJsonAsync<List<ProductDto>>(fullUrl);
        return response ?? new List<ProductDto>();
    }

    public async Task<bool> HasEnoughStock(int productId, int amount)
    {
        var fullUrl = $"http://productservice/api/product/HasEnoughStock/{productId}/{amount}";
        var response = await _httpClient.GetFromJsonAsync<bool>(fullUrl);
        return response;
    }

    public async Task<bool> UpdateStock(int productId, int amount)
    {
        var fullUrl = $"http://productservice/api/product/{productId}";
        var body = new StringContent(amount.ToString(), System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PatchAsync(fullUrl, body);
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error updating stock: {errorMessage}");
        }
        return response.IsSuccessStatusCode;
    }
}
