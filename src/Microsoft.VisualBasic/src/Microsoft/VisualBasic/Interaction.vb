' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.
Option Explicit On
Option Strict On

Namespace Microsoft.VisualBasic

    Partial Public Module Interaction
        Friend Function IIf(Of T)(ByVal condition As Boolean, ByVal truePart As T, ByVal falsePart As T) As T
            If condition Then
                Return truePart
            End If

            Return falsePart
        End Function
    End Module
End Namespace

