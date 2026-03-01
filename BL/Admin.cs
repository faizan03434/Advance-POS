using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StationeryStoreManagementSystem.DL;

namespace StationeryStoreManagementSystem.BL
{
    public class Admin : Employee
    {
        public Admin(string? firstname = null,
                        string? lastname = null,
                        string? cnic = null,
                        string? contact = null,
                        string? gender = null,
                        string? dateofbirth = null,
                        string? town = null,
                        string? city = null,
                        string? streetaddress = null,
                        string? postalcode = null,
                        string? username = null,
                        string? password = null,
                        string? email = null,
                        double? salary = null) : base(firstname, lastname, cnic, contact, gender, dateofbirth, town, city, streetaddress, postalcode, username, password, email, salary)
        {

        }
        public Admin(Employee E)
        {
            Id = E.Id;
            Username = E.Username;
            Password = E.Password;
            FirstName = E.FirstName;
            LastName = E.LastName;
            Gender = E.Gender;
            CNIC = E.CNIC;
            DateOfBirth = E.DateOfBirth;
            Contact = E.Contact;
            Email = E.Email;
            City = E.City;
            Town = E.Town;
            StreetAddress = E.StreetAddress;
            PostalCode = E.PostalCode;
            Salary = E.Salary;
            InitialArgs = E.InitialArgs;
        }
        public Admin(List<object> args)
        {
            Id = (int)args[0];
            FirstName = (string?)args[1];
            LastName = (string?)args[2];
            Gender = (string?)args[3];
            CNIC = (string?)args[4];
            DateOfBirth = (string?)args[5]?.ToString();
            Contact = (string?)args[6];
            Email = (string?)args[7];
            City = (string?)args[8];
            Town = (string?)args[9];
            StreetAddress = (string?)args[10];
            PostalCode = (string?)args[11];
            Salary = double.Parse(args[12].ToString());
            InitialArgs = args;
            InitialArgs.RemoveAt(0);
        }
        public override void Save(bool isAdd = false)
        {
            EmployeeDL.SaveEmployee(this, isAdd);
        }
    }
}
