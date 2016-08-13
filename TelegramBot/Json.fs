namespace Shkoda.Telegram.Bot
module Json =
    open FSharp.Data
    type Update = JsonProvider<"../data/update.json">
    type CommitsResponse = JsonProvider<"../data/commits_sample.json">