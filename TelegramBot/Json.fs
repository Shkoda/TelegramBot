namespace Shkoda.Telegram.Bot
module Json =
    open FSharp.Data
    open System

    type Update = JsonProvider<"../data/update.json">
    type CommitsResponse = JsonProvider<"../data/commits_sample.json">
    type UserConfig = JsonProvider<"../data/user_config_sample.json">


  