namespace FSharpRestaurantSample.Infrastructure

module Validator = 

    open FSharpx.Collections

    let validator pred error value =
        if pred value then Choice1Of2 value
        else Choice2Of2 (NonEmptyList.singleton error)