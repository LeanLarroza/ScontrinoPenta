del "C:\Proyectos VB.Net\ScontrinoPenta\*.zip" /s /q
xcopy /s "C:\Proyectos VB.Net\ScontrinoPenta\Output" "C:\Proyectos VB.Net\ScontrinoPenta\Output2\"
del "C:\Proyectos VB.Net\ScontrinoPenta\Output2\*.config" /s /q
del "C:\Proyectos VB.Net\ScontrinoPenta\Output2\*.application" /s /q
del "C:\Proyectos VB.Net\ScontrinoPenta\Output2\*.manifest" /s /q
del "C:\Proyectos VB.Net\ScontrinoPenta\Output2\*.pdb" /s /q
del "C:\Proyectos VB.Net\ScontrinoPenta\Output2\*.ini" /s /q
del "C:\Proyectos VB.Net\ScontrinoPenta\Output2\*.xml" /s /q
del "C:\Proyectos VB.Net\ScontrinoPenta\Output2\*.lnk" /s /q
@RD /S /Q "C:\Proyectos VB.Net\ScontrinoPenta\Output2\Logs"
@RD /S /Q "C:\Proyectos VB.Net\ScontrinoPenta\Output2\app.publish"
"C:\Program Files\7-Zip\7z" a -tzip "C:\Proyectos VB.Net\ScontrinoPenta\ScontrinoPenta.zip" "C:\Proyectos VB.Net\ScontrinoPenta\Output2\*.*" -mx5
"C:\Program Files\7-Zip\7z" x "C:\Proyectos VB.Net\ScontrinoPenta\ScontrinoPenta.zip" -o"C:\Proyectos VB.Net\ScontrinoPenta\ScontrinoPenta" -aoa
@RD /S /Q "C:\Proyectos VB.Net\ScontrinoPenta\Output2"
echo File ScontrinoPenta.zip / Cartella ScontrinoPenta creati
start %windir%\explorer.exe "C:\Proyectos VB.Net\ScontrinoPenta\ScontrinoPenta" 
pause