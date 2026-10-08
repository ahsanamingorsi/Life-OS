$ErrorActionPreference = 'Stop'
$base = 'http://127.0.0.1:5199'
$marker = 'StudyTask-QA-' + [guid]::NewGuid().ToString('N')
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
function Query($sql) { $result = sqlcmd -S '.\SQLEXPRESS' -d LifeOSDb -E -C -h -1 -W -Q "SET NOCOUNT ON; $sql"; if ($LASTEXITCODE) { throw 'SQL failed' }; ($result | Where-Object {$_.Trim()}) -join '' }
function Post($action,$body,$page) {
 $html = (Invoke-WebRequest ($base+$page) -WebSession $session).Content
 $token = [regex]::Match($html,'name="__RequestVerificationToken" type="hidden" value="([^"]+)"').Groups[1].Value
 if (!$token) { throw 'Missing CSRF token' }
 $body['__RequestVerificationToken'] = $token
 Invoke-WebRequest ($base+'/Study/'+$action) -Method Post -Body $body -WebSession $session -Headers @{Referer=$base+$page} | Out-Null
}
function Assert($condition,$label) { if (!$condition) { throw "FAIL $label" }; "PASS $label" }
try {
 Post 'CreateSubject' @{Name=$marker;Category='General';Status='Active';TargetHours=4} '/Study'
 $subject = [int](Query "SELECT Id FROM StudySubjects WHERE Name='$marker'")
 $page = "/Study/Subject/$subject"
 Post 'AddTask' @{StudySubjectId=$subject;Title=$marker;Notes='Practice questions';DueDate='2026-10-12'} $page
 $task = [int](Query "SELECT Id FROM StudyTasks WHERE StudySubjectId=$subject")
 Assert ($task -gt 0) 'Task persists under subject'
 Post 'UpdateTask' @{Id=$task;Title=$marker+' edited';Notes='Updated';DueDate='2026-10-13'} $page
 Assert ((Query "SELECT Notes FROM StudyTasks WHERE Id=$task") -eq 'Updated') 'Task editing persists'
 Post 'ToggleTask' @{id=$task} $page
 Assert ((Query "SELECT COUNT(*) FROM StudyTasks WHERE Id=$task AND IsCompleted=1 AND CompletedAt IS NOT NULL") -eq '1') 'Completion saves timestamp'
 Post 'ToggleTask' @{id=$task} $page
 Assert ((Query "SELECT COUNT(*) FROM StudyTasks WHERE Id=$task AND IsCompleted=0 AND CompletedAt IS NULL") -eq '1') 'Reopening clears completion'
 Post 'AddTask' @{StudySubjectId=$subject;Title='   '} $page
 Assert ((Query "SELECT COUNT(*) FROM StudyTasks WHERE StudySubjectId=$subject") -eq '1') 'Blank task rejected'
 Post 'DeleteTask' @{id=$task} $page
 Assert ((Query "SELECT COUNT(*) FROM StudyTasks WHERE Id=$task") -eq '0') 'Task deletion works'
 Post 'AddTask' @{StudySubjectId=$subject;Title=$marker+' cascade'} $page
 Post 'DeleteSubject' @{id=$subject} $page
 Assert ((Query "SELECT COUNT(*) FROM StudyTasks WHERE StudySubjectId=$subject") -eq '0') 'Subject deletion removes its tasks'
} finally {
 if ($subject) { Query "DELETE FROM StudySubjects WHERE Id=$subject AND Name='$marker'" | Out-Null }
 Query "DELETE FROM ActivityLogs WHERE Detail LIKE '%$marker%'" | Out-Null
}
