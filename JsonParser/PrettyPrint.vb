Imports JsonParser.Parsing
Imports System.Collections.Immutable
Imports System.Runtime.CompilerServices

Public Module PrettyPrint
    <Extension>
    Public Sub PrettyPrint(node As JToken, Optional trivia As Boolean = False)
        PrettyPrintRecursive(node, trivia:=trivia)
    End Sub

    Private Sub PrettyPrintRecursive(node As JToken, Optional indent As String = "", Optional isLast As Boolean = True, Optional trivia As Boolean = False)
        Dim marker As String = If(isLast, "└──", "├──")
        Console.Write(indent)
        Console.Write(marker)
        Console.Write(node.GetType.Name)
        If TypeOf node Is SyntaxToken Then Console.Write($": {node}")
        Console.WriteLine()
        indent &= If(isLast, "     ", "|    ")
        Dim children As ImmutableList(Of JToken) = node.Nodes
        If trivia AndAlso TypeOf node Is SyntaxToken Then
            children = children.InsertRange(0, DirectCast(node, SyntaxToken).LeadingTrivia).AddRange(DirectCast(node, SyntaxToken).TrailingTrivia)
        End If
        Dim lastChild As JToken = children.LastOrDefault
        For Each child As JToken In children
            PrettyPrintRecursive(child, indent, child Is lastChild, trivia)
        Next
    End Sub
End Module