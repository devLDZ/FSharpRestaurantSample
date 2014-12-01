module Helpers

open FSharpRestaurantSample.Infrastructure
open FSharpx.Collections
open FsUnit


let Given (aggregate : Aggregate.Aggregate<'TState, 'TCommand, 'TEvent, 'TError>, events : 'TEvent seq) = 
    let state = Seq.fold aggregate.apply aggregate.zero events
    (aggregate,state)

let When (command : 'TCommand) (aggregate : Aggregate.Aggregate<'TState, 'TCommand, 'TEvent, 'TError>, state : 'TState) = 
    (aggregate.exec state command)

let Then (expected : 'TEvent) (event : Choice<NonEmptyList<'TEvent>, NonEmptyList<'TError>>) =   
    match event with
    | Choice1Of2 e -> (e |> NonEmptyList.toList) |> should contain expected
    | Choice2Of2 e -> failwith ( sprintf "%A" e)

let Fail (expected : 'TError) (event : Choice<NonEmptyList<'TEvent>, NonEmptyList<'TError>>) = 
    match event with
    | Choice1Of2 e -> failwith "Expected Error"
    | Choice2Of2 e -> e |> should contain expected