module IntegrationTests

open FSharpRestaurantSample.Infrastructure
open FSharpRestaurantSample.Domain.Tab
open FSharpRestaurantSample.ReadModel.TabModel
open Helpers
open NUnit.Framework
open FsUnit
open EventStore.ClientAPI
open EventStore.ClientAPI.SystemData
open System
open System.Net
open FSharpx.Collections

let endPoint = IPEndPoint(IPAddress.Loopback, 1113)
let conn = EventStore.conn endPoint |> Async.RunSynchronously
let handleCommand' = Aggregate.makeHandler Tab.Aggragate (EventStore.makeRepository conn "Tab")
let handleCommand (id,v) c = handleCommand' (id,v) c |> Async.RunSynchronously

let get = EventStore.makeReadModelGetter<ReadModels.TabInvocie> conn

let i = Guid.NewGuid()
let testDrink1 = {MenuNumber = 4; Description = "Sprite"; IsDrink = true; Price = 4m}
let testDrink2 = {MenuNumber = 10; Description = "Beer"; IsDrink  = true; Price = 2.5m}
let testFood1 = {MenuNumber = 16; Description = "Beef Noodles"; IsDrink = false; Price = 7.5m}
let testFood2 = {MenuNumber = 25; Description = "Vegetable Curry"; IsDrink = false; Price = 6m}
let tabNo = 1
let waiterName = "test"

let handle c = 
    let result = c |> handleCommand (i,0)
    match result with
    | Choice1Of2 a -> a |> Async.RunSynchronously |> ignore
    | _ -> failwith "Unexpected errors"

[<Test>]
let test () =    
    Tab.OpenTab(tabNo, waiterName) |> handle |> ignore
    Tab.PlaceOrder([testFood1; testDrink1]) |> handle |> ignore
    Tab.MarkDrinkServed([testDrink1.MenuNumber]) |> handle |> ignore
    let t = get ("TabInvoiceProjection-" + i.ToString("N")) |> Async.RunSynchronously
    match t with
    | Some t' -> t'.Total |> should equal 11.5
                 t'.TableNumber |> should equal 1
                 t'.Items.Length |> should equal 2
    | _ -> failwith "Expeced result"
    ()
