namespace Shkoda.Telegram.Bot
module UserConfigProvider = 
    open System.IO
    open FSharp.Data

    let credentialsFolder = "../../../user_data/"

    let createCredentialsFolderifNotExist = 
        if not (Directory.Exists credentialsFolder) 
            then Directory.CreateDirectory(credentialsFolder); ignore()

    let userCredentialsPath telegramUsername = credentialsFolder + telegramUsername 

    let isKnownUser telegramUsername = File.Exists(userCredentialsPath telegramUsername)

    let getUser telegramUsername = 
        if isKnownUser telegramUsername then    
            Json.UserConfig.Load (userCredentialsPath telegramUsername) |> Some
        else None          

    let saveUserBitbucketCredentials 
        (telegramUsername:string) (email:string) (password:string) (repoOwner:string) (repoName:string) = 
        createCredentialsFolderifNotExist
        let filePath = userCredentialsPath telegramUsername
        let jsonString = sprintf "{\"bitbucket\":{\"email\":\"%s\",\"password\":\"%s\",\"repositories\":[{\"owner\":\"%s\",\"name\":\"%s\"}]}}"
                             email password repoOwner repoName
        using (new StreamWriter(filePath)) (fun writer -> writer.Write jsonString)

    let saveStingrayCredentials (telegramUsername:string) (email:string) (password:string) = 
        saveUserBitbucketCredentials telegramUsername email password "reddotsquaresolutions" "unity-cuttlefish"
