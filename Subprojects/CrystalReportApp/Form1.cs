using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrystalReportApp
{
    public partial class Form1 : Form
    {
        public Form1(string[] args)
        {
            InitializeComponent();
            if (args[0]=="Today")
            {
                ReportDocument r = new Report1();
                viewer.ReportSource = r;
            }
            else if (args[0] == "Weekly")
            {
                ReportDocument r = new Report2();
                viewer.ReportSource = r;
            }
            else if (args[0]== "Monthly")
            {
                ReportDocument r = new Report3();
                viewer.ReportSource = r;
            }
            else if(args[0]=="Employees")
            {
                ReportDocument r = new Report4();
                viewer.ReportSource = r;
            }
        }
    }
}
