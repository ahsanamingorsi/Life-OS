$ErrorActionPreference='Stop'
$base='http://localhost:5190'
$marker='LifeOS-QA-'+[guid]::NewGuid().ToString('N')
$session=New-Object Microsoft.PowerShell.Commands.WebRequestSession
function Page($path) { Invoke-WebRequest ($base+$path) -WebSession $session }
function Post($path,$body,$page) {
 $html=(Page $page).Content
 $token=[regex]::Match($html,'name="__RequestVerificationToken" type="hidden" value="([^"]+)"').Groups[1].Value
 if(!$token){throw "Missing token on $page"}
 $body['__RequestVerificationToken']=$token
 Invoke-WebRequest ($base+$path) -Method Post -Body $body -WebSession $session -Headers @{Referer=$base+$page}
}
function Query($sql) { $result=sqlcmd -S '.\SQLEXPRESS' -d LifeOSDb -E -C -h -1 -W -Q "SET NOCOUNT ON; $sql"; if($LASTEXITCODE -ne 0){throw 'SQL check failed'}; ($result | Where-Object { $_.Trim() }) -join '' }
function Assert($condition,$label) {if(!$condition){throw "FAILED: $label"}; Write-Output "PASS: $label"}
try {
 $r=Post '/PersonalTasks/Create' @{Title=$marker;Priority='A';DueDate='2026-10-07';returnUrl='/Planner'} '/Planner'
 Assert ($r.BaseResponse.RequestMessage.RequestUri.AbsolutePath -eq '/Planner') 'Quick add returns to My day'
 $id=[int](Query "SELECT Id FROM PersonalTasks WHERE Title='$marker'")
 $null=Post '/PersonalTasks/ToggleComplete' @{id=$id;returnUrl='/Planner'} '/Planner'
 Assert ((Query "SELECT CAST(IsCompleted AS int) FROM PersonalTasks WHERE Id=$id") -eq '1') 'Task completion persists'
 $null=Post '/Habits/Create' @{Name=$marker;Category='General';Frequency='Daily'} '/Habits'
 $habit=[int](Query "SELECT Id FROM Habits WHERE Name='$marker'")
 $null=Post '/Habits/MarkComplete' @{habitId=$habit;returnUrl='/Planner'} '/Planner'
 Assert ((Query "SELECT COUNT(*) FROM HabitLogs WHERE HabitId=$habit") -eq '1') 'Planner habit completion persists'
 $null=Post '/Habits/UnmarkComplete' @{habitId=$habit;returnUrl='/Planner'} '/Planner'
 Assert ((Query "SELECT COUNT(*) FROM HabitLogs WHERE HabitId=$habit") -eq '0') 'Habit undo works'
 $null=Post '/Projects/Add' @{Title=$marker;Category='General';Status='Active';Priority='A'} '/Projects'
 $project=[int](Query "SELECT Id FROM Projects WHERE Title='$marker'")
 $null=Post '/Projects/AddTask' @{Title=$marker;ProjectId=$project;Priority='A'} "/Projects/Detail/$project"
 $task=[int](Query "SELECT Id FROM ProjectTasks WHERE Title='$marker'")
 $null=Post '/Projects/MoveTask' @{taskId=$task;status='Done'} "/Projects/Detail/$project"
 Assert ((Query "SELECT ProgressPercent FROM Projects WHERE Id=$project") -eq '100') 'Completing project task updates progress'
 $null=Post '/Projects/MoveTask' @{taskId=$task;status='Todo'} "/Projects/Detail/$project"
 Assert ((Query "SELECT Status FROM Projects WHERE Id=$project") -eq 'Active') 'Reopening project task reactivates project'
 $null=Post '/TechGrowth/Create' @{Name=$marker;Category='Other';Level='Beginner';WeeklyTargetHours=1} '/TechGrowth'
 $skill=[int](Query "SELECT Id FROM TechSkills WHERE Name='$marker'")
 $null=Post '/TechGrowth/LogHours' @{skillId=$skill;hours=1;progress=25} '/TechGrowth'
 Assert ([double](Query "SELECT HoursLogged FROM TechSkills WHERE Id=$skill") -eq 1) 'Skill session updates hours'
 $null=Post '/Study/CreateSubject' @{Name=$marker;Category='General';Status='Active';TargetHours=4} '/Study'
 $subject=[int](Query "SELECT Id FROM StudySubjects WHERE Name='$marker'")
 $null=Post '/Study/LogSession' @{StudySubjectId=$subject;DurationHours=1;FocusRating=5;SessionDate='2026-10-07'} "/Study/Subject/$subject"
 Assert ((Query "SELECT ProgressPercent FROM StudySubjects WHERE Id=$subject") -eq '25') 'Study session updates progress'
 $studySession=[int](Query "SELECT Id FROM StudySessions WHERE StudySubjectId=$subject")
 $null=Post '/Study/DeleteSession' @{sessionId=$studySession;subjectId=$subject} "/Study/Subject/$subject"
 Assert ((Query "SELECT ProgressPercent FROM StudySubjects WHERE Id=$subject") -eq '0') 'Study session deletion recalculates progress'
 $null=Post '/Money/Create' @{'Entry.Description'=$marker;'Entry.Type'='Expense';'Entry.Category'='Other';'Entry.Amount'='12.50';'Entry.Date'='2026-10-07'} '/Money'
 Assert ([decimal](Query "SELECT Amount FROM MoneyTransactions WHERE Description='$marker'") -eq 12.50) 'Money transaction persists exact amount'
 $transaction=[int](Query "SELECT Id FROM MoneyTransactions WHERE Description='$marker'")
 $null=Post '/Money/Delete' @{id=$transaction} '/Money'
 Assert ((Query "SELECT COUNT(*) FROM MoneyTransactions WHERE Description='$marker'") -eq '0') 'Transaction deletion updates ledger'
 $null=Post '/Money/Create' @{'Entry.Description'=$marker;'Entry.Type'='Expense';'Entry.Category'='Other';'Entry.Amount'='0';'Entry.Date'='2026-10-07'} '/Money'
 Assert ((Query "SELECT COUNT(*) FROM MoneyTransactions WHERE Description='$marker'") -eq '0') 'Zero amount is rejected'
 if ((Query "SELECT COUNT(*) FROM MonthlyBudgets WHERE Month='2099-12-01'") -eq '0') {
  $null=Post '/Money/Budget' @{month='2099-12-01';amount='5000'} '/Money'
  Assert ([decimal](Query "SELECT Amount FROM MonthlyBudgets WHERE Month='2099-12-01'") -eq 5000) 'Monthly budget persists'
  $null=Query "DELETE FROM MonthlyBudgets WHERE Month='2099-12-01' AND Amount=5000"
 }
 if ((Query "SELECT COUNT(*) FROM SpiritualEntries WHERE Date='2099-12-31'") -eq '0') {
  $null=Post '/Spiritual/Save' @{Date='2099-12-31';PrayersDone=3;PrayersTotal=5;GratitudeNote=$marker} '/Spiritual'
  Assert ((Query "SELECT PrayersDone FROM SpiritualEntries WHERE GratitudeNote='$marker'") -eq '3') 'Spiritual routine persists'
 }
 $null=Post '/PersonalTasks/Create' @{Title='';Priority='A'} '/PersonalTasks'
 Assert ((Query "SELECT COUNT(*) FROM PersonalTasks WHERE Title='' AND CreatedAt > DATEADD(minute,-5,GETDATE())") -eq '0') 'Blank task is rejected'
} finally {
 if ($subject) { $null=Query "DELETE FROM ActivityLogs WHERE Type='Study' AND ReferenceId=$subject AND Action='SessionLogged'" }
 $null=Query "DELETE FROM SpiritualEntries WHERE GratitudeNote='$marker'"
 $null=Query "DELETE FROM MoneyTransactions WHERE Description='$marker'; DELETE FROM PersonalTasks WHERE Title='$marker'; DELETE FROM Projects WHERE Title='$marker'; DELETE FROM Habits WHERE Name='$marker'; DELETE FROM TechSkills WHERE Name='$marker'; DELETE FROM StudySubjects WHERE Name='$marker'; DELETE FROM ActivityLogs WHERE Detail LIKE '%$marker%';"
 Write-Output 'Temporary test records cleaned up.'
}

