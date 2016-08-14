namespace Shkoda.Telegram.Bot
module UserConfigProvider = 
    open System.IO
    open System

    let credentialsFolder = "../../../user_data/"
    let credentialsPattern = "{\"bitbucket\":{\"email\":\"{0}\",\"password\":\"{1}\",\"repositories\":[{\"owner\":\"{2}\",\"name\":\"{3}\"}]}}"
    let credentialsString email password repoOwner repoName = String.Format(credentialsPattern, email, password, repoOwner, repoName)

    let createCredentialsFolderifNotExist = 
        if not (Directory.Exists credentialsFolder) 
            then Directory.CreateDirectory(credentialsFolder); ignore()

    let userCredentialsPath telegramUsername = credentialsFolder + telegramUsername 

    let isKnownUser telegramUsername = File.Exists(userCredentialsPath telegramUsername)

        
    let saveUserBitbucketCredentials telegramUsername email password repoOwner repoName = 
        createCredentialsFolderifNotExist
        let filePath = userCredentialsPath telegramUsername
        let jsonString = credentialsString (email, password, repoOwner, repoName)
        using (new StreamWriter(filePath)) (fun writer -> writer.Write jsonString)

    let saveStingrayCredentials (telegramUsername:string) (email:string) (password:string) = 
        saveUserBitbucketCredentials telegramUsername email password "reddotsquaresolutions" "unity-cuttlefish"

    let getUser telegramUsername = 
        if not (isKnownUser telegramUsername) then  saveUserBitbucketCredentials telegramUsername "" "" "reddotsquaresolutions" "unity-cuttlefish"
        Json.UserConfig.Load (userCredentialsPath telegramUsername)

    let saveEmail telegramUsername email = 
        let credentials = getUser telegramUsername
        saveUserBitbucketCredentials telegramUsername email credentials.Bitbucket.Password credentials.Bitbucket.Repositories.[0].Owner credentials.Bitbucket.Repositories.[0].Name

    let savePassword telegramUsername password = 
        let credentials = getUser telegramUsername
        saveUserBitbucketCredentials telegramUsername credentials.Bitbucket.Email password credentials.Bitbucket.Repositories.[0].Owner credentials.Bitbucket.Repositories.[0].Name
         
    
