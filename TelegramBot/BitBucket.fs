namespace Shkoda.Telegram.Bot
module BitBucket =
    open FSharp.Data
    open System
    open System.Text
    open System.Text.RegularExpressions

    let cuttlefishCommitsUrl = "https://api.bitbucket.org/2.0/repositories/reddotsquaresolutions/unity-cuttlefish/commits?page="
   
    let basicAuthHeaderValue (username:string) (password:string) = 
        let base64Encode (s:string) = 
            let bytes = Encoding.UTF8.GetBytes(s)
            Convert.ToBase64String(bytes)
        sprintf "%s:%s" username password |> base64Encode |> sprintf "Basic %s"// |>  Authorization 

    let isAuthor (email:string)(commit:Json.CommitsResponse.Value) =
        commit.Author.Raw.Contains email

    let isMergeCommit(commit:Json.CommitsResponse.Value) =
        commit.Message.Contains "Merge remote-tracking branch" || commit.Message.Contains "Merged in"
   
    type ShortCommitInfo = {message: string; date: DateTime; link: string}

    let commitShortInfo (commit:Json.CommitsResponse.Value) = 
        {message = commit.Message; date = commit.Date; link = commit.Parents.[0].Links.Html.Href}     

    let commitDate (commit:ShortCommitInfo) = 
        commit.date.Date  
     
    let getCommitsFromPage page email pass=
         let url = sprintf "%s%i"cuttlefishCommitsUrl page
         Http.RequestString (url,  headers = ["Authorization", basicAuthHeaderValue email pass])
        
    let getCommitList email pass = 
        let response = getCommitsFromPage 1 email pass
        let parsed = Json.CommitsResponse.Parse response 
        printf " %s\n" (parsed.Next) 
        parsed.Values
            |> Array.filter (fun c -> isAuthor email c) 
            |> Array.filter (fun c -> not (isMergeCommit c)) 
            |> Array.map commitShortInfo
        
    let groupByDate (commits:ShortCommitInfo[]) = 
        commits |> Array.groupBy commitDate
    
   
    let stringify (commits : ShortCommitInfo[]) = 
        commits
                |> Array.map (fun c -> sprintf "%s" c.message ) 
                |> Array.map (fun m -> TelegramMarkdown.markJiraTasksInCommitMessage(m))
                |> String.concat ("")

    let formattedMessage (commits : ShortCommitInfo[]) =  
        groupByDate commits 
           |> Array.map (fun (d, a) -> d, stringify(a))
           |> Array.map (fun (d, s) -> sprintf "*%s*\n%s" (d.ToLongDateString()) s)
           |> String.concat ("\n")

    let getUserCommits (config: Json.UserConfig.Root) = 
        let commits = getCommitList config.Bitbucket.Email config.Bitbucket.Password
        formattedMessage commits

     

    let getCommitStatistics telegramUsername = 
        let storedConfig = UserConfigProvider.getUser telegramUsername
        match storedConfig with
            | Some config -> getUserCommits config
            | None -> "unknown user"

    

