' Outputs current date in date version format: yy.mm.dd
Dim Today
Today = Date
WScript.Echo Right(Year(Today), 2) & "." & Month(Today) & "." & Day(Today)