using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace RISKHackTool
{
    public class KeyData
    {
        public string Key { get; set; }
        public string Duration { get; set; }
        public bool IsActivated { get; set; }
        public DateTime? ActivationDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string HardwareHash { get; set; }
        public bool IsValid { get; set; }
        public string ActivatedBy { get; set; }
        public int DurationDays { get; set; }
    }

    public class KeyStatus
    {
        public bool IsValid { get; set; }
        public bool IsActivated { get; set; }
        public bool IsExpired { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public double RemainingDays { get; set; }
        public string Message { get; set; }
    }

    internal class LicenseApiResponse
    {
        [JsonPropertyName("status")] public string Status { get; set; }
        [JsonPropertyName("message")] public string Message { get; set; }
        [JsonPropertyName("expiresAt")] public DateTime? ExpiresAt { get; set; }
        [JsonPropertyName("activatedAt")] public DateTime? ActivatedAt { get; set; }
        [JsonPropertyName("remainingDays")] public double? RemainingDays { get; set; }
        [JsonPropertyName("durationDays")] public int? DurationDays { get; set; }
        [JsonPropertyName("signature")] public string Signature { get; set; }
    }

    public class KeyValidator
    {
        // =========================================================
        // CONFIG — replace both values
        // =========================================================
        private const string ApiBase = "https://risk-license-server-production-415f.up.railway.app";
        private const string ResponseHmacKey = "Y+HJdvL2GPE9VDk2i1mMXyigijzKy7rj0ET4TJJbFP3Lirj6AMJKGXrgBoVUthGT";

        private const string KeyCachePath = "key.cache";

        private static readonly HttpClient _http = CreateClient();
        private static readonly object _lock = new object();

        private static KeyValidator _instance;
        private bool _isValidated = false;
        private KeyData _currentActivation = null;

        public static KeyValidator Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null) _instance = new KeyValidator();
                    }
                }
                return _instance;
            }
        }

        private KeyValidator() { }

        private static HttpClient CreateClient()
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol |=
                    System.Net.SecurityProtocolType.Tls12;
            }
            catch { }

            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                UseCookies = false
            };
            var c = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
            c.DefaultRequestHeaders.Add("User-Agent", "RISKHackTool/1.0");
            return c;
        }

        // =========================================================
        // PUBLIC API
        // =========================================================

        public async Task<KeyStatus> ValidateKeyAsync(string key, string username = "")
        {
            if (string.IsNullOrWhiteSpace(key))
                return Invalid("Please enter a license key");

            string hwid = GetHardwareHash();

            LicenseApiResponse api;
            try
            {
                var payload = JsonSerializer.Serialize(new
                {
                    key = key.Trim(),
                    hwid = hwid,
                    user = username ?? ""
                });

                using var content = new StringContent(payload, Encoding.UTF8, "application/json");
                using var resp = await _http.PostAsync(ApiBase + "/api/license/validate", content);
                string body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                    return Invalid($"Server error ({(int)resp.StatusCode})");

                api = JsonSerializer.Deserialize<LicenseApiResponse>(body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (api == null)
                    return Invalid("Malformed server response");
            }
            catch (TaskCanceledException)
            {
                return Invalid("License server timed out");
            }
            catch
            {
                return Invalid("Cannot reach license server");
            }

            // Verify signature on VALID responses only (server doesn't sign failures)
            string statusLower = (api.Status ?? "").ToLowerInvariant();
            if (statusLower == "valid" && !VerifySignature(api))
                return Invalid("Response signature invalid");

            switch (statusLower)
            {
                case "valid":
                    _isValidated = true;
                    SaveCachedKey(key);
                    _currentActivation = new KeyData
                    {
                        Key = key.Trim(),
                        Duration = api.DurationDays == 9999 ? "Lifetime" : $"{api.DurationDays}Days",
                        DurationDays = api.DurationDays ?? 0,
                        IsActivated = true,
                        ActivationDate = api.ActivatedAt,
                        ExpirationDate = api.ExpiresAt,
                        HardwareHash = hwid,
                        IsValid = true,
                        ActivatedBy = string.IsNullOrEmpty(username) ? "User" : username
                    };
                    return new KeyStatus
                    {
                        IsValid = true,
                        IsActivated = true,
                        IsExpired = false,
                        ExpirationDate = api.ExpiresAt,
                        RemainingDays = api.RemainingDays ?? 0,
                        Message = api.Message ?? "Key is active"
                    };

                case "expired":
                    _isValidated = false;
                    return new KeyStatus
                    {
                        IsValid = false,
                        IsActivated = true,
                        IsExpired = true,
                        ExpirationDate = api.ExpiresAt,
                        Message = api.Message ?? "Key expired"
                    };

                case "revoked":
                    _isValidated = false;
                    SaveCachedKey(null);
                    return Invalid("This key has been revoked");

                case "hwid_mismatch":
                    _isValidated = false;
                    return Invalid("This key is already bound to another device");

                default:
                    _isValidated = false;
                    return Invalid(api.Message ?? "Invalid license key");
            }
        }

        public bool IsValidated() => _isValidated;
        public KeyData GetCurrentActivation() => _currentActivation;

        public bool TryBootstrap(out string reason)
        {
            reason = null;

            string cached;
            try { cached = LoadCachedKey(); }
            catch (Exception ex) { reason = "LoadCachedKey: " + ex.Message; return false; }

            if (string.IsNullOrWhiteSpace(cached))
            {
                reason = "No cached license key";
                return false;
            }

            KeyStatus status;
            try
            {
                // Task.Run is required: calling an async method from the UI thread
                // via GetAwaiter().GetResult() would deadlock.
                status = Task.Run(() => ValidateKeyAsync(cached)).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                reason = "ValidateKeyAsync: " + ex.Message;
                return false;
            }

            if (status == null || !status.IsValid)
            {
                reason = status?.Message ?? "null status";
                SaveCachedKey(null);
                return false;
            }
            return true;
        }
        // =========================================================
        // KEY CACHE (local, only stores the key string)
        // =========================================================

        public string LoadCachedKey()
        {
            try
            {
                if (System.IO.File.Exists(KeyCachePath))
                    return System.IO.File.ReadAllText(KeyCachePath).Trim();
            }
            catch { }
            return null;
        }

        public void SaveCachedKey(string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    if (System.IO.File.Exists(KeyCachePath))
                        System.IO.File.Delete(KeyCachePath);
                    return;
                }
                System.IO.File.WriteAllText(KeyCachePath, key.Trim());
            }
            catch { }
        }

        // =========================================================
        // HWID (WMI)
        // =========================================================

        public string GetHardwareHash()
        {
            string combined = GetHardwareInfo();
            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(combined));
            var sb = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash) sb.Append(b.ToString("X2"));
            return sb.ToString();
        }

        private string GetHardwareInfo()
        {
            // Mix several stable machine identifiers. No WMI, no System.Management.
            string machineName = "";
            string userName = "";
            string osVersion = "";
            string machineGuid = "";

            try { machineName = Environment.MachineName ?? ""; } catch { }
            try { userName = Environment.UserName ?? ""; } catch { }
            try { osVersion = Environment.OSVersion?.VersionString ?? ""; } catch { }

            // MachineGuid is created by Windows at install and survives user account changes.
            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Cryptography");
                if (key != null)
                {
                    var v = key.GetValue("MachineGuid");
                    machineGuid = v?.ToString() ?? "";
                }
            }
            catch { }

            return $"{machineName}|{userName}|{osVersion}|{machineGuid}";
        }

        // =========================================================
        // SIGNATURE VERIFICATION
        // =========================================================

        private bool VerifySignature(LicenseApiResponse r)
        {
            if (string.IsNullOrEmpty(r.Signature)) return false;

            string expires = r.ExpiresAt.HasValue
                ? r.ExpiresAt.Value.ToUniversalTime().ToString("o")
                : "";
            string remaining = r.RemainingDays?.ToString("R") ?? "";
            string duration = r.DurationDays?.ToString() ?? "";
            string canonical = $"{r.Status}|{expires}|{remaining}|{duration}";

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(ResponseHmacKey));
            byte[] mac = hmac.ComputeHash(Encoding.UTF8.GetBytes(canonical));
            string expected = BitConverter.ToString(mac).Replace("-", "");

            return FixedTimeEquals(expected, r.Signature);
        }

        private static bool FixedTimeEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }

        private static KeyStatus Invalid(string message)
            => new KeyStatus { IsValid = false, IsActivated = false, IsExpired = false, Message = message };
    }
}