namespace Shkoda.Telegram.Bot

module Main =
  open System

  [<EntryPoint>]
  let main args =
    Telegram.UpdateSubscribers.Add(CommandHandler.handleUpdates)
    Async.Start (Telegram.getUpdatesBackground CommandHandler.TOKEN 0)

    while true do Console.ReadLine()
    0