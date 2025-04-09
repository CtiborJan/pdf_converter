Imports System.Text.RegularExpressions
Imports System.Xml
Public Class cls_notes_setting
    Public name As String
    Public description As String
    Public transformation_pattern As String
    Public no_luck_transformation_pattern As String
    Public bind_pattern As String
    Public inner_pattern As String
    Public default_anchor As String
    Public Sub New(name_ As String, description_ As String, inner_pattern_ As String, transformation_pattern_ As String, no_luck_transformation_pattern_ As String,
                   bind_pattern_ As String)
        name = name_
        description = description_
        transformation_pattern = transformation_pattern_
        no_luck_transformation_pattern = no_luck_transformation_pattern_
        bind_pattern = bind_pattern_
        inner_pattern = inner_pattern_
        default_anchor = "*"
    End Sub

End Class

Public Class cls_environment_2
    Public wsp As cls_workspace
    Public _p As cls_preXML_section_page
    Public opened_document As cls_preXML_document
    Public frm As Object

    Public settings_path As String
    Public default_workspace_path As String
    Public default_folder_path As String

    Public last_opened(10) As String

    Public cmd_add_page As Button
    Public ctrl_container As Object

    Friend lastctrl As Control
    Private thisctrl As Control

    Public Event on_document_opened(ByRef doc As cls_preXML_document, page_to_open_on As Integer)

    Public cmd_save_all As Button

    Public lbl_pdf As Label
    Public txt_pdf As TextBox
    Public cmd_load_pdf As Button

    Public lbl_docname As Label
    Public txt_docname As TextBox

    Public lbl_path As Label
    Public txt_path As TextBox
    Public cmd_select_path As Button

    Public mstr As MenuStrip
    Public mnu_main As ToolStripMenuItem
    Public mnu_new_document As ToolStripMenuItem
    Public mnu_open_document As ToolStripMenuItem
    Public mnu_last_opened As ToolStripMenuItem
    Public mnu_close_document As ToolStripMenuItem
    Public mnu_save_all As ToolStripMenuItem
    Public mnu_wsp As ToolStripMenuItem
    Public mnu_wsp_save As ToolStripMenuItem
    Public mnu_export_to_xml As ToolStripMenuItem
    Public mnu_env_lang As ToolStripMenuItem


    Public mnu_end As ToolStripMenuItem

    Public txt_pdf_index_of_first_page As TextBox
    Public txt_pages_on_one_pdf_page As TextBox
    Public rbts_env_sizes(2) As RadioButton

    Public rtb_font_size As Integer
    Public def_font As Font
    Private environment_size_ As Single = 1
    Public lang As String = "cz"

    Public Sub New(frm_ As Object)
        settings_path = Application.StartupPath & "\settings.xml"
        frm = frm_
        ctrl_container = frm.get_env_container
        rtb_font_size = 24
        def_font = New Font("Calibri", 11.25)
        environment_size = 1
    End Sub

    Public Sub open_document(path As String)
        If My.Computer.FileSystem.FileExists(path) = True Then
            Dim xml As Xml.XmlDocument
            xml = New XmlDocument()
            'On Error GoTo Err
            xml.Load(path)
            'On Error GoTo - 1

            opened_document = New cls_preXML_document
            With opened_document
                .name = xml.SelectSingleNode("/doc/name").InnerText
                .path = xml.SelectSingleNode("/doc/path").InnerText
                .path = ""
                If .path = "" Or My.Computer.FileSystem.DirectoryExists(.path) = False Then .path = My.Computer.FileSystem.GetParentPath(path)
                .pdf_path = xml.SelectSingleNode("/doc/pdf").InnerText
                If rgxt(.pdf_path, "^[A-Z]:\\") = False Then .pdf_path = My.Computer.FileSystem.CombinePath(.path, .pdf_path)


                If My.Computer.FileSystem.FileExists(.pdf_path) = False Then
                    .pdf_path = My.Computer.FileSystem.CombinePath(.path, My.Computer.FileSystem.GetName(.pdf_path))
                    If My.Computer.FileSystem.FileExists(.pdf_path) = False Then
                        'zkusíme najít pdf ve stejné složce, z níž nahráváme dokument
                        Dim files = My.Computer.FileSystem.GetFiles(.path)
                        Dim n_pdfs As Long = 0
                        For i = 0 To files.Count - 1
                            If files(i).EndsWith(".pdf") = True Then
                                .pdf_path = files(i)
                                n_pdfs += 1
                                If n_pdfs > 1 Then
                                    .pdf_path = ""
                                    Exit For
                                End If
                            End If
                        Next


                        If .pdf_path = "" Then MsgBox(c("Neplatná cesta k pdf. Vyberte soubor na kartě Dokumenty->Cesta k pdf"))
                    End If
                End If
                Me.frm.get_pdf_control.src = .pdf_path
                .n_pages = get_singlenode_value(xml.FirstChild, "n_pages", "-1")
                .load_pages()

                If My.Computer.FileSystem.DirectoryExists(.path & "\zaloha") = False Then .create_backup()

                Dim page As Long = -1
                If xml.SelectSingleNode("/doc/last_opened_page") IsNot Nothing Then
                    page = xml.SelectSingleNode("/doc/last_opened_page").InnerText
                End If
                'wsp.document_opened(opened_document, page)

                If xml.SelectSingleNode("/doc/pdf_first_page") IsNot Nothing Then
                    opened_document.pdf_first_page = xml.SelectSingleNode("/doc/pdf_first_page").InnerText
                End If

                If xml.SelectSingleNode("/doc/pdf_pages_per_page") IsNot Nothing Then
                    opened_document.pdf_pages_per_page = xml.SelectSingleNode("/doc/pdf_pages_per_page").InnerText
                End If

                If opened_document.pdf_pages_per_page < 1 Then opened_document.pdf_pages_per_page = 1
                Dim pdf_page As Integer = opened_document.pdf_first_page + ((1 + page) / opened_document.pdf_pages_per_page)
                Me.frm.get_pdf_control.setCurrentPage(pdf_page)

                Me.new_last_opened(CStr(My.Computer.FileSystem.GetFileInfo(path).DirectoryName))
                'wsp.open_page(0)
                update_doc_info_controls()
                .load_my_workspace()
                RaiseEvent on_document_opened(opened_document, page)
                Me.wsp.synchronize_pdf_and_text()

            End With

        Else
Err:
            MsgBox(c("Soubor neexistuje nebo se jej nepodařilo nahrát."),, c("Otvírání dokumentu"))
        End If
    End Sub
    Public Function c(caption As String, Optional eng As String = "")
        If lang = "en" Then

            Select Case caption
                Case "Aktivní stránka"
                    Return "Current page"
                Case "Neplatná cesta k pdf. Vyberte soubor na kartě Dokumenty->Cesta k pdf"
                    Return "Invalid path to pdf file. Select the file on the card Documents->Path to pdf file"
                Case "Dokument"
                    Return "Document"
                Case "Dokumenty"
                    Return "Documents"
                Case "Kolik stran je na jedné pdf straně: "
                    Return "Textual pages on one pdf page: "
                Case "Kontrola slov"
                    Return "Word check"
                Case "Uložit vše"
                    Return "Save all"
                Case "Exportovat do XML"
                    Return "Export to XML"
                Case "Hledaný výraz"
                    Return "Search term"
                Case "Jméno dokumentu"
                    Return "Document name"
                Case "Malá"
                    Return "Small"
                Case "Nahrát"
                    Return "Load"
                Case "Nahrazení"
                    Return "Replacement"
                Case "Nahrát pracovní prostředí dokumentu"
                    Return "Load work environment of the document"
                Case "Najdi"
                    Return "Find"
                Case "Naposledy otevřené dokumenty"
                    Return "Last opened documents"
                Case "Nepodařilo se nahrát slovník, cesta neexistuje. Nástroj ""Kontrola slov"" nebude fungovat."
                    Return "The dictionary could not be loaded, the path is not valid. The tool ""word check"" will not work"
                Case "Otvírání dokumentu"
                    Return "Open file"
                Case "Otevřít existující dokument"
                    Return "Open existing document"
                Case "PDF soubor: "
                    Return "PDF file: "
                Case "Pracovní prostředí"
                    Return "Work environment"
                Case "První strana textu v PDF: "
                    Return "First page of the text in the PDF file: "

                Case "Přidat do pracovního prostředí"
                    Return "Add to work environment"
                Case "Přidej slovo do slovníku."
                    Return "Add word to dictionary"
                Case "Přidej prázdnou stranu na konec dokumentu"
                    Return "Add an empty page at the end of the document"
                Case "Přidej prázdnou stranu PŘED právě otevřenou stranu"
                    Return "Add an empty page BEFORE the opened one"
                Case "Přidej prázdnou stranu ZA právě otevřenou stranu"
                    Return "Add an empty page AFTER the opened one"
                Case "Přidej stranu na konec dokumentu A VLOŽ TEXT"
                    Return "Add an empty page at the end of the document and INSERT TEXT"
                Case "Přidej stranu PŘED právě otevřenou stranu A VLOŽ TEXT"
                    Return "Add an empty page BEFORE the opened one and INSERT TEXT"
                Case "Přidej stranu ZA právě otevřenou stranu A VLOŽ TEXT"
                    Return "Add an empty page AFTER the opened one and INSERT TEXT"
                Case "Velikost písma a ovl. prvků: "
                    Return "Size of the font and controls: "
                Case "Soubor neexistuje nebo se jej nepodařilo nahrát."
                    Return "The file does not exist or it could not be loaded"
                Case "Střední"
                    Return "Middle"
                Case "Uložit otevřený dokument?"
                    Return "Save opened document?"
                Case "Uložit před založením nového otevřený dokument?"
                    Return "Save the opened document before creating a new one?"
                Case "Uložit pracovní prostředí dokumentu"
                    Return "Save work environment of the document"
                Case "Ukládat do: "
                    Return "Save to: "
                Case "Ukončit program"
                    Return "End the program"
                Case "Velká"
                    Return "Big"
                Case "Vkládání tagů"
                    Return "Tag insertion"
                Case "Vkládat jako text (nyní rtf)"
                    Return "Insert text as plain text (now rtf)"
                Case "Vkládat jako rtf (nyní text)"
                    Return "Insert text as rtf (now plain text)"
                Case "V regulérním výrazu v políčku je chyba: "
                    Return "There is an error in the regular expression: "
                Case "Vybrat"
                    Return "Select"

                Case "Zadejte název nového dokumentu."
                    Return "Enter the name of the new document."
                Case "Zavřít otevřený dokument"
                    Return "Close opened document"
                Case "Chyba v regulérním výrazu!"
                    Return "Error in regular expression"
                Case "Reg. výr."
                    Return "Reg. expr."
                Case "Celý dokument"
                    Return "Entire document"
                Case "Vybraný text"
                    Return "Selected text"
                Case "Řádek"
                    Return "Line"
                Case "sloupec"
                    Return "column"
                Case "znak"
                    Return "character"
                Case "XML elementy otevřené na začátku této strany: "
                    Return "XML elements opened at the top of the page: "
                Case "XML elementy otevřené na konci této strany: "
                    Return "XML element opened at the end of this page: "
                Case "XML v pořádku"
                    Return "XML correct"
                Case "Chyba XML"
                    Return "XML error"
                Case "CHYBA V XML: Uzavírá se tag, který není poslední otevřený."
                    Return "XML ERROR: Mismatched closing element."
                Case "CHYBA V XML: Neuzavřený tag."
                    Return "XML ERROR: element not closed."
                Case "CHYBA V XML: chyba na straně "
                    Return "XML ERROR: error on the page "
                Case "Strana"
                    Return "Page"
                Case " z "
                    Return " of "
                Case "Prázdný dokument"
                    Return "Empty document"
                Case "Není otevřen žádný dokument"
                    Return "No opened document"
                Case "Zavírací tag elementu"
                    Return "Closing tag for element"
                Case "ze strany"
                    Return "from the page"
                Case "Nepodařilo se nahrát soubor. Jeho XML je zřejmě chybné."
                    Return "The xml could not be loaded. It's XML is probably wrong."
                Case "Uzavřít element"
                    Return "Close element"
                Case "Vložit tag"
                    Return "Insert tag"
                Case "Nástroje"
                    Return "Tools"
                Case "Založit nový dokument"
                    Return "Create new document"
                Case "Zkontroluj"
                    Return "Check"
                Case Else
                    If eng = "" Then
                        Return caption & "-!- "
                    Else
                        Return eng
                    End If
            End Select
        Else
            Return caption
        End If

    End Function
    Public Sub close_document()
        If Me.opened_document IsNot Nothing Then
            Me.opened_document.save()
            new_last_opened(Me.opened_document.path)
            Me.opened_document = Nothing
            Me.wsp.doc = Nothing
            Me.wsp.p = Nothing
            Me._p = Nothing
            Me.wsp.flti = Nothing
            Me.wsp.p_index = -1
            Me.wsp.tmr_highlight.Stop()
            Me.wsp.tmr_generate_rtf.Stop()
            Me.wsp.tmr_ln.Stop()
            Me.wsp.adjust_controls(Nothing, Nothing)
            Me.wsp.rtb.Text = ""
            Me.wsp.delete_ln()
            Me.wsp.update_doc_info_controls()
            Me.frm.get_pdf_control.src = ""
            txt_docname.Text = ""
            txt_path.Text = ""
            txt_pdf.Text = ""
            save_settings()
        End If
    End Sub

    Public Sub new_document(name As String, path_to_save As String, pdf_path As String)
        'vytvoření nového prázdného dokumentu...
        opened_document = New cls_preXML_document
        With opened_document
            .name = name
            .path = path_to_save
        End With


        wsp.document_opened(opened_document, -1)
        wsp.update_doc_info_controls()
        Me.update_doc_info_controls()
        RaiseEvent on_document_opened(opened_document, -1)

    End Sub
    Public Sub update_doc_info_controls()
        If opened_document IsNot Nothing Then
            txt_docname.Text = opened_document.name
            txt_path.Text = opened_document.path
            txt_pdf.Text = opened_document.pdf_path
            txt_pages_on_one_pdf_page.Text = opened_document.pdf_pages_per_page
            txt_pdf_index_of_first_page.Text = opened_document.pdf_first_page
            If Me.environment_size = CSng(0.7) Then
                rbts_env_sizes(0).Checked = True
            ElseIf Me.environment_size = CSng(0.85) Then
                rbts_env_sizes(1).Checked = True
            Else
                rbts_env_sizes(2).Checked = True
            End If

        End If
    End Sub
    Public Sub save_settings()
        Dim x As Xml.XmlDocument
        Dim n As Xml.XmlNode
        Dim n2 As Xml.XmlNode
        Dim xml As String
        Dim i As Integer
        x = New XmlDocument()
        n = x.AppendChild(x.CreateNode(XmlNodeType.Element, "settings", ""))

        If last_opened IsNot Nothing Then
            n2 = n.AppendChild(x.CreateNode(XmlNodeType.Element, "last_opened_documents", ""))
            For i = 0 To UBound(last_opened)
                If last_opened(i) <> "" Then
                    n2.AppendChild(x.CreateNode(XmlNodeType.Element, "path", "")).InnerText = last_opened(i)
                End If
            Next
        End If
        n.AppendChild(x.CreateNode(XmlNodeType.Element, "default_workspace_path", "")).InnerText = default_workspace_path
        n.AppendChild(x.CreateNode(XmlNodeType.Element, "rtb_font_size", "")).InnerText = rtb_font_size
        n.AppendChild(x.CreateNode(XmlNodeType.Element, "environment_size", "")).InnerText = environment_size
        n.AppendChild(x.CreateNode(XmlNodeType.Element, "lang", "")).InnerText = lang
        If Me.frm.get_left_panel_width <> 0 Then '0 znamená, že je pracovní prostředí rozděleno napůl, tj. výchozí stav (pokud bychom to ukládali pokaždé natvrdo,
            n.AppendChild(x.CreateNode(XmlNodeType.Element, "tool_panel_width", "")).InnerText = Me.frm.get_left_panel_width
            'došlo by k problémům při přenášení na počítače s jiným rozlišením
        End If
        x.Save(Me.settings_path)
    End Sub

    Public Sub load_settings()
        Dim xml As XmlDocument
        If My.Computer.FileSystem.FileExists(settings_path) Then
            xml = New XmlDocument()
            On Error GoTo err
            xml.Load(settings_path)
            Dim i As Integer
            Dim n As XmlNode
            Dim ns As XmlNodeList
            ns = xml.SelectNodes("settings/last_opened_documents/path")
            For i = 0 To ns.Count - 1
                last_opened(i) = ns(i).InnerText
            Next
            n = xml.SelectSingleNode("settings/default_workspace_path")
            If n IsNot Nothing Then
                default_workspace_path = n.InnerText
                If My.Computer.FileSystem.FileExists(default_workspace_path) = False Then
                    default_workspace_path = Application.StartupPath & "\default_workspace.xml"
                End If
            Else
                    default_workspace_path = Application.StartupPath & "\default_workspace.xml"
            End If
            n = xml.SelectSingleNode("settings/tool_panel_width")
            If n IsNot Nothing Then
                'uložené nastavení rozměrů dvou hlavních částí prac. prostředí - není-li zadáno, bude to půl na půl
                If rgxt(n.InnerText, "^[0-9]+$") = True Then Me.frm.set_left_panel_width(CInt(n.InnerText))
            End If

            rtb_font_size = get_singlenode_value(xml.FirstChild, "rtb_font_size", "24")
            environment_size = get_singlenode_value(xml.FirstChild, "environment_size", "1")
            lang = get_singlenode_value(xml.FirstChild, "lang", "cz")
            env.wsp.rtb_(0).ZoomFactor = get_singlenode_value(xml.FirstChild, "rtb_zoom", "1")
            env.wsp.rtb_(1).ZoomFactor = get_singlenode_value(xml.FirstChild, "rtb_zoom", "1")
Err:
            If default_folder_path = "" Then default_folder_path = Application.StartupPath
        End If
    End Sub
    Public Property environment_size As Single
        Get
            Return environment_size_
        End Get
        Set(value As Single)
            If environment_size_ <> value And environment_size_ <> 0 Then 'dochází-li ke změně, anebo na samém začátku při prvním nastavení
                If value < 0.7 Then value = 0.7
                environment_size_ = value
                Me.def_font = New Font("Calibri", 11.25 * value)

                change_font_of_child_controls(frm_main)
                wsp.display_page(Nothing)
            End If
        End Set
    End Property
    Private Sub change_font_of_child_controls(parent As Control)
        Dim ctrl As Control
        For Each ctrl In parent.Controls
            Try
                If TypeOf ctrl IsNot RichTextBox Then
                    ctrl.Font = def_font
                End If
                change_font_of_child_controls(ctrl)
            Catch ex As Exception
            End Try
        Next
    End Sub
    Public Sub adjust_last_opened_menu(mnu As ToolStripMenuItem)
        mnu.DropDownItems.Clear()
        Dim i As Integer
        If last_opened IsNot Nothing Then
            For i = 0 To UBound(last_opened)
                If last_opened(i) <> "" Then mnu.DropDownItems.Add(last_opened(i))
            Next
        End If
    End Sub
    Public Sub new_last_opened(last_opened_path As String)
        Dim i As Integer
        Dim j As Integer
        j = UBound(last_opened) - 1
        For i = 0 To UBound(last_opened) - 1
            If last_opened(i) = last_opened_path Then
                j = i
                Exit For
            End If
        Next
        For i = j To 1 Step -1
            last_opened(i) = last_opened(i - 1)
        Next
        last_opened(0) = last_opened_path
        adjust_last_opened_menu(mnu_last_opened)
    End Sub
    Public Sub run_workspace()
        create_controls()
        'Me.wsp = New cls_workspace()
        'wsp = Me.wsp
        Me.wsp = New cls_workspace(frm, Me)
    End Sub
    Public Sub create_controls()
        'lastctrl = ctrl_container.mnu_document
        Dim lbl As Label
        With NewCtrl(mstr, New MenuStrip)
            .Parent = ctrl_container
        End With
        Dim mnu As ToolStripMenuItem
        mnu_main = mstr.Items.Add(c("Dokumenty"))
        AddHandler mnu_main.DropDownOpened, AddressOf mnu_main_ddo
        AddHandler mnu_main.MouseEnter, AddressOf mnu_main_me
        AddHandler mnu_main.MouseLeave, AddressOf mnu_main_ml
        mnu_new_document = mnu_main.DropDownItems.Add(c("Založit nový dokument"))
        AddHandler mnu_new_document.Click, AddressOf mnu_new_doc_click
        mnu_open_document = mnu_main.DropDownItems.Add(c("Otevřít existující dokument"))
        AddHandler mnu_open_document.Click, AddressOf mnu_open_document_click
        mnu_last_opened = mnu_main.DropDownItems.Add(c("Naposledy otevřené dokumenty"))
        adjust_last_opened_menu(mnu_last_opened)
        AddHandler mnu_last_opened.DropDownItemClicked, AddressOf mnu_last_opened_subitem_click
        mnu_close_document = mnu_main.DropDownItems.Add(c("Zavřít otevřený dokument"))
        AddHandler mnu_close_document.Click, AddressOf mnu_close_click
        mnu_main.DropDownItems.Add("-")
        mnu_save_all = mnu_main.DropDownItems.Add(c("Uložit vše"))
        AddHandler mnu_save_all.Click, AddressOf mnu_save_all_click
        mnu_export_to_xml = mnu_main.DropDownItems.Add(c("Exportovat do XML"))
        AddHandler mnu_export_to_xml.Click, AddressOf mnu_export_to_xml_click
        mnu_main.DropDownItems.Add("-")
        mnu = mnu_main.DropDownItems.Add(c("Pracovní prostředí"))
        mnu_env_lang = mnu.DropDownItems.Add(c("Změň jazyk/Change language"))
        AddHandler mnu_env_lang.Click, AddressOf mnu_change_lang_click
        mnu_wsp_save = mnu.DropDownItems.Add(c("Uložit pracovní prostředí dokumentu"))
        AddHandler mnu_wsp_save.Click, AddressOf mnu_wsp_save_click
        mnu_wsp = mnu.DropDownItems.Add(c("Nahrát pracovní prostředí dokumentu"))
        mnu_wsp.Name = "mnu_load_wsp"
        AddHandler mnu_wsp.Click, AddressOf mnu_wsp_load_click
        mnu_wsp = mnu.DropDownItems.Add(c("Přidat do pracovního prostředí"))
        mnu_wsp.Name = "mnu_add_to_wsp"
        AddHandler mnu_wsp.Click, AddressOf mnu_wsp_load_click

        mnu_main.DropDownItems.Add("-")
        mnu_end = mnu_main.DropDownItems.Add(c("Ukončit program"))
        AddHandler mnu_end.Click, AddressOf mnu_end_click

        With NewCtrl(lbl_docname, New Label)
            .Parent = ctrl_container
            .Left = 5
            .Top = lastctrl.Top + lastctrl.Height + 25 * Me.environment_size_
            .Width = 150 * Me.environment_size_
            .Text = c("Jméno dokumentu")
        End With
        With NewCtrl(txt_docname, New TextBox)
            .Parent = ctrl_container
            .Top = lastctrl.Top
            .Left = lastctrl.Left + lastctrl.Width + 5
            .Width = 250 * Me.environment_size_
            '.Text = Me.opened_document.name
            AddHandler .TextChanged, AddressOf txt_docname_changed
        End With

        With NewCtrl(lbl_path, New Label)
            .Parent = ctrl_container
            .Left = 5
            .Top = lastctrl.Top + lastctrl.Height + 25 * Me.environment_size_
            .Width = 150 * Me.environment_size_
            .Text = c("Ukládat do: ")
        End With
        With NewCtrl(txt_path, New TextBox)
            .Parent = ctrl_container
            .Top = lastctrl.Top
            .Left = lastctrl.Left + lastctrl.Width + 5
            .Width = 250 * Me.environment_size_
            '.Text = Me.opened_document.path
        End With
        With NewCtrl(cmd_load_pdf, New Button)
            .Parent = ctrl_container
            .Top = lastctrl.Top
            .Left = lastctrl.Left + lastctrl.Width
            .Width = 100 * Me.environment_size_
            .Text = c("Vybrat")
            cmd_load_pdf.AutoSize = True
            AddHandler .Click, AddressOf cmd_select_path_click
        End With


        With NewCtrl(lbl_pdf, New Label)
            .Parent = ctrl_container
            .Left = 5
            .Top = lastctrl.Top + lastctrl.Height + 25 * Me.environment_size_
            .Width = 150 * Me.environment_size_
            .Text = c("PDF soubor: ")
        End With
        With NewCtrl(txt_pdf, New TextBox)
            .Parent = ctrl_container
            .Top = lastctrl.Top
            .Left = lastctrl.Left + lastctrl.Width + 5
            .Width = 250 * Me.environment_size_
        End With
        With NewCtrl(cmd_load_pdf, New Button)
            .Parent = ctrl_container
            .Top = lastctrl.Top
            .Left = lastctrl.Left + lastctrl.Width
            .Width = 100 * Me.environment_size_
            .Text = c("Nahrát")
            cmd_load_pdf.AutoSize = True
            AddHandler .Click, AddressOf cmd_add_pdf_click
        End With
        With NewCtrl(lbl_docname, New Label)
            .Parent = ctrl_container
            .Top = lastctrl.Top + lastctrl.Height + 25 * Me.environment_size_
            .Left = 5
            .Width = 200 * Me.environment_size_
            .Text = c("První strana textu v PDF: ")
        End With
        With NewCtrl(txt_pdf_index_of_first_page, New TextBox)
            .Parent = ctrl_container
            .Top = lastctrl.Top
            .Left = lastctrl.Left + lastctrl.Width
            .Width = 50 * Me.environment_size_
            AddHandler .TextChanged, AddressOf txt_pdf_index_of_first_page_changed
        End With

        With NewCtrl(lbl_docname, New Label)
            .Parent = ctrl_container
            .Top = lastctrl.Top + lastctrl.Height + 25 * Me.environment_size_
            .Left = 5
            .Width = 200 * Me.environment_size_
            .Text = c("Kolik stran je na jedné pdf straně: ")
        End With
        With NewCtrl(txt_pages_on_one_pdf_page, New TextBox)
            .Parent = ctrl_container
            .Top = lastctrl.Top
            .Left = lastctrl.Left + lastctrl.Width
            .Width = 50 * Me.environment_size_
            AddHandler .TextChanged, AddressOf txt_pages_on_one_pdf_page_changed
        End With
        With NewCtrl(lbl, New Label)
            .Parent = ctrl_container
            .Top = lastctrl.Top + lastctrl.Height + 25 * Me.environment_size_
            .Left = 5
            lbl.AutoSize = True
            .Text = c("Velikost písma a ovl. prvků: ")
        End With
        With NewCtrl(rbts_env_sizes(0), New RadioButton)
            .Parent = ctrl_container
            .Left = lastctrl.Left + lastctrl.Width
            .Font = New Font("Calibri", 11.25 * 0.7)
            .Height = 25 * 0.7
            .Tag = "0,7"
            .Text = c("Malá")
            .Width = 100 * 0.7
            .Top = lastctrl.Top + lastctrl.Height - .Height
            rbts_env_sizes(0).Appearance = Appearance.Button
            AddHandler rbts_env_sizes(0).CheckedChanged, AddressOf rbt_envsize_checked_changed
        End With
        With NewCtrl(rbts_env_sizes(1), New RadioButton)
            .Parent = ctrl_container
            .Left = lastctrl.Left + lastctrl.Width
            .Font = New Font("Calibri", 11.25 * 0.85)
            .Height = 25 * 0.85
            .Tag = "0,85"
            .Text = c("Střední")
            .Width = 100 * 0.85
            .Top = lastctrl.Top + lastctrl.Height - .Height
            rbts_env_sizes(1).Appearance = Appearance.Button
            AddHandler rbts_env_sizes(1).CheckedChanged, AddressOf rbt_envsize_checked_changed
        End With
        With NewCtrl(rbts_env_sizes(2), New RadioButton)
            .Parent = ctrl_container
            .Left = lastctrl.Left + lastctrl.Width
            .Font = New Font("Calibri", 11.25)
            .Height = 25
            .Tag = "1"
            .Text = c("Velká")
            .Width = 100
            .Top = lastctrl.Top + lastctrl.Height - .Height
            rbts_env_sizes(2).Appearance = Appearance.Button
            AddHandler rbts_env_sizes(2).CheckedChanged, AddressOf rbt_envsize_checked_changed
        End With

    End Sub 'mnu_save_all_click
    Private Sub mnu_change_lang_click(sender As Object, e As EventArgs)
        If Me.lang = "cz" Then
            Me.lang = "en"
        Else
            Me.lang = "cz"
        End If
        Me.save_settings()
    End Sub
    Private Sub mnu_main_me(sender As Object, e As EventArgs)
        mnu_main.Tag = "M"
    End Sub
    Private Sub mnu_main_ml(sender As Object, e As EventArgs)
        mnu_main.Tag = ""
    End Sub
    Private Sub mnu_main_ddo(sender As Object, e As EventArgs)
        If mnu_main.Tag <> "M" Then
            SendKeys.Send("{ESC}")
            SendKeys.Send("{ESC}")
            wsp.rtb.Select()
        End If
    End Sub
    Public Sub rbt_envsize_checked_changed(sender As Object, e As EventArgs)
        For i = 0 To 2
            If rbts_env_sizes(i).Checked = True Then
                Me.environment_size = CSng(rbts_env_sizes(i).Tag)
            End If
        Next
    End Sub
    Public Sub txt_pdf_index_of_first_page_changed(sender As Object, e As EventArgs)
        If opened_document IsNot Nothing Then
            Try
                opened_document.pdf_first_page = CInt(sender.text)
            Catch
                opened_document.pdf_first_page = 1
            End Try
        End If
    End Sub
    Public Sub txt_pages_on_one_pdf_page_changed(sender As Object, e As EventArgs)
        If opened_document IsNot Nothing Then
            Try
                opened_document.pdf_pages_per_page = CInt(sender.text)
            Catch
                opened_document.pdf_pages_per_page = 1
            End Try
        End If
    End Sub

    Public Sub mnu_export_to_xml_click(sender As Object, e As EventArgs)
        Dim xml As String
        If Me.opened_document IsNot Nothing Then
            xml = Me.opened_document.export_to_xml
            Dim p As String
            Dim d As SaveFileDialog
            d = New SaveFileDialog()
            d.InitialDirectory = opened_document.path
            d.FileName = Me.opened_document.name & ".xml"
            d.ShowDialog()

            If d.FileName <> "" Then
                My.Computer.FileSystem.WriteAllText(d.FileName, xml, False)
            End If
        End If
    End Sub
    Public Sub mnu_wsp_save_click(sender As Object, e As EventArgs)
        If Me.opened_document IsNot Nothing Then
            Dim d As SaveFileDialog
            d = New SaveFileDialog
            d.FileName = Me.default_workspace_path
            d.ShowDialog()
            If d.FileName <> "" Then
                Me.wsp.tm.export_to_xml.Save(d.FileName)
            End If
        End If
    End Sub
    Public Sub mnu_wsp_load_click(sender As Object, e As EventArgs)
        If opened_document IsNot Nothing Then
            Dim d As OpenFileDialog
            d = New OpenFileDialog
            d.FileName = Me.default_workspace_path
            d.ShowDialog()
            If d.FileName <> "" Then
                If sender.name = "mnu_load_wsp" Then
                    opened_document.load_my_workspace(d.FileName)
                ElseIf sender.name = "mnu_add_to_wsp" Then
                    opened_document.add_to_workspace(d.FileName)
                End If
            End If
        End If
    End Sub

    Public Sub mnu_save_all_click(sender As Object, e As EventArgs)
        If opened_document IsNot Nothing Then
            opened_document.save(True)
        End If
    End Sub
    Public Sub mnu_last_opened_subitem_click(sender As Object, e As ToolStripItemClickedEventArgs)
        Dim save As Integer
        If opened_document IsNot Nothing Then
            save = MsgBox(c("Uložit před založením nového otevřený dokument?"), MsgBoxStyle.YesNoCancel Or MsgBoxStyle.Question,
                          c("Uložit otevřený dokument?"))
            If save = MsgBoxResult.Yes Then
            ElseIf save = MsgBoxResult.Cancel Then
                Exit Sub
            End If
        End If
        Me.close_document()
        Dim f As String
        f = e.ClickedItem.Text
        If My.Computer.FileSystem.DirectoryExists(f) = True Then
            If My.Computer.FileSystem.FileExists(f & "\dokument.xml") Then
                Me.open_document(f & "\dokument.xml")
            End If
        End If
    End Sub
    Public Sub mnu_open_document_click(sender As Object, e As EventArgs)
        Dim save As Integer
        If opened_document IsNot Nothing Then
            opened_document.save()
        End If
        Dim fd As OpenFileDialog
        fd = New OpenFileDialog()
        fd.FileName = last_opened(0)
        fd.ShowDialog()
        If fd.FileName <> "" Then
            Me.close_document()
            Me.open_document(fd.FileName)
        End If
    End Sub
    Public Sub mnu_close_click(sender As Object, e As EventArgs)
        Me.close_document()
    End Sub
    Public Sub mnu_end_click(sender As Object, e As EventArgs)
        close_document()
        End
    End Sub
    Public Sub mnu_new_doc_click(sender As Object, e As EventArgs)
        If opened_document IsNot Nothing Then
            'save = MsgBox("Uložit před založením nového otevřený dokument?", MsgBoxStyle.YesNoCancel Or MsgBoxStyle.Question, "Uložit otevřený dokument?")
            opened_document.save()

            close_document()
        End If
        Dim ndname As String = InputBox("Zadejte název nového dokumentu.")
        If ndname <> "" Then
            new_document(ndname, "", "")
        End If
    End Sub

    Public Sub txt_docname_changed(sender As Object, e As EventArgs)
        If txt_docname.Text <> "" And Me.opened_document IsNot Nothing Then
            Me.opened_document.name = txt_docname.Text
            'Me.opened_document.save()
        End If
    End Sub
    Public Sub cmd_select_path_click(sender As Object, e As EventArgs)
        If Me.opened_document IsNot Nothing Then
            'If Me.opened_document.path = "" Then
            Dim d As New FolderBrowserDialog
            d.ShowDialog()
            If d.SelectedPath <> "" Then
                Me.opened_document.path = d.SelectedPath
            End If
            'End If
            If Me.opened_document.path <> "" Then Me.opened_document.save()
            txt_path.Text = Me.opened_document.path
        End If
    End Sub


    Public Sub cmd_add_pdf_click(sender As Object, e As EventArgs)
        If Me.opened_document IsNot Nothing Then
            Dim d As OpenFileDialog
            d = New OpenFileDialog()
            d.ShowDialog()
            Dim f As String
            Dim pdf As AxAcroPDFLib.AxAcroPDF
            f = d.FileName
            If f <> "" Then
                txt_pdf.Text = f
                Me.opened_document.pdf_path = f
                pdf = Me.frm.get_pdf_control
                pdf.src = f
                Me.opened_document.save()
            End If
        End If
    End Sub


    Public Sub cmd_add_page_click(sender As Object, e As EventArgs)
        Me.opened_document.new_page()
    End Sub
    Friend Function NewCtrl(ByRef obj_reference As Object, ctrl As Control) As Control
        'pro snazší vkládání ovl. prvků jednoho za druhým (aby se nemuselo při nastavování pozice nově vloženého prvku vždy odkazovat jmenovitě na předchozí,
        'od jehož pozice se pozice nového odvíjí, ale dalo se odkazovat vždy jednoduše na lastctrl)
        obj_reference = ctrl
        lastctrl = thisctrl
        thisctrl = ctrl
        thisctrl.Font = Me.def_font
        Return ctrl
    End Function
End Class