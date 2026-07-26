using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;
using Reciplex.Server.Host.Options;

namespace Reciplex.Server.Host.AccessControl;

/// <summary>
/// Extensions to configure data protection
/// </summary>
public static class DataProtectionWebApplicationBuilderExtensions
{
    /// <summary>
    /// Configures data protection
    /// </summary>
    /// <param name="builder">app builder</param>
    /// <returns>builder with data protection configured based on the application options</returns>
    /// <exception cref="InvalidOperationException">thrown if <see cref="DataProtectionOptions.InsecureDisableEncryption"/> is false and a certificate was not supplied.</exception>
    public static WebApplicationBuilder ConfigureDataProtection(this WebApplicationBuilder builder)
    {
        DataProtectionOptions? configOptions = builder
            .Configuration.GetSection(DataProtectionOptions.SectionPath)
            .Get<DataProtectionOptions>();

        IDataProtectionBuilder dpBuilder = builder.Services.AddDataProtection();

        if (!string.IsNullOrWhiteSpace(configOptions?.KeyStorageDirectory))
        {
            dpBuilder = dpBuilder.PersistKeysToFileSystem(
                new DirectoryInfo(configOptions.KeyStorageDirectory)
            );
        }

        if (
            !string.IsNullOrWhiteSpace(configOptions?.EncryptionCertificate)
            && !string.IsNullOrWhiteSpace(configOptions?.EncryptionPrivateKey)
        )
        {
            X509Certificate2 x509Cert = X509Certificate2.CreateFromPemFile(
                configOptions.EncryptionCertificate,
                configOptions.EncryptionPrivateKey
            );

            dpBuilder.ProtectKeysWithCertificate(x509Cert);
        }
        else if (!(configOptions?.InsecureDisableEncryption ?? false))
        {
            throw new InvalidOperationException(
                "no encryption certificate provided to protect data protect keys. If this is intentional then disable encryption."
            );
        }

        return builder;
    }
}
