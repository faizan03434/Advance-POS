using StationeryStoreManagementSystem.DL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace StationeryStoreManagementSystem.BL
{
    public class Customer:AbstractUser
    {
        public double PaymentDues {  get; set; }
        public List<object> InitialArgs;
        public Customer(string? firstname = null,
                        string? lastname = null,
                        string? cnic = null,
                        string? contact = null,
                        string gender = null,
                        string? dateofbirth = null,
                        string? town = null,
                        string? city = null,
                        string? streetaddress = null,
                        string? postalcode = null,
                        double? paymentdues = null)
        {
            FirstName = firstname;
            LastName = lastname;
            CNIC = cnic;
            Contact = contact;
            Gender = gender;
            DateOfBirth = dateofbirth;
            Town = town;
            City = city;
            StreetAddress = streetaddress;
            PostalCode = postalcode;
            PaymentDues = paymentdues ?? 0;
        }
        public Customer(List<object> args)
        {
            Id = (int)args[0];
            FirstName = (string?)args[1];
            LastName = (string?)args[2];
            Gender = (string?)args[3];
            CNIC = (string?)args[4];
            DateOfBirth = (string?)args[5]?.ToString();
            Contact = (string?)args[6];
            City = (string?)args[7];
            Town = (string?)args[8];
            StreetAddress = (string?)args[9];
            PostalCode = (string?)args[10];
            PaymentDues = args[11] == null ? 0 : Convert.ToDouble(args[11].ToString());
            InitialArgs = args;
            InitialArgs.RemoveAt(0);
        }
        public override void Save(bool isAdd = false)
        {
            CustomerDL.SaveCustomer(this, isAdd);
        }
    }
}
