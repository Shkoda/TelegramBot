namespace Shkoda.Telegram.Bot
module BitBucket =
    open FSharp.Data
    open System
    open System.Text

    let bitbucketApiUrl = "https://api.bitbucket.org/2.0/"
    let cuttlefishCommitsUrl = bitbucketApiUrl + "repositories/reddotsquaresolutions/unity-cuttlefish/commits?page="
    let userInfoUrl = bitbucketApiUrl + "user"
   
    let basicAuthHeaderValue (username:string) (password:string) = 
        let base64Encode (s: string) = 
            Convert.ToBase64String(Encoding.UTF8.GetBytes(s))
        sprintf "%s:%s" username password |> base64Encode |> sprintf "Basic %s"

    let authorizedRequest url email password = 
        try
            Http.RequestString (url,  headers = ["Authorization", basicAuthHeaderValue email password]) |> Some
        with 
        | ex -> printf "AuthorizedRequest exception: %s" ex.Message; None

   

    let getCommitsFromPage page email pass=
         let url = sprintf "%s%i"cuttlefishCommitsUrl page
         //Http.RequestString (url,  headers = ["Authorization", basicAuthHeaderValue email pass])
         authorizedRequest url email pass
        
    let getCommitList email pass = 
        let isAuthor (email:string)(commit:Json.CommitsResponse.Value) = 
            commit.Author.Raw.Contains email

        let isMergeCommit(commit:Json.CommitsResponse.Value) =
            commit.Message.Contains "Merge remote-tracking branch" || commit.Message.Contains "Merged in"

        let response = getCommitsFromPage 1 email pass 
        match response with
            |Some commits -> 
                commits
                |> Json.CommitsResponse.Parse 
                |> fun p -> p.Values
                |> Array.filter (fun c -> isAuthor email c) 
                |> Array.filter (fun c -> not (isMergeCommit c)) 
                |> Some
            |None -> None
    let isValidConfig (config: Json.UserConfig.Root) = 
        let auth = authorizedRequest userInfoUrl config.Bitbucket.Email config.Bitbucket.Password
        auth.IsSome

    let getUserCommits (config: Json.UserConfig.Root) = 
        if isValidConfig (config) then 
            let commits = getCommitList config.Bitbucket.Email config.Bitbucket.Password
            match commits with
            | Some c -> TelegramMarkdown.commitsByDateText c 
            | None -> "Failed to load commits"
        else "Invalid credentials"      

    let getCommitStatistics telegramUsername = 
        let storedConfig = UserConfigProvider.getUser telegramUsername
        match storedConfig with
            | Some config -> getUserCommits config
            | None -> "Credentials not found"
