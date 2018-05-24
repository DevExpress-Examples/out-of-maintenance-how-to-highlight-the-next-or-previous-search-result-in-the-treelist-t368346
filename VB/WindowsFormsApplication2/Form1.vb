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
			Dim [set] As New DataSet()
			[set].ReadXml(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName & "\Contacts.xml")
			treeList1.DataSource = [set].Tables(0)
			treeList1.Columns("Description").Visible = False
			treeList1.BestFitColumns()
		End Sub
	End Class
End Namespace
