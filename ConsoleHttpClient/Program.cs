
using ConsoleHttpClient.dto;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace ConsoleHttpClient
{
    class Program
    {
        static private string _urlServer = "https://bv012.novakvova.com";
        public static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            var user = new RegisterUserDTO();
            Console.Write("First name: ");
            user.FirstName = Console.ReadLine();
            Console.Write("Second name: ");
            user.SecondName = Console.ReadLine();
            Console.Write("Email: ");
            user.Email = Console.ReadLine();
            Console.Write("Phone: ");
            user.Phone = Console.ReadLine();
            Console.Write("Password: ");
            user.Password = Console.ReadLine();
            Console.Write("Confirm password: ");
            user.ConfirmPassword = Console.ReadLine();
            Console.Write("Image: ");
            string image = Console.ReadLine();
            user.Photo = ImageToBase64(image);

            RegisterUser(user);
            ReadData();
        }

        private static string ImageToBase64(string path)
        {
            using (System.Drawing.Image image = System.Drawing.Image.FromFile(path))
            {
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();
                    string base64String = Convert.ToBase64String(imageBytes);
                    return base64String;
                }
            }
        }
        private static void ReadData()
        {
            WebRequest request = WebRequest.Create($"{_urlServer}/api/account/users");
            request.Method = "GET";
            request.ContentType = "application/json";
            try
            {
                var response = request.GetResponse();
                using(var stream = new StreamReader(response.GetResponseStream()))
                {
                    string data = stream.ReadToEnd();
                    var users = JsonConvert.DeserializeObject<List<UserItemDTO>>(data);
                    foreach(var user in users)
                    {
                        Console.WriteLine("First name: {0}", user.FirstName);
                        Console.WriteLine("Second name: {0}", user.SecondName);
                        Console.WriteLine("Email: {0}", user.Email);
                        Console.WriteLine("Phone: {0}", user.Phone);
                        Console.WriteLine("Photo: {0}", user.Photo);
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private static void RegisterUser(RegisterUserDTO registerUser)
        {
            string json = JsonConvert.SerializeObject(registerUser);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            WebRequest request = WebRequest.Create($"{_urlServer}/api/account/register");
            request.Method = "POST";
            request.ContentType = "application/json";
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }
            try
            {
                var response = request.GetResponse();
                using(var stream = new StreamReader(response.GetResponseStream()))
                {
                    string data = stream.ReadToEnd();
                    Console.WriteLine(data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
