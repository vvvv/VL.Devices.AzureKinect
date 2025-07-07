@echo off
setlocal enabledelayedexpansion

:: Input and output folders
set "inputFolder=.\help"
set "outputFolder=.\help-femto"

:: Create output folder if it doesn't exist
if not exist "%outputFolder%" (
    mkdir "%outputFolder%"
)

:: Loop through all .vl files
for %%f in ("%inputFolder%\*.vl") do (
    set "filename=%%~nxf"
    powershell -NoProfile -Command ^
        "$text = Get-Content -Raw '%%f';" ^
        "$text = $text -replace 'Location=\"VL.Devices.AzureKinect\"', 'Location=\"VL.Devices.AzureKinect.Femto\"';" ^
        "Set-Content -Encoding UTF8 '%outputFolder%\!filename!' $text"
)

echo Done.
pause
