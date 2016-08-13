namespace Shkoda.Telegram.Bot
module BitBucket =
    open System
    open FSharp.Data
    open FSharp.Data.JsonExtensions
    open System.Net
    open System
    open System.Globalization
    open System.IO
    open System.Net
    open System.Text
    open System.Text.RegularExpressions
    open System.Reflection
    open System.Runtime.CompilerServices
    open System.Runtime.InteropServices
    open FSharp.Data.Runtime
//https://bitbucket.org/site/oauth2/authorize?client_id=MuzSM5B3T2aw8ZGSq4&response_type=code
//https://bitbucket.org/site/oauth2/authorize?client_id=MuzSM5B3T2aw8ZGSq4&response_type=token

//https://api.telegram.org/bot231953668:AAHUQ8HEQr8Scnl_ViDZ6dWtH9JXDeMy5hw/getUpdates#access_token=mUEswUVix2VSBpHG3qa1XIgpO-_V5hR7chFnfaglsgo5NvKxhWqW8WeP4BCuvj6x8wPzTrrh3eEh7JJO0rM%3D&scopes=pipeline%3Avariable+webhook+snippet%3Awrite+issue%3Awrite+pullrequest+repository%3Awrite+project+team+account%3Awrite&expires_in=3600&token_type=bearer

//redirects to https://api.telegram.org/bot231953668:AAHUQ8HEQr8Scnl_ViDZ6dWtH9JXDeMy5hw/getUpdates?code=5X5yTK9HLfJV2UkKUY

//replace %3D with = =)
//123456_Stingray

    let cuttlefishUrl = "https://api.bitbucket.org/2.0/repositories/reddotsquaresolutions/unity-cuttlefish/commits"
    let BasicAuth (username:string) (password:string) = 
        let base64Encode (s:string) = 
            let bytes = Encoding.UTF8.GetBytes(s)
            Convert.ToBase64String(bytes)

        sprintf "%s:%s" username password |> base64Encode |> sprintf "Basic %s"// |>  Authorization 

    let isAuthor (email:string)(commit:Json.CommitsResponse.Value) =
        commit.Author.Raw.Contains email
   
    type ShortCommitInfo = {message:string; date: DateTime}

    let commitShortInfo (commit:Json.CommitsResponse.Value) = 
        {message = commit.Message; date = commit.Date}     

        

    let getCommitList email pass = 
        let authHeader = BasicAuth email pass
        let response = Http.RequestString (cuttlefishUrl,  headers = ["Authorization", authHeader])
        printf " %s" authHeader 
        let parsed = Json.CommitsResponse.Parse response 
        parsed.Values
            |> Array.filter (fun c -> isAuthor email c) 
            |> Array.map commitShortInfo
        //    |>  Array.map (fun c -> (c.message+" "+c.date.ToLongDateString()))

        
        //r
    
