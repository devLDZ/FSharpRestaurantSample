namespace FSharpRestaurantSample.Infrastructure

/// Aggregate framework.
/// Based on https://github.com/eulerfx/DDDInventoryItemFSharp/blob/master/DDDInventoryItemFSharp/Aggregate.fs
[<RequireQualifiedAccess>]
module Aggregate = 

    open FSharpx.Collections
    open FSharpx.Choice
    open System

    /// <summary>
    /// Type representing Aggragate
    /// </summary>
    type Aggregate<'TState, 'TCommand, 'TEvent, 'TError> = {    
        zero : 'TState;
        apply : 'TState -> 'TEvent -> 'TState;
        exec : 'TState -> 'TCommand -> Choice<NonEmptyList<'TEvent>, NonEmptyList<'TError>>;
    }

    exception OptimisticConcurrencyException

    /// <summary>
    /// Function representing command handler for given Aggragate
    /// </summary>
    /// <param name="aggregate">Definition of Aggragate</param>
    /// <param name="load">Function returning sequence of events for given aggragate</param>
    /// <param name="commit">Function commiting event for given aggragate</param>
    let makeHandler (aggregate:Aggregate<'TState, 'TCommand, 'TEvent, 'TError>) (load: (Type * Guid -> Async<obj seq>), commit: ((Guid*int) -> NonEmptyList<'TEvent> -> Async<unit>)) =
        fun (id,ver) command -> async { 
            let! events = load (typeof<'TEvent>,id)
            let events' = events |> Seq.cast :> 'TEvent seq
            let state = Seq.fold aggregate.apply aggregate.zero events'
            return commit (id,ver) <!> aggregate.exec state command   
        }
