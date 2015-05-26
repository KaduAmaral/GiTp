WshShell = CreateObject("WScript.Shell")
strDesktop = WshShell.SpecialFolders("Desktop")
oMyShortCut= WshShell.CreateShortcut(strDesktop+"\GiTp.lnk")
oMyShortcut.IconLocation = objShell.CurrentDirectory + "GiTp.ico"
oMyShortCut.TargetPath = objShell.CurrentDirectory + "GiTp.exe" 
oMyShortCut.Description = "GiTp"
oMyShortCut.WorkingDirectory = "%HOMEPATH%"
oMyShortCut.Save