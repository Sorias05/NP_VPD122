using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;
using Newtonsoft.Json;
using ShowUserHttp.dto;

namespace ShowUserHttp
{
    public partial class RegisterForm : Form
    {
        static private string _urlServer = "https://bv012.novakvova.com";
        string errorText = " ";
        public RegisterForm()
        {
            InitializeComponent();
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                          RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on     invalid)
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (isValidForm())
            {
                RegisterUserDTO registerUser = new RegisterUserDTO
                {
                    Email = txtEmail.Text,
                    FirstName = txtFName.Text,
                    SecondName = txtSName.Text,
                    Phone = txtPhone.Text,
                    Password = txtPassword.Text,
                    ConfirmPassword = txtCPassword.Text,
                    Photo = ImageToBase64(txtImage.Text),
                };
                RegisterUser(registerUser);
                Close();
            }
            else
                lblError.Text = errorText;
        }

        private bool isValidForm()
        {
            if (IsValidEmail(txtEmail.Text) && txtImage.Text != "" && txtFName.Text != ""
                && txtSName.Text != "" && txtPhone.Text != "" && txtPassword.Text != ""
                && txtCPassword.Text == txtPassword.Text)
                return true;
            else
            {
                if (txtEmail.Text == "" || txtFName.Text == "" || txtSName.Text == "" || txtPhone.Text == "" || txtPassword.Text == "" || txtCPassword.Text == "")
                    errorText = "Enter all fields!";
                else if (!IsValidEmail(txtEmail.Text))
                    errorText = "Incorrect email!";
                else if (txtCPassword.Text != txtPassword.Text)
                    errorText = "Confirm password!";
                else if (txtImage.Text == "")
                    errorText = "Upload image!";
                return false;
            }   
        }

        private void btnImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = "c:\\";
            dlg.Filter = "Image files (*.PNG;*.JPG;*.GIF)|*.PNG;*.JPG;*.GIF|All Files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtImage.Text = dlg.FileName;
                pImage.Image = new Bitmap(dlg.FileName);
            }
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
                using (var stream = new StreamReader(response.GetResponseStream()))
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
