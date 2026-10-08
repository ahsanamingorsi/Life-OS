$ErrorActionPreference = 'Stop'
$appDirectory = Join-Path (Split-Path $PSScriptRoot -Parent) 'DesktopApp'
$pidFile = Join-Path $appDirectory 'lifeos.pid'
if (Test-Path -LiteralPath $pidFile) {
    $serverId = [int](Get-Content -LiteralPath $pidFile)
    $server = Get-Process -Id $serverId -ErrorAction SilentlyContinue
    $expected = Join-Path $appDirectory 'LifeOS.MVC.exe'
    if ($server -and $server.Path -eq $expected) { Stop-Process -Id $serverId }
    Remove-Item -LiteralPath $pidFile
}
