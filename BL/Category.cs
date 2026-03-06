using StationeryStoreManagementSystem.DL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationeryStoreManagementSystem.BL
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double? GST { get; set; }

        public List<object> InitialArgs;
        public Category(string? name = null, double? gst = 16)
        {
            Name = name;
            GST = gst;
        }
        public Category(List<object> args)
        {
            Id = (int)args[0];
            Name = (string)args[1];
            GST = (double?)args[2];
            InitialArgs = args;
            InitialArgs.RemoveAt(0);
        }
        public void Save(bool isAdd = false)
        {
            CategoryDL.SaveCategory(this, isAdd);
        }
    }
}
