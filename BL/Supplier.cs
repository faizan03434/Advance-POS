using StationeryStoreManagementSystem.DL;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationeryStoreManagementSystem.BL
{
    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Contact { get; set; }
        public string? Email { get; set; }
        public string? StreetAddress { get; set; }
        public string? Town { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public List<Product> Products { get; set; }
        public List<object> InitialArgs;
        public Supplier(int id):this()
        {
            Id = id;
        }
        public Supplier()
        {
            Id = -1;
            InitialArgs = new List<object>();
        }
        public Supplier(string name
                       , string code
                       , string contact
                       , string? email = null
                       , string? streetAddress = null
                       , string? town = null
                       , string? city = null
                       , string? country = null
                       , string? postalCode = null
                       , List<Product> products = null) : this()

        {
            Name = name;
            Code = code;
            Contact = contact;
            Email = email;
            StreetAddress = streetAddress;
            Town = town;
            City = city;
            Country = country;
            PostalCode = postalCode;
            Products = products;
        }
        public Supplier(List<object> args)
        {
            Id = (int)args[0];
            Name = (string?)args[1];
            Code = (string?)args[2];
            Contact = (string?)args[3];
            Email = (string?)args[4];
            StreetAddress = (string?)args[5];
            Town = (string?)args[6];
            City = (string?)args[7];
            Country = (string?)args[8];
            PostalCode = (string?)args[9];
            if (args.Count < 11)
                Products = new List<Product>();
            else
                Products = (List<Product>?)args[10];
            InitialArgs = args;
            InitialArgs.RemoveAt(0);
        }
        public void Save(bool isAdd = false)
        {
            SupplierDL.SaveSupplier(this, isAdd);
        }
    }
}
