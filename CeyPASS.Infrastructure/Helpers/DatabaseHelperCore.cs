using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace CeyPASS.Infrastructure.Helpers
{
    public static class DatabaseHelperCore
    {
        private static readonly string ENCRYPTION_KEY = "MySecretKey12345MySecretKey12345"; // 32 karakter
        private static readonly string ENCRYPTION_IV = "1234567890123456"; // 16 karakter
        private const string ENCRYPTED_PASSWORD = "BGyIDNpaLNh015WPAB8/kw==";

        public static string GetSqlConnectionString(string applicationName = "CeyPASS")
        {
            try
            {
                string password = DecryptAES(ENCRYPTED_PASSWORD);

                var sqlBuilder = new SqlConnectionStringBuilder
                {
                    DataSource = @"192.168.0.23\SQLEXPRESS01",
                    InitialCatalog = "CeyPASS",
                    UserID = "sa",
                    Password = password,
                    IntegratedSecurity = false,
                    MultipleActiveResultSets = true,
                    Encrypt = true,
                    TrustServerCertificate = true,
                    ApplicationName = applicationName,
                    PersistSecurityInfo = true
                };

                return sqlBuilder.ConnectionString;
            }
            catch (Exception ex)
            {
                throw new Exception("Veritabanı bağlantısı oluşturulamadı: " + ex.Message);
            }
        }

        private static string DecryptAES(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(ENCRYPTION_KEY);
                aes.IV = Encoding.UTF8.GetBytes(ENCRYPTION_IV);
                using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
