<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frm_rulesCollection
    Inherits System.Windows.Forms.Form

    'Formulář přepisuje metodu Dispose, aby vyčistil seznam součástí.
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

    'Vyžadováno Návrhářem Windows Form
    Private components As System.ComponentModel.IContainer

    'POZNÁMKA: Následující procedura je vyžadována Návrhářem Windows Form
    'Může být upraveno pomocí Návrháře Windows Form.  
    'Neupravovat pomocí editoru kódu
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.lst_main_collection = New System.Windows.Forms.ListBox()
        Me.SuspendLayout()
        '
        'lst_main_collection
        '
        Me.lst_main_collection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lst_main_collection.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lst_main_collection.FormattingEnabled = True
        Me.lst_main_collection.ItemHeight = 20
        Me.lst_main_collection.Location = New System.Drawing.Point(13, 25)
        Me.lst_main_collection.Name = "lst_main_collection"
        Me.lst_main_collection.Size = New System.Drawing.Size(649, 402)
        Me.lst_main_collection.TabIndex = 0
        '
        'frm_rulesCollection
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 450)
        Me.Controls.Add(Me.lst_main_collection)
        Me.Name = "frm_rulesCollection"
        Me.Text = "Form1"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents lst_main_collection As ListBox
End Class
