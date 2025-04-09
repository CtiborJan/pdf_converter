<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frm_add_tool
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
        Me.lbl_info1 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lst_templates = New System.Windows.Forms.ListBox()
        Me.trv_collections = New System.Windows.Forms.TreeView()
        Me.btn_add_to_collection = New System.Windows.Forms.Button()
        Me.btn_add_template = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txt_name = New System.Windows.Forms.TextBox()
        Me.txt_name_id = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txt_mark = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.txt_description = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.txt_hgl = New System.Windows.Forms.TextBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'lbl_info1
        '
        Me.lbl_info1.AutoSize = True
        Me.lbl_info1.Location = New System.Drawing.Point(2, 9)
        Me.lbl_info1.Name = "lbl_info1"
        Me.lbl_info1.Size = New System.Drawing.Size(159, 13)
        Me.lbl_info1.TabIndex = 0
        Me.lbl_info1.Text = "Vytvořit nový nástroj ze šablony:"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(2, 241)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(180, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "nebo zkopírovat již existující nástroj:"
        '
        'lst_templates
        '
        Me.lst_templates.FormattingEnabled = True
        Me.lst_templates.Location = New System.Drawing.Point(12, 25)
        Me.lst_templates.Name = "lst_templates"
        Me.lst_templates.Size = New System.Drawing.Size(285, 173)
        Me.lst_templates.TabIndex = 2
        '
        'trv_collections
        '
        Me.trv_collections.Location = New System.Drawing.Point(12, 257)
        Me.trv_collections.Name = "trv_collections"
        Me.trv_collections.Size = New System.Drawing.Size(285, 419)
        Me.trv_collections.TabIndex = 3
        '
        'btn_add_to_collection
        '
        Me.btn_add_to_collection.Location = New System.Drawing.Point(12, 682)
        Me.btn_add_to_collection.Name = "btn_add_to_collection"
        Me.btn_add_to_collection.Size = New System.Drawing.Size(285, 23)
        Me.btn_add_to_collection.TabIndex = 5
        Me.btn_add_to_collection.Text = "Přidat do kolekce"
        Me.btn_add_to_collection.UseVisualStyleBackColor = True
        '
        'btn_add_template
        '
        Me.btn_add_template.Location = New System.Drawing.Point(10, 200)
        Me.btn_add_template.Name = "btn_add_template"
        Me.btn_add_template.Size = New System.Drawing.Size(285, 23)
        Me.btn_add_template.TabIndex = 6
        Me.btn_add_template.Text = "Přidat do kolekce"
        Me.btn_add_template.UseVisualStyleBackColor = True
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(319, 27)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(38, 13)
        Me.Label2.TabIndex = 7
        Me.Label2.Text = "Jméno"
        '
        'txt_name
        '
        Me.txt_name.Location = New System.Drawing.Point(322, 43)
        Me.txt_name.Name = "txt_name"
        Me.txt_name.Size = New System.Drawing.Size(183, 20)
        Me.txt_name.TabIndex = 8
        '
        'txt_name_id
        '
        Me.txt_name_id.Location = New System.Drawing.Point(322, 82)
        Me.txt_name_id.Name = "txt_name_id"
        Me.txt_name_id.Size = New System.Drawing.Size(183, 20)
        Me.txt_name_id.TabIndex = 10
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(319, 66)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(52, 13)
        Me.Label3.TabIndex = 9
        Me.Label3.Text = "Jméno ID"
        '
        'txt_mark
        '
        Me.txt_mark.Location = New System.Drawing.Point(322, 121)
        Me.txt_mark.Name = "txt_mark"
        Me.txt_mark.Size = New System.Drawing.Size(183, 20)
        Me.txt_mark.TabIndex = 12
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(319, 105)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(44, 13)
        Me.Label4.TabIndex = 11
        Me.Label4.Text = "Značka"
        '
        'txt_description
        '
        Me.txt_description.Location = New System.Drawing.Point(322, 160)
        Me.txt_description.Name = "txt_description"
        Me.txt_description.Size = New System.Drawing.Size(183, 20)
        Me.txt_description.TabIndex = 14
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(319, 144)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(33, 13)
        Me.Label5.TabIndex = 13
        Me.Label5.Text = "Popis"
        '
        'txt_hgl
        '
        Me.txt_hgl.Location = New System.Drawing.Point(322, 201)
        Me.txt_hgl.Name = "txt_hgl"
        Me.txt_hgl.Size = New System.Drawing.Size(183, 20)
        Me.txt_hgl.TabIndex = 16
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(319, 185)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(94, 13)
        Me.Label6.TabIndex = 15
        Me.Label6.Text = "Formát zvýraznění"
        '
        'frm_add_tool
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(517, 717)
        Me.Controls.Add(Me.txt_hgl)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.txt_description)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.txt_mark)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.txt_name_id)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.txt_name)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.btn_add_template)
        Me.Controls.Add(Me.btn_add_to_collection)
        Me.Controls.Add(Me.trv_collections)
        Me.Controls.Add(Me.lst_templates)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.lbl_info1)
        Me.Name = "frm_add_tool"
        Me.Text = "Přidat nový/zkopírovat existující nástroj do kolekce"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lbl_info1 As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents lst_templates As ListBox
    Friend WithEvents trv_collections As TreeView
    Friend WithEvents btn_add_to_collection As Button
    Friend WithEvents btn_add_template As Button
    Friend WithEvents Label2 As Label
    Friend WithEvents txt_name As TextBox
    Friend WithEvents txt_name_id As TextBox
    Friend WithEvents Label3 As Label
    Friend WithEvents txt_mark As TextBox
    Friend WithEvents Label4 As Label
    Friend WithEvents txt_description As TextBox
    Friend WithEvents Label5 As Label
    Friend WithEvents txt_hgl As TextBox
    Friend WithEvents Label6 As Label
End Class
