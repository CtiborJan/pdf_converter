Public Class cls_mousevent_args
    Public button As Integer
    Public ctrl As Boolean
    Public alt As Boolean
    Public shift As Boolean
    Public Sub New(Optional button_ As Integer = 1, Optional ctrl_ As Boolean = False, Optional alt_ As Boolean = False, Optional shift_ As Boolean = False)
        Me.button = button_
        Me.ctrl = ctrl_
        Me.alt = alt_
        Me.shift = shift_
    End Sub
    Public Overrides Function ToString() As String
        Dim rv As String = ""
        If ctrl = True Then rv = "CTRL"
        If alt = True Then rv += "+ALT"
        If shift = True Then rv += "+SHIFT"
        If button = 1 Then
            rv += "+levé tl."
        ElseIf button = 2 Then
            rv += "+pravé tl."
        Else
            rv += "+prostřední tl."
        End If

        If Left(rv, 1) = "+" Then rv = Mid(rv, 2)
        Return rv
    End Function
    Public Sub New(n As Xml.XmlNode)
        Dim xpath_base As String = ""
        If n.SelectSingleNode("button") Is Nothing Then
            xpath_base = "mouse_event_args/"
        End If
        ctrl = CBool(get_singlenode_value(n, xpath_base & "ctrl"))
        alt = CBool(get_singlenode_value(n, xpath_base & "alt"))
        shift = CBool(get_singlenode_value(n, xpath_base & "shift"))
        button = get_singlenode_value(n, xpath_base & "button")
    End Sub
    Public Function export_to_xml(x As Xml.XmlDocument) As Xml.XmlNode
        Dim n As Xml.XmlNode
        n = x.CreateNode(Xml.XmlNodeType.Element, "mouse_event_args", "")
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "button", "")).InnerText = Me.button
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "alt", "")).InnerText = Me.alt
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "ctrl", "")).InnerText = Me.ctrl
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "shift", "")).InnerText = Me.shift
    End Function
End Class
Public Class cls_keyevent_args
    Public ctrl As Boolean
    Public alt As Boolean
    Public shift As Boolean
    Public value As Integer
    Public Sub New(str As String)
        Dim arr() As String
        arr = Split(UCase(str), "+")
        Dim i As Long
        For i = 0 To UBound(arr)
            If arr(i) = "CTRL" Then
                Me.ctrl = True
            ElseIf arr(i) = "ALT" Then
                Me.alt = True
            ElseIf arr(i) = "SHIFT" Then
                Me.shift = True
            Else
                If Len(arr(i)) = 1 Then
                    Me.value = Asc(arr(i))
                End If
            End If
        Next
    End Sub
    Public Sub New(Optional value_ As Integer = 0, Optional ctrl_ As Boolean = False, Optional alt_ As Boolean = False, Optional shift_ As Boolean = False)
        Me.ctrl = ctrl_
        Me.alt = alt_
        Me.shift = shift_
        Me.value = value_
    End Sub
    Public Sub New(n As Xml.XmlNode)
        Dim xpath_base As String = ""
        If n.SelectSingleNode("value") Is Nothing Then
            xpath_base = "key_event_args/"
        End If
        ctrl = CBool(get_singlenode_value(n, xpath_base & "ctrl"))
        alt = CBool(get_singlenode_value(n, xpath_base & "alt"))
        shift = CBool(get_singlenode_value(n, xpath_base & "shift"))
        value = get_singlenode_value(n, xpath_base & "value")
    End Sub
    Public Overrides Function toString() As String
        Dim rv As String = ""
        If ctrl = True Then rv = "CTRL"
        If alt = True Then rv += "+ALT"
        If shift = True Then rv += "+SHIFT"
        Dim arr As Object
        arr = [Enum].GetNames(GetType(Keys))

        rv += "+" & GetType(Keys).GetEnumName(value)
        If Left(rv, 1) = "+" Then rv = Mid(rv, 2)
        Return rv
    End Function
    Public Function export_to_xml(x As Xml.XmlDocument) As Xml.XmlNode
        Dim n As Xml.XmlNode
        n = x.CreateNode(Xml.XmlNodeType.Element, "key_event_args", "")
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "value", "")).InnerText = Me.value
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "alt", "")).InnerText = Me.alt
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "ctrl", "")).InnerText = Me.ctrl
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "shift", "")).InnerText = Me.shift
        Return n
    End Function
End Class
Public Class cls_event_description
    '
    Public parent As Object
    Public ev as Integer 'číslo události (viz enum events_name)
    Public key_ev As cls_keyevent_args 'pokud je to událost klávesnice, tohle jsou její argumenty
    Public mouse_ev As cls_mousevent_args
    Public Sub New(ev_ as Integer, key_ev_ As cls_keyevent_args, mouse_ev_ As cls_mousevent_args)
        ev = ev_
        key_ev = key_ev_
        mouse_ev = mouse_ev_
    End Sub
    Public Function toStringSim() As String
        If key_ev IsNot Nothing Then
            Return key_ev.toString
        ElseIf mouse_ev IsNot Nothing Then
            Return mouse_ev.ToString
        Else
            Return ev_toString()
        End If

    End Function
    Private Function ev_toString() As String

        If ev = EN.evn_PAGE_CLOSED Then
            Return "Zavření stránky"
        ElseIf ev = EN.evn_PAGE_OPENED Then
            Return "Otevření stránky"
        ElseIf ev = EN.evn_TEXT_INSERTED Then
            Return "Vložení textu"
        ElseIf ev = EN.evn_RTB_CHANGED Then
            Return "Změna textu"
        ElseIf ev = EN.evn_RTB_SELECTION_CHANGED Then
            Return "Změna výběru textu"

        ElseIf ev = EN.evn_FRM_KEY_DOWN Then
            Return "Stisk kl."
        ElseIf ev = EN.evn_FRM_KEY_UP Then
            Return "Uvolnění kl."
        ElseIf ev = EN.evn_FRM_KEY_PRESS Then
            Return "Úder kl."

        ElseIf ev = EN.evn_RTB_MOUSE_CLICK Then
            Return "Klik myší"
        ElseIf ev = EN.evn_RTB_MOUSE_DBL_CLICK Then
            Return "Dvojklik. myší"
        ElseIf ev = EN.evn_RTB_MOUSE_DOWN Then
            Return "Stisknutí tl. myši"
        ElseIf ev = EN.evn_RTB_MOUSE_UP Then
            Return "Uvolnění tl. myši"
        ElseIf ev = EN.evn_RTB_MOUSE_MOVE Then
            Return "Pohyb myší"
        End If
    End Function
    Public Overrides Function toString() As String
        If key_ev IsNot Nothing Then
            Return ev_toString() & " {" & key_ev.toString & "}"
        ElseIf mouse_ev IsNot Nothing Then
            Return ev_toString() & " {" & mouse_ev.ToString & "}"
        Else
            Return ev_toString()
        End If

    End Function
    Public Function export_to_xml(x As Xml.XmlDocument) As Xml.XmlNode
        Dim n As Xml.XmlNode
        n = x.CreateNode(Xml.XmlNodeType.Element, "event_desc", "")
        n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "event_nr", "")).InnerText = Me.ev
        If Me.mouse_ev IsNot Nothing Then
            n.AppendChild(Me.mouse_ev.export_to_xml(x))
        End If
        If Me.key_ev IsNot Nothing Then
            n.AppendChild(Me.key_ev.export_to_xml(x))
        End If
        Return n
    End Function
    Public Sub New(n As Xml.XmlNode)
        ev = get_singlenode_value(n, "event_nr")
        Dim tmp As Xml.XmlNode
        tmp = n.SelectSingleNode("mouse_event_args")
        If tmp IsNot Nothing Then
            Me.mouse_ev = New cls_mousevent_args(n)
        End If
        tmp = n.SelectSingleNode("key_event_args")
        If tmp IsNot Nothing Then
            Me.key_ev = New cls_keyevent_args(n)
        End If
    End Sub
End Class
Public Class cls_tools_collection
    Inherits cls_events_handling
    Public t_o() As cls_tools_organizer
    Public n_tools as Integer = -1
    Public name As String
    Public type As String

    Public container As Control 'v čem jsem obsažen
    Public activating_button As RadioButton 'tlačítko v seznamu kolekcí, které mi patří (abych mohl změnit zkratku tam napsanou, když se změní)
    Private Structure ctrls
        Public trv_tools As TreeView
        Public btn_add_tool As Button
        Public btn_remove_tool As Button
        Public btn_move_down As Button
        Public btn_move_up As Button
        Public btn_indent As Button
        Public btn_deindent As Button
        Public btn_run_all As Button
        Public pnl_tools_container As Panel 'v čem budu zobrazovat své nástroje
        Public lbl_collection_info As Label
        Public btn_listeners As Button
    End Structure
    Private ctrl As ctrls

    Friend lastctrl As Control
    Private thisctrl As Control
    'Public t() As Object
    Public parent As cls_tools_manager

    Public Const ACTIVATE as Integer = 0
    Public Const RUN_ALL as Integer = 1
    Public event_listeners(1) As cls_event_listener

    Public Property triggering_event As cls_event_description
        Get
            If event_listeners(RUN_ALL).connections IsNot Nothing Then
                Return event_listeners(RUN_ALL).connections(0).event_dispatcher.event_desc
            End If
        End Get
        Set(value As cls_event_description)
            Do While event_listeners(RUN_ALL).n_connections <> -1
                event_listeners(RUN_ALL).connections(0).delete()
            Loop
            If value IsNot Nothing Then
                event_listeners(RUN_ALL).connect_to_event(value, 0, Me.parent)
            End If
        End Set
    End Property
    Public Property gl_shortcut As cls_keyevent_args
        Get
            If event_listeners(ACTIVATE).connections IsNot Nothing Then
                Return event_listeners(ACTIVATE).connections(0).event_dispatcher.event_desc.key_ev
            End If
        End Get
        Set(value As cls_keyevent_args)
            Dim i as Integer = 0
            'nejprve odstraníme staré
            Do While event_listeners(ACTIVATE).n_connections <> -1 'kolekce nemůže mít lokální kl. zkratku
                event_listeners(ACTIVATE).connections(0).delete()
            Loop
            If value IsNot Nothing Then
                event_listeners(ACTIVATE).connect_to_event(New cls_event_description(EN.evn_FRM_KEY_DOWN, value, Nothing), 0, Me.parent)
            End If
            display_label_on_activating_button()
        End Set
    End Property
    Public Sub display_label_on_activating_button()
        'vypíše název a kl. zkratku na tlačítko v seznamu kolekcí
        If activating_button IsNot Nothing Then
            With activating_button
                If Me.gl_shortcut IsNot Nothing Then
                    .Text = Me.name & vbNewLine & " {" & Me.gl_shortcut.toString & "}"
                    .Height = 45
                Else
                    .Text = Me.name
                    .Height = 30
                End If
            End With
        End If
    End Sub
    Public Sub listener_changed(listener As cls_event_listener)
        If listener.description = event_listeners(ACTIVATE).description Then
            Me.display_label_on_activating_button()
        End If
    End Sub

    Public Sub New(name_ As String, container_ As Object, type_ As String, parent_ As cls_tools_manager,
                   gl_shortcut_ As cls_keyevent_args, triggering_event_ As cls_event_description)

        container = container_
        name = name_
        type = type_

        parent = parent_
        new_base()
        gl_shortcut = gl_shortcut_
        triggering_event = triggering_event_
    End Sub
    Public Sub New()

        name = "Výchozí"
        new_base()
    End Sub
    Public Sub New(name_ As String)
        name = name_
        new_base()
    End Sub
    Private Sub new_base()
        event_listeners(0) = New cls_event_listener(Me, 0, "Aktivace panelu kolekce")
        AddHandler event_listeners(0).connection_changed, AddressOf listener_changed
        event_listeners(1) = New cls_event_listener(Me, 1, "Spuštění všech nástrojů v kolekci")
        AddHandler event_listeners(0).connection_changed, AddressOf listener_changed
    End Sub
    Public Function Raise(p As cls_preXML_section_page, e As Object, mode as Integer) As Object
        If mode = 0 Then
            parent.activate_collection(Me.name)
        ElseIf mode = 1 Then
            run(p, -1)
        End If
    End Function


    Public Sub display(container_ As Object)
        'vypíšeme ovl. prvky kolekce (seznam s nástroji, tlačítka pro přidání/odebrání nástrojů, kl. zkratku apod.)
        If container_ IsNot Nothing Then container = container_
        ctrl.trv_tools = New TreeView
        With NewCtrl(ctrl.trv_tools, New TreeView)
            .Parent = container
            .Left = 5
            .Top = 5
            .Width = container.Width - 35
            .Height = 170

            AddHandler ctrl.trv_tools.AfterSelect, AddressOf trv_tools_AfterSelect
        End With
        list_tools()

        With NewCtrl(ctrl.btn_add_tool, New Button)
            .Parent = container
            .Left = lastctrl.Left + lastctrl.Width
            .Top = lastctrl.Top
            .Width = 25
            .Height = 25
            .Text = "+"
            AddHandler .Click, AddressOf Me.btn_add_tool_Click
        End With
        With NewCtrl(ctrl.btn_remove_tool, New Button)
            .Parent = container
            .Left = lastctrl.Left
            .Top = lastctrl.Top + lastctrl.Height
            .Width = 25
            .Height = 25
            .Text = "-"
            AddHandler .Click, AddressOf Me.btn_remove_tool_Click
        End With

        With NewCtrl(ctrl.btn_move_up, New Button)
            .Parent = container
            .Left = lastctrl.Left
            .Top = lastctrl.Top + lastctrl.Height + 10
            .Width = 25
            .Height = 25
            .Text = "˄"
            AddHandler .Click, AddressOf btn_move_up_Click
        End With
        With NewCtrl(ctrl.btn_move_down, New Button)
            .Parent = container
            .Left = lastctrl.Left
            .Top = lastctrl.Top + lastctrl.Height
            .Width = 25
            .Height = 25
            .Text = "˅"
            AddHandler .Click, AddressOf btn_move_down_Click
        End With
        With NewCtrl(ctrl.btn_indent, New Button)
            .Parent = container
            .Left = lastctrl.Left
            .Top = lastctrl.Top + lastctrl.Height + 10
            .Width = 25
            .Height = 25
            .Text = ">"
            AddHandler .Click, AddressOf cmd_indent_click
        End With
        With NewCtrl(ctrl.btn_deindent, New Button)
            .Parent = container
            .Left = lastctrl.Left
            .Top = lastctrl.Top + lastctrl.Height
            .Width = 25
            .Height = 25
            .Text = "<"
        End With
        With NewCtrl(ctrl.btn_listeners, New Button)
            .Parent = container
            .Left = ctrl.trv_tools.Left
            .Top = ctrl.trv_tools.Top + ctrl.trv_tools.Height
            .AutoSize = True
            .Text = "Nastavit posluchače událostí kolekce (2)..."
            AddHandler .Click, AddressOf btn_listeners_Click
        End With


        With NewCtrl(ctrl.lbl_collection_info, New Label)
            .Parent = container
            .Left = lastctrl.Left
            .Top = lastctrl.Top + lastctrl.Height
            .Text = "Vybraný nástroj:"
            .AutoSize = True
        End With
        With NewCtrl(ctrl.pnl_tools_container, New Panel)
            .Parent = container
            .Top = lastctrl.Top + lastctrl.Height + 10
            .Left = 5
            .Width = container.Width - 10
            .Height = container.Height - .Top - 5
            ctrl.pnl_tools_container.AutoScroll = True
        End With
    End Sub
    Public Sub adapt_control_positions()
        ctrl.trv_tools.Width = container.Width - 35
        ctrl.trv_tools.Height = 170
        ctrl.btn_add_tool.Left = ctrl.trv_tools.Width + ctrl.trv_tools.Left
        ctrl.btn_remove_tool.Left = ctrl.btn_add_tool.Left
        ctrl.btn_move_down.Left = ctrl.btn_add_tool.Left
        ctrl.btn_move_up.Left = ctrl.btn_add_tool.Left
        ctrl.btn_indent.Left = ctrl.btn_add_tool.Left
        ctrl.btn_deindent.Left = ctrl.btn_add_tool.Left
        ctrl.pnl_tools_container.Width = container.Width - 10
    End Sub
    Public Sub dispose()
        With ctrl
            .btn_add_tool = Nothing
            .btn_deindent = Nothing
            .btn_indent = Nothing
            .btn_move_down = Nothing
            .btn_move_up = Nothing
            .btn_remove_tool = Nothing
            .btn_run_all = Nothing
            .lbl_collection_info = Nothing
            .pnl_tools_container = Nothing
            .trv_tools = Nothing
        End With
    End Sub
    Public Sub list_tools()
        'vypíše seznam nástrojů, organizéry se vypisují rekurzivně...
        If ctrl.trv_tools IsNot Nothing Then

            Dim i As Integer
            ctrl.trv_tools.Nodes.Clear()

            For i = 0 To n_tools
                ctrl.trv_tools.Nodes.Add(t_o(i).list_me)
                '.tag = t_o(i).id
            Next
        End If
        ctrl.trv_tools.ExpandAll()
    End Sub
    Public Sub list_tools(parent_node As TreeNode)
        Dim i as Integer
        For i = 0 To n_tools
            parent_node.Nodes.Add(t_o(i).list_me)
        Next
        If parent_node.TreeView IsNot Nothing Then
            parent_node.TreeView.ExpandAll()
        End If
    End Sub
    Public Sub cmd_indent_click(sender As Object, e As EventArgs)

        If ctrl.trv_tools.SelectedNode IsNot Nothing Then
            With ctrl.trv_tools
                If .SelectedNode.Index <> 0 Then
                    Dim t_o As cls_tools_organizer
                    t_o = .Nodes(.SelectedNode.Index - 1).Tag
                    t_o.add_subtool(.SelectedNode.Tag.t)
                    btn_remove_tool_Click(Nothing, Nothing)
                End If

            End With
        End If
    End Sub

    Public Sub btn_add_tool_Click(sender As Object, e As EventArgs)
        frm_add_tool.load_me(Me.parent, Me)
    End Sub
    Public Sub btn_listeners_Click(sender As Object, e As EventArgs)
        'dialogové okno umožňující nastavit spouštěcí události pro nabízené akce (v tomto případě: aktivovat panel a spustit všechny nástroje)
    End Sub
    Public Sub btn_remove_tool_Click(sender As Object, e As EventArgs)
        If ctrl.trv_tools.SelectedNode IsNot Nothing Then
            Dim i as Integer
            If ctrl.trv_tools.SelectedNode.Parent Is Nothing Then 'pokud nejde o podřazený nástroj
                For i = find_tool_index(ctrl.trv_tools.SelectedNode.Text) To n_tools - 1
                    Me.t_o(i) = Me.t_o(i + 1)
                Next
                Me.n_tools -= 1
                list_tools()
                If Me.n_tools > -1 Then
                    ReDim Preserve Me.t_o(Me.n_tools)
                Else
                    Erase Me.t_o
                End If
            End If
        End If
    End Sub
    Public Sub btn_move_up_Click(sender As Object, e As EventArgs)
        'posuneme nástroj výše
        If ctrl.trv_tools.SelectedNode IsNot Nothing Then
            Dim i As Integer
            Dim tname As String
            If ctrl.trv_tools.SelectedNode.Parent Is Nothing Then 'pokud nejde o podřazený nástroj
                i = find_tool_index(ctrl.trv_tools.SelectedNode.Text)
                If i > 0 Then
                    Dim prev As Object
                    prev = Me.t_o(i - 1)
                    Me.t_o(i - 1) = Me.t_o(i)
                    Me.t_o(i) = prev
                    tname = Me.t_o(i - 1).t.name

                    list_tools()
                    select_tool_by_name(tname)
                    ctrl.trv_tools.Select()
                End If
            End If

        End If
    End Sub
    Public Sub btn_move_down_Click(sender As Object, e As EventArgs)
        'posuneme nástroj výše
        If ctrl.trv_tools.SelectedNode IsNot Nothing Then
            Dim i As Integer
            Dim tname As String
            If ctrl.trv_tools.SelectedNode.Parent Is Nothing Then 'pokud nejde o podřazený nástroj
                i = find_tool_index(ctrl.trv_tools.SelectedNode.Text)
                If i < Me.n_tools Then
                    Dim nextt As Object
                    nextt = Me.t_o(i + 1)
                    Me.t_o(i + 1) = Me.t_o(i)
                    Me.t_o(i) = nextt
                    tname = Me.t_o(i + 1).t.name

                    list_tools()
                    select_tool_by_name(tname)
                    ctrl.trv_tools.Select()
                End If
            End If

        End If
    End Sub
    Private Sub select_tool_by_name(tname As String)
        Dim i As Integer
        For i = 0 To ctrl.trv_tools.Nodes.Count - 1
            If ctrl.trv_tools.Nodes.Item(i).Text = tname And ctrl.trv_tools.Nodes.Item(i).Parent Is Nothing Then
                ctrl.trv_tools.SelectedNode = ctrl.trv_tools.Nodes.Item(i)
                Exit For
            End If
        Next
    End Sub
    Public Function add_tool(tool As Object, Optional trigger As cls_tools_organizer = Nothing) As cls_tools_organizer
        If trigger Is Nothing Then
            n_tools += 1
            ReDim Preserve t_o(n_tools)
            t_o(n_tools) = New cls_tools_organizer(tool, Me, n_tools)

            t_o(n_tools).t.parent = t_o(n_tools)

            Return t_o(n_tools)
        End If
    End Function
    Public Function find_tool(tname As String) As cls_tools_organizer
        Dim i as Integer
        For i = 0 To n_tools
            If t_o(i).t.name = tname Or t_o(i).t.name_id = tname Then
                Return t_o(i)
            End If
        Next
    End Function
    Public Function find_tool_nameid(tname_id As String) As cls_tools_organizer
        Dim i as Integer
        For i = 0 To n_tools
            If t_o(i).t.name_id = tname_id Then
                Return t_o(i)
            End If
        Next
    End Function
    Public Function find_tool_index(tname As String) as Integer
        Dim i as Integer
        For i = 0 To n_tools
            If t_o(i).t.name = tname Then
                Return i
            End If
        Next
    End Function
    Public Function find(n As String)
        Return False
    End Function

    Public Sub run(p As cls_preXML_section_page, mode as Integer)
        Dim i as Integer
        For i = 0 To n_tools
            t_o(i).t.run(p, mode)
        Next
    End Sub
    Private Function NewCtrl(ByRef obj_reference As Object, ctrl As Control) As Control
        'pro snazší vkládání ovl. prvků jednoho za druhým (aby se nemuselo při nastavování pozice nově vloženého prvku vždy odkazovat jmenovitě na předchozí,
        'od jehož pozice se pozice nového odvíjí, ale dalo se odkazovat vždy jednoduše na lastctrl)
        obj_reference = ctrl
        lastctrl = thisctrl
        thisctrl = ctrl
        Return ctrl
    End Function
    Public Sub trv_tools_AfterSelect(sender As Object, e As TreeViewEventArgs)
        get_selected_tool().t.create_controls(Me.ctrl.pnl_tools_container, Me.parent.last_vizualized_tool)
    End Sub

    Public Function get_selected_tool() As Object
        Dim i as Integer, j as Integer
        Dim id as Integer
        Dim tmp As cls_tools_organizer
        'najdeme příslušný nástroj podle výběru v treeview
        If ctrl.trv_tools.SelectedNode IsNot Nothing Then
            If ctrl.trv_tools.SelectedNode.Tag IsNot Nothing Then
                Return ctrl.trv_tools.SelectedNode.Tag
            End If
            id = CLng(ctrl.trv_tools.SelectedNode.Tag)
            For i = 0 To Me.n_tools
                tmp = Me.t_o(i).get_by_id(id)
                If tmp IsNot Nothing Then
                    Return tmp
                End If
            Next

            If ctrl.trv_tools.SelectedNode.Parent Is Nothing Then
                'pokud node nemá žádného rodiče, je to nejvyšší úroveň a nemusíme nic složitě hledat - stačí nám index
                Return Me.t_o(ctrl.trv_tools.SelectedNode.Index)
            Else
                i = 0
                Dim a() As Object
                ReDim a(0)
                a(0) = ctrl.trv_tools.SelectedNode
                Do While a(i).parent IsNot Nothing
                    'jinak musíme projet od vybraného nodu výš a výš dokud existuje rodič
                    i = i + 1
                    ReDim Preserve a(i)
                    a(i) = a(i - 1).parent 'a vždy se přesunout na rodiče
                Loop
                'až se dostaneme zase na nejvyšší úroveň
                'Dim tmp As Object
                tmp = Me.t_o(a(i).index)
                For j = i - 1 To 0 Step -1 'a zase začneme v opačném směru podle uložených indexů
                    tmp = tmp.triggered_tools(a(j).index)
                Next
                Return tmp
                'předpokladem samozřejmě je, že v treeview je přesně to, co má být...
            End If
        End If
    End Function

    Public Sub list_my_event_listeners(n As TreeNode)
        Dim n2 As TreeNode
        For j = 0 To Me.event_listeners.Count - 1
            Me.event_listeners(j).list_me(n)
        Next
        For j = 0 To Me.n_tools
            n2 = n.Nodes.Add(Me.t_o(j).t.name)
            n2.Tag = "nástroj"
            n2.ForeColor = Color.DeepPink
            Me.t_o(j).list_my_event_listeners(n2)
        Next
    End Sub
    Public Function all_tools(ByRef arr() As Object) As Object()
        Dim i As Long
        For i = 0 To Me.n_tools
            Me.t_o(i).all_tools(arr)
        Next
    End Function
    Public Sub New(n As Xml.XmlNode, parent_ As Object)
        Me.parent = parent_
        __xml(Nothing, n, False)
    End Sub
    Private Function __xml(x As Xml.XmlDocument, n_imp As Xml.XmlNode, export As Boolean) As Xml.XmlNode
        Dim n As Xml.XmlNode


        If export = False Then
            new_base()
        End If

        If export = True Then
        Else
            Dim nl As Xml.XmlNodeList
            Me.name = get_singlenode_value(n_imp, "name")
            Me.type = get_singlenode_value(n_imp, "type")
            n = n_imp.SelectSingleNode("triggering_event/event_desc")
            If n IsNot Nothing Then
                Me.triggering_event = New cls_event_description(n)
            End If
            n = n_imp.SelectSingleNode("gl_shortcut/key_event_args")
            If n IsNot Nothing Then
                Me.gl_shortcut = New cls_keyevent_args(n)
            End If

            nl = n_imp.SelectNodes("tool_organizer")
            If nl IsNot Nothing Then
                Me.n_tools = nl.Count - 1
                ReDim Me.t_o(n_tools)
                For i = 0 To n_tools
                    t_o(i) = New cls_tools_organizer(nl.Item(i))
                    t_o(i).parent = Me
                Next
            End If
        End If

        If export = True Then Return n
    End Function
    Public Function add_tools(nl As Xml.XmlNodeList)
        Dim i As Long, j As Long, k As Long = 0
        If nl IsNot Nothing Then
            j = Me.n_tools + 1
            Me.n_tools += nl.Count
            ReDim Preserve Me.t_o(n_tools)
            For i = j To Me.n_tools
                t_o(i) = New cls_tools_organizer(nl.Item(k))
                t_o(i).parent = Me
                k+=1
            Next
        End If
    End Function
    Public Function export_to_xml(x As Xml.XmlDocument) As Xml.XmlNode
        Dim e As Xml.XmlNodeType
        e = Xml.XmlNodeType.Element
        Dim i As Long
        Dim o As String
        Dim n As Xml.XmlNode
        n = x.CreateNode(e, "collection", "")
        n.AppendChild(x.CreateNode(e, "name", "")).InnerText = Me.name
        n.AppendChild(x.CreateNode(e, "type", "")).InnerText = Me.type
        If Me.triggering_event IsNot Nothing Then
            n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "triggering_event", "")).AppendChild(Me.triggering_event.export_to_xml(x))
        End If
        If Me.gl_shortcut IsNot Nothing Then
            n.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "gl_shortcut", "")).AppendChild(Me.gl_shortcut.export_to_xml(x))
        End If
        For i = 0 To Me.n_tools
            n.AppendChild(Me.t_o(i).export_to_xml(x))
        Next
        Return n
    End Function

End Class