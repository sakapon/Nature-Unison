$msbuild = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"

cd ..\NatureUnison
Invoke-Expression ($msbuild + " NatureUnison.sln /p:Configuration=Release /t:Clean")
Invoke-Expression ($msbuild + " NatureUnison.sln /p:Configuration=Release /t:Rebuild")

cd .\Core
.\NuGetPackup.exe
move *.nupkg ..\..\Published -Force

cd ..\Leap
.\NuGetPackup.exe
move *.nupkg ..\..\Published -Force
