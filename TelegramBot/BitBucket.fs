﻿namespace Shkoda.Telegram.Bot
module BitBucket =

    let bitbucketApiUrl = "https://api.bitbucket.org/2.0/"
    let cuttlefishCommitsUrl = bitbucketApiUrl + "repositories/reddotsquaresolutions/unity-cuttlefish/commits?page="
    let userInfoUrl = bitbucketApiUrl + "user"
   
    let getCommitsFromPage page email pass=
         let url = sprintf "%s%i"cuttlefishCommitsUrl page
         Http.authorizedRequest url email pass
        
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
        let auth =  Http.authorizedRequest userInfoUrl config.Bitbucket.Email config.Bitbucket.Password
        auth.IsSome

    let getUserCommits (config: Json.UserConfig.Root) = 
        if isValidConfig (config) then 
            let commits = getCommitList config.Bitbucket.Email config.Bitbucket.Password
            match commits with
            | Some c -> TelegramMarkdown.commitsByDateText c 
            | None -> "Failed to load commits"
        else "Invalid credentials"      

    let getCommitStatistics telegramUsername = 
        let credentials = UserConfigProvider.getUser telegramUsername
        getUserCommits credentials

    let setLogin (sender:string)(args:string[]) = 
        match args.Length with
        | 0 -> "/setlogin should have one argument"
        | _ -> sprintf "Login was updated. Current credentials:\n\n%s" (TelegramMarkdown.credentialsAsString(UserConfigProvider.saveEmail sender args.[0]))

    let setPassword (sender:string)(args:string[]) = 
        match args.Length with
        | 0 -> "/setpass should have one argument"
        | _ -> sprintf "Password was updated. Current credentials:\n\n%s" (TelegramMarkdown.credentialsAsString(UserConfigProvider.savePassword sender args.[0]))

