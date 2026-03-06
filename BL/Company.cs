using StationeryStoreManagementSystem.DL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationeryStoreManagementSystem.BL
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<object> InitialArgs;
        public Company(string name = null)
        {
            Name = name;
        }
        public Company(List<object> args)
        {
            Id = (int)args[0];
            Name = (string)args[1];
            InitialArgs = args;
            InitialArgs.RemoveAt(0);
        }
        public void Save(bool isAdd = false)
        {
            CompanyDL.SaveCompany(this, isAdd);
        }
    }
}
