set WshShell = WScript.CreateObject("WScript.Shell" )
strStartup = WshShell.SpecialFolders("AppData" )
set oMyShortCut = WshShell.CreateShortcut(strStartup+"\GiTp.lnk" )
oMyShortCut.TargetPath = objShell.CurrentDirectory+"GiTp.exe" 
oMyShortCut.IconLocation = objShell.CurrentDirectory+"GiTp.ico"
oMyShortCut.Description = "GiTp"
oMyShortCut.WorkingDirectory = "%HOMEPATH%"
oMyShortCut.Save