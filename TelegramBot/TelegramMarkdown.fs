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

    let userAsString (user: Json.UserConfig.Root) = 
        let repositoriesAsString (repos: Json.UserConfig.Repository[]) = 
            let repositoryAsString (repo: Json.UserConfig.Repository) = 
                let bitbucketUrl = "https://bitbucket.org"
                sprintf "%s/%s/%s" bitbucketUrl repo.Owner repo.Name
            repos |> Array.map repositoryAsString |> String.concat("\n")  
        
        sprintf "*Bitbucked account:*\n_login:_%s\n_password_:%s\n\n*Repositories:*\n%s" 
            user.Bitbucket.Email 
            user.Bitbucket.Password 
            (repositoriesAsString (user.Bitbucket.Repositories))



