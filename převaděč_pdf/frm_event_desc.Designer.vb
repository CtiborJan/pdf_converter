<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frm_event_desc
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
        Me.lst_events = New System.Windows.Forms.ListBox()
        Me.tbc_main = New System.Windows.Forms.TabControl()
        Me.tab_keyevents = New System.Windows.Forms.TabPage()
        Me.txt_ke_keycode = New System.Windows.Forms.TextBox()
        Me.lbl_ke_info = New System.Windows.Forms.Label()
        Me.chb_ke_shift = New System.Windows.Forms.CheckBox()
        Me.chb_ke_alt = New System.Windows.Forms.CheckBox()
        Me.chb_ke_ctrl = New System.Windows.Forms.CheckBox()
        Me.tab_mouseevents = New System.Windows.Forms.TabPage()
        Me.cmb_button = New System.Windows.Forms.ComboBox()
        Me.chb_me_shift = New System.Windows.Forms.CheckBox()
        Me.chb_me_alt = New System.Windows.Forms.CheckBox()
        Me.chb_me_ctrl = New System.Windows.Forms.CheckBox()
        Me.cmd_ok = New System.Windows.Forms.Button()
        Me.tbc_main.SuspendLayout()
        Me.tab_keyevents.SuspendLayout()
        Me.tab_mouseevents.SuspendLayout()
        Me.SuspendLayout()
        '
        'lst_events
        '
        Me.lst_events.FormattingEnabled = True
        Me.lst_events.Location = New System.Drawing.Point(12, 12)
        Me.lst_events.Name = "lst_events"
        Me.lst_events.Size = New System.Drawing.Size(248, 225)
        Me.lst_events.TabIndex = 0
        '
        'tbc_main
        '
        Me.tbc_main.Controls.Add(Me.tab_keyevents)
        Me.tbc_main.Controls.Add(Me.tab_mouseevents)
        Me.tbc_main.Location = New System.Drawing.Point(266, 12)
        Me.tbc_main.Name = "tbc_main"
        Me.tbc_main.SelectedIndex = 0
        Me.tbc_main.Size = New System.Drawing.Size(347, 225)
        Me.tbc_main.TabIndex = 1
        '
        'tab_keyevents
        '
        Me.tab_keyevents.Controls.Add(Me.txt_ke_keycode)
        Me.tab_keyevents.Controls.Add(Me.lbl_ke_info)
        Me.tab_keyevents.Controls.Add(Me.chb_ke_shift)
        Me.tab_keyevents.Controls.Add(Me.chb_ke_alt)
        Me.tab_keyevents.Controls.Add(Me.chb_ke_ctrl)
        Me.tab_keyevents.Location = New System.Drawing.Point(4, 22)
        Me.tab_keyevents.Name = "tab_keyevents"
        Me.tab_keyevents.Padding = New System.Windows.Forms.Padding(3)
        Me.tab_keyevents.Size = New System.Drawing.Size(339, 199)
        Me.tab_keyevents.TabIndex = 0
        Me.tab_keyevents.Text = "Události klávesnice"
        Me.tab_keyevents.UseVisualStyleBackColor = True
        '
        'txt_ke_keycode
        '
        Me.txt_ke_keycode.Location = New System.Drawing.Point(84, 20)
        Me.txt_ke_keycode.Name = "txt_ke_keycode"
        Me.txt_ke_keycode.Size = New System.Drawing.Size(100, 20)
        Me.txt_ke_keycode.TabIndex = 4
        '
        'lbl_ke_info
        '
        Me.lbl_ke_info.AutoSize = True
        Me.lbl_ke_info.Location = New System.Drawing.Point(12, 23)
        Me.lbl_ke_info.Name = "lbl_ke_info"
        Me.lbl_ke_info.Size = New System.Drawing.Size(48, 13)
        Me.lbl_ke_info.TabIndex = 3
        Me.lbl_ke_info.Text = "Klávesa:"
        '
        'chb_ke_shift
        '
        Me.chb_ke_shift.AutoSize = True
        Me.chb_ke_shift.Location = New System.Drawing.Point(189, 65)
        Me.chb_ke_shift.Name = "chb_ke_shift"
        Me.chb_ke_shift.Size = New System.Drawing.Size(57, 17)
        Me.chb_ke_shift.TabIndex = 2
        Me.chb_ke_shift.Text = "SHIFT"
        Me.chb_ke_shift.UseVisualStyleBackColor = True
        '
        'chb_ke_alt
        '
        Me.chb_ke_alt.AutoSize = True
        Me.chb_ke_alt.Location = New System.Drawing.Point(102, 65)
        Me.chb_ke_alt.Name = "chb_ke_alt"
        Me.chb_ke_alt.Size = New System.Drawing.Size(46, 17)
        Me.chb_ke_alt.TabIndex = 1
        Me.chb_ke_alt.Text = "ALT"
        Me.chb_ke_alt.UseVisualStyleBackColor = True
        '
        'chb_ke_ctrl
        '
        Me.chb_ke_ctrl.AutoSize = True
        Me.chb_ke_ctrl.Location = New System.Drawing.Point(15, 65)
        Me.chb_ke_ctrl.Name = "chb_ke_ctrl"
        Me.chb_ke_ctrl.Size = New System.Drawing.Size(54, 17)
        Me.chb_ke_ctrl.TabIndex = 0
        Me.chb_ke_ctrl.Text = "CTRL"
        Me.chb_ke_ctrl.UseVisualStyleBackColor = True
        '
        'tab_mouseevents
        '
        Me.tab_mouseevents.Controls.Add(Me.cmb_button)
        Me.tab_mouseevents.Controls.Add(Me.chb_me_shift)
        Me.tab_mouseevents.Controls.Add(Me.chb_me_alt)
        Me.tab_mouseevents.Controls.Add(Me.chb_me_ctrl)
        Me.tab_mouseevents.Location = New System.Drawing.Point(4, 22)
        Me.tab_mouseevents.Name = "tab_mouseevents"
        Me.tab_mouseevents.Padding = New System.Windows.Forms.Padding(3)
        Me.tab_mouseevents.Size = New System.Drawing.Size(339, 199)
        Me.tab_mouseevents.TabIndex = 1
        Me.tab_mouseevents.Text = "Události myši"
        Me.tab_mouseevents.UseVisualStyleBackColor = True
        '
        'cmb_button
        '
        Me.cmb_button.FormattingEnabled = True
        Me.cmb_button.Items.AddRange(New Object() {"Levé", "Pravé", "Prostřední"})
        Me.cmb_button.Location = New System.Drawing.Point(15, 19)
        Me.cmb_button.Name = "cmb_button"
        Me.cmb_button.Size = New System.Drawing.Size(231, 21)
        Me.cmb_button.TabIndex = 6
        '
        'chb_me_shift
        '
        Me.chb_me_shift.AutoSize = True
        Me.chb_me_shift.Location = New System.Drawing.Point(189, 65)
        Me.chb_me_shift.Name = "chb_me_shift"
        Me.chb_me_shift.Size = New System.Drawing.Size(57, 17)
        Me.chb_me_shift.TabIndex = 5
        Me.chb_me_shift.Text = "SHIFT"
        Me.chb_me_shift.UseVisualStyleBackColor = True
        '
        'chb_me_alt
        '
        Me.chb_me_alt.AutoSize = True
        Me.chb_me_alt.Location = New System.Drawing.Point(102, 65)
        Me.chb_me_alt.Name = "chb_me_alt"
        Me.chb_me_alt.Size = New System.Drawing.Size(46, 17)
        Me.chb_me_alt.TabIndex = 4
        Me.chb_me_alt.Text = "ALT"
        Me.chb_me_alt.UseVisualStyleBackColor = True
        '
        'chb_me_ctrl
        '
        Me.chb_me_ctrl.AutoSize = True
        Me.chb_me_ctrl.Location = New System.Drawing.Point(15, 65)
        Me.chb_me_ctrl.Name = "chb_me_ctrl"
        Me.chb_me_ctrl.Size = New System.Drawing.Size(54, 17)
        Me.chb_me_ctrl.TabIndex = 3
        Me.chb_me_ctrl.Text = "CTRL"
        Me.chb_me_ctrl.UseVisualStyleBackColor = True
        '
        'cmd_ok
        '
        Me.cmd_ok.Location = New System.Drawing.Point(266, 250)
        Me.cmd_ok.Name = "cmd_ok"
        Me.cmd_ok.Size = New System.Drawing.Size(347, 23)
        Me.cmd_ok.TabIndex = 2
        Me.cmd_ok.Text = "Ok"
        Me.cmd_ok.UseVisualStyleBackColor = True
        '
        'frm_event_desc
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(642, 280)
        Me.Controls.Add(Me.cmd_ok)
        Me.Controls.Add(Me.tbc_main)
        Me.Controls.Add(Me.lst_events)
        Me.Name = "frm_event_desc"
        Me.Text = "frm_event_desc"
        Me.tbc_main.ResumeLayout(False)
        Me.tab_keyevents.ResumeLayout(False)
        Me.tab_keyevents.PerformLayout()
        Me.tab_mouseevents.ResumeLayout(False)
        Me.tab_mouseevents.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents lst_events As ListBox
    Friend WithEvents tbc_main As TabControl
    Friend WithEvents tab_keyevents As TabPage
    Friend WithEvents chb_ke_shift As CheckBox
    Friend WithEvents chb_ke_alt As CheckBox
    Friend WithEvents chb_ke_ctrl As CheckBox
    Friend WithEvents tab_mouseevents As TabPage
    Friend WithEvents txt_ke_keycode As TextBox
    Friend WithEvents lbl_ke_info As Label
    Friend WithEvents cmb_button As ComboBox
    Friend WithEvents chb_me_shift As CheckBox
    Friend WithEvents chb_me_alt As CheckBox
    Friend WithEvents chb_me_ctrl As CheckBox
    Friend WithEvents cmd_ok As Button
End Class
