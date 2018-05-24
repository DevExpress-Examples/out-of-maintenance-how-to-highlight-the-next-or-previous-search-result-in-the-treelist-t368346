using DevExpress.XtraTreeList.Columns;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            SetDataSource();
        }

        private void SetDataSource()
        {
            DataSet set = new DataSet();
            set.ReadXml(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + @"\Contacts.xml");
            treeList1.DataSource = set.Tables[0];
            treeList1.Columns["Description"].Visible = false;
            treeList1.BestFitColumns();
        }
    }
}
