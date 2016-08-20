namespace Shkoda.Telegram.Bot
module Http = 
    open FSharp.Data
    open System
    open System.Text

    let basicAuthHeaderValue (username:string) (password:string) = 
        let base64Encode (s: string) = 
            Convert.ToBase64String(Encoding.UTF8.GetBytes(s))
        sprintf "%s:%s" username password |> base64Encode |> sprintf "Basic %s"

    let authorizedRequest url email password = 
        try
            Http.RequestString (url,  headers = ["Authorization", basicAuthHeaderValue email password]) |> Some
        with 
        | ex -> printf "AuthorizedRequest exception: %s" ex.Message; None 
