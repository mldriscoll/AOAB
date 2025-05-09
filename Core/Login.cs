﻿using System.Runtime.Serialization.Json;

namespace Core
{
    public class Login
    {
        public readonly string UserName;
        public readonly string Password;
        public readonly string AccessToken;
        private Login(string username, string password, string accessToken)
        {
            UserName = username;
            Password = password;
            AccessToken = accessToken;
        }

        public static async Task<Login> FromConsole(HttpClient client)
        {
            Console.Clear();
            Console.WriteLine("Creating a user account file");
            Console.WriteLine();
            Console.WriteLine("Please enter your j-novel club username");
            var un = Console.ReadLine();

            Console.WriteLine("Please enter your j-novel club password");
            var pass = Console.ReadLine();

            File.WriteAllText("Account.txt", $"{un}\r\n{pass}");
            return await CreateLogin(un, pass, client);
        }

        public static async Task<Login> FromUI(HttpClient client, string username, string password)
        {
            var login = await CreateLogin(username, password, client);
            if (login != null)
            {
                File.WriteAllText("Account.txt", $"{username}\r\n{password}");
            }

            return login;
        }

        public static async Task<Login> FromFile(HttpClient client)
        {
            if (!File.Exists("Account.txt"))
            {
                return null;
            }

            var text = File.ReadAllText("Account.txt");
            var split = text.Split("\r\n");
            if (split.Length == 2)
            {
                return await CreateLogin(split[0], split[1], client);
            }

            return null;
        }

        private static async Task<Login> CreateLogin(string username, string password, HttpClient client)
        {
            try
            {
                var loginCall = await client.PostAsync("https://labs.j-novel.club/app/v2/auth/login?format=json", new StringContent($"{{\"login\":\"{username}\",\"password\":\"{password}\",\"slim\":true}}", System.Text.Encoding.ASCII, "application/json"));

                string bearerToken;
                using (var loginStream = await loginCall.Content.ReadAsStreamAsync())
                {
                    var deserializer = new DataContractJsonSerializer(typeof(LoginResponse));
                    bearerToken = (deserializer.ReadObject(loginStream) as LoginResponse).id;
                }

                return new Login(username, password, bearerToken);
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
