$revision = (git rev-list --count --first-parent HEAD) -join "`n"
$version = "0.5.0.$revision"
$year = (Get-Date).year
$copyright = "Copyright $year - John Andrews"

$osWindows = Test-Path env:ProgramFiles

if ($osWindows) {
    $dotnet_cmd= "dotnet.exe"
} else {
    $dotnet_cmd= "dotnet"
}