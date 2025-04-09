<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frm_main
    Inherits System.Windows.Forms.Form

    'Formulář přepisuje metodu Dispose, aby vyčistil seznam součástí.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Vyžadováno Návrhářem Windows Form
    Private components As System.ComponentModel.IContainer

    'POZNÁMKA: Následující procedura je vyžadována Návrhářem Windows Form
    'Může být upraveno pomocí Návrháře Windows Form.  
    'Neupravovat pomocí editoru kódu
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frm_main))
        Me.spc_main = New System.Windows.Forms.SplitContainer()
        Me.pnl_workspace_container = New System.Windows.Forms.Panel()
        Me.tbc = New System.Windows.Forms.TabControl()
        Me.tbp_doc = New System.Windows.Forms.TabPage()
        Me.tbp_tools = New System.Windows.Forms.TabPage()
        Me.tbp_pdf = New System.Windows.Forms.TabPage()
        Me.pdf = New AxAcroPDFLib.AxAcroPDF()
        CType(Me.spc_main, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.spc_main.Panel1.SuspendLayout()
        Me.spc_main.SuspendLayout()
        Me.pnl_workspace_container.SuspendLayout()
        Me.tbc.SuspendLayout()
        Me.tbp_pdf.SuspendLayout()
        CType(Me.pdf, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'spc_main
        '
        Me.spc_main.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.spc_main.Dock = System.Windows.Forms.DockStyle.Fill
        Me.spc_main.Location = New System.Drawing.Point(0, 0)
        Me.spc_main.Margin = New System.Windows.Forms.Padding(4)
        Me.spc_main.Name = "spc_main"
        '
        'spc_main.Panel1
        '
        Me.spc_main.Panel1.Controls.Add(Me.pnl_workspace_container)
        '
        'spc_main.Panel2
        '
        Me.spc_main.Panel2.BackColor = System.Drawing.SystemColors.Control
        Me.spc_main.Size = New System.Drawing.Size(1539, 796)
        Me.spc_main.SplitterDistance = 667
        Me.spc_main.SplitterWidth = 5
        Me.spc_main.TabIndex = 0
        '
        'pnl_workspace_container
        '
        Me.pnl_workspace_container.Controls.Add(Me.tbc)
        Me.pnl_workspace_container.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnl_workspace_container.Location = New System.Drawing.Point(0, 0)
        Me.pnl_workspace_container.Margin = New System.Windows.Forms.Padding(4)
        Me.pnl_workspace_container.Name = "pnl_workspace_container"
        Me.pnl_workspace_container.Size = New System.Drawing.Size(665, 794)
        Me.pnl_workspace_container.TabIndex = 0
        '
        'tbc
        '
        Me.tbc.Controls.Add(Me.tbp_doc)
        Me.tbc.Controls.Add(Me.tbp_tools)
        Me.tbc.Controls.Add(Me.tbp_pdf)
        Me.tbc.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tbc.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.tbc.Location = New System.Drawing.Point(0, 0)
        Me.tbc.Margin = New System.Windows.Forms.Padding(4)
        Me.tbc.Multiline = True
        Me.tbc.Name = "tbc"
        Me.tbc.SelectedIndex = 0
        Me.tbc.Size = New System.Drawing.Size(665, 794)
        Me.tbc.TabIndex = 0
        '
        'tbp_doc
        '
        Me.tbp_doc.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.tbp_doc.Location = New System.Drawing.Point(4, 27)
        Me.tbp_doc.Margin = New System.Windows.Forms.Padding(4)
        Me.tbp_doc.Name = "tbp_doc"
        Me.tbp_doc.Padding = New System.Windows.Forms.Padding(4)
        Me.tbp_doc.Size = New System.Drawing.Size(657, 763)
        Me.tbp_doc.TabIndex = 0
        Me.tbp_doc.Text = "Dokument"
        Me.tbp_doc.UseVisualStyleBackColor = True
        '
        'tbp_tools
        '
        Me.tbp_tools.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.tbp_tools.Location = New System.Drawing.Point(4, 27)
        Me.tbp_tools.Margin = New System.Windows.Forms.Padding(4)
        Me.tbp_tools.Name = "tbp_tools"
        Me.tbp_tools.Padding = New System.Windows.Forms.Padding(4)
        Me.tbp_tools.Size = New System.Drawing.Size(657, 763)
        Me.tbp_tools.TabIndex = 1
        Me.tbp_tools.Text = "Nástroje"
        Me.tbp_tools.UseVisualStyleBackColor = True
        '
        'tbp_pdf
        '
        Me.tbp_pdf.Controls.Add(Me.pdf)
        Me.tbp_pdf.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.tbp_pdf.Location = New System.Drawing.Point(4, 27)
        Me.tbp_pdf.Margin = New System.Windows.Forms.Padding(4)
        Me.tbp_pdf.Name = "tbp_pdf"
        Me.tbp_pdf.Size = New System.Drawing.Size(657, 763)
        Me.tbp_pdf.TabIndex = 2
        Me.tbp_pdf.Text = "PDF"
        Me.tbp_pdf.UseVisualStyleBackColor = True
        '
        'pdf
        '
        Me.pdf.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pdf.Enabled = True
        Me.pdf.Location = New System.Drawing.Point(0, 0)
        Me.pdf.Name = "pdf"
        Me.pdf.OcxState = CType(resources.GetObject("pdf.OcxState"), System.Windows.Forms.AxHost.State)
        Me.pdf.Size = New System.Drawing.Size(657, 763)
        Me.pdf.TabIndex = 0
        '
        'frm_main
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 18.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1539, 796)
        Me.Controls.Add(Me.spc_main)
        Me.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.KeyPreview = True
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "frm_main"
        Me.Text = "frm_main"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.spc_main.Panel1.ResumeLayout(False)
        CType(Me.spc_main, System.ComponentModel.ISupportInitialize).EndInit()
        Me.spc_main.ResumeLayout(False)
        Me.pnl_workspace_container.ResumeLayout(False)
        Me.tbc.ResumeLayout(False)
        Me.tbp_pdf.ResumeLayout(False)
        CType(Me.pdf, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents spc_main As SplitContainer
    Friend WithEvents pnl_workspace_container As Panel
    Friend WithEvents tbc As TabControl
    Friend WithEvents tbp_doc As TabPage
    Friend WithEvents tbp_tools As TabPage
    Friend WithEvents tbp_pdf As TabPage
    Friend WithEvents pdf As AxAcroPDFLib.AxAcroPDF
End Class
