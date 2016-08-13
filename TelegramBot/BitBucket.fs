namespace Shkoda.Telegram.Bot
module BitBucket =
    open FSharp.Data
    open System
    open System.Text
    open System.Text.RegularExpressions

    let cuttlefishCommitsUrl = "https://api.bitbucket.org/2.0/repositories/reddotsquaresolutions/unity-cuttlefish/commits"
    //[Nn][Gg]-\d{1,}    pattern NG-****
    let taskTextPattern = Regex @"[Nn][Gg]-\d{1,}"
    
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

    let getCommitList email pass = 
        let response = Http.RequestString (cuttlefishCommitsUrl,  headers = ["Authorization", basicAuthHeaderValue email pass])
        let parsed = Json.CommitsResponse.Parse response 
        printf " %s\n" (parsed.Next) 
        parsed.Values
            |> Array.filter (fun c -> isAuthor email c) 
            |> Array.filter (fun c -> not (isMergeCommit c)) 
            |> Array.map commitShortInfo
        
    let groupByDate (commits:ShortCommitInfo[]) = 
        commits |> Array.groupBy commitDate