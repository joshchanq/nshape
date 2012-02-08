Imports System.Collections.Generic
Imports System.Threading
Imports System.Windows.Forms
Imports Dataweb.NShape
Imports Dataweb.NShape.Advanced
Imports Dataweb.NShape.GeneralShapes


Public Class MainForm
	Dim stones As New List(Of RoundedBox)()
	Dim shapeType As ShapeType
	Dim positions As New List(Of Point)
	Dim moveCnt As Integer = 0
	Const rectSize = 100


	Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
		' Prepare project
		Project.Name = "API DEMO"
		Project.Create()
		Project.AddLibraryByName("Dataweb.NShape.GeneralShapes")

		' Prepare diagram
		Dim diagram As New Diagram("Diagram")
		diagram.Width = 3 * rectSize
		diagram.Height = 3 * rectSize
		diagram.BackgroundImage = New NamedImage(NShape_Shuffle_Game.My.Resources.Background, "Background")
		diagram.BackgroundImageLayout = ImageLayoutMode.Tile

		' Prepare visual styles
		' Create a fill style for the stones
		Dim fillstyle As New FillStyle
		fillstyle.AdditionalColorStyle = Project.Design.ColorStyles.Transparent
		fillstyle.BaseColorStyle = Project.Design.ColorStyles.Black
		fillstyle.FillMode = FillMode.Gradient
		' Create character style for the stone's labels
		Dim characterStyle As New CharacterStyle
		characterStyle.ColorStyle = New ColorStyle("White", Color.White)
		characterStyle.Size = 36
		characterStyle.FontName = "Tahoma"
		characterStyle.Style = Drawing.FontStyle.Bold
		' Create line style for the stone's outlines
		Dim lineStyle As New LineStyle
		lineStyle.ColorStyle = New ColorStyle("White", Color.White)

		' Find shape type for creating the stones
		Dim shapeType As ShapeType
		shapeType = Project.ShapeTypes("RoundedBox")

		Dim r, c As Integer
		Dim rectangle As RoundedBox
		For r = 0 To 2
			For c = 0 To 2
				Dim p As Point = New Point((rectSize / 2) + (c * rectSize), (rectSize / 2) + (r * rectSize))
				positions.Add(p)

				If Not (r = 2 And c = 2) Then
					' Create a new stone and set up its visual appearance
					rectangle = shapeType.CreateInstance()
					rectangle.FillStyle = fillstyle
					rectangle.CharacterStyle = characterStyle
					rectangle.LineStyle = lineStyle
					' Set the stone's size and label
					rectangle.Width = rectSize
					rectangle.Height = rectSize
					rectangle.Text = (stones.Count + 1).ToString()
					' move the stone to its position
					rectangle.MoveTo(p.X, p.Y)

					stones.Add(rectangle)
					diagram.Shapes.Add(rectangle)
				Else
					stones.Add(Nothing)
				End If
			Next
		Next
		Display.Diagram = diagram
		Display.Invalidate()
		Form1_Resize(Me, Nothing)

		Shuffle()
	End Sub


	Private Sub ZoomDiagramToForm()
		Dim zoom As Single
		If Not Display Is Nothing And Not Display.Diagram Is Nothing Then
			zoom = Math.Min(Display.Bounds.Width / CSng(Display.Diagram.Width), Display.Bounds.Height / CSng(Display.Diagram.Height))
			If (zoom > 0) Then
				Display.ZoomLevel = zoom * 100
			Else
				Display.ZoomLevel = 1
			End If
		End If
	End Sub


	Private Sub Shuffle()
		Dim Randomizer As New Random(DateTime.Now.Millisecond)
		Dim idxList As New List(Of Integer)(9)
		Dim stoneBuffer As New List(Of RoundedBox)(9)
		For i As Integer = 0 To 8
			idxList.Add(i)
			stoneBuffer.Add(Nothing)
		Next

		For i As Integer = 0 To 8
			Dim idx As Integer
			idx = Randomizer.Next(1, idxList.Count) - 1
			stoneBuffer(i) = stones(idxList(idx))
			idxList.RemoveAt(idx)
		Next

		moveCnt = 0
		stones.Clear()
		stones.AddRange(stoneBuffer)
		For i As Integer = 0 To 8
			If TypeOf stones(i) Is RoundedBox Then
				stones(i).MoveTo(positions(i).X, positions(i).Y)
			End If
		Next
	End Sub


	Private Sub CheckStones(ByVal x As Integer, ByVal y As Integer)
		Dim stone As RoundedBox
		stone = Display.Diagram.Shapes.FindShape(x, y, ControlPointCapabilities.None, 0, Nothing)
		If Not stone Is Nothing Then
			If stone.ContainsPoint(x, y) Then
				Dim idx As Integer = stones.IndexOf(stone)

				Select Case idx
					' first row
					' > 0 1 2
					'   3 4 5
					'   6 7 8
					Case 0
						If stones(1) Is Nothing Then
							MoveStone(stone, idx, 1)
						ElseIf stones(3) Is Nothing Then
							MoveStone(stone, idx, 3)
						End If
					Case 1
						If stones(0) Is Nothing Then
							MoveStone(stone, idx, 0)
						ElseIf stones(2) Is Nothing Then
							MoveStone(stone, idx, 2)
						ElseIf stones(4) Is Nothing Then
							MoveStone(stone, idx, 4)
						End If
					Case 2
						If stones(1) Is Nothing Then
							MoveStone(stone, idx, 1)
						ElseIf stones(5) Is Nothing Then
							MoveStone(stone, idx, 5)
						End If

						' second row
						'   0 1 2
						' > 3 4 5
						'   6 7 8
					Case 3
						If stones(0) Is Nothing Then
							MoveStone(stone, idx, 0)
						ElseIf stones(4) Is Nothing Then
							MoveStone(stone, idx, 4)
						ElseIf stones(6) Is Nothing Then
							MoveStone(stone, idx, 6)
						End If
					Case 4
						If stones(1) Is Nothing Then
							MoveStone(stone, idx, 1)
						ElseIf stones(3) Is Nothing Then
							MoveStone(stone, idx, 3)
						ElseIf stones(5) Is Nothing Then
							MoveStone(stone, idx, 5)
						ElseIf stones(7) Is Nothing Then
							MoveStone(stone, idx, 7)
						End If
					Case 5
						If stones(2) Is Nothing Then
							MoveStone(stone, idx, 2)
						ElseIf stones(4) Is Nothing Then
							MoveStone(stone, idx, 4)
						ElseIf stones(8) Is Nothing Then
							MoveStone(stone, idx, 8)
						End If

						' third row
						'   0 1 2
						'   3 4 5
						' > 6 7 8
					Case 6
						If stones(3) Is Nothing Then
							MoveStone(stone, idx, 3)
						ElseIf stones(7) Is Nothing Then
							MoveStone(stone, idx, 7)
						End If
					Case 7
						If stones(4) Is Nothing Then
							MoveStone(stone, idx, 4)
						ElseIf stones(6) Is Nothing Then
							MoveStone(stone, idx, 6)
						ElseIf stones(8) Is Nothing Then
							MoveStone(stone, idx, 8)
						End If
					Case 8
						If stones(5) Is Nothing Then
							MoveStone(stone, idx, 5)
						ElseIf stones(7) Is Nothing Then
							MoveStone(stone, idx, 7)
						End If
				End Select
			End If
		End If

		Dim won As Boolean = True
		For i As Integer = 0 To 7
			If stones(i) Is Nothing Then
				won = False
				Exit For
			ElseIf stones(i).Text <> (i + 1).ToString() Then
				won = False
				Exit For
			End If
		Next
		If won And moveCnt > 0 Then
			MessageBox.Show(String.Format("Game solved with {0} moves. {1}Congratulations!", moveCnt, vbCrLf))
		End If
	End Sub


	Private Sub MoveStone(ByVal stone As RoundedBox, ByVal fromPos As Integer, ByVal toPos As Integer)
		moveCnt += 1

		stones(fromPos) = Nothing
		stones(toPos) = stone

		Dim stepping As Integer = 0
		Dim steps As Integer = 10

		Dim distanceX As Integer = positions(toPos).X - positions(fromPos).X
		Dim distanceY As Integer = positions(toPos).Y - positions(fromPos).Y

		If distanceX <> 0 Then
			stepping = distanceX / steps
			While stone.X <> positions(toPos).X
				stone.X += stepping
				Application.DoEvents()
			End While
		ElseIf distanceY <> 0 Then
			stepping = distanceY / steps
			While stone.Y <> positions(toPos).Y
				stone.Y += stepping
				Application.DoEvents()
			End While
		End If
		If stone.X <> positions(toPos).X Or stone.Y <> positions(toPos).Y Then
			stone.MoveTo(positions(toPos).X, positions(toPos).Y)
		End If
	End Sub


	Private Sub Display1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Display.Click
		Dim p As Point = Point.Empty
		Display.ControlToDiagram(Me.PointToClient(Control.MousePosition), p)
		CheckStones(p.X, p.Y)
	End Sub


	Private Sub ToolStripButton2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton2.Click
		Shuffle()
	End Sub


	Private Sub Form1_Resize(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Resize
		ZoomDiagramToForm()
	End Sub


	Private Sub MaximumQualityToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MaximumQualityToolStripMenuItem.Click
		Display.RenderingQualityHighQuality = RenderingQuality.HighQuality
		Display.HighQualityRendering = True
		HighQualityToolStripMenuItem.Checked = False
		MediumQualityToolStripMenuItem.Checked = False
		LowQualityToolStripMenuItem.Checked = False
	End Sub


	Private Sub HighQualityToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles HighQualityToolStripMenuItem.Click
		Display.RenderingQualityHighQuality = RenderingQuality.HighQuality
		Display.HighQualityRendering = True
		MaximumQualityToolStripMenuItem.Checked = False
		MediumQualityToolStripMenuItem.Checked = False
		LowQualityToolStripMenuItem.Checked = False
	End Sub


	Private Sub DefaultQualityToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MediumQualityToolStripMenuItem.Click
		Display.RenderingQualityLowQuality = RenderingQuality.DefaultQuality
		Display.HighQualityRendering = False
		MaximumQualityToolStripMenuItem.Checked = False
		HighQualityToolStripMenuItem.Checked = False
		LowQualityToolStripMenuItem.Checked = False
	End Sub


	Private Sub LowQualityToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LowQualityToolStripMenuItem.Click
		Display.RenderingQualityLowQuality = RenderingQuality.LowQuality
		Display.HighQualityRendering = False
		MaximumQualityToolStripMenuItem.Checked = False
		HighQualityToolStripMenuItem.Checked = False
		MediumQualityToolStripMenuItem.Checked = False
	End Sub
End Class
