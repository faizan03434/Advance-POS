using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace StationeryStoreManagementSystem.BL
{
    public class Employee : AbstractUser
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string Email { get; set; }
        public double Salary { get; set; }

        public List<object> InitialArgs;
        public Employee(string? firstname = null,
                        string? lastname = null,
                        string? cnic = null,
                        string? contact = null,
                        string gender = null,
                        string? dateofbirth = null,
                        string? town = null,
                        string? city = null,
                        string? streetaddress = null,
                        string? postalcode = null,
                        string? username = null,
                        string? password = null,
                        string? email = null,
                        double? salary = null)
        {
            FirstName = firstname;
            LastName = lastname;
            CNIC = cnic;
            Contact = contact;
            Gender =gender;
            DateOfBirth = dateofbirth;
            Town = town;
            City = city;
            StreetAddress = streetaddress;
            PostalCode = postalcode;
            Username = username;
            Password = password;
            Email = email;
            Salary = salary ?? 0;
        }

        /*public bool VerifyPassword(string password)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
             
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }

            return Password == sb.ToString();
        }*/
        public string DetermineRole(Employee emp)
        {
            if (emp is Cashier)
            {
                return "Cashier";
            }
            else
            {
                return "Admin";
            }
        }
        public override void Save(bool isAdd = false)
        {

        }
    }
}


