dotnet publish -c Release -f net8.0 -r linux-x64
del /q "C:\Users\Filip\Desktop\LinuxFolder\publish\*.*"
move "C:\Users\Filip\Desktop\C#\CrymexEngine\CrymexEngine\bin\Release\net8.0\linux-x64\publish\*.*" "C:\Users\Filip\Desktop\LinuxFolder\publish\"