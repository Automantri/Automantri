using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Automantri.Infrastructure.External.ApiNinjas;

internal static class ApiNinjasHttp
{
    public static async Task<T?> SendAsync<T>(
        HttpClient httpClient,
        string apiKey,
        string url,
        CancellationToken cancellationToken)
    {
        var response = await TrySendAsync<T>(httpClient, apiKey, url, cancellationToken);
        if (response.IsSuccess)
        {
            return response.Data;
        }

        throw new HttpRequestException(
            $"API Ninjas request failed ({(int)response.StatusCode} {response.StatusCode}) for {url}. {response.ErrorBody}");
    }

    public static async Task<ApiNinjasHttpResult<T>> TrySendAsync<T>(
        HttpClient httpClient,
        string apiKey,
        string url,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("X-Api-Key", apiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return new ApiNinjasHttpResult<T>(false, response.StatusCode, default, errorBody);
        }

        var data = await response.Content.ReadFromJsonAsync<T>(ApiNinjasJson.SerializerOptions, cancellationToken);
        return new ApiNinjasHttpResult<T>(true, response.StatusCode, data, null);
    }
}

internal sealed record ApiNinjasHttpResult<T>(
    bool IsSuccess,
    HttpStatusCode StatusCode,
    T? Data,
    string? ErrorBody);
