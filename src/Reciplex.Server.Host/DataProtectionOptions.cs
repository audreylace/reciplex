namespace Reciplex.Server.Host.AccessControl;

/// <summary>
/// Configures data protection for cookies and other sensitive data sent to the client.
/// </summary>
public class DataProtectionOptions
{
    /// <summary>
    /// Section path
    /// </summary>
    public const string SectionPath = "Reciplex:DataProtection";

    /// <summary>
    /// The path to the certificate that will be used to encrypt and decrypt data protection keys.
    /// </summary>
    public string EncryptionCertificate { get; set; } = "";

    /// <summary>
    /// Set to true to disable encryption of data protection keys at rest
    /// </summary>
    public bool InsecureDisableEncryption { get; set; }

    /// <summary>
    /// The directory to store data protection keys. If not provided, the default directory is used.
    /// </summary>
    public string KeyDirectory { get; set; } = "";
}
