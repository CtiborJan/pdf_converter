<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frm_dispatchers
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
        Me.components = New System.ComponentModel.Container()
        Me.trv_dispatchers = New System.Windows.Forms.TreeView()
        Me.cxm = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.mnu_remove_connection = New System.Windows.Forms.ToolStripMenuItem()
        Me.trv_listeners = New System.Windows.Forms.TreeView()
        Me.cxm_actionname = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.mnu_set_dispatcher = New System.Windows.Forms.ToolStripMenuItem()
        Me.cxm.SuspendLayout()
        Me.cxm_actionname.SuspendLayout()
        Me.SuspendLayout()
        '
        'trv_dispatchers
        '
        Me.trv_dispatchers.ContextMenuStrip = Me.cxm
        Me.trv_dispatchers.Location = New System.Drawing.Point(4, 32)
        Me.trv_dispatchers.Name = "trv_dispatchers"
        Me.trv_dispatchers.Size = New System.Drawing.Size(397, 596)
        Me.trv_dispatchers.TabIndex = 2
        '
        'cxm
        '
        Me.cxm.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnu_remove_connection})
        Me.cxm.Name = "ContextMenuStrip1"
        Me.cxm.Size = New System.Drawing.Size(172, 26)
        '
        'mnu_remove_connection
        '
        Me.mnu_remove_connection.Name = "mnu_remove_connection"
        Me.mnu_remove_connection.Size = New System.Drawing.Size(171, 22)
        Me.mnu_remove_connection.Text = "Odebrat propojení"
        '
        'trv_listeners
        '
        Me.trv_listeners.ContextMenuStrip = Me.cxm
        Me.trv_listeners.Location = New System.Drawing.Point(480, 32)
        Me.trv_listeners.Name = "trv_listeners"
        Me.trv_listeners.Size = New System.Drawing.Size(397, 596)
        Me.trv_listeners.TabIndex = 3
        '
        'cxm_actionname
        '
        Me.cxm_actionname.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnu_set_dispatcher})
        Me.cxm_actionname.Name = "cxm_actionname"
        Me.cxm_actionname.Size = New System.Drawing.Size(219, 26)
        '
        'mnu_set_dispatcher
        '
        Me.mnu_set_dispatcher.Name = "mnu_set_dispatcher"
        Me.mnu_set_dispatcher.Size = New System.Drawing.Size(218, 22)
        Me.mnu_set_dispatcher.Text = "Nastavit spouštějící událost"
        '
        'frm_dispatchers
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(889, 632)
        Me.Controls.Add(Me.trv_listeners)
        Me.Controls.Add(Me.trv_dispatchers)
        Me.Name = "frm_dispatchers"
        Me.Text = "Nastavení dispečerů a posluchačů událostí"
        Me.cxm.ResumeLayout(False)
        Me.cxm_actionname.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents trv_dispatchers As TreeView
    Friend WithEvents trv_listeners As TreeView
    Friend WithEvents cxm As ContextMenuStrip
    Friend WithEvents mnu_remove_connection As ToolStripMenuItem
    Friend WithEvents cxm_actionname As ContextMenuStrip
    Friend WithEvents mnu_set_dispatcher As ToolStripMenuItem
End Class
