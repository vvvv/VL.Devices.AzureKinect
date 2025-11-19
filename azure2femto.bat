@echo off

echo Making femto files...

setlocal enabledelayedexpansion

:: Input and output folders
set "helpInFolder=.\help"
set "helpOutFolder=.\help-femto"
set "vlInFolder=."
set "vlOutFolder=.\vl-femto"

:: Create output folder if it doesn't exist
if not exist "%helpOutFolder%" (
    mkdir "%helpOutFolder%"
)

if not exist "%vlOutFolder%" (
    mkdir "%vlOutFolder%"
)

powershell Copy-Item -Path "%helpInFolder%\help.xml", "%helpInFolder%\lankydude.fbx" -Destination "%helpOutFolder%"

:: Loop through all .vl files and change references
for %%f in ("%helpInFolder%\*.vl") do (
    set "filename=%%~nxf"
    powershell -NoProfile -Command ^
        "$text = Get-Content -Raw '%%f';" ^
        "$text = $text -replace 'Location=\"VL.Devices.AzureKinect\"', 'Location=\"VL.Devices.AzureKinect.Femto\"';" ^
        "Set-Content -Encoding UTF8 '%helpOutFolder%\!filename!' $text"
)

:: Loop through all .vl files and change references
for %%f in ("%vlInFolder%\*.vl") do (
    set "filename=%%~nxf"
	
	rem Default newname
    set "newname="

    rem Check if the filename ends with .HDE.vl
    if /I "!filename:~-7!"==".HDE.vl" (
        rem Remove the last 7 characters (.HDE.vl) and append .Femto.HDE.vl
        set "base=!filename:~0,-7!"
        set "newname=!base!.Femto.HDE.vl"
    ) else (
        rem Remove the last 3 characters (.vl) and append .Femto.vl
        set "base=!filename:~0,-3!"
        set "newname=!base!.Femto.vl"
    )	
	
    powershell -NoProfile -Command ^
        "$text = Get-Content -Raw '%%f';" ^
        "$text = $text -replace 'Location=\"VL.Devices.AzureKinect\"', 'Location=\"VL.Devices.AzureKinect.Femto\"';" ^
        "Set-Content -Encoding UTF8 '%vlOutFolder%\!newname!' $text"
)

echo Done.
pause
