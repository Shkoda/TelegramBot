namespace Shkoda.Telegram.Bot
module TelegramMarkdown =
    open System
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

    let commitsToString (commits : Json.CommitsResponse.Value[]) = 
        commits
                |> Array.map (fun c -> sprintf "%s" c.Message ) 
                |> Array.map  markJiraTasksInCommitMessage
                |> String.concat ("")         

    let commitsByDateText (commits : Json.CommitsResponse.Value[]) = 
        let groupByDate (commits: Json.CommitsResponse.Value[]) = 
            commits |> Array.groupBy (fun c -> c.Date.Date)

        let datedCommitsToString ((date: DateTime),(commits: Json.CommitsResponse.Value[])) =                 
            sprintf "*%s*\n%s" (date.ToLongDateString()) (commitsToString(commits))

        groupByDate commits 
           |> Array.map datedCommitsToString
           |> String.concat ("\n")

    let passwordSafeView (rawPassword: string) =
        match rawPassword.Length with
        | 0 -> "PASSWORD IS NOT DEFINED"
        | _ ->   rawPassword.ToCharArray() |> Array.map (fun c -> "#") |> String.Concat
      

    let credentialsAsString (user: Json.UserConfig.Root) = 
        let repositoriesAsString (repos: Json.UserConfig.Repository[]) = 
            let repositoryAsString (repo: Json.UserConfig.Repository) = 
                let bitbucketUrl = "https://bitbucket.org"
                sprintf "%s/%s/%s" bitbucketUrl repo.Owner repo.Name
            repos |> Array.map repositoryAsString |> String.concat("\n")  
        let jiraConfigAsString =
            sprintf "*Jira account:*\n_url:_    %s\n_login:_    %s\n_password:_    %s\n\n" 
                (user.Jira.Url) (user.Jira.Email) (passwordSafeView(user.Jira.Password))
        
        let bitbucketAsString = sprintf "*Bitbucked account:*\n_login:_    %s\n_password:_    %s\n\n*Repositories:*\n%s\n" 
                                    (user.Bitbucket.Email)
                                    (passwordSafeView(user.Bitbucket.Password))
                                    (repositoriesAsString (user.Bitbucket.Repositories))

        jiraConfigAsString + bitbucketAsString

    let jiraUserAsString (user:Json.JiraSearchUser.Root option) = 
        match user with
        | Some data -> sprintf "*Jira profile:*\nusername:    %s\nfull name:    %s\nemail:    %s" 
                            data.Name data.DisplayName data.EmailAddress
        | None -> sprintf "jira user not found"

    let sortIssuesByAssignee (i1:Json.ActiveIssuesResponse.Issue)(i2:Json.ActiveIssuesResponse.Issue) = 
       match i1.Fields.Assignee, i2.Fields.Assignee with
       |None, None -> 0
       |Some _, None -> -1
       |None, Some _ -> 1
       |Some s1, Some s2 -> compare s1.DisplayName s2.DisplayName

    let issuesToString (response:Json.ActiveIssuesResponse.Root option) =   
        let toString (issue: Json.ActiveIssuesResponse.Issue) = 
            let url = jiraTaskRootUrl + issue.Key
            let text = issue.Fields.Summary  
            let status = issue.Fields.Status.Name 
            sprintf "[%s](%s) %s -- _%s_\n"  issue.Key url text status

        let assignedIssues (issues : Json.ActiveIssuesResponse.Issue[]) = 
            issues
            |> Array.filter (fun i -> i.Fields.Assignee.IsSome)
            |> Array.map toString
            |> String.Concat 
        let unassignedIssues (issues : Json.ActiveIssuesResponse.Issue[]) = 
            issues
            |> Array.filter (fun i -> i.Fields.Assignee.IsNone)
            |> Array.map toString
            |> String.Concat 

        let issuestoString (issues : Json.ActiveIssuesResponse.Issue[]) = 
            sprintf "*Assigned to you:*\n%s\n*Unassigned:*\n%s" (assignedIssues issues) (unassignedIssues issues)

        match response with
        |Some issues -> issuestoString issues.Issues
        |None -> "Issues not found"

    let toInlineLeyboard (buttons: DataClasses.InlineButton[][]) = 
        let buttonMarkup (button: DataClasses.InlineButton) = 
           sprintf "{\"text\": \"%s\", \"callback_data\": \"%s\"}" button.text button.callback
        let rowMarkup (buttons:DataClasses.InlineButton[]) = 
           sprintf "[%s]" (String.concat "," (buttons |> Array.map buttonMarkup))
        let keyboardMarkup (buttons: DataClasses.InlineButton[][]) = 
           sprintf "[%s]" (String.concat "," (buttons |> Array.map rowMarkup))
        sprintf "{\"inline_keyboard\":%s}" (keyboardMarkup buttons)


