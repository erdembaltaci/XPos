using System.Net.Http.Headers;
using System.Net.Http.Json;
using XPos.Shared.DTOs;

namespace XPos.Mobile.Services;

public class AuthStateService
{
    private readonly HttpClient _http;
    public bool IsLoggedIn { get; private set; }
    public string WaiterName { get; private set; } = string.Empty;
    public StaffDto? CurrentUser { get; private set; }
    public string Role { get; private set; } = string.Empty;
    public string? JwtToken { get; private set; }

    public AuthStateService(HttpClient http)
    {
        _http = http;
    }

    public async Task<bool> TryLogin(string phone, string loginPassword)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", new LoginDto
            {
                Phone = phone,
                AuthPassword = loginPassword
            });

            if (!response.IsSuccessStatusCode)
                return false;

            var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            if (auth == null || string.IsNullOrEmpty(auth.Token))
                return false;

            // JWT token'ı sakla ve HttpClient header'ına ekle
            JwtToken = auth.Token;
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", auth.Token);

            // SecureStorage'a kaydet (MAUI)
            try
            {
                await SecureStorage.SetAsync("jwt_token", auth.Token);
                await SecureStorage.SetAsync("staff_role", auth.Role);
                await SecureStorage.SetAsync("staff_name", auth.FullName);
                await SecureStorage.SetAsync("staff_id", auth.StaffId.ToString());
            }
            catch { /* SecureStorage bazı platformlarda çalışmayabilir */ }

            CurrentUser = new StaffDto
            {
                Id = auth.StaffId,
                Name = auth.FullName.Split(' ').FirstOrDefault() ?? auth.FullName,
                Surname = auth.FullName.Split(' ').Skip(1).FirstOrDefault() ?? "",
                Role = auth.Role,
                IsActive = true
            };
            WaiterName = auth.FullName;
            Role = auth.Role;
            IsLoggedIn = true;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login Exception: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Uygulama başlangıcında SecureStorage'dan token yükle
    /// </summary>
    public async Task TryRestoreSession()
    {
        try
        {
            var token = await SecureStorage.GetAsync("jwt_token");
            if (string.IsNullOrEmpty(token)) return;

            // Token geçerlilik süresi kontrolü (basit JWT parse)
            var parts = token.Split('.');
            if (parts.Length != 3) return;

            var payload = parts[1];
            // Base64 padding
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            var doc = System.Text.Json.JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("exp", out var expEl))
            {
                var exp = expEl.GetInt64();
                var expTime = DateTimeOffset.FromUnixTimeSeconds(exp);
                if (expTime <= DateTimeOffset.UtcNow) return; // Süresi dolmuş
            }

            JwtToken = token;
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var role = await SecureStorage.GetAsync("staff_role") ?? "";
            var name = await SecureStorage.GetAsync("staff_name") ?? "";
            var idStr = await SecureStorage.GetAsync("staff_id") ?? "0";

            Role = role;
            WaiterName = name;
            IsLoggedIn = true;
            CurrentUser = new StaffDto
            {
                Id = int.TryParse(idStr, out var id) ? id : 0,
                Name = name.Split(' ').FirstOrDefault() ?? name,
                Surname = name.Split(' ').Skip(1).FirstOrDefault() ?? "",
                Role = role,
                IsActive = true
            };
        }
        catch { /* SecureStorage erişim hatası */ }
    }

    public void Logout()
    {
        IsLoggedIn = false;
        WaiterName = string.Empty;
        Role = string.Empty;
        CurrentUser = null;
        JwtToken = null;
        _http.DefaultRequestHeaders.Authorization = null;

        try
        {
            SecureStorage.Remove("jwt_token");
            SecureStorage.Remove("staff_role");
            SecureStorage.Remove("staff_name");
            SecureStorage.Remove("staff_id");
        }
        catch { }
    }

    public bool IsAdmin => Role is "Admin";
    public bool IsAdminOrSubAdmin => Role is "Admin" or "SubAdmin";
}

