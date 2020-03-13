set CurDir=%~dp0
set LibName=%1
xcopy %LibName%.dll %CurDir%..\..\Checkmate\Assets\Plugins\ManagedLib\ /y