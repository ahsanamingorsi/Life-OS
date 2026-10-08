$ErrorActionPreference = 'Stop'
$appDirectory = Join-Path (Split-Path $PSScriptRoot -Parent) 'DesktopApp'
$executable = Join-Path $appDirectory 'LifeOS.MVC.exe'
$address = 'http://127.0.0.1:5199'
$pidFile = Join-Path $appDirectory 'lifeos.pid'
function IsReady {
    try { (Invoke-RestMethod "$address/lifeos-health" -TimeoutSec 2).application -eq 'LifeOS' } catch { $false }
}
try {
    if (!(Test-Path -LiteralPath $executable)) { throw 'Life OS build is missing. Run Update Life OS.cmd first.' }
    if (!(IsReady)) {
        if (Get-NetTCPConnection -LocalPort 5199 -State Listen -ErrorAction SilentlyContinue) { throw 'Port 5199 is in use by another app.' }
        $server = Start-Process -FilePath $executable -WorkingDirectory $appDirectory -ArgumentList '--urls', $address -WindowStyle Hidden -RedirectStandardOutput (Join-Path $appDirectory 'startup.log') -RedirectStandardError (Join-Path $appDirectory 'error.log') -PassThru
        $server.Id | Set-Content -LiteralPath $pidFile
        $ready = $false
        for ($attempt = 0; $attempt -lt 40; $attempt++) {
            if (IsReady) { $ready = $true; break }
            $server.Refresh()
            if ($server.HasExited) { throw "Life OS could not start. See $appDirectory\error.log" }
            Start-Sleep -Milliseconds 500
        }
        if (!$ready) { throw "Life OS took too long to start. See $appDirectory\startup.log" }
    }
    $edge = "${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe"
    $chrome = "$env:ProgramFiles\Google\Chrome\Application\chrome.exe"
    if (Test-Path -LiteralPath $edge) { $browserApp = $edge }
    elseif (Test-Path -LiteralPath $chrome) { $browserApp = $chrome }
    else { throw "Install Microsoft Edge or Google Chrome to open the Life OS desktop window." }
    Start-Process -FilePath $browserApp -ArgumentList "--app=$address"
} catch {
    Add-Type -AssemblyName PresentationFramework
    [System.Windows.MessageBox]::Show($_.Exception.Message, 'Life OS', 'OK', 'Error') | Out-Null
    exit 1
}

