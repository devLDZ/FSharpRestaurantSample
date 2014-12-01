namespace FSharpRestaurantSample.Infrastructure

/// Integration with EventStore.
/// Based on https://github.com/eulerfx/DDDInventoryItemFSharp/blob/master/DDDInventoryItemFSharp/EventStore.fs
[<RequireQualifiedAccess>]
module EventStore = 

    open System
    open System.Net
    open EventStore.ClientAPI
    open FSharpx.Collections

    /// Creates and opens an EventStore connection.
    let conn (endPoint : IPEndPoint) = async { 
        let settings = ConnectionSettings.Create()
                            .SetDefaultUserCredentials(SystemData.UserCredentials("admin","changeit")).Build()
        let conn = EventStoreConnection.Create(settings,endPoint)
        do! conn.ConnectAsync() |> Async.AwaitIAsyncResult |> Async.Ignore
        return conn }

    /// Creates event store based repository.
    let makeRepository (conn:IEventStoreConnection) category =

        let streamId (id:Guid) = category + "-" + id.ToString("N").ToLower()

        let load (t,id) = async {
            let streamId = streamId id
            let! eventsSlice = conn.ReadStreamEventsForwardAsync(streamId, StreamPosition.Start, Int32.MaxValue, false) |> Async.AwaitTask
            return eventsSlice.Events |> Seq.map (fun e -> Serializer.deserialize(t, e.Event.EventType, e.Event.Data))
        }

        let commit (id,expectedVersion) (events : NonEmptyList<_>) = async {
            let streamId = streamId id
            
            let map e = 
                let eventType,data = Serializer.serialize e
                let metaData = [||] : byte array
                EventData(Guid.NewGuid(), eventType, true, data, metaData)

            let events' = Seq.map map events
            let! result =  match expectedVersion with
                           | 0 -> conn.AppendToStreamAsync(streamId, ExpectedVersion.Any, events') |> Async.AwaitTask
                           | _ -> conn.AppendToStreamAsync(streamId, expectedVersion, events') |> Async.AwaitTask
            ()
        }

        load,commit

    /// Creates a function that returns a read model from the last event of a stream.
    let makeReadModelGetter<'T> (conn:IEventStoreConnection) streamId = async {
            let! eventsSlice = conn.ReadStreamEventsBackwardAsync(streamId, -1, 1, false) |> Async.AwaitTask
            if eventsSlice.Status <> SliceReadStatus.Success then return None
            elif eventsSlice.Events.Length = 0 then return None
            else 
                let lastEvent = eventsSlice.Events.[0]
                if lastEvent.Event.EventNumber = 0 then return None
                else return Some(Serializer.deserialize'<'T>(lastEvent.Event.Data))    
        }