<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
		Me.components = New System.ComponentModel.Container
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
		Dim DefaultSecurity2 As Dataweb.NShape.DefaultSecurity = New Dataweb.NShape.DefaultSecurity
		Me.Display1 = New Dataweb.NShape.WinFormsUI.Display
		Me.DiagramSetController1 = New Dataweb.NShape.Controllers.DiagramSetController
		Me.Project1 = New Dataweb.NShape.Project(Me.components)
		Me.CachedRepository1 = New Dataweb.NShape.Advanced.CachedRepository
		Me.ToolStripContainer1 = New System.Windows.Forms.ToolStripContainer
		Me.ToolStrip1 = New System.Windows.Forms.ToolStrip
		Me.ToolStripButton2 = New System.Windows.Forms.ToolStripButton
		Me.ToolStripContainer1.ContentPanel.SuspendLayout()
		Me.ToolStripContainer1.TopToolStripPanel.SuspendLayout()
		Me.ToolStripContainer1.SuspendLayout()
		Me.ToolStrip1.SuspendLayout()
		Me.SuspendLayout()
		'
		'Display1
		'
		Me.Display1.AllowDrop = True
		Me.Display1.AutoScroll = True
		Me.Display1.BackColorGradient = System.Drawing.SystemColors.Control
		Me.Display1.BackgroundGradientAngle = 45.0!
		Me.Display1.ConnectionPointShape = Dataweb.NShape.Controllers.ControlPointShape.Circle
		Me.Display1.ControlPointAlpha = CType(255, Byte)
		Me.Display1.DiagramSetController = Me.DiagramSetController1
		Me.Display1.Dock = System.Windows.Forms.DockStyle.Fill
		Me.Display1.GridAlpha = CType(255, Byte)
		Me.Display1.GridColor = System.Drawing.Color.White
		Me.Display1.GridSize = 20
		Me.Display1.GripSize = 3
		Me.Display1.HighQualityBackground = True
		Me.Display1.HighQualityRendering = True
		Me.Display1.ImeMode = System.Windows.Forms.ImeMode.NoControl
		Me.Display1.Location = New System.Drawing.Point(0, 0)
		Me.Display1.MinRotateRange = 30
		Me.Display1.Name = "Display1"
		Me.Display1.PropertyController = Nothing
		Me.Display1.RenderingQualityHighQuality = Dataweb.NShape.Advanced.RenderingQuality.HighQuality
		Me.Display1.RenderingQualityLowQuality = Dataweb.NShape.Advanced.RenderingQuality.DefaultQuality
		Me.Display1.ResizeGripShape = Dataweb.NShape.Controllers.ControlPointShape.Square
		Me.Display1.SelectionHilightColor = System.Drawing.Color.Firebrick
		Me.Display1.SelectionInactiveColor = System.Drawing.Color.Gray
		Me.Display1.SelectionInteriorColor = System.Drawing.Color.WhiteSmoke
		Me.Display1.SelectionNormalColor = System.Drawing.Color.DarkGreen
		Me.Display1.ShowDefaultContextMenu = False
		Me.Display1.ShowGrid = False
		Me.Display1.ShowScrollBars = True
		Me.Display1.Size = New System.Drawing.Size(412, 427)
		Me.Display1.SnapDistance = 5
		Me.Display1.SnapToGrid = False
		Me.Display1.TabIndex = 0
		Me.Display1.ToolPreviewBackColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(119, Byte), Integer), CType(CType(136, Byte), Integer), CType(CType(153, Byte), Integer))
		Me.Display1.ToolPreviewColor = System.Drawing.Color.FromArgb(CType(CType(96, Byte), Integer), CType(CType(70, Byte), Integer), CType(CType(130, Byte), Integer), CType(CType(180, Byte), Integer))
		Me.Display1.ZoomLevel = 100
		Me.Display1.ZoomWithMouseWheel = True
		'
		'DiagramSetController1
		'
		Me.DiagramSetController1.ActiveTool = Nothing
		Me.DiagramSetController1.Project = Me.Project1
		'
		'Project1
		'
		Me.Project1.AutoGenerateTemplates = True
		Me.Project1.LibrarySearchPaths = CType(resources.GetObject("Project1.LibrarySearchPaths"), System.Collections.Generic.IList(Of String))
		Me.Project1.Name = ""
		Me.Project1.Repository = Me.CachedRepository1
		DefaultSecurity2.CurrentRole = Dataweb.NShape.StandardRole.Administrator
		DefaultSecurity2.CurrentRoleName = "Administrator"
		Me.Project1.SecurityManager = DefaultSecurity2
		'
		'CachedRepository1
		'
		Me.CachedRepository1.ProjectName = ""
		Me.CachedRepository1.Store = Nothing
		Me.CachedRepository1.Version = 0
		'
		'ToolStripContainer1
		'
		'
		'ToolStripContainer1.ContentPanel
		'
		Me.ToolStripContainer1.ContentPanel.Controls.Add(Me.Display1)
		Me.ToolStripContainer1.ContentPanel.Size = New System.Drawing.Size(412, 427)
		Me.ToolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill
		Me.ToolStripContainer1.Location = New System.Drawing.Point(0, 0)
		Me.ToolStripContainer1.Name = "ToolStripContainer1"
		Me.ToolStripContainer1.Size = New System.Drawing.Size(412, 452)
		Me.ToolStripContainer1.TabIndex = 1
		Me.ToolStripContainer1.Text = "ToolStripContainer1"
		'
		'ToolStripContainer1.TopToolStripPanel
		'
		Me.ToolStripContainer1.TopToolStripPanel.Controls.Add(Me.ToolStrip1)
		'
		'ToolStrip1
		'
		Me.ToolStrip1.Dock = System.Windows.Forms.DockStyle.None
		Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripButton2})
		Me.ToolStrip1.Location = New System.Drawing.Point(3, 0)
		Me.ToolStrip1.Name = "ToolStrip1"
		Me.ToolStrip1.Size = New System.Drawing.Size(74, 25)
		Me.ToolStrip1.TabIndex = 0
		'
		'ToolStripButton2
		'
		Me.ToolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
		Me.ToolStripButton2.Image = CType(resources.GetObject("ToolStripButton2.Image"), System.Drawing.Image)
		Me.ToolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta
		Me.ToolStripButton2.Name = "ToolStripButton2"
		Me.ToolStripButton2.Size = New System.Drawing.Size(62, 22)
		Me.ToolStripButton2.Text = "New Game"
		'
		'Form1
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(412, 452)
		Me.Controls.Add(Me.ToolStripContainer1)
		Me.Name = "Form1"
		Me.Text = "Form1"
		Me.ToolStripContainer1.ContentPanel.ResumeLayout(False)
		Me.ToolStripContainer1.TopToolStripPanel.ResumeLayout(False)
		Me.ToolStripContainer1.TopToolStripPanel.PerformLayout()
		Me.ToolStripContainer1.ResumeLayout(False)
		Me.ToolStripContainer1.PerformLayout()
		Me.ToolStrip1.ResumeLayout(False)
		Me.ToolStrip1.PerformLayout()
		Me.ResumeLayout(False)

	End Sub
	Friend WithEvents Display1 As Dataweb.NShape.WinFormsUI.Display
	Friend WithEvents ToolStripContainer1 As System.Windows.Forms.ToolStripContainer
	Friend WithEvents ToolStrip1 As System.Windows.Forms.ToolStrip
	Friend WithEvents ToolStripButton2 As System.Windows.Forms.ToolStripButton
	Friend WithEvents Project1 As Dataweb.NShape.Project
	Friend WithEvents DiagramSetController1 As Dataweb.NShape.Controllers.DiagramSetController
	Friend WithEvents CachedRepository1 As Dataweb.NShape.Advanced.CachedRepository

End Class
