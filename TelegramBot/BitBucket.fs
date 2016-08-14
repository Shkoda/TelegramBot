namespace Shkoda.Telegram.Bot
module BitBucket =
    open FSharp.Data
    open System
    open System.Text

    let cuttlefishCommitsUrl = "https://api.bitbucket.org/2.0/repositories/reddotsquaresolutions/unity-cuttlefish/commits?page="
   
    let basicAuthHeaderValue (username:string) (password:string) = 
        let base64Encode (s: string) = 
            Convert.ToBase64String(Encoding.UTF8.GetBytes(s))
        sprintf "%s:%s" username password |> base64Encode |> sprintf "Basic %s"
   
    let getCommitsFromPage page email pass=
         let url = sprintf "%s%i"cuttlefishCommitsUrl page
         Http.RequestString (url,  headers = ["Authorization", basicAuthHeaderValue email pass])
        
    let getCommitList email pass = 
        let isAuthor (email:string)(commit:Json.CommitsResponse.Value) = 
            commit.Author.Raw.Contains email

        let isMergeCommit(commit:Json.CommitsResponse.Value) =
            commit.Message.Contains "Merge remote-tracking branch" || commit.Message.Contains "Merged in"

        let response = getCommitsFromPage 1 email pass
        let parsed = Json.CommitsResponse.Parse response 
        parsed.Values
            |> Array.filter (fun c -> isAuthor email c) 
            |> Array.filter (fun c -> not (isMergeCommit c)) 

    let getUserCommits (config: Json.UserConfig.Root) = 
        let commits = getCommitList config.Bitbucket.Email config.Bitbucket.Password
        TelegramMarkdown.commitsByDateText commits 

    let getCommitStatistics telegramUsername = 
        let storedConfig = UserConfigProvider.getUser telegramUsername
        match storedConfig with
            | Some config -> getUserCommits config
            | None -> "unknown user"
