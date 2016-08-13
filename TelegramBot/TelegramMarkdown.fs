namespace Shkoda.Telegram.Bot
module TelegramMarkdown =

    open System.Text.RegularExpressions

    let taskPattern = @"\[?[Nn][Gg]-\d{1,}]?"
    let regex = Regex taskPattern
    let jiraTaskRootUrl = "https://reddotsquare.atlassian.net/browse/"


    let markJiraTasksInCommitMessage (message:string) = 
         let namedTaskLink (m : Match) = 
            let taskName = m.Value.Replace("[", "").Replace("]", "")
            sprintf "[%s](%s%s) "  taskName  jiraTaskRootUrl taskName
  
         Regex.Replace(message, taskPattern,
                (fun (x : Match) -> namedTaskLink (x)),
                RegexOptions.IgnoreCase)
