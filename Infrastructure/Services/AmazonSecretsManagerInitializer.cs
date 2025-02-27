using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public static class AmazonSecretsManagerInitializer
{
    public static void AddAmazonSecretsManager(this IConfigurationBuilder configurationBuilder,
                        string region,
                        string secretName)
    {
        var configurationSource =
                new AmazonSecretsManagerConfigurationSource(region, secretName);

        configurationBuilder.Add(configurationSource);
    }
}
