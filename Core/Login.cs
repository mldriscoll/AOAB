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

        public static string ConsoleGetUsername()
        {
            Console.WriteLine("Please enter your j-novel club username");
            return Console.ReadLine()!;

        }

        public static string ConsoleGetPassword()
        {
            Console.WriteLine("Please enter your j-novel club password");

            // Copied from https://stackoverflow.com/questions/3404421/password-masking-console-application
            var pass = string.Empty;
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    Console.Write("\b \b");
                    pass = pass[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    pass += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);

            return pass;
        }

        public static async Task PersistLoginInfo(string username, string password)
        {
            await File.WriteAllTextAsync("Account.txt", $"{username}\r\n{Convert.ToBase64String(ToCiphertext(password))}");
        }

        public static Tuple<string, string>? RetrieveLoginInfo()
        {
            if (!File.Exists("Account.txt"))
            {
                return null;
            }
            else
            {
                var text = File.ReadAllText("Account.txt");
                var split = text.Split("\r\n");

                if (split.Length != 2)
                {
                    return null;
                }
                else
                {
                    return new(split[0], FromCiphertext(Convert.FromBase64String(split[1])));
                }
            }
        }

        public static async Task<Login?> FromConsole(HttpClient client)
        {
            Console.Clear();
            Console.WriteLine("Creating a user account file");
            Console.WriteLine();
            string username = ConsoleGetUsername();
            string password = ConsoleGetPassword();

            Login? login = await CreateLogin(username, password, client);
            if (login != null)
            {
                await PersistLoginInfo(username, password);
            }

            return login;
        }

        public static async Task<Login?> FromUI(HttpClient client, string username, string password)
        {
            var login = await CreateLogin(username, password, client);
            if (login != null)
            {
                await PersistLoginInfo(username, password);
            }

            return login;
        }

        public static async Task<Login?> FromFile(HttpClient client)
        {
            var loginInfo = RetrieveLoginInfo();
            if (loginInfo != null)
            {
                return await CreateLogin(loginInfo.Item1, loginInfo.Item2, client);
            }
            else
            {
                return null;
            }
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
