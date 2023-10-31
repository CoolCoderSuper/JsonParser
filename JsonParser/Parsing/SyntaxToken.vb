Imports System.Collections.Immutable

Namespace Parsing
    Public NotInheritable Class SyntaxToken
        Inherits JToken
        Protected _leadingTrivia As ImmutableList(Of SyntaxToken) = ImmutableList(Of SyntaxToken).Empty
        Protected _trailingTrivia As ImmutableList(Of SyntaxToken) = ImmutableList(Of SyntaxToken).Empty

        Public Sub New(kind As SyntaxKind, position As Integer, text As String, value As Object)
            Me.Kind = kind
            Me.Position = position
            Me.Text = text
            Me.Value = value
        End Sub

        Public ReadOnly Property Kind As SyntaxKind

        Public ReadOnly Property Position As Integer

        Public ReadOnly Property Text As String

        Public ReadOnly Property Value As Object

        Public ReadOnly Property LeadingTrivia As ImmutableList(Of SyntaxToken)
            Get
                Return _leadingTrivia
            End Get
        End Property

        Public ReadOnly Property TrailingTrivia As ImmutableList(Of SyntaxToken)
            Get
                Return _trailingTrivia
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{Kind}: '{Text}'"
        End Function

        Public Function WithLeadingTrivia(trivia As IEnumerable(Of SyntaxToken)) As SyntaxToken
            Dim newToken As New SyntaxToken(Kind, Position, Text, Value) With {
                ._leadingTrivia = trivia.ToImmutableList,
                ._trailingTrivia = _trailingTrivia
            }
            Return newToken
        End Function

        Public Function AddLeadingTrivia(trivia As SyntaxToken) As SyntaxToken
            Dim newToken As New SyntaxToken(Kind, Position, Text, Value) With {
                ._leadingTrivia = _leadingTrivia.Add(trivia),
                ._trailingTrivia = _trailingTrivia
            }
            Return newToken
        End Function

        Public Function AddLeadingTrivia(trivia As IEnumerable(Of SyntaxToken)) As SyntaxToken
            Dim newToken As New SyntaxToken(Kind, Position, Text, Value) With {
                ._leadingTrivia = _leadingTrivia.AddRange(trivia),
                ._trailingTrivia = _trailingTrivia
            }
            Return newToken
        End Function

        Public Function RemoveLeadingTrivia(trivia As SyntaxToken) As SyntaxToken
            Dim newToken As New SyntaxToken(Kind, Position, Text, Value) With {
                ._leadingTrivia = _leadingTrivia.Remove(trivia),
                ._trailingTrivia = _trailingTrivia
            }
            Return newToken
        End Function

        Public Function WithTrailingTrivia(trivia As IEnumerable(Of SyntaxToken)) As SyntaxToken
            Dim newToken As New SyntaxToken(Kind, Position, Text, Value) With {
                ._trailingTrivia = trivia.ToImmutableList,
                ._leadingTrivia = _leadingTrivia
            }
            Return newToken
        End Function

        Public Function AddTrailingTrivia(trivia As SyntaxToken) As SyntaxToken
            Dim newToken As New SyntaxToken(Kind, Position, Text, Value) With {
                ._trailingTrivia = _trailingTrivia.Add(trivia),
                ._leadingTrivia = _leadingTrivia
            }
            Return newToken
        End Function

        Public Function AddTrailingTrivia(trivia As IEnumerable(Of SyntaxToken)) As SyntaxToken
            Dim newToken As New SyntaxToken(Kind, Position, Text, Value) With {
                ._trailingTrivia = _trailingTrivia.AddRange(trivia),
                ._leadingTrivia = _leadingTrivia
            }
            Return newToken
        End Function

        Public Function RemoveTrailingTrivia(trivia As SyntaxToken) As SyntaxToken
            Dim newToken As New SyntaxToken(Kind, Position, Text, Value) With {
                ._trailingTrivia = _trailingTrivia.Remove(trivia),
                ._leadingTrivia = _leadingTrivia
            }
            Return newToken
        End Function

        Public Overrides Function WithNodes(nodes As IEnumerable(Of JToken)) As JToken
            Throw New NotImplementedException()
        End Function

        Public Overrides Function AddNode(node As JToken) As JToken
            Throw New NotImplementedException()
        End Function

        Public Overrides Function AddNodes(nodes As IEnumerable(Of JToken)) As JToken
            Throw New NotImplementedException()
        End Function

        Public Overrides Function RemoveNode(node As JToken) As JToken
            Throw New NotImplementedException()
        End Function

        Public Overrides Function ReplaceNode(oldNode As JToken, newNode As JToken) As JToken
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace