Imports DevExpress.XtraTreeList.Columns
Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.IO
Imports System.Linq
Imports System.Windows.Forms

Namespace WindowsFormsApplication2
	Partial Public Class Form1
		Inherits Form

		Public Sub New()
			InitializeComponent()
			SetDataSource()
		End Sub

		Private Sub SetDataSource()
			Dim dset As DataSet = New DataSet()
            dset.ReadXml(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName & "\Contacts.xml")
            treeList1.DataSource = dset.Tables(0)
			treeList1.Columns("Description").Visible = False
			treeList1.BestFitColumns()
		End Sub
	End Class
End Namespace
