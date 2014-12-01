namespace FSharpRestaurantSample.Infrastructure

open System
open System.IO
open System.Text
open Newtonsoft.Json
open System.Collections.Generic
open Newtonsoft.Json.Serialization
open Newtonsoft.Json.Converters
open Microsoft.FSharp.Reflection

///Serializer
///Based on https://github.com/eulerfx/DDDInventoryItemFSharp/blob/master/DDDInventoryItemFSharp/Serialization.fs
[<RequireQualifiedAccess>]
module Serializer =    
    let private s = new JsonSerializer() 

    let private eventType o =
        let t = o.GetType()
        if FSharpType.IsUnion(t) || (t.DeclaringType <> null && FSharpType.IsUnion(t.DeclaringType)) then
            let cases = FSharpType.GetUnionCases(t)
            let unionCase,_ = FSharpValue.GetUnionFields(o, t)
            unionCase.Name
        else t.Name
        
    let serialize o =
        use ms = new MemoryStream()
        (use jsonWriter = new JsonTextWriter(new StreamWriter(ms))
        s.Serialize(jsonWriter, o))
        let data = ms.ToArray()
        (eventType o),data

    let deserialize (t, et:string, data:byte array) =
        use ms = new MemoryStream(data)
        use jsonReader = new JsonTextReader(new StreamReader(ms))
        s.Deserialize(jsonReader, t)

    let deserialize'<'T> (data:byte array) =
        let json = Encoding.UTF8.GetString(data)
        JsonConvert.DeserializeObject<'T>(json)

