using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;

namespace Core
{
    public class Login
    {
        public readonly string AccessToken;
        private Login(string accessToken)
        {
            AccessToken = accessToken;
        }

        public static byte[] ToCiphertext(string password)
        {
            byte[] plaintextPassword = Encoding.ASCII.GetBytes(password)!;
            return ProtectedData.Protect(plaintextPassword, null, DataProtectionScope.CurrentUser );
        }
        
        public static string FromCiphertext(byte[] ciphertextBuffer)
        {
            byte[] plaintextBuffer = ProtectedData.Unprotect(ciphertextBuffer, null, DataProtectionScope.CurrentUser);
            return Encoding.ASCII.GetString(plaintextBuffer);
        }
        
        public static async Task<Login?> FromConsole(HttpClient client)
        {
            Console.Clear();
            Console.WriteLine("Creating a user account file");
            Console.WriteLine();
            Console.WriteLine("Please enter your j-novel club username");
            var un = Console.ReadLine()!;

            Console.WriteLine("Please enter your j-novel club password");
            var pass = Console.ReadLine()!;

            string username = un!;
            string password = pass!;

            File.WriteAllText("Account.txt", username);
            File.WriteAllBytes("Password.txt", ToCiphertext(password));
            
            return await CreateLogin(un!, ct, client);
        }

        public static async Task<Login?> FromUI(HttpClient client, string username, string password)
        {
            var login = await CreateLogin(username, password, client);
            if (login != null)
            {
                File.WriteAllText("Account.txt", username);
                File.WriteAllBytes("Password.txt", ToCiphertext(password));
            }

            return login;
        }

        public static async Task<Login?> FromFile(HttpClient client)
        {
            if (!File.Exists("Account.txt") || !File.Exists("Password.txt"))
            {
                return null;
            }

            string username = File.ReadAllText("Account.txt")!;
            byte[] ct = File.ReadAllBytes("Password.txt");
            
            return await CreateLogin(username, FromCiphertext(ct), client);
        }

        private static async Task<Login?> CreateLogin(string username, string plaintextPassword, HttpClient client)
        {
            try
            {
                var loginCall = await client.PostAsync("https://labs.j-novel.club/app/v2/auth/login?format=json", new StringContent($"{{\"login\":\"{username}\",\"password\":\"{plaintextPassword}\",\"slim\":true}}", System.Text.Encoding.ASCII, "application/json"));

                string bearerToken;
                using (var loginStream = await loginCall.Content.ReadAsStreamAsync())
                {
                    var deserializer = new DataContractJsonSerializer(typeof(LoginResponse));
                    bearerToken = (deserializer.ReadObject(loginStream) as LoginResponse)!.id;
                }

                return new Login(bearerToken);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return null;
            }

        }
        public class LoginResponse
        {
            public string id { get; set; } = string.Empty;
        }
    }
}
