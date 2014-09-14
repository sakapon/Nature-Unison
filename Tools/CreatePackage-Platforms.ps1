$msbuild = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"

cd ..\Platforms
Invoke-Expression ($msbuild + " Platforms.sln /p:Configuration=Release /t:Clean")
Invoke-Expression ($msbuild + " Platforms.sln /p:Configuration=Release /t:Rebuild")

cd .\Kinect
.\NuGetPackup.exe
move *.nupkg ..\..\Published -Force

cd ..\Kinect2
.\NuGetPackup.exe
move *.nupkg ..\..\Published -Force

cd ..\Leap
.\NuGetPackup.exe
move *.nupkg ..\..\Published -Force
