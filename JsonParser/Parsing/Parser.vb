Namespace Parsing
    'TODO: Hexadecimal numbers
    'TODO: Factory
    'TODO: Clean up syntax tree
    Public Class Parser
        Private ReadOnly _tokens As SyntaxToken()
        Private _position As Integer
        Private _diagnostics As New List(Of String)

        Public Sub New(text As String)
            Dim tokens As New List(Of SyntaxToken)
            Dim lex As New Lexer(text)
            Dim token As SyntaxToken
            Do
                token = lex.Lex
                If token.Kind <> SyntaxKind.BadToken Then
                    tokens.Add(token)
                End If
            Loop While token.Kind <> SyntaxKind.EndOfFileToken
            _tokens = tokens.ToArray
            _diagnostics.AddRange(lex.Diagnostics)
        End Sub

        Public ReadOnly Property Diagnostics As IEnumerable(Of String)
            Get
                Return _diagnostics
            End Get
        End Property

        Private Function Peek(offset As Integer) As SyntaxToken
            Dim index As Integer = _position + offset
            If index >= _tokens.Length Then Return _tokens.LastOrDefault
            Return _tokens(index)
        End Function

        Private ReadOnly Property Current As SyntaxToken
            Get
                Return Peek(0)
            End Get
        End Property

        Private Sub [Next]()
            _position += 1
        End Sub

        Public Function Parse() As JToken
            Dim val As JValue = ParseValue()
            If TypeOf val.Nodes.First Is JObject Then
                Return val.Nodes.First
            ElseIf TypeOf val.Nodes.First Is JArray Then
                Return val.Nodes.First
            Else
                Return val
            End If
        End Function

        Private Function ParseObject() As JObject
            If Current.Kind = SyntaxKind.OpenBraceToken Then
                Dim obj As New JObject
                obj = obj.AddNode(Current)
                [Next]()
                If Current.Kind <> SyntaxKind.CloseBraceToken Then
                    Dim prop As JProperty = ParseProperty()
                    While prop IsNot Nothing
                        obj = obj.AddNode(prop)
                        While Current.Kind = SyntaxKind.CommaToken
                            obj = obj.AddNode(Current)
                            [Next]()
                        End While
                        If Current.Kind = SyntaxKind.CloseBraceToken Then Exit While
                        prop = ParseProperty()
                    End While
                End If
                If Current.Kind = SyntaxKind.CloseBraceToken Then
                    obj = obj.AddNode(Current)
                    Return obj
                Else
                    _diagnostics.Add($"Expected CloseBraceToken: but got {Current.Kind}")
                    Return Nothing
                End If
                Return obj
            Else
                Return Nothing
            End If
        End Function

        Private Function ParseArray() As JArray
            If Current.Kind = SyntaxKind.OpenBracketToken Then
                Dim arr As New JArray
                arr = arr.AddNode(Current)
                [Next]()
                If Current.Kind <> SyntaxKind.CloseBracketToken Then
                    Dim val As JValue = ParseValue()
                    While val IsNot Nothing
                        arr = arr.AddNode(val)
                        While Current.Kind = SyntaxKind.CommaToken
                            arr = arr.AddNode(Current)
                            [Next]()
                        End While
                        If Current.Kind = SyntaxKind.CloseBracketToken Then Exit While
                        val = ParseValue()
                    End While
                End If
                If Current.Kind = SyntaxKind.CloseBracketToken Then
                    arr = arr.AddNode(Current)
                Else
                    _diagnostics.Add($"Expected CloseBracketToken: but got {Current.Kind}")
                    Return Nothing
                End If
                Return arr
            Else
                Return Nothing
            End If
        End Function

        Private Function ParseProperty() As JProperty
            Dim prop As JProperty = Nothing
            If Current.Kind = SyntaxKind.StringLiteralToken OrElse Current.Kind = SyntaxKind.IdentifierToken Then
                prop = New JProperty
                prop = prop.AddNode(Current)
                [Next]()
                If Current.Kind = SyntaxKind.ColonToken Then
                    prop = prop.AddNode(Current)
                    [Next]()
                    Dim val As JValue = ParseValue()
                    If val IsNot Nothing Then
                        prop = prop.AddNode(val)
                    End If
                Else
                    _diagnostics.Add($"Expected ColonToken: but got {Current.Kind}")
                    prop = Nothing
                End If
            Else
                _diagnostics.Add($"Expected StringLiteralToken or IdentifierToken: but got {Current.Kind}")
                prop = Nothing
            End If
            Return prop
        End Function

        Private Function ParseValue() As JValue
            Dim val As JValue
            If Current.Kind = SyntaxKind.StringLiteralToken OrElse Current.Kind = SyntaxKind.NumberLiteralToken OrElse Current.Kind = SyntaxKind.TrueLiteralToken OrElse Current.Kind = SyntaxKind.FalseLiteralToken OrElse Current.Kind = SyntaxKind.NullLiteralToken OrElse Current.Kind = SyntaxKind.InfinityLiteralToken OrElse Current.Kind = SyntaxKind.NegativeInfinityLiteralToken OrElse Current.Kind = SyntaxKind.NaNLiteralToken Then
                val = New JLiteral().WithNodes({Current})
            ElseIf Current.Kind = SyntaxKind.OpenBraceToken Then
                val = ParseObject()
            ElseIf Current.Kind = SyntaxKind.OpenBracketToken Then
                val = ParseArray()
            Else
                val = Nothing
                _diagnostics.Add($"Expected StringLiteralToken, NumberLiteralToken, InfinityLiteralToken, NegativeInfinityLiteralToken, NaNLiteralToken, TrueLiteralToken, FalseLiteralToken or NullLiteralToken: but got {Current.Kind}")
            End If
            If val IsNot Nothing Then
                [Next]()
            End If
            Return val
        End Function
    End Class
End Namespace