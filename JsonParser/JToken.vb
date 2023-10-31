Imports System.Collections.Immutable
Imports System.Text

Namespace Parsing
    Public MustInherit Class JToken
        Protected _nodes As ImmutableList(Of JToken) = ImmutableList(Of JToken).Empty

        ''' <summary>
        ''' The immediate children of this node.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Nodes As ImmutableList(Of JToken)
            Get
                Return _nodes
            End Get
        End Property

        ''' <summary>
        ''' Returns this node with the specified nodes.
        ''' </summary>
        ''' <param name="nodes">The nodes.</param>
        ''' <returns></returns>
        Public MustOverride Function WithNodes(nodes As IEnumerable(Of JToken)) As JToken

        ''' <summary>
        ''' Returns this node with the specified node added to it.
        ''' </summary>
        ''' <param name="node">The node to add.</param>
        ''' <returns></returns>
        Public MustOverride Function AddNode(node As JToken) As JToken

        ''' <summary>
        ''' Returns this node with the specified nodes added to it.
        ''' </summary>
        ''' <param name="nodes">The nodes to add.</param>
        ''' <returns></returns>
        Public MustOverride Function AddNodes(nodes As IEnumerable(Of JToken)) As JToken

        ''' <summary>
        ''' Returns this node with the specified node removed.
        ''' </summary>
        ''' <param name="node">The node to remove.</param>
        ''' <returns></returns>
        Public MustOverride Function RemoveNode(node As JToken) As JToken

        ''' <summary>
        ''' Returns this node with the specified node replaced with the specified nodes.
        ''' </summary>
        ''' <param name="oldNode">The node to replace.</param>
        ''' <param name="newNode">The node to replace the old node with.</param>
        ''' <returns></returns>
        Public MustOverride Function ReplaceNode(oldNode As JToken, newNode As JToken) As JToken

        Public Overrides Function ToString() As String
            Dim builder As New StringBuilder
            ToStringRecursive(builder, Me)
            Return builder.ToString
        End Function

        Private Shared Sub ToStringRecursive(builder As StringBuilder, parent As JToken)
            For Each node As JToken In parent.Nodes
                Dim token As SyntaxToken = TryCast(node, SyntaxToken)
                If token IsNot Nothing Then
                    For Each trivia As SyntaxToken In token.LeadingTrivia
                        builder.Append(trivia.Text)
                    Next
                    builder.Append(token.Text)
                    For Each trivia As SyntaxToken In token.TrailingTrivia
                        builder.Append(trivia.Text)
                    Next
                    Continue For
                End If
                ToStringRecursive(builder, node)
            Next
        End Sub

        ''' <summary>
        ''' Retrieves all the descendants of this node including it's self.
        ''' </summary>
        ''' <returns></returns>
        Public Function DescendantNodesAndSelf() As IEnumerable(Of JToken)
            Return {Me}.Concat(DescendantNodes())
        End Function

        ''' <summary>
        ''' Retrieves all the descendants of this node.
        ''' </summary>
        ''' <returns></returns>
        Public Function DescendantNodes() As IEnumerable(Of JToken)
            Dim lNodes As New List(Of JToken)
            LoadListRecursive(lNodes, Nodes)
            Return lNodes
        End Function

        Private Sub LoadListRecursive(list As List(Of JToken), nodes As IEnumerable(Of JToken))
            For Each node As JToken In nodes
                list.Add(node)
                LoadListRecursive(list, node.Nodes)
            Next
        End Sub
    End Class
End Namespace