Imports System.ComponentModel
Imports převaděč_pdf
Imports System.Text.RegularExpressions
Imports System.Xml

Public Class cls_tools_organizer

    'kolekce organizující nástroje spouštěné po dokončení nadřazeného nástroje...
    Public WithEvents t As Object
    Public triggered_tools() As cls_tools_organizer
    Public n_tt As Integer

    Public gl_shortcut As cls_keyevent_args 'případná globální zkratka (spouštěná odkudkoliv)
    Public lc_shortcut As cls_keyevent_args 'lokální zkratka (pokud je v toolmanageru otevřená kolekce, která obsahuje tento nástroj
    'tyto dvě zkratky pouze aktivují kartu nástroje, ale nespustí ho

    Public parent As Object
    Public id As Integer 'id konkrétní instance nástroje - unikátní napříč celým programem, slouží k vyhledání nástroje třeba po kliknutí do trveeview
    Private Shared last_tool_id As Integer
    Public m_index As Integer
    Public Sub New(tool As Object, parent_ As Object, m_index_ As Integer)

        m_index = m_index_
        last_tool_id += 1
        t = tool
        Me.id = last_tool_id
        Me.parent = parent_
        Dim evt = t.GetType().GetEvent("executed")
        'evt.AddEventHandler(t, New EventHandler(AddressOf execute))
        'Dim u As Object
        'přiřadit ovladač k události obj. t rovnou nejde - událost je neznámá...
        'AddHandler t.GetType().GetEvent("executed"), AddressOf execute
        n_tt = -1
    End Sub
    Public Sub New(n As Xml.XmlNode)
        last_tool_id += 1
        Me.id = last_tool_id
        n_tt = -1
        __xml(Nothing, n, False)
    End Sub
    Public Sub execute(p As cls_preXML_section_page) ' Handles t.executed
        Dim i As Integer
        For i = 0 To n_tt 'pro jistotu...
            triggered_tools(i).m_index = i
        Next
        For i = 0 To n_tt
            triggered_tools(i).t.run(p, -1)
            If triggered_tools(i).t IsNot Nothing Then
            End If
        Next
    End Sub
    Public Sub execute(p As cls_preXML_section_page, s As Integer)
        Dim i As Integer
        For i = s To n_tt
            triggered_tools(i).t.run(p, -1)
            If triggered_tools(i).t IsNot Nothing Then
            End If
        Next
    End Sub
    Public Function add_subtool(tool As Object, Optional position As Integer = -1)

        'position:-1=na konec; -2=na začátek, jinak na zadaný index
        n_tt += 1

        ReDim Preserve triggered_tools(n_tt)
        If position = -1 And tool IsNot Nothing Then
            If tool.GetType = GetType(cls_tools_organizer) Then
                triggered_tools(n_tt) = New cls_tools_organizer(tool.t, Me, n_tt)
            Else
                triggered_tools(n_tt) = New cls_tools_organizer(tool, Me, n_tt)
            End If
            triggered_tools(n_tt).t.parent = triggered_tools(n_tt)
            Return triggered_tools(n_tt)
        End If
    End Function
    Public Function list_me() As TreeNode
        Dim i As Integer
        Dim tmp As TreeNode = New TreeNode(t.name)
        tmp.Tag = Me
        For i = 0 To n_tt
            tmp.Nodes.Add(triggered_tools(i).list_me)
        Next
        Return tmp
    End Function
    Public Function list_my_event_listeners(n As TreeNode)
        Dim i As Integer
        Dim n2 As TreeNode
        Me.t.list_my_event_listeners(n)
        For i = 0 To n_tt
            n2 = n.Nodes.Add(Me.triggered_tools(i).t.name)
            n2.Tag = env.c("nástroj", "tool")
            n2.ForeColor = Color.DeepPink
            Me.triggered_tools(i).list_my_event_listeners(n2)
        Next
    End Function
    Public Function get_by_id(tid As Integer) As cls_tools_organizer
        'každý nástroj bude mít unikátní id - podle toho je pak budme hledat (po kliknutí do trývjů)
        If Me.id = tid Then
            Return Me
        Else
            Dim i As Integer
            Dim tmp As cls_tools_organizer
            For i = 0 To Me.n_tt
                tmp = Me.triggered_tools(i).get_by_id(tid)
                If tmp IsNot Nothing Then Return tmp
            Next
        End If
    End Function
    Public Sub raise(p As cls_preXML_section_page, e As Object, mode As Integer)
        t.raise(p, e, mode)
    End Sub
    Public Sub activate()

    End Sub
    Public Function clone() As Object
        Return Me.t.clone
    End Function
    Public Function all_tools(ByRef arr() As Object) As Object()
        Dim i As Long
        If arr Is Nothing Then
            ReDim arr(0)
        Else
            ReDim Preserve arr(UBound(arr) + 1)
        End If
        arr(UBound(arr)) = Me.t
        For i = 0 To Me.n_tt
            Me.triggered_tools(i).all_tools(arr)
        Next
    End Function

    Public Function export_to_xml(x As Xml.XmlDocument) As Xml.XmlNode
        Return __xml(x, Nothing, True)
    End Function
    Private Function __xml(x As Xml.XmlDocument, n_imp As Xml.XmlNode, export As Boolean) As Xml.XmlNode
        Dim n As Xml.XmlNode
        If export = True Then
            n = x.CreateNode(Xml.XmlNodeType.Element, "tool_organizer", "")
        End If

        If export = True Then
            If Me.gl_shortcut IsNot Nothing Then
                n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "gl_shortcut", "")).AppendChild(Me.gl_shortcut.export_to_xml(x))
            End If
        Else
            n = n_imp.SelectSingleNode("gl_shortcut")
            If n IsNot Nothing Then
                Me.gl_shortcut = New cls_keyevent_args(n)
            End If
        End If

        If export = True Then
            If Me.lc_shortcut IsNot Nothing Then
                n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "lc_shortcut", "")).AppendChild(Me.lc_shortcut.export_to_xml(x))
            End If
        Else
            n = n_imp.SelectSingleNode("lc_shortcut")
            If n IsNot Nothing Then
                Me.lc_shortcut = New cls_keyevent_args(n)
            End If
        End If

        If export = True Then
            With n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "main_tool", ""))
                .AppendChild(Me.t.export_to_xml(x))
            End With
        Else
            n = n_imp.SelectSingleNode("main_tool/tool")
            If n IsNot Nothing Then
                Dim ttype_str As String
                Dim ttype As Type
                ttype_str = n.SelectSingleNode("class_name").InnerText
                If ttype_str <> "" Then
                    ttype = System.Reflection.Assembly.GetExecutingAssembly.GetType("převaděč_pdf." & ttype_str)
                    If ttype IsNot Nothing Then
                        Me.t = System.Activator.CreateInstance(ttype, n)
                        Me.t.parent = Me
                        'If Me.t.name = "Označ krit. ap." Then Stop
                    End If
                End If
            End If
        End If
        If export = True Then
            With n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "triggered_tools", ""))
                Dim i As Long
                For i = 0 To Me.n_tt
                    .AppendChild(triggered_tools(i).export_to_xml(x))
                Next
            End With
        Else
            Dim nl As Xml.XmlNodeList
            nl = n_imp.SelectNodes("triggered_tools/tool_organizer")
            If nl IsNot Nothing Then
                Me.n_tt = nl.Count - 1
                ReDim Me.triggered_tools(n_tt)
                For i = 0 To n_tt
                    Me.triggered_tools(i) = New cls_tools_organizer(nl.Item(i))
                    Me.triggered_tools(i).parent = Me
                Next
            End If
        End If

        If export = True Then Return n
    End Function
End Class
Public MustInherit Class cls_tool
    'Inherits cls_event_reciever
    Public event_listeners() As cls_event_listener '
    Public n_event_listeners As Integer = -1
    Public type As String
    Public name As String
    Public name_id As String
    Shared id As Integer
    Public mark As String
    Public description As String
    Public hgl(1) As cls_highligh_rule

    Friend lastctrl As Control
    Private thisctrl As Control
    Public parent As Object
    Public disabled As Boolean
    Public Sub New()
        id += 1
    End Sub
    Public Sub set_basics(name_ As String, name_id_ As String, mark_ As String, description_ As String, mark1_hgl As cls_highligh_rule)
        id += 1
        name = name_
        name_id = name_id_
        mark = mark_
        description = description_
        hgl(0) = mark1_hgl
        env.wsp.marks.add_mark(mark, hgl(0))
    End Sub
    Public Overridable Sub list_my_event_listeners(n As TreeNode)
        Dim i As Integer
        Dim n2 As TreeNode
        For i = 0 To n_event_listeners
            Me.event_listeners(i).list_me((n.Nodes.Add(Me.event_listeners(i).description)))
        Next
    End Sub
    'Public Event executed()
    Friend Function add_event_listener(mode As Integer, description As String)
        n_event_listeners += 1
        ReDim Preserve event_listeners(n_event_listeners)
        event_listeners(n_event_listeners) = New cls_event_listener(Me, mode, description)
    End Function

    Friend Function NewCtrl(ByRef obj_reference As Object, ctrl As Control, Optional container As Control = Nothing) As Control
        'pro snazší vkládání ovl. prvků jednoho za druhým (aby se nemuselo při nastavování pozice nově vloženého prvku vždy odkazovat jmenovitě na předchozí,
        'od jehož pozice se pozice nového odvíjí, ale dalo se odkazovat vždy jednoduše na lastctrl)
        obj_reference = ctrl
        If container IsNot Nothing Then
            obj_reference.parent = container
        End If
        lastctrl = thisctrl
        thisctrl = ctrl
        thisctrl.Font = env.def_font
        Return ctrl
    End Function
    Friend Function T() As Integer
        If lastctrl IsNot Nothing Then Return lastctrl.Top Else Return 0
    End Function
    Friend Function TpH() As Integer
        If lastctrl IsNot Nothing Then Return lastctrl.Top + lastctrl.Height Else Return 0
    End Function
    Friend Function L() As Integer
        If lastctrl IsNot Nothing Then Return lastctrl.Left Else Return 0
    End Function
    Friend Function LpW() As Integer
        If lastctrl IsNot Nothing Then Return lastctrl.Left + lastctrl.Width Else Return 0
    End Function
    Friend Function NewCtrl(ctrl As Control) As Control
        'pro snazší vkládání ovl. prvků jednoho za druhým (aby se nemuselo při nastavování pozice nově vloženého prvku vždy odkazovat jmenovitě na předchozí,
        'od jehož pozice se pozice nového odvíjí, ale dalo se odkazovat vždy jednoduše na lastctrl)
        lastctrl = thisctrl
        thisctrl = ctrl
        thisctrl.Font = env.def_font
        Return ctrl
    End Function
    Public Sub clone_base(ByRef tmp As Object)
        tmp.type = Me.type
        tmp.name = Me.name
        tmp.mark = Me.mark
        tmp.description = Me.description
        tmp.name_id = Me.name_id
    End Sub
    Public Sub set_base(name_ As String, mark_ As String, description_ As String, name_id_ As String)
        Me.name = name
        Me.name_id = name_id_
        Me.description = description_
        Me.mark = mark_
    End Sub
    Public Enum tm_mode As Integer
        TM_ONLY_HIGHLIGHT = 0
        TM_EXECUTE = 1
        TM_REPLACE_MARKED = 2
    End Enum
    Public MustOverride Sub dispose_controls()
    Public MustOverride Sub create_controls(container As Control, last_visualized_tool As Object)
    Public MustOverride Sub run(pp As cls_preXML_section_page, mode As Integer)
    Public MustOverride Function clone()
    Public MustOverride Function generate_context_menu(p As cls_preXML_section_page, cmn As cls_context_menu)
    Public MustOverride Function context_menu_activated(p As cls_preXML_section_page, p1 As Object, p2 As Object)
    Public MustOverride Function export_to_xml(x As Xml.XmlDocument) As Xml.XmlNode
    Public Overridable Function raise_function(fname As String, p As cls_preXML_section_page, supress_triggering As Boolean, ParamArray params() As Object)
        '
    End Function
    Public Overridable Function has_function(fname As String) As Boolean
        '
    End Function
    Friend Function export_base_to_xml(n As Xml.XmlNode, x As Xml.XmlDocument)
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "name", "")).InnerText = Me.name
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "name_id", "")).InnerText = Me.name_id
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "description", "")).InnerText = Me.description
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "mark", "")).InnerText = Me.mark
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "class_name", "")).InnerText = Me.GetType.Name
        If Me.hgl(0) IsNot Nothing Then n.AppendChild(Me.hgl(0).export_to_xml(x))
        If Me.hgl(1) IsNot Nothing Then n.AppendChild(Me.hgl(1).export_to_xml(x))
    End Function
    Public Sub import_base_from_xml(n As Xml.XmlNode)
        name = get_singlenode_value(n, "name")
        name_id = get_singlenode_value(n, "name_id")
        description = get_singlenode_value(n, "description")
        mark = get_singlenode_value(n, "mark")
        Dim tmp As Xml.XmlNodeList
        tmp = n.SelectNodes("highlight_rule")
        If tmp IsNot Nothing Then
            For i = 0 To tmp.Count - 1
                hgl(i) = New cls_highligh_rule(tmp.Item(i))
                env.wsp.marks.add_mark(mark, hgl(i))
            Next
        End If
    End Sub

    Friend Sub clean_container(container As Object, last_visualized_tool As Object)
        'funkce, která vyčistí místo pro tvoření ovládacích prvků
        lastctrl = Nothing
        container.visible = False
        If last_visualized_tool IsNot Nothing Then
            last_visualized_tool.dispose_controls
        End If
        While container.Controls.Count > 0
            container.Controls.Item(container.Controls.Count - 1).Dispose()
        End While
        container.Refresh()
    End Sub
    Public Sub activate_me()

    End Sub
End Class
'############################################################################################################################################################
'############################################################################################################################################################
Public Class cls_tool_Group_Page_Extraction
    Inherits cls_tool
    Private file_path As String
    Private import_as_rtf As Boolean
    Private p1 As Integer
    Private p2 As Integer
    Private txt_path As TextBox
    Private txt_p1 As TextBox
    Private txt_p2 As TextBox
    Private rtb_preview As RichTextBox
    Private lbl_prview_info As Label
    Private chb_remove_empty_lines As CheckBox
    Private chb_remove_first_line As CheckBox
    Private chb_rtf As CheckBox

    Private loaded_string As String
    Private pages() As String
    Private n_pages As Integer = -1
    Public Sub New(name_ As String, name_id_ As String, mark_ As String, description_ As String, mark1_hgl As cls_highligh_rule)
        MyBase.set_basics(name_, name_id_, mark_, description_, mark1_hgl)
    End Sub
    Public Overrides Sub dispose_controls()
        txt_path = Nothing
        txt_p1 = Nothing
        txt_p2 = Nothing
        rtb_preview = Nothing
        lbl_prview_info = Nothing
        chb_remove_empty_lines = Nothing
        chb_remove_first_line = Nothing
    End Sub

    Public Overrides Sub create_controls(container As Control, last_visualized_tool As Object)
        Me.clean_container(container, last_visualized_tool)
        Dim lbl As Label
        Dim txt As TextBox
        Dim chb As CheckBox
        Dim cmd As Button
        With NewCtrl(lbl, New Label, container)
            .Top = 5
            .Left = 5
            .Text = env.c("Adresa k souboru:", "Path to the file:")
            lbl.AutoSize = True
        End With
        With NewCtrl(txt_path, New TextBox, container)
            .Text = file_path
            .Width = 200
            .Left = LpW()
            .Top = T()
        End With
        With NewCtrl(cmd, New Button, container)
            .Top = T()
            .Left = LpW()
            .Height = lastctrl.Height
            .Width = .Height
            AddHandler .Click, AddressOf cmd_select_file_click
        End With
        With NewCtrl(chb_rtf, New CheckBox, container)
            .Text = "RTF"
            chb_rtf.Checked = import_as_rtf
            .Top = TpH() + 5
            .Left = 5
            chb_rtf.AutoSize = True
        End With
        With NewCtrl(lbl, New Label, container)
            .Top = TpH() + 20
            .Left = 5
            .Text = env.c("Rozsah stran:", "Page range:")
            lbl.AutoSize = True
        End With
        With NewCtrl(txt_p1, New TextBox, container)
            .Top = T()
            .Left = LpW() + 5
            .Text = p1
            .Width = 40
            .Name = "txt_p1"
            AddHandler .TextChanged, AddressOf txt_p_text_changed
        End With
        With NewCtrl(lbl, New Label, container)
            .Top = T()
            .Left = LpW() + 5
            .Text = " - "
            lbl.AutoSize = True
        End With
        With NewCtrl(txt_p2, New TextBox, container)
            .Top = T()
            .Left = LpW() + 5
            .Text = p2
            .Width = 40
            .Name = "txt_p2"
            AddHandler .TextChanged, AddressOf txt_p_text_changed
        End With
        With NewCtrl(lbl_prview_info, New Label, container)
            .Top = TpH() + 20
            .Left = 5
            .Text = env.c("Strana...", "Page...")
            .Width = 200
        End With

        With NewCtrl(cmd, New Button, container)
            .Text = env.c("Zpracuj!", "Go!")
            .Top = T()
            .Left = LpW() + 20
            cmd.AutoSize = True
            AddHandler .Click, AddressOf cmd_process_click
        End With
        With NewCtrl(chb_remove_empty_lines, New CheckBox, container)
            .Text = env.c("Odstraň prázdné řádky", "Remove blank lines")
            .Top = TpH() + 5
            .Left = 5
            chb_remove_empty_lines.AutoSize = True
        End With
        With NewCtrl(chb_remove_first_line, New CheckBox, container)
            .Text = env.c("Odstraň první řádku", "Remove first line")
            .Top = T()
            .Left = LpW() + 10
            chb_remove_first_line.AutoSize = True
        End With
        With NewCtrl(rtb_preview, New RichTextBox, container)
            .Left = 5
            .Width = container.Width - 10
            If .Width < 250 Then .Width = 250
            .Top = TpH() + 10
            .Height = container.Height - .Top - 10
            If .Height < 500 Then .Height = 500
            rtb_preview.BorderStyle = BorderStyle.None
            rtb_preview.ScrollBars = RichTextBoxScrollBars.Both
        End With
        container.Visible = True
    End Sub

    Private Sub txt_p_text_changed(sender As Object, e As EventArgs)
        If rgxt(sender.text, "^[0-9]+$") = True Then
            If sender.name = "txt_p1" Then
                p1 = CInt(sender.text)
                page_preview(p1)
            Else
                p2 = CInt(sender.text)
                page_preview(p2)
            End If
        End If
    End Sub
    Private Sub page_preview(p As Integer)
        If n_pages > -1 Then
            If p > 0 And p <= (n_pages + 1) Then
                If chb_rtf.Checked = True = True Then
                    rtb_preview.Rtf = "{\rtf " & pages(p - 1) & "}"
                    env.wsp.rtb_locked = True
                    env.wsp.rtb.Rtf = "{\rtf " & pages(p - 1) & "}"
                    env.wsp.rtb_locked = False
                Else
                    rtb_preview.Text = pages(p - 1)
                End If
                lbl_prview_info.Text = env.c("Náhled strany ", "Page preview ") & p & " z " & n_pages + 1 & ":"
            End If
        Else
                rtb_preview.Text = ""
        End If
    End Sub
    Private Sub cmd_select_file_click(sender As Object, e As EventArgs)
        If env.opened_document IsNot Nothing Then
            Dim d As OpenFileDialog
            d = New OpenFileDialog
            d.FileName = env.opened_document.path
            d.ShowDialog()
            If d.FileName <> "" Then
                file_path = d.FileName
                txt_path.Text = file_path
                If My.Computer.FileSystem.FileExists(file_path) Then
                    loaded_string = My.Computer.FileSystem.ReadAllText(file_path)
                    Dim i As Integer
                    Dim j As Integer = 0
                    n_pages = -1
                    Erase pages
                    If chb_rtf.Checked = False Then
                        Dim separator As String
                        If InStr(1, loaded_string, Chr(12)) = 0 Then
                            separator = "pb_tag"
                        Else
                            separator = Chr(12)
                        End If
                        If separator = Chr(12) Then
                            Do While InStrX(i + 1, loaded_string, separator, i) <> 0
                                n_pages += 1
                                ReDim Preserve pages(n_pages)
                                pages(n_pages) = Mid(loaded_string, j + Len(separator), i - j)
                                j = i
                            Loop
                        Else
                            Dim tmp As String
                            i = 0
                            Do While InStrX(i + 1, loaded_string, "<pb", i) <> 0
                                If j > 0 And i - j > 0 Then
                                    'tmp = Mid(loaded_string, j, i - j)
                                    n_pages += 1
                                    ReDim Preserve pages(n_pages)
                                    pages(n_pages) = Mid(loaded_string, j, i - j)
                                End If
                                j = i
                            Loop
                        End If
                    Else
                            Dim k As Long
                        Dim zyx As Object


                        Do While InStrX(i + 1, loaded_string, "\sbkpage", i) <> 0
                            k = i
                            i = InStrRev(loaded_string, "\par", i) - 1
                            n_pages += 1
                            ReDim Preserve pages(n_pages)
                            pages(n_pages) = Mid(loaded_string, j + 1, i - j)
                            j = i
                            i = k + 1
                        Loop
                    End If
                End If
            End If
        End If
    End Sub
    Private Sub cmd_process_click(sender As Object, e As EventArgs)
        If n_pages <> -1 And env.opened_document IsNot Nothing Then
            If p1 > 0 And p1 <= n_pages + 1 Then
                Dim tmp As cls_preXML_section_page
                Dim txt As String
                Dim tmp_page_txt As String
                Dim page_rtf As String
                Dim md()() As String
                env.wsp.rtf_to_prepreXML("{\rtf " & pages(0) & "}", md, page_rtf)
                For i = p1 - 1 To p2 - 1
                    With env.opened_document
                        Dim p_nr As String
                        Dim rm As Match
                        tmp_page_txt = pages(i)
                        p_nr = rgx_g(tmp_page_txt, "<pb\s*n=[""']([0-9]+)[""']/>",, rm)
                        If p_nr <> "" Then
                            tmp_page_txt = Replace(tmp_page_txt, rm.Value, "")
                        End If

                        tmp = .new_page(,, p_nr)
                        If chb_rtf.Checked = False Then
                            txt = Replace(tmp_page_txt, "<", "&lt;")
                            txt = Replace(txt, ">", "&gt;")
                            txt = Replace(txt, vbCr, "")
                            txt = Replace(txt, Chr(12), "")
                            If chb_remove_empty_lines.Checked = True Then
                                txt = Trim(txt)
                                Do While Left(txt, 1) = vbLf
                                    txt = Mid(txt, 2)
                                Loop
                                Do While Right(txt, 1) = vbLf
                                    txt = Left(txt, Len(txt) - 1)
                                Loop

                            End If
                            If chb_remove_first_line.Checked = True Then
                                Dim fle As Integer
                                fle = InStr(1, txt, vbLf)
                                If fle <> 0 Then txt = Mid(txt, fle + 1)
                            End If


                            tmp.text_changed(txt, 2)
                        Else


                            txt = env.wsp.rtf_to_prepreXML("{\rtf " & pages(i) & "}", md, "")
                            tmp.text_changed(txt, md, 1)
                            tmp.page_rtf = page_rtf
                        End If
                        env.wsp.raise_event(EN.evn_TEXT_INSERTED, tmp, Nothing)
                    End With
                Next i
                env.wsp.actualize_info_controls()
                env.wsp.display_page(Nothing, Nothing,,, 0)
                env.wsp.open_page(env.opened_document.n_pages)
            End If
        End If
    End Sub
    Public Overrides Sub run(pp As cls_preXML_section_page, mode As Integer)
        'nic
    End Sub

    Public Overrides Function clone() As Object
    End Function

    Public Overrides Function generate_context_menu(p As cls_preXML_section_page, cmn As cls_context_menu) As Object
    End Function

    Public Overrides Function context_menu_activated(p As cls_preXML_section_page, p1 As Object, p2 As Object) As Object
    End Function
    Public Sub New(n As Xml.XmlNode)
        __xml(Nothing, n, False)
    End Sub
    Private Function __xml(x As Xml.XmlDocument, n_imp As Xml.XmlNode, export As Boolean) As Xml.XmlNode
        Dim n As Xml.XmlNode
        Dim i As Long
        If export = True Then
            n = x.CreateNode(Xml.XmlNodeType.Element, "tool", "")
            MyBase.export_base_to_xml(n, x)
            n.AppendChild(x.CreateNode(XmlNodeType.Element, "file_path", "")).InnerText = Me.file_path
            n.AppendChild(x.CreateNode(XmlNodeType.Element, "p1", "")).InnerText = Me.p1
            n.AppendChild(x.CreateNode(XmlNodeType.Element, "p2", "")).InnerText = Me.p2
            n.AppendChild(x.CreateNode(XmlNodeType.Element, "import_as_rtf", "")).InnerText = Me.import_as_rtf
        Else
            Me.file_path = get_singlenode_value(n_imp, "file_path")
            Me.p1 = get_singlenode_value(n_imp, "p1")
            Me.p2 = get_singlenode_value(n_imp, "p2")
            Me.import_as_rtf = get_singlenode_value(n_imp, "import_as_rtf")
            MyBase.import_base_from_xml(n_imp)
        End If

        If export = True Then
            Return n
        End If
    End Function
    Public Overrides Function export_to_xml(x As XmlDocument) As XmlNode
        Return __xml(x, Nothing, True)
    End Function

End Class
Public Class cls_tool_Text_ReArrange
    Inherits cls_tool
    Private file_path As String
    Private import_as_rtf As Boolean
    Private p1 As Integer
    Private p2 As Integer
    Private m() As String
    Private Class scls_line_rule
        Public to_part As Integer
        Public rgx_rule1 As String
    End Class

    Dim cl_rules(1) As scls_line_rule
    Public Sub New(name_ As String, name_id_ As String, mark_ As String, description_ As String, mark1_hgl As cls_highligh_rule)
        MyBase.set_basics(name_, name_id_, mark_, description_, mark1_hgl)
        cl_rules(0) = New scls_line_rule
        cl_rules(1) = New scls_line_rule

        cl_rules(0).rgx_rule1 = "^[^ ]"
        cl_rules(1).rgx_rule1 = "^\s"

        hgl(0) = New cls_highligh_rule("bc:orange3")
        hgl(1) = New cls_highligh_rule("bc:yellow3")
        ReDim m(1)
        m(0) = "přeskládaný text - sl 1"
        m(1) = "přeskládaný text - sl 2"
        env.wsp.marks.add_mark(m(0), hgl(0))
        env.wsp.marks.add_mark(m(1), hgl(1))
    End Sub
    Public Overrides Sub dispose_controls()

    End Sub

    Public Overrides Sub create_controls(container As Control, last_visualized_tool As Object)
        Me.clean_container(container, last_visualized_tool)
        Dim lbl As Label
        Dim txt As TextBox
        Dim chb As CheckBox
        Dim cmd As Button

        With NewCtrl(cmd, New Button, container)
            .Top = 5
            .Left = 5
            .Text = "Přeskládej!"
            cmd.AutoSize = True
            AddHandler .Click, AddressOf cmd_run_click
        End With
        container.Visible = True
    End Sub
    Private Sub cmd_run_click(sender As Object, e As EventArgs)
        run(env._p, 0)
        env.wsp.display_page(Nothing, Split(m(0) & "|" & m(1)))
    End Sub

    Public Overrides Sub run(pp As cls_preXML_section_page, mode As Integer)
        'nic
        Dim c() As String
        ReDim c(UBound(cl_rules))


        Dim i As Integer
        Dim j As Integer
        For i = 0 To pp.n_lines
            If rgxt(pp.line(i), "^\s*<[^>]*>\s*$") = False Then
                For j = 0 To UBound(cl_rules)
                    If rgxt(pp.line(i), cl_rules(j).rgx_rule1) = True Then
                        If cl_rules(j).to_part <> -1 Then 'to znamená smazat
                            c(cl_rules(j).to_part) &= pp.line(i) & vbLf
                        End If
                    End If
                Next j
            End If
        Next
        pp.text_changed(c(0), 1, "přeskládaný sloupec 1")
        pp.text_changed(c(1), 2, "přeskládaný sloupec 2")

        If Me.parent.n_tt = -1 Then
            env.wsp.display_page(Nothing)
        Else
            Me.parent.execute(pp)
        End If
    End Sub

    Public Overrides Function clone() As Object
    End Function

    Public Overrides Function generate_context_menu(p As cls_preXML_section_page, cmn As cls_context_menu) As Object
    End Function

    Public Overrides Function context_menu_activated(p As cls_preXML_section_page, p1 As Object, p2 As Object) As Object
    End Function
    Public Sub New(n As Xml.XmlNode)
        __xml(Nothing, n, False)
    End Sub
    Private Function __xml(x As Xml.XmlDocument, n_imp As Xml.XmlNode, export As Boolean) As Xml.XmlNode
        Dim n As Xml.XmlNode
        Dim i As Long
        If export = True Then
            n = x.CreateNode(Xml.XmlNodeType.Element, "tool", "")
            MyBase.export_base_to_xml(n, x)
            n.AppendChild(x.CreateNode(XmlNodeType.Element, "file_path", "")).InnerText = Me.file_path
            n.AppendChild(x.CreateNode(XmlNodeType.Element, "p1", "")).InnerText = Me.p1
            n.AppendChild(x.CreateNode(XmlNodeType.Element, "p2", "")).InnerText = Me.p2
            n.AppendChild(x.CreateNode(XmlNodeType.Element, "import_as_rtf", "")).InnerText = Me.import_as_rtf
        Else
            Me.file_path = get_singlenode_value(n_imp, "file_path")
            Me.p1 = get_singlenode_value(n_imp, "p1")
            Me.p2 = get_singlenode_value(n_imp, "p2")
            Me.import_as_rtf = get_singlenode_value(n_imp, "import_as_rtf")
            MyBase.import_base_from_xml(n_imp)
        End If

        If export = True Then
            Return n
        End If
    End Function
    Public Overrides Function export_to_xml(x As XmlDocument) As XmlNode
        Return __xml(x, Nothing, True)
    End Function

End Class
Public Class cls_tool_IF
    Inherits cls_tool

    Private shortcut_ As cls_event_description
    Private primum_comparationis As OBJECTS
    Private on_false As String
    Public Enum OPERATORS
        IS_EQUAL
        LOWER_THAN
        LOWER_OR_EQUAL
        GREATER_THAN
        GREATER_OR_EQUAL
        CONTAINS
        REGEX_IS_EQUAL
    End Enum

    Private object_to_evaluate

    Public Class scls_condition
        Public parent As cls_tool_IF
        Public o As OPERATORS 'operator
        Public rgx_pattern As String
        'Public primum_comparationis As OBJECTS
        Public secundum_comparationis As Object

        Public negate As Boolean
        Public Function evaluate(against As Object) As Boolean
            Dim my_result As Boolean
            Try
                If o = OPERATORS.IS_EQUAL Then
                    my_result = CBool(against = secundum_comparationis)
                ElseIf o = OPERATORS.CONTAINS Then
                    If against IsNot Nothing Then
                        If IsArray(against) Then
                            my_result = False
                            For i = 0 To UBound(against)
                                If against(i) = secundum_comparationis Then
                                    my_result = True
                                    Exit For
                                End If
                            Next
                        End If
                    End If
                End If
            Catch
                Return False
            End Try
            If AND_cond IsNot Nothing Then
                my_result = my_result And AND_cond.evaluate(against)
            ElseIf OR_cond IsNot Nothing Then
                my_result = my_result Or OR_cond.evaluate(against)
            End If
            If negate Then my_result = Not my_result
            Return my_result
        End Function

        Public AND_cond As scls_condition
        Public OR_cond As scls_condition

        Public ON_TRUE() As Object
        Public ON_FALSE() As Object
        Public Function export_to_xml(x As Xml.XmlDocument) As Xml.XmlNode
            Return __xml(x, Nothing, True)
        End Function
        Private Function __xml(x As Xml.XmlDocument, n_imp As Xml.XmlNode, export As Boolean) As Xml.XmlNode
            Dim n As Xml.XmlNode
            Dim n2 As Xml.XmlNode
            If export = True Then
                n = x.CreateNode(XmlNodeType.Element, "condition", "")
                n.AppendChild(x.CreateNode(XmlNodeType.Element, "negate", "")).InnerText = negate
                n.AppendChild(x.CreateNode(XmlNodeType.Element, "operator", "")).InnerText = o
                n2 = n.AppendChild(x.CreateNode(XmlNodeType.Element, "secundum_comparationis", ""))
                If secundum_comparationis.GetType = GetType(String) Then
                    n2.Attributes.Append(x.CreateAttribute("dtype")).InnerText = "string"
                    n2.InnerText = CStr(secundum_comparationis)
                End If
                If AND_cond IsNot Nothing Then
                    n2 = n.AppendChild(x.CreateNode(XmlNodeType.Element, "and_cond", ""))
                    n2.AppendChild(AND_cond.export_to_xml(x))
                End If
                If OR_cond IsNot Nothing Then
                    n2 = n.AppendChild(x.CreateNode(XmlNodeType.Element, "or_cond", ""))
                    n2.AppendChild(OR_cond.export_to_xml(x))
                End If
                Return n
            Else
                negate = CBool(get_singlenode_value(n_imp, "negate", "false"))
                o = CInt(get_singlenode_value(n_imp, "operator", "-1"))
                Dim sc_type As String
                sc_type = get_singlenode_value(n_imp, "secundum_comparationis/@dtype", "")
                If sc_type = "string" Then
                    secundum_comparationis = get_singlenode_value(n_imp, "secundum_comparationis", "")
                End If
                n = n_imp.SelectSingleNode("and_cond/condition")
                If n IsNot Nothing Then AND_cond = New scls_condition(n)
                n = n_imp.SelectSingleNode("or_cond/condition")
                If n IsNot Nothing Then OR_cond = New scls_condition(n)

            End If
        End Function
        Public Sub New(n As Xml.XmlNode)
            __xml(Nothing, n, False)
        End Sub
        Public Sub New(secundum_comparationis_ As Object, negate_ As Boolean, operator_ As Integer, Optional and_cond_ As scls_condition = Nothing,
                       Optional or_cond_ As scls_condition = Nothing)
            secundum_comparationis = secundum_comparationis_
            negate = negate_
            o = operator_
            AND_cond = and_cond_
            OR_cond = or_cond_
        End Sub
    End Class
    Private Class scls_reaction
        Public raise_function As String
        Public Function raise(p As cls_preXML_section_page)
            If raise_function <> "" Then
                env.wsp.tm.evoke(raise_function, p)
            End If
        End Function
    End Class
    Public Enum OBJECTS
        AC_P_SELSTART_0b
        AC_P_CONTEXT_WORD
        AC_P_CONTEXT_LAST_OPENED_ELEMENT
        AC_P_CONTEXT_IN_TAG
        AC_P_CONTEXT_TAG
        AC_P_CONTEXT_OPENED_ELEMENTS
    End Enum

    Private Function getObj(page As cls_preXML_section_page, obj As OBJECTS) As Object
        Select Case obj
            Case OBJECTS.AC_P_SELSTART_0b
                Return page.SelStart0b
            Case OBJECTS.AC_P_CONTEXT_WORD
                Return page.context.word
            Case OBJECTS.AC_P_CONTEXT_TAG
                Return page.context.inside_of_tag
            Case OBJECTS.AC_P_CONTEXT_LAST_OPENED_ELEMENT
                If page.context.n_tags_opened <> -1 Then Return page.context.tags_opened(page.context.n_tags_opened)
            Case OBJECTS.AC_P_CONTEXT_OPENED_ELEMENTS
                Dim tmp() As cls_preXML_tag
                If page.context.n_tags_opened <> -1 Then
                    ReDim tmp(page.context.n_tags_opened)
                    For i = 0 To page.context.n_tags_opened
                        tmp(i) = page.context.tags_opened(i)
                    Next i
                    Return tmp
                End If
        End Select
    End Function

    Private type_of_object_examined As Integer '0=řádek

    Private cond As scls_condition
    Private react() As scls_reaction

    Public Property shortcut As cls_event_description
        Set(value As cls_event_description)
            If shortcut_ IsNot Nothing Then
                event_listeners(0).remove_all_connections()
                Erase event_listeners
                ReDim event_listeners(0)
            Else
                ReDim event_listeners(0)
            End If
            event_listeners(0) = New cls_event_listener(Me, 1, "Spustit vyhodnocení podmínky a případné následné akce.")
            event_listeners(0).connect_to_event(value, 0)
        End Set
        Get
            Return shortcut_
        End Get
    End Property

    Public Sub New(name_ As String, name_id_ As String, mark_ As String, description_ As String, mark1_hgl As cls_highligh_rule)
        MyBase.set_basics(name_, name_id_, mark_, description_, mark1_hgl)

    End Sub
    Public Sub New(name_ As String, name_id_ As String, mark_ As String, description_ As String, mark1_hgl As cls_highligh_rule,
                   cond_ As scls_condition, primum_comparationis_ As Object, on_false_ As String)
        MyBase.set_basics(name_, name_id_, mark_, description_, mark1_hgl)
        Me.on_false = on_false_
        Me.primum_comparationis = primum_comparationis_
        Me.cond = cond_
    End Sub
    Public Overrides Sub dispose_controls()

    End Sub

    Public Overrides Sub create_controls(container As Control, last_visualized_tool As Object)

    End Sub
    Public Function Raise(p As cls_preXML_section_page, e As Object, mode As Integer) As Object
        run(p, 1)
    End Function
    Public Overrides Sub run(pp As cls_preXML_section_page, mode As Integer)
        'cond = New scls_condition
        'cond.o = OPERATORS.CONTAINS

        If cond IsNot Nothing Then
            If cond.evaluate(getObj(pp, primum_comparationis)) = True Then
                Me.parent.execute(pp)
            Else
                If Me.on_false <> "" Then
                    If LCase(Left(Me.on_false, 4)) = "msg " Then
                        Dim msg As String = rgx_g(on_false, "^msg\s(.*)$")
                        MsgBox(msg)
                    End If
                End If
                End If
        End If
    End Sub

    Public Overrides Function clone() As Object
    End Function

    Public Overrides Function generate_context_menu(p As cls_preXML_section_page, cmn As cls_context_menu) As Object
    End Function

    Public Overrides Function context_menu_activated(p As cls_preXML_section_page, p1 As Object, p2 As Object) As Object
    End Function
    Public Sub New(n As Xml.XmlNode)
        __xml(Nothing, n, False)
    End Sub
    Private Function __xml(x As Xml.XmlDocument, n_imp As Xml.XmlNode, export As Boolean) As Xml.XmlNode
        Dim n As Xml.XmlNode
        Dim n2 As Xml.XmlNode
        Dim i As Long
        If export = True Then
            n = x.CreateNode(Xml.XmlNodeType.Element, "tool", "")
            MyBase.export_base_to_xml(n, x)

            If shortcut_ IsNot Nothing Then
                n2 = n.AppendChild(x.CreateNode(XmlNodeType.Element, "shortcut", ""))
                n2.AppendChild(shortcut_.export_to_xml(x))
            End If
            If cond IsNot Nothing Then
                n2 = n.AppendChild(cond.export_to_xml(x))
            End If
            n.AppendChild(x.CreateNode(XmlNodeType.Element, "primum_comparationis", "")).InnerText = primum_comparationis
            n.AppendChild(x.CreateNode(XmlNodeType.Element, "on_false", "")).InnerText = on_false
            'n.AppendChild(x.CreateNode(XmlNodeType.Element, "p1", "")).InnerText = Me.p1
            'n.AppendChild(x.CreateNode(XmlNodeType.Element, "p2", "")).InnerText = Me.p2
            'n.AppendChild(x.CreateNode(XmlNodeType.Element, "import_as_rtf", "")).InnerText = Me.import_as_rtf
        Else
            MyBase.import_base_from_xml(n_imp)
            n = n_imp.SelectSingleNode("shortcut")
            If n IsNot Nothing Then Me.shortcut = New cls_event_description(n)
            n = n_imp.SelectSingleNode("condition")
            If n IsNot Nothing Then Me.cond = New scls_condition(n)
            primum_comparationis = get_singlenode_value(n_imp, "primum_comparationis", "0")
            on_false = get_singlenode_value(n_imp, "on_false", "")
        End If

        If export = True Then
            Return n
        End If
    End Function
    Public Overrides Function export_to_xml(x As XmlDocument) As XmlNode
        Return __xml(x, Nothing, True)
    End Function

End Class
'############################################################################################################################################################
'############################################################################################################################################################



Public Class cls_tool_Insert_lb_tags
    Inherits cls_tool
    Public on_beginning As Boolean = True
    Public continued_numbering As Boolean 'zda se má pokračovat tam, kde se skončilo na minulé stránce
    Public blank_lines_too As Boolean 'zda číslovat i prázdné řádky
    Public n_param_every_x As Integer = 5
    Public tag_mode As String = "lb"

    Public lbl_name As Label
    Public chb_continue As CheckBox
    Public chb_tag_on_beginning As CheckBox
    Public chb_blank_lines As CheckBox
    Public lbl_txtinfo As Label
    Public txt_every_x As TextBox
    Public lbl_description As Label
    Public cmd_run As Button


    Public anchor_to_preXML_ln As Boolean = True

    Public Overrides Function clone() As Object
        Dim tmp As cls_tool_Insert_lb_tags
        tmp = New cls_tool_Insert_lb_tags
        clone_base(tmp)
        tmp.on_beginning = Me.on_beginning
        tmp.continued_numbering = Me.continued_numbering
        tmp.blank_lines_too = Me.blank_lines_too
        tmp.n_param_every_x = Me.n_param_every_x
        Return tmp
    End Function

    Public Sub New()
        'kvůli klonování
    End Sub
    Public Sub New(name_id_ As String, name_ As String, description_ As String, mark_ As String,
                        Optional on_beginning_ As Boolean = True, Optional continued_numbering_ As Boolean = False,
                        Optional blank_lines_too_ As Boolean = False, Optional every_x_ As Integer = 5,
                        Optional default_mode_ As Integer = 1, Optional auto_insert_ As Integer = 0, Optional type_ As String = "",
                        Optional hgl1_ As cls_highligh_rule = Nothing)

        name_id = name_id_
        on_beginning = on_beginning_
        continued_numbering = continued_numbering_
        blank_lines_too = blank_lines_too_
        n_param_every_x = every_x_
        mark = mark_
        name = name_
        description = description_
        type = type_
        hgl(0) = hgl1_
        env.wsp.marks.add_mark(mark, hgl(0))
    End Sub
    Public Sub New(name_ As String, name_id_ As String, mark_ As String, description_ As String, mark1_hgl As cls_highligh_rule)
        MyBase.set_basics(name_, name_id_, mark_, description_, mark1_hgl)
    End Sub

    Public Overrides Function export_to_xml(x As Xml.XmlDocument) As Xml.XmlNode
        Dim n As Xml.XmlNode
        n = x.CreateNode(Xml.XmlNodeType.Element, "tool", "")
        MyBase.export_base_to_xml(n, x)

        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "on_beginning", "")).InnerText = Me.on_beginning
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "continued_numbering", "")).InnerText = Me.continued_numbering
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "blank_lines_too", "")).InnerText = Me.blank_lines_too
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "n_param_every_x", "")).InnerText = Me.n_param_every_x
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "tag_mode", "")).InnerText = Me.tag_mode

        Return n
    End Function
    Public Sub New(n As Xml.XmlNode)
        If n IsNot Nothing Then
            MyBase.import_base_from_xml(n)
            on_beginning = get_singlenode_value(n, "on_beginning")
            continued_numbering = get_singlenode_value(n, "continued_numbering")
            blank_lines_too = get_singlenode_value(n, "blank_lines_too")
            n_param_every_x = get_singlenode_value(n, "n_param_every_x")
            tag_mode = get_singlenode_value(n, "tag_mode", "lb")
        End If
    End Sub
    Public Function Raise(p As cls_preXML_section_page, e As Object, mode As Integer) As Object
        run(p, 1)
    End Function
    Public Overloads Sub run()
        run(env._p, -1)
    End Sub

    Public Overrides Sub run(pp As cls_preXML_section_page, mode As Integer)
        If pp IsNot Nothing Then
            pp.save_state()
            Dim i As Integer
            Dim n As Integer
            Dim preXML_ln As String
            Dim ls_index As Integer
            Dim line_end As Integer
            Dim m As Match
            If anchor_to_preXML_ln = True And Me.n_param_every_x > 0 Then
                For i = 0 To pp.n_lines
                    If (Trim(rgxr(pp.line(i), "<[^>]*>", "") <> "") Or rgxt(pp.line(i), "<preXML_ln[^>]*>") = True) Or Me.blank_lines_too = True Then 'jen neprázdné řádky, nebo se zapnutou volbou i prázdných řádků
                        ls_index = pp.line_start_index(i) + 1
                        If rgxt(pp.line(i), "^\s*<lb[\s/]") = False And rgxt(pp.line(i), "^\s*<l[\s>]") = False Then 'nebudeme je přidávat, pokud už tam jsou...
                            preXML_ln = rgx(pp.line(i), "^\s*<preXML_ln n='([0-9]+)'/>",, m)

                            If preXML_ln <> "" Then
                                pp.delete_text_on_position(pp.line_start_index(i) + 1 + m.Index, pp.line_start_index(i) + m.Index + m.Length)
                                'pp.search_and_replace(False, preXML_ln, "")
                                preXML_ln = rgx_g(preXML_ln, "^<preXML_ln n='([0-9]+)'/>")
                                If tag_mode = "lb" Then
                                    pp.insert_on_position(pp.line_start_index(i) + 1, "<lb n='" & preXML_ln & "'/>", mark)
                                Else
                                    pp.insert_on_position(pp.line_start_index(i) + 1, "<l n='" & preXML_ln & "'>", mark)
                                    line_end = pp.line_start_index(i) + Len(pp.line(i)) + 1
                                    pp.insert_on_position(line_end, "</l>", mark)
                                End If
                            Else
                                If tag_mode = "lb" Then
                                    pp.insert_on_position(pp.line_start_index(i) + 1, "<lb/>", mark)
                                Else
                                    pp.insert_on_position(pp.line_start_index(i) + 1, "<l>", mark)
                                    line_end = pp.line_start_index(i) + Len(pp.line(i)) + 1
                                    pp.insert_on_position(line_end, "</l>", mark)
                                End If
                            End If
                        End If
                    End If
                Next i
            Else
                'If mode = tm_mode.TM_EXECUTE Then

                If Me.continued_numbering = True Then
                    'tady budeme muset zjistit poslední použité číslo
                Else
                    n = 1
                End If

                For i = 0 To pp.n_lines
                    If Trim(rgxr(pp.line(i), "<[^>]*>", "") <> "") Or Me.blank_lines_too = True Then 'jen neprázdné řádky, nebo se zapnutou volbou i prázdných řádků
                        ls_index = pp.line_start_index(i) + 1
                        If Me.n_param_every_x <> 0 Then
                            If n Mod Me.n_param_every_x = 0 And Me.n_param_every_x <> -1 Then
                                If tag_mode = "lb" Then
                                    pp.insert_on_position(ls_index, "<lb n='" & n & "'/>", mark)
                                Else
                                    pp.insert_on_position(ls_index, "<l n='" & n & "'>", mark)
                                    line_end = pp.line_start_index(i) + Len(pp.line(i)) + 1
                                    pp.insert_on_position(line_end, "</l>", mark)
                                End If
                            Else
                                If tag_mode = "lb" Then
                                    pp.insert_on_position(ls_index, "<lb/>", mark)
                                Else
                                    pp.insert_on_position(ls_index, "<l>", mark)
                                    line_end = pp.line_start_index(i) + Len(pp.line(i)) + 1
                                    pp.insert_on_position(line_end, "</l>", mark)
                                End If
                            End If
                        Else
                            If tag_mode = "lb" Then
                                pp.insert_on_position(ls_index, "<lb/>", mark)
                            Else
                                pp.insert_on_position(ls_index, "<l>", mark)
                                line_end = pp.line_start_index(i) + Len(pp.line(i)) + 1
                                pp.insert_on_position(line_end, "</l>", mark)
                            End If
                        End If
                        n = n + 1
                    End If
                Next

                ' End If
            End If
            env.wsp.display_page(Nothing)
            Me.parent.execute(pp) 'rodičem je objekt tool_organizer, který na tento podnět spustí všechny podřazené nástroje.
            'Pomocí události to (snadno) nejde, protože v rodiči je tento nástroj deklarován jako "object", ne svým datovým typem. A to pak nefunguje...
        End If
    End Sub
    Public Overrides Function generate_context_menu(p As cls_preXML_section_page, cmn As cls_context_menu) As Object
        Return False
    End Function
    Public Overrides Function context_menu_activated(p As cls_preXML_section_page, p1 As Object, p2 As Object)

    End Function
    Public Overrides Sub dispose_controls()
        lbl_description = Nothing
        lbl_name = Nothing
        lbl_txtinfo = Nothing
        chb_blank_lines = Nothing
        chb_continue = Nothing
        chb_tag_on_beginning = Nothing
        txt_every_x = Nothing
        cmd_run.Dispose()
        cmd_run = Nothing
    End Sub
    Public Overrides Sub create_controls(container As Control, last_visualized_tool As Object)
        Me.clean_container(container, last_visualized_tool)
        lastctrl = Nothing

        Dim lbl As Label
        With NewCtrl(lbl_description, New Label, container)
            .Top = 0
            .Left = 0
            .Width = container.Width
            .Text = Me.name
            .AutoSize = False
        End With
        chb_continue = New CheckBox
        With chb_continue
            .Top = TpH() + 5
            .Width = container.Width
            .Parent = container
            .Text = env.c("Číslování pokračuje z předchozí strany", "Numbering continues from preceding page")
            .Checked = continued_numbering
        End With

        chb_blank_lines = New CheckBox
        With chb_blank_lines
            .Top = chb_continue.Top + chb_continue.Height + 5
            .Width = container.Width
            .Parent = container
            .Text = env.c("Očíslovat i prázdné řádky", "Number blank lines too")
            .Checked = blank_lines_too
        End With

        chb_tag_on_beginning = New CheckBox
        With chb_tag_on_beginning
            .Top = chb_blank_lines.Top + chb_blank_lines.Height + 5
            .Width = container.Width
            .Parent = container
            .Text = env.c("Tagy vkládat na začátek řádků (nezaškrtnuto=na konec, jen u <lb/>)",
                          "Insert tags on the beginning of the lines (only for <lb/>)")
            .Checked = on_beginning
        End With

        With NewCtrl(lbl, New Label, container)
            .Top = chb_tag_on_beginning.Top + chb_tag_on_beginning.Height
            .AutoSize = True
            .Text = env.c("Číslo vložit každých X řádků (0=bez číslování): ",
                          "Insert number every X lines (0=no numbering): ")
        End With

        With NewCtrl(txt_every_x, New TextBox)
            .Top = chb_tag_on_beginning.Top + chb_tag_on_beginning.Height
            .Left = LpW() + 5
            .Width = 20
            .Parent = container
            .Text = n_param_every_x
            AddHandler .TextChanged, AddressOf txt_every_X_changed
        End With
        Dim chb As CheckBox
        With NewCtrl(chb, New CheckBox, container)
            .Text = env.c("Kotvit k <preXML_ln>", "Anchor to <preXML_ln>")
            .Top = TpH() + 5
            .Left = 5
            chb.AutoSize = True
            chb.Checked = Me.anchor_to_preXML_ln
            AddHandler chb.CheckedChanged, AddressOf chb_anchor_changed
        End With

        Dim rbt As RadioButton
        With NewCtrl(rbt, New RadioButton, container)
            .Text = env.c("Vkládat značky <lb/>", "Insert <lb/> tags")
            .Top = TpH() + 5
            .Left = 5
            rbt.AutoSize = True
            rbt.Checked = CBool(tag_mode = "lb")
            rbt.Tag = "lb"
            AddHandler rbt.CheckedChanged, AddressOf rbt_mode_changed
        End With
        With NewCtrl(rbt, New RadioButton, container)
            .Text = env.c("Vkládat značky <l>...</l>", "Insert <l>...</l> tags")
            .Top = TpH() + 5
            .Left = 5
            rbt.AutoSize = True
            rbt.Checked = CBool(tag_mode = "l")
            rbt.Tag = "l"
            AddHandler rbt.CheckedChanged, AddressOf rbt_mode_changed
        End With
        With NewCtrl(cmd_run, New Button)
            .Top = TpH() + 10
            .Width = 100
            .Text = "Vlož!"
            .Left = container.Width - .Width - 10
            .Parent = container
            AddHandler .Click, AddressOf cmd_run_click
        End With
        With NewCtrl(lbl_description, New Label, container)
            .Top = TpH() + 5
            .Left = 5
            .Width = container.Width
            .Height = 100
            .Text = Me.description
            .AutoSize = False
        End With

        container.Visible = True
    End Sub

    Private Sub chb_anchor_changed(sender As Object, e As EventArgs)
        Me.anchor_to_preXML_ln = sender.checked
    End Sub
    Private Sub rbt_mode_changed(sender As Object, e As EventArgs)
        If sender.checked = True Then
            tag_mode = sender.tag
        End If
    End Sub
    Public Sub cmd_run_click(sender As Object, e As EventArgs)
        run(env._p, -1)
    End Sub
    Public Sub chb_continued_CheckedChanged(sender As Object, e As EventArgs)
        continued_numbering = sender.checked
    End Sub
    Public Sub chb_onbeginning_CheckedChanged(sender As Object, e As EventArgs)
        on_beginning = sender.checked
    End Sub
    Public Sub chb_blank_lines_CheckedChanged(sender As Object, e As EventArgs)
        blank_lines_too = sender.checked
    End Sub

    Public Sub txt_every_X_changed(sender As Object, e As EventArgs)
        Static locked As Boolean
        If locked = True Then Exit Sub
        If rgxt(sender.text, "^\-?[0-9]+$") = True Then
            n_param_every_x = CLng(sender.text)
            If n_param_every_x > 100 Then
                n_param_every_x = 100
                locked = True
                sender.text = n_param_every_x
                locked = False
                If n_param_every_x <= 0 Then n_param_every_x = -1
            End If
        End If
    End Sub



End Class

Public Class cls_tool_Tags_Insertion

    Inherits cls_tool

    Private Class cls_tag_setting
        Public tag As String
        Public attributes() As String
        Public self_closing As Boolean
        Public n_attr As Long
        Private shortcut_ As cls_keyevent_args
        Private parent As cls_tool_Tags_Insertion
        Shared mode As Long
        Public event_listener As cls_event_listener 'toto je reference na eventlistener rodičovského objektu, který je tím "skutečným" event listenerem
        'tady je to jenom proto, abychom věděli, jaký event listener případně měnit při změně kl. zkratky
        Public event_listener_context As String
        Public selection_transform_pattern As String
        Public isolate_line As Boolean
        Public wrap_every_line As Boolean

        Public Sub New(t As String, self_closing_ As Boolean, isolate As Boolean, ParamArray attr() As String)
            'pro dočasné objekty bez kl. zkratky
            mode += 1
            If attr Is Nothing Then
                n_attr = -1
                Erase attributes
            Else
                n_attr = UBound(attr)
                attributes = attr
            End If
            tag = t
            self_closing = self_closing_
            isolate_line = isolate
        End Sub


        Public Sub New(parent_ As Object, t As String, self_closing_ As Boolean, isolate As Boolean, kshortcut As cls_keyevent_args, ParamArray attr() As String)
            If mode = 0 Then
                mode = 1000
            End If
            mode += 1
            parent = parent_
            If attr Is Nothing Then
                n_attr = -1
                Erase attributes
            Else
                n_attr = UBound(attr)
                attributes = attr
            End If
            tag = t
            self_closing = self_closing_
            isolate_line = isolate

            shortcut = kshortcut

        End Sub

        Friend Function export_to_xml(x As Xml.XmlDocument) As Xml.XmlNode
            Return __xml(x, Nothing, True)
        End Function
        Private Function __xml(x As Xml.XmlDocument, n_imp As Xml.XmlNode, export As Boolean) As Xml.XmlNode
            Dim n As Xml.XmlNode
            If export = True Then n = x.CreateNode(Xml.XmlNodeType.Element, "tag_setting", "")
            If export = True Then
                n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "tag", "")).InnerText = Me.tag
            Else
                tag = get_singlenode_value(n_imp, "tag")
            End If
            If export = True Then
                n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "self_closing", "")).InnerText = Me.self_closing
            Else
                self_closing = CBool(get_singlenode_value(n_imp, "self_closing"))
            End If
            If export = True Then
                n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "isolate_line", "")).InnerText = Me.isolate_line
            Else
                isolate_line = get_singlenode_value(n_imp, "isolate_line")
            End If
            If export = True Then
                n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "wrap_every_line", "")).InnerText = Me.wrap_every_line
            Else
                wrap_every_line = get_singlenode_value(n_imp, "wrap_every_line", "False")
            End If
            If export = True Then
                If Me.shortcut_ IsNot Nothing Then
                    n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "shortcut", "")).AppendChild(Me.shortcut_.export_to_xml(x))
                End If
            Else
                n = n_imp.SelectSingleNode("shortcut")
                If Not n Is Nothing Then shortcut = New cls_keyevent_args(n)
            End If
            If export = True Then Return n
        End Function
        Public Sub New(n As Xml.XmlNode, parent_ As Object)
            Me.parent = parent_
            If mode = 0 Then
                mode = 1000
            End If
            mode += 1
            __xml(Nothing, n, False)
        End Sub
        Public Property shortcut As cls_keyevent_args
            Get
                Return shortcut_
            End Get
            Set(value As cls_keyevent_args)
                If value IsNot Nothing Then
                    shortcut_ = value
                    If event_listener IsNot Nothing Then
                        event_listener.remove_all_connections()
                    Else
                        event_listener = New cls_event_listener(Me.parent, Me.mode, "Tag " & tag)
                    End If
                    event_listener.connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, value, Nothing), 0)
                End If
            End Set
        End Property

        Public Sub insert(p As cls_preXML_section_page, Optional closing As Boolean = False, Optional ByRef first_char_after0b As Integer = 0)
            Dim pos As Long
            Dim pos_end As Long
            p.context.get_context()

            If p.SelStart1b = 0 Then Exit Sub


            If p.context.inside_of_tag <> "" Then
                pos_end = p.SelStart1b + p.SelLength
                pos = InStr(p.SelStart1b, p.plain_text, ">") + 1

            Else
                pos_end = p.SelStart1b + p.SelLength
                pos = p.SelStart1b
            End If
            'xyz = Asc(p.plain_text(pos_end - 1))
            If pos_end > 1 Then
                If p.plain_text(pos_end - 2) = vbLf And pos_end <> 1 Then pos_end -= 1
            End If
            If p.SelLength = 0 Then

                If closing = False Then
                    insert_opening(p, pos, first_char_after0b)
                Else
                    insert_closing(p, pos, first_char_after0b)
                End If
            Else
                If Me.self_closing = False Then
                    If Me.wrap_every_line = False Then
                        insert_closing(p, pos_end, first_char_after0b)
                        insert_opening(p, pos, first_char_after0b)
                    Else
                        Dim first_l As Integer
                        Dim last_l As Integer
                        first_l = p.line_from_char_index(pos)
                        last_l = p.line_from_char_index(pos_end)
                        Dim i As Long
                        For i = first_l To last_l
                            If rgxt(p.line(i), "^\s*<" & Me.tag & "[ />]*>") = False Then
                                p.insert_on_position(p.line_start_index(i) + 1, "<" & Me.tag & ">", "", first_char_after0b)
                                p.insert_on_position(p.line_end_index(i) + 1, "</" & Me.tag & ">", "", first_char_after0b)
                            End If
                        Next

                    End If
                End If
            End If
            p.force_SelLength = 0
            p.force_SelStart = p.SelStart0b + 1
            My.Computer.Keyboard.SendKeys("{RIGHT}")
            My.Computer.Keyboard.SendKeys("{LEFT}")
        End Sub
        Public Sub insert_opening(p As cls_preXML_section_page, pos1b As Long, Optional ByRef first_char_after0b As Integer = -1)
            Dim sc As String
            If self_closing = True Then sc = "/"
            If isolate_line = False Then
                p.insert_on_position(pos1b, "<" & Me.tag & get_attributes() & sc & ">", "", first_char_after0b)
            Else
                If Trim(p.line(p.line_from_char_index(pos1b))) <> "" Then
                    p.insert_on_position(pos1b, vbLf & "<" & Me.tag & get_attributes() & sc & ">" & vbLf, "<<<|>>>", first_char_after0b)
                Else
                    p.insert_on_position(pos1b, "<" & Me.tag & get_attributes() & sc & ">", "<<<|>>>", first_char_after0b)
                End If
            End If
            p.plain_text_selection_changed(p.SelStart0b + Len("<" & Me.tag & get_attributes() & sc & ">"), 0)
        End Sub
        Public Sub insert_closing(p As cls_preXML_section_page, pos1b As Long, Optional ByRef first_char_after0b As Integer = 0)
            Dim sc As String
            Dim c As String
            Dim first_after As Integer
            If self_closing = True Then
                sc = get_attributes() & "/"
                c = ""
            Else
                sc = ""
                c = "/"
            End If
            Dim ttag As String = tag
            If InStr(ttag, " ") > 1 Then ttag = Left(ttag, InStr(1, ttag, " ") - 1)
            If isolate_line = False Then
                p.insert_on_position(pos1b, "<" & c & ttag & sc & ">", "<<<|>>>", first_char_after0b)
            Else
                If Trim(p.line(p.line_from_char_index(pos1b))) <> "" Then
                    p.insert_on_position(pos1b, vbLf & "<" & c & ttag & sc & ">" & vbLf, "<<<|>>>", first_char_after0b)
                Else
                    p.insert_on_position(pos1b, "<" & c & ttag & sc & ">", "<<<|>>>", first_char_after0b)
                End If
            End If
            p.plain_text_selection_changed(p.SelStart0b + Len("<" & c & ttag & sc & ">"), 0)
        End Sub
        Private Function get_attributes() As String
            If n_attr = -1 Then Return ""
        End Function

    End Class

    Public closing_tag_modification_key As String
    Private tags() As cls_tag_setting
    Private n_tags As Long = -1
    Public event_listeners(1) As cls_event_listener
    Public tags_events_listeners() As cls_event_listener
    Private closing_tag_insertion_ As cls_keyevent_args
    Public Sub New()
        '
    End Sub
    Public Sub New(name_id_ As String, name_ As String, description_ As String, mark_ As String)
        Me.name_id = name_id_
        Me.name = name_
        Me.description = description_
        Me.mark = mark_

        Me.closing_tag_modification_key = "shift"
        create_standart_tags()
    End Sub
    Public Sub New(name_ As String, name_id_ As String, mark_ As String, description_ As String, mark1_hgl As cls_highligh_rule)
        MyBase.set_basics(name_, name_id_, mark_, description_, mark1_hgl)
        create_standart_tags()
    End Sub
    Public Overrides Function export_to_xml(x As Xml.XmlDocument) As Xml.XmlNode

        Return __xml(x, Nothing, True)
    End Function
    Private Function __xml(x As Xml.XmlDocument, n_imp As Xml.XmlNode, export As Boolean) As Xml.XmlNode
        Dim n As Xml.XmlNode, n2 As Xml.XmlNode
        Dim i As Long
        If export = True Then
            n = x.CreateNode(Xml.XmlNodeType.Element, "tool", "")
            MyBase.export_base_to_xml(n, x)
        Else
            MyBase.import_base_from_xml(n_imp)
        End If

        If export = True Then
            If Me.closing_tag_insertion_ IsNot Nothing Then
                n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "insert_closing_shortcut", "")).AppendChild(Me.closing_tag_insertion_.export_to_xml(x))
            End If
        Else
            n = n_imp.SelectSingleNode("insert_closing_shortcut/key_event_args")
            If n IsNot Nothing Then
                Me.closing_tag_insertion_ = New cls_keyevent_args(n)
                event_listeners(1) = New cls_event_listener(Me, 2, env.c("Uzavře otevřený element.", "Closes opened element"))
                event_listeners(1).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, Me.closing_tag_insertion_, Nothing), 0)
            End If
        End If

        If export = True Then
            n2 = n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "tags", ""))
            For i = 0 To Me.n_tags
                n2.AppendChild(Me.tags(i).export_to_xml(x))
            Next
        Else
            n = n_imp.SelectSingleNode("tags")
            If Not n Is Nothing Then
                n_tags = n.ChildNodes.Count - 1
                ReDim tags(n_tags)
                For i = 0 To n_tags
                    tags(i) = New cls_tag_setting(n.ChildNodes(i), Me)
                Next
            Else
                create_standart_tags()
            End If
        End If
        If export = True Then Return n
    End Function
    Public Sub New(n As Xml.XmlNode)
        __xml(Nothing, n, False)
    End Sub
    Private Sub create_standart_tags()
        n_tags = 5
        ReDim tags(n_tags)
        ReDim tags_events_listeners(n_tags)

        event_listeners(0) = New cls_event_listener(Me, 1, "Aktivace panelu nástroje.")

        'CTRL Q na uzavírání otevřených tagů
        event_listeners(1) = New cls_event_listener(Me, 2, "Uzavírání otevřených tagů.")
        closing_tag_insertion_ = New cls_keyevent_args(Keys.Q, True)
        event_listeners(1).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, closing_tag_insertion_, Nothing), 0)

        'event_listeners(2) = New cls_event_listener(Me, 2, "Tag <div>")
        'event_listeners(2).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, New cls_keyevent_args(Keys.D, True), Nothing), 0)
        tags(0) = New cls_tag_setting(Me, "div", False, True, New cls_keyevent_args(Keys.D, True))
        'tags(0).event_listener = event_listeners(2)

        'event_listeners(3) = New cls_event_listener(Me, 3, "Tag <p>")
        'event_listeners(3).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, New cls_keyevent_args(Keys.P, True), Nothing), 0)
        tags(1) = New cls_tag_setting(Me, "p", False, True, New cls_keyevent_args(Keys.P, True))
        'tags(1).event_listener = event_listeners(3)

        'event_listeners(4) = New cls_event_listener(Me, 4, "Tag <head>")
        'event_listeners(4).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, New cls_keyevent_args(Keys.H, True), Nothing), 0)
        tags(2) = New cls_tag_setting(Me, "head", False, False, New cls_keyevent_args(Keys.H, True))
        'tags(2).event_listener = event_listeners(4)

        'event_listeners(5) = New cls_event_listener(Me, 5, "Tag <note>")
        'event_listeners(5).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, New cls_keyevent_args(Keys.N, True), Nothing), 0)
        tags(3) = New cls_tag_setting(Me, "note", False, False, New cls_keyevent_args(Keys.N, True))
        'tags(3).event_listener = event_listeners(5)

        tags(4) = New cls_tag_setting(Me, "app", False, False, Nothing)

        'event_listeners(6) = New cls_event_listener(Me, 6, "Tag <lemma>")
        'event_listeners(6).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, New cls_keyevent_args(Keys.L, True), Nothing), 0)
        tags(5) = New cls_tag_setting(Me, "lemma", False, False, Nothing)
        'tags(5).event_listener = event_listeners(6)

    End Sub
    Public Sub raise(p As cls_preXML_section_page, e As Object, mode As Integer)
        Dim i As Long
        Dim cl As Boolean
        If e IsNot Nothing Then
            If e.GetType = GetType(cls_keyevent_args) Then
                'xyz = e.GetType
                If closing_tag_modification_key = "shift" Then
                    cl = e.shift
                ElseIf closing_tag_modification_key = "alt" Then
                    cl = e.alt
                ElseIf closing_tag_modification_key = "ctrl" Then
                    cl = e.ctrl
                End If
            End If
        End If
        If mode > 1000 Then
            For i = 0 To n_tags
                If Me.tags(i).event_listener IsNot Nothing Then
                    If Me.tags(i).event_listener.mode = mode Then 'našli jsme správný tag
                        tags(i).insert(p, cl)
                        p.check_xml(True)
                    End If
                End If
            Next
            env.wsp.display_page(Nothing, Nothing,,, 0)
        ElseIf mode = 2 Then 'zavírání otevřeného tagu
            p.context.get_context()
            If p.context.last_opened_element <> "" And p.context.inside_of_tag = "" Then
                Dim cltag As cls_tag_setting
                Dim el As String
                Dim page As Integer
                Dim index As Integer
                Dim first_char_after0b As Integer

                p.context.last_opened_element(page, index)

                cltag = close_last_opened(p, first_char_after0b)
                el = cltag.tag

                If page <> -1 And index > 0 Then
                    If page = p.m_index Then
                        el = ": " & Mid(p.plain_text, index, 50)
                    Else

                        el = " ze strany " & page + 1 & ": " & Mid(env.opened_document.page(page).plain_text, index, 50)
                    End If
                End If
                p.context.set_flying_tool(New cls_flyingtool(Me, "Byl uzavřen element " & el & "...", Nothing, Nothing, True))
                p.check_xml(True)
                p.force_SelStart = first_char_after0b
                env.wsp.display_page(Nothing, Nothing,,, 0)
            End If
        End If
    End Sub
    Private Function close_last_opened(p As cls_preXML_section_page, Optional ByRef first_char_after0b As Integer = 0) As cls_tag_setting
        If p.context.last_opened_element <> "" And p.context.inside_of_tag = "" Then
            Dim cltag As cls_tag_setting
            For i = 0 To n_tags
                If tags(i).tag = p.context.last_opened_element() Then
                    cltag = tags(i)
                End If
            Next


            If cltag Is Nothing Then
                cltag = New cls_tag_setting(p.context.last_opened_element, False, False)
            End If
            cltag.insert(p, True, first_char_after0b)

            Return cltag
        End If
    End Function
    Public Overrides Sub dispose_controls()

    End Sub

    Public Overrides Sub create_controls(container As Control, last_visualized_tool As Object)
        MyBase.clean_container(container, last_visualized_tool)
        Dim i As Long
        Dim btn As Button
        Dim txt As TextBox
        Dim lbl As Label
        For i = 0 To n_tags
            With NewCtrl(lbl, New Label, container)
                .Top = TpH() + 5
                .Left = 5
                .Text = "Tag: "
                lbl.AutoSize = True
            End With
            With NewCtrl(txt, New TextBox, container)
                .Top = T()
                .Left = LpW() + 5
                .Width = 150
                .Text = tags(i).tag
                .Tag = tags(i)
                AddHandler .TextChanged, AddressOf txt_tagname_changed
            End With
            With NewCtrl(btn, New Button, container)
                .Top = T()
                .Left = LpW() + 5
                .Width = 100
                .Text = "<" & tags(i).tag & ">"
                .Tag = tags(i)
                AddHandler .Click, AddressOf btn_opening_tag_click
            End With
            If tags(i).self_closing = False Then
                With NewCtrl(btn, New Button, container)
                    .Top = T()
                    .Left = LpW() + 5
                    .Width = 100
                    .Text = "</" & tags(i).tag & ">"
                    .Tag = tags(i)
                    AddHandler .Click, AddressOf btn_closing_tag_click
                End With
            End If
        Next
        With NewCtrl(btn, New Button, container)
            .Top = TpH() + 5
            .Left = L()
            .Width = 50
            .Text = "+"
            AddHandler .Click, AddressOf btn_add_tag_click
        End With
        container.Visible = True
    End Sub
    Private Sub btn_add_tag_click(sender As Object, e As EventArgs)
        Dim new_tag As String
        new_tag = InputBox("Zadejte nový tag (i s atributy, např.: hi rend='italic')", "Nový tag")
        If new_tag <> "" Then
            Dim new_attrs As String
            'new_attrs = InputBox("Zadejte atributy (v podobě @attr1='hodnota' @attr2='hodnota')", "Nový tag - atributy")
            Dim new_self_closing As Boolean = (MsgBox("Je nový tag samozavírací?", MsgBoxStyle.YesNo, "Nový tag") = MsgBoxResult.Yes)
            Dim new_isolate As Boolean = (MsgBox("Izolovat nový tag na řádce?", MsgBoxStyle.YesNo, "Nový tag") = MsgBoxResult.Yes)
            Dim new_shortcut As String = InputBox("Zadejte kl. zkratku, např. CTRL+A")
            Me.n_tags += 1
            ReDim Preserve Me.tags(Me.n_tags)
            Dim t As String
            Dim new_attrs_arr() As String = Split(new_attrs, "@")
            Me.tags(Me.n_tags) = New cls_tag_setting(Me, new_tag, new_self_closing, new_isolate, New cls_keyevent_args(new_shortcut), Nothing)
        End If
    End Sub
    Private Sub btn_opening_tag_click(sender As Object, e As EventArgs)

    End Sub
    Private Sub btn_closing_tag_click(sender As Object, e As EventArgs)

    End Sub
    Private Sub txt_tagname_changed(sender As Object, e As EventArgs)

    End Sub
    Public Overrides Sub run(pp As cls_preXML_section_page, mode As Integer)

    End Sub

    Public Overrides Function clone() As Object
        Dim tmp As cls_tool_Tags_Insertion
        MyBase.clone_base(tmp)

        Return tmp
    End Function
    Public Overrides Function generate_context_menu(p As cls_preXML_section_page, cmn As cls_context_menu) As Object
        If p.context.inside_of_tag = "" Then
            If p.context.last_opened_element() <> "" Then
                cmn.add_tool_cm(env.c("Vkládání tagů"), Me, env.c("Uzavřít element") & " <" & p.context.last_opened_element & ">", 3, "<close>", True)
            End If
            If p.context.last_opened_element = "div" Or p.context.last_opened_element = "" Then
                cmn.add_tool_cm(env.c("Vkládání tagů"), Me, env.c("Vložit tag") & " <div>", 3, "div", False)
            End If
            If p.context.last_opened_element = "div" Then
                cmn.add_tool_cm(env.c("Vkládání tagů"), Me, env.c("Vložit tag") & " <head>", 3, "head", False)
            End If
            If p.context.opened_element("p") = False And p.context.opened_element("head") = False Then
                cmn.add_tool_cm(env.c("Vkládání tagů"), Me, env.c("Vložit tag") & " <p>", 3, "p", False)
            End If

        End If
    End Function
    Private Sub close_all_up_to(p As cls_preXML_section_page, n_tags_to_close1b As Integer, close_last_too As Boolean, Optional pos0b As Integer = -1)
        Dim i As Long, j As Long
        If pos0b <> -1 Then p.plain_text_selection_changed(pos0b + 1, 0)
        Dim first_to_close As Integer
        If close_last_too = True Then 'spočítáme si, který je první (resp. poslední - prostě nejvyšší v hierarchii) tag, který zavřeme
            first_to_close = p.context.n_tags_opened - (n_tags_to_close1b)
        Else
            first_to_close = p.context.n_tags_opened - (n_tags_to_close1b - 1)
        End If
        If first_to_close < 0 Then first_to_close = 0
        Dim pos_after_closed As Integer
        For i = p.context.n_tags_opened To first_to_close Step -1
            'zavíráme samozřejmě od zadu, od naposledy otevřených
            Me.close_last_opened(p, pos_after_closed)
            p.plain_text_selection_changed(pos_after_closed, 0)
        Next
    End Sub
    Private Sub close_all_up_to(p As cls_preXML_section_page, upto As String, close_this_element_too As Boolean, Optional pos0b As Integer = -1)
        Dim i As Long, j As Long
        If pos0b <> -1 Then p.plain_text_selection_changed(pos0b + 1, 0)
        Dim atribut As String, value As String, value_set As Boolean, type As String, rm As Match, nselstart As Integer
        If Left(upto, 1) = "@" Then
            type = "attr"
            atribut = rgx_g(upto, "^@([^ ><=!]+)")
            value = rgx_g(upto, "^@[""']([^""']*)",, rm)
            If rm.Captures.Count > 0 Then value_set = True
        End If
        For i = p.context.n_tags_opened To 0 Step -1
            If type = "attr" Then
                If (value_set = False And p.context.tags_opened(i).has_attribute(atribut) = True) Or
                    (value_set = True And p.context.tags_opened(i).has_attribute_with_value(atribut, value) = True) Then
                    'až pocaď musíme zavřít všechny otevřené
                    If close_this_element_too = False Then i += 1
                    For j = i To p.context.n_tags_opened
                        Me.close_last_opened(p, nselstart)
                        p.plain_text_selection_changed(nselstart, 0)
                    Next
                End If
            End If
        Next

    End Sub
    Public Overrides Function raise_function(fname As String, p As cls_preXML_section_page, suppress_triggering As Boolean, ParamArray params() As Object) As Object
        If fname = "close_all_up_to" Then
            If params IsNot Nothing Then
                If UBound(params) >= 0 Then
                    Try
                        Dim upto As String = params(0)
                        Dim this_too As Boolean
                        Dim pos0b As Integer
                        If UBound(params) >= 1 Then this_too = CBool(params(1)) Else this_too = True
                        If UBound(params) >= 2 Then pos0b = CInt(params(2)) Else pos0b = -1
                        close_all_up_to(p, upto, this_too, pos0b)

                    Catch ex As Exception
                        Return False
                    End Try
                End If
            End If
        ElseIf fname = "close_all_up_to_NR" Then
            If params IsNot Nothing Then
                If UBound(params) >= 0 Then
                    Try
                        Dim upto As Integer = params(0)
                        Dim this_too As Boolean
                        Dim pos0b As Integer
                        If UBound(params) >= 1 Then this_too = CBool(params(1)) Else this_too = True
                        If UBound(params) >= 2 Then pos0b = CInt(params(2)) Else pos0b = -1
                        close_all_up_to(p, upto, this_too, pos0b)

                    Catch ex As Exception
                        Return False
                    End Try
                End If
            End If
        End If
    End Function
    Public Overrides Function has_function(fname As String) As Boolean
        If fname = "close_all_up_to" Then
            Return True
        ElseIf fname = "close_all_up_to_NR" Then
            Return True

        End If
    End Function

    Public Overrides Function context_menu_activated(p As cls_preXML_section_page, p1 As Object, p2 As Object)
        If p1 = "<close>" Then
            Dim cltag As cls_tag_setting
            If cltag Is Nothing Then
                cltag = New cls_tag_setting(p.context.last_opened_element, False, False)
            End If
            cltag.insert(p, True)
            p.force_SelStart = p.SelStart1b + Len(cltag.tag) + 3
        Else
            For i = 0 To n_tags
                If tags(i).tag = p1 Then
                    If p2 Is Nothing Then p2 = False
                    Me.tags(i).insert(p, p2)
                    Return 0
                End If
            Next
        End If
    End Function

End Class

'#################################################################################################################################################################
'#################################################################################################################################################################
Public Class cls_tool_MarkSelection
    Inherits cls_tool
    Public lbl_mark As Label
    Public txt_mark As TextBox
    Public lbl_description As Label
    Public cmd_mark As Button
    Public remove_lb_tags As Boolean

    Public event_listeners(0) As cls_event_listener
    Private shortcut_ As cls_keyevent_args

    Public Property shortcut() As cls_keyevent_args
        Get
            Return shortcut_
        End Get
        Set(value As cls_keyevent_args)
            If value IsNot Nothing Then
                If event_listeners(0) IsNot Nothing Then
                    event_listeners(0).remove_all_connections()
                    event_listeners(0).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, value, Nothing), 0)
                Else
                    event_listeners(0) = New cls_event_listener(Me, 0, "Spustí událost označení textu značkou")
                    event_listeners(0).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, value, Nothing), 0)
                End If
                shortcut_ = value
            End If
        End Set
    End Property

    Public Sub New(name_id_ As String, name_ As String, description_ As String, mark_ As String, remove_lb_tags_ As Boolean, hgl_ As cls_highligh_rule,
                        Optional kshortcut As cls_keyevent_args = Nothing)
        Me.mark = mark_
        Me.name = name_
        Me.description = description_
        Me.name_id = name_id_
        Me.hgl(0) = hgl_
        Me.remove_lb_tags = remove_lb_tags_
        env.wsp.marks.add_mark(mark, Me.hgl(0))
        event_listeners(0) = New cls_event_listener(Me, 1, "Spustí nástroj (označí vybraný text značkou)")
        shortcut = kshortcut
    End Sub
    Public Sub New()
        'klonovací
        event_listeners(0) = New cls_event_listener(Me, 1, "Spustí nástroj (označí vybraný text značkou)")
    End Sub
    Public Sub New(name_ As String, name_id_ As String, mark_ As String, description_ As String, mark1_hgl As cls_highligh_rule)
        MyBase.set_basics(name_, name_id_, mark_, description_, mark1_hgl)
    End Sub
    Public Sub New(n As Xml.XmlNode)
        __xml(Nothing, n, False)
    End Sub
    Public Overrides Function export_to_xml(x As Xml.XmlDocument) As Xml.XmlNode
        Return __xml(x, Nothing, True)

    End Function
    Private Function __xml(x As Xml.XmlDocument, n_imp As Xml.XmlNode, export As Boolean) As Xml.XmlNode
        Dim n As Xml.XmlNode
        Dim i As Long
        If export = True Then
            n = x.CreateNode(Xml.XmlNodeType.Element, "tool", "")
            MyBase.export_base_to_xml(n, x)
        Else
            MyBase.import_base_from_xml(n_imp)
        End If

        If export = True Then
            n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "remove_lb_tags", "")).InnerText = remove_lb_tags
            If shortcut_ IsNot Nothing Then n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "shortcut", "")).AppendChild(shortcut_.export_to_xml(x))
        Else
            remove_lb_tags = CBool(get_singlenode_value(n_imp, "remove_lb_tags"))
            n = n_imp.SelectSingleNode("shortcut/key_event_args")
            If n IsNot Nothing Then Me.shortcut = New cls_keyevent_args(n)
        End If
        If export = True Then Return n
    End Function
    Public Overrides Sub dispose_controls()
        lbl_description = Nothing
        lbl_mark = Nothing
        txt_mark = Nothing
        cmd_mark = Nothing
    End Sub

    Public Overrides Sub create_controls(container As Control, last_visualized_tool As Object)
        MyBase.clean_container(container, last_visualized_tool)
        With NewCtrl(lbl_mark, New Label)
            .Parent = container
            .Top = 5
            .Left = 10
            lbl_mark.AutoSize = True
            .Text = "Značka: "
        End With
        With NewCtrl(txt_mark, New TextBox)
            .Parent = container
            .Top = lastctrl.Top
            .Left = LpW() + 5
            .Text = Me.mark
            .Width = 150
            AddHandler .TextChanged, AddressOf txt_mark_text_changed
        End With
        Dim chb As CheckBox
        With NewCtrl(chb, New CheckBox, container)
            .Left = 5
            .Top = TpH() + 5
            .Text = "Odstranit v označeném prostoru tagy <lb/>/<l>"
            chb.Checked = Me.remove_lb_tags
            chb.AutoSize = True
            AddHandler chb.CheckedChanged, AddressOf rtb_remove_lb_checked_changed
        End With
        With NewCtrl(cmd_mark, New Button)
            .Parent = container
            .Top = lastctrl.Top + lastctrl.Height + 5
            .Left = 5
            .Text = "Označ"
            .Width = 150
            AddHandler .Click, AddressOf cmd_mark_click
        End With

        With NewCtrl(lbl_description, New Label)
            .Parent = container
            .AutoSize = True
            .Top = lastctrl.Top + lastctrl.Height + 10
            .Left = 10
            .Text = description
        End With

        container.Visible = True
    End Sub
    Private Sub txt_mark_text_changed(sender As Object, e As EventArgs)
        Me.mark = sender.text
    End Sub
    Private Sub rtb_remove_lb_checked_changed(sender As Object, e As EventArgs)
        Me.remove_lb_tags = sender.checked
    End Sub
    Public Sub cmd_mark_click(sender As Object, e As EventArgs)
        run(env._p, -1)
    End Sub

    Public Sub raise(p As cls_preXML_section_page, e As Object, mode As Integer)
        run(p, mode)
    End Sub
    Public Overrides Sub run(pp As cls_preXML_section_page, mode As Integer)
        pp.save_state()
        'xyz = Mid(env.wsp.rtb.Text, 1)
        'xyz = Mid(pp.plain_text, 1)
        Dim i As Integer
        Dim sel_end As Integer

        sel_end = pp.SelStart0b + pp.SelLength - 1
        'xyz = Mid(pp.plain_text, pp.SelStart1b, 1)
        If mark <> "CLEANUP" Then
            For i = pp.SelStart1b To pp.SelStart1b + pp.SelLength - 1
                pp.add_char_metadata_value(mark, i - 1)
            Next
        Else
            'naopak všechny značky odstraníme
            For i = pp.SelStart1b To pp.SelStart1b + pp.SelLength - 1
                Erase pp.meta_data(i - 1)
            Next
            remove_lb_tags = False
        End If

        If remove_lb_tags = True Then
            'odstraníme tagy zlomu řádků
            Dim mc As MatchCollection
            Dim rx As New Regex("<lb[\s/][^\>]*>")
            mc = rx.Matches(pp.plain_text, CInt(pp.SelStart0b))

            For i = mc.Count - 1 To 0 Step -1 'odzadu, aby nám fungovaly indexy...
                If mc(i).Index < sel_end Then 'pokud jsme v označeném prostoru
                    pp.insert_on_position(mc(i).Index + mc(i).Length + 1, vbLf, "<<<")
                    pp.delete_text_on_position(mc(i).Index, mc(i).Index + mc(i).Length) 'smažeme...
                End If
            Next
        End If
        If Me.parent.GetType = GetType(cls_tools_organizer) Then
            If Me.parent.n_tt = -1 Then
                env.wsp.display_page(Nothing, Split(Me.mark))
            Else
                Me.parent.execute(pp)
                env.wsp.display_page(Nothing, Split(Me.mark))
            End If
        End If

    End Sub

    Public Overrides Function clone() As Object
        Dim tmp As New cls_tool_MarkSelection
        clone_base(tmp)
        tmp.remove_lb_tags = Me.remove_lb_tags
        Return tmp
    End Function
    Public Overrides Function generate_context_menu(p As cls_preXML_section_page, cmn As cls_context_menu) As Object
        Return False
    End Function
    Public Overrides Function context_menu_activated(p As cls_preXML_section_page, p1 As Object, p2 As Object)

    End Function

End Class


'#################################################################################################################################################################
'#################################################################################################################################################################
Public Class cls_tool_Join_hyphenation
    Inherits cls_tool
    Public lbl_mark As Label
    Public txt_mark As TextBox
    Public lbl_description As Label
    Public cmd_mark As Button
    Public remove_lb_tags As Boolean

    Public event_listeners(0) As cls_event_listener
    Private shortcut_ As cls_keyevent_args

    Public Property shortcut() As cls_keyevent_args
        Get
            Return shortcut_
        End Get
        Set(value As cls_keyevent_args)
            If value IsNot Nothing Then
                If event_listeners(0) IsNot Nothing Then
                    event_listeners(0).remove_all_connections()
                    event_listeners(0).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, value, Nothing), 0)
                Else
                    event_listeners(0) = New cls_event_listener(Me, 0, "Spustí událost spojení slov rozdělených zlomem řádku")
                    event_listeners(0).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, value, Nothing), 0)
                End If
                shortcut_ = value
            End If
        End Set
    End Property

    Public Sub New(name_id_ As String, name_ As String, description_ As String, mark_ As String, remove_lb_tags_ As Boolean, hgl_ As cls_highligh_rule,
                        Optional kshortcut As cls_keyevent_args = Nothing)
        Me.mark = mark_
        Me.name = name_
        Me.description = description_
        Me.name_id = name_id_
        Me.hgl(0) = hgl_
        Me.remove_lb_tags = remove_lb_tags_
        env.wsp.marks.add_mark(mark, Me.hgl(0))
        event_listeners(0) = New cls_event_listener(Me, 1, "Spustí nástroj (spojí slova rozdělená přes zlom řádku)")
        shortcut = kshortcut
    End Sub
    Public Sub New()
        'klonovací
        event_listeners(0) = New cls_event_listener(Me, 1, "Spustí nástroj (spojí slova rozdělená přes zlom řádku)")
    End Sub
    Public Sub New(name_ As String, name_id_ As String, mark_ As String, description_ As String, mark1_hgl As cls_highligh_rule)
        MyBase.set_basics(name_, name_id_, mark_, description_, mark1_hgl)
    End Sub
    Public Sub New(n As Xml.XmlNode)
        __xml(Nothing, n, False)
    End Sub
    Public Overrides Function export_to_xml(x As Xml.XmlDocument) As Xml.XmlNode
        Return __xml(x, Nothing, True)

    End Function
    Private Function __xml(x As Xml.XmlDocument, n_imp As Xml.XmlNode, export As Boolean) As Xml.XmlNode
        Dim n As Xml.XmlNode
        Dim i As Long
        If export = True Then
            n = x.CreateNode(Xml.XmlNodeType.Element, "tool", "")
            MyBase.export_base_to_xml(n, x)
        Else
            MyBase.import_base_from_xml(n_imp)
        End If

        If export = True Then
            If shortcut_ IsNot Nothing Then n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "shortcut", "")).AppendChild(shortcut_.export_to_xml(x))
        Else
            n = n_imp.SelectSingleNode("shortcut/key_event_args")
            If n IsNot Nothing Then Me.shortcut = New cls_keyevent_args(n)
        End If
        If export = True Then Return n
    End Function
    Public Overrides Sub dispose_controls()
        lbl_description = Nothing
        lbl_mark = Nothing
        txt_mark = Nothing
        cmd_mark = Nothing
    End Sub

    Public Overrides Sub create_controls(container As Control, last_visualized_tool As Object)
        MyBase.clean_container(container, last_visualized_tool)
        With NewCtrl(lbl_mark, New Label)
            .Parent = container
            .Top = 5
            .Left = 10
            lbl_mark.AutoSize = True
            .Text = "Značka: "
        End With
        With NewCtrl(txt_mark, New TextBox)
            .Parent = container
            .Top = lastctrl.Top
            .Left = LpW() + 5
            .Text = Me.mark
            .Width = 150
            AddHandler .TextChanged, AddressOf txt_mark_text_changed
        End With
        Dim chb As CheckBox

        With NewCtrl(cmd_mark, New Button)
            .Parent = container
            .Top = lastctrl.Top + lastctrl.Height + 5
            .Left = 5
            .Text = "Spoj"
            .Width = 150
            AddHandler .Click, AddressOf cmd_join_click
        End With

        With NewCtrl(lbl_description, New Label)
            .Parent = container
            .AutoSize = True
            .Top = lastctrl.Top + lastctrl.Height + 10
            .Left = 10
            .Text = description
        End With

        container.Visible = True
    End Sub
    Private Sub txt_mark_text_changed(sender As Object, e As EventArgs)
        Me.mark = sender.text
    End Sub

    Public Sub cmd_join_click(sender As Object, e As EventArgs)
        run(env._p, -1)
    End Sub

    Public Sub raise(p As cls_preXML_section_page, e As Object, mode As Integer)
        run(p, mode)
    End Sub
    Public Overrides Sub run(pp As cls_preXML_section_page, mode As Integer)
        pp.save_state()
        Dim ac_word As String
        ac_word = pp.context.word
        Dim ac_line As Long = pp.line_from_char_index(pp.SelStart0b)
        Dim pos_on_line As Long = pp.SelStart0b - pp.line_start_index(ac_line)
        Dim last_space_on_line = InStrRev(RTrim(pp.lines(ac_line)), " ")
        last_space_on_line += pp.line_start_index(ac_line)
        If last_space_on_line < pp.SelStart0b Then 'jsme na konci řádku, takže najdeme první slovo na následujícím řádku
            Dim first_word_on_next As String
            Dim rv() As Point
            Dim rm As Match
            first_word_on_next = rgx_g(pp.line(ac_line + 1), "^\s*(?:<lb[^>]*>)?([^ \n]*)",, rm)
            If (rm IsNot Nothing) Then
                Dim hyphen_pos As Integer = InStr_first(pos_on_line, pp.line(ac_line), 0, 0, "-", ".", "_")
                If hyphen_pos > 0 Then
                    hyphen_pos += pp.line_start_index(ac_line)
                    
                    pp.delete_text_on_position(hyphen_pos, hyphen_pos)
                Else
                    hyphen_pos = pp.context.word_boundaries1b.Y
                End If
                xyz = pp.lines(ac_line + 1)
                pp.delete_text_on_position(pp.line_start_index(ac_line + 1) + rm.Groups(1).Index + 1, pp.line_start_index(ac_line + 1) + rm.Groups(1).Index + 1 + rm.Groups(1).Length)
                pp.insert_on_position(hyphen_pos, first_word_on_next, "")

                env.wsp.display_page(Nothing)
            End If
            'rv = pp.search(True, "\n\s*(?<lb[^>]*>)?([^ \n]*)", pp.line_end_index(ac_line),,, True)
        Else
            If pp.context.word_boundaries1b.Y > 0 Then
                'xyz = InStr(pp.context.word_boundaries1b.Y, pp.plain_text, "-")
                Dim start_deleting_at As Long = pp.context.word_boundaries1b.Y

                If InStr(pp.context.word_boundaries1b.Y, pp.plain_text, "-") = pp.context.word_boundaries1b.Y Then 'na konci slova je pomlčka
                    If InStr(pp.context.word_boundaries1b.Y + 1, pp.plain_text, " ") = pp.context.word_boundaries1b.Y + 1 Then
                        pp.delete_text_on_position(start_deleting_at, pp.context.word_boundaries1b.Y + 1)
                        env.wsp.display_page(Nothing)
                    End If
                End If
            End If
        End If
    End Sub

    Public Overrides Function clone() As Object
        Dim tmp As New cls_tool_MarkSelection
        clone_base(tmp)
        tmp.remove_lb_tags = Me.remove_lb_tags
        Return tmp
    End Function
    Public Overrides Function generate_context_menu(p As cls_preXML_section_page, cmn As cls_context_menu) As Object
        Return False
    End Function
    Public Overrides Function context_menu_activated(p As cls_preXML_section_page, p1 As Object, p2 As Object)

    End Function

End Class
Public Class cls_tool_Delete_rest_of_line
    Inherits cls_tool
    Public cmd_delete As Button
    Public cmd_delete2 As Button
    Public event_listeners(1) As cls_event_listener



    Public Sub New()
        'klonovací
        set_event_listeners()
    End Sub
    Public Sub New(name_ As String, name_id_ As String, mark_ As String, description_ As String, mark1_hgl As cls_highligh_rule)
        MyBase.set_basics(name_, name_id_, mark_, description_, mark1_hgl)
        env.wsp.marks.add_mark(mark, Me.hgl(0))
        event_listeners(0) = New cls_event_listener(Me, 1, "Spustí nástroj (odstraní zbytek řádky)")
        set_event_listeners()
    End Sub
    Public Sub New(n As Xml.XmlNode)
        __xml(Nothing, n, False)
        set_event_listeners()
    End Sub
    Public Sub set_event_listeners()
        event_listeners(0) = New cls_event_listener(Me, 0, "Spustí nástroj odstranění zbytku řádky")
        event_listeners(0).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, New cls_keyevent_args(Keys.F4), Nothing), 0)
        event_listeners(1) = New cls_event_listener(Me, 1, "Spustí nástroj odstranění počátku řádky")
        event_listeners(1).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, New cls_keyevent_args(Keys.F4, True), Nothing), 0)
    End Sub
    Public Overrides Function export_to_xml(x As Xml.XmlDocument) As Xml.XmlNode
        Return __xml(x, Nothing, True)

    End Function
    Private Function __xml(x As Xml.XmlDocument, n_imp As Xml.XmlNode, export As Boolean) As Xml.XmlNode
        Dim n As Xml.XmlNode
        Dim i As Long
        If export = True Then
            n = x.CreateNode(Xml.XmlNodeType.Element, "tool", "")
            MyBase.export_base_to_xml(n, x)
            Return n
        Else
            MyBase.import_base_from_xml(n_imp)
        End If
    End Function
    Public Overrides Sub dispose_controls()
        cmd_delete = Nothing
        cmd_delete2 = Nothing
    End Sub

    Public Overrides Sub create_controls(container As Control, last_visualized_tool As Object)
        MyBase.clean_container(container, last_visualized_tool)
        With NewCtrl(cmd_delete, New Button)
            .Parent = container
            .Top = 5
            .Left = 10
            .Text = "Smaž do konce řádky"
            AddHandler .Click, AddressOf cmd_delete_click
        End With
        With NewCtrl(cmd_delete2, New Button)
            .Parent = container
            .Top = 5
            .Left = 10
            .Text = "Smaž do začátku řádky"
            AddHandler .Click, AddressOf cmd_delete2_click
        End With

        container.Visible = True
    End Sub
    Private Sub cmd_delete_click(sender As Object, e As EventArgs)
        run(env._p, 0)
    End Sub
    Private Sub cmd_delete2_click(sender As Object, e As EventArgs)
        run(env._p, 1)
    End Sub
    Public Sub raise(p As cls_preXML_section_page, e As Object, mode As Integer)
        run(p, mode)
    End Sub
    Public Overrides Sub run(pp As cls_preXML_section_page, mode As Integer)
        pp.save_state()

        Dim ac_line As Long = pp.line_from_char_index(pp.SelStart0b)
        If mode = 0 Then 'smazat do konce řádky
            pp.delete_text_on_position(pp.SelStart0b + 1, pp.line_end_index(ac_line))
        Else 'smazat do začátku řádky
            Dim i As Long, j As Long
            Dim lb_pos As Integer = InStr_first(pp.line_start_index(ac_line) + 1, pp.plain_text, i, j, "<lb/", "<lb ", "<l>", "<l ")
            If lb_pos > -1 Then
                Dim lb_end_pos As Integer = InStr(lb_pos, pp.plain_text, ">")
                If lb_end_pos > 0 Then pp.delete_text_on_position(lb_end_pos + 1, pp.SelStart0b)
            End If
        End If
        env.wsp.display_page(Nothing)
    End Sub

    Public Overrides Function clone() As Object
        Dim tmp As New cls_tool_MarkSelection
        clone_base(tmp)
        Return tmp
    End Function
    Public Overrides Function generate_context_menu(p As cls_preXML_section_page, cmn As cls_context_menu) As Object
        Return False
    End Function
    Public Overrides Function context_menu_activated(p As cls_preXML_section_page, p1 As Object, p2 As Object)

    End Function

End Class
Public Class cls_tool_Join_words
    Inherits cls_tool
    Public cmd_join As Button
    Public cmd_join2 As Button
    Public event_listeners(2) As cls_event_listener

    Public Sub New()
        'klonovací
        set_event_listeners()
    End Sub
    Public Sub New(name_ As String, name_id_ As String, mark_ As String, description_ As String, mark1_hgl As cls_highligh_rule)
        MyBase.set_basics(name_, name_id_, mark_, description_, mark1_hgl)
        env.wsp.marks.add_mark(mark, Me.hgl(0))
        event_listeners(0) = New cls_event_listener(Me, 1, "Spustí nástroj (spojení slov)")
        set_event_listeners()
    End Sub
    Public Sub New(n As Xml.XmlNode)
        __xml(Nothing, n, False)
        set_event_listeners()
    End Sub
    Public Sub set_event_listeners()
        event_listeners(0) = New cls_event_listener(Me, 0, "Spustí nástroj spojení slov (dopředu)")
        event_listeners(0).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, New cls_keyevent_args(Keys.F5), Nothing), 0)
        event_listeners(1) = New cls_event_listener(Me, 1, "Skočí na další slovo")
        event_listeners(1).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, New cls_keyevent_args(Keys.F5,, True), Nothing), 0)
        event_listeners(2) = New cls_event_listener(Me, 2, "Spustí nástroj spojení slov (dozadu)")
        event_listeners(2).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, New cls_keyevent_args(Keys.F5, True), Nothing), 0)
    End Sub
    Public Overrides Function export_to_xml(x As Xml.XmlDocument) As Xml.XmlNode
        Return __xml(x, Nothing, True)

    End Function
    Private Function __xml(x As Xml.XmlDocument, n_imp As Xml.XmlNode, export As Boolean) As Xml.XmlNode
        Dim n As Xml.XmlNode
        Dim i As Long
        If export = True Then
            n = x.CreateNode(Xml.XmlNodeType.Element, "tool", "")
            MyBase.export_base_to_xml(n, x)
            Return n
        Else
            MyBase.import_base_from_xml(n_imp)
        End If
    End Function
    Public Overrides Sub dispose_controls()
        cmd_join = Nothing
        cmd_join2 = Nothing
    End Sub

    Public Overrides Sub create_controls(container As Control, last_visualized_tool As Object)
        MyBase.clean_container(container, last_visualized_tool)
        With NewCtrl(cmd_join, New Button)
            .Parent = container
            .Top = 5
            .Left = 10
            .Text = "Spoj >"
            AddHandler .Click, AddressOf cmd_join_click
        End With
        With NewCtrl(cmd_join2, New Button)
            .Parent = container
            .Top = 5
            .Left = 10
            .Text = "< Spoj"
            AddHandler .Click, AddressOf cmd_join2_click
        End With

        container.Visible = True
    End Sub
    Private Sub cmd_join_click(sender As Object, e As EventArgs)
        run(env._p, 0)
    End Sub
    Private Sub cmd_join2_click(sender As Object, e As EventArgs)
        run(env._p, 1)
    End Sub
    Public Sub raise(p As cls_preXML_section_page, e As Object, mode As Integer)
        run(p, mode)
    End Sub
    Public Overrides Sub run(pp As cls_preXML_section_page, mode As Integer)
        pp.save_state()
        If pp.SelStart1b < 1 Then Exit Sub
        Dim ac_line As Long = pp.line_from_char_index(pp.SelStart0b)
        If mode = 0 Then 'směrem ke konci řádku
            Dim line As Integer = pp.line_from_char_index(pp.SelStart0b)
            If pp.SelLength = 0 Or pp.SelLength =-1 Then
                Dim next_space As Integer = InStr(pp.SelStart1b, pp.plain_text, " ")
                If (next_space < pp.line_end_index(line)) Then
                    pp.delete_text_on_position(next_space, next_space)
                End If
            Else
                pp.search_and_replace(False, " ", "", "",, pp.SelStart1b,, pp.SelLength)
            End If
        ElseIf mode = 1 Then 'skočí na další slovo
                Dim next_space As Integer = InStr(pp.SelStart1b + 1, pp.plain_text, " ")
                pp.force_SelStart = next_space
                env.wsp.display_page(Nothing)
            ElseIf mode = 2 Then

            End If
        env.wsp.display_page(Nothing)
    End Sub

    Public Overrides Function clone() As Object
        Dim tmp As New cls_tool_MarkSelection
        clone_base(tmp)
        Return tmp
    End Function
    Public Overrides Function generate_context_menu(p As cls_preXML_section_page, cmn As cls_context_menu) As Object
        Return False
    End Function
    Public Overrides Function context_menu_activated(p As cls_preXML_section_page, p1 As Object, p2 As Object)

    End Function

End Class
Public Class cls_tool_PlainText_replacement
    Inherits cls_tool
    Public str_search_for As String
    Public str_replace As String
    Public replacing_allowed As Boolean
    Public replacing_forced As Boolean
    Public rgx As Boolean
    Public two_steps As Boolean 'jestli je nástroj dvojkrokový, tj. nejprve zvýrazní, a případně až pak nahradí
    Public only_in_selection As Boolean
    Public exclude_tags As Boolean

    'ovládací prvky pro nastavování nástroje
    Public lbl_name As Label
    Public lbl_description As Label
    Public txt_find As TextBox
    Public txt_replace As TextBox
    Public btn_run_mode0 As Button
    Public btn_run_mode1 As Button
    Public chb_regex As CheckBox
    Public btn_second_step As Button
    Private shortcut_ As cls_keyevent_args
    Public event_listeners(0) As cls_event_listener

    Public Property shortcut() As cls_keyevent_args
        Get
            Return shortcut_
        End Get
        Set(value As cls_keyevent_args)
            If value IsNot Nothing Then
                If event_listeners(0) IsNot Nothing Then
                    event_listeners(0).remove_all_connections()
                    event_listeners(0).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, value, Nothing), 0)
                Else
                    event_listeners(0) = New cls_event_listener(Me, 0, "Spustí událost označení textu značkou")
                    event_listeners(0).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, value, Nothing), 0)
                End If
                shortcut_ = value
            End If
        End Set
    End Property
    Public Overrides Function clone() As Object
        Dim tmp As cls_tool_PlainText_replacement
        tmp = New cls_tool_PlainText_replacement
        clone_base(tmp)

        tmp.str_search_for = Me.str_search_for
        tmp.str_replace = Me.str_replace
        tmp.replacing_allowed = Me.replacing_allowed
        tmp.replacing_forced = Me.replacing_forced
        tmp.rgx = Me.rgx
        tmp.two_steps = Me.two_steps
        Return tmp
    End Function

    Public Sub New()
        'pro případ klonování
        event_listeners(0) = New cls_event_listener(Me, 1, "Spustí nástroj (označí vybraný text značkou)")
    End Sub
    Public Sub New(name_ As String, name_id_ As String, mark_ As String, description_ As String, mark1_hgl As cls_highligh_rule)
        MyBase.set_basics(name_, name_id_, mark_, description_, mark1_hgl)
        event_listeners(0) = New cls_event_listener(Me, 1, "Spustí nástroj (označí vybraný text značkou)")
    End Sub
    Public Sub New(n As Xml.XmlNode)
        event_listeners(0) = New cls_event_listener(Me, 1, "Spustí nástroj (označí vybraný text značkou)")
        __xml(Nothing, n, False)
    End Sub
    Private Function __xml(x As Xml.XmlDocument, n_imp As Xml.XmlNode, export As Boolean) As Xml.XmlNode
        Dim n As Xml.XmlNode
        Dim i As Long
        If export = True Then
            n = x.CreateNode(Xml.XmlNodeType.Element, "tool", "")
            MyBase.export_base_to_xml(n, x)
        Else
            MyBase.import_base_from_xml(n_imp)
        End If

        If export = True Then
            Dim tmpn As Xml.XmlNode
            tmpn = x.CreateNode(Xml.XmlNodeType.Element, "str_search_for", "")
            If str_search_for = " " Then
                tmpn.Attributes.Append(x.CreateAttribute("value")).Value = str_search_for
            End If
            n.AppendChild(tmpn).InnerText = str_search_for
        Else
            str_search_for = get_singlenode_value(n_imp, "str_search_for")
        End If
        If export = True Then
            n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "str_replace", "")).InnerText = str_replace
        Else
            str_replace = get_singlenode_value(n_imp, "str_replace")
        End If
        If export = True Then
            n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "replacing_allowed", "")).InnerText = replacing_allowed
        Else
            replacing_allowed = CBool(get_singlenode_value(n_imp, "replacing_allowed"))
        End If
        If export = True Then
            n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "replacing_forced", "")).InnerText = replacing_forced
        Else
            replacing_forced = CBool(get_singlenode_value(n_imp, "replacing_forced"))
        End If
        If export = True Then
            n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "rgx", "")).InnerText = Me.rgx
        Else
            Me.rgx = CBool(get_singlenode_value(n_imp, "rgx"))
        End If
        If export = True Then
            n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "two_steps", "")).InnerText = two_steps
        Else
            Me.two_steps = CBool(get_singlenode_value(n_imp, "two_steps"))
        End If
        If export = True Then
            If shortcut_ IsNot Nothing Then n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "shortcut", "")).AppendChild(shortcut_.export_to_xml(x))
        Else
            n = n_imp.SelectSingleNode("shortcut/key_event_args")
            If n IsNot Nothing Then Me.shortcut = New cls_keyevent_args(n)
        End If
        If export = True Then
            n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "only_in_selection", "")).InnerText = only_in_selection
        Else
            only_in_selection = CBool(get_singlenode_value(n_imp, "only_in_selection", False))
        End If
        If export = True Then
            n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "exclude_tags", "")).InnerText = exclude_tags
        Else
            exclude_tags = CBool(get_singlenode_value(n_imp, "exclude_tags", False))
        End If
        If export = True Then Return n
    End Function

    Public Sub New(reg_ex As Boolean, text_to_search_for As String, text_to_replace As String, allow_replace As Boolean, force_replace As Boolean, name_id_ As String,
                        name_ As String, description_ As String, mark_ As String,
                        Optional auto_insert As Integer = 0, Optional def_mode_on_autorun As Integer = 0,
                        Optional type_ As String = "", Optional two_stp As Boolean = False,
                        Optional hgl1_ As cls_highligh_rule = Nothing)

        name_id = name_id_
        rgx = reg_ex
        str_replace = text_to_replace
        str_search_for = text_to_search_for
        replacing_allowed = allow_replace
        replacing_forced = force_replace
        name = name_
        mark = mark_
        description = description_
        type = type_
        two_steps = two_stp
        hgl(0) = hgl1_
        env.wsp.marks.add_mark(mark, hgl(0))
        event_listeners(0) = New cls_event_listener(Me, tm_mode.TM_EXECUTE, "Spustí nástroj (označí vybraný text značkou)")

    End Sub

    Public Sub raise(p As cls_preXML_section_page, e As Object, mode As Integer)
        run(p, mode)
    End Sub

    Public Overrides Sub run(pp As cls_preXML_section_page, ByVal mode As Integer)
        'mode0=jen najdi
        'mode1=najdi a nahraď
        pp.save_state()

        If mode = tm_mode.TM_ONLY_HIGHLIGHT Or Me.replacing_allowed = False Then
            If only_in_selection = False Then
                pp.search_and_highlight(rgx, str_search_for, mark)
            End If
        ElseIf mode = tm_mode.TM_EXECUTE Then
            If only_in_selection = False Then
                pp.search_and_replace(rgx, str_search_for, str_replace, mark, False,,,, exclude_tags)
            Else
                pp.search_and_replace(rgx, str_search_for, str_replace, mark, False, pp.SelStart0b,, pp.SelLength, exclude_tags)
            End If
        End If
        env.wsp.display_page(Nothing, Split(Me.mark))
    End Sub

    Public Overrides Sub dispose_controls()
        lbl_description = Nothing
        lbl_name = Nothing
        btn_run_mode0 = Nothing
        btn_run_mode1 = Nothing
        txt_find = Nothing
        txt_replace = Nothing
        chb_regex = Nothing
    End Sub
    Public Overrides Sub create_controls(container As Control, last_visualized_tool As Object)

        Me.clean_container(container, last_visualized_tool)

        lbl_name = New Label

        With lbl_name
            .Parent = container
            .Top = 0
            .Left = 0
            .Width = container.Width
            .AutoSize = False
            .AutoEllipsis = True
            .Text = Me.name

        End With
        txt_find = New TextBox
        With txt_find
            .Parent = container
            .Top = lbl_name.Top + lbl_name.Height + 5
            .Left = 5
            .Width = container.Width - 10
            .Height = 20
            .Text = Me.str_search_for
            AddHandler .TextChanged, AddressOf Me.txtbox_changed
        End With
        txt_replace = New TextBox
        With txt_replace
            .Parent = container
            .Top = txt_find.Top + txt_find.Height + 5
            .Left = 5
            .Width = container.Width - 10
            .Height = 20
            .Text = Me.str_replace
            AddHandler .TextChanged, AddressOf Me.txtbox_changed
        End With
        chb_regex = New CheckBox
        With chb_regex
            .Parent = container
            .Top = txt_replace.Top + txt_replace.Height + 5
            .Left = 5
            .AutoSize = True
            .Height = 20
            .Text = "Regulérní výraz"
            .Checked = Me.rgx
        End With
        btn_run_mode0 = New Button
        With btn_run_mode0
            .Parent = container
            .Top = chb_regex.Top + chb_regex.Height + 20
            .Width = container.Width / 2
            .Left = (container.Width / 2) - 5
            .Text = "Spusť (a jen zvýrazni)"
            AddHandler .Click, AddressOf Me.cmd_run0_clicked
        End With
        btn_run_mode1 = NewCtrl(New Button)
        With btn_run_mode1
            .Parent = container
            .Top = btn_run_mode0.Top + btn_run_mode0.Height + 20
            .Width = container.Width / 2
            .Left = (container.Width / 2) - 5
            .Text = "Spusť a nahraď"
            .Height = 30
            AddHandler .Click, AddressOf Me.cmd_run1_clicked
        End With
        If two_steps = True Then
            btn_second_step = NewCtrl(New Button)
            With btn_second_step
                .Top = lastctrl.Top + lastctrl.Height + 25
                .Left = 5
                .Width = container.Width - 10
                .Parent = container
                .Height = 45
                .Text = "Nahraď označené"
                AddHandler .Click, AddressOf Me.cmd_run2_clicked
            End With
        End If
        lbl_description = NewCtrl(New Label)
        With lbl_description
            .Parent = container
            .Top = lastctrl.Top + lastctrl.Height + 10
            .Left = 0
            .Width = container.Width
            .Height = container.Height - .Top
            If .Height < 60 Then .Height = 60
            .AutoSize = False
            .AutoEllipsis = True
            .Text = Me.description
        End With

        container.Visible = True
    End Sub
    Public Sub txtbox_changed(sender As Object, e As EventArgs)
        If sender.name = "txt_find" Then
            str_search_for = sender.text
        Else
            str_replace = sender.text
        End If
    End Sub
    Public Sub cmd_run0_clicked(sender As Object, e As EventArgs)
        If env._p IsNot Nothing Then
            run(env._p, tm_mode.TM_ONLY_HIGHLIGHT)
            env.wsp.display_page(Split(Me.mark))
        End If
    End Sub
    Public Sub cmd_run1_clicked(sender As Object, e As EventArgs)
        If env.wsp.p IsNot Nothing Then
            run(env._p, tm_mode.TM_EXECUTE)
            env.wsp.display_page(Split(Me.mark))
        End If
    End Sub
    Public Sub cmd_run2_clicked(sender As Object, e As EventArgs)
        If env._p IsNot Nothing Then
            run(env._p, tm_mode.TM_REPLACE_MARKED)
            env.wsp.display_page(Split(Me.mark))
        End If
    End Sub
    Public Overrides Function generate_context_menu(p As cls_preXML_section_page, cmn As cls_context_menu) As Object
        Return False
    End Function
    Public Overrides Function context_menu_activated(p As cls_preXML_section_page, p1 As Object, p2 As Object)

    End Function
    Public Overrides Function export_to_xml(x As Xml.XmlDocument) As Xml.XmlNode
        Return __xml(x, Nothing, True)
    End Function

End Class
'####################################################################################'####################################################################################
Public Class cls_tool_Notes
    'velenástroj pro práci s poznámkami - jejich identifikaci v textu, rozdělování, identifikaci příslunšných kotev a přemisťování na kotvy
    Inherits cls_tool
    Public splitting_pattern As String 'maska pro rozdělení jednotlivých poznámek od sebe
    Public working_context_mark As String 'značka, v jíž označeném textě budeme precovat
    Public tag As String
    Public cmd_run As Button

    Public transformation_pattern As String
    Public no_luck_transformation_pattern As String
    Public binding_tag As String
    Public inner_pattern As String
    Public default_anchor As String

    Public placing_mode As Long
    Public placing_mode1_wrap_prev As Boolean
    Public placing_range As String '""=aktuální stránka, jinak se bude hledat předcházející zadaný tag (např. div[@type="dipl_app_regest"]) a umisťování se bude provádět v něm

    Public automatic_transformation As Boolean
    Public automatic_placing As Boolean



    Public event_listeners() As cls_event_listener

    Private Structure tag_capture_index
        Public tag As String
        Public index As Integer
    End Structure
    Public Sub New()
        'klonovací
    End Sub

    Public Sub New(name_id_ As String, name_ As String, description_ As String, mark_ As String, working_context_ As String, splitting_pattern_ As String,
                        tag_ As String, inner_pattern_ As String, inner_transformation_pattern_ As String, inner_transformation_pattern_2_ As String,
                        binding_tag_ As String, Optional automatic_transformation_ As Boolean = True, Optional automatic_placing_ As Boolean = True,
                        Optional default_anchor_ As String = "*")
        name_id = name_id_
        name = name_
        description = description_
        mark = mark_
        working_context_mark = working_context_
        splitting_pattern = splitting_pattern_
        tag = tag_
        inner_pattern = inner_pattern_
        transformation_pattern = inner_transformation_pattern_
        no_luck_transformation_pattern = inner_transformation_pattern_2_
        binding_tag = binding_tag_
        default_anchor = default_anchor_
        automatic_placing = automatic_placing_
        automatic_transformation = automatic_transformation_
        placing_range = ""

        create_event_listeners()
    End Sub
    Public Sub New(name_ As String, name_id_ As String, mark_ As String, description_ As String, mark1_hgl As cls_highligh_rule)
        MyBase.set_basics(name_, name_id_, mark_, description_, mark1_hgl)
        create_event_listeners()
    End Sub
    Public Overrides Function export_to_xml(x As Xml.XmlDocument) As Xml.XmlNode
        Dim n As Xml.XmlNode
        n = x.CreateNode(Xml.XmlNodeType.Element, "tool", "")
        MyBase.export_base_to_xml(n, x)
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "tag", "")).InnerText = tag
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "splitting_pattern", "")).InnerText = splitting_pattern
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "transformation_pattern", "")).InnerText = transformation_pattern
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "no_luck_transformation_pattern", "")).InnerText = no_luck_transformation_pattern
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "binding_tag", "")).InnerText = binding_tag
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "inner_pattern", "")).InnerText = inner_pattern
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "default_anchor", "")).InnerText = default_anchor
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "automatic_transformation", "")).InnerText = automatic_transformation
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "automatic_placing", "")).InnerText = automatic_placing
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "working_context_mark", "")).InnerText = working_context_mark
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "placing_mode", "")).InnerText = placing_mode
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "placing_mode1_wrap_prev", "")).InnerText = placing_mode1_wrap_prev
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "placing_range", "")).InnerText = placing_range
        Return n
    End Function
    Public Sub New(n As Xml.XmlNode)
        If n IsNot Nothing Then
            MyBase.import_base_from_xml(n)
            tag = get_singlenode_value(n, "tag")
            splitting_pattern = get_singlenode_value(n, "splitting_pattern")
            transformation_pattern = get_singlenode_value(n, "transformation_pattern")
            no_luck_transformation_pattern = get_singlenode_value(n, "no_luck_transformation_pattern")
            binding_tag = get_singlenode_value(n, "binding_tag")
            inner_pattern = get_singlenode_value(n, "inner_pattern")
            default_anchor = get_singlenode_value(n, "default_anchor")
            automatic_placing = get_singlenode_value(n, "automatic_placing")
            automatic_transformation = get_singlenode_value(n, "automatic_transformation")
            working_context_mark = get_singlenode_value(n, "working_context_mark")
            placing_mode1_wrap_prev = get_singlenode_value(n, "placing_mode1_wrap_prev", "True")
            placing_mode = get_singlenode_value(n, "placing_mode", "0")
            placing_range = get_singlenode_value(n, "placing_range")
            create_event_listeners()
        End If
    End Sub

    Private Function create_event_listeners()
        ReDim event_listeners(4)
        event_listeners(0) = New cls_event_listener(Me, 10, "Manuální rozdělování poznámek")
        event_listeners(0).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, New cls_keyevent_args(Keys.Enter,, True), Nothing), 0)
        event_listeners(1) = New cls_event_listener(Me, 100, "(Polo)automatická transformace poznámek")
        event_listeners(1).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, New cls_keyevent_args(Keys.Space,, True), Nothing), 0)
        event_listeners(2) = New cls_event_listener(Me, 1000, "Manuální umisťování poznámek")
        event_listeners(2).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, New cls_keyevent_args(Keys.Enter, True, True), Nothing), 0)
        event_listeners(3) = New cls_event_listener(Me, 101, "Transformace poznámek")
        event_listeners(3).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, New cls_keyevent_args(Keys.Space, True, True), Nothing), 0)
        event_listeners(4) = New cls_event_listener(Me, 102, "Automatické umístění poznámky")
        event_listeners(4).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, New cls_keyevent_args(Keys.Space, False, False, True), Nothing), 0)

    End Function
    Public Overrides Sub dispose_controls()
        cmd_run.Dispose()
        cmd_run = Nothing
    End Sub

    Public Overrides Sub create_controls(container As Control, last_visualized_tool As Object)
        Me.clean_container(container, last_visualized_tool)
        lastctrl = Nothing
        Dim txt As TextBox
        Dim lbl As Label
        Dim cmd As Button
        Dim chb As CheckBox
        With NewCtrl(lbl, New Label, container)
            .Top = +5
            .Left = 5
            .Text = "Hlavní obalující tag:"
            lbl.AutoSize = True
        End With
        With NewCtrl(txt, New TextBox, container)
            .Top = T()
            .Left = LpW() + 5
            .Width = 150
            .Text = tag
            .Tag = "tag"
            AddHandler .TextChanged, AddressOf txt_text_changed
        End With

        With NewCtrl(lbl, New Label, container)
            .Top = TpH() + 5
            .Left = 5
            .Text = "Maska pro rozdělení jednotlivých poznámek od sebe:"
            lbl.AutoSize = True
        End With
        With NewCtrl(txt, New TextBox, container)
            .Top = T()
            .Left = LpW() + 5
            .Width = 150
            .Text = splitting_pattern
            .Tag = "splitting_pattern"
            AddHandler .TextChanged, AddressOf txt_text_changed
        End With

        With NewCtrl(lbl, New Label, container)
            .Top = TpH() + 5
            .Left = 5
            .Text = "Maska vnitřní struktury původní podoby poznámky:"
            lbl.AutoSize = True
        End With
        With NewCtrl(txt, New TextBox, container)
            .Top = TpH() + 5
            .Left = L() + 10
            .Width = container.Width - 10 - .Left
            .Text = inner_pattern
            .Tag = "inner_pattern"
            AddHandler .TextChanged, AddressOf txt_text_changed
        End With

        With NewCtrl(lbl, New Label, container)
            .Top = TpH() + 5
            .Left = 5
            .Text = "Maska pro transformaci (pokud maska vnitřní podoby odpovídá):"
            lbl.AutoSize = True
        End With
        With NewCtrl(txt, New TextBox, container)
            .Top = TpH() + 5
            .Left = L() + 10
            .Width = container.Width - 10 - .Left
            .Text = transformation_pattern
            .Tag = "transformation_pattern"
            AddHandler .TextChanged, AddressOf txt_text_changed
        End With

        With NewCtrl(lbl, New Label, container)
            .Top = TpH() + 5
            .Left = 5
            .Text = "Maska pro transformaci (pokud maska vnitřní podoby neodpovídá):"
            lbl.AutoSize = True
        End With
        With NewCtrl(txt, New TextBox, container)
            .Top = TpH() + 5
            .Left = L() + 10
            .Width = container.Width - 10 - .Left
            .Text = no_luck_transformation_pattern
            .Tag = "no_luck_transformation_pattern"
            AddHandler .TextChanged, AddressOf txt_text_changed
        End With

        With NewCtrl(lbl, New Label, container)
            .Top = TpH() + 5
            .Left = 5
            .Text = "Tag pro navázání poznámky do textu:"
            lbl.AutoSize = True
        End With
        With NewCtrl(txt, New TextBox, container)
            .Top = T()
            .Left = LpW() + 5
            .Width = 150
            .Text = binding_tag
            .Tag = "binding_tag"
            AddHandler .TextChanged, AddressOf txt_text_changed
        End With
        With NewCtrl(lbl, New Label, container)
            .Top = TpH() + 5
            .Left = 5
            .Text = "Značka pracovní oblasti"
            lbl.AutoSize = True
        End With
        With NewCtrl(txt, New TextBox, container)
            .Top = T()
            .Left = LpW() + 5
            .Width = 150
            .Text = working_context_mark
            .Tag = "working_context_mark"
            AddHandler .TextChanged, AddressOf txt_text_changed
        End With


        With NewCtrl(lbl, New Label, container)
            .Top = TpH() + 5
            .Left = 5
            .Text = "Oblast poznámek (je-li prázdné, hledají se kotvy pro poznámky nebo poznámky na aktuální stránce, jinak v zadaném elementu"
            lbl.AutoSize = True
        End With
        With NewCtrl(txt, New TextBox, container)
            .Top = T()
            .Left = LpW() + 5
            .Width = 150
            .Text = placing_range
            .Tag = "placing_range"
            AddHandler .TextChanged, AddressOf txt_text_changed
        End With


        With NewCtrl(chb, New CheckBox, container)
            .Top = TpH() + 5
            .Left = 5
            .Text = "Automaticky transformovat poznámky"
            .Name = "auto_transform"
            chb.Checked = Me.automatic_transformation
            chb.AutoSize = True
            AddHandler chb.CheckedChanged, AddressOf chb_checked_changed
        End With
        With NewCtrl(chb, New CheckBox, container)
            .Top = TpH() + 5
            .Left = 5
            .Text = "Automaticky umisťovat poznámky"
            .Name = "auto_place"
            chb.Checked = Me.automatic_placing
            chb.AutoSize = True
            AddHandler chb.CheckedChanged, AddressOf chb_checked_changed
        End With

        With NewCtrl(cmd_run, New Button, container)
            .Left = 5
            .Top = TpH() + 5
            .Text = "Rozděl na jednotlivé poznámky"
            .Width = 350
            .Height = 25
            AddHandler .Click, AddressOf cmd_run_click
        End With

        With NewCtrl(cmd, New Button, container)
            .Left = 5
            .Top = TpH() + 5
            .Text = "Vlož zlom poznámky (</" & tag & "><" & tag & ">) na místo kurzoru"
            .Width = 350
            .Height = 25
            AddHandler .Click, AddressOf cmd_insert_note_break_click
        End With
        With NewCtrl(cmd, New Button, container)
            .Left = 5
            .Top = TpH() + 5
            .Text = "Umísti poznámku (v níž je kurzor) na její místo v textu (povede-li se to)"
            .Width = 350
            .Height = 25
            AddHandler .Click, AddressOf cmd_move_to_right_place_click
        End With
        Dim rbt As RadioButton
        With NewCtrl(rbt, New RadioButton, container)
            .Top = TpH() + 5
            .Left = 5
            .Text = "<lem> + <ln> (lemma a číslo řádku)"
            rbt.Checked = CBool(placing_mode = 0)
            rbt.AutoSize = True
            .Tag = 0
            AddHandler rbt.CheckedChanged, AddressOf rbt_placing_mode_click
        End With
        With NewCtrl(rbt, New RadioButton, container)
            .Top = TpH() + 5
            .Left = 5
            .Text = "<a> + ~sm (kotva a menší písmo)"
            rbt.Checked = CBool(placing_mode = 1)
            rbt.AutoSize = True
            .Tag = 1
            AddHandler rbt.CheckedChanged, AddressOf rbt_placing_mode_click
        End With
        With NewCtrl(rbt, New RadioButton, container)
            .Top = TpH() + 5
            .Left = 5
            .Text = "a) "
            rbt.Checked = CBool(placing_mode = 2)
            rbt.AutoSize = True
            .Tag = 2
            AddHandler rbt.CheckedChanged, AddressOf rbt_placing_mode_click
        End With
        'Dim chb As CheckBox
        With NewCtrl(chb, New CheckBox, container)
            .Top = TpH() + 5
            .Left = L() + 15
            .Text = "Ukotvit na předcházející slovo"
            chb.AutoSize = True
            chb.Checked = placing_mode1_wrap_prev
            AddHandler chb.CheckedChanged, AddressOf chb_placing_wrap_prev_word_changed
        End With

        lastctrl = Nothing

        container.Visible = True
    End Sub
    Private Sub chb_placing_wrap_prev_word_changed(sender As Object, e As EventArgs)
        placing_mode1_wrap_prev = sender.checked
    End Sub
    Private Sub rbt_placing_mode_click(sender As Object, e As EventArgs)
        If sender.checked = True Then Me.placing_mode = sender.tag
    End Sub
    Private Sub chb_checked_changed(sender As Object, e As EventArgs)
        If sender.name = "auto_transform" Then
            Me.automatic_transformation = sender.checked
        ElseIf sender.name = "auto_place" Then
            Me.automatic_placing = sender.checked
        End If
    End Sub

    Private Sub txt_text_changed(sender As Object, e As EventArgs)
        Dim is_rgx As Boolean = True
        Select Case sender.tag
            Case "tag"
                tag = sender.text
                is_rgx = False
            Case "inner_pattern"
                inner_pattern = sender.text
            Case "splitting_pattern"
                splitting_pattern = sender.text
            Case "transformation_pattern"
                transformation_pattern = sender.text
            Case "no_luck_transformation_pattern"
                no_luck_transformation_pattern = sender.text
            Case "binding_tag"
                binding_tag = sender.text
                is_rgx = False
            Case "working_context_mark"
                working_context_mark = sender.text
            Case "placing_range"
                placing_range = sender.text
        End Select
        If is_rgx = True Then check_RX_in_textbox(sender)
    End Sub
    Private Sub cmd_insert_note_break_click(sender As Object, e As EventArgs)
        'to samé co ALT+ENTER 

        If env._p.context.mark(working_context_mark) = True Then 'pokud jsme v našem kontextu...
            If env._p.context.inside_of_tag() = "" And env._p.context.flt Is Nothing And env._p.context.last_opened_element = tag Then
                env._p.save_state()
                'pokud nejsme uvnitř nějakého tagu a zároveň je posledním otevřeným tagem náš obalující tag...
                insert_tag_on_position(env._p, env._p.SelStart0b + 1, env._p.SelLength)
                env.wsp.display_page(Nothing, Split(mark & " " & Me.working_context_mark))
            End If
        End If
    End Sub
    Private Sub cmd_move_to_right_place_click(sender As Object, e As EventArgs)
        Dim note As String
        Dim indices As Point
        note = get_note_from_position(env._p.plain_text, env._p.SelStart0b, indices, 0, 0)
        If note <> "" Then
            env._p.save_state()
            move_to_right_place(env._p, note, indices.X - 1, indices.Y)
            env.wsp.display_page(Nothing)
        End If
    End Sub
    Public Sub raise(p As cls_preXML_section_page, e As Object, mode As Integer)
        Dim fi As Integer
        Dim li As Integer
        Dim flt_name As String
        env.wsp.tm = env.wsp.tm
        If p.context.flt IsNot Nothing Then If p.context.flt.tool IsNot Nothing Then flt_name = p.context.flt.tool.name_id
        If (p.context.mark(working_context_mark) = True) Or (flt_name = Me.name_id) Then
            If mode = 10 Then 'ALT+enter - zlom poznámky
                If p.context.mark(working_context_mark) = True Then 'pokud jsme v našem kontextu...
                    env._p.save_state()

                    If p.context.inside_of_tag() = "" And p.context.flt Is Nothing And p.context.opened_element(tag) = True Then
                        xyz = Mid(p.plain_text, p.SelStart1b - 10, 100)
                        'pokud nejsme uvnitř nějakého tagu a zároveň je posledním otevřeným tagem náš obalující tag...
                        insert_tag_on_position(p, p.SelStart0b + 1, p.SelLength, True) 'vložíme nový zlom poznámky (ale nespouštímě transformaci!)
                        Dim original_SelStart As Integer = p.SelStart0b
                        Dim closing_tags_to_insert As String()
                        Dim k As Long = -1
                        For i As Long = p.context.n_tags_opened To 0 Step -1
                            If p.context.tags_opened(i).name <> tag Then 'zjistíme, co zbylo po rozdělení poznámky otevřené, a zacelíme to
                                k += 1
                                ReDim Preserve closing_tags_to_insert(k)
                                closing_tags_to_insert(k) &= "</" & p.context.tags_opened(i).name & ">"
                            Else
                                xyz = Mid(p.plain_text, p.SelStart1b, 100)
                                p.insert_on_position(p.SelStart1b, Join(closing_tags_to_insert, ""), ">>>")
                                xyz = Mid(p.plain_text, p.SelStart1b, 100)
                                'zároveň je zřejmé, že na konci druhé nově vzniklé poznámky zůstaly některé zavírací tagy, kterým teď chybí otvírací... Ty tedy musíme odstranit
                                Dim ttd_pos As Integer
                                For j = 0 To closing_tags_to_insert.Count - 1
                                    ttd_pos = InStr(original_SelStart + Len(Join(closing_tags_to_insert, "")), p.plain_text, closing_tags_to_insert(j))
                                    xyz = Mid(p.plain_text, ttd_pos, 100)
                                    p.delete_text_on_position(ttd_pos, ttd_pos + Len(closing_tags_to_insert(j)) - 1)
                                Next j
                                If Me.automatic_transformation = True Then 'a teď transformujeme nově vzniklou poznámku
                                    Me.transform_note_on_position(p, ttd_pos)
                                End If

                                Exit For
                            End If
                        Next i
                        env.wsp.display_page(Nothing, Split(mark & " " & Me.working_context_mark))
                    ElseIf p.context.inside_of_tag_name = Me.tag And p.context.flt Is Nothing Then 'opak předchozího 
                        'poud jsme uvnitř tagu, zrušíme ho (zcelíme rozdělení dvou poznámek...)
                        Dim end_of_n1 As Integer, start_of_n2 As Integer
                        If InStr(1, p.context.inside_of_tag, "/") <> 0 Then
                            'jsme uvnitř zavíracího tagu...
                            end_of_n1 = InStrRev(p.plain_text, "<", p.SelStart1b)
                            start_of_n2 = InStr_first(end_of_n1 + 1, p.plain_text, 0, 0, "<" & Me.tag & " ", "<" & Me.tag & ">")
                        Else 'jsme uvnitř otvíracího tagu
                            start_of_n2 = InStrRev(p.plain_text, "<", p.SelStart1b)
                            end_of_n1 = InStrRev(p.plain_text, "</" & tag & ">", start_of_n2)
                        End If
                        If start_of_n2 > 0 And end_of_n1 > 0 Then
                            'pokud hledané značky existují 
                            Dim tmp1 As Integer, tmp2 As Integer
                            If p.is_marked(Me.working_context_mark, start_of_n2 - 1, InStrX(start_of_n2, p.plain_text, ">", tmp1)) And
                                    p.is_marked(Me.working_context_mark, end_of_n1, InStrX(end_of_n1, p.plain_text, ">", tmp2)) Then
                                'a pokud jsou stále v kontextu, který předpokládáme, prostě je odstraníme

                                p.plain_text_selection_changed(end_of_n1 - Len("</" & tag & ">") - 1, 0)
                                xyz = Mid(p.plain_text, end_of_n1 - Len("</" & tag & ">") - 1, 100)
                                Dim ac_in_n1 As String = ""
                                Dim ac_in_n2 As String = ""
                                Dim opened_in_n1 As String()
                                Dim noin1 As Long = -1
                                Dim opened_in_n2 As String()
                                Dim noin2 As Long = -1
                                Dim search_on_in_n1 As Integer = InStrRev(p.plain_text, "<", end_of_n1) - 2
                                Dim search_on_in_n2 As Integer = InStr(start_of_n2 + 1, p.plain_text, ">") + 1

                                If Mid(p.plain_text, end_of_n1 - 1, 1) = ">" And Mid(p.plain_text, tmp1 + 1, 1) = "<" Then 'pokud před ukončovacím tagem první poznámky končí nějaký jiný tag
                                    'zjistíme si, co je zač
                                    Dim n1_prev_tag_start = InStrRev(p.plain_text, "</", end_of_n1 - 1)
                                    If n1_prev_tag_start > 0 Then
                                        Dim n1_prev_tag = Mid(p.plain_text, n1_prev_tag_start, end_of_n1 - n1_prev_tag_start)
                                        n1_prev_tag = rgx_g(n1_prev_tag, "^</([^ >]+)")
                                        If n1_prev_tag <> "" Then 'nějaký tag tam je - zjistíme, jestli jím začíná i druhá poznámka     
                                            Dim n2_next_tag_end = InStr(tmp1 + 1, p.plain_text, ">")
                                            If n2_next_tag_end > 0 Then
                                                Dim n2_next_tag As String = Mid(p.plain_text, tmp1 + 1, n2_next_tag_end - (tmp1))
                                                n2_next_tag = rgx_g(n2_next_tag, "^<([^ />]+)")

                                                If n2_next_tag = n1_prev_tag Then 'jsou to stejné tag - odstraníme i ty
                                                    end_of_n1 = n1_prev_tag_start
                                                    tmp1 = n2_next_tag_end
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                                'xyz = Mid(p.plain_text, tmp2 + 1)
                                If Mid(p.plain_text, tmp2 + 1, 1) = vbLf Then tmp2 += 1 'pokud je za koncem první poznámky zlom řádku, odstraníme ho
                                p.delete_text_on_position(start_of_n2, tmp1)
                                p.delete_text_on_position(end_of_n1, tmp2)
                                env.wsp.display_page(Nothing, Split(mark & " " & Me.working_context_mark))
                            End If
                        End If

                    End If
                End If
            ElseIf mode = 100 Then 'alt+mezerník
                Dim si As Integer
                Dim ei As Integer
                Dim note As String
                Dim new_note As String
                Dim indices As Point
                If p.context.mark(working_context_mark) = True And p.context.last_opened_element = tag Then 'pokud jsme v našem kontextu...
                    env._p.save_state()
                    'mezerník+alt -> zkusíme (znovu) zpracovat poznámku
                    note = Me.get_note_from_position(p.plain_text, p.SelStart0b, indices, 0, 0, True)
                    'note = Mid(p.plain_text, si, ei - si)
                    new_note = Me.transform(note)
                    If new_note <> note Then

                        p.delete_text_on_position(indices.X, indices.Y - 1)
                        p.insert_on_position(indices.X, new_note, working_context_mark)
                        move_to_right_place(p, new_note, indices.X - 1, indices.Y + 1)
                        env.wsp.display_page(Nothing)
                    End If
                Else
                    'note = get_note_from_position(p.plain_text, p.SelStart0b, indices, 0, 0)
                    'move_to_right_place(p, note, indices.X - 1, indices.Y)
                    'env.wsp.generate_rtf(Nothing)
                End If
            ElseIf mode = 101 Then 'ctrl+alt+mezerník
                env._p.save_state()
                get_wcx_selection_boundaries(p, fi, li)
                Dim n As Integer
                Dim pos() As Point
                Dim notes() As String
                notes = get_all_notes(p.plain_text, fi, li, n,, pos)
                For i = n To 0 Step -1
                    notes(i) = transform(notes(i))
                    p.delete_text_on_position(pos(i).X, pos(i).Y)
                    p.insert_on_position(pos(i).X, notes(i), Me.working_context_mark)
                Next
                env.wsp.display_page(Nothing)
            ElseIf mode = 102 Then 'shift+mezerník - automatické umístění poznámky
                Dim note As String
                Dim indices As Point
                note = get_note_from_position(env._p.plain_text, env._p.SelStart0b, indices, 0, 0)
                If note <> "" Then
                    env._p.save_state()
                    move_to_right_place(env._p, note, indices.X - 1, indices.Y)
                    env.wsp.display_page(Nothing)
                End If
            ElseIf mode = 1000 Then 'ctrl+alt+enter

                If p.context.inside_of_tag() = tag Or p.context.opened_element(tag) = True And p.context.flt Is Nothing Then  'ALT+enter uvnitř tagu,
                    'anebo uvnitř elementu (pokud je element už rozčleněný nebo pokud jsme v nějakém jeho podelementu)
                    env._p.save_state()
                    Dim tmp_sni As Point
                    Dim text As String
                    get_wcx_boundaries(p, fi, li)
                    text = get_note_from_position(p.plain_text, p.SelStart0b, tmp_sni, 0, 0, True)
                    p.context.set_flying_tool(New cls_flyingtool(Me, "Označením a stiskem CTRL+ALT+ENTER umísti vybranou poznámku:" & vbLf & Left(text, 100), text, tmp_sni))
                    'If tag = "app" Then
                    text = rgx_g(text, "<" & binding_tag & "[^>]*>([^<]*)</" & binding_tag & ">")
                    If text <> "" Then
                        p.search_and_highlight(False, text)
                        env.wsp.display_page(Split(""), Split(working_context_mark))
                    End If
                    'End If


                ElseIf p.context.flt IsNot Nothing Then
                    If p.context.flt.tool.name_id = Me.name_id Then 'pohybujeme se s létajícím nástrojem někde jinde...
                        Dim ntext As String

                        ntext = Me.bind(p.selection, 1, Len(RTrim(p.selection)), p.context.flt.value)
                        If ntext <> "" Then
                            env._p.save_state()

                            If p.context.flt.value2.X > p.SelStart0b Then
                                'pokud oblast poznámek, odkud bereme umisťovanou poznámku, je po místě, kam ji umisťujeme
                                p.delete_text_on_position(p.context.flt.value2.X, p.context.flt.value2.Y) 'smažeme starou pozn. 
                                p.delete_text_on_position(p.SelStart0b + 1, p.SelStart0b + 1 + Len(RTrim(p.selection)) - 1) 'selstart je 0-based! p2 získáme tak, že k selstart+1 (0based!) připočteme délku výběru, čímž se ale dostaneme 
                                'o jedno místo za (na první místo za koncem výběru), takže ještě musíme 1 odečíst: je-li začátek (první mazaný index) na indexu 10 a délka je 2, je posledním mazaným indexem 11, ne 12
                                p.insert_on_position(p.SelStart0b + 1, ntext, "")
                            Else
                                p.delete_text_on_position(p.SelStart0b + 1, p.SelStart0b + 1 + Len(RTrim(p.selection)))
                                p.insert_on_position(p.SelStart0b + 1, ntext, "")
                                p.delete_text_on_position(p.context.flt.value2.X, p.context.flt.value2.Y) 'smažeme starou pozn. 
                            End If
                            'ntext = rgxr(p.context.flt.value, "(<lemma[^>]*>)([^<]*)(</lemma>)", "$1" & p.selection & "$3")

                            env.wsp.display_page(Nothing, Split(mark & " " & working_context_mark))
                            p.context.set_flying_tool(Nothing)
                        End If
                    End If
                End If
            Else
                split_all(p)
            End If
        End If
    End Sub
    Private Function cl() As String
        Return "</" & tag & ">"
    End Function
    Private Function op() As String
        Return "<" & tag & ">"
    End Function
    Public Function transform(text As String) As String
        Dim rx As Regex
        Dim mc As MatchCollection
        'outer_pattern = " ^\ s *[A-Z]?\s*([0-9]+)?\s*([^\]]+)\]([^\]]*)\s(F)\s*$"
        'outer_pattern = "^\s*[A-Z]?\s*(?<LN>[0-9]+)?\s*(?<LEMMA>[^\]]+)\](?<RDG>[^\]]*)\s(?<WIT>F)\s*$"
        rx = New Regex(inner_pattern)
        mc = rx.Matches(text)
        Dim newtxt As String
        If mc.Count <> 0 Then
            newtxt = transformation_pattern
            Dim mc2 As MatchCollection
            mc2 = rx.Matches(newtxt, "\$([A-Z]*)")

            Dim i As Integer
            For i = 0 To mc2.Count - 1
                newtxt = Replace(newtxt, "$" & mc2(i).Groups(1).Value, mc(0).Groups.Item(mc2(i).Groups(1).Value).Value)
            Next
        Else 'v případě, že nenajdeme potřebné součásti, sáhneme po záložním vzorci, který někam umístí všechen text poznámky
            Dim mod_pat As String
            mod_pat = Replace(no_luck_transformation_pattern, "$ALL", ".*")
            'ještě si ověříme, jestli poznámka, kterou chceme přeměnit, už není podle tohoto vzoru přeměněná...
            Dim opening_tag As String
            opening_tag = rgx(text, "^\s*(<" & tag & "[^>]*>)")
            text = rgxr(text, "^\s*(<" & tag & "[^>]*>)", "") 'odstraníme si případný obaalující tag na začátku
            text = rgxr(text, "</" & tag & ">\s*$", "") 'a na konci

            If rgxt(text, mod_pat) = False Then
                newtxt = Replace(no_luck_transformation_pattern, "$ALL", text)
            Else
                newtxt = text
            End If
            newtxt = Replace(newtxt, "<" & tag & ">", opening_tag)
        End If
        newtxt = Replace(newtxt, Chr(10), " ") 'odstraníme zlomy řádky uvnitř poznámek
        newtxt = Replace(newtxt, Chr(13), " ")
        newtxt = Replace(newtxt, "  ", " ")
        Return newtxt
    End Function
    Private Function transform_note_on_position(p As cls_preXML_section_page, pos As Long)
        Dim note As String
        Dim indices As Point
        note = Me.get_note_from_position(p.plain_text, pos, indices, 0, 0)
        Dim transformed_note = Me.transform(note)
        p.delete_text_on_position(indices.X, indices.Y)
        p.insert_on_position(indices.X, transformed_note, ">>>|<<<")
    End Function

    Public Function bind(text_to_bind_to As String, pos_1b As Integer, length As Integer, note As String) As String
        Dim wrapped_text As String
        wrapped_text = Mid(text_to_bind_to, pos_1b, length)
        If wrapped_text = "" Then wrapped_text = default_anchor

        If rgxt(wrapped_text, ".+b[0-9a-z'*]\)\s*$") = True Then 'pokud je na konci vybraného textu něco jako "1)" nebo "a)", smažeme to - nejspíš jde o původní odkaz
            Dim note_nr As String
            wrapped_text = rgxr(wrapped_text, "\b([0-9a-z]\))\s*$", "")
        End If


        If rgxt(note, "<" & binding_tag & "[^>]*>[^<]*</" & binding_tag & ">") = False And binding_tag <> "" Then
            'pokud v poznámce vázací element chybí, doplníme ho
            note = rgxr(note, "^(\s*<[^>]+>)", "$1<" & binding_tag & "></" & binding_tag & ">")
        End If
        bind = rgxr(note, "<" & binding_tag & "[^>]*>[^<]*</" & binding_tag & ">", "<" & binding_tag & ">" & wrapped_text & "</" & binding_tag & ">")
        Return bind 'rgxr(note, "<" & bind_pattern & "[^>]*>[^<]*</" & bind_pattern & ">", "<" & bind_pattern & "[^>]*>" & wrapped_text & "</" & bind_pattern & ">")
    End Function
    Private Sub insert_tag_on_position(p As cls_preXML_section_page, pos_1b As Integer, length As Integer, Optional surpress_automatic_transformation As Boolean = False)
        Dim fi As Integer
        Dim li As Integer
        get_wcx_boundaries(p, fi, li)
        If fi <> -1 Then
            Dim preceding As Integer
            Dim preceding_pos As Integer
            Dim nextt As Integer
            Dim next_pos As Integer
            'xyz = Mid(p.plain_text, pos, 100)
            InStr_first(pos_1b, p.plain_text, next_pos, nextt, "</" & tag & ">", "<" & tag & ">")
            InStrRev_first(pos_1b, p.plain_text, preceding_pos, preceding, "</" & tag & ">", "<" & tag & ">")
            If next_pos = -1 Or next_pos > li Then 'do konce kontextu není žádný tag->umístíme koncový tag na konec kontextu
                p.insert_on_position(li + 1, "</" & tag & ">", working_context_mark)
            Else
                If nextt = 1 Then 'následuje nějaký otvírací tag, -> před něj umístíme zavírací
                    p.insert_on_position(next_pos - 1, "</" & tag & ">" & vbLf, working_context_mark)
                Else 'následuje zavírací tag, tj. jsme uprostřed nějaké poznámky, tzn. na místě pos ji rozdělíme, za to místo nevkládáme už nic
                    'p.insert_on_position(pos, op(), working_context_mark)
                End If
            End If
            If length = 0 Then
                p.insert_on_position(pos_1b, cl() & vbLf & op(), working_context_mark)
            ElseIf length <= 3 Then
                p.delete_text_on_position(pos_1b, pos_1b + length)
                p.insert_on_position(pos_1b, cl() & vbLf & op(), working_context_mark)
            End If

            If preceding_pos = -1 Or preceding_pos < fi Then 'od začátku kontextu ještě žádný tag není
                p.insert_on_position(fi + 1, op, working_context_mark)
            Else
                If preceding = 1 Then 'předchází otvírací tag, tj. tam nic nevkládáme
                    '
                Else 'předchází zavírací tag->na tom místě otevřeme 
                    p.insert_on_position(preceding_pos + Len(cl) + 1, vbLf & op(), working_context_mark)
                End If
            End If


        End If
        If Me.automatic_transformation = True And surpress_automatic_transformation = False Then split_single_notes(p)
    End Sub
    Public Overloads Sub run()
        split_all(env._p)
    End Sub
    Public Overrides Sub run(pp As cls_preXML_section_page, mode As Integer)
        split_all(pp)
    End Sub
    Private Function get_wcx_selection_boundaries(p As cls_preXML_section_page, ByRef fi As Integer, ByRef li As Integer) As Boolean
        Dim i As Long
        Dim index_to_check As Long = p.SelStart0b + (p.SelLength / 2)
        If (p.SelLength <= 1) Then index_to_check = p.SelStart0b
        If index_to_check >= p.meta_data.Length Then
            Return False
        End If
        If p.meta_data(index_to_check) IsNot Nothing Then

            If p.meta_data(index_to_check).Contains(Me.working_context_mark) = True Then
                li = index_to_check
                For i = index_to_check To UBound(p.meta_data)
                    If p.meta_data(i) IsNot Nothing Then
                        If p.meta_data(i).Contains(working_context_mark) = False Then
                            li = i - 1
                            Exit For
                        Else
                            li = i
                        End If
                    Else
                        li = i - 1
                        Exit For
                    End If
                Next
                fi = index_to_check
                For i = index_to_check To 0 Step -1
                    If p.meta_data(i) IsNot Nothing Then
                        If p.meta_data(i).Contains(working_context_mark) = False Then
                            fi = i + 1
                            Exit For
                        Else
                            fi = i
                        End If
                    Else
                        fi = i + 1
                        Exit For
                    End If
                Next
                Return True
            Else
                Return False
            End If
        End If
    End Function
    Private Function get_wcx_boundaries(p As cls_preXML_section_page, ByRef fi As Integer, ByRef li As Integer) As Boolean
        Dim i As Integer
        fi = -1
        For i = 0 To Len(p.plain_text) - 1
            If p.meta_data(i) IsNot Nothing Then
                If p.meta_data(i).Contains(working_context_mark) Then
                    fi = i
                    Exit For
                End If
            End If
        Next
        If fi <> -1 Then
            For i = Len(p.plain_text) - 1 To fi Step -1
                If p.meta_data(i) IsNot Nothing Then
                    If p.meta_data(i).Contains(working_context_mark) = True Then
                        li = i
                        Exit For
                    End If
                End If
            Next
            If i = Len(p.plain_text) Then
                li = Len(p.plain_text)
            End If
        End If
    End Function
    Private Function get_note_from_position(text As String, pos As Integer, ByRef sn_indices As Point, fi As Integer, li As Integer, Optional get_tags_too As Boolean = True) As String
        Dim si As Integer
        Dim ei As Integer
        Dim si2 As Integer
        si2 = InStr(pos, text, ">")

        If si2 <> 0 Then 'musíme se posunout na nejbližší zavírací závorku, protože pokud bychom zrovna byly v tom tagu, ta funkce InStrRev... by ho nenašla (kus hledaného řetězce by
            'končil až za zadanou pozicí)
            si = InStrRev_first(si2, text, si, 0, "<" & tag & ">", "<" & tag & " ")
        End If
        ei = InStrRev(text, "</" & tag & ">", pos) 'nejdřív zjistíme, jestli jsme vůbec uvnitř poznámky (tj. jestli předchází vůbec nějaký tag elementu, a pokud ano,
        'pak je-li poslední otevřený tag otvírací
        xyz = Mid(text, pos, 100)
        If si > 0 Then
            If si > ei Then
                'sn_indices.X = si

                si2 = InStr_first(pos + 1, text, si2, 0, "<" & tag & ">", "<" & tag & " ")
                ei = InStr(pos, text, "</" & tag & ">")
                If ei <> 0 And (ei < si2 Or si2 <= 0) Then 'za pozicí následuje dříve zavírací tag než otvírací - >jsme uvnitř poznámky...
                    If get_tags_too = True Then
                        sn_indices.X = si
                        sn_indices.Y = ei + Len(cl)
                    Else
                        sn_indices.X = InStr(sn_indices.X, text, ">")
                        sn_indices.Y = ei
                    End If
                    Return Mid(text, sn_indices.X, sn_indices.Y - sn_indices.X) 'tady vracíme správně nalezenou poznámku
                Else
                    sn_indices.X = 0
                    sn_indices.Y = 0
                    Return ""
                End If
            Else 'nejsme uvnitř elementu
                sn_indices.X = 0
                sn_indices.Y = 0
                Return ""
            End If
        Else
            sn_indices.X = 0
            sn_indices.Y = 0
            Return ""
        End If
    End Function
    Private Function get_all_notes(text As String, fi As Integer, li As Integer, ByRef n As Integer,
                                        Optional get_tags_too As Boolean = True, Optional ByRef pos() As Point = Nothing) As String()
        Dim i As Integer
        Dim si As Integer
        Dim nsi As Integer
        Dim ei As Integer
        si = fi
        n = -1
        Dim tmp() As String
        Dim t As String
        Do While InStr_first(si, text, si, 0, "<" & tag & " ", "<" & tag & ">") <> -1
            nsi = InStr_first(si + 1, text, nsi, 0, "<" & tag & " ", "<" & tag & ">")
            ei = InStr(si, text, "</" & tag & ">")
            'xyz = Mid(text, si, 100)
            If ei <> 0 And (ei < nsi Or nsi = -1) Then
                If get_tags_too = False Then
                    si = InStr(si, text, ">") + 1
                Else
                    ei = InStr(ei, text, ">")
                End If
                t = Mid(text, si, ei + 1 - si)
                n += 1
                ReDim Preserve tmp(n)
                ReDim Preserve pos(n)
                tmp(n) = t
                pos(n).X = si
                pos(n).Y = ei
                si = ei
            Else
                Exit Do
            End If
        Loop
        Return tmp
    End Function
    Private Function get_single_note(text As String, ByRef start As Integer, ByRef sn_indices As Point, fi As Integer, li As Integer, Optional get_tags_too As Boolean = False) As String
        Dim si As Integer
        Dim ei As Integer
        'Dim tmp As String
        Dim note As String
        If start > li And li <> 0 Then Return "" 'dostali jsme se mimo zadaný kontext
        If get_tags_too = False Then
            si = start + Len("<" & tag & ">")
            ei = InStr(CInt(start), text, "</" & rgx(tag, "^([^ ])*") & ">")
        Else
            si = start
            ei = InStr(CInt(start), text, "</" & rgx(tag, "^([^ ])*") & ">") + Len("</" & rgx(tag, "^([^ ])*") & ">")
        End If
        note = Mid(text, si, ei - si)
        note = Replace(note, "<lb/>", " ")
        note = Replace(note, "  ", " ")
        'note = Replace(note, vbLf, " ")
        sn_indices.X = si
        sn_indices.Y = ei '+ Len("</" & tag & ">")
        Return note
    End Function
    Private Function split_single_notes(p As cls_preXML_section_page)
        'projde jednotlivé elementy a pokusí se je rozdělit na jejich složky a případně, když se to povede, je přestaví do nové podoby 
        If inner_pattern = "" Then Exit Function
        Dim fi As Integer
        Dim li As Integer
        Dim i As Integer
        Dim i2 As Integer
        Dim i3 As Integer
        Dim si As Integer
        Dim j As Integer
        If get_wcx_selection_boundaries(p, fi, li) = False Then Exit Function
        i3 = fi
        Dim tmp As String
        tmp = p.plain_text
        Dim note As String
        Dim single_items() As String
        Dim single_items_indices() As Point
        Dim tmp_sni As Point
        Dim n_si As Integer = -1
        Do While InStrX(i3 + 1, tmp, "<" & tag & ">", i) <> 0 Or InStrX(i3 + 1, tmp, "<" & tag & " ", i2) <> 0 'nejprve si vytaháme jednotlivé položky
            If i = 0 And i2 <> 0 Then i = i2
            i3 = i
            'If i > li Then Exit Do 'dostali jsme se mimo zadaný kontext
            'si = i + Len("<" & tag & ">")
            'j = InStr(CInt(i), tmp, "</" & tag & ">")
            'note = Mid(tmp, si, j - si)
            'note = Replace(note, "<lb/>", "")
            'note = Replace(note, vbLf, " ")

            note = get_single_note(tmp, i, tmp_sni, fi, li, True)
            If note <> "" Then
                n_si += 1
                ReDim Preserve single_items(n_si)
                ReDim Preserve single_items_indices(n_si)
                single_items(n_si) = note
                single_items_indices(n_si) = tmp_sni
                'single_items_indices(n_si).Y = j + Len("</" & tag & ">")
            Else
                Exit Do
            End If
        Loop
        Dim new_single_items() As String
        ReDim new_single_items(n_si)
        For i = n_si To 0 Step -1 'zase od zadu, jinak si budeme kurvit indexy...
            Dim new_note As String
            new_note = transform(single_items(i))



            p.search_and_replace(False, single_items(i), new_note, working_context_mark, False, fi)

            If Me.automatic_placing = True Then move_to_right_place(p, new_note, fi, li)
            'p.insert_on_position(single_items_indices(i).X, new_note, "") 'vložíme novou

        Next
        p.search_and_replace(True, "^\n\s*$", "", "", True)
        p.force_SelLength = 0
        env.wsp.display_page(Nothing, Split(working_context_mark))
    End Function
    Private Function move_to_right_place(p As cls_preXML_section_page, note As String, fi As Integer, li As Integer, Optional mode As Long = 1)
        Dim i As Long
        If Me.placing_mode = 0 Then 'pomocí lemmatu a čísla řádky...
            Dim ln As String
            Dim lemma As String
            If binding_tag <> "" Then
                lemma = rgx_g(note, "<" & binding_tag & "[^>]*>([^<]*)<")
            Else
                lemma = ""
            End If
            ln = rgx_g(note, "<[^>]+\sln='([0-9]+)'")
            If (ln = "") Then
                ln = rgx_g(note, "<[^>]+\sa\s*=\s*'([0-9]+)\)?'") 'kotva může také být schovaná pod atributem a
            End If
            If ln <> "" Then 'podařilo se identifikovat číslo řádky...
                Dim line As String
                Dim l_index As Integer
                l_index = find_line_in_p(p, ln)
                If l_index <> -1 Then
                    line = p.line(l_index)
                Else
                    Exit Function
                End If
                Dim w_index As Integer
                If Trim(lemma) <> "" Then
                    w_index = InStr(1, line, lemma)
                    If w_index <> 0 Then 'podařilo se nalézt řádku (v číslování v <lb>)
                        If InStr(CInt(w_index + 1), line, lemma) = 0 Then 'a hledané lemma se na ní nachází jen jednou
                            'přesuneme poznámku na místo, kam patří
                            'xyz = Mid(p.plain_text, p.line_start_index(l_index))
                            'xyz = Mid(p.line(l_index), w_index, 100)
                            'xyz = Mid(p.plain_text, p.line_start_index(l_index) + w_index)
                            Dim b As String
                            b = bind(p.plain_text, p.line_start_index(l_index) + w_index, Len(lemma), note)
                            p.search_and_replace(False, note, "",,, fi)
                            p.delete_text_on_position(p.line_start_index(l_index) + w_index, p.line_start_index(l_index) + w_index + Len(lemma))
                            p.insert_on_position(p.line_start_index(l_index) + w_index, note, "")
                            Return True
                        Else

                        End If

                    End If
                Else
                    p.search_and_replace(False, note, "",,, fi)
                    Dim lb_pos As Long
                    lb_pos = InStr_first(1, line, lb_pos, 0, "<lb/>", "<lb ")
                    Dim lb_end As Long
                    If lb_pos > 0 Then
                        lb_end = InStr(CInt(lb_pos), line, ">")
                    End If
                    p.insert_on_position(p.line_start_index(l_index) + lb_end + 1, note, "")
                    Return True
                End If
            End If
        ElseIf Me.placing_mode = 1 Then 'pomocí písmena/čísla poznámky a jiného formátování (menší písmo)
            Dim anch As String
            anch = rgx_g(note, "<lem>([^<]*)</lem>")
            If anch = "" Then Exit Function
            Dim res() As Point
            Dim true_res() As Point
            Dim ntr As Integer = -1
            res = p.search(True, "[\.\s\n](" & anch & ")[\.\s\n]",,,, True)

            If res IsNot Nothing Then
                For i = 0 To UBound(res)
                    If p.is_marked("sm", res(i).X, res(i).Y) = True Then
                        ntr += 1
                        ReDim Preserve true_res(ntr)
                        true_res(ntr) = res(i)
                    End If
                Next
                If ntr > 0 Then 'když je víc než jeden možných výsledků, jenom je zvýrazníme...
                    For i = 0 To ntr
                        p.add_metadata_to_section("~search", true_res(i).X, true_res(i).Y, True)
                    Next
                ElseIf ntr = 0 Then 'je jenom jeden hledaný znak s požadovaným formátem
                    If placing_mode1_wrap_prev = False Then
                        note = Me.bind(p.plain_text, true_res(0).X + 1, true_res(0).Y - true_res(0).X, note)
                        p.search_and_replace(False, note, "",,, fi)
                        xyz = Mid(p.plain_text, true_res(0).X + 1, 100)
                        p.delete_text_on_position(true_res(0).X + 1, true_res(0).Y + 1)
                        p.insert_on_position(true_res(0).X + 1, note, "")
                        Return True
                    End If
                End If
            End If
        ElseIf Me.placing_mode = 2 Then 'pomocí atributu a, kde je výsledek rozdělovací masky, tedy asi To, jak je poznámka označena v textu (např. 1), a) apod.)
            Dim a As String
            Dim ac_p As cls_preXML_section_page
            If Trim(placing_range) = "" Then ac_p = p
            a = rgx_g(note, "<[^ ]* a\s*=\s*'([^']*)") 'atribut "a" (anchor) v tagu poznámky říká, jaké je její písmeno/číslo v textu
            Dim m As Match
            Dim mc As MatchCollection
            Dim rx As New Regex("[\n\s,.:]" & Replace(Replace(Replace(a, "+", "\+"), "*", "\*"), ")", "\)") & "[^a-zA-Z0-9]")

            Dim start_p As Long
            Dim start_p_start_index As Long
            Dim end_p As Long
            Dim end_p_end_index As Long
            If Trim(placing_range) = "" Then 'pracujeme jen s aktuální stránkou
                start_p = p.m_index_
                start_p_start_index = 0
                end_p = start_p
                end_p_end_index = -1
            Else 'pracujeme v otevřeném XML elementu
                Dim tag As String
                Dim attribute As String
                Dim attr_value As String
                'placing_range = "div[@type='dipl_app']"
                tag = rgx(placing_range, "^[^ \[]+")
                attribute = rgx_g(placing_range, "\[@([^\] =]+)")
                attr_value = rgx_g(placing_range, "=\s*['""]([^'""]+)")
                start_p = -1
                For i = p.context.n_tags_opened To 0 Step -1
                    If p.context.tags_opened(i).name = tag Then
                        If attribute = "" Or
                            (attr_value = "" And p.context.tags_opened(i).has_attribute(attribute) = True) Or
                            (attr_value <> "" And p.context.tags_opened(i).has_attribute_with_value(attribute, attr_value) = True) Then

                            start_p = p.context.tags_opened(i).position.X 'tady je uložena strana
                            start_p_start_index = p.context.tags_opened(i).position.Y 'a tady pozice na té straně
                            If p.context.tags_opened(i).second_to_pair IsNot Nothing Then 'pokud už je požadovaný element uzavřen...
                                end_p = p.context.tags_opened(i).second_to_pair.position.X
                                end_p_end_index = p.context.tags_opened(i).second_to_pair.position.Y
                            Else 'pokud ne, skončíme na stránce s poznámkami u jejich začátku
                                end_p = p.m_index_
                                end_p_end_index = fi
                            End If
                            Exit For
                        End If
                    End If
                Next
                If start_p = -1 Then 'nepovedlo se najít požadovaný element, neděláme nic...
                    Return False
                    Exit Function
                End If
            End If

            Dim ac_start_index As Long
            Dim ac_end_index As Long
            For i = start_p To end_p
                ac_p = p.parent_d.page(i)
                If Trim(placing_range) = "" Then
                    ac_start_index = 1
                    ac_end_index = -1
                Else
                    If i = start_p Then 'jsme na stránce, kde začíná element
                        ac_start_index = start_p_start_index 'tamhldáme až od počátku požadovaného elementu
                    Else
                        ac_start_index = 1 'jinde hledáme už od začátku
                    End If
                    If i = end_p Then
                        ac_end_index = end_p_end_index 'a jsme-li na poslední stránce, hledáme do konce elementu
                    Else
                        ac_end_index = -1 'jinak do konce stránky
                    End If
                End If
                xyz = Mid(p.plain_text, fi, 100)

                mc = rx.Matches(ac_p.plain_text, CInt(ac_start_index))
                If mc.Count = 1 Then
                    For j = 0 To mc.Count - 1

                        If (mc.Item(0).Index < ac_end_index Or ac_end_index = -1 And p.safe_metadata(mc.Item(0).Index).Contains(Me.working_context_mark) = False) Then 'pokud jsme našli jeden výskyt a ten je v naší požadované oblasti

                            If Me.placing_mode1_wrap_prev = True Then
                                Dim prev_w_start As Integer
                                prev_w_start = InStrRev_first(mc.Item(0).Index - 1, ac_p.plain_text, 0, 0, " ", vbLf, ">") + 1
                                If prev_w_start > 0 Then
                                    Dim prev_w = Trim(Mid(ac_p.plain_text, prev_w_start, mc.Item(0).Index - prev_w_start + 1))
                                    ac_p.delete_text_on_position(mc.Item(0).Index + 1, mc.Item(0).Index + 1 + Len(a))
                                    Dim bound = bind(ac_p.plain_text, prev_w_start, mc.Item(0).Index - prev_w_start + 1, note)
                                    ac_p.delete_text_on_position(prev_w_start, mc.Item(0).Index)
                                    ac_p.insert_on_position(prev_w_start, bound, "")
                                    p.search_and_replace(False, note, "",,, fi) 'toto děláme ne na stránce, kam vkládáme poznámku, ale tam, odkud ji bereme!!!
                                    Return True
                                End If

                            Else
                                Dim bound = bind(ac_p.plain_text, mc.Item(0).Index + 2, Len(a), note)
                                ac_p.delete_text_on_position(mc.Item(0).Index + 2, mc.Item(0).Index + 2 + Len(a))
                                ac_p.insert_on_position(mc.Item(0).Index + 2, bound & " ", "")
                                p.search_and_replace(False, note, "",,, fi) 'toto děláme ne na stránce, kam vkládáme poznámku, ale tam, odkud ji bereme!!!
                                Return True
                            End If
                        End If
                    Next j
                    Exit For 'na další stránku už nepokračujeme
                End If
            Next i
        End If
    End Function
    Private Function find_line_in_p(p As cls_preXML_section_page, nl As Integer) As Integer
        'najde řádku očíslovanou pomocí tagu <lb/>
        Dim i As Integer
        Dim tmp_ln1 As Long = -1
        Dim tmp_ln2 As Long = -1
        For i = 0 To p.n_lines
            Dim n As String
            Dim m As Integer
            Dim j As Integer
            n = rgx_g(p.line(i), "<lb n\s*=\s*'([0-9]+)\s*'/>")
            If n <> "" Then
                m = CInt(n)

                If CInt(n) = nl Then
                    Return i
                ElseIf m < nl And tmp_ln1 = -1 And (nl - m < 5) Then
                    For j = i + 1 To p.n_lines
                        If rgxt(p.line(j), "<lb[\s/]") = True Then
                            m += 1
                        End If
                        If m = nl Then
                            tmp_ln1 = j
                            Exit For
                        End If
                    Next
                ElseIf CInt(n) > nl And tmp_ln2 = -1 Then

                    For j = i - 1 To 0 Step -1
                        If rgxt(p.line(j), "<lb[\s/]") = True Then
                            m -= 1
                        End If
                        If m = nl Then
                            tmp_ln2 = j
                            Exit For
                        End If
                    Next
                End If
            End If
            If tmp_ln1 = tmp_ln2 And tmp_ln1 <> -1 Then
                Return tmp_ln1
            End If
        Next
        If tmp_ln1 = -1 And tmp_ln2 <> -1 Then
            Return tmp_ln2
        ElseIf tmp_ln2 = -1 And tmp_ln1 <> -1 Then
            Return tmp_ln1
        ElseIf tmp_ln1 <> -1 Then
            Return tmp_ln1
        Else
            Return -1
        End If
    End Function
    Private Sub split_all(p As cls_preXML_section_page)
        Dim fi As Integer
        Dim li As Integer
        Dim i As Integer
        'xyz = Me.automatic_placing
        get_wcx_selection_boundaries(p, fi, li)
        'xyz = Mid(p.plain_text, fi , 10)
        'xyz = p.meta_data(li)
        If fi <> -1 Then 'pokud se náš pracovní kontext v textu nenachází, těžko budeme pracovat...
            If splitting_pattern <> "" Then

                Dim mc As MatchCollection
                Dim rx As Regex
                Try
                    rx = New Regex(splitting_pattern)
                Catch
                    MsgBox(env.c("Chyba v regulérním výrazu"))
                    Exit Sub
                End Try
                If (fi > 1) Then fi -= 1
                mc = rx.Matches(p.plain_text, CInt(fi))
                If mc IsNot Nothing Then
                    Dim prev_note_end As Integer
                    Dim shift As Integer
                    'xyz = Mid(p.plain_text, li, 100)
                    If p.plain_text(li) = vbLf Then
                        p.insert_on_position(li + 1, "</" & rgx(tag, "^([^ ])*") & ">" & vbLf, working_context_mark)
                    Else
                        p.insert_on_position(li + 2, "</" & rgx(tag, "^([^ ])*") & ">" & vbLf, working_context_mark)
                    End If
                    'xyz = Mid(p.plain_text, 261)

                    For i = mc.Count - 1 To 0 Step -1
                        If mc(i).Index < li Then
                            If mc(i).Captures.Count = 0 Then
                                p.insert_on_position(mc(i).Index, "</" & rgx(tag, "^([^ ])*") & ">" & vbLf & "<" & tag & ">", working_context_mark)
                            ElseIf False Then 'pokud je v masce nějaký zachycený řetězec, je tento řetězec místem zlom, tj. místem, které nepatří ani jedné poznámce
                                'xyz = Mid(p.plain_text, mc(i).Captures(0).Index, 100)
                                xyz = mc(i).ToString
                                p.insert_on_position(1 + mc(i).Captures(0).Index + mc(i).Captures(0).Length, "<" & tag & " a ='" &
                                                     Replace(Replace(Trim(mc(i).ToString), vbLf, ""), vbCr, "") & "'>", working_context_mark)
                                'xyz = Mid(p.plain_text, mc(i).Captures(0).Index, mc(i).Captures(0).Length)
                                p.delete_text_on_position(mc(i).Captures(0).Index + 1, mc(i).Captures(0).Length + mc(i).Captures(0).Index)
                                If i <> 0 Then
                                    p.insert_on_position(1 + mc(i).Captures(0).Index, "</" & tag & ">" & vbLf, working_context_mark)
                                Else
                                    p.insert_on_position(1 + mc(i).Captures(0).Index, vbLf & vbLf, "")
                                End If

                            Else
                                'xyz = mc(i).Captures(0).ToString
                                Dim group_value As String
                                If (mc(i).Groups.Count > 1) Then
                                    group_value = mc(i).Groups(1).ToString
                                Else
                                    group_value = mc(i).Captures(0).ToString
                                End If

                                xyz = Mid(p.plain_text, 1 + mc(i).Groups(0).Index + mc(i).Captures(0).Length, 100)
                                p.insert_on_position(1 + mc(i).Captures(0).Index + mc(i).Captures(0).Length, "<" & tag & " a ='" &
                                                     Replace(Replace(Trim(group_value), vbLf, ""), vbCr, "") & "'>", working_context_mark)

                                p.delete_text_on_position(mc(i).Captures(0).Index + 1, mc(i).Captures(0).Length + mc(i).Captures(0).Index)
                                If i <> 0 Then
                                    p.insert_on_position(1 + mc(i).Captures(0).Index, "</" & tag & ">" & vbLf, working_context_mark)
                                Else
                                    p.insert_on_position(1 + mc(i).Captures(0).Index, vbLf & vbLf, "")
                                End If
                            End If

                        End If
                    Next
                    'p.insert_on_position(fi + 1, vbLf & "<" & tag & ">", working_context_mark)
                    'p.search_and_replace(true, "<" & tag & "></" & tag & ">", "")
                End If
            Else
                If tag = "" Then 'není-li ani rozdělovací maska, ani obalovací div, transformujeme celou oblast pomocí transformační masky
                    Dim tags() As String
                    tags = str_to_tags(transformation_pattern)
                    If tags IsNot Nothing Then
                        Dim closing_tags As String = ""
                        For i = UBound(tags) To 0 Step -1
                            closing_tags &= "</" & rgx(tags(i), "^[^ />]+") & ">"
                        Next
                        If InStr(1, transformation_pattern, "\n") <> 0 Then closing_tags &= vbLf
                        If fi > 0 And li >= fi Then
                            p.remove_mark_on_position(Me.working_context_mark, fi, li)
                            p.insert_on_position(li + 2, closing_tags, "")
                            p.insert_on_position(fi + 1, Replace(transformation_pattern, "\n", vbLf), "")
                        ElseIf fi = 0 And li = 0 Then
                            p.insert_on_position(p.SelStart1b, Replace(transformation_pattern, "\n", vbLf), "")
                        End If

                    End If
                End If
            End If
            If Me.automatic_transformation = True Then split_single_notes(p)
            env.wsp.display_page(Nothing)
        End If
    End Sub
    Public Overrides Function clone() As Object
        Dim tmp As cls_tool_Notes
        tmp = New cls_tool_Notes
        clone_base(tmp)
        tmp.splitting_pattern = Me.splitting_pattern
        tmp.working_context_mark = Me.working_context_mark
        Return tmp
    End Function
    Private Sub cmd_run_click(sender As Object, e As EventArgs)
        Me.split_all(env._p)
    End Sub
    Private Function get_all_wcx() As Point()
        Dim i As Long
        Dim opened As Boolean
        Dim n As Long = -1
        Dim tmp() As Point
        If env._p.meta_data IsNot Nothing Then
            For i = 0 To UBound(env._p.meta_data)
                With env._p
                    If opened = False Then 'momentálně jsme mimo kontext
                        If .meta_data(i) IsNot Nothing Then
                            If .meta_data(i).Contains(working_context_mark) Then
                                opened = True
                                n += 1
                                ReDim Preserve tmp(n)
                                tmp(n).X = i 'našli jsme první index pole
                            End If
                        End If
                    Else
                        If .meta_data(i) Is Nothing Then
                            opened = False
                            tmp(n).Y = i
                        ElseIf .meta_data(i).Contains(working_context_mark) = False Then
                            opened = False
                            tmp(n).Y = i
                        End If
                    End If
                End With
            Next
            If opened = True Then 'nějaká značka pokračuje až do konce
                tmp(n).Y = i - 1
            End If
        End If
        Return tmp
    End Function
    Private Function get_notes_in_context_span(cnt As Point) As Point()
        Dim si As Integer
        si = cnt.X
        Dim n As Integer = -1
        Dim tmp() As Point
        xyz = Mid(env._p.plain_text, si, 100)
        Do While InStr_first(si, env._p.plain_text, si, 0, "<" & tag, "<" & tag & " ", "<" & tag & "/>") <> -1 And si < cnt.Y
            If (n >= 0) Then
                If (si = tmp(n).X) Then
                    Exit Do
                End If
            End If
            n += 1
            ReDim Preserve tmp(n)
            get_note_from_position(env._p.plain_text, si, tmp(n), cnt.X, cnt.Y)
            si = tmp(n).Y
            If si = 0 Then Exit Do
        Loop
        Return tmp
    End Function
    Public Overrides Function generate_context_menu(p As cls_preXML_section_page, cmn As cls_context_menu) As Object
        If cmn.mtype = 1 Then 'jen u "poletavého" menu
            Dim contexts() As Point
            Dim notes() As Point
            contexts = get_all_wcx()
            Dim str As String
            If contexts IsNot Nothing Then
                Dim i As Long, j As Long
                For i = 0 To UBound(contexts)
                    notes = get_notes_in_context_span(contexts(i))
                    If notes IsNot Nothing Then
                        For j = 0 To UBound(notes)
                            str = Mid(p.plain_text, notes(j).X, notes(j).Y - notes(j).X)
                            cmn.add_tool_cm("Aparát", Me, "Vložit " & str, 9, 0, notes(j))
                        Next j
                    End If
                Next
            End If
        End If
    End Function
    Public Overrides Function context_menu_activated(p As cls_preXML_section_page, p1 As Object, p2 As Object)
        If p1 = 0 Then 'vložení poznámky do textu
            Dim note As String = ""
            If p2.X > p.SelStart1b Then 'musíme dávat pozor na to, jestli místo, na které vkládáme, je před, nebo za místem, z něhož bereme...
                env._p.save_state()
                note = Mid(p.plain_text, p2.X, p2.Y - p2.X)
                p.delete_text_on_position(p2.x, p2.y)
                note = Me.bind(p.plain_text, p.SelStart1b, p.SelLength, note)
                p.delete_text_on_position(p.SelStart1b, p.SelStart1b + p.SelLength - 1)
                p.insert_on_position(p.SelStart1b, note, "")
            Else 'apodle toho buď nejdřív vložit a pak odstranit, nebo naopak (prostě: měnit indexy od zadu)
                env._p.save_state()
                note = Me.bind(p.plain_text, p.SelStart1b, p.SelLength, note)
                p.delete_text_on_position(p.SelStart1b, p.SelStart1b + p.SelLength - 1)
                p.insert_on_position(p.SelStart1b, note, "")
                p.delete_text_on_position(p2.x, p2.y)
            End If
            env.wsp.display_page(Nothing)
            env.wsp.cnm_floating.hide()
        End If
    End Function
End Class
'#################################################'####################################################################################'####################################################################################
Public Class cls_tool_Remove_line_numbers
    'nástroj na odstranění čísel řádků (tj. číslování řádků v edici po straně, které se někdy dostane do OCR)
    Inherits cls_tool

    Public ln_interval As Integer
    Public starts_from_0 As Boolean
    Public active As Boolean

    Public cmd_run As Button
    Public lbl As Label
    Public chb_active As CheckBox



    Public Sub New(name_id_ As String, name_ As String, description_ As String, mark_ As String,
                        ln_interval_ As Integer, starts_from_0_ As Boolean, Optional active_ As Boolean = True)
        name_id = name_id_
        name = name_
        description = description_
        mark = mark
        starts_from_0 = starts_from_0_
        ln_interval = ln_interval_
        active = active_
    End Sub
    Public Sub New(name_ As String, name_id_ As String, mark_ As String, description_ As String, mark1_hgl As cls_highligh_rule)
        MyBase.set_basics(name_, name_id_, mark_, description_, mark1_hgl)
    End Sub
    Public Overrides Function export_to_xml(x As Xml.XmlDocument) As Xml.XmlNode
        Dim n As Xml.XmlNode
        n = x.CreateNode(Xml.XmlNodeType.Element, "tool", "")
        MyBase.export_base_to_xml(n, x)
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "ln_interval", "")).InnerText = ln_interval
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "starts_from_0", "")).InnerText = starts_from_0
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "active", "")).InnerText = active
        Return n
    End Function
    Public Sub New(n As Xml.XmlNode)
        MyBase.import_base_from_xml(n)
        ln_interval = get_singlenode_value(n, "ln_interval")
        starts_from_0 = get_singlenode_value(n, "starts_from_0")
        active = CBool(get_singlenode_value(n, "active", "False"))
    End Sub
    Public Sub New()
        'klonovací
    End Sub

    Public Sub raise(p As cls_preXML_section_page, e As Object, mode As Integer)

    End Sub

    Public Overrides Sub dispose_controls()
        cmd_run.Dispose()
        cmd_run = Nothing
    End Sub

    Public Overrides Sub create_controls(container As Control, last_visualized_tool As Object)
        MyBase.clean_container(container, last_visualized_tool)


        With NewCtrl(chb_active, New CheckBox, container)
            .Text = "Je nástroj aktivní?"
            chb_active.Checked = active
            .Top = TpH() + 5
            .Left = 5
            chb_active.AutoSize = True
            .Visible = True
            AddHandler chb_active.CheckedChanged, AddressOf chb_active_checked_changed
        End With
        With NewCtrl(cmd_run, New Button)
            .Parent = container
            .Left = 5
            .Top = 10
            .Width = 50
            .Height = 25
            .Text = "Proveď"
            .Visible = True
            AddHandler .Click, AddressOf cmd_run_click
        End With
        container.Visible = True
    End Sub


    Public Overrides Sub run(p As cls_preXML_section_page, mode As Integer)
        If p.n_lines > -1 Then
            If Me.active = True Then
                p.save_state()
                Dim i As Integer
                Dim j As Integer
                Dim last_hit_on_start As Integer
                Dim last_hit_on_end As Integer
                Dim assumed_ln() As Integer

                Dim hits_on_start() As Integer
                Dim hits_on_start_nr() As Integer
                Dim n_hos As Integer = -1
                Dim hits_on_end() As Integer
                Dim hits_on_end_nr() As Integer
                Dim n_hoe As Integer = -1

                ReDim assumed_ln(((p.n_lines + 1) / ln_interval) + 1)
                For i = 0 To UBound(assumed_ln)
                    assumed_ln(i) = (i + 1) * ln_interval
                Next

                For i = 0 To UBound(assumed_ln)
                    For j = last_hit_on_start To p.n_lines
                        If rgxt(p.line(j), "^\s*" & assumed_ln(i) & "[\s|$]*") = True Then 'našli jsme potencionální zásah na začátku...
                            n_hos += 1
                            ReDim Preserve hits_on_start(n_hos)
                            ReDim Preserve hits_on_start_nr(n_hos)
                            hits_on_start_nr(n_hos) = assumed_ln(i)
                            hits_on_start(n_hos) = j
                            last_hit_on_start = j + 1
                            Exit For
                        End If

                    Next j
                Next
                For i = 0 To UBound(assumed_ln)
                    For j = last_hit_on_end To p.n_lines
                        If rgxt(p.line(j), "[^0-9]" & assumed_ln(i) & "\s*$") = True Then 'našli jsme potencionální zásah na začátku...
                            n_hoe += 1
                            ReDim Preserve hits_on_end(n_hoe)
                            ReDim Preserve hits_on_end_nr(n_hoe)
                            hits_on_end_nr(n_hoe) = assumed_ln(i)
                            hits_on_end(n_hoe) = j
                            last_hit_on_end = j + 1
                            Exit For
                        End If

                    Next j
                Next

                Dim tmp As String
                If n_hoe > n_hos Then 'asi budou čísla na konci...
                    For i = n_hoe To 0 Step -1
                        'xyz = p.line(hits_on_end(i))
                        p.search_and_replace(True, "([^1-9]" & hits_on_end_nr(i) & "\s*)$", "", "",, p.line_start_index(hits_on_end(i)), 1)
                        p.insert_on_position(p.line_start_index(hits_on_end(i)) + 1, "<preXML_ln n='" & hits_on_end_nr(i) & "'/>", "")
                        'tmp = "<preXML_ln n='" & hits_on_end_nr(i) & "'/>" & rgxr(p.line(hits_on_end(i)), "(\s*" & hits_on_end_nr(i) & "\s*)$", "")
                        'p.line(hits_on_end(i)) = tmp


                    Next
                ElseIf n_hos >= n_hoe Then
                    For i = 0 To n_hos
                        p.search_and_replace(True, "^(\s*" & hits_on_start_nr(i) & ")[\s|$]*", "", "",, p.line_start_index(hits_on_start(i)), 1)
                        p.insert_on_position(p.line_start_index(hits_on_start(i)) + 1, "<preXML_ln n='" & hits_on_start_nr(i) & "'/>", "")
                        'xyz = Mid(p.plain_text, 243, 100)
                        'tmp = "<preXML_ln n='" & hits_on_start_nr(i) & "'/>" & rgxr(p.line(hits_on_start(i)), "^(\s*" & hits_on_start_nr(i) & ")[\s|$]*", "")
                        'p.line(hits_on_start(i)) = tmp
                    Next
                End If
            End If
            If Me.parent.n_tt = -1 Then
                env.wsp.display_page(Nothing)
            Else
                Me.parent.execute(p)
            End If
        End If
    End Sub

    Public Overrides Function clone() As Object
        Dim tmp As cls_tool_Remove_line_numbers
        tmp = New cls_tool_Remove_line_numbers
        clone_base(tmp)
        tmp.ln_interval = Me.ln_interval
        tmp.starts_from_0 = Me.starts_from_0
        Return tmp
    End Function

    Private Sub chb_active_checked_changed(sender As Object, e As EventArgs)
        Me.active = sender.checked
    End Sub
    Private Sub cmd_run_click(sender As Object, e As EventArgs)
        run(env._p, -1)
    End Sub
    Public Overrides Function generate_context_menu(p As cls_preXML_section_page, cmn As cls_context_menu) As Object
        Return False
    End Function
    Public Overrides Function context_menu_activated(p As cls_preXML_section_page, p1 As Object, p2 As Object)

    End Function

End Class
Public Class cls_tool_Wordlist
    Inherits cls_tool
    Public path As String
    Private arr() As String
    Private indices() As word
    Private n_ind As Integer = -1
    Private txt_path As TextBox

    Private Class subcls_wlist
        Public words() As String
        Public letter As String
        Public n_words As Long
        Private file_path As String
        Public Sub New(path As String, fname As String)
            If fname = "_additamenta.txt" Then
                Me.letter = "additamenta"
            Else
                Me.letter = Left(fname, 1)
            End If
            Dim f As IO.StreamReader
            file_path = path
            f = My.Computer.FileSystem.OpenTextFileReader(path)
            Dim n As Integer
            Dim l As String
            Do Until f.EndOfStream
                l = f.ReadLine()

                n_words += 1
            Loop
            n_words -= 1
            f.Close()
            f = My.Computer.FileSystem.OpenTextFileReader(path, System.Text.Encoding.UTF8)
            ReDim words(n_words)
            n = 0
            Do Until f.EndOfStream
                l = LCase(f.ReadLine())

                words(n) = l
                n = n + 1
            Loop
        End Sub
        Public Sub save()
            Dim f As IO.StreamWriter
            Dim i As Long
            f = My.Computer.FileSystem.OpenTextFileWriter(file_path, False, System.Text.Encoding.UTF8)
            For i = 0 To Me.n_words
                f.WriteLine(Me.words(i))
            Next
            f.Close()
        End Sub
    End Class
    Private wlists() As subcls_wlist
    Private n_wlists As Long
    Private Structure word
        Public str As String
        Public start_i As Integer
    End Structure

    Public Sub New()
        'klonovací
    End Sub
    Public Sub New(n As Xml.XmlNode)
        __xml(Nothing, n, False)
    End Sub
    Private Function __xml(x As Xml.XmlDocument, n_imp As Xml.XmlNode, export As Boolean) As Xml.XmlNode
        Dim n As Xml.XmlNode
        Dim i As Long
        If export = True Then
            n = x.CreateNode(Xml.XmlNodeType.Element, "tool", "")
            MyBase.export_base_to_xml(n, x)
        Else
            MyBase.import_base_from_xml(n_imp)
        End If

        If export = True Then
            n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "path", "")).InnerText = path
        Else
            path = get_singlenode_value(n_imp, "path")
        End If
        If export = True Then
            Return n
        Else
            load()
            set_event_listeners()
        End If

    End Function


    Private Sub set_event_listeners()
        ReDim event_listeners(0)
        event_listeners(0) = New cls_event_listener(Me, 1, env.c("Přidej slovo do slovníku."))
        event_listeners(0).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, New cls_keyevent_args(Keys.F1), Nothing), 0)

    End Sub

    Private original_word As String
    Private in_marked_word As Boolean
    Private cancel_edition As Boolean
    Private edited_word_start As Integer

    Public Sub raise(p As cls_preXML_section_page, e As Object, mode As Integer)
        If mode = 1 Then 'přídání slova do slovníku
            add_to_wordlist(p, 1, p.context.word)
        End If
    End Sub
    Public Overrides Function export_to_xml(x As Xml.XmlDocument) As Xml.XmlNode
        Return __xml(x, Nothing, True)
    End Function
    Public Sub New(name_id_ As String, name_ As String, description_ As String, mark_ As String, path_ As String, medieval As Boolean, hgl_ As cls_highligh_rule)
        Me.name_id = name_id_
        Me.name = name_
        Me.description = description_
        Me.mark = mark_
        Me.path = path_
        Me.hgl(0) = hgl_
        env.wsp.marks.add_mark(mark, hgl(0))
        load()
        set_event_listeners()
    End Sub
    Public Sub New(name_ As String, name_id_ As String, mark_ As String, description_ As String, mark1_hgl As cls_highligh_rule)
        MyBase.set_basics(name_, name_id_, mark_, description_, mark1_hgl)
        set_event_listeners()
    End Sub
    Private Sub load()
        If My.Computer.FileSystem.DirectoryExists(path) = True Then
            Dim i As Long
            Dim dir As IO.DirectoryInfo
            dir = My.Computer.FileSystem.GetDirectoryInfo(path)
            n_wlists = dir.GetFiles.Count - 1
            Dim flist() As IO.FileInfo = dir.GetFiles
            ReDim wlists(n_wlists)
            For i = 0 To n_wlists
                wlists(i) = New subcls_wlist(flist(i).FullName, flist(i).Name)
            Next
        Else
            If path <> env.default_folder_path & "\latin-wordlist" Then
                path = env.default_folder_path & "\latin-wordlist"
                load()
            Else
                MsgBox(env.c("Nepodařilo se nahrát slovník, cesta neexistuje. Nástroj ""Kontrola slov"" nebude fungovat."))
                disabled = True
            End If
        End If
    End Sub
    Public Overrides Sub dispose_controls()

    End Sub

    Public Overrides Sub create_controls(container As Control, last_visualized_tool As Object)
        clean_container(container, last_visualized_tool)
        Dim txt As TextBox
        Dim cmd As Button
        With NewCtrl(cmd, New Button, container)
            .Text = env.c("Zkontroluj!")
            cmd.AutoSize = True
            .Left = 5
            .Top = 5
            AddHandler .Click, AddressOf cmd_run_click
        End With
        With NewCtrl(txt_path, New TextBox, container)
            .Top = TpH() + 5
            .Width = container.Width - 35
            .Left = 5
            .Text = Me.path
            .Enabled = False
        End With
        With NewCtrl(cmd, New Button, container)
            .Top = T()
            .Left = LpW()
            .Width = 25
            .Height = 25
            AddHandler .Click, AddressOf cmd_change_path_click
        End With
        container.Visible = True
    End Sub
    Private Sub cmd_change_path_click(sender As Object, e As EventArgs)
        Dim d As FolderBrowserDialog = New FolderBrowserDialog()
        d.SelectedPath = path
        d.ShowDialog()
        If d.SelectedPath <> "" Then
            txt_path.Text = d.SelectedPath
            path = d.SelectedPath
            load()
        End If
    End Sub
    Public Sub cmd_run_click(sender As Object, e As EventArgs)
        If env._p IsNot Nothing Then
            run(env._p, -1)
        End If
    End Sub

    Public Overrides Sub run(pp As cls_preXML_section_page, mode As Integer)
        If disabled = False Then
            Dim i As Integer
            Dim wl() As word
            wl = split_in_words(pp.plain_text)
            If UBound(wl) < 50000 Then
                If wl IsNot Nothing Then
                    For i = 0 To UBound(wl)
                        If check(LCase(wl(i).str)) = False Then
                            pp.add_metadata_to_section(mark, wl(i).start_i, wl(i).start_i + Len(wl(i).str) - 1, True)
                        End If
                    Next
                End If
            End If
            env.wsp.display_page(Nothing, Split(mark))
        End If
    End Sub
    Private Function split_in_words(text As String, Optional index_shift As Integer = 0) As word()
        Dim mc As MatchCollection
        Dim tmp() As word
        Dim rx As Regex
        rx = New Regex("\b[\w]*\b")
        mc = rx.Matches(text)
        Dim i As Integer
        Dim n As Integer = -1
        Dim last_ts As Integer
        Dim last_te As Integer
        If mc IsNot Nothing Then
            If mc.Count <> 0 Then
                ReDim tmp(mc.Count - 1)
            End If
            For i = 0 To mc.Count - 1
                If mc(i).Index <> 0 Then
                    If Mid(text, mc(i).Index, 2) <> "</" And text(mc(i).Index - 1) <> "<" And text(mc(i).Index - 1) <> "&" And Trim(mc(i).Value <> "") And
                        mc(i).Value <> vbLf Then 'pokud to je tag nebo escape sekvence, tak ho přeskočíme
                        n += 1
                        tmp(n).str = mc(i).Value
                        tmp(n).start_i = mc(i).Index + index_shift
                    End If
                End If
            Next
            If n <> -1 Then
                ReDim Preserve tmp(n)
            End If
        End If
        Return tmp
    End Function
    Public Function check(ByVal word As String) As Boolean
        Dim i As Integer
        Dim j As Long
        Dim fl As String
        word = Trim(word)
        If rgxt(word, "^[0-9]+$") = True Then Return True

        fl = Left(word, 1)
first_loop:
        For i = 0 To n_wlists
            If wlists(i).letter = fl Then
                For j = 0 To wlists(i).n_words
                    If wlists(i).words(j) = word Then
                        Return True

                    End If
                Next
            End If
        Next
        If (Right(word, 3) = "que" And word <> "que") Then
            word = Left(word, Len(word) - 3)
            GoTo first_loop
        End If

        For i = 0 To n_wlists
            If wlists(i).letter = "additamenta" Then
                For j = 0 To wlists(i).n_words
                    If wlists(i).words(j) = word Then
                        Return True
                    End If
                Next
            End If
        Next

    End Function
    Public Overrides Function clone() As Object
        Dim tmp As cls_tool_Wordlist
        MyBase.clone_base(tmp)
        tmp.path = Me.path

        Return tmp
    End Function
    Public Overrides Function generate_context_menu(p As cls_preXML_section_page, cmn As cls_context_menu) As Object
        If p IsNot Nothing Then
            If p.context.mark(Me.mark) = True Then
                cmn.add_tool_cm(env.c("Kontrola slov"), Me, "Přidej '" & p.context.word & "' do seznamu slov.", 6, 1, p.context.word)
            End If
        End If
    End Function
    Public Overrides Function context_menu_activated(p As cls_preXML_section_page, p1 As Object, p2 As Object)
        add_to_wordlist(p, p1, p2)
    End Function

    Private Sub add_to_wordlist(p As cls_preXML_section_page, p1 As Object, p2 As Object)
        If p IsNot Nothing Then
            If p1 = 1 Then
                Dim i As Long
                For i = 0 To Me.n_wlists
                    If wlists(i).letter = "additamenta" Then
                        With wlists(i)
                            Dim j As Long
                            p2 = LCase(p2)
                            For j = 0 To .n_words
                                If .words(j) = p2 Then
                                    GoTo replace_added
                                End If
                            Next
                            .n_words += 1
                            ReDim Preserve .words(.n_words)
                            .words(.n_words) = LCase(p2)
                            .save()
replace_added:
                            Dim k As Long
                            Dim replaced_on_this_page As Boolean
                            Dim tmp As String
                            For j = p.m_index To p.parent_d.n_pages
                                tmp = LCase(p.parent_d.page(j).plain_text)
                                k = 0
                                Do While InStrX(k + 1, tmp, p2, k)
                                    p.parent_d.page(j).remove_mark_on_position(mark, k - 1, k - 1 + Len(p2))
                                    If j = p.m_index_ Then replaced_on_this_page = True
                                Loop

                            Next
                            If replaced_on_this_page = True Then env.wsp.display_page(Nothing, Nothing,,, 0)
                        End With
                    End If
                Next
            End If
        End If
    End Sub

End Class
Public Class cls_tool_group_replacing
    Inherits cls_tool
    Private Class cls_replacement
        Public searched As String
        Public replacement As String
        Public rx As Boolean
        Public saved_in_workspace As Boolean
        Public Sub New()
            '
        End Sub
        Public Sub New(searched_ As String, replacement_ As String, rgx_ As Boolean, Optional saved_in_workspace_ As Boolean = False)
            rx = rgx_
            searched = searched_
            replacement = replacement_
            saved_in_workspace = saved_in_workspace_
        End Sub
    End Class
    Private items() As cls_replacement
    Private n_items As Long = -1
    Private path As String

    Private pnl As Panel
    Private txt_repl As TextBox
    Private cmd_ok As Button
    Private chb_save_to_workspace As CheckBox

    Private lsv As ListView
    Private saving_in_workspace As Boolean = True
    Public Sub New(name_id_ As String, name_ As String, description_ As String, mark_ As String, path_ As String, hgl_ As cls_highligh_rule)
        name = name_
        name_id = name_id_
        description = description_
        mark = mark_
        path = path_
        If path = "" Then path = env.default_folder_path & "\group_replacement.xml"
        Me.hgl(0) = hgl_
        env.wsp.marks.add_mark(mark_, Me.hgl(0))
        load()
        create_event_listeners()
    End Sub
    Public Sub New(name_ As String, name_id_ As String, mark_ As String, description_ As String, mark1_hgl As cls_highligh_rule)
        MyBase.set_basics(name_, name_id_, mark_, description_, mark1_hgl)
        create_event_listeners()
    End Sub
    Private Sub create_event_listeners()
        ReDim event_listeners(0)
        event_listeners(0) = New cls_event_listener(Me, 1, "Přidání do tabulky")
        event_listeners(0).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, New cls_keyevent_args(Keys.Right,, True), Nothing), 0)
    End Sub
    Public Sub load()
        Dim i As Long
        Dim nl As Xml.XmlNodeList

        If My.Computer.FileSystem.FileExists(path) Then
            Dim x As New Xml.XmlDocument

            x.Load(path)
            nl = x.SelectNodes("/items/item")
            If nl.Count <> 0 Then
                n_items = nl.Count - 1
                ReDim items(n_items)
            Else
                n_items = -1
                Erase items
            End If
            For i = 0 To nl.Count - 1
                items(i) = New cls_replacement
                items(i).replacement = nl(i).SelectSingleNode("replacement").InnerText
                items(i).searched = nl(i).SelectSingleNode("searched").InnerText
                items(i).rx = CBool(nl(i).SelectSingleNode("rx").InnerText)
                items(i).saved_in_workspace = False
            Next
        Else
            n_items = -1
            Erase items
        End If
        If My.Computer.FileSystem.FileExists(env.opened_document.path & "\replacements.xml") Then
            Dim x2 As New Xml.XmlDocument
            x2.Load(env.opened_document.path & "\replacements.xml")
            nl = x2.SelectNodes("/items/item")

            Dim j As Long
            Dim exists_already As Boolean
            For i = 0 To nl.Count - 1
                Dim repl As String = nl(i).SelectSingleNode("replacement").InnerText
                Dim searched As String = nl(i).SelectSingleNode("searched").InnerText
                Dim regex As Boolean = CBool(nl(i).SelectSingleNode("rx").InnerText)
                'najdeme, jestliuž není v tabulce, aby se to nedublovalo
                exists_already = False
                For j = 0 To Me.n_items
                    If Me.items(j).rx = regex And Me.items(j).searched = searched And Me.items(j).replacement = repl Then
                        exists_already = True
                        Exit For
                    End If
                Next j
                If exists_already = False Then
                    Me.n_items += 1
                    ReDim Preserve Me.items(Me.n_items)
                    items(Me.n_items) = New cls_replacement
                    items(Me.n_items).replacement = repl
                    items(Me.n_items).searched = searched
                    items(Me.n_items).rx = regex
                    items(Me.n_items).saved_in_workspace = False
                End If
            Next
        End If

    End Sub
    Public Sub save()
        ' If My.Computer.FileSystem.FileExists(path) Then
        Dim x As Xml.XmlDocument = New XmlDocument
        Dim root As Xml.XmlNode
        root = x.AppendChild(x.CreateNode(XmlNodeType.Element, "items", ""))
        Dim i As Long
        For i = 0 To n_items
            If items(i).saved_in_workspace = False Then
                With root.AppendChild(x.CreateNode(XmlNodeType.Element, "item", ""))
                    .AppendChild(x.CreateNode(XmlNodeType.Element, "replacement", "")).InnerText = items(i).replacement
                    .AppendChild(x.CreateNode(XmlNodeType.Element, "searched", "")).InnerText = items(i).searched
                    .AppendChild(x.CreateNode(XmlNodeType.Element, "rx", "")).InnerText = items(i).rx
                End With
            End If
        Next
        x.Save(path)
        'End If
    End Sub

    Public Overrides Sub dispose_controls()
        lsv.Dispose()
        lsv = Nothing
    End Sub

    Public Overrides Sub create_controls(container As Control, last_visualized_tool As Object)
        Dim lst As ListView
        Dim txt As TextBox
        Dim cmd As Button
        Dim i As Long
        Me.clean_container(container, last_visualized_tool)
        With NewCtrl(lsv, New ListView, container)
            .Left = 5
            .Top = 5
            .Width = container.Width - 35
            .Height = 300
            lsv.Columns.Add("regex", 50)
            lsv.Columns.Add(env.c("Hledaný výraz"), CInt((lsv.Width - 90) / 2))
            lsv.Columns.Add(env.c("Nahrazení"), CInt((lsv.Width - 90) / 2))
            lsv.Columns.Add("StoW", 20)
            lsv.CheckBoxes = True
            lsv.MultiSelect = True
            lsv.View = View.Details
            lsv.FullRowSelect = True
            lsv.Tag = False
            For i = 0 To n_items
                lsv.Items.Add(New ListViewItem({"", items(i).searched, items(i).replacement}))
                lsv.Items(CInt(i)).Checked = items(i).rx
                If items(i).saved_in_workspace =True Then lsv.Items(CInt(i)).BackColor =Color.Aquamarine 
            Next
            lsv.Tag = True
            AddHandler lsv.AfterLabelEdit, AddressOf lsv_AfterLabelEdit
            AddHandler lsv.ItemChecked, AddressOf lsv_ItemChecked
            AddHandler lsv.MouseDoubleClick, AddressOf lsv_MouseDoubleClick
        End With
        With NewCtrl(cmd, New Button, container)
            .Top = TpH() + 5
            .Left = L()
            .Height = 25
            .Width = 100
            .Text = "+"
            AddHandler .Click, AddressOf cmd_add_click
        End With
        With NewCtrl(cmd, New Button, container)
            .Top = T()
            .Left = LpW() + 5
            .Height = 25
            .Width = 175
            .Text = env.c("Smazat vybrané", "Delete selected")
            AddHandler .Click, AddressOf cmd_remove_click

        End With
        With NewCtrl(txt, New TextBox, container)
            .Text = path
            .Top = TpH() + 5
            .Left = 5
            .Width = container.Width - 35
        End With
        With NewCtrl(cmd, New Button, container)
            .Top = T()
            .Left = LpW()
            .Width = 25
            .Height = 25
        End With
        With NewCtrl(chb_save_to_workspace, New CheckBox, container)
            .Top = TpH() + 5
            .Left = 5
            .Width = 25
            .Height = 25
            .Text = "Ukládat do workspace"
            chb_save_to_workspace.Checked = Me.saving_in_workspace
            AddHandler chb_save_to_workspace.CheckedChanged, AddressOf chc_checked_changed
        End With
        container.Visible = True
    End Sub
    Public Sub lsv_MouseDoubleClick(sender As Object, e As MouseEventArgs)
        If lsv.SelectedItems IsNot Nothing Then
            If lsv.SelectedItems.Count <> 0 Then
                Dim c As Long
                Dim ib As String
                If e.X < lsv.Columns(1).Width + lsv.Columns(0).Width Then
                    ib = InputBox("Co se má hledat?",, items(lsv.SelectedItems(0).Index).searched)
                    If ib <> "" Then
                        items(lsv.SelectedItems(0).Index).searched = ib
                        lsv.SelectedItems(0).SubItems(1).Text = items(lsv.SelectedItems(0).Index).searched
                    End If
                ElseIf e.X < lsv.Columns(2).Width + lsv.Columns(1).Width + lsv.Columns(0).Width Then
                    ib = InputBox("Čím se to má nahradit?",, items(lsv.SelectedItems(0).Index).replacement)
                    If ib <> "" Then
                        items(lsv.SelectedItems(0).Index).replacement = ib
                        lsv.SelectedItems(0).SubItems(2).Text = items(lsv.SelectedItems(0).Index).replacement
                    End If
                Else
                    items(lsv.SelectedItems(0).Index).saved_in_workspace = Not items(lsv.SelectedItems(0).Index).saved_in_workspace
                    If (items(lsv.SelectedItems(0).Index).saved_in_workspace = True) Then
                        lsv.SelectedItems.Item(0).BackColor = Color.Aquamarine
                    Else
                        lsv.SelectedItems.Item(0).BackColor = Color.White
                    End If
                End If
            End If
        End If
    End Sub
    Private Sub chc_checked_changed(sender As Object, e As EventArgs)
        Me.saving_in_workspace = sender.checked
    End Sub


    Private Sub cmd_remove_click(sender As Object, e As EventArgs)
        If Me.n_items > -1 Then
            Dim i As Integer, j As Long
            Dim to_remove() As Boolean
            ReDim to_remove(Me.n_items)
            For i = 0 To lsv.Items.Count - 1
                If lsv.Items(i).Selected = True Then
                    to_remove(i) = True
                End If
            Next
            i = 0
            Do While i <= Me.n_items
                If to_remove(i) = True Then
                    For j = i To Me.n_items - 1
                        items(j) = items(j + 1)
                        to_remove(j) = to_remove(j + 1)
                    Next
                    Me.n_items -= 1
                    i = i - 1
                End If
                i += 1
            Loop
            If Me.n_items > -1 Then
                ReDim Preserve Me.items(Me.n_items)
            Else
                Erase Me.items
            End If
            lsv.Items.Clear()
            lsv.Tag = False
            For i = 0 To n_items
                lsv.Items.Add(New ListViewItem({"", items(i).searched, items(i).replacement}))
                lsv.Items(CInt(i)).Checked = items(i).rx
            Next
            lsv.Tag = True
        End If
    End Sub
    Private Sub cmd_add_click(sender As Object, e As EventArgs)
        n_items += 1
        ReDim Preserve items(n_items)
        items(n_items) = New cls_replacement("", "", False, Me.saving_in_workspace)
        With lsv.Items.Add("")
            .SubItems.Add("co se má hledat?")
            .SubItems.Add("čím se to má nahradit?")
        End With

    End Sub
    Public Sub lsv_ItemChecked(sender As Object, e As ItemCheckedEventArgs)
        If sender.tag <> False Then
            With items(e.Item.Index)
                If e.Item.SubItems.Count > 1 Then
                    .replacement = e.Item.SubItems(2).Text
                    .searched = e.Item.SubItems(1).Text
                    .rx = e.Item.Checked
                End If
            End With
        End If
    End Sub
    Public Sub lsv_AfterLabelEdit(sender As Object, e As LabelEditEventArgs)
        With items(e.Item)
            .replacement = lsv.Items(e.Item).SubItems(2).Text
            .searched = lsv.Items(e.Item).SubItems(1).Text
            .rx = lsv.Items(e.Item).Checked
        End With
    End Sub

    Private before_changed_word As String
    Private after_changed_word As String
    Public Sub raise(p As cls_preXML_section_page, e As Object, mode As Integer)
        If p IsNot Nothing Then
            If p.context.inside_of_tag = "" And p.context.word <> "" Then
                If mode = 1 Then 'přídání slova do slovníku pomocí malého okénka přímo v textu

                    With env.wsp.rtb.Parent
                        Dim tmp() As Control
                        tmp = .Controls.Find("grp_replacement_pnl", False)
                        If tmp.Count > 0 Then If tmp(0) IsNot Nothing Then tmp(0).Dispose()
                    End With
                    pnl = Nothing
                    If pnl Is Nothing Then
                        Dim lbl As Label
                        pnl = New Panel
                        pnl.Parent = env.wsp.rtb.Parent
                        pnl.Height = 25
                        pnl.Name = "grp_replacement_pnl"

                        lbl = New Label
                        lbl.Parent = pnl
                        lbl.Visible = False
                        lbl.AutoSize = True

                        Dim tmp_font As Font
                        tmp_font = env.wsp.rtb.SelectionFont

                        lbl.Font = tmp_font
                        lbl.Text = p.context.word

                        txt_repl = New TextBox
                        txt_repl.Parent = pnl
                        txt_repl.Text = p.context.word

                        'musíme si uložit, co je kolem měněného slova, abychom pak věděli, jeslti je to celé slovo nebo jen nějaká část...
                        before_changed_word = ""
                        after_changed_word = ""
                        If p.context.word_boundaries1b.X > 1 Then before_changed_word = p.plain_text(p.context.word_boundaries1b.X - 2)
                        If p.context.word_boundaries1b.Y < Len(p.plain_text) Then after_changed_word = p.plain_text(p.context.word_boundaries1b.Y - 1)

                        txt_repl.Font = tmp_font
                        txt_repl.Width = lbl.Width + 2
                            txt_repl.Height = lbl.Height + 2
                            txt_repl.Text = p.context.word
                            txt_repl.BackColor = Color.LightGray
                            txt_repl.Tag = p.context.word
                            AddHandler txt_repl.KeyDown, AddressOf txt_repl_keydown
                            AddHandler txt_repl.Leave, AddressOf txt_repl_leave

                            pnl.Width = txt_repl.Width
                            pnl.Height = txt_repl.Height


                            Dim pos As Point
                            pos = env.wsp.rtb.GetPositionFromCharIndex(p.context.word_boundaries1b.X - 1)
                            pnl.Left = pos.X + env.wsp.rtb.Left
                            pnl.Top = pos.Y + env.wsp.rtb.Top
                            pnl.Visible = True
                            pnl.BringToFront()
                            txt_repl.Select()
                            txt_repl.SelectionStart = p.SelStart1b - (p.context.word_boundaries1b.X)
                            txt_repl.SelectionLength = p.SelLength
                            n_lefts = 0
                            n_rights = 0
                            n_ups = 0
                            n_downs = 0
                        End If
                    End If
            End If
        End If
    End Sub

    Private n_lefts As Integer
    Private n_downs As Integer
    Private n_ups As Integer
    Private n_rights As Integer
    Private Sub txt_repl_keydown(sender As Object, e As KeyEventArgs)
        If e.KeyValue = Keys.Escape Then
            txt_repl_leave(sender, Nothing)
            env.wsp.rtb.Select()
        ElseIf e.KeyValue = Keys.Enter Then
            If txt_repl.Text <> txt_repl.Tag Then
                Dim asrgx As Boolean
                If Len(sender.tag) < 7 Then
                    If rgxt(before_changed_word & sender.tag, "^.\b(\w+)") = True Then
                        asrgx = True
                        'If rgxt(txt_repl.Tag, "([\\.*|()[\]])") = True Then Stop

                        txt_repl.Tag = rgxr(CStr(txt_repl.Tag), "([\\.*'|()[\]])", "\$1")
                        txt_repl.Tag = "\b" & txt_repl.Tag
                    End If
                    If rgxt(sender.tag & after_changed_word, "(\w+)\b.$") = True Then
                        If asrgx = False Then txt_repl.Tag = rgxr(CStr(txt_repl.Tag), "([\\.*'|()[\]])", "\$1")
                        asrgx = True
                        txt_repl.Tag &= "\b"
                    End If
                End If

                If does_already_exists(txt_repl.Text, asrgx) = False Then
                        n_items += 1
                        ReDim Preserve items(n_items)
                    items(n_items) = New cls_replacement(txt_repl.Tag, txt_repl.Text, asrgx, Me.saving_in_workspace)
                    For i = env.wsp.p.m_index To env.opened_document.n_pages
                            env.opened_document.page(i).search_and_replace(asrgx, txt_repl.Tag, txt_repl.Text, Me.mark)
                        Next
                        env.wsp.display_page(Nothing, Split(Me.mark),,, 0)
                        txt_repl_leave(sender, Nothing)
                        env.wsp.rtb.Select()
                    End If
                End If
            ElseIf e.KeyValue = Keys.Right Then
                If txt_repl.SelectionStart = Len(txt_repl.Text) Then n_lefts += 1
                If n_lefts > 2 Then
                    txt_repl_leave(sender, Nothing)
                    env.wsp.rtb.Select()
                    env.wsp.rtb.SelectionStart = env._p.context.word_boundaries1b.Y
                End If
            ElseIf e.KeyValue = Keys.Left Then
                If txt_repl.SelectionStart = 0 Then n_lefts += 1
                If n_lefts > 2 Then
                    txt_repl_leave(sender, Nothing)
                    env.wsp.rtb.Select()
                    env.wsp.rtb.SelectionStart = env._p.context.word_boundaries1b.X - 1
                End If
            ElseIf e.KeyValue = Keys.Up Or e.KeyValue = Keys.Down Then
                n_ups += 1
            If n_ups > 2 Then
                txt_repl_leave(sender, Nothing)
                env.wsp.rtb.Select()
            End If
        End If
    End Sub
    Private Sub txt_repl_leave(sender As Object, e As EventArgs)
        If pnl IsNot Nothing Then
            Dim par As Control
            par = pnl.Parent
            Dim ctrl() As Control
            If par IsNot Nothing Then
                ctrl = par.Controls.Find(pnl.Name, False)
                If ctrl IsNot Nothing Then
                    For i = 0 To UBound(ctrl)
                        ctrl(0).Dispose()
                    Next
                End If
            End If
            pnl = Nothing
        End If
    End Sub

    Public Overrides Sub run(pp As cls_preXML_section_page, mode As Integer)
        If pp IsNot Nothing Then
            pp.save_state()
            Dim i As Long
            For i = 0 To Me.n_items
                If Me.items(i).searched <> "" Then
                    pp.search_and_replace(Me.items(i).rx, Me.items(i).searched, Me.items(i).replacement, Me.mark, True, 0, -1, -1, True)
                End If
            Next
        End If
    End Sub

    Public Overrides Function clone() As Object

    End Function

    Public Overrides Function generate_context_menu(p As cls_preXML_section_page, cmn As cls_context_menu) As Object
        If p.context.word <> "" Then
            cmn.add_tool_cm(env.c("Kontrola slov"), Me, env.c("Přidej", "Add") & " '" & p.context.word & "' " &
                            env.c("do tabulky hrom. nahr.", "in the table of repl.s"), 3, 1, p.context.word)
        End If
    End Function

    Public Overrides Function context_menu_activated(p As cls_preXML_section_page, p1 As Object, p2 As Object) As Object
        If p1 = 1 Then 'přídání do tabulky pro hromadné nahrazení
            Dim co As String = InputBox(env.c("Co se má hledat?", "Searchterm"),, CStr(p2))
            If co <> "" Then
                Dim cim As String = InputBox(env.c("Čím se to má nahradit?", "Replacement"))
                Dim je_rx As Long = MsgBox(env.c("Jde o regulérní výraz?", "Regular expression?"), MsgBoxStyle.Question Or MsgBoxStyle.YesNoCancel)
                If je_rx = MsgBoxResult.Cancel Then
                    Exit Function
                ElseIf je_rx = MsgBoxResult.Yes Then
                    je_rx = 1
                Else
                    je_rx = 0
                End If
                If does_already_exists(co, je_rx) = False Then
                    n_items += 1
                    ReDim Preserve items(n_items)
                    items(n_items) = New cls_replacement(co, cim, je_rx, Me.saving_in_workspace)

                    If MsgBox(env.c("Provést rovnou nahrazení tohoto výrazu v aktuálním textu?", "Replace the searchterm in the text now?"), MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                        p.search_and_replace(je_rx, co, cim, Me.mark)
                    End If

                    If lsv IsNot Nothing Then 'nástoj nemusí být aktivován, ale pokud je, aktualizujeme tabulku
                        If lsv.Items IsNot Nothing Then
                            With lsv.Items.Add("")
                                .Checked = je_rx
                                .SubItems.Add(co)
                                .SubItems.Add(cim)
                            End With
                        End If
                    End If
                End If
            End If
        End If
    End Function
    Private Function does_already_exists(what As String, isrx As Boolean) As Boolean
        Dim i As Long
        For i = 0 To Me.n_items
            If items(i).searched = what And items(i).rx = isrx Then
                Dim tmp As String
                tmp = "Toto slovo už v tabulce nahrazení existuje a je nahrazováno tímto '" & items(i).replacement & "'."
                If items(i).rx = True Then
                    tmp &= vbNewLine & "(Jedná se o regulérní výraz)."
                Else
                    tmp &= vbNewLine & "(Nejedná se o regulérní výraz)."
                End If
                MsgBox(tmp)
                Return True
            End If
        Next
    End Function
    Public Overrides Function export_to_xml(x As XmlDocument) As XmlNode
        Return __xml(x, Nothing, True)
    End Function
    Private Function __xml(x As Xml.XmlDocument, n_imp As Xml.XmlNode, export As Boolean) As Xml.XmlNode
        Dim n As Xml.XmlNode

        If export = True Then
            save()
            n = x.CreateNode(Xml.XmlNodeType.Element, "tool", "")
            MyBase.export_base_to_xml(n, x)
            Dim root As Xml.XmlNode
            root = n.AppendChild(x.CreateNode(XmlNodeType.Element, "items", ""))
            Dim i As Long
            For i = 0 To n_items
                If items(i).saved_in_workspace = True Then
                    With root.AppendChild(x.CreateNode(XmlNodeType.Element, "item", ""))
                        .AppendChild(x.CreateNode(XmlNodeType.Element, "replacement", "")).InnerText = items(i).replacement
                        .AppendChild(x.CreateNode(XmlNodeType.Element, "searched", "")).InnerText = items(i).searched
                        .AppendChild(x.CreateNode(XmlNodeType.Element, "rx", "")).InnerText = items(i).rx
                    End With
                End If
            Next
        Else
            MyBase.import_base_from_xml(n_imp)
            If path = "" Then
                path = env.default_folder_path & "\group_replacement.xml"
            End If
            load()
            Dim xitems As Xml.XmlNode
            xitems = n_imp.SelectSingleNode("items")
            If (xitems IsNot Nothing) Then
                Dim i As Long = Me.n_items
                Dim xitem As Xml.XmlNode
                For Each xitem In xitems.ChildNodes
                    If xitem.NodeType = XmlNodeType.Element Then
                        Me.n_items += 1
                        ReDim Preserve Me.items(Me.n_items)
                        Me.items(Me.n_items) = New cls_replacement
                        Me.items(Me.n_items).replacement = xitem.SelectSingleNode("replacement").InnerText
                        Me.items(Me.n_items).searched = xitem.SelectSingleNode("searched").InnerText
                        Me.items(Me.n_items).rx = CBool(xitem.SelectSingleNode("rx").InnerText)
                        Me.items(Me.n_items).saved_in_workspace = True
                    End If
                Next
            End If
        End If

        If export = True Then Return n
    End Function
    Public Sub New(n As Xml.XmlNode)
        __xml(Nothing, n, False)
        If path = "" Then
            path = env.default_folder_path & "\group_replacement.xml"
        End If
        create_event_listeners()
    End Sub
End Class
Public Class cls_tool_Attributes_insertion
    Inherits cls_tool
    Private Class scls_attribute_settings
        Public attribute As String
        Public value As String
        Public tags_i_can_be_in As String
        Public attributes_ancestor_must_have As String
        Public Sub New(attribute_ As String, value_ As String, tags_i_can_be_in_ As String, attributes_ancestor_must_have_ As String)
            attribute = attribute_
            value = value_
            tags_i_can_be_in = tags_i_can_be_in_
            attributes_ancestor_must_have = attributes_ancestor_must_have_
        End Sub
        Public Function export_to_xml(x As XmlDocument) As XmlNode
            Dim n As XmlNode
            n = x.CreateNode(XmlNodeType.Element, "attribute", "")
            n.AppendChild(x.CreateNode(XmlNodeType.Element, "attr_name", "")).InnerText = attribute
            n.AppendChild(x.CreateNode(XmlNodeType.Element, "attr_value", "")).InnerText = value
            n.AppendChild(x.CreateNode(XmlNodeType.Element, "tags_i_can_be_in", "")).InnerText = tags_i_can_be_in
            n.AppendChild(x.CreateNode(XmlNodeType.Element, "attributes_ancestors_must_have", "")).InnerText = attributes_ancestor_must_have
            Return n
        End Function
        Public Sub New(n As XmlNode)
            If n IsNot Nothing Then
                attribute = get_singlenode_value(n, "attr_name")
                value = get_singlenode_value(n, "attr_value")
                tags_i_can_be_in = get_singlenode_value(n, "tags_i_can_be_in")
                attributes_ancestor_must_have = get_singlenode_value(n, "attributes_ancestors_must_have")
            End If
        End Sub
    End Class
    Private attr() As scls_attribute_settings
    Private n_attr As Long

    Private Sub default_attributes()
        n_attr = 14
        ReDim attr(n_attr)
        attr(0) = New scls_attribute_settings("type", "treatise", "div", "")
        attr(1) = New scls_attribute_settings("type", "book", "div", "")
        attr(2) = New scls_attribute_settings("type", "chapter", "div", "type='book'")
        attr(3) = New scls_attribute_settings("type", "subchapter", "div", "type='chapter'")
        attr(4) = New scls_attribute_settings("type", "dipl_app", "div", "")
        attr(5) = New scls_attribute_settings("type", "dipl_app_regest", "div", "type='dipl_app'")
        attr(6) = New scls_attribute_settings("type", "dipl_app_date_and_place", "div", "type='dipl_app'")
        attr(7) = New scls_attribute_settings("type", "dipl_app_editions_info", "div", "type='dipl_app'")
        attr(8) = New scls_attribute_settings("type", "dipl_app_manuscript_info", "div", "type='dipl_app'")
        attr(9) = New scls_attribute_settings("type", "dipl_app_scribae_info", "div", "type='dipl_app'")
        attr(10) = New scls_attribute_settings("type", "dipl_app_seal_info", "div", "type='dipl_app'")
        attr(11) = New scls_attribute_settings("type", "editorial_preface", "div", "")
        attr(12) = New scls_attribute_settings("type", "document", "div", "")
        attr(13) = New scls_attribute_settings("type", "example", "div", "")
        attr(14) = New scls_attribute_settings("type", "questio", "div", "")
    End Sub

    Public Overrides Sub dispose_controls()
    End Sub

    Public Overrides Sub create_controls(container As Control, last_visualized_tool As Object)

    End Sub

    Public Overrides Sub run(pp As cls_preXML_section_page, mode As Integer)

    End Sub

    Public Overrides Function clone() As Object

    End Function

    Public Overrides Function generate_context_menu(p As cls_preXML_section_page, cmn As cls_context_menu) As Object
        Dim i As Long, j As Long, k As Long
        If Me.attr IsNot Nothing Then
            If p.context.inside_of_tag <> "" And InStr(1, p.context.inside_of_tag, "/") = 0 Then

                For i = 0 To Me.n_attr
                    If Split(attr(i).tags_i_can_be_in, "|").Contains(p.context.inside_of_tag_name) = True Then
                        If attr(i).attributes_ancestor_must_have <> "" Then
                            Dim at_w_v() As String
                            at_w_v = Split(attr(i).attributes_ancestor_must_have, "|")
                            Dim at As String
                            Dim at_v As String
                            For j = 0 To UBound(at_w_v)
                                at = Trim(Left(at_w_v(j), InStr(1, at_w_v(j), "=") - 1))
                                at_v = Replace(Trim(Mid(at_w_v(j), InStr(1, at_w_v(j), "=") + 1)), "'", "")

                                For k = 0 To p.context.n_tags_opened
                                    If p.context.tags_opened(k).has_attribute_with_value(at, at_v) = True Then
                                        cmn.add_tool_cm(env.c("Vkládání atributů", "Adding attributes"), Me, attr(i).attribute & "=" & attr(i).value, 8, 1, i)
                                        Exit For
                                    End If
                                Next
                            Next
                        Else
                            cmn.add_tool_cm(env.c("Vkládání atributů", "Adding attributes"), Me, attr(i).attribute & "=" & attr(i).value, 8, 1, i)
                        End If
                    End If
                Next
            End If
        End If
    End Function

    Public Overrides Function context_menu_activated(p As cls_preXML_section_page, p1 As Object, p2 As Object) As Object
        If p IsNot Nothing Then
            p.save_state()

            If p1 = 1 Then
                Dim si As Integer
                Dim ei As Integer
                If p.context.n_tags_opened <> -1 Then
                    p.context.tags_opened(p.context.n_tags_opened).add_attribute(attr(p2).attribute, attr(p2).value)
                    p.context.tags_opened(p.context.n_tags_opened).write_out(p)
                    env.wsp.display_page(Nothing, Nothing)
                End If
            End If
        End If
    End Function

    Public Overrides Function export_to_xml(x As XmlDocument) As XmlNode
        Return __xml(x, Nothing, True)
    End Function
    Public Sub New(n As XmlNode)
        __xml(Nothing, n, False)
    End Sub
    Public Sub New(name_ As String, name_id_ As String, mark_ As String, description_ As String, mark1_hgl As cls_highligh_rule)
        MyBase.set_basics(name_, name_id_, mark_, description_, mark1_hgl)
        default_attributes()
    End Sub

    Private Function __xml(x As Xml.XmlDocument, n_imp As Xml.XmlNode, export As Boolean) As Xml.XmlNode
        Dim n As Xml.XmlNode
        Dim nl As Xml.XmlNodeList
        Dim i As Long
        If export = True Then
            n = x.CreateNode(Xml.XmlNodeType.Element, "tool", "")
            MyBase.export_base_to_xml(n, x)
            For i = 0 To n_attr
                n.AppendChild(attr(i).export_to_xml(x))
            Next
        Else
            MyBase.import_base_from_xml(n_imp)
            nl = n_imp.SelectNodes("attribute")
            n_attr = -1
            If nl IsNot Nothing Then
                n_attr = nl.Count - 1
                ReDim attr(n_attr)
                For i = 0 To n_attr
                    attr(i) = New scls_attribute_settings(nl(i))
                Next
            End If
        End If

        If export = True Then Return n
    End Function
End Class
Public Class cls_tool_XML_manipulation
    Inherits cls_tool

    Private Const SWICH_TO_OPENING As Integer = 0
    Private Const SWICH_TO_CLOSING As Integer = 1

    Public Sub New(name_ As String, name_id_ As String, mark_ As String, description_ As String, mark1_hgl As cls_highligh_rule)
        MyBase.set_basics(name_, name_id_, mark_, description_, mark1_hgl)
    End Sub
    Public Overrides Sub dispose_controls()

    End Sub

    Public Overrides Sub create_controls(container As Control, last_visualized_tool As Object)
        Me.clean_container(container, last_visualized_tool)
        Dim lbl As Label
        With NewCtrl(lbl, New Label, container)
            .Top = 5
            .Left = 5
            lbl.AutoSize = True
            .Text = "Nástroj nemá žádné ovládací prvky."
        End With
    End Sub

    Public Overrides Sub run(pp As cls_preXML_section_page, mode As Integer)
        'nic
    End Sub

    Public Overrides Function clone() As Object
    End Function

    Public Overrides Function generate_context_menu(p As cls_preXML_section_page, cmn As cls_context_menu) As Object
        If p.context.inside_of_tag <> "" Then
            Dim last_opened_tag As cls_preXML_tag
            last_opened_tag = p.context.tags_opened(p.context.n_tags_opened)
            If last_opened_tag IsNot Nothing Then
                If InStr(1, p.context.inside_of_tag, "/") = 1 Then
                    cmn.add_tool_cm("XML", Me, env.c("Přepnout na příslušný otvírací tag", "Go to opening tag"), 4, SWICH_TO_OPENING, last_opened_tag)
                ElseIf InStr(1, p.context.inside_of_tag, "/") = 0 Then
                    cmn.add_tool_cm("XML", Me, env.c("Přepnout na příslušný otvírací tag", "Go to closing tag"), 4, SWICH_TO_CLOSING, last_opened_tag.second_to_pair)
                End If
            End If
        End If
    End Function

    Public Overrides Function context_menu_activated(p As cls_preXML_section_page, p1 As Object, p2 As Object) As Object
        Dim sl As Integer
        If p1 = SWICH_TO_OPENING Or p1 = SWICH_TO_CLOSING And p2 IsNot Nothing Then
            If env._p.m_index <> p2.position.x Then
                env.wsp.open_page(p2.position.x)
                env.wsp.p.force_SelStart = p2.position.y
                sl = InStr(p2.position.y, env._p.plain_text, ">")
                If sl <> 0 Then sl = sl - p2.position.y
                If sl > 0 And sl < 500 Then env._p.force_SelLength = sl
                env.wsp.display_page(Nothing,,,, 1)
            Else
                env.wsp.p.force_SelStart = p2.position.y
                sl = InStr(p2.position.y, env._p.plain_text, ">")
                If sl <> 0 Then sl = sl - p2.position.y
                If sl > 0 And sl < 500 Then env._p.force_SelLength = sl
                env.wsp.rtb.SelectionStart = p2.position.y
            End If
        End If
    End Function
    Public Sub New(n As Xml.XmlNode)
        __xml(Nothing, n, False)
    End Sub
    Private Function __xml(x As Xml.XmlDocument, n_imp As Xml.XmlNode, export As Boolean) As Xml.XmlNode
        Dim n As Xml.XmlNode
        Dim i As Long
        If export = True Then
            n = x.CreateNode(Xml.XmlNodeType.Element, "tool", "")
            MyBase.export_base_to_xml(n, x)

        Else
            MyBase.import_base_from_xml(n_imp)
        End If

        If export = True Then
            Return n
        End If
    End Function
    Public Overrides Function export_to_xml(x As XmlDocument) As XmlNode
        Return __xml(x, Nothing, True)
    End Function

End Class