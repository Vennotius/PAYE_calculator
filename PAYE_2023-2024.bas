Attribute VB_Name = "Module1"

'  The ranges that it will reference in Excel could look like this:
'  Threshold    UpperLimit         Percentage
'  -            95 750.00          0%
'  95750        237100.00          18%
'  237100       370500.00          26%
'  370500        512800.00         31%
'  512800       673000.00          36%
'  673000       857900.00          39%
'  857900        1817000.00        41%
'  1817000      9999999999.00      45%
'
'  Max UIF
'  177.12


' This function computes the Netto from the Bruto amount
Function NettoAfterPAYEandUIF(bracketInfo As Range, maxUif As Double, brutoMonthlyIncome As Double) As Double
    Dim yearlyIncome As Double
    yearlyIncome = brutoMonthlyIncome * 12
    
    Dim monthlyTax As Double
    monthlyTax = 0

    Dim remainingIncome As Double
    remainingIncome = yearlyIncome
    
    ' Iterate over the bracketInfo rows
    Dim row As Range
    For Each row In bracketInfo.Rows
        Dim upperLimit As Double
        upperLimit = row.Cells(1, 2).Value
        
        Dim threshold As Double
        threshold = row.Cells(1, 1).Value
        
        Dim bracketWidth As Double
        bracketWidth = upperLimit - threshold

        Dim percentage As Double
        percentage = row.Cells(1, 3).Value

        Dim incomeInsideBracket As Double
        incomeInsideBracket = WorksheetFunction.Min(bracketWidth, remainingIncome)

        If incomeInsideBracket <= 0 Then Exit For

        monthlyTax = monthlyTax + incomeInsideBracket * percentage
        remainingIncome = remainingIncome - incomeInsideBracket
    Next row

    monthlyTax = monthlyTax / 12

    Dim uif As Double
    uif = WorksheetFunction.Min(brutoMonthlyIncome * 0.01, maxUif)

    Dim monthlyIncomeAfterTax As Double
    monthlyIncomeAfterTax = brutoMonthlyIncome - monthlyTax

    NettoAfterPAYEandUIF = monthlyIncomeAfterTax - uif
End Function

' This function computes the Bruto for the given target Netto
Function GetBrutoForTargetNettoAfterPAYEandUIF(bracketInfo As Range, maxUif As Double, targetNetto As Double) As Double
    Dim lowerBound As Double
    lowerBound = targetNetto
    
    Dim upperBound As Double
    upperBound = targetNetto * 2

    Dim candidateTarget As Double
    candidateTarget = 0

    Do While Abs(upperBound - lowerBound) > 0.01
        Dim middle As Double
        middle = (upperBound + lowerBound) / 2

        Dim candidateNetto As Double
        candidateNetto = NettoAfterPAYEandUIF(bracketInfo, maxUif, middle)

        If candidateNetto < targetNetto Then
            lowerBound = middle
        ElseIf candidateNetto > targetNetto Then
            upperBound = middle
        Else
            Exit Do
        End If

        candidateTarget = middle
    Loop

    GetBrutoForTargetNettoAfterPAYEandUIF = candidateTarget
End Function

