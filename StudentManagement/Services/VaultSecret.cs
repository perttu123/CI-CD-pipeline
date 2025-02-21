using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Collections.Concurrent;

namespace StudentManagement.Services
{
    public class VaultSecret
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public interface IKeyVaultSecretManager
    {
        Task<VaultSecret> GetSecretAsync(string secretName);
        Task RefreshSecretsAsync();
    }

    public interface IKeyVaultConfigurationManager
    {
        Task<IDictionary<string, string>> GetAllSecretsAsync();
    }

    public class KeyVaultSecretManager : IKeyVaultSecretManager
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<KeyVaultSecretManager> _logger;
        private readonly SecretClient _secretClient;
        private static readonly ConcurrentDictionary<string, VaultSecret> _secretsCache = new ConcurrentDictionary<string, VaultSecret>();

        public KeyVaultSecretManager(IConfiguration configuration, ILogger<KeyVaultSecretManager> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var keyVaultEndpoint = _configuration["KeyVault:BaseUrl"];

            _secretClient = new SecretClient(new Uri(keyVaultEndpoint), new DefaultAzureCredential());
        }

        public async Task<VaultSecret> GetSecretAsync(string secretName)
        {
            if (_secretsCache.TryGetValue(secretName, out VaultSecret cachedSecret))
            {
                return cachedSecret;
            }

            return await FetchAndCacheSecretAsync(secretName);
        }

        public async Task RefreshSecretsAsync()
        {
            try
            {
                await foreach (var secretProperty in _secretClient.GetPropertiesOfSecretsAsync())
                {
                    await FetchAndCacheSecretAsync(secretProperty.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while refreshing the secrets from Key Vault.");
            }
        }

        private async Task<VaultSecret> FetchAndCacheSecretAsync(string secretName)
        {
            try
            {
                var response = await _secretClient.GetSecretAsync(secretName);
                var secret = new VaultSecret { Name = secretName, Value = response.Value.Value };
                _secretsCache.AddOrUpdate(secretName, secret, (key, oldValue) => secret);
                return secret;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to fetch secret {secretName} from Key Vault.");
                return null;
            }
        }
    }
}