Imports DevExpress.Utils.Paint
Imports DevExpress.XtraEditors
Imports DevExpress.XtraEditors.Controls
Imports DevExpress.XtraEditors.Repository
Imports DevExpress.XtraEditors.ViewInfo
Imports DevExpress.XtraTreeList
Imports DevExpress.XtraTreeList.Columns
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

Namespace WindowsFormsApplication2
	Public Class FindHelper
		Inherits Component

		Public Sub New()
			BackGroundColor = Color.Green
			HighLightColor = Color.Gold
		End Sub

		Private edit As ButtonEdit
		Private treeList As TreeList
		Private filterCellText As String = String.Empty
		Private findList As New Dictionary(Of TreeCell, Boolean)()
		Private ReadOnly Property FindListIsEmpty() As Boolean
			Get
				Return findList.Keys.Count = 0
			End Get
		End Property
		Public Property BackGroundColor() As Color
		Public Property HighLightColor() As Color
		Private showResult As EditorButton

		Public Property TargetControl() As TreeList
			Get
				Return treeList
			End Get
			Set(ByVal value As TreeList)
				SubscibeTreeListEvent(False)
				treeList = value
				SubscibeTreeListEvent(True)
			End Set
		End Property

		Public Property SearchControl() As ButtonEdit
			Get
				Return edit
			End Get
			Set(ByVal value As ButtonEdit)
				SubscribeRIEvent(False)
				edit = value
				ResetActiveRI()
			End Set
		End Property

		Private Sub ResetActiveRI()
			If DesignMode Then
				Return
			End If
			edit.Properties.Buttons.Clear()
			showResult = New EditorButton(ButtonPredefines.Glyph, "0 of 0", 0, False, True, False, ImageLocation.MiddleCenter, Nothing, New DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.None), Nothing, "", Nothing, Nothing, True)
			edit.Properties.Buttons.AddRange(New EditorButton() {
				New EditorButton(ButtonPredefines.Search),
				New EditorButton(ButtonPredefines.Clear),
				New EditorButton(ButtonPredefines.SpinLeft),
				New EditorButton(ButtonPredefines.SpinRight),
				showResult
			})

			SubscribeRIEvent(True)
		End Sub

		Private Sub SubscribeRIEvent(ByVal subscribe As Boolean)
			If edit Is Nothing Then
				Return
			End If
			RemoveHandler edit.Properties.ButtonClick, AddressOf ButtonClick
			If subscribe Then
				AddHandler edit.Properties.ButtonClick, AddressOf ButtonClick
			End If
		End Sub

		Private Sub SubscibeTreeListEvent(ByVal subscribe As Boolean)
			If treeList Is Nothing Then
				Return
			End If
			RemoveHandler treeList.CustomDrawNodeCell, AddressOf treeList_CustomDrawNodeCell
			If subscribe Then
				AddHandler treeList.CustomDrawNodeCell, AddressOf treeList_CustomDrawNodeCell
			End If
		End Sub

		Private Sub treeList_CustomDrawNodeCell(ByVal sender As Object, ByVal e As CustomDrawNodeCellEventArgs)
			If FindListIsEmpty Then
				Return
			End If

			Dim filterTextIndex As Integer = e.CellText.IndexOf(filterCellText, StringComparison.CurrentCultureIgnoreCase)
			If filterTextIndex = -1 Then
				Return
			End If
			Dim temp As New TreeCell(e.Node.Id, e.Column)

			If NeedHighLight(temp) Then
				Using brush As New SolidBrush(BackGroundColor)
					e.Cache.FillRectangle(brush, e.Bounds)
				End Using
			End If


			Dim tevi As TextEditViewInfo = TryCast(e.EditViewInfo, TextEditViewInfo)
			If tevi Is Nothing Then
				Return
			End If
			e.Appearance.BackColor = Color.Empty
			e.Cache.Paint.DrawMultiColorString(e.Cache, tevi.MaskBoxRect, e.CellText, filterCellText, e.Appearance, e.Appearance.ForeColor, HighLightColor, False, filterTextIndex)
			e.Handled = True
		End Sub

		Protected Overrides Sub Dispose(ByVal disposing As Boolean)
			SubscibeTreeListEvent(False)
			SubscribeRIEvent(False)
			MyBase.Dispose(disposing)
		End Sub


		Private Sub ButtonClick(ByVal sender As Object, ByVal e As ButtonPressedEventArgs)
			Dim edit As ButtonEdit = TryCast(sender, ButtonEdit)
			Select Case e.Button.Kind
				Case ButtonPredefines.Search
					PerformSearch(edit.EditValue)
				Case ButtonPredefines.Clear
					ClearEditValue(edit)
					PerformSearch(Nothing)
				Case ButtonPredefines.SpinLeft
					HighLightPrevious()
				Case ButtonPredefines.SpinRight
					HighLightNext()
			End Select
		End Sub

		Private Sub UpdateShowResult()
			Dim index As Integer
			For index = 0 To findList.Keys.Count - 1
				If findList(findList.Keys.ElementAt(index)) Then
					index += 1
					Exit For
				End If
			Next index
			showResult.Caption = String.Format("{0} of {1}", index, findList.Keys.Count)

		End Sub

		Private Sub HighLightPrevious()

			If FindListIsEmpty Then
				Return
			End If

			Dim currItem As TreeCell = findList.Keys.ElementAt(0)
			Dim targetItem As TreeCell = findList.Keys.ElementAt(findList.Keys.Count - 1)
			Dim temp As TreeCell
			For i As Integer = 1 To findList.Keys.Count - 1
				temp = findList.Keys.ElementAt(i)
				If findList(temp) Then
					targetItem = findList.Keys.ElementAt(i - 1)
					currItem = temp
					Exit For
				End If
			Next i

			findList(currItem) = False
			findList(targetItem) = True
			EnsureNodeVisible(targetItem)
			RefreshTreeList()
			UpdateShowResult()
		End Sub

		Private Sub HighLightNext()
			If FindListIsEmpty Then
				Return
			End If

			Dim needBreak As Boolean = False
			Dim currItem As TreeCell = Nothing
			Dim targetItem As TreeCell = Nothing
			For Each item As TreeCell In findList.Keys
				If needBreak Then
					targetItem = item
					Exit For
				End If
				If findList(item) Then
					currItem = item
					needBreak = True
				End If
			Next item

			If targetItem Is Nothing Then
				targetItem = findList.Keys.ElementAt(0)
			End If

			findList(currItem) = False
			findList(targetItem) = True
			EnsureNodeVisible(targetItem)
			RefreshTreeList()
			UpdateShowResult()
		End Sub

		Private Sub EnsureNodeVisible(ByVal cell As TreeCell)
			treeList.MakeNodeVisible(treeList.FindNodeByID(cell.NodeID))
			treeList.FocusedColumn = cell.Column
		End Sub

		Private Sub ClearEditValue(ByVal edit As ButtonEdit)
			edit.EditValue = Nothing
		End Sub

		Private Sub PerformSearch(ByVal val As Object)
			findList.Clear()
			If val Is Nothing Then
				val = String.Empty
			End If
			filterCellText = val.ToString()
			InitFindList()
			If Not FindListIsEmpty Then
				EnsureNodeVisible(findList.Keys.ElementAt(0))
			End If
			RefreshTreeList()
			UpdateShowResult()
		End Sub

		Private Sub InitFindList()
			If String.IsNullOrEmpty(filterCellText) Then
				Return
			End If
			treeList.NodesIterator.DoOperation(Sub(node)
				For Each col As TreeListColumn In node.TreeList.Columns
					If Not col.Visible OrElse (TryCast(col.RealColumnEdit, RepositoryItemTextEdit)) Is Nothing Then
						Continue For
					End If
					If node.GetDisplayText(col).IndexOf(filterCellText, StringComparison.CurrentCultureIgnoreCase) = -1 Then
						Continue For
					End If
					findList.Add(New TreeCell(node.Id, col), False)
				Next col
			End Sub)
			If FindListIsEmpty Then
				Return
			End If
			findList(findList.Keys.ElementAt(0)) = True
		End Sub

		Private Sub RefreshTreeList()
			treeList.LayoutChanged()
		End Sub

		Private Function NeedHighLight(ByVal cell As TreeCell) As Boolean
			For Each item As TreeCell In findList.Keys
				If item.NodeID = cell.NodeID AndAlso item.Column Is cell.Column Then
					Return findList(item)
				End If
			Next item
			Return False
		End Function

		Private Class TreeCell
			Public Property NodeID() As Integer
			Public Property Column() As TreeListColumn
			Public Sub New(ByVal id As Integer, ByVal c As TreeListColumn)
				NodeID = id
				Column = c
			End Sub
		End Class
	End Class
End Namespace
