Imports převaděč_pdf
Public Class cls_flyingtool
    Public tool As Object
    Public description As Object
    Public value As Object
    Public value2 As Object
    Public deactivateOnAnyAction As Boolean 'pokud jde jen o infromační okénko (třeba s infem, jaký tag byl právě uzavřen), 
    'nástroj se zavře při jakémkolikv úderu klávesnice nebo kliknutí
    Public Sub New(parent As Object, description_ As String, value_ As Object, value2_ As Object, Optional deactivateOnAnyAction_ As Boolean = False)
        tool = parent
        description = description_
        value = value_
        value2 = value2_
        deactivateOnAnyAction = deactivateOnAnyAction_
    End Sub
End Class


Public Class cls_DL_connection 'dispatcher<->listener
    Private connection_id_ As Integer
    Public event_listener As cls_event_listener
    Public event_dispatcher As cls_event_dispatcher
    Public connection_type As Integer
    Shared last_id As Integer
    Public Sub New()
        last_id += 1
        connection_id = last_id
    End Sub
    Public Sub dispose()
        event_dispatcher = Nothing
        event_dispatcher = Nothing
    End Sub
    Public Sub delete()
        event_dispatcher.remove_connection(connection_id)
        event_listener.remove_connection(connection_id)
        event_dispatcher = Nothing
        event_listener = Nothing
    End Sub
    Public Property connection_id As Integer
        Get
            connection_id = connection_id_
        End Get
        Set(value As Integer)
            connection_id_ = value
        End Set
    End Property
End Class

Public Class cls_event_dispatcher
    'objekt reagující na nějakou událost - uchovává informace o tom, o jakou událost jde a které objekty na tuto událost čekají
    Public event_desc As cls_event_description
    Public connections() As cls_DL_connection
    Public n_connections As Integer = -1
    Public parent As Object
    Public Sub dispose()
        event_desc = Nothing
        For i = 0 To n_connections
            connections(i).dispose
            connections(i) = Nothing
            parent = Nothing
        Next
    End Sub
    Public Function key_event_desc() As cls_keyevent_args
        'popis události, kterou tento dispečer rozdílí (není to dokonalé - neříká to, jestli keypress, keyup apod)
        Dim tmp As cls_event_description
        If Me.event_desc.ev > EN.first_keyevent And Me.event_desc.ev < EN.last_keyevent Then
            Return Me.event_desc.key_ev
        Else
            Return Nothing
        End If
    End Function

    Public Overrides Function toString() As String
        Return event_desc.toString
    End Function
    Public Function toStringSim() As String
        'popis události, kterou tento dispečer rozdílí (není to dokonalé - neříká to, jestli keypress, keyup apod)
        Return Me.event_desc.toStringSim
    End Function

    Public Sub New(parent_ As Object, event_description_ As cls_event_description)
        parent = parent_
        event_desc = event_description_
    End Sub

    Public Function add_listening_object(listener As cls_event_listener, connection_type As Integer) As Integer
        n_connections += 1
        ReDim Preserve connections(n_connections)
        connections(n_connections) = New cls_DL_connection
        With connections(n_connections)
            '.connection_id = id
            .event_listener = listener
            .event_dispatcher = Me
            .connection_type = connection_type
            listener.add_connection(connections(n_connections))
            Return .connection_id
        End With
    End Function
    Public Sub add_connection(connection As cls_DL_connection)
        'pokud spojení vytvořil posluchač, tímto se jen přidá ke spojení dispečera. Obdbnou funkci má i posluchač
        Dim i As Integer
        For i = 0 To n_connections
            If connections(i).connection_id = connection.connection_id Then
                Exit Sub
            End If
        Next
        n_connections += 1
        ReDim Preserve connections(n_connections)
        connections(n_connections) = connection
    End Sub
    Public Sub remove_connection(id As Integer)
        Dim i As Integer
        For i = 0 To n_connections
            If connections(i).connection_id = id Then
                Dim j As Integer
                connections(i) = Nothing
                For j = i To n_connections - 1
                    connections(j) = connections(j + 1)
                Next
                n_connections -= 1
                If n_connections > -1 Then
                    ReDim Preserve connections(n_connections)
                Else
                    Erase connections
                    'v takovém případě můžeme smazat dispečera úplně (u posluchače to neplatí - posluchači jsou stálí)
                    Me.parent.clean
                End If
            End If
        Next
    End Sub
    Public Function get_connection_by_id(id As Integer) As cls_DL_connection
        Dim i As Integer
        For i = 0 To n_connections
            If connections(i).connection_id = id Then
                Return connections(i)
            End If
        Next
        Return Nothing
    End Function
    Public Sub dispatch(p As cls_preXML_section_page, e As Object)
        Dim i As Integer
        For i = 0 To n_connections
            connections(i).event_listener.raise(p, e)
        Next
    End Sub
End Class

Public Class cls_event_listener

    'objekt, který bude součástí tříd, které mohou poslouchat a reagovat na nějaké události 
    '(např. nástrojů). Každý takový objekt bude moci mít více těchto posluchačů, který bude každý vyvolávat jinou reakci
    'např. jeden posluchač nástroj pouze aktvuje, druhý spustí jednu jeho funkcionalitu, další třeba jinou.
    Public parent As Object
    'Private tm As cls_tools_manager
    Public mode As Integer

    Public description As String

    Public event_restriction As Integer '0=vše, 1=jen události klávesnice, 2=jen události myši, 3=1+2, 4=jen události dokumentu


    Public n_connections As Integer
    Public connections() As cls_DL_connection

    Public Event connection_changed(sender As cls_event_listener)

    Public Sub raise(p As cls_preXML_section_page, e As Object)
        parent.raise(p, e, Me.mode)
    End Sub

    Public Sub New(parent_ As Object, mode_ As Integer, description_ As String, Optional event_restriction_ As Integer = 0)
        parent = parent_
        mode = mode_
        description = description_
        event_restriction = event_restriction_
        n_connections = -1
    End Sub

    Public Sub connect_to_event(event_desc As cls_event_description, connection_type As Integer, Optional tm As cls_tools_manager = Nothing)

        If tm IsNot Nothing Then ' env.wsp.tm: tools_manager bude prostě jen jeden...
            tm.add_event_listener(Me, event_desc)
        Else
            env.wsp.tm.add_event_listener(Me, event_desc)
        End If
        RaiseEvent connection_changed(Me)
    End Sub

    Public Sub add_connection(connection As cls_DL_connection)
        'přidá spojení, které vytvořil dispečer - ten ho pošle tomuto objektu, aby si ho přidal sám do sebe
        Dim i As Integer
        For i = 0 To n_connections 'zkontrolujeme, jestli už tu toto propojení není
            If connections(i).connection_id = connection.connection_id Then Exit Sub
        Next
        n_connections += 1
        ReDim Preserve connections(n_connections)
        connections(n_connections) = connection

        RaiseEvent connection_changed(Me)
    End Sub
    Public Sub remove_all_connections()
        n_connections = -1
        Erase connections
    End Sub
    Public Sub remove_connection(id As Integer)
        Dim i As Integer
        For i = 0 To n_connections
            If connections(i).connection_id = id Then
                connections(i) = Nothing
                Dim j As Integer
                For j = i To n_connections - 1
                    connections(j) = connections(j + 1)
                Next
                n_connections -= 1
                If n_connections > -1 Then
                    ReDim Preserve connections(n_connections)
                Else
                    Erase connections
                End If
            End If
        Next
        RaiseEvent connection_changed(Me)
    End Sub
    Public Sub list_me(parent_n As TreeNode)
        Dim i As Integer
        Dim tmp As TreeNode
        Dim tmp2 As TreeNode
        tmp = parent_n.Nodes.Add(Me.description)
        tmp.Tag = Me
        tmp.ForeColor = Color.Gray
        For i = 0 To Me.n_connections
            tmp2 = tmp.Nodes.Add(Me.connections(i).connection_id, Me.connections(i).event_dispatcher.toString & " (" & Me.connections(i).connection_id & ")")
            tmp2.ForeColor = Color.DodgerBlue
            tmp2.Tag = Me.connections(i)
        Next
    End Sub

End Class
Public MustInherit Class cls_events_handling
    Friend event_dispatchers() As cls_event_dispatcher
    Friend n_edisp As Integer = -1



    Public Function add_event_listener(listening_object As Object, event_description_ As cls_event_description) As Integer
        'přídáme dispečerovi událostí nový poslouchající objekt, a pokud požadovaný dispečer neexistuje, vytvoříme ho
        Dim i As Integer
        Dim connection_id As Integer
        Dim found As Boolean
        For i = 0 To n_edisp
            With event_dispatchers(i)
                If .event_desc.ev = event_description_.ev Then
                    If event_description_.ev > EN.first_keyevent And event_description_.ev < EN.last_keyevent Then
                        If .event_desc.key_ev.value = event_description_.key_ev.value And .event_desc.key_ev.alt = event_description_.key_ev.alt And
                            .event_desc.key_ev.ctrl = event_description_.key_ev.ctrl And .event_desc.key_ev.shift = event_description_.key_ev.shift Then
                            'connection_id += 1
                            connection_id = .add_listening_object(listening_object, 0)
                            Return connection_id
                        End If
                    ElseIf event_description_.ev > EN.first_mouseevent And event_description_.ev < EN.last_mouseevent Then
                        If .event_desc.mouse_ev.button = event_description_.mouse_ev.button And .event_desc.mouse_ev.alt = event_description_.mouse_ev.alt And
                            .event_desc.mouse_ev.ctrl = event_description_.mouse_ev.ctrl And .event_desc.mouse_ev.shift = event_description_.mouse_ev.shift Then
                            'connection_id += 1
                            connection_id = .add_listening_object(listening_object, 0)
                            Return connection_id
                        End If
                    Else 'If event_description_.ev > EN.first_mouseevent And event_description_.ev < EN.last_mouseevent Then
                        'connection_id += 1
                        connection_id = .add_listening_object(listening_object, 0)
                        Return connection_id
                    End If
                End If
            End With
        Next
        n_edisp += 1
        ReDim Preserve event_dispatchers(n_edisp)
        event_dispatchers(n_edisp) = New cls_event_dispatcher(Me, event_description_)

        connection_id = event_dispatchers(n_edisp).add_listening_object(listening_object, 0)
        Return connection_id
    End Function
    Public Sub clean()
        'smaže všechny prázdné dispečery
        Dim i As Integer
        Dim j As Integer
        Do While i <= n_edisp
            If event_dispatchers(i).n_connections = -1 Then
                event_dispatchers(i) = Nothing
                For j = i To Me.n_edisp - 1
                    event_dispatchers(j) = event_dispatchers(j + 1)
                Next
                Me.n_edisp -= 1
                i -= 1
            End If
            i += 1
        Loop
        If Me.n_edisp > -1 Then
            ReDim Preserve Me.event_dispatchers(Me.n_edisp)
        Else
            Erase Me.event_dispatchers
        End If
    End Sub
    Friend Overridable Sub general_event_triggered(ev As Integer, p As cls_preXML_section_page, e As Object)
        'samo o sobě nic...
    End Sub
    Friend Sub event_dispatching(ev As Integer, p As cls_preXML_section_page, e As Object)
        Dim i As Integer

        'tady se zpracují některé "systémové" zkratky, tj. takové, které jdou mimo nástroje
        If ev = EN.evn_FRM_KEY_DOWN Then
           
        End If

        For i = 0 To n_edisp
            'tato funkce pak najde - existuje-li takový - odpovídajícího dispečera funkce s modifikátory (tj. třeba stisknutí klávesy+CTRL apod)
            'a předá vše dál jednotlivým poslouchajícím objektům
            With event_dispatchers(i)
                ' xyz = EN.evn_RTB_SELECTION_CHANGED
                If .event_desc.ev = ev Then
                    If e Is Nothing Then
                        'žádné dodatečné eventsarguments (=e)
                        .dispatch(p, e)
                    ElseIf e.GetType = GetType(myEventArgs) Then
                        'Dim ctrl as Integer = e.k = Keys.Controlx
                        If e.e.GetType = GetType(KeyEventArgs) Then
                            If .event_desc.key_ev.value = e.e.keyvalue And .event_desc.key_ev.ctrl = e.e.control And
                                .event_desc.key_ev.shift = e.e.shift And .event_desc.key_ev.alt = e.e.Alt Then
                                'vše odpovídá... 
                                e.e.SuppressKeyPress = True
                                .dispatch(p, e)
                            End If
                        ElseIf e.e.GetType = GetType(KeyPressEventArgs) Then
                            If .event_desc.key_ev.value = e.e.keychar And .event_desc.key_ev.ctrl = e.e.control And
                                .event_desc.key_ev.shift = e.e.shift And .event_desc.key_ev.alt = e.e.Alt Then
                                'vše odpovídá... 
                                e.e.SurpressKeyPress = True
                                .dispatch(p, e)
                            End If
                        ElseIf e.e.GetType = GetType(MouseEventArgs) Then
                            If .event_desc.mouse_ev.button = e.e.button And .event_desc.mouse_ev.ctrl = e.ctrl And .event_desc.mouse_ev.shift = e.shift And
                                .event_desc.mouse_ev.alt = e.Alt Then
                                'vše odpovídá... 
                                .dispatch(p, e)
                            End If
                        End If
                    End If
                End If
            End With
        Next
    End Sub
End Class

Public Class cls_tools_manager
    Inherits cls_events_handling
    Public collections() As cls_tools_collection
    Public active_collection As cls_tools_collection
    Public last_vizualized_tool As Object
    Public n_coll As Integer = -1
    Public wsp As cls_workspace
    Public container As Control
    Public pnl_coll_container As Panel
    Public rbtn_collections() As RadioButton
    Public rbtn_add_collection As RadioButton

    Private thisctrl As Control
    Private lastctrl As Control
    Private Const TC_BASIC_COLLECTION As String = "basic_collection"
    Private Const TC_NORMAL_COLLECTION As String = "normal_collection"

    Shared cls_id As Long
    Private id As Long

    Public cmd_manage_events As Button
    Public Sub dispose()
        Dim i As Long
        For i = 0 To n_coll
            collections(i).dispose()
            collections(i) = Nothing
        Next
        Erase collections
        n_coll = -1
        For i = 0 To n_edisp
            event_dispatchers(i).dispose()
            event_dispatchers(i) = Nothing
        Next
        Erase event_dispatchers
        n_edisp = -1
        destroy_controls()
        active_collection = Nothing
    End Sub
    Public Sub New(workspace As cls_workspace)
        cls_id += 1
        id = cls_id
        Me.wsp = workspace
        Me.wsp.tm = Me
        container = wsp.tools_container
        AddHandler workspace.event_triggered, AddressOf general_event_triggered
        n_edisp = -1
        n_coll = -1
        Exit Sub
        '###### později vyřešit nahráváním uložených WSP
        add_collection("Všechny nástroje", TC_BASIC_COLLECTION, New cls_keyevent_args(Keys.F1, True))
        Dim coll_po_v As cls_tools_collection = add_collection("Po vložení 1", , New cls_keyevent_args(Keys.F2, True), New cls_event_description(EN.evn_TEXT_INSERTED, Nothing, Nothing))
        Dim coll_cr_app As cls_tools_collection = add_collection("Krititický ap.",, New cls_keyevent_args(Keys.F3, True))
        Dim coll_bibl As cls_tools_collection = add_collection("Bibliograf. pozn.",, New cls_keyevent_args(Keys.F4, True))
        Dim coll_notes As cls_tools_collection = add_collection("Věcné poznámky",, New cls_keyevent_args(Keys.F5, True))
        add_collection(env.c("Vkládání tagů"))
        '#######
        load_default_tools()
        'add_event_listener(get_basic_collection(), EN.evn_RTB_KEY_DOWN, 112, True)

        'get_basic_collection().add_event_listener(get_basic_collection.t_o.Last, EN.evn_RTB_KEY_DOWN, 113)
        With coll_po_v
            .add_tool(get_basic_collection.find_tool("<<REPL").clone)
            .add_tool(get_basic_collection.find_tool(">>REPL").clone)
            '.add_tool(get_basic_collection.find_tool("<REPL").clone)
            '.add_tool(get_basic_collection.find_tool(">REPL").clone)
            .add_tool(get_basic_collection.find_tool("STRANGE1").clone)
            .add_tool(get_basic_collection.find_tool("STRANGE_COMB").clone)
            .add_tool(New cls_tool_group_replacing("GRREPL", "Hromadné nahrazení", "Seznam slov, která budou nahrazena při vložení textu", "~grrepl", "",
                                                   New cls_highligh_rule("bc:green3")))
            .add_tool(New cls_tool_Wordlist("WLIST", "Kontrola slov", "Zkontroluje, zda se slova vyskytují v seznamu TT", "~not_in_wortlist",
                                           "C:\Users\jctibor\Documents\FLU\latin-wordlist.txt", True, New cls_highligh_rule("bc:red3")))
            Dim st As Object
            st = get_basic_collection.find_tool("LB_INS").t.clone
            'xyz = get_basic_collection.find_tool("RM_LN")
            xyz = .add_tool(get_basic_collection.find_tool("RM_LN").clone)
            xyz.add_subtool(st)

        End With
        Dim tmp As cls_tools_organizer
        With coll_cr_app

            tmp = .add_tool(New cls_tool_MarkSelection("MARK_CRIT", "Označ krit. ap.", "Označí vybranou oblast jako kritický aparát", "~critical_app", True,
                                                 New cls_highligh_rule("bc:lblue3"),
                                                  New cls_keyevent_args(Keys.C,, True)))
            tmp.add_subtool(New cls_tool_Notes("SPLIT_CA", "Rozdělí kr. ap.", "Extrahuje jednotlivé poznámky kr. aparátu", "~split_ca", "~critical_app", "(\-)",
                                               "app", "^(?:<app>)?\s*[A-Z]?\s*(?<LN>[0-9]+)?\s*(?<LEMMA>[^\]]+)\](?<RDG>[^\]]*)\s(?<WIT>F)\s*(?:</app>)?$",
                                               "<app><lemma ln='$LN'>$LEMMA</lemma><rdg wit='$WIT'>$RDG</rdg></app>", "<app>$ALL</app>", "lemma"))
        End With

        With coll_bibl
            tmp = .add_tool(New cls_tool_MarkSelection("MARK_BIBL", "Označ bibl.", "Označí vybranou oblast jako bibliografii", "~bibl", True,
                                                       New cls_highligh_rule("bc:yellow3"),
                                                       New cls_keyevent_args(Keys.B,, True)))
            tmp.add_subtool(New cls_tool_Notes("SPLIT_CIT", "Rozdělí bibliogr.", "Extrahuje jednotlivé bibliografické poznámky.", "~split_bibl", "~bibl",
                                               "\([0-9Il]+\)", "cit", "^\s*(?:<cit>)?(?<BIBL>[^<]*)(?:</cit>)?\s*$",
                                               "<cit><quote></quote><bibl>$BIBL</bibl></cit>", "<cit><quote></quote><bibl>$ALL</bibl></cit>", "quote"))
        End With
        With coll_notes
            tmp = .add_tool(New cls_tool_MarkSelection("MARK_NOTES", "Označ pozn.", "Označí oblast jako poznámky", "~notes", True,
                                                       New cls_highligh_rule("bc:green3"),
                                                       New cls_keyevent_args(Keys.N,, True)))
            tmp.add_subtool(New cls_tool_Notes("SPLIT_NOT", "Rozděl poznámky", "Usnadňuje rozdělení, transformaci a umisťování poznámek (věcných nebo 'obecných').",
                                               "~split_notes", "~notes", "(\-)", "app", "^\s*(?:<cit>)?(?<NOTE>[^<]*)(?:</cit>)?\s*$",
                                               "<app><lemma></lemma><note>$NOTE</note></app>", "<app><lemma></lemma><note>$ALL</note></app>", "app"))
        End With

        display()
    End Sub
    Public Sub load_default_tools()
        Dim basic_collection As cls_tools_collection
        basic_collection = Me.get_basic_collection
        With basic_collection
            .add_tool(New cls_tool_PlainText_replacement(False, "&lt;&lt;", "«", True, True, "<<REPL", "Nahrazení sekvence '<<' znakem uvozovek «",
                                                             "Znak uvozovek « bývá často při OCR převodu nahrazen dvěma znaménky <",
                                                             "~rep_arrqm", 2, 1,,, New cls_highligh_rule("fc:red3")))

            .add_tool(New cls_tool_PlainText_replacement(False, "&gt;&gt;", "»", True, True, ">>REPL", "Nahrazení sekvence '>>' znakem uvozovek »",
                                                                  "Znak uvozovek » bývá často při OCR převodu nahrazen dvěma znaménky >",
                                                                  "~rep_arrqm", 2, 1,,, New cls_highligh_rule("fc:red3")))
            .add_tool(New cls_tool_PlainText_replacement(True, "(\s*[0-9]*[05])\s*(?:\n|$)", "\n", True, False, "NUM_ON_END", "Vyznačení čísel řádků na konci řádků",
                                                                 "Vyznačí (a odstraní) čísla řádků, která zůstala v textu na konci řádků a je potřeba je odstranit",
                                                                 "~line_numbers_end_of_line", 2, 0,, True))

            .add_tool(New cls_tool_PlainText_replacement(True, "(?:^|\n)\s*[0-9]*[05]\s*", "\n", True, False, "NUM_ON_BEG", "Vyznačení čísel řádků na začátku řádků",
                                                                "Vyznačí (a odstraní) čísla řádků, která zůstala v textu na začátku řádků a je potřeba je odstranit",
                                                                "~line_numbers_start_of_line", 2, 0,, True))

            .add_tool(New cls_tool_PlainText_replacement(True, "[\{\}\^%~§]", "\n", False, False, "STRANGE1", "Vyznačení zvláštních znaků",
                                                                "Vyznačí zvláštní znaky ({, }, ^, %...), které mohou být chybou v OCR",
                                                                "~strange_symbols", 2, 0))

            .add_tool(New cls_tool_PlainText_replacement(True, "(?:lI+|l{3,}|[a-z][I][a-z]|[1!][a-zA-Z]+)", "\n", False, False, "STRANGE_COMB", "Vyznačení zvláštních kombinací znaků",
                                                                "Vyznačí zvláštní kombinace znaků, které mohou být výsledkem obvyklých chyb v OCR (záměna l, I, 1 atd.)",
                                                                "~strange_combinations", 2, 0,,, New cls_highligh_rule("bc:red3")))

            'cls_tool_insert_lb_tags
            .add_tool(New cls_tool_Insert_lb_tags("LB_INS", "Vložení tagů <lb n='#'/>",
                                                          "Provede automatické vložení a očíslování tagů označujících zlom řádků, případně tyto tagy přečísluje",
                                                          "~LB_tag_added",,,,,, 1))

            .add_tool(New cls_tool_Tags_Insertion("MAN_TAG_INS", env.c("Vkládání tagů"), "Umožňuje snadné vkládání rozličných XML tagů", ""))
            .add_tool(New cls_tool_Remove_line_numbers("RM_LN", "Odstranění čísel řádků", "Odstraní čísla řádků na koncích nebo začátcích řádků", "~remove_ln", 5, True))

            '.add_tool(New cls_tool_Wordlist("WLIST", "Kontrola slov", "Zkontroluje, zda se slova vyskytují v seznamu TT", "~not_in_wortlist",
            '                                "C:\Users\jctibor\Documents\FLU\parses.txt", True, New cls_highligh_rule("bc:red3")))

            .add_tool(New cls_tool_MarkSelection("REMOVE_MARK", "Odstraň všechny značky", "Vyčistí vybranou oblast.", "CLEANUP", False, Nothing,
                                                       New cls_keyevent_args(Keys.Escape,, True)))
        End With
    End Sub
    Friend Overrides Sub general_event_triggered(ev As Integer, p As cls_preXML_section_page, e As Object)
        MyBase.event_dispatching(ev, p, e) 'základní funkce, která obslouží dispečery funkcí přímo v tomto objektu
        If active_collection IsNot Nothing Then
            active_collection.event_dispatching(ev, p, e) 'a pak ještě pošleme událost právě aktivované kolekci nástrojů, která může mít nějaké své lokální zkratky...
        End If
    End Sub
    Public Function add_collection(cname As String, Optional c_type As String = TC_NORMAL_COLLECTION,
                                   Optional shortcut As cls_keyevent_args = Nothing, Optional launching_event_ As cls_event_description = Nothing) As cls_tools_collection
        'přidání položky do kolekce nástrojů - jméno musí být unikátní, slouží jako identifikátor
        n_coll += 1
        If cname = "" Then cname = "Kolekce nástrojů" & (n_coll + 1)
        ReDim Preserve collections(n_coll)
        collections(n_coll) = New cls_tools_collection(cname, pnl_coll_container, c_type, Me, shortcut, launching_event_)
        Return collections(n_coll)
    End Function
    Public Sub remove_collection(index As Integer)
        'a odebrání... - podle indexu
        Dim i As Integer
        For i = index To n_coll - 1
            collections(i) = collections(i + 1)
        Next
        n_coll -= 1
        If n_coll > -1 Then
            ReDim Preserve collections(n_coll)
        Else
            Erase collections
        End If
    End Sub
    Public Sub remove_collection(cname As String)
        'podle jména
        Dim i As Integer
        For i = 0 To n_coll
            If collections(i).name = cname Then
                remove_collection(i)
                Exit Sub
            End If
        Next
    End Sub
    Public Function get_collection(cname As String) As cls_tools_collection
        Dim i As Integer
        For i = 0 To n_coll
            If LCase(collections(i).name) = LCase(cname) Then
                Return collections(i)
            End If
        Next
    End Function
    Public Function get_basic_collection() As cls_tools_collection
        'basic_collection je výchozí kolekce, která obsahuje všechny nástroje
        Dim i As Integer
        For i = 0 To n_coll
            If LCase(collections(i).type) = LCase(TC_BASIC_COLLECTION) Then
                Return collections(i)
            End If
        Next
    End Function
    Public Sub destroy_controls()
        Dim n As Integer
        If rbtn_collections IsNot Nothing Then 'ať máme jistotu, že sažeme opravdu celé to pole
            n = rbtn_collections.Count - 1
        Else
            n = -1
        End If

        For i = 0 To n
            rbtn_collections(i).Dispose()
            rbtn_collections(i) = Nothing
        Next
        If rbtn_add_collection IsNot Nothing Then
            rbtn_add_collection.Dispose()
            rbtn_add_collection = Nothing
        End If
        If pnl_coll_container IsNot Nothing Then
            pnl_coll_container.Dispose()
            pnl_coll_container = Nothing
        End If
        If cmd_manage_events IsNot Nothing Then
            cmd_manage_events.Dispose()
            cmd_manage_events = Nothing
        End If

        Erase rbtn_collections

    End Sub
    Public Sub display()
        'vykreslíme správce nástrojů i s kolekcemi a nátroji...
        Dim i As Integer, j As Integer

        destroy_controls()
        ReDim rbtn_collections(n_coll)
        For i = 0 To n_coll
            rbtn_collections(i) = New RadioButton
            With rbtn_collections(i) 'svislý seznam záložek. Standartní objekt tab umí záložky jen vodorovně, nebo svisle, ale i se svislými nápisy (a to nechci)


                .Parent = container
                .Appearance = Appearance.Button
                collections(i).activating_button = rbtn_collections(i)
                collections(i).display_label_on_activating_button()


                .AutoEllipsis = True
                .Width = 150
                .BackColor = Color.White
                .FlatStyle = FlatStyle.Flat
                .FlatAppearance.MouseOverBackColor = Color.FromArgb(216, 234, 249) 'SystemColors.ActiveCaption  'SystemColors.ControlDark
                .FlatAppearance.CheckedBackColor = SystemColors.Control
                .FlatAppearance.BorderSize = 0
                .Left = 5
                .Tag = collections(i)
                If i <> 0 Then
                    .Top = rbtn_collections(i - 1).Top + rbtn_collections(i - 1).Height
                Else
                    .Top = 5
                End If
                AddHandler .MouseClick, AddressOf collection_selected
            End With

        Next
        rbtn_add_collection = New RadioButton
        With rbtn_add_collection
            .Parent = container
            .Appearance = Appearance.Button
            .Width = 150
            .BackColor = Color.White
            .FlatStyle = FlatStyle.Flat
            .FlatAppearance.MouseOverBackColor = Color.FromArgb(216, 234, 249) 'SystemColors.ActiveCaption  'SystemColors.ControlDark
            .FlatAppearance.CheckedBackColor = SystemColors.Control
            .FlatAppearance.BorderSize = 0
            .Left = 5
            .Text = "+ (nová kolekce)"
            .Font = New Font(.Font, FontStyle.Italic)
            AddHandler .MouseClick, AddressOf rbtn_add_collection_click
            If Me.n_coll <> -1 Then
                .Top = rbtn_collections(n_coll).Top + rbtn_collections(n_coll).Height + 10
            Else
                .Top = 5
            End If
        End With

        pnl_coll_container = New Panel
        With pnl_coll_container
            If rbtn_collections IsNot Nothing And UBound(rbtn_collections) >= 0 Then
                .Left = rbtn_collections(0).Left + rbtn_collections(0).Width - 1
            Else
                .Left = 5
            End If
            '.Dock = DockStyle.Right
            .Top = 5
            .BorderStyle = BorderStyle.FixedSingle
            .Width = (container.Width - .Left()) - 5
            .Height = (container.Height - 27 - .Top) ' - 45
            .BackColor = SystemColors.Control
            .Parent = container
            .BringToFront()
            AddHandler .SizeChanged, AddressOf pnl_coll_container_SizeChanged
        End With
        'container.Height = CInt(container.Parent.DisplayRectangle.Height - container.Parent.DisplayRectangle.Y) '- container.Top
        cmd_manage_events = New Button
        With cmd_manage_events
            .Parent = container
            .Top = pnl_coll_container.Top + pnl_coll_container.Height
            .Left = 5

            .Text = "Spravovat dispečery a posluchače událostí"
            .AutoSize = True
            AddHandler .Click, AddressOf cmd_manage_events_Click
        End With

    End Sub
    Private Sub rbtn_add_collection_click(sender As Object, e As EventArgs)
        Dim name As String
        name = InputBox("Zadejte jméno kolekce")
        If name <> "" Then
            Me.add_collection(name)
            Me.active_collection = Me.collections(Me.n_coll)
            Me.display()
        End If
    End Sub
    Private Sub cmd_manage_events_Click(sender As Object, e As EventArgs)
        frm_dispatchers.loadme(Me)
    End Sub
    Private Sub pnl_coll_container_SizeChanged(sender As Object, e As EventArgs)
        'active_collection.adapt_control_positions()
    End Sub
    Private Sub collection_selected(sender As Object, e As EventArgs)
        'Me.wsp.frm.text = Me.wsp.frm.controls.count

        activate_collection(sender.tag.name)
    End Sub
    Public Sub activate_collection(cname As String)
        Dim i As Integer
        Me.pnl_coll_container.Width = pnl_coll_container.Parent.Width - pnl_coll_container.Left - 5

        For i = 0 To n_coll 'nejprve odstraníme starou...
            If collections(i).name = Me.pnl_coll_container.Tag Then
                collections(i).dispose()
                pnl_coll_container.Controls.Clear()
            End If
        Next

        For i = 0 To n_coll 'a pak zobrazíme novou...
            If collections(i).name = cname Then
                collections(i).display(pnl_coll_container)
                pnl_coll_container.Tag = collections(i).name
                rbtn_collections(i).Checked = True
                active_collection = collections(i) 'abychom věděli, s čím právě pracujeme...
                Exit Sub
            End If
        Next
    End Sub

    Public Function all_tools() As Object()
        Dim i As Long
        Dim arr() As Object
        For i = 0 To n_coll
            collections(i).all_tools(arr)
        Next
        Return arr
    End Function
    Public Sub New(x As Xml.XmlDocument, workspace As cls_workspace)
        cls_id += 1
        id = cls_id
        Me.wsp = workspace
        Me.wsp.tm = Me
        container = wsp.tools_container
        AddHandler workspace.event_triggered, AddressOf general_event_triggered
        n_edisp = -1

        __xml(x, x.SelectSingleNode("tools_manager"), False)
        display()
        Exit Sub
        With Me.add_collection("IF 1", "", Nothing, New cls_event_description(EN.evn_FRM_KEY_DOWN, New cls_keyevent_args(Keys.A, False, True, False), Nothing))
            Dim t As cls_tools_organizer
            t = .add_tool(New cls_tool_IF("podmínka_1", "IF1", "", "", Nothing,
                                          New cls_tool_IF.scls_condition("@type=""dipl_app""", True, cls_tool_IF.OPERATORS.CONTAINS), cls_tool_IF.OBJECTS.AC_P_CONTEXT_OPENED_ELEMENTS,
                                          "msg Nelze otevřít nový elment div s @type=""dipl_app"" uvnitř jiného takového elementu."))
            Dim t2 As cls_tools_organizer
            t2 = t.add_subtool(New cls_tool_MarkSelection("Označ jako dipl_app", "MARK_DIPLAPP", "dipl_app", "", New cls_highligh_rule("bc:yellow2")))
            t2.add_subtool(New cls_tool_Notes("Vlož tag", "INSERT_DIPL_APP", "", "", "dipl_app", "", "", "", "<div type=""dipl_app"">\n", "", "", False, False, False))
        End With

    End Sub

    Private Function __xml(x As Xml.XmlDocument, n_imp As Xml.XmlNode, export As Boolean) As Xml.XmlNode
        If export = False Then
            Dim nl As Xml.XmlNodeList
            nl = n_imp.SelectNodes("tools_collections/collection")
            If nl IsNot Nothing Then
                Dim i As Long
                n_coll = nl.Count - 1
                ReDim Me.collections(n_coll)
                For i = 0 To n_coll
                    Me.collections(i) = New cls_tools_collection(nl.Item(i), Me)
                Next
            End If
        End If
    End Function

    Public Function export_to_xml() As Xml.XmlDocument
        Dim x As Xml.XmlDocument
        x = New Xml.XmlDocument
        With x.AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "tools_manager", ""))
            Dim c As Xml.XmlNode
            c = .AppendChild(x.CreateNode(Xml.XmlNodeType.Element, "tools_collections", ""))
            Dim i As Long
            For i = 0 To n_coll
                c.AppendChild(Me.collections(i).export_to_xml(x))
            Next
        End With
        Return x
    End Function
    Public Sub add_tools(x As Xml.XmlDocument)
        Dim nl As Xml.XmlNodeList
        nl = x.SelectNodes("//collection")
        If nl IsNot Nothing Then
            For i = 0 To nl.Count - 1
                Dim cn As String
                cn = get_singlenode_value(nl.Item(i), "name")
                If cn <> "" Then
                    If Me.get_collection(cn) IsNot Nothing Then
                        Dim c As cls_tools_collection
                        c = Me.get_collection(cn)
                        Dim tnl As Xml.XmlNodeList
                        tnl = nl(i).SelectNodes("tool_organizer")
                        c.add_tools(tnl)
                    Else
                        n_coll += 1
                        ReDim Preserve collections(n_coll)
                        collections(n_coll) = New cls_tools_collection(nl.Item(i), Me)
                    End If
                End If
            Next
            Me.display()
        End If
    End Sub
    Public Function evoke(f As String, p As cls_preXML_section_page) As Boolean
        Try
            Dim x As Xml.XmlDocument, i As Integer
            x.LoadXml(f)
            Dim t_nameid As String = get_singlenode_value(x.FirstChild, "t_nameid")
            Dim f_name As String = get_singlenode_value(x.FirstChild, "f_name")
            Dim suppress_triggering As String = get_singlenode_value(x.FirstChild, "suppress_triggering")
            Dim nl As Xml.XmlNodeList
            nl = x.FirstChild.SelectNodes("param")
            Dim params() As Object
            If nl IsNot Nothing Then
                Dim dtype As String
                If nl.Count > 0 Then ReDim params(nl.Count - 1)
                Try
                    For i = 0 To nl.Count - 1
                        If nl(i).Attributes IsNot Nothing Then If nl(i).Attributes.GetNamedItem("dtype") IsNot Nothing Then dtype = nl(i).Attributes.GetNamedItem("dtype").InnerText
                        If dtype = "string" Then
                            params(i) = CStr(nl(i).InnerText)
                        ElseIf dtype = "boolean" Then
                            params(i) = CBool(nl(i).InnerText)
                        ElseIf dtype = "integer" Then
                            params(i) = CInt(nl(i).InnerText)
                        ElseIf dtype = "double" Then
                            params(i) = CDbl(nl(i).InnerText)
                        End If
                Next
                    evoke(t_nameid, f_name, p, suppress_triggering, params)
                Catch
                Return False
            End Try
        End If
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function evoke(t_nameid As String, fname As String, p As cls_preXML_section_page, suppress_triggering As Boolean, ParamArray params() As Object)
        Dim at As Object()
        at = Me.all_tools
        Dim i As Long
        If at IsNot Nothing Then
            For i = 0 To UBound(at)
                If t_nameid <> "" Then
                Else
                    If at(i).has_function(fname) = True Then
                        at(i).raise_function(fname, p, suppress_triggering, params)
                    End If
                End If
            Next
        End If
    End Function

End Class