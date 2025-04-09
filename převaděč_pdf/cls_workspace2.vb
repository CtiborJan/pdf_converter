
Public Class cls_context_menu
    Public Structure cm_tool
        Public tool As Object
        Public p1 As Object
        Public p2 As Object
        Public priority As Integer
        Public title As String
    End Structure
    Private Structure cm_tool_type_collection
        Public name As String
        Public cmt() As cm_tool
        Public n As Long
    End Structure
    Public pnl As Panel
    Public parent As cls_workspace
    Private lst_tool_types As ListBox
    Private cmds() As Button
    Private cmd_up As Button
    Private cmd_down As Button
    Private n_cmds As Long
    Private tcmnsc() As cm_tool_type_collection
    Private n_tool_types As Long
    Private activated_by_mouse As Boolean
    Public mtype As Integer

    Private last_displayed As Integer
    Private last_offset As Integer

    Public visible As Boolean

    Private Const FIXED As Integer = 0
    Private Const FLOATING As Integer = 1
    Private max_visible_buttons As Integer
    Private last_visible_button As Integer
    Public Sub New(parent_ As cls_workspace, mtype_ As Integer, Optional max_visible_buttons_ As Integer = 0)
        parent = parent_
        mtype = mtype_ '0=pevný ve středovém boxu, 1=plovoucí v textu aktivovaný pr. tlačítkem apod.
        max_visible_buttons = max_visible_buttons_
        If max_visible_buttons = 0 And mtype = FIXED Then
            max_visible_buttons = 9
        ElseIf max_visible_buttons = 0 And mtype = FLOATING Then
            max_visible_buttons = 4
        End If

        Dim t As Integer = 0
        cmd_up = New Button
        cmd_down = New Button
        pnl = New Panel
        pnl.BorderStyle = BorderStyle.None
        pnl.Parent = parent.container
        If Me.mtype = FIXED Then
            pnl.Height = 400
            pnl.Width = 250
            pnl.BorderStyle = BorderStyle.FixedSingle
            pnl.Font = env.def_font
        End If

        With cmd_up 'tlačítko pro posunutí seznamu nástrojů 
            .Parent = pnl
            .TabIndex = max_visible_buttons + 1
            .TabStop = False
            .Name = "cmd_up"
            .Text = "˄" '˅
            AddHandler cmd_up.Enter, AddressOf cmd_updown_Click
        End With
        With cmd_down 'tlačítko pro posunutí seznamu nástrojů 
            .Parent = pnl
            .TabIndex = max_visible_buttons + 2
            .TabStop = False
            .Name = "cmd_down"
            .Text = "˅"
            AddHandler cmd_down.Enter, AddressOf cmd_updown_Click
        End With
        ReDim cmds(max_visible_buttons) 'a tady vygenerujeme samotná "akční" tlačítka 
        'těch bude pevně daný počet, vygenerujou se jednou na začátku běhu programu a podle potřeby se budou skrývat/zobrazovat a bude se jim přiřazovat 
        'nějaká hodnota do tagu
        For i = 0 To max_visible_buttons
            cmds(i) = New Button


            With cmds(i)
                .Parent = pnl
                .Top = t
                .Height = 30 * env.environment_size
                t = t + .Height
                If mtype = FIXED Then
                    .Width = pnl.Width
                Else
                    .Width = pnl.Width - 100
                End If
                .TabIndex = i
                .Name = "cmd_" & i
                .Visible = False
                AddHandler .Click, AddressOf cmd_click

                If i = 0 Or i = max_visible_buttons Then AddHandler cmds(i).PreviewKeyDown, AddressOf cmd_PreKeyDown
            End With
        Next
        lst_tool_types = New ListBox
        With lst_tool_types
            .Parent = pnl
            If mtype = FIXED Then
                .Left = 0
                .Width = pnl.Width
                .Height = 100
                .Top = pnl.Height - 100
            Else
                .Left = cmds(0).Width
                .Top = 0
                .Width = 100
                .Height = pnl.Height
            End If
            .BorderStyle = BorderStyle.FixedSingle
            AddHandler .SelectedIndexChanged, AddressOf lst_tool_types_Changed
            AddHandler .KeyDown, AddressOf lst_tool_types_KeyDown
        End With
    End Sub
    Private Sub cmd_updown_Click(sender As Object, e As EventArgs)

        If sender.name = "cmd_down" Then
            display_group(last_displayed, last_offset + 1)
        Else
            display_group(last_displayed, last_offset - 1)
        End If
    End Sub

    Public Sub next_tool_group()
        If lst_tool_types.Items.Count > 0 Then
            If lst_tool_types.SelectedIndex < lst_tool_types.Items.Count - 1 Then
                lst_tool_types.SelectedIndex += 1
            Else
                lst_tool_types.SelectedIndex = 0
            End If
        End If
    End Sub
    Private Sub lst_tool_types_KeyDown(sender As Object, e As KeyEventArgs)
        If e.KeyValue = Keys.Right Or e.KeyValue = Keys.Left Then
            cmds(0).Select()
            e.SuppressKeyPress = True
        End If
    End Sub
    Public Sub lst_tool_types_Changed(sender As Object, e As EventArgs)
        If lst_tool_types.SelectedIndex <> -1 Then
            display_group(lst_tool_types.SelectedIndex)
        End If
    End Sub
    Public Sub hide()
        If Me.mtype = FLOATING And Me.pnl.Visible = True Then
            Me.pnl.Visible = False
            Me.parent.rtb.Select()
            Me.visible = False
        End If
    End Sub
    Public Sub generate_context_menu(Optional activated_by_mouse_ As Boolean = False)
        n_tool_types = -1
        Erase tcmnsc
        Dim tls() As Object
        activated_by_mouse = activated_by_mouse_
        tls = Me.parent.tm.all_tools
        Dim i As Long
        If tls IsNot Nothing Then
            For i = 0 To UBound(tls)
                'If i = 22 Then Stop
                tls(i).generate_context_menu(parent.p, Me)
            Next
        End If
        Dim max_tools_in_group As Integer
        Dim prioritized As Integer = 0
        Dim prioritized_value As Single
        Me.last_offset = 0
        If Me.n_tool_types = -1 Then 'žádné kontextové menu není k dispozici
            display_none()
        Else
            Dim j As Long
            For i = 0 To Me.n_tool_types 'zjistíme si největší počet příkazů ve skupinách a největší prioritu
                If Me.tcmnsc(i).n > max_tools_in_group Then max_tools_in_group = Me.tcmnsc(i).n

                If Me.n_tool_types > 0 Then 'pokud je víc skupin zjistíme prioritu a jako defaultní zobrazíme tu nejprioritnější
                    Dim priority As Long
                    priority = 0
                    For j = 0 To Me.tcmnsc(i).n
                        priority += Me.tcmnsc(i).cmt(j).priority
                    Next
                    If prioritized_value < priority / (Me.tcmnsc(i).n + 1) Then
                        prioritized = i
                        prioritized_value = priority / (Me.tcmnsc(i).n + 1)
                    End If
                End If
            Next



            lst_tool_types.Items.Clear()
            For i = 0 To n_tool_types
                lst_tool_types.Items.Add(Me.tcmnsc(i).name)
            Next

            display_group(prioritized)


            Me.pnl.Visible = True
            Me.visible = True
            If Me.mtype = FLOATING Then
                If cmds(0) IsNot Nothing Then
                    cmds(0).Select()
                End If
            End If
        End If
    End Sub

    Private Sub positionate_controls(max_tools As Integer)
        Dim i As Long
        Dim cmd_t As Long
        cmd_t = 15

        If Me.mtype = FLOATING Then
            'If max_tools > 3 Then max_tools = 3
            Dim pos As Point
            pos = parent.rtb.GetPositionFromCharIndex(parent.rtb.SelectionStart + parent.rtb.SelectionLength)
            Dim l As Integer
            Dim r As Integer
            Dim t As Integer
            Dim b As Integer
            Dim w As Integer
            Dim h As Integer
            w = 400
            If max_tools > max_visible_buttons Then max_tools = max_visible_buttons
            h = (max_tools + 1) * cmds(0).Height + 30
            If h < 55 * env.environment_size Then h = 55 * env.environment_size

            l = pos.X + Me.parent.rtb.Left 'rodičem panelu totiž není rtb, ale jeho (tj. rtb) rodič
            r = l + w
            If r > pnl.Parent.Width - 25 Then
                l = l - ((r - pnl.Parent.Width) + 25)
            End If
            t = pos.Y + Me.parent.rtb.Top
            If Me.activated_by_mouse = False Then
                t = t + 20 'pokud není menu aktivováno kliknutím pr. tlačítka, ale klávesnicí, zobrazíme ho pod aktuálně vybranou řádkou
            End If
            b = t + h

            pnl.Left = l
            pnl.Width = w
            pnl.Top = t
            pnl.Height = h
            pnl.BringToFront()

            cmd_up.Top = 0
            cmd_up.Height = 15
            cmd_up.Left = 0
            cmd_up.Width = pnl.Width - 100
            cmd_up.TabIndex = 1

            For i = 0 To max_visible_buttons
                cmds(i).Left = 0
                cmds(i).Top = cmd_t
                cmds(i).Width = pnl.Width - 100
                cmds(i).Height = 25
                cmd_t = cmd_t + cmds(0).Height
                cmds(i).TabIndex = i + 2
            Next

            cmd_down.Top = pnl.Height - 15
            cmd_down.Width = pnl.Width - 100
            cmd_down.Height = 15
            cmd_down.Left = 0
            cmd_down.Visible = True
            cmd_down.TabIndex = i + 2

            lst_tool_types.Left = pnl.Width - 100
            lst_tool_types.Width = 100
            lst_tool_types.Top = 0
            lst_tool_types.Height = pnl.Height
            lst_tool_types.IntegralHeight = False
            lst_tool_types.TabIndex = i + 3

        Else
            If max_tools > 9 Then max_tools = 3
            cmd_up.Top = 0
            cmd_up.Height = 15
            cmd_up.Left = 0
            cmd_up.Width = pnl.Width
            cmd_up.TabIndex = 1
            For i = 0 To max_visible_buttons
                cmds(i).Left = 0
                cmds(i).Top = cmd_t
                cmds(i).Width = pnl.Width
                cmds(i).Height = 25
                cmds(i).TabIndex = i + 1
                cmd_t = cmd_t + cmds(0).Height
            Next
            lst_tool_types.Left = 0
            lst_tool_types.Width = pnl.Width
            lst_tool_types.Top = pnl.Height - 100
            lst_tool_types.Height = 100


            cmd_down.TabIndex = i + 2
            cmd_down.Top = lst_tool_types.Top - 15
            cmd_down.Width = pnl.Width
            cmd_down.Height = 15
        End If
    End Sub
    Public Sub display_none()
        Dim i As Long
        last_displayed = -1
        last_offset = 0
        For i = 0 To max_visible_buttons
            cmds(i).Visible = False
        Next
        'If Me.mtype = FLOATING Then
        Me.pnl.Visible = False

        'End If
    End Sub
    Public Sub display_group(group_nr As Integer, Optional offset As Integer = 0)
        Static displaing As Boolean
        If displaing = False Then
            last_displayed = group_nr
            If (offset) + (max_visible_buttons) > Me.tcmnsc(group_nr).n And Me.tcmnsc(group_nr).n > max_visible_buttons Then
                offset = Me.tcmnsc(group_nr).n - max_visible_buttons
            End If
            If offset < 0 Then offset = 0
            last_offset = offset
            displaing = True
            lst_tool_types.SelectedIndex = group_nr
            positionate_controls(Me.tcmnsc(group_nr).n)

            Dim i As Integer
            For i = 0 To Me.tcmnsc(group_nr).n
                If i <= max_visible_buttons Then
                    If cmds(i).Height + cmds(i).Top < (cmd_down.Top + 2) Then
                        cmds(i).Visible = True
                        If i + offset <= Me.tcmnsc(group_nr).n Then
                            cmds(i).Tag = Me.tcmnsc(group_nr).cmt(i + offset)
                            cmds(i).Text = Me.tcmnsc(group_nr).cmt(i + offset).title
                            last_visible_button = i
                        Else
                            Exit For
                        End If
                    Else
                        Exit For
                    End If
                Else
                    Exit For
                End If
            Next

            'tlačítka, pro které zrovna nemáme uplatnění, skryjeme
            For i = i To max_visible_buttons
                cmds(i).Visible = False
            Next
            displaing = False
        End If
        positionate_controls(Me.max_visible_buttons)
    End Sub

    Public Sub cmd_click(sender As Object, e As EventArgs)
        If sender.tag IsNot Nothing Then
            sender.tag.tool.context_menu_activated(Me.parent.p, sender.tag.p1, sender.tag.p2)

            Me.parent.display_page(Nothing)
        End If
    End Sub
    Private Sub cmd_PreKeyDown(sender As Object, e As PreviewKeyDownEventArgs)
        Dim t As Timer = New Timer()
        AddHandler t.Tick, AddressOf tmr_tick
        t.Interval = 5

        If e.KeyValue = Keys.Down Then
            If sender.name = "cmd_" & last_visible_button Then 'tohle se bude uplatňovat jen a pouze na posledním, jinde to nejde
                display_group(last_displayed, last_offset + 1)
                'e.SuppressKeyPress = True
                t.Tag = cmds.Last
                t.Start()
            End If
        ElseIf e.KeyValue = Keys.Up Then
            If sender.name = "cmd_0" Then
                display_group(last_displayed, last_offset - 1)
                'e.SuppressKeyPress = True
                t.Tag = cmds(0)
                t.Start()
            End If
        End If
    End Sub
    Private Sub tmr_tick(sender As Object, e As EventArgs)
        sender.tag.select
        sender.stop
    End Sub

    Public Sub add_tool_cm(ttype As String, tool As Object, title As String, priority As Integer, p1 As Object, p2 As Object)
        Dim i As Long
        Dim found As Boolean = False
        For i = 0 To n_tool_types
            If tcmnsc(i).name = ttype Then 'tenhle typ kontextových nabídek už existuje
                found = True
                Exit For
            End If
        Next
        If found = False Then
            n_tool_types += 1
            ReDim Preserve tcmnsc(n_tool_types)
            i = n_tool_types
            tcmnsc(n_tool_types).n = -1
            tcmnsc(n_tool_types).name = ttype
        End If
        tcmnsc(i).n += 1 'tak do ní jen přidáme další sadu
        ReDim Preserve tcmnsc(i).cmt(tcmnsc(i).n)
        tcmnsc(i).cmt(tcmnsc(i).n).tool = tool
        tcmnsc(i).cmt(tcmnsc(i).n).p1 = p1
        tcmnsc(i).cmt(tcmnsc(i).n).p2 = p2
        tcmnsc(i).cmt(tcmnsc(i).n).title = Replace(title, vbLf, " ")
        tcmnsc(i).cmt(tcmnsc(i).n).priority = priority
    End Sub
End Class
Public Class cls_workspace
    Private __rtf_renders As Long
    Private __ln_renders As Long
    Private __lines_renders As Long
    Private __tag_renders As Long

    Public Class subcls_used_marks
        'bude uchovávat a dodávat informace o použitých značkách a jim přiřazených formátech
        Private marks() As String
        Private marks_description() As String
        Private meaningful() As Boolean

        Private hgl() As cls_highligh_rule
        Private n As Integer
        Private basic_hgl As cls_highligh_rule
        Public Function get_marks() As String()
            Return marks
        End Function
        Public Sub New()
            n = -1
            basic_hgl = New cls_highligh_rule("bc:white;fc:red3")
        End Sub
        Public Sub add_mark(mark_ As String, hgl_ As cls_highligh_rule, Optional meaningful_ As Boolean = False, Optional description As String = "")
            Dim i As Integer
            For i = 0 To n
                If marks(i) = mark_ Then
                    hgl(i) = hgl_
                    Exit Sub
                End If
            Next i
            n += 1
            ReDim Preserve hgl(n)
            ReDim Preserve marks(n)
            ReDim Preserve meaningful(n)
            ReDim Preserve marks_description(n)
            hgl(n) = hgl_
            marks(n) = mark_
            meaningful(n) = meaningful_
            marks_description(n) = description
        End Sub
        Public Function get_hgl(mark_ As String) As cls_highligh_rule
            Dim i As Integer
            For i = 0 To n
                If marks(i) = mark_ Then
                    If hgl(i) Is Nothing Then Return basic_hgl
                    Return hgl(i)
                End If
            Next
            Return basic_hgl
        End Function
        Public Function set_meaningful(mark_ As String, Optional meaningful_ As Boolean = True)
            Dim i As Long
            For i = 0 To Me.n
                If Me.marks(i) = mark_ Then
                    Me.meaningful(i) = meaningful_
                    Exit Function
                End If
            Next
        End Function
        Public Function set_description(mark_ As String, description_ As String)
            Dim i As Long
            For i = 0 To Me.n
                If Me.marks(i) = mark_ Then
                    Me.marks_description(i) = description_
                    Exit Function
                End If
            Next
        End Function
        Public Function is_meaningful(mark) As Boolean
            Dim i As Long
            For i = 0 To Me.n
                If Me.marks(i) = mark Then
                    Return Me.meaningful(i)
                End If
            Next
            Return False
        End Function
        Public Function get_desc(mark) As String
            Dim i As Long
            For i = 0 To Me.n
                If Me.marks(i) = mark Then
                    Return Me.marks_description(i)
                End If
            Next
        End Function
    End Class

    Public tm As cls_tools_manager
    Public doc As cls_preXML_document
    Public par As cls_environment_2
    Public p As cls_preXML_section_page

    Public p2(1) As cls_preXML_section_page

    Public cnm_fixed As cls_context_menu
    Public cnm_floating As cls_context_menu

    Public p_index As Integer

    Friend lastctrl As Control
    Friend thisctrl As Control
    Public rtb As RichTextBox
    Public rtb_(1) As RichTextBox
    Public lnpnl As Panel
    Public lnrtb As RichTextBox

    Public flti As Label
    Public rtb_locked As Boolean
    Public draw_formatted As Boolean
    Private Class scls_view
        Public continous_view_activated As Boolean
        Public prev_page_displayed As Boolean
        Public prev_page_lenght As Integer
        Public prev_page_rtf As String
        Public next_page_displayed As Boolean
        Public next_page_lenght As Integer
        Public next_page_rtf As String
        Public ac_page_lenght As Integer
        Public next_page_start0b As Integer
        Public Function acFI() As Integer 'ac_page_first_index
            If continous_view_activated = False Then Return 0
            If prev_page_displayed = False Then
                Return 0
            Else
                Return prev_page_lenght + 2
            End If
        End Function
        Public Function nextFI() As Integer 'ac_page_first_index
            If continous_view_activated = False Then Return ac_page_lenght + 1
            If next_page_displayed = False Then
                Return ac_page_lenght + 1
            Else
                Return next_page_start0b
            End If
        End Function
    End Class
    Private view As scls_view

    Private clrtbl As New cls_color_tbl

    Public hgl() As String
    Private last_hgl() As String

    Private redraw_tags_hgl As Boolean
    Private redraw_lines_hgl As Boolean

    Public def_clrtbl As cls_color_tbl
    Public marks As subcls_used_marks
    Public meaningful_marks As subcls_used_marks

    Private lbl_xml_before As Label
    Private lbl_xml_after As Label
    Private lbl_xml As Label

    Private stb As Panel
    Private stb_lbl1 As Label
    Private stb_info_lbl As Label
    Private stb_cmd_next_page As Button
    Private stb_cmd_prev_page As Button
    Private stb_cmd_refresh As Button
    Private stb_cmd_break_lines As Button
    Private stb_txt_pagenr As TextBox

    Public cmd_insert_text As Button
    Public cmd_inser_into_new As Button
    Public cmd_add_page As Button
    Public cmb_add_page_options As ComboBox
    Public tmr_show_add_page_options As Timer
    Public cmd_add_text_to_page As Button
    Private cmd_delete_page As Button


    Public cmd_expand_bar As Button
    Public bar_expanded As Boolean

    Public lst_marks As CheckedListBox

    Public container As Control
    Public tools_container As Control
    Public frm As Object

    Public tools As cls_tools_manager
    Public Event event_triggered(ev As Integer, page As cls_preXML_section_page, e As Object)

    Private pnl_search As Panel
    Private txt_search As TextBox
    Private txt_replace As TextBox
    Private chb_regex As CheckBox
    Private cmd_search As Button
    Private cmd_replace As Button
    Private cmb_where As ComboBox

    Private cmd_join_prev_page As Button
    Private cmd_join_next_page As Button

    Private trv_xml As TreeView
    Private mnu_trv_xml As ContextMenuStrip

    Private tmr_check_xml As Timer
    Public tmr_highlight As Timer
    Public tmr_cnm As Timer
    Public tmr_generate_rtf As Timer
    Public tmr_ln As Timer
    Private tmr_go_on_other_page As Timer
    Private go_on_other_page_delay As Long

    Public cmn As Panel

    Private rtb_left As Integer

    Private on_end_of_page As Integer
    Private on_begin_of_page As Integer

    Private pnl_pdw_expanded_view As New Panel
    Public textformat As TextDataFormat = TextDataFormat.Text

    Public Sub New()
        
    End Sub
    Private Sub create_stb()
        stb = New Panel
        stb.Parent = container
        stb.AutoSize = False
        Dim tmp As Button
        Dim chb As CheckBox

        With NewCtrl(stb_info_lbl, New Label)
            .Parent = stb
            .Dock = DockStyle.Left
            .Height = stb.Height
            stb_info_lbl.AutoSize = True
        End With
        With NewCtrl(chb, New CheckBox)
            .Parent = stb
            .Dock = DockStyle.Left
            .Height = .Parent.Height / 2
            .Width = 30
            chb.Appearance = Appearance.Button
            chb.Checked = Me.draw_formatted
            .Text = "F"
            AddHandler chb.CheckedChanged, AddressOf chb_draw_formatted_checked_changed
        End With
        With NewCtrl(tmp, New Button)
            .Parent = stb
            .Dock = DockStyle.Left
            .Height = .Parent.Height / 2
            .Width = 30
            .Text = "^"
            AddHandler .Click, AddressOf env_rtb_font_size_changed
        End With
        With NewCtrl(tmp, New Button)
            .Parent = stb
            .Top = .Parent.Height / 2
            .Dock = DockStyle.Left
            .Height = .Parent.Height / 2
            .Width = 30
            .Text = "˅"
            AddHandler .Click, AddressOf env_rtb_font_size_changed
        End With
        With NewCtrl(tmp, New Button)
            .Parent = stb
            .Dock = DockStyle.Left
            .Height = .Parent.Height
            .Width = 30
            .Text = "H"
            AddHandler .Click, AddressOf cmd_show_search_box_click
        End With

        With NewCtrl(stb_cmd_break_lines, New Button)
            .Parent = stb
            .Dock = DockStyle.Left
            .Height = .Parent.Height
            .Width = 30
            .Text = "Z"
            AddHandler .Click, AddressOf cmd_break_lines_click
        End With
        stb_cmd_refresh = New Button
        With stb_cmd_refresh
            .Parent = stb
            .Height = .Parent.Height
            .Width = 30
            .Text = "O"
            .Dock = DockStyle.Left
            AddHandler .Click, AddressOf cmd_refresh_click
        End With

        With NewCtrl(tmp, New Button)
            .Parent = stb
            .Width = 30
            .Height = .Parent.Height
            .Dock = DockStyle.Left
            .Text = ">|"
            AddHandler .Click, AddressOf cmd_next_page_click
        End With

        stb_cmd_next_page = New Button
        stb_cmd_next_page.Width = 30
        stb_cmd_next_page.Height = stb.Height
        stb_cmd_next_page.Text = ">"
        stb_cmd_next_page.Dock = DockStyle.Left
        stb_cmd_next_page.Parent = stb
        AddHandler stb_cmd_next_page.Click, AddressOf cmd_next_page_click

        stb_lbl1 = New Label
        stb_lbl1.Font = New Font("Calibri", 12)
        stb_lbl1.Width = 150
        stb_lbl1.TextAlign = ContentAlignment.MiddleCenter
        stb_lbl1.AutoSize = False
        stb_lbl1.Parent = stb
        stb_lbl1.Dock = DockStyle.Left
        AddHandler stb_lbl1.Click, AddressOf stb_lbl1_click
        With NewCtrl(stb_txt_pagenr, New TextBox)
            .Parent = stb
            .Visible = False
            AddHandler .KeyDown, AddressOf txt_pagenr_KeyDown
        End With

        stb_cmd_prev_page = New Button
        stb_cmd_prev_page.Width = 30
        stb_cmd_prev_page.Height = stb.Height
        stb_cmd_prev_page.Text = "<"
        stb_cmd_prev_page.Dock = DockStyle.Left
        stb_cmd_prev_page.Parent = stb
        AddHandler stb_cmd_prev_page.Click, AddressOf cmd_prev_page_click
        With NewCtrl(tmp, New Button)
            .Parent = stb
            .Width = 30
            .Height = .Parent.Height
            .Dock = DockStyle.Left
            .Text = "|<"
            AddHandler .Click, AddressOf cmd_prev_page_click
        End With
    End Sub
    Private Sub create_middle_panel()
        Dim ttt As ToolTip

        With NewCtrl(cmb_add_page_options, New ComboBox)
            .Left = 0
            .Top = 0
            .Width = 20 * env.environment_size
            .Height = 25 * env.environment_size
            .Text = "+"
            cmb_add_page_options.DropDownStyle = ComboBoxStyle.DropDownList
            cmb_add_page_options.FlatStyle = FlatStyle.Flat
            .Parent = container
            .Enabled = False
            cmb_add_page_options.DropDownWidth = 350 * env.environment_size
            AddHandler cmb_add_page_options.SelectedIndexChanged, AddressOf cmd_add_page_click

        End With
        With cmb_add_page_options.Items
            .Add(env.c("Přidej prázdnou stranu na konec dokumentu"))
            .Add(env.c("Přidej prázdnou stranu PŘED právě otevřenou stranu"))
            .Add(env.c("Přidej prázdnou stranu ZA právě otevřenou stranu"))
            .Add(env.c("Přidej stranu na konec dokumentu A VLOŽ TEXT"))
            .Add(env.c("Přidej stranu PŘED právě otevřenou stranu A VLOŽ TEXT"))
            .Add(env.c("Přidej stranu ZA právě otevřenou stranu A VLOŽ TEXT"))
            .Add("---------------------------")
            If Me.textformat = TextDataFormat.Rtf Then
                .Add(env.c("Vkládat jako text (nyní rtf)"))
            Else
                .Add(env.c("Vkládat jako rtf (nyní text)"))
            End If
        End With
        ttt = New ToolTip()
        ttt.ToolTipTitle = "Možnosti přidání stránky"
        ttt.SetToolTip(cmb_add_page_options, "Nabízí všechny možnosti přidání stránky a vložení textu do ní.")

        With NewCtrl(cmd_inser_into_new, New Button)
            .Left = cmb_add_page_options.Width
            .Top = 0
            .Width = 50 * env.environment_size
            .Height = 25 * env.environment_size
            .Text = "+ >>>"
            .Parent = container
            .Name = "cmd_add_and_insert"
            AddHandler .Click, AddressOf cmd_insert_text_click
            ttt = New ToolTip()
            ttt.ToolTipTitle = "Přidej stránku a vlož text"
            ttt.SetToolTip(cmd_inser_into_new,
                           "Přidá novou stránku za právě otevřenou stranu, vloží do ní text ve schránce a spustí nástroje reagující na událost vložení textu.")
        End With

        With NewCtrl(cmd_insert_text, New Button)
            .Left = 0
            .Top = lastctrl.Height + lastctrl.Top + (5 * env.environment_size)
            .Width = 25 * env.environment_size
            .Height = 25 * env.environment_size
            .Text = ">>>"
            .Parent = container
            .Enabled = False
            .Name = "cmd_insert_text"
            AddHandler .Click, AddressOf cmd_insert_text_click
            ttt = New ToolTip()
            ttt.ToolTipTitle = "Nahraď text stránky"
            ttt.SetToolTip(cmd_insert_text, "Smaže všechen text ve stránce, vloží nový ze schránky a spustí nástroje reagující na událost vložení textu.")
        End With
        With NewCtrl(cmd_add_text_to_page, New Button)
            .Left = lastctrl.Width + lastctrl.Left()
            .Top = lastctrl.Top
            .Width = 25 * env.environment_size
            .Height = 25 * env.environment_size
            .Text = "...>"
            .Parent = container
            .Enabled = False
            .Name = "cmd_add_text"
            AddHandler .Click, AddressOf cmd_insert_text_click
            ttt = New ToolTip()
            ttt.ToolTipTitle = "Přidej text do stránky"
            ttt.SetToolTip(cmd_add_text_to_page, "Vloží do aktuální stránky text ze schránky a spustí nástroje reagující na událost vložení textu.")
        End With
        With NewCtrl(cmd_delete_page, New Button)
            .Left = 0
            .Top = lastctrl.Height + lastctrl.Top + (5 * env.environment_size)
            .Width = 50 * env.environment_size
            .Height = 30 * env.environment_size
            .Parent = container
            .Enabled = False
            .Text = "Smazat stranu"
            AddHandler .Click, AddressOf cmd_delete_page_click
        End With


        With NewCtrl(lst_marks, New CheckedListBox)
            .Top = lastctrl.Top + lastctrl.Height
            '.Height = 200 * env.environment_size
            .Parent = container
            .Visible = True
            ttt = New ToolTip
            ttt.ToolTipTitle = "Zvýraznit"
            ttt.SetToolTip(lst_marks, "Zaškrtněte pro zvýraznění příslušných míst v textu")
            AddHandler lst_marks.ItemCheck, AddressOf lst_marks_item_check
            AddHandler lst_marks.MouseEnter, AddressOf middle_panel_box_mouse_enter
            AddHandler lst_marks.MouseLeave, AddressOf middle_panel_box_mouse_leave
        End With

        With NewCtrl(trv_xml, New TreeView)
            .Parent = container
            .Top = container.Height / 3
            AddHandler trv_xml.NodeMouseDoubleClick, AddressOf trv_xml_dbclicked
            AddHandler trv_xml.MouseEnter, AddressOf middle_panel_box_mouse_enter
            AddHandler trv_xml.MouseLeave, AddressOf middle_panel_box_mouse_leave
        End With
        mnu_trv_xml = New ContextMenuStrip()
        AddHandler mnu_trv_xml.ItemClicked, AddressOf mnu_trv_xml_itemclicked
        mnu_trv_xml.Items.Add("Zavřít vše až potud (KROMĚ tohoto elementu)").Name = "mnu_close_el_EX"
        mnu_trv_xml.Items.Add("Zavřít vše až potud (VČETNĚ tohoto elementu)").Name = "mnu_close_el_IN"



        bar_expanded = True
        With NewCtrl(cmd_expand_bar, New Button)
            .Left = 0
            .Parent = container
            .Text = ">"
            .Height = 25 * env.environment_size
            .Width = 25 * env.environment_size
            AddHandler .Click, AddressOf cmd_expand_bar_click
        End With
        'stb.Items.Add(New ToolStripControlHost(stb_lbl1))

        cmn = New Panel 'kontextové menu
        cmn.Parent = container
    End Sub
    Public Sub New(frm_ As Object, parent As cls_environment_2)
        parent.wsp = Me
        frm = frm_
        par = parent
        container = frm.get_page_rtb_container
        tools_container = frm.get_tools_container

        def_clrtbl = New cls_color_tbl
        def_clrtbl.create_default_clrtbl()

        marks = New subcls_used_marks()
        meaningful_marks = New subcls_used_marks()
        meaningful_marks.add_mark("~not_in_wortlist", Nothing, True, env.c("Slova nenalezená ve slovníku", "Words not found in the wordlist"))
        meaningful_marks.add_mark("~strange_symbols", Nothing, True, env.c("Podivné znaky", "Strange characters"))
        meaningful_marks.add_mark("~grrepl", Nothing, True, env.c("Hromadně nahrazené sekvence", "Replaced sequences"))
        meaningful_marks.add_mark("~bibl", Nothing, True, env.c("Sekce bibl. poznámek", "Bibl. notes"))
        meaningful_marks.add_mark("~critical_app", Nothing, True, env.c("Sekce kritického aparátu", "Critical app."))
        meaningful_marks.add_mark("~notes", Nothing, True, env.c("Sekce poznámek", "Notes"))
        meaningful_marks.add_mark("~rubr", Nothing, True, env.c("Sekce rubrik", "Rubriques"))
        meaningful_marks.add_mark("~search", Nothing, True, env.c("Vyhledání textu", "Text searching"))
        meaningful_marks.add_mark("~replace", Nothing, True, env.c("Nahrazený text", "Replaced text"))

        view = New scls_view

        tm = New cls_tools_manager(Me)

        AddHandler container.Resize, AddressOf adjust_controls
        AddHandler frm_main.SizeChanged, AddressOf adjust_controls
        rtb_(0) = New RichTextBox
        rtb_(0).Parent = container
        rtb_(0).BorderStyle = BorderStyle.None
        rtb_(1) = New RichTextBox
        rtb_(1).Parent = container
        rtb_(1).BorderStyle = BorderStyle.None
        rtb_(1).Visible = False

        lnpnl = New Panel
        lnpnl.Parent = container
        lnpnl.BorderStyle = BorderStyle.None
        lnpnl.BackColor = Color.LightBlue
        AddHandler lnpnl.Paint, AddressOf lnpnl_paint


        Dim evt = frm.GetType().GetEvent("KeyDown")
        evt.AddEventHandler(frm, New KeyEventHandler(AddressOf frm_keyDown))
        Dim evt2 = frm.GetType().GetEvent("KeyUp")
        evt2.AddEventHandler(frm, New KeyEventHandler(AddressOf frm_keyUp))
        Dim evt3 = frm.GetType().GetEvent("KeyPress")
        evt3.AddEventHandler(frm, New KeyPressEventHandler(AddressOf frm_keyPress))

        'AddHandler frm.KeyDown, AddressOf rtb_keyDown
        'AddHandler frm.KeyUp, AddressOf rtb_keyUp
        'AddHandler frm.KeyPress, AddressOf rtb_keyPress

        AddHandler rtb_(0).MouseClick, AddressOf rtb_mouseClick
        AddHandler rtb_(1).MouseClick, AddressOf rtb_mouseClick
        AddHandler rtb_(0).MouseDoubleClick, AddressOf rtb_mouseDblClick
        AddHandler rtb_(1).MouseDoubleClick, AddressOf rtb_mouseDblClick
        AddHandler rtb_(0).MouseDown, AddressOf rtb_mouseDown
        AddHandler rtb_(1).MouseDown, AddressOf rtb_mouseDown
        AddHandler rtb_(0).MouseUp, AddressOf rtb_mouseUp
        AddHandler rtb_(1).MouseUp, AddressOf rtb_mouseUp
        AddHandler rtb_(0).MouseMove, AddressOf rtb_mouseMove
        AddHandler rtb_(1).MouseMove, AddressOf rtb_mouseMove

        AddHandler rtb_(0).KeyDown, AddressOf rtb_keyDown
        AddHandler rtb_(1).KeyDown, AddressOf rtb_keyDown

        AddHandler rtb_(0).TextChanged, AddressOf rtb_textChanged
        AddHandler rtb_(1).TextChanged, AddressOf rtb_textChanged

        AddHandler rtb_(0).SelectionChanged, AddressOf rtb_selectionChanged
        AddHandler rtb_(1).SelectionChanged, AddressOf rtb_selectionChanged

        AddHandler rtb_(0).VScroll, AddressOf rtb_vscroll
        AddHandler rtb_(1).VScroll, AddressOf rtb_vscroll

        AddHandler rtb_(0).HScroll, AddressOf rtb_hscroll
        AddHandler rtb_(1).HScroll, AddressOf rtb_hscroll
        rtb_(0).CausesValidation = True
        rtb_(1).CausesValidation = True
        AddHandler rtb_(0).Validated, AddressOf rtb_validated
        AddHandler rtb_(1).Validated, AddressOf rtb_validated

        With NewCtrl(lbl_xml_before, New Label)
            .Parent = container
            lbl_xml_before.AutoSize = False
        End With
        With NewCtrl(cmd_join_prev_page, New Button)
            .Parent = lbl_xml_before
            .Text = "^"
            .Width = 50 * env.environment_size
            .Name = "cmd_join_prev"
            AddHandler .Click, AddressOf cmd_join_pages_click
        End With
        With NewCtrl(lbl_xml_after, New Label)
            .Parent = container
            lbl_xml_after.AutoSize = False
        End With
        With NewCtrl(cmd_join_next_page, New Button)
            .Parent = lbl_xml_after
            .Text = "˅"
            .Width = 50 * env.environment_size
            .Name = "cmd_join_next"
            AddHandler .Click, AddressOf cmd_join_pages_click
        End With
        With NewCtrl(lbl_xml, New Label)
            .Parent = lbl_xml_after
            lbl_xml_after.AutoSize = False
        End With
        flti = New Label
        flti.Parent = rtb
        flti.Visible = False


        Dim tmp As Button
        create_stb()
        create_middle_panel()
        ' stb.Items.Add(New ToolStripControlHost(stb_cmd_prev_page))



        ' rtb.Visible = False

        AddHandler par.on_document_opened, AddressOf Me.document_opened
        thisctrl = Nothing
        lastctrl = Nothing

        tmr_highlight = New Timer
        tmr_highlight.Interval = 50
        AddHandler tmr_highlight.Tick, AddressOf tmr_highlight_tick
        tmr_generate_rtf = New Timer
        tmr_generate_rtf.Interval = 100
        AddHandler tmr_generate_rtf.Tick, AddressOf tmr2_tick
        tmr_ln = New Timer
        tmr_ln.Interval = 100
        AddHandler tmr_ln.Tick, AddressOf tmr_ln_tick
        tmr_cnm = New Timer
        tmr_cnm.Interval = 50
        AddHandler tmr_cnm.Tick, AddressOf tmr_cnm_tick
        tmr_check_xml = New Timer()
        tmr_check_xml.Interval = 500
        AddHandler tmr_check_xml.Tick, AddressOf tmr_check_xml_tick

        tmr_go_on_other_page = New Timer
        tmr_go_on_other_page.Interval = 1000
        AddHandler tmr_go_on_other_page.Tick, AddressOf tmr_go_on_other_page_tick

        cnm_fixed = New cls_context_menu(Me, 0)
        cnm_floating = New cls_context_menu(Me, 1)

        'vytvoříme okénko s hledáním:
        pnl_search = New Panel
        pnl_search.Parent = container
        Dim lbl As Label
        With NewCtrl(lbl, New Label)
            .Parent = pnl_search
            .Text = env.c("Hledaný výraz")
            .Width = 100
        End With
        With NewCtrl(lbl, New Label)
            .Parent = pnl_search
            .Text = env.c("Nahrazení")
            .Width = 100
            .Top = lastctrl.Height + lastctrl.Top + 2
        End With
        With NewCtrl(txt_search, New TextBox)
            .Parent = pnl_search : .Left = 105 : .Name = "txt_search"
            AddHandler .KeyDown, AddressOf search_and_replace_activated_txtb
        End With
        With NewCtrl(txt_replace, New TextBox)
            .Parent = pnl_search : .Left = 105 : .Top = lastctrl.Top + lastctrl.Height + 2 : .Name = "txt_replace"
            AddHandler .KeyDown, AddressOf search_and_replace_activated_txtb
        End With
        With NewCtrl(cmd_search, New Button)
            .Parent = pnl_search : .Text = env.c("Najdi") : .Width = 75 : .Name = "cmd_search"
            AddHandler .Click, AddressOf search_and_replace_activated
        End With
        With NewCtrl(cmd_replace, New Button)
            .Parent = pnl_search : .Top = lastctrl.Top + lastctrl.Height + 2 : .Text = "Nahraď" : .Width = 75 : .Name = "cmd_replace"
            AddHandler .Click, AddressOf search_and_replace_activated
        End With
        With NewCtrl(chb_regex, New CheckBox)
            .Parent = pnl_search
            .Text = env.c("Reg. výr.")
            chb_regex.Appearance = Appearance.Button
            .Top = 0
            .Width = 75
        End With
        With NewCtrl(cmb_where, New ComboBox)
            .Parent = pnl_search
            .Top = lastctrl.Top + lastctrl.Height + 2
            .Width = 75
            cmb_where.Items.Add(env.c("Aktivní stránka"))
            cmb_where.Items.Add(env.c("Celý dokument"))
            cmb_where.Items.Add(env.c("Vybraný text"))
        End With
        Me.marks.add_mark("~BAD_CLOSING_TAG", New cls_highligh_rule("bc:red1"))
        rtb = rtb_(0)
        adjust_controls(Nothing, Nothing)

        Me.marks.add_mark("i", New cls_highligh_rule("bc:grey8"))
        Me.marks.add_mark("b", New cls_highligh_rule("bc:grey7"))
        Me.marks.add_mark("u", New cls_highligh_rule("bc:grey6"))
        Me.marks.add_mark("sm", New cls_highligh_rule("bc:grey5"))
    End Sub
    Public Sub adjust_controls(sender As Object, e As EventArgs)

        rtb_left = 50 * env.environment_size
        If bar_expanded = True Then rtb_left = 200 * env.environment_size

        Dim ctrl As Control
        stb.Dock = DockStyle.Bottom
        stb.Height = 40 * env.environment_size
        stb_lbl1.Height = stb.Height
        For Each ctrl In stb.Controls
            ctrl.Height = stb.Height
        Next

        lbl_xml_after.Top = stb.Top - lbl_xml_after.Height

        cmd_expand_bar.Left = rtb_left - cmd_expand_bar.Width
        cmd_expand_bar.Top = stb.Top - cmd_expand_bar.Height

        lbl_xml_before.Top = 0
        lbl_xml_before.Left = rtb_left
        lbl_xml_before.Width = container.Width - lbl_xml_before.Left
        lbl_xml_before.Height = 20 * env.environment_size
        cmd_join_prev_page.Height = cmd_join_prev_page.Parent.Height
        cmd_join_prev_page.Left = cmd_join_prev_page.Parent.Width - cmd_join_prev_page.Width

        'lbl_xml_after.Top = container.Height - stb.Height - lbl_xml_after.Height - 5
        lbl_xml_after.Left = rtb_left
        lbl_xml_after.Height = 20 * env.environment_size
        lbl_xml_after.Width = container.Width - lbl_xml_before.Left
        cmd_join_next_page.Height = cmd_join_next_page.Parent.Height
        cmd_join_next_page.Left = cmd_join_next_page.Parent.Width - cmd_join_next_page.Width
        cmd_join_next_page.BringToFront()


        lbl_xml.Left = lbl_xml.Parent.Width / 2
        lbl_xml.Height = lbl_xml.Parent.Height
        lbl_xml.Width = lbl_xml.Parent.Width / 2

        Dim search_panel_height As Integer
        If pnl_search.Tag = "docked" Then
            pnl_search.Width = rtb.Width
            pnl_search.Left = rtb_left
            pnl_search.Height = 54
            pnl_search.Top = lbl_xml_after.Top - pnl_search.Height
            If pnl_search.Visible = True Then
                search_panel_height = pnl_search.Height
                pnl_search.BringToFront()
            End If
            txt_search.Width = pnl_search.Width - (txt_search.Left + cmd_search.Width + chb_regex.Width + 10)
            txt_replace.Width = txt_search.Width
            cmd_search.Left = txt_search.Left + txt_search.Width + 5
            cmd_replace.Left = cmd_search.Left
            chb_regex.Left = cmd_search.Left + cmd_search.Width + 5
            cmb_where.Left = chb_regex.Left
        End If

        rtb_(0).Top = lbl_xml_before.Height + lbl_xml_before.Top
        rtb_(0).Left = rtb_left + lnpnl.Width
        rtb_(0).Width = container.Width - rtb_(0).Left
        rtb_(0).Height = lbl_xml_after.Top - rtb_(0).Top - search_panel_height

        rtb_(1).Top = lbl_xml_before.Height + lbl_xml_before.Top
        rtb_(1).Left = rtb_left + lnpnl.Width
        rtb_(1).Width = container.Width - rtb_(1).Left
        rtb_(1).Height = lbl_xml_after.Top - rtb_(1).Top - search_panel_height


        lnpnl.Top = rtb.Top
        lnpnl.Left = rtb_left
        lnpnl.Width = 40 * env.environment_size
        lnpnl.Height = rtb.Height

        cmd_insert_text.Width = rtb_left / 2

        cmd_add_text_to_page.Left = cmd_insert_text.Width + cmd_insert_text.Left
        cmd_add_text_to_page.Width = cmd_insert_text.Width

        cmd_inser_into_new.Left = cmb_add_page_options.Width
        cmd_inser_into_new.Width = rtb_left - cmd_inser_into_new.Left
        cmd_delete_page.Width = rtb_left

        lst_marks.Width = rtb_left
        lst_marks.Height = (lst_marks.Parent.Height / 3) - lst_marks.Top
        If lst_marks.Height < 100 Then lst_marks.Height = 100



        If bar_expanded = True Then cmd_expand_bar.Text = "<" Else cmd_expand_bar.Text = ">"

        Dim enable As Boolean = CBool(Me.doc IsNot Nothing)
        For Each ctrl In container.Controls
            ctrl.Enabled = enable
        Next
        If p Is Nothing Then cmd_insert_text.Enabled = False
        stb.Enabled = enable
        'stb.BackColor = Color.FromArgb(153, 213, 234)

        cnm_fixed.pnl.Width = rtb_left
        cnm_fixed.pnl.Height = cnm_fixed.pnl.Parent.Height / 3
        If cnm_fixed.pnl.Height > 400 Then cnm_fixed.pnl.Height = 400
        If cnm_fixed.pnl.Height < 150 Then cnm_fixed.pnl.Height = 150
        cnm_fixed.pnl.Top = cmd_expand_bar.Top - cnm_fixed.pnl.Height

        trv_xml.Top = lst_marks.Top + lst_marks.Height + 5 * env.environment_size
        trv_xml.Height = cnm_fixed.pnl.Top - trv_xml.Top
        trv_xml.Width = rtb_left
        trv_xml.Indent = 10
        trv_xml.ShowPlusMinus = False
    End Sub

    Public Sub actualize_info_controls()
        If p IsNot Nothing Then
            Me.frm.text = Me.doc.name & program_version
            With stb_info_lbl
                Dim column As Long
                Dim character As String
                Dim chasci As String
                If p.SelStart1b > 0 Then character = Mid(p.plain_text, p.SelStart1b, 1)
                If character <> "" Then chasci = CStr(AscW(character))
                column = p.SelStart1b - p.line_start_index(p.line_from_char_index(p.SelStart0b))
                .Text = env.c("Řádek") & ": " & p.line_from_char_index(p.SelStart0b) + 1 & ", " & env.c("sloupec") & ": " & column
                .Text &= vbNewLine & LCase(p.context.word) & " " & UCase(p.context.word) & "; " & env.c("znak") & ": '" & character &
                    "' (" & chasci & ")"

                '.Text &= " rtf: " & __rtf_renders & " ln: " & __ln_renders & " uln: " & __lines_renders & " tag: " & __tag_renders
            End With
            Dim i As Long
            Dim n As TreeNode
            Dim rn As TreeNode
            'trv_xml.Nodes.Clear()

            If p.context.n_tags_opened > -1 Then
                If trv_xml.Nodes.Count > 0 Then
                    rn = trv_xml.Nodes(0)
                Else
                    rn = trv_xml.Nodes.Add("")
                End If
                For i = 0 To p.context.n_tags_opened

                    rn.ContextMenuStrip = mnu_trv_xml
                    If rn.Text <> p.context.tags_opened(i).ToString Then
                        rn.Nodes.Clear()
                        rn.Text = (p.context.tags_opened(i).ToString)
                        rn.Tag = i
                        If i < p.context.n_tags_opened Then
                            If rn.NodeFont IsNot Nothing Then rn.NodeFont = env.def_font
                            rn = rn.Nodes.Add("")
                        Else
                            'Stop
                        End If
                    Else
                        If rn.Nodes.Count <> 0 Then
                            If i = p.context.n_tags_opened Then
                                rn.Nodes.Clear()
                            Else
                                rn = rn.FirstNode
                            End If
                            If rn.NodeFont IsNot Nothing Then rn.NodeFont = env.def_font
                        ElseIf i < p.context.n_tags_opened Then
                            If rn.NodeFont IsNot Nothing Then rn.NodeFont = env.def_font
                            rn = rn.Nodes.Add("")
                        End If
                    End If
                Next
                If rn IsNot Nothing Then
                    rn.Nodes.Clear()
                    rn.NodeFont = New Font(trv_xml.Font.FontFamily, CSng(env.def_font.Size * 1.2), FontStyle.Bold)
                    rn.Text = rn.Text
                End If

                trv_xml.ExpandAll()
            Else
                trv_xml.Nodes.Clear()
            End If
        Else
            stb_info_lbl.Text = ""
            trv_xml.Nodes.Clear()
            lst_marks.Items.Clear()
        End If
    End Sub
    Private Sub chb_draw_formatted_checked_changed(sender As Object, e As EventArgs)
        Me.draw_formatted = sender.checked
        rtf(Nothing, Nothing)
    End Sub

    Private Sub cmd_join_pages_click(sender As Object, e As EventArgs)
        If p IsNot Nothing Then
            Dim ni As Integer
            If sender.name = "cmd_join_prev" Then
                ni = p.m_index - 1
                If ni > -1 Then
                    doc.join_pages(p.m_index - 1, p.m_index)
                    Me.redraw_lines_hgl = True
                    Me.redraw_tags_hgl = True
                    Me.p = Nothing
                    Me.open_page(ni)
                End If
            ElseIf sender.name = "cmd_join_next" Then
                ni = p.m_index
                If ni < doc.n_pages Then
                    doc.join_pages(p.m_index, p.m_index + 1)
                    Me.redraw_lines_hgl = True
                    Me.redraw_tags_hgl = True
                    Me.p = Nothing
                    Me.open_page(ni)
                End If
            End If
        End If
    End Sub
    Private Sub middle_panel_box_mouse_enter(sender As Object, e As EventArgs)
        sender.Width = 300 * env.environment_size
        sender.BringToFront()
    End Sub

    Private Sub middle_panel_box_mouse_leave(sender As Object, e As EventArgs)
        sender.Width = rtb_left
    End Sub
    Private Sub trv_xml_dbclicked(sender As Object, e As TreeNodeMouseClickEventArgs)
        trv_xml.ExpandAll()
        If env._p IsNot Nothing Then
            Dim page As Integer
            Dim pos As Integer
            If trv_xml.SelectedNode IsNot Nothing Then
                page = env._p.context.tags_opened(trv_xml.SelectedNode.Tag).position.X
                pos = env._p.context.tags_opened(trv_xml.SelectedNode.Tag).position.Y
                If page <> env._p.m_index Then
                    env.wsp.open_page(page)
                End If
                rtb.SelectionStart = pos + view.acFI
                rtb.Select()
            End If
        End If
    End Sub
    Private Sub mnu_trv_xml_itemclicked(sender As Object, e As ToolStripItemClickedEventArgs)
        If Me.p IsNot Nothing Then
            If e.ClickedItem.Name = "mnu_close_el_EX" Then
                tm.evoke("", "close_all_up_to_NR", Me.p, True, p.context.n_tags_opened - trv_xml.SelectedNode.Tag, False)
                Me.display_page(Nothing, Nothing,,, 1)
            Else
                tm.evoke("", "close_all_up_to_NR", Me.p, True, p.context.n_tags_opened - trv_xml.SelectedNode.Tag, True)
                Me.display_page(Nothing, Nothing,,, 1)
            End If
        End If
    End Sub
    Private Sub env_rtb_font_size_changed(sender As Object, e As EventArgs)
        If sender.text = "^" Then
            env.rtb_font_size += 2
        Else
            If env.rtb_font_size > 8 Then
                env.rtb_font_size -= 2
            End If
        End If
        display_page(Nothing, Nothing,,, 1)
    End Sub
    Private Sub search_and_replace_activated(sender As Object, e As EventArgs)
        If Not Me.p Is Nothing Then
            If sender.name = "cmd_search" Then
                do_search()
            ElseIf sender.name = "cmd_replace" Then
                do_replace()
            End If
        End If
    End Sub
    Private Sub search_and_replace_activated_txtb(sender As Object, e As KeyEventArgs)
        If sender.name = "txt_search" Then
            If e.KeyValue = Keys.Enter Then
                do_search()
            End If
        ElseIf sender.name = "txt_replace" Then
            If e.KeyValue = Keys.Enter Then
                do_replace()
            End If
        End If
    End Sub
    Private Sub do_search()
        If cmb_where.SelectedIndex = -1 Then cmb_where.SelectedIndex = 0
        If cmb_where.SelectedIndex = 0 Then
            Me.p.search_and_highlight(chb_regex.Checked, txt_search.Text)
        ElseIf cmb_where.SelectedIndex = 1 Then
            Me.doc.search_and_highlight(chb_regex.Checked, txt_search.Text)
        End If
        Me.display_page(Nothing,,, True)
    End Sub
    Private Sub do_replace()
        If cmb_where.SelectedIndex = -1 Then cmb_where.SelectedIndex = 0
        If cmb_where.SelectedIndex = 0 Then
            Me.p.search_and_replace(chb_regex.Checked, txt_search.Text, txt_replace.Text)
        ElseIf cmb_where.SelectedIndex = 1 Then
            Me.doc.search_and_replace(chb_regex.Checked, txt_search.Text, txt_replace.Text)
        End If
        Me.display_page(Nothing,,, True)
    End Sub
    Private Sub tmr_highlight_tick(sender As Object, e As System.EventArgs)
        'se zpožděním vykreslí text - třeba při psaní, aby se v průběhu zadávání textu pořád nepřekresloval
        If p IsNot Nothing Then
            highlight_tags(True)

        End If
        tmr_highlight.Stop()
    End Sub
    Private Sub tmr_cnm_tick(sender As Object, e As EventArgs)
        'se zpožděním vygeneruje kontextové menu
        If p IsNot Nothing And Me.cnm_fixed IsNot Nothing Then
            Me.cnm_fixed.generate_context_menu()
            'Me.highlight_tags(True)
        End If
        tmr_cnm.Stop()
    End Sub
    Private Sub tmr_check_xml_tick(sender As Object, e As EventArgs)
        If p IsNot Nothing Then
            p.check_xml(False)
        End If
        tmr_check_xml.Stop()
    End Sub
    Private Sub cmd_show_search_box_click(sender As Object, e As EventArgs)
        show_search_box(True)
    End Sub
    Private Sub show_search_box(docked As Boolean)
        If Me.p IsNot Nothing Then
            Me.pnl_search.Visible = Not Me.pnl_search.Visible
            If docked = True Then Me.pnl_search.Tag = "docked"
            adjust_controls(Nothing, Nothing)
        End If
    End Sub
    Private Sub cmd_break_lines_click(sender As Object, e As EventArgs)
        If rtb.ScrollBars = RichTextBoxScrollBars.Both Or rtb.ScrollBars = RichTextBoxScrollBars.ForcedBoth Then

            rtb_(0).ScrollBars = RichTextBoxScrollBars.ForcedVertical
            rtb_(1).ScrollBars = RichTextBoxScrollBars.ForcedVertical
            rtb_(0).WordWrap = True
            rtb_(1).WordWrap = True
        Else
            rtb_(0).ScrollBars = RichTextBoxScrollBars.ForcedBoth
            rtb_(1).ScrollBars = RichTextBoxScrollBars.ForcedBoth
            rtb_(0).WordWrap = False
            rtb_(1).WordWrap = False
            'generate_rtf(Nothing)
        End If
    End Sub

    Private Sub update_marks_list()
        If Me.p IsNot Nothing Then
            Dim str() As String
            Dim i As Integer
            str = p.marks()
            If str IsNot Nothing Then
                lst_marks.Items.Clear()
                lst_marks.Items.Add("Vše")
                For i = 0 To UBound(str)
                    If env.wsp.meaningful_marks.is_meaningful(str(i)) Then

                        If Left(str(i), 1) <> "?" And Left(str(i), 1) <> "\" Then
                            lst_marks.Tag = "locked"
                            Dim text As String
                            text = env.wsp.meaningful_marks.get_desc(str(i)) & " (" & str(i) & ")"
                            If last_hgl IsNot Nothing Then
                                lst_marks.Items.Add(text, last_hgl.Contains(str(i)))
                            Else
                                lst_marks.Items.Add(text, False)
                            End If
                            lst_marks.Tag = ""
                        End If
                    End If
                Next
            Else
                lst_marks.Items.Clear()
            End If

        End If

    End Sub
    Private Sub lst_marks_item_check(sender As Object, e As ItemCheckEventArgs)
        If lst_marks.Tag = "locked" Then Exit Sub
        Dim i As Integer, j As Integer
        Dim n As Integer
        n = -1
        j = 0
        'ReDim last_hgl(lst_marks.Items.Count - 2)
        For i = 1 To lst_marks.Items.Count - 1
            If (lst_marks.GetItemCheckState(i) = CheckState.Checked And i <> e.Index) Or (i = e.Index And e.NewValue = CheckState.Checked) Then
                n = n + 1
            End If
        Next
        If n <> -1 Then
            ReDim Preserve hgl(n)
            For i = 1 To lst_marks.Items.Count - 1
                If (lst_marks.GetItemCheckState(i) = CheckState.Checked And i <> e.Index) Or (i = e.Index And e.NewValue = CheckState.Checked) Then
                    hgl(j) = rgx_g(lst_marks.Items(i), "\(([^)]*)\)")
                    j = j + 1
                End If
            Next
        Else
            hgl = Split("")
        End If
        display_page(hgl)
    End Sub

    Private Sub tmr_show_add_page_options_tick(sender As Object, e As EventArgs)

    End Sub
    Private Sub cmd_add_page_mousedown(sender As Object, e As MouseEventArgs)

    End Sub
    Private Sub cmd_add_page_mouseup(sender As Object, e As MouseEventArgs)

    End Sub

    Private Sub cmd_add_page_leave(sender As Object, e As EventArgs)

    End Sub

    Public Sub cmd_add_page_click(sender As Object, e As EventArgs)
        If doc IsNot Nothing Then
            Select Case cmb_add_page_options.SelectedIndex
                Case 0
                    open_page(doc.new_page(-1).m_index)
                Case 1
                    If p IsNot Nothing Then open_page(doc.new_page(p.m_index).m_index)
                Case 2
                    If p IsNot Nothing Then open_page(doc.new_page(p.m_index + 1).m_index)
                Case 3
                    open_page(doc.new_page(-1).m_index)
                    If Me.p IsNot Nothing Then p.insert_text_from_clipboard(2)
                    RaiseEvent event_triggered(EN.evn_TEXT_INSERTED, p, Nothing)
                Case 4
                    If p IsNot Nothing Then open_page(doc.new_page(p.m_index).m_index)
                    If Me.p IsNot Nothing Then p.insert_text_from_clipboard(2)
                    RaiseEvent event_triggered(EN.evn_TEXT_INSERTED, p, Nothing)
                Case 5
                    If p IsNot Nothing Then open_page(doc.new_page(p.m_index + 1).m_index)
                    If Me.p IsNot Nothing Then p.insert_text_from_clipboard(2)
                    RaiseEvent event_triggered(EN.evn_TEXT_INSERTED, p, Nothing)
                Case 7
                    If cmb_add_page_options.SelectedItem = env.c("Vkládat jako text (nyní rtf)") Then
                        Me.textformat = TextDataFormat.UnicodeText
                        cmb_add_page_options.Items.RemoveAt(7)
                        cmb_add_page_options.Items.Add(env.c("Vkládat jako rtf (nyní text)"))
                    Else
                        Me.textformat = TextDataFormat.Rtf
                        cmb_add_page_options.Items.RemoveAt(7)
                        cmb_add_page_options.Items.Add(env.c("Vkládat jako text (nyní rtf)"))
                    End If
            End Select
        End If
    End Sub
    Public Sub cmd_delete_page_click(sender As Object, e As EventArgs)
        If doc IsNot Nothing Then
            If p IsNot Nothing Then
                Dim page_to_open As Integer = doc.delete_page(Me.p.m_index, 0)
                Me.p = Nothing
                open_page(page_to_open)
            End If
        End If
    End Sub
    Public Sub cmd_insert_text_click(sender As Object, e As EventArgs)
        If doc IsNot Nothing Then
            Dim mode As Long
            If sender.name = "cmd_add_and_insert" Then
                If p IsNot Nothing Then
                    Me.open_page(doc.new_page(p.m_index + 1).m_index)
                Else
                    Me.open_page(doc.new_page().m_index)
                End If
                mode = 2
            ElseIf sender.name = "cmd_add_text" Then
                mode = 2
            ElseIf sender.name = "cmd_insert_text" Then
                mode = 1
            End If
            If p IsNot Nothing Then
                p.insert_text_from_clipboard(mode)
                RaiseEvent event_triggered(EN.evn_TEXT_INSERTED, p, Nothing)
                Me.display_page(last_hgl,,,, 10)
            End If
        End If
    End Sub

    Public Sub raise_event(en_ As Long, p As cls_preXML_section_page, e As Object)
        RaiseEvent event_triggered(en_, p, e)
    End Sub
    Private Sub add_to_tmpmd(ByRef tmpmd() As String, value As String)
        'přidá prvek do stringového pole
        If tmpmd IsNot Nothing Then
            ReDim Preserve tmpmd(UBound(tmpmd) + 1)
        Else
            ReDim tmpmd(0)
        End If
        tmpmd(UBound(tmpmd)) = value
    End Sub

    Public Function rtf_to_prepreXML(rtf As String, ByRef md()() As String, Optional ByRef page_rtf As String = "") As String

        Dim trtb As RichTextBox
        trtb = New RichTextBox

        rtf = Replace(rtf, "<", "&lt;")
        rtf = Replace(rtf, "\'3E}", "&gt;}")
        rtf = Replace(rtf, "\'3E ", "&gt; ")
        rtf = Replace(rtf, "\'3E\", "&gt;\")
        rtf = Replace(rtf, ">", "&gt;")

        trtb.Rtf = rtf
        Dim i As Integer
        Dim j As Integer
        Dim txt As String
        Dim pre_f As Font = New Font("calibri", 1)
        Dim pre_sup As Boolean
        Dim f As Font
        Dim chrtf As String
        Dim ch As String
        Dim pch As String
        Dim fsize() As Integer
        Dim nfsize As Integer = -1
        Dim count_for_av_size = 10



        ReDim md(Len(trtb.Text) - 1)
        page_rtf &= rgx(rtf, "\\paperw[0-9]+")
        page_rtf &= rgx(rtf, "\\margl[0-9]+")
        page_rtf &= rgx(rtf, "\\margr[0-9]+")
        For i = 0 To Len(trtb.Text) - 1

            trtb.SelectionStart = i
            trtb.SelectionLength = 1
            ch = Mid(trtb.Text, trtb.SelectionStart + 1, 1)
            'xyz = Mid(trtb.Text, i + 1, 100)
            'If ch = "," Then Stop
            chrtf = trtb.SelectedRtf
            f = trtb.SelectionFont
            If ch <> " " And ch <> vbLf And ch <> vbTab And ch <> "." Then
                nfsize += 1
                If nfsize > count_for_av_size Then
                    nfsize = count_for_av_size
                    For j = 0 To count_for_av_size - 1
                        fsize(j) = fsize(j + 1)
                    Next j
                Else
                    ReDim Preserve fsize(nfsize)
                End If
                fsize(nfsize) = f.Size
                xyz = fsize.Average
                add_to_tmpmd(md(i), "\fs" & Math.Round(f.Size * env.environment_size, 0))
                If f.Size - fsize.Average() < -2 Then
                    add_to_tmpmd(md(i), "sm")
                End If
                If f.Size - fsize.Average() > 2 Then
                    add_to_tmpmd(md(i), "lg")
                End If
                If pch = vbLf Or i = 0 Then

                    Dim v As String
                    v = rgx_g(chrtf, "\\li([0-9]+)")
                    If v <> "" Then
                        If CInt(v > 100) Then
                            'xyz = Mid(trtb.Text, i, 100)
                            add_to_tmpmd(md(i), "\li" & v)
                        End If
                    End If
                    v = rgx_g(chrtf, "\\sb([0-9]+)")
                    If v <> "" Then
                        If CInt(v > 100) Then
                            'xyz = Mid(trtb.Text, i, 100)
                            add_to_tmpmd(md(i), "\sb" & v)
                        End If
                    End If
                    v = rgx_g(chrtf, "\\sl([0-9]+)")
                    If v <> "" Then
                        If CInt(v > 100) Then
                            'xyz = Mid(trtb.Text, i, 100)
                            add_to_tmpmd(md(i), "\sl" & v)
                        End If
                    End If
                End If
            End If

            If f.Bold = True Then
                add_to_tmpmd(md(i), "b")
            End If
            If f.Italic = True Then
                add_to_tmpmd(md(i), "i")
            End If
            If f.Underline = True Then
                add_to_tmpmd(md(i), "u")
            End If
            pch = ch
        Next i
        Return trtb.Text
    End Function

    Public Sub cmd_next_page_click(sender As Object, e As EventArgs)
        If sender.text = ">" Then
            open_page(p_index + 1)
        ElseIf sender.text = ">|" Then
            open_page(Me.doc.n_pages)
        End If
        stb_txt_pagenr.Visible = False
    End Sub
    Public Sub cmd_prev_page_click(sender As Object, e As EventArgs)
        If sender.text = "<" Then
            open_page(p_index - 1)
        ElseIf sender.text = "|<" Then
            open_page(0)
        End If
        stb_txt_pagenr.Visible = False
    End Sub
    Private Sub stb_lbl1_click(sender As Object, e As EventArgs)
        With stb_txt_pagenr
            .Top = sender.top
            .Left = sender.left
            .Width = sender.width
            .Height = sender.height
            .Text = Me.p.m_index + 1
            .Visible = True
            .BringToFront()
        End With
    End Sub
    Private Sub txt_pagenr_KeyDown(sender As Object, e As KeyEventArgs)
        'změna otevřené stránky přímým zadáním
        If e.KeyCode = Keys.Enter Then
            If rgxt(sender.text, "^[0-9]+$") = True Then
                Dim nr As Integer
                nr = CInt(sender.text)
                If nr > 0 And nr <= Me.doc.n_pages + 1 Then
                    open_page(nr - 1)
                    sender.visible = False
                End If
            End If
        ElseIf e.KeyCode = Keys.Escape Then
            sender.visible = False
        End If
    End Sub
    Public Sub cmd_refresh_click(sender As Object, e As EventArgs)
        If p IsNot Nothing Then
            Me.display_page(Nothing,,,, 0)
        End If
    End Sub
    Public Sub cmd_expand_bar_click(sender As Object, e As EventArgs)
        bar_expanded = Not bar_expanded
        adjust_controls(Nothing, Nothing)
    End Sub

    Public Sub document_opened(ByRef d As cls_preXML_document, open_on_page As Integer)
        doc = d
        AddHandler doc.page_added, AddressOf document_page_added
        open_page(open_on_page)
        adjust_controls(Nothing, Nothing)
    End Sub
    Public Sub open_page(p_nr As Integer, Optional open_with_selStart As Integer = -1)
        If p_nr > -1 And p_nr <= doc.n_pages And doc.n_pages <> -1 Then
            'v pořádku...
        ElseIf p_nr > doc.n_pages And doc.n_pages <> -1 Then
            p_nr = doc.n_pages

        ElseIf p_nr < 0 And doc.n_pages <> -1 Then
            p_nr = 0
        Else
            p_index = -1
            p_nr = -1
            p = Nothing
            p2(0) = Nothing
            p2(1) = Nothing
            Me.rtb.Text = ""
        End If
        If p_nr <> -1 Then
            If p IsNot Nothing Then
                'RaiseEvent page_closed(p)
                RemoveHandler p.xml_checked, AddressOf page_xml_checked
                RaiseEvent event_triggered(EN.evn_PAGE_CLOSED, p, Nothing)

                Me.doc.save_page(p.m_index)
            End If


            p = doc.page(p_nr)

            continous_page_view = False
            If continous_page_view = True Then
                view.prev_page_lenght = 0
                If p_nr > 0 Then
                    p2(0) = doc.page(p_nr - 1)
                Else
                    view.prev_page_displayed = False
                End If
                If p_nr < doc.n_pages Then
                    p2(1) = doc.page(p_nr + 1)
                Else
                    view.next_page_displayed = False
                End If
                view.next_page_rtf = ""
                view.prev_page_rtf = ""
            End If
            If open_with_selStart <> -1 Then
                p.force_SelStart = open_with_selStart
                p.plain_text_selection_changed(open_with_selStart, 0)
            End If

            p.get_Opened_elements()

            If p IsNot Nothing Then
                'RaiseEvent page_opened(p)

                RaiseEvent event_triggered(EN.evn_PAGE_OPENED, p, Nothing)

                AddHandler p.xml_checked, AddressOf page_xml_checked_evh
            End If

            p_index = p_nr
            p.get_all_marks()

            rtb_locked = True
            rtb_(0).Clear()
            rtb_(1).Clear()
            rtb_locked = False

            Me.display_page(last_hgl)
            rtb.SelectionStart = 0 + view.acFI

        End If
        env._p = p 'zkratka 

        on_end_of_page = 0
        on_begin_of_page = 0
        tmr_go_on_other_page.Tag = False
        update_doc_info_controls()
    End Sub

    Public Property continous_page_view() As Boolean
        Set(value As Boolean)
            view.continous_view_activated = value
            If view.continous_view_activated = True Then
                If Me.p IsNot Nothing Then
                    If Me.p.m_index > 0 Then p2(0) = Me.doc.page(Me.p.m_index - 1)
                    If Me.p.m_index < Me.doc.n_pages Then p2(1) = Me.doc.page(Me.p.m_index + 1)
                End If
            End If
        End Set
        Get
            Return view.continous_view_activated
        End Get
    End Property

    Public Sub page_xml_checked_evh(no_rendering As Boolean)
        'Stop
        page_xml_checked(no_rendering)
    End Sub
    Public Sub page_xml_checked(no_rendering As Boolean)
        If p IsNot Nothing Then
            Dim i As Long
            lbl_xml_before.Text = env.c("XML elementy otevřené na začátku této strany: ")
            lbl_xml_after.Text = env.c("XML elementy otevřené na konci této strany: ")
            If p.elements_found_opened IsNot Nothing Then
                For i = 0 To UBound(p.elements_found_opened)
                    lbl_xml_before.Text &= " > " & p.elements_found_opened(i).name
                Next
            End If
            If p.elements_left_opened IsNot Nothing Then
                For i = 0 To UBound(p.elements_left_opened)
                    lbl_xml_after.Text &= " > " & p.elements_left_opened(i).name
                Next
            End If

            lbl_xml.Width = stb.Width - lbl_xml.Left
            lbl_xml.Height = stb.Height
            lbl_xml.AutoSize = False
            If p.XML_error = 0 Then
                lbl_xml.BackColor = SystemColors.ButtonFace
                lbl_xml.Text = env.c("XML v pořádku")
                lbl_xml.BackColor = Color.SpringGreen
                lbl_xml.ForeColor = Color.Black
            Else
                lbl_xml.BackColor = Color.Red
                lbl_xml.ForeColor = Color.Black

                lbl_xml.Text = env.c("Chyba XML")
                If p.XML_error = p.XML_ERRORS.BAD_CLOSING_TAG Then
                    lbl_xml.Text = env.c("CHYBA V XML: Uzavírá se tag, který není poslední otevřený.")
                ElseIf p.XML_error = p.XML_ERRORS.NONCLOSED_TAG Then
                    lbl_xml.Text = env.c("CHYBA V XML: Neuzavřený tag.")
                ElseIf p.XML_error = p.XML_ERRORS.ERROR_ON_PREVIOUS_PAGE Then
                    lbl_xml.Text = "CHYBA V XML: chyba na straně " & p.XML_error_on_page + 1
                End If
            End If
        End If
        If no_rendering = False Then display_page(Nothing) 'xml kontrolujeme třeba při každém zpožděném překreslení textu, takže nemá smysl text překreslovat dvakrát 
        'těsně za sebou
    End Sub
    Public Sub document_page_added(inserted_after As Integer, id As String)
        If p_index = -1 Then open_page(doc.n_pages)
        update_doc_info_controls()

    End Sub
    Public Sub update_doc_info_controls()
        If doc IsNot Nothing Then
            If doc.n_pages <> -1 Then
                If p_index <> -1 Then
                    stb_lbl1.Text = env.c("Strana") & " " & p_index + 1 & env.c(" z ") & doc.n_pages + 1
                    cmd_insert_text.Enabled = True
                    page_xml_checked(True)
                Else
                    stb_lbl1.Text = env.c("Strana") & " ---" & env.c(" z ") & doc.n_pages + 1
                    cmd_insert_text.Enabled = False
                End If
            Else
                p_index = -1
                stb_lbl1.Text = env.c("Prázdný dokument")
                cmd_insert_text.Enabled = False
            End If
            actualize_info_controls()
        Else
            stb_lbl1.Text = env.c("Není otevřen žádný dokument")
            actualize_info_controls()
        End If
    End Sub

    Private grt_to_highlight() As String
    Private grt_to_highlight_add() As String
    Private grt_highlight_XML_tags As Boolean
    Private Sub tmr2_tick(sender As Object, e As EventArgs)
        If p IsNot Nothing Then p.check_xml(True)
        rtf(grt_to_highlight, grt_to_highlight_add, grt_highlight_XML_tags)
        tmr_generate_rtf.Stop()
        tmr_generate_rtf.Interval = 50
    End Sub
    Public Sub display_page(to_highlight() As String, Optional to_highlight_add() As String = Nothing, Optional highlight_XML_tags As Boolean = True,
                            Optional force As Boolean = False, Optional interval As Integer = 50)
        If interval < 1 Then force = True
        If force = False Then
            grt_to_highlight = to_highlight
            grt_to_highlight_add = to_highlight_add
            grt_highlight_XML_tags = highlight_XML_tags
            tmr_generate_rtf.Stop()
            tmr_generate_rtf.Interval = interval
            tmr_generate_rtf.Start()
            update_marks_list()
        Else
            rtf(to_highlight, to_highlight_add, highlight_XML_tags)
        End If
    End Sub




    Public Function generate_rtf(page As cls_preXML_section_page, to_highlight() As String, Optional to_highlight_add() As String = Nothing,
                                 Optional highlight_XML_tags As Boolean = True, Optional dimmed As Boolean = False) As String
        'Static n_refreshes As Integer
        'n_refreshes += 1
        'Me.frm.text = n_refreshes

        If page IsNot Nothing Then
            Dim f_time As Integer
            f_time = Environment.TickCount
            Me.rtb_locked = True


            Dim tag_fc(2) As Long
            Dim element_hgl(2) As Long
            Dim tag_fc_index_to_use As Long
            Dim inside_of_head As Boolean = False
            Dim element_hgl_index_to_use As Long
            tag_fc(0) = 21
            tag_fc(1) = 19
            tag_fc(2) = 16
            element_hgl(0) = -1
            element_hgl(1) = 21
            element_hgl(2) = 16


            Dim h As String
            Dim t As String
            Dim i As Integer
            Dim j As Integer
            Dim k As Integer

            Dim line_h As String
            Dim line_ As String

            Dim n_opened_elements As Integer
            Dim opened_tag As String
            Dim last_opened_element As String
            Dim opened_tags() As cls_preXML_tag
            Dim n_opened_tags As Integer = -1
            If p.elements_found_opened IsNot Nothing Then
                n_opened_tags = p.elements_found_opened.Length - 1
                opened_tags = p.elements_found_opened
            End If
            Dim li As Integer
            'last_hgl = marks.get_marks
            'to_highlight = marks.get_marks




            If to_highlight Is Nothing Then
                'ještě nic k zvýraznění vybráno není
                to_highlight = last_hgl
            End If
            If to_highlight_add IsNot Nothing Then
                If to_highlight IsNot Nothing Then
                    For i = 0 To UBound(to_highlight_add)
                        If to_highlight.Contains(to_highlight_add(i)) = False Then
                            'pokud to tam ještě není, přidáme
                            ReDim Preserve to_highlight(UBound(to_highlight) + 1)
                            to_highlight(UBound(to_highlight)) = to_highlight_add(i)
                        End If
                    Next
                Else
                    to_highlight = to_highlight_add
                End If
            End If
            last_hgl = to_highlight


            Dim dch As cls_highligh_rule.character_rtf_formating
            dch.fs = env.rtb_font_size
            Dim pch As cls_highligh_rule.character_rtf_formating
            Dim tch As cls_highligh_rule.character_rtf_formating

            'tch = dch

            'h = rtf_header() & vbNewLine & clrtbl.create_clrtbl
            If draw_formatted = True Then
                h &= page.page_rtf & "\sectd "
                Dim margl As String
                Dim margr As String
                Dim paperw As String
                If page.page_rtf = Nothing Then p.page_rtf = ""
                margl = rgx_g(p.page_rtf, "\\margl([0-9]+)")
                margr = rgx_g(p.page_rtf, "\\margr([0-9]+)")
                paperw = rgx_g(p.page_rtf, "\\paperw([0-9]+)")
                If paperw <> "" Then
                    Dim w As Long
                    Dim ml As Long
                    Dim mr As Long
                    If margl <> "" Then ml = CInt(margl) / 15
                    If margr <> "" Then mr = CInt(margr) / 15
                    w = CInt(paperw) / 15
                    'rtb.ZoomFactor = 1
                    'rtb.Width = rtb.ZoomFactor * w - (mr * rtb.ZoomFactor + ml * rtb.ZoomFactor)
                End If
            End If
            'clrtbl.set_clr_tbl(o)
            'o = o & vbNewLine & "\par \pard " & vbNewLine

            Dim buffer(10) As String
            Dim p_plain_text As String = page.plain_text
            Dim def_to_highlight(4)

            def_to_highlight(0) = "~search"
            def_to_highlight(1) = "~replace"
            def_to_highlight(2) = "~removed_after"
            def_to_highlight(3) = "~BAD_CLOSING_TAG"
            def_to_highlight(4) = "~TAG_NOT_CLOSED"
            Dim tag As Boolean
            Dim tag_name As String

            Dim inside_of_closing_tag As String = ""


            For i = 0 To Len(page.plain_text) - 1
                pch = tch
                tch = dch

                li = 200 + (n_opened_elements * 400)
                'If i = 15 Then Stop
                If p_plain_text(i) = Chr(10) Then

                    buffer(0) &= line_h & " " & line_ 'nejprve "vyexpedujeme" do bufferů předchozí řádku
                    For j = 0 To 6
                        If Len(buffer(j)) > 10 ^ (j + 2) Then
                            buffer(j + 1) = buffer(j + 1) & buffer(j)
                            buffer(j) = ""
                        Else
                            Exit For
                        End If
                    Next
                    line_ = ""
                    line_h = ""
                    If Me.draw_formatted = False Then
                        line_h = "\line\li" & li
                    Else 'po zlomu řádku totiž může mít znak informace o formátování celého řádku (odsazení a tak)
                        line_h = "\par\pard"
                    End If



                ElseIf p_plain_text(i) = "}" Then
                    tch._ch = "\}"
                ElseIf p_plain_text(i) = "{" Then
                    tch._ch = "\{"
                ElseIf p_plain_text(i) = "\" Then
                    tch._ch = "\\"
                Else
                    tch._ch = p_plain_text(i)

                    If Asc(tch._ch) <> AscW(tch._ch) Then
                        tch._ch = "\u" & AscW(tch._ch)
                        tch.utf = True
                    Else
                        If AscW(tch._ch) = 129 Then 'nezlomitelná či jakási divná mezera
                            tch.highlight = 3
                        End If
                        tch.utf = False
                    End If
                End If

                If Me.draw_formatted = True Then
                    If page.meta_data(i) IsNot Nothing Then
                        For j = 0 To UBound(page.meta_data(i))
                            If Left(page.meta_data(i)(j), 3) = "\li" Or Left(page.meta_data(i)(j), 3) = "\sb" Or Left(page.meta_data(i)(j), 3) = "\sl" Then
                                line_h &= page.meta_data(i)(j)
                            End If
                        Next

                    End If
                End If


                If highlight_XML_tags = True Then
                    If tch._ch = "<" Then

                        tag = True
                        k = InStr_first(i + 1, page.plain_text, k, 0, ">", " ", vbLf)
                        If k > i Then 'zjistíme si, jaký tag máme právě otevřený
                            opened_tag = Mid(page.plain_text, i + 2, k - (i + 2))
                        End If
                        Dim whole_tag As String
                        k = InStr(i + 1, page.plain_text, ">")
                        If k <> 0 Then whole_tag = Mid(page.plain_text, i + 1, k - (i))
                        tag_fc_index_to_use = 0 'a nastavíme případné odlišné zvýraznění...
                        Dim opened_tag_name = Replace(opened_tag, "/", "")
                        If opened_tag_name = "p" Or opened_tag_name = "div" Or opened_tag_name = "head" Or opened_tag_name = "lg" Or opened_tag_name = "l" _
                            Or opened_tag_name = "front" Or opened_tag_name = "list" Or opened_tag_name = "item" Or opened_tag_name = "title" Then
                            tag_fc_index_to_use = 1
                            element_hgl_index_to_use = 0
                        ElseIf opened_tag_name = "app" Or opened_tag_name = "note" Or opened_tag_name = "quote" Or opened_tag_name = "lem" _
                            Or opened_tag_name = "cit" Or opened_tag_name = "bibl" Or opened_tag_name = "rdg" Or opened_tag_name = "rubric" Then
                            tag_fc_index_to_use = 2

                        End If
                        'xyz = p.elements_found_opened

                        If i + 1 < Len(page.plain_text) Then
                            'xyz = Len(page.plain_text)
                            'If i + 1 > Len(p.plain_text) - 1 Then Stop
                            If Left(whole_tag, 2) = "</" Then 'zavírací tag
                                n_opened_elements -= 1
                                If n_opened_tags > -1 Then
                                    inside_of_closing_tag = opened_tags(n_opened_tags).name
                                    If n_opened_elements < 0 Then n_opened_elements = 0
                                    n_opened_tags -= 1
                                    If n_opened_tags > -1 Then
                                        ReDim Preserve opened_tags(n_opened_tags)
                                    Else
                                        Erase opened_tags
                                    End If
                                End If
                            ElseIf Right(whole_tag, 2) <> "/>" Then
                                inside_of_closing_tag = ""
                                n_opened_tags += 1
                                If n_opened_tags < -1 Then n_opened_tags = -1
                                If n_opened_tags > -1 Then
                                    ReDim Preserve opened_tags(n_opened_tags)
                                    opened_tags(n_opened_tags) = New cls_preXML_tag(opened_tag, Nothing, New Point(0, 0), False)
                                End If
                                n_opened_elements += 1

                            End If
                        End If
                    Else
                        If inside_of_closing_tag <> "" Then
                            If pch._ch = ">" Then
                                inside_of_closing_tag = ""
                            End If
                        End If
                    End If

                End If
                'xyz = Mid(page.plain_text, i + 1, 100)
                inside_of_head = False
                If n_opened_tags > -1 Then

                    Dim loe As String = opened_tags(n_opened_tags).name
                    If inside_of_closing_tag <> "" Then loe = inside_of_closing_tag
                    If loe = "app" Or loe = "note" Or loe = "bibl" Or loe = "rdg" Or loe = "rubric" Or loe = "cit" Then
                        element_hgl_index_to_use = 1
                        tch.highlight = element_hgl(element_hgl_index_to_use)

                    Else
                        tch.highlight = 0
                        element_hgl_index_to_use = 0
                    End If


                    For j = 0 To n_opened_tags 'tučně zvýrazníme text v head, ale už ne text v rdg apod. je-li to v head
                        If opened_tags(j).name = "head" Or opened_tags(j).name = "title" Then
                            inside_of_head = True
                        End If
                        If (inside_of_head = True) Then
                            If opened_tags(j).name = "rdg" Or opened_tags(j).name = "note" Or opened_tags(j).name = "bibl" Then
                                inside_of_head = False
                            End If
                        End If
                    Next
                End If
                If to_highlight IsNot Nothing Then


                    For j = 0 To to_highlight.Count - 1

                        If page.meta_data(i) IsNot Nothing Then
                            If p.meta_data(i).Contains(to_highlight(j)) = True Then 'pokud má znak příznak od pravidla, které chceme zvýraznit
                                Me.marks.get_hgl(to_highlight(j)).get_chrtf(tch, clrtbl)
                                'tch.highlight = 18
                            End If
                        End If
                    Next
                End If
                If page.meta_data IsNot Nothing Then


                    'zvýraznění standartních značek
                    If page.meta_data(i) IsNot Nothing Then
                        If page.meta_data(i).Contains("~search") = True Then
                            tch.highlight = 14
                        End If
                        If page.meta_data(i).Contains("~search_capture") = True Then
                            tch.highlight = 16
                        End If
                        If page.meta_data(i).Contains("~replaced") = True Then
                            tch.highlight = 14
                        End If
                        If page.meta_data(i).Contains("~removed_after") = True Then
                            tch.uld = True
                        End If


                    End If
                    'zvýraznění tagů...
                    If tag = True Then
                        Dim tag_not_closed As Boolean
                        If page.meta_data(i) IsNot Nothing Then
                            If page.meta_data(i).Contains("~TAG_NOT_CLOSED") = True Then 'zvýrazníme neuzavřené tagy
                                tag_not_closed = True
                            End If
                        End If
                        If tag_not_closed = True Then
                            tch.uld = True
                            tch.cf = 14

                            'tch.highlight = 0
                        Else
                            If page.meta_data(i) IsNot Nothing Then
                                If page.meta_data(i).Contains("~BAD_CLOSING_TAG") = True Then
                                    tch.highlight = 13
                                    tch.cf = 0
                                Else
                                    tch.cf = tag_fc(tag_fc_index_to_use)
                                    If tch.highlight = tch.cf Then tch.cf -= 2
                                End If
                            Else
                                tch.cf = tag_fc(tag_fc_index_to_use)
                                If tch.highlight = tch.cf Then tch.cf -= 2

                            End If

                        End If
                        tch.bold = True
                        tch.f = 1
                        tch.fs = env.rtb_font_size + 2
                        If tch._ch = ">" Then
                            If i <> 0 Then
                                If page.plain_text(i - 1) = "/" Then 'samozavírací tag...
                                    'n_opened_elements -= 1 'počítáme, kolik otevřených elementů zrovna máme
                                    If n_opened_elements < 0 Then n_opened_elements = 0
                                End If

                            End If
                            tag = False
                            tag_not_closed = False

                        End If
                    Else
                        If dimmed = True Then tch.cf = 8
                        If inside_of_head Then
                            tch.bold = True
                        End If
                        If draw_formatted = True Then
                            tch.f = 2
                            If page.meta_data(i) IsNot Nothing Then
                                If page.meta_data(i).Contains("i") Then
                                    tch.italics = True
                                End If
                                If page.meta_data(i).Contains("b") Then
                                    tch.bold = True
                                End If
                                'If p.meta_data(i).Contains("sm") Then
                                'tch.fs = env.rtb_font_size * 0.6
                                'End If
                                Dim coef As Double
                                coef = env.rtb_font_size / 11
                                For j = 0 To UBound(page.meta_data(i))
                                    If Left(page.meta_data(i)(j), 3) = "\fs" Then
                                        tch.fs = CInt(Mid(page.meta_data(i)(j), 4)) * coef
                                        'tch.fs = env.rtb_font_size + (tch.fs - 12)

                                    End If
                                Next
                            End If

                        End If
                    End If

                End If
                'If tch._ch = "a" Or tch._ch = "b" Or tch._ch = "c" Then
                '    tch.bold = True
                'ElseIf tch._ch = "e" Then
                '    tch.ulth = True
                '    tch.highlight = clrtbl.def_clr("yellow3") + 1
                'ElseIf tch._ch = "l" Or tch._ch = "r" Then
                '    tch.cf = clrtbl.def_clr("red") + 1
                'End If

                If tch.cf <> pch.cf Then
                        tch._form = tch._form & "\cf" & tch.cf
                    End If
                    If tch.f <> pch.f Then
                        tch._form = tch._form & "\f" & tch.f
                    End If
                    If tch.fs <> pch.fs And tch.fs <> 0 Then
                        tch._form = tch._form & "\fs" & Math.Round(tch.fs * env.environment_size, 0)
                    End If
                    If tch.highlight <> pch.highlight Then
                        tch._form = tch._form & "\highlight" & tch.highlight
                    End If
                    If tch.bold <> pch.bold Then
                        If tch.bold = True Then
                            tch._form = tch._form & "\b"
                        Else
                            tch._form = tch._form & "\b0"
                        End If
                    End If
                    If tch.italics <> pch.italics Then
                        If tch.italics = True Then
                            tch._form = tch._form & "\i"
                        Else
                            tch._form = tch._form & "\i0"
                        End If
                    End If
                    If tch.ul <> pch.ul Then
                        If tch.ul = True Then
                            tch._form = tch._form & "\ul"
                        Else
                            tch._form = tch._form & "\ul0"
                        End If
                    End If
                    If tch.ulth <> pch.ulth Then
                        If tch.ulth = True Then
                            tch._form = tch._form & "\ulth"
                        Else
                            tch._form = tch._form & "\ul0"
                        End If
                    End If
                    If tch.uld <> pch.uld Then
                        If tch.uld = True Then
                            tch._form = tch._form & "\uld"
                        Else
                            tch._form = tch._form & "\ul0"
                        End If
                    End If



                    Dim output As String
                    If tch.utf = True Then
                        output = "{\ud{" & tch._ch & "}}"
                    Else
                        output = tch._ch
                    End If
                    If tch._form = "" Then
                        'output = tch._ch
                    Else
                        output = tch._form & " " & output
                    End If
                    line_ &= output



                Next

            buffer(0) &= line_h & " " & line_
            For j = 0 To 6
                    buffer(j + 1) = buffer(j + 1) & buffer(j)
                Next j

                t = "\sb90\sa90\li200" 'trochu odsazení od levého okraje (\li=left indent), odshora (\sb=sbace before) a dole (\sa=space after)

                t = t & buffer(7)

                t = t & "\par"


                Return t
                f_time = Environment.TickCount - f_time
            End If
            Return ""
    End Function
    Private Sub rtf(to_highlight() As String, Optional to_highlight_add() As String = Nothing, Optional highlight_XML_tags As Boolean = True)
        If Me.p Is Nothing Then Exit Sub
        clrtbl.create_default_clrtbl()

        Dim h As String = rtf_header() & vbNewLine & clrtbl.create_clrtbl
        Dim t As String
        t = generate_rtf(Me.p, to_highlight, to_highlight_add, highlight_XML_tags)
        Dim prev_t As String
        Dim after_t As String

        Dim first_visible_index As Integer
        Dim last_visible_index As Integer

        Dim selstart As Integer = rtb.SelectionStart
        Dim sellength As Integer = rtb.SelectionLength
        first_visible_index = rtb.GetCharIndexFromPosition(New Point(1, 1))
        last_visible_index = rtb.GetCharIndexFromPosition(New Point(1, rtb.Height - 1))

        Me.continous_page_view = False
        If Me.continous_page_view = True Then
            If view.prev_page_rtf = "" Then
                view.prev_page_rtf = generate_rtf(Me.p2(0), to_highlight, to_highlight_add, highlight_XML_tags, True)
            End If
            If view.next_page_rtf = "" Then
                view.next_page_rtf = generate_rtf(Me.p2(1), to_highlight, to_highlight_add, highlight_XML_tags, True)
            End If
            view.prev_page_displayed = True
            view.next_page_displayed = True
            If p2(0) IsNot Nothing Then
                view.prev_page_lenght = Len(p2(0).plain_text)
            Else
                view.prev_page_displayed = False
            End If


            t = h & view.prev_page_rtf & vbNewLine & "{\par\pard\sb100\sa100}" & vbNewLine & t

            If p2(1) IsNot Nothing Then
                view.next_page_start0b = Len(t)
                t &= vbNewLine &
                "{\par\pard\sb100\sa100}" & vbNewLine & view.next_page_rtf & "}"
            Else
                view.next_page_displayed = False
                view.next_page_start0b = Len(-1)
                t &= "}"
            End If

        Else
            t = h & t & "}"
        End If

        Me.rtb_locked = True
        'Clipboard.SetText(h & vbNewLine & t)

        Dim activated_rtb As RichTextBox, deactivated_rtb As RichTextBox
        If rtb_(0).Visible = True Then
            activated_rtb = rtb_(1)
            deactivated_rtb = rtb_(0)
        Else
            activated_rtb = rtb_(0)
            deactivated_rtb = rtb_(1)
        End If


        activated_rtb.Visible = False
        activated_rtb.WordWrap = deactivated_rtb.WordWrap
        Dim zoom As Single

        zoom = activated_rtb.ZoomFactor

        activated_rtb.Rtf = t
        activated_rtb.SelectionStart = last_visible_index + view.acFI
        'frm_main.Text &= " " & activated_rtb.SelectionStart
        activated_rtb.SelectionStart = first_visible_index + view.acFI
        'frm_main.Text &= " " & activated_rtb.SelectionStart
        If p.force_SelStart <> -1 Then
            selstart = p.force_SelStart
            p.plain_text_selection_changed(p.force_SelStart - view.acFI, p.force_SelLength)
            p.force_SelStart = -1
        End If
        If p.force_SelLength > -1 Then
            sellength = p.force_SelLength
            p.plain_text_selection_changed(p.force_SelStart - view.acFI, p.force_SelLength)
            p.force_SelLength = -1
        End If
        activated_rtb.SelectionStart = selstart + view.acFI
        activated_rtb.SelectionLength = sellength
        'deactivated_rtb.SelectionStart = selstart + view.acFI
        'deactivated_rtb.SelectionLength = sellength
        'frm_main.Text &= " " & activated_rtb.SelectionStart & "x"

        'deactivated_rtb.ZoomFactor = zoom

        activated_rtb.Visible = True
        activated_rtb.BringToFront()
        'deactivated_rtb.SendToBack()
        'deactivated_rtb.SelectionStart = activated_rtb.SelectionStart 'last_visible_index + view.acFI
        'frm_main.Text &= " " & deactivated_rtb.SelectionStart
        ''frm_main.Text &= " " & deactivated_rtb.SelectionStart
        'frm_main.Text &= " " & deactivated_rtb.SelectionStart
        'deactivated_rtb.Visible = False

        activated_rtb.ZoomFactor = 1
        activated_rtb.ZoomFactor = zoom


        Me.rtb_locked = False
        rtb = activated_rtb
        rtb.Select()
        __rtf_renders += 1
        redraw_tags_hgl = True
        highlight_tags()
        actualize_info_controls()
        'highlight_tags()
        If p.context.flt IsNot Nothing Then
            If env.wsp.flti IsNot Nothing Then If env.wsp.flti.Visible = True Then env.wsp.flti.BringToFront()
        End If

        'frm_main.Text = "rtf: " & f_time

        'tmr_ln.Start()
    End Sub
    Private Sub tmr_ln_tick(sender As Object, e As System.EventArgs)
        'se zpožděním vykreslí text - třeba při psaní, aby se v průběhu zadávání textu pořád nepřekresloval
        If p IsNot Nothing Then
            generate_linenumbers()
            underline_lines()
            highlight_tags(True)
        End If
        tmr_ln.Stop()
    End Sub
    Private Sub rtb_validated(sender As Object, e As EventArgs)
        Me.redraw_tags_hgl = True
        highlight_tags(True)
    End Sub
    Private Sub rtb_vscroll(sender As Object, e As EventArgs)
        If sender.visible = True Then
            tmr_ln.Stop()
            Me.redraw_tags_hgl = True
            tmr_ln.Start()
        End If
    End Sub
    Private Sub rtb_hscroll(sender As Object, e As EventArgs)
        If sender.visible = True Then
            tmr_ln.Stop()
            Me.redraw_tags_hgl = True
            tmr_ln.Start()
        End If
    End Sub
    Public Function delete_ln()
        Do While lnpnl.Controls.Count <> 0
            lnpnl.Controls.RemoveAt(0)
        Loop
    End Function


    Public Sub underline_lines()
        If p Is Nothing Then Exit Sub
        Dim g As Graphics = rtb.CreateGraphics

        Dim b As SolidBrush = New SolidBrush(Color.Black)
        Dim b2 As SolidBrush = New SolidBrush(Color.LightGray)
        Dim pen As Pen = New Pen(b2)
        Dim fvi As Integer
        fvi = rtb.GetCharIndexFromPosition(New Point(1, 1))
        Dim fl As Integer
        fl = Me.p.line_from_char_index(fvi)
        Dim y As Integer
        Dim ch_pos As Point
        'y = rtb.GetPositionFromCharIndex(0).Y

        Dim j As Integer = 0
        Dim i As Integer
        'lnpnl.Visible = False


        For i = fl To rtb.Lines.Count - 1
            ch_pos = rtb.GetPositionFromCharIndex(p.line_start_index(i))
            If ch_pos.Y > rtb.Height Then
                Exit For
            End If
            g.DrawLine(pen, ch_pos, New Point(rtb.Width, ch_pos.Y))
        Next i
        __lines_renders += 1
        actualize_info_controls()
    End Sub
    Private last_highlighted() As cls_preXML_tag
    Private n_lh As Long = -1
    Public Function highlight_tags(Optional immediatly As Boolean = False)
        'Static last_highlighted() As cls_preXML_tag
        'Static n_lh As Long = -1
        If env._p Is Nothing Then Exit Function
        Dim newly_highlighted() As cls_preXML_tag
        Dim n_nh As Long = -1
        If immediatly = True Then
            Dim t As String
            Dim i As Integer
            Dim j As Integer
            Dim k As Integer
            Dim g As Graphics = rtb.CreateGraphics

            For k = 0 To env._p.context.n_tags_opened
                t = env._p.context.tags_opened(k).name
                i = env._p.context.tags_opened(k).position.X
                j = env._p.context.tags_opened(k).position.Y

                If i = env._p.m_index And j <> 0 Then
                    n_nh += 1
                    ReDim Preserve newly_highlighted(n_nh)
                    newly_highlighted(n_nh) = env._p.context.tags_opened(k)

                End If
            Next k
            Dim render As Boolean = False
            If n_nh = n_lh Then
                For k = 0 To n_lh
                    If last_highlighted(k).name <> newly_highlighted(k).name Or
                            last_highlighted(k).position.X <> newly_highlighted(k).position.X Or
                            last_highlighted(k).position.Y <> newly_highlighted(k).position.Y Then
                        render = True
                    End If
                Next
            Else
                render = True
            End If
            If render = True Or Me.redraw_tags_hgl = True Then
                rtb.Refresh()
                underline_lines()
                generate_linenumbers()
                Me.redraw_tags_hgl = False
                Dim last_tag_height As Integer
                Dim last_opened_graph_position As Point
                Dim last_opened_cltag_graph_position As Point
                Dim last_opened_cltag_end As Point

                Dim cltag As cls_preXML_tag
                For k = 0 To n_nh
                    Dim last_opened_tag_start As Point
                    Dim last_opened_tag_end As Point
                    Dim tag_bottom As Integer
                    Dim te As Integer
                    t = newly_highlighted(k).name
                    i = newly_highlighted(k).position.X
                    j = newly_highlighted(k).position.Y
                    te = InStr(j, env._p.plain_text, ">")
                    last_opened_tag_start = rtb.GetPositionFromCharIndex(j - 1)
                    last_opened_tag_end = rtb.GetPositionFromCharIndex(te - 1)
                    If k <> n_nh Then
                        g.DrawRectangle(New Pen(Color.Orange, 2), New Rectangle(last_opened_tag_start.X, last_opened_tag_start.Y,
                                                                                (last_opened_tag_end.X - last_opened_tag_start.X) + 8,
                                                                                (last_opened_tag_end.Y - last_opened_tag_start.Y) + 19 * env.environment_size))
                    Else 'zvýrazníme otvírací tag
                        g.DrawRectangle(New Pen(Color.OrangeRed, 3), New Rectangle(last_opened_tag_start.X, last_opened_tag_start.Y,
                                                                                   (last_opened_tag_end.X - last_opened_tag_start.X) + 8,
                                                                                   (last_opened_tag_end.Y - last_opened_tag_start.Y) + 20 * env.environment_size))
                        last_tag_height = (last_opened_tag_end.Y - last_opened_tag_start.Y) + 20 * env.environment_size
                        cltag = p.context.tags_opened(p.context.n_tags_opened).second_to_pair
                        If cltag IsNot Nothing Then
                            If cltag.position.X = p.m_index Then 'a ještě zavírací tag, je-li na stejné stránce
                                last_opened_cltag_graph_position = rtb.GetPositionFromCharIndex(cltag.position.Y - 1)
                                te = InStr(cltag.position.Y, env._p.plain_text, ">")
                                last_opened_cltag_end = rtb.GetPositionFromCharIndex(te - 1)
                                g.DrawRectangle(New Pen(Color.OrangeRed, 3), New Rectangle(last_opened_cltag_graph_position.X,
                                                                                       last_opened_cltag_graph_position.Y,
                                                                                       (last_opened_cltag_end.X - last_opened_cltag_graph_position.X) + 8,
                                                                                       (last_opened_cltag_end.Y - last_opened_cltag_graph_position.Y) + 20 * env.environment_size))

                                If last_opened_cltag_graph_position.Y = last_opened_tag_end.Y Then 'otvírací a zavírací tag jsou na stejné řádce,
                                    'jednoduše nakreslíme čáru mezi nimi
                                    g.DrawLine(New Pen(Color.OrangeRed, 1), last_opened_tag_end, last_opened_cltag_graph_position)
                                    g.DrawLine(New Pen(Color.OrangeRed, 1), New Point(last_opened_tag_end.X, last_opened_tag_end.Y + 20 * env.environment_size),
                                               New Point(last_opened_cltag_graph_position.X, last_opened_cltag_graph_position.Y + 20 * env.environment_size))
                                Else
                                    g.DrawLine(New Pen(Color.OrangeRed, 1), last_opened_tag_end, New Point(rtb.Width - 19, last_opened_tag_end.Y))
                                    g.DrawLine(New Pen(Color.OrangeRed, 1), New Point(1, last_opened_tag_end.Y + 20 * env.environment_size),
                                               New Point(last_opened_tag_end.X, last_opened_tag_end.Y + 20 * env.environment_size))

                                    g.DrawLine(New Pen(Color.OrangeRed, 1), last_opened_cltag_end, New Point(rtb.Width - 19, last_opened_cltag_end.Y))
                                    g.DrawLine(New Pen(Color.OrangeRed, 1), New Point(1, last_opened_cltag_end.Y + 20 * env.environment_size),
                                               New Point(last_opened_cltag_end.X, last_opened_cltag_end.Y + 20 * env.environment_size))
                                    g.DrawLine(New Pen(Color.OrangeRed, 3), New Point(rtb.Width - 19, last_opened_tag_end.Y),
                                               New Point(rtb.Width - 19, last_opened_cltag_end.Y))

                                End If
                            End If
                        End If
                    End If
                Next k
                If (p.context.n_tags_opened > 0) Then
                    cltag = p.context.tags_opened(p.context.n_tags_opened).second_to_pair
                    last_opened_cltag_graph_position.X = 1
                    last_opened_graph_position.X = 1

                    If cltag Is Nothing Then
                        last_opened_cltag_graph_position.Y = rtb.GetPositionFromCharIndex(Len(p.plain_text)).Y + 20 * env.environment_size
                    Else
                        If cltag.position.X = p.m_index Then 'zavírací tag je ještě na téhle stránce, nakreslíme pak čáru i k němu
                            last_opened_cltag_graph_position = rtb.GetPositionFromCharIndex(cltag.position.Y)
                            last_opened_cltag_graph_position.Y += (last_tag_height Or 20)
                        Else
                            last_opened_cltag_graph_position.Y = rtb.GetPositionFromCharIndex(Len(p.plain_text)).Y + 20 * env.environment_size 'element končí až na další stránce, nakreslíme čáru k poslední řádce
                            last_opened_cltag_graph_position.X = 1
                        End If
                        ''Dim te = InStr(cltag.position.Y, env._p.plain_text, ">")
                        'last_opened_cltag_end = rtb.GetPositionFromCharIndex(te - 1)
                        

                    End If

                    Dim last_opened_position = p.context.tags_opened(p.context.n_tags_opened).position
                    If last_opened_position.X < p.m_index Then
                        last_opened_graph_position.Y = 1 'element je otevřen  na předcházející stránce
                    Else
                        last_opened_graph_position.Y = rtb.GetPositionFromCharIndex(last_opened_position.Y).Y
                    End If
                    g.DrawLine(New Pen(Color.OrangeRed, 3), New Point(last_opened_graph_position.X, last_opened_graph_position.Y + 20 * env.environment_size),
                               New Point(1, last_opened_cltag_graph_position.Y))
                    'g.DrawLine(New Pen(Color.OrangeRed, 1), New Point(1, last_opened_cltag_graph_position.Y), last_opened_cltag_graph_position)
                End If
                __tag_renders += 1
                actualize_info_controls()

                n_lh = n_nh
                Erase last_highlighted
                ReDim last_highlighted(n_lh)
                For i = 0 To n_lh
                    last_highlighted(i) = New cls_preXML_tag(newly_highlighted(i).name, newly_highlighted(i).parent,
                                                           New Point(newly_highlighted(i).position.X, newly_highlighted(i).position.Y), newly_highlighted(i).self_closing)
                Next
            End If
        Else
            tmr_highlight.Start()
        End If
    End Function
    Private Function highlight_elements()

    End Function
    Private Sub lnpnl_paint(sender As Object, e As PaintEventArgs)
        If sender.tag <> "cleaning" Then
            generate_linenumbers(True)
        End If
    End Sub
    Public Function generate_linenumbers(Optional force As Boolean = False)

        Static n_lln As Integer = -1
        Dim n_ln As Integer = -1
        Static last_ln() As Point
        Dim now_ln() As Point

        If p Is Nothing Then Exit Function

        Dim fvi As Integer
        fvi = rtb.GetCharIndexFromPosition(New Point(1, 1)) - view.acFI
        Dim fl As Integer
        If fvi >= 0 Then
            fl = Me.p.line_from_char_index(fvi)
        Else
            fl = 0

        End If
        Dim prevy As Integer
        Dim y As Integer
        Dim ch_pos As Point
        'y = rtb.GetPositionFromCharIndex(0).Y

        'lnrt = rtf_header()
        Dim fs As Integer = env.rtb_font_size / 2
        If fs < 8 Then fs = 8

        Dim j As Integer = 0
        Dim i As Integer
        'lnpnl.Visible = False
        Dim h As Long
        For i = fl To rtb.Lines.Count - 1
            If (p.line_start_index(i) = p.line_end_index(i)) Then
                If p.line_start_index(i) > 0 Then
                    ch_pos = rtb.GetPositionFromCharIndex(p.line_start_index(i) + view.acFI)
                Else
                    ch_pos = rtb.GetPositionFromCharIndex(p.line_start_index(i) + view.acFI + 1)
                End If
            Else
                    ch_pos = rtb.GetPositionFromCharIndex(p.line_start_index(i) + view.acFI + 1)
            End If
            n_ln += 1
            ReDim Preserve now_ln(n_ln)
            now_ln(n_ln).X = i + 1
            now_ln(n_ln).Y = ch_pos.Y
            If n_ln > 0 Then
                h = now_ln(n_ln).Y - now_ln(n_ln - 1).Y
            End If
            If ch_pos.Y > rtb.Height Then
                Exit For
            End If

        Next i

        Dim render As Boolean

        If n_ln = n_lln Then
            For i = 0 To n_lln
                If last_ln(i).X <> now_ln(i).X Or last_ln(i).Y <> now_ln(i).Y Then
                    render = True
                    Exit For
                End If
            Next
        Else
            render = True
        End If
        If render = True Or force = True Then
            Dim g As Graphics = lnpnl.CreateGraphics
            Dim b As SolidBrush = New SolidBrush(Color.Black)
            Dim b2 As SolidBrush = New SolidBrush(Color.LightGray)
            Dim pen As Pen = New Pen(b2)
            Dim pen2 As Pen = New Pen(Color.Black)
            lnpnl.Tag = "cleaning"
            lnpnl.Refresh()
            lnpnl.Tag = ""
            Dim f As Font = New Font("Consolas", fs, FontStyle.Bold)
            For i = 0 To n_ln
                g.DrawString(now_ln(i).X, f, b, New PointF(1, now_ln(i).Y))
                g.DrawLine(pen2, New Point(1, now_ln(i).Y - 1), New Point(lnpnl.Width, now_ln(i).Y - 1))
                'g2.DrawLine(pen, ch_pos, New Point(rtb.Width, ch_pos.Y))
                prevy = y
            Next i
            __ln_renders += 1
            actualize_info_controls()

            Erase last_ln
            n_lln = n_ln
            last_ln = now_ln
        End If
    End Function

    Private Function rtf_header()
        Dim o As String
        o = "{\rtf1\ansi\deff0{\fonttbl{\f0\fnil\fcharset238 Consolas;\f1\fscript\fcharset238 Consolas;\f2\fscript\fcharset238 Times New Roman;}}"

        Return o
    End Function

    Friend Function NewCtrl(ByRef obj_reference As Object, ctrl As Control) As Control
        'pro snazší vkládání ovl. prvků jednoho za druhým (aby se nemuselo při nastavování pozice nově vloženého prvku vždy odkazovat jmenovitě na předchozí,
        'od jehož pozice se pozice nového odvíjí, ale dalo se odkazovat vždy jednoduše na lastctrl)
        obj_reference = ctrl
        lastctrl = thisctrl
        thisctrl = ctrl
        thisctrl.Font = env.def_font
        Return ctrl
    End Function

    Private Sub tmr_go_on_other_page_tick(sender As Object, e As EventArgs)
        on_end_of_page = 0
        on_begin_of_page = 0
        tmr_go_on_other_page.Stop()
        tmr_go_on_other_page.Tag = False
    End Sub
    '######### události editace ####
    Private nshift_pressed As Integer
    Public Sub frm_keyDown(sender As Object, e As KeyEventArgs)
        If p IsNot Nothing Then
            If e.KeyValue = Keys.S And e.Control = True And e.Alt = False And e.Shift = False Then 'CTRL+S - vše uložíme
                If Me.doc IsNot Nothing Then
                    Me.doc.save(True)
                End If
            ElseIf e.KeyValue = Keys.F And e.Control = True And e.Shift = False And e.Alt = False Then 'ctrl+f - aktivuje okénko najít a nahradit
                show_search_box(True)
            ElseIf e.KeyCode = Keys.KeyCode.Z And e.Control = True And e.Shift = False And e.Alt = False Then
                Me.p.one_step_back()
            ElseIf e.KeyCode = Keys.F10 And e.Control = False And e.Alt = False And e.Shift = False Then
                synchronize_pdf_and_text()
            ElseIf e.KeyCode = Keys.F8 Then
                expand_pdf_box()
            ElseIf e.KeyCode = Keys.F9 Then
                find_selection_in_xml()

            End If
            'RaiseEvent rtb_key_down(p, e)
            If e.KeyValue = Keys.Escape Then
                p.context.set_flying_tool(Nothing)
                env.wsp.cnm_floating.hide()
            ElseIf p.context.flt IsNot Nothing Then
                If p.context.flt.deactivateOnAnyAction = True Then p.context.set_flying_tool(Nothing)
            End If

            If e.KeyCode = Keys.Down And e.Alt = True Then 'alt + šipka dolu otevře kontextové menu
                If env.wsp.cnm_floating.visible = True Then 'aktivace je v rtb_keydown, ale tohle musí být tady-protože když je menu aktivní, je fokus v něm, a ne v rtb
                    env.wsp.cnm_floating.pnl.BringToFront()
                    env.wsp.cnm_floating.next_tool_group()
                End If
            End If

            'If e.KeyValue = Keys.Space And e.Alt = True Then Stop
            RaiseEvent event_triggered(EN.evn_FRM_KEY_DOWN, p, create_myEventArgs(e, Control.ModifierKeys, frm.ActiveControl))
            'If rtb.SelectedText <> "" Then

            'End If
        End If
        If e.Alt = True Then
            If e.KeyValue = Keys.Enter Or e.KeyValue = Keys.Space Or e.KeyValue = Keys.Escape Then 'potlačíme všechny systémové kl. zkratky s alt krom alt+tab
                e.SuppressKeyPress = True
            End If
        End If
        If e.Control = True And e.Alt = False And e.Shift = False Then
            If (e.KeyValue = Keys.L Or e.KeyValue = Keys.R Or e.KeyValue = Keys.I) Then
                e.SuppressKeyPress = True
            End If
        End If

    End Sub

    Public Sub rtb_keyDown(sender As Object, e As KeyEventArgs)
        'Exit Sub
        If p IsNot Nothing Then
            If e.KeyValue = Keys.F2 Then
                Dim unicode As String
                unicode = Trim(InputBox("Zadejte:" & vbNewLine _
                    & "1) &&H a hexadecimální, anebo decimální unicode hodnotu znaku (např. pro ""A"" &&H41 nebo 65)" & vbNewLine _
                    & "2) < či > pro html entitu příslušného znaku)." & vbNewLine _
                    & "3) Písmeno pro vložení příslušného znaku alfabety. NB: η=h, θ=u, ξ=j, ς (koncová)=w, χ=x, ψ=c, ω=v"))
                If unicode <> "" Then
                    Dim ke_vlozeni As String = ""
                    If rgxt(unicode, "^&H[0-9A-Fa-f]+$") = True Or rgxt(unicode, "^[0-9]+$") = True Then
                        'xyz = Convert.ToInt32(unicode)
                        'xyz = CInt("&H0305")
                        ke_vlozeni = ChrW(CInt(unicode))
                    ElseIf unicode = "<" Then
                        ke_vlozeni = "&lt;"
                    ElseIf unicode = "<<" Then
                        ke_vlozeni = ""
                    ElseIf unicode = ">" Then
                        ke_vlozeni = "&gt;"
                    ElseIf rgxt(unicode, "^[a-zA-Z]$") = True Then
                        Dim abc As String = "abgdezhuiklmnjoprswtyfxcvABGDEZHUIKLMNJOPRSWTYFXCV"
                        Dim alfabeta As String = "αβγδεζηθικλμνξοπρσςτυφχψωΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΣΤΥΦΧΨΩ"
                        Dim pos As Integer
                        pos = InStr(abc, unicode)
                        If pos <> 0 Then
                            ke_vlozeni = Mid(alfabeta, pos, 1)
                        End If
                        'xyz = Convert.ToInt32(unicode)
                        'xyz = CInt("&H0305")
                    Else

                        If unicode = "theta" Then
                            ke_vlozeni = "θ"
                        ElseIf unicode = "Theta" Or unicode = "THETA" Then
                            ke_vlozeni = "Θ"
                        End If
                        If unicode = "omega" Or unicode Then
                            ke_vlozeni = "ω"
                        ElseIf unicode = "Omega" Or unicode = "OMEGA" Then
                            ke_vlozeni = "Ω"
                        End If

                    End If
                    If ke_vlozeni <> "" Then
                        If p.SelLength > 0 Then
                            p.delete_text_on_position(p.SelStart1b, p.SelStart1b + p.SelLength - 1)
                        End If
                        p.insert_on_position(p.SelStart1b, ke_vlozeni, "")
                        p.force_SelStart = p.SelStart1b
                        p.force_SelLength = 0
                        display_page(Nothing, Nothing, 10)
                    End If
                End If
            End If

            If Me.cnm_floating IsNot Nothing Then
                Me.cnm_floating.hide()
            End If
            If e.KeyCode = Keys.Down And e.Alt = True Then 'alt + šipka dolu otevře kontextové menu
                If env.wsp.cnm_floating.visible = False Then
                    env.wsp.cnm_floating.generate_context_menu(False)
                Else
                    env.wsp.cnm_floating.pnl.BringToFront()
                    env.wsp.cnm_floating.next_tool_group()
                End If
            End If

            If e.Control = False And e.Alt = False And e.Shift = False Then
                If Me.doc IsNot Nothing And Me.p IsNot Nothing Then
                    If e.KeyCode = Keys.F11 Then
                        If p.m_index > 0 Then
                            open_page(p.m_index - 1)
                            Me.frm.get_pdf_control.gotoPreviousPage
                        End If
                    ElseIf e.KeyCode = Keys.F12 Then
                        If p.m_index < Me.doc.n_pages Then
                            open_page(p.m_index + 1)
                            Me.frm.get_pdf_control.gotoNextPage
                        End If
                    ElseIf e.KeyCode = Keys.Down And p.SelStart0b >= p.last_line_start Then
                        on_end_of_page += 1
                        If on_end_of_page = 1 Then go_on_other_page_delay = Environment.TickCount

                        If tmr_go_on_other_page.Tag <> True Then
                            tmr_go_on_other_page.Start()
                            tmr_go_on_other_page.Tag = True
                        End If

                        If p.m_index < Me.doc.n_pages Then
                            If on_end_of_page >= 3 And Environment.TickCount - go_on_other_page_delay > 250 Then
                                open_page(p.m_index + 1)
                                Me.frm.get_pdf_control.gotoNextPage
                            End If
                        End If
                    ElseIf e.KeyCode = Keys.Up And p.SelStart0b <= p.line_end_index(0) Then
                        on_begin_of_page += 1
                        If on_begin_of_page = 1 Then go_on_other_page_delay = Environment.TickCount
                        If tmr_go_on_other_page.Tag <> True Then
                            tmr_go_on_other_page.Start()
                            tmr_go_on_other_page.Tag = True
                        End If
                        If on_begin_of_page >= 3 And Environment.TickCount - go_on_other_page_delay > 250 Then
                            If p.m_index > 0 Then
                                open_page(p.m_index - 1)
                                Me.frm.get_pdf_control.gotoPreviousPage
                            End If
                        End If
                    End If
                End If

            End If
        End If
        If e.Alt = True Then
            If e.KeyValue = Keys.Escape Or e.KeyValue = Keys.F4 Or e.KeyValue = Keys.Space Then 'potlačíme všechny systémové kl. zkratky s alt krom alt+tab
                e.SuppressKeyPress = True
            End If
        End If
        If (e.KeyValue = Keys.L Or e.KeyValue = Keys.R Or e.KeyValue = Keys.I) And e.Control = True Then
            e.SuppressKeyPress = True
        End If
        If e.Alt = True And e.Control = True Then
            'e.SuppressKeyPress = True

        End If
    End Sub
    Public Sub frm_keyUp(sender As Object, e As KeyEventArgs)
        If p IsNot Nothing Then
            'RaiseEvent rtb_key_up(p, e)
            RaiseEvent event_triggered(EN.evn_FRM_KEY_UP, p, create_myEventArgs(e, Control.ModifierKeys, frm.ActiveControl))
        End If
        If (e.KeyValue = Keys.Space And e.Alt = True) Or (e.KeyValue = Keys.Escape And e.Control = True) Then 'alt+mezerník otevře nabídku okna,
            'alt+esc přepne na nějaké jiné okno. Ani jedno nechceme
            e.SuppressKeyPress = True
        End If
    End Sub
    Public Sub frm_keyPress(sender As Object, e As KeyPressEventArgs)
        If p IsNot Nothing Then
            'RaiseEvent rtb_key_press(p, e)
            RaiseEvent event_triggered(EN.evn_FRM_KEY_PRESS, p, create_myEventArgs(e, Control.ModifierKeys, frm.ActiveControl))
        End If
    End Sub
    Public Sub rtb_mouseDown(sender As Object, e As MouseEventArgs)
        If p IsNot Nothing Then
            'RaiseEvent rtb_mouse_down(p, e)
            RaiseEvent event_triggered(EN.evn_RTB_MOUSE_DOWN, p, create_myEventArgs(e, Control.ModifierKeys, Me.rtb))

            If p.context.flt IsNot Nothing Then If p.context.flt.deactivateOnAnyAction = True Then p.context.set_flying_tool(Nothing)
            env.wsp.cnm_floating.hide()

            If e.Button = MouseButtons.Right Then
                cnm_floating.generate_context_menu(True)
            End If

            If p.context.inside_of_tag <> "" Then
                If Left(p.context.inside_of_tag, 1) = "/" Then 'jsme uvnitř zavíracího tagu
                    Dim le_p As Integer, le_i As Integer, le_attr() As cls_preXML_attribute
                    Dim le As String = p.context.last_opened_element(le_p, le_i, le_attr)

                    If p.context.flt Is Nothing Then
                        Dim txt As String
                        If le_attr IsNot Nothing Then
                            le &= " "
                            For i = 0 To UBound(le_attr)
                                le &= le_attr(i).ToString
                            Next
                        End If
                        le = "<" & le & ">"
                        If le_p <> p.m_index And le_i > 0 Then
                            txt = env.c("Zavírací tag elementu") & " " & le & " " & env.c("ze strany") & " " & le_p & ":" & vbNewLine & vbNewLine & Mid(doc.page(le_p).plain_text, le_i, 100)
                        ElseIf le_i > 0 Then
                            txt = env.c("Zavírací tag elementu") & " " & le & ":" & vbNewLine & vbNewLine & Mid(doc.page(le_p).plain_text, le_i, 100)
                        End If
                        p.context.set_flying_tool(New cls_flyingtool(Me, txt, "---closing_tag_info---", le, True))
                    End If
                End If
            End If
        End If
    End Sub
    Public Sub rtb_mouseUp(sender As Object, e As MouseEventArgs)
        If p IsNot Nothing Then
            'RaiseEvent rtb_mouse_up(p, e)
            RaiseEvent event_triggered(EN.evn_RTB_MOUSE_UP, p, create_myEventArgs(e, Control.ModifierKeys, Me.rtb))
        End If
    End Sub
    Public Sub rtb_mouseMove(sender As Object, e As MouseEventArgs)
        If p IsNot Nothing Then
            'RaiseEvent rtb_mouse_move(p, e)
            RaiseEvent event_triggered(EN.evn_RTB_MOUSE_MOVE, p, create_myEventArgs(e, Control.ModifierKeys, Me.rtb))


            If p.context.flt IsNot Nothing Then
                If p.context.flt.value IsNot Nothing Then
                    If CStr(p.context.flt.value) = "---closing_tag_info---" Then
                        Dim i As Integer
                        i = rtb.GetCharIndexFromPosition(e.Location)
                        Dim t As String = tag_on_position(p.plain_text, i)
                        If Left(t, 2) <> "</" Then p.context.set_flying_tool(Nothing) 'vypneme políčko ukazující uzavíraný elmenty
                    End If
                End If
            End If


        End If
        If Me.flti IsNot Nothing Then
            If Me.flti.Visible = True Then
                Dim l As Integer
                Dim t As Integer
                l = e.X + 20 + sender.left
                If l + Me.flti.Width > Me.flti.Parent.Width Then l = Me.flti.Parent.Width - Me.flti.Width
                Me.flti.Left = l
                Me.flti.Top = e.Y + 5 + sender.top
            End If
        End If
    End Sub

    Public Sub rtb_mouseClick(sender As Object, e As MouseEventArgs)
        If p IsNot Nothing Then
            'RaiseEvent rtb_click(p, e)
            'Me.frm.text = p.get_position_context(p.SelStart0b)
            RaiseEvent event_triggered(EN.evn_RTB_MOUSE_CLICK, p, create_myEventArgs(e, Control.ModifierKeys, Me.rtb))
            env.wsp.cnm_floating.hide()
        End If
    End Sub
    Public Sub rtb_mouseDblClick(sender As Object, e As MouseEventArgs)
        If p IsNot Nothing Then
            'RaiseEvent rtb_dblclick(p, e)
            RaiseEvent event_triggered(EN.evn_RTB_MOUSE_DBL_CLICK, p, create_myEventArgs(e, Control.ModifierKeys, Me.rtb))
        End If
    End Sub
    Public Sub rtb_textChanged(sender As Object, e As EventArgs)
        If p IsNot Nothing Then
            If rtb_locked = False Then
                'RaiseEvent rtb_text_changed(p)
                RaiseEvent event_triggered(EN.evn_RTB_CHANGED, p, create_myEventArgs(e, Control.ModifierKeys, Me.rtb))
                p.text_changed(Mid(sender.text, view.acFI + 1), 0)
                display_page(Nothing, Nothing,,, 500)
                highlight_tags()
                generate_linenumbers()
                actualize_info_controls()
            End If
        End If
    End Sub
    Public Sub rtb_selectionChanged(sender As Object, e As EventArgs)
        If p IsNot Nothing And rtb_locked = False Then
            If view.continous_view_activated = True Then
                Dim SSS As Integer = sender.selectionstart
                If sender.selectionstart < view.acFI - 1 Then 'přepnuli jsme se na předchozí stránku
                    Me.open_page(p.m_index - 1, SSS)
                    Exit Sub
                ElseIf sender.selectionstart >= view.nextFI Or sender.selectionstart - sender.selectionlength >= view.nextFI Then 'na následující
                    Me.open_page(p.m_index + 1, 0)
                    Exit Sub
                End If
            End If
            If sender.selectionstart - view.acFI >= 0 Then
                p.plain_text_selection_changed(sender.SelectionStart - view.acFI, sender.selectionLength)
                RaiseEvent event_triggered(EN.evn_RTB_SELECTION_CHANGED, p, Nothing)
                tmr_cnm.Stop()
                tmr_cnm.Start()
                highlight_tags()
                actualize_info_controls()
                If Me.cnm_floating IsNot Nothing Then Me.cnm_floating.hide()
                on_begin_of_page = 0
                on_end_of_page = 0
            End If

        End If
    End Sub
    Public Sub synchronize_pdf_and_text()
        On Error GoTo Err
        Dim i As Long
        If Me.doc IsNot Nothing Then
            If Me.p IsNot Nothing Then
                Dim pb_tag As String
                Dim pdf_nr As String
                Dim pdf_ppp As String
                Dim j As Integer, k As Integer, n As Integer
                Dim tmp_p() As Point
                For i = Me.p.m_index To 0 Step -1
                    j = 0
                    tmp_p = Me.doc.page(i).search(True, "<pb [^>]*>", j)
                    If tmp_p IsNot Nothing Then
                        For k = UBound(tmp_p) To 0 Step -1
                            pb_tag = Mid(Me.doc.page(i).plain_text, tmp_p(k).X + 1, tmp_p(k).Y - tmp_p(k).X + 1)
                            pdf_nr = rgx_g(pb_tag, " pdf\s*=\s*[""']([0-9]+)[""']")
                            pdf_ppp = rgx_g(pb_tag, " pdf_ppp\s*=\s*[""']([0-9]+)[""']")
                            Dim ppp As Long
                            If (pdf_ppp = "") Then
                                ppp = 1
                            Else
                                ppp = CLng(pdf_ppp)
                            End If
                            If pdf_nr <> "" Then
                                n = Math.Ceiling((Me.p.m_index - i) / ppp) + CInt(pdf_nr)
                                If n > 0 Then
                                    Me.frm.get_pdf_control.setCurrentPage(n)
                                    Me.frm.activate_pdf_container

                                    Exit Sub
                                End If
                            End If
                        Next k
                    End If
                Next
            End If
        End If
Err:
    End Sub
    Private Sub tmrtick(sender As Object, e As EventArgs)

    End Sub
    Public Sub find_selection_in_xml()

    End Sub
    Public Sub expand_pdf_box()
        If pnl_pdw_expanded_view Is Nothing Then
            pnl_pdw_expanded_view = New Panel
            pnl_pdw_expanded_view.Parent = Me.frm
            pnl_pdw_expanded_view.Top = 0
            pnl_pdw_expanded_view.Left = 0
            pnl_pdw_expanded_view.Height = Me.frm.clientsize.height
            pnl_pdw_expanded_view.Width = Me.frm.clientsize.width / 2

        End If
        If pnl_pdw_expanded_view.Visible = False Then
            pnl_pdw_expanded_view = New Panel
            pnl_pdw_expanded_view.Parent = Me.frm
            pnl_pdw_expanded_view.Top = 0
            pnl_pdw_expanded_view.Left = 0
            pnl_pdw_expanded_view.Height = Me.frm.clientsize.height
            pnl_pdw_expanded_view.Width = Me.frm.clientsize.width / 2
            frm.get_pdf_control.parent = pnl_pdw_expanded_view
            pnl_pdw_expanded_view.Visible = True
            pnl_pdw_expanded_view.BringToFront()
            pnl_pdw_expanded_view.BringToFront()
            pnl_pdw_expanded_view.BringToFront()
        Else
            pnl_pdw_expanded_view.Visible = False
            Me.frm.get_pdf_control.parent = Me.frm.get_pdf_container

        End If
    End Sub
End Class