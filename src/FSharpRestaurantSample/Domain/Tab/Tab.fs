namespace FSharpRestaurantSample.Domain.Tab

[<RequireQualifiedAccess>]
module Tab = 

    open FSharpRestaurantSample.Infrastructure
    open FSharpx.Collections
    open FSharpx.Choice
    open FSharpx.Validation
    open System
    open Validator



    type State = {
        isOpen : bool;
        servedItemsValue : decimal;
        outstandingDrinks : OrderedItem list;
        outstandingFood : OrderedItem list;
        preparedFood : OrderedItem list;
        Waiter : string;
        Table : int;
    }
    with static member Zero = {isOpen = false; servedItemsValue = 0m; outstandingDrinks = list.Empty; outstandingFood = list.Empty; preparedFood = list.Empty; Waiter = String.Empty; Table = 0 }

    type Command =
        | OpenTab of Table : int * Waiter : string
        | PlaceOrder of Items : OrderedItem list
        | MarkFoodPrepared of Items : int list
        | MarkFoodServed of Items : int list
        | MarkDrinkServed of Items : int list
        | CloseTab of PaidAmmount : decimal

    type Error = 
        | TabNotOpened
        | TabAlreadyOpened
        | EmptyOrder
        | DrinksNotOutstanding 
        | FoodNotOutstanding
        | FoodNotPrepared 
        | MustPayEnough 
        | TabHasUnservedItems 
    

    type Event = 
        | TabOpened of  Table : int * Waiter : string
        | DrinksOrdered of  Items : OrderedItem list
        | FoodOrdered of  Items : OrderedItem list
        | FoodPrepared of  Items : int list
        | FoodServed of  Items : int list
        | DrinksServed of  Items : int list
        | TabClosed of PaidAmmount : decimal * OrderValue : decimal * TipValue : decimal

    let apply item = function
        | TabOpened(Table = t; Waiter = w) -> {item with  Table = t; Waiter = w; isOpen = true}
        | DrinksOrdered(Items = i) -> 
            let items' = item.outstandingDrinks @ i
            {item with outstandingDrinks = items' }
        | FoodOrdered(Items = i) ->
            let items' = item.outstandingFood @ i
            {item with outstandingFood = items'}
        | FoodPrepared(Items = i) ->
            let prepared, outstanding = item.outstandingFood |> List.partition (fun n -> i |> List.exists (fun m -> m = n.MenuNumber ) )
            let prepared' = item.preparedFood @ prepared
            {item with outstandingFood = outstanding; preparedFood = prepared'}
        | FoodServed(Items = i) ->
            let served, prepared = item.preparedFood |> List.partition (fun n -> i |> List.exists (fun m -> m = n.MenuNumber ) )
            let value = item.servedItemsValue + (served |> List.sumBy (fun n -> n.Price ))
            {item with servedItemsValue = value; preparedFood = prepared}
        | DrinksServed(Items = i) ->
            let served, drinks = item.outstandingDrinks |> List.partition (fun n -> i |> List.exists (fun m -> m = n.MenuNumber ) )
            let value = item.servedItemsValue + (served |> List.sumBy (fun n -> n.Price ))
            {item with servedItemsValue = value; outstandingDrinks = drinks}
        | TabClosed(_,_,_) -> {item with isOpen = false}

    module private Assert =
        let areAllinList want have = 
            let have' = have |> List.map (fun n -> n.MenuNumber)
            want |> List.forall ( fun n -> have' |> List.exists ( fun m -> m = n))
    
        let isNotOpen state = state |> validator (fun n -> n.isOpen = false ) Error.TabAlreadyOpened
        let isOpen state = state |>  validator (fun n -> n.isOpen) Error.TabNotOpened 
        let isOrderNotEmpty (items : list<_>) state =  state |> validator ( fun _ -> not (items.IsEmpty)) Error.EmptyOrder
    
        let areDrinksOutstanding drinks state = state |> validator (fun n -> areAllinList drinks n.outstandingDrinks) Error.DrinksNotOutstanding
        let areFoodOutstanding food state = state |> validator (fun n -> areAllinList food n.outstandingFood) Error.FoodNotOutstanding
        let areFoodPrepared food state = state |> validator (fun n -> areAllinList food n.preparedFood) Error.FoodNotPrepared
        let hasUnservedItems state = state |> validator ( fun n ->  n.outstandingDrinks.IsEmpty && n.outstandingFood.IsEmpty && n.preparedFood.IsEmpty) Error.TabHasUnservedItems
        let hasPayedEnough ammount state = state |> validator ( fun n -> n.servedItemsValue <= ammount) Error.MustPayEnough

    

    let exec state = function
        | OpenTab(Table =t; Waiter = w) -> choose {
                                            let! state' = Assert.isNotOpen state
                                            return state' |> (fun _ ->  TabOpened(t, w) |> NonEmptyList.singleton)}
        | PlaceOrder(Items = i) -> choose {
                                            let validation s i = returnM i <* sequence [ Assert.isOpen s; Assert.isOrderNotEmpty i s ]
                                            let! state' = validation state i
                                            return state' |> (fun _ -> 
                                                                    let foods,drinks = i |> List.split (fun n -> n.IsDrink)
                                                                    match foods.IsEmpty, drinks.IsEmpty with
                                                                    | false, false -> (DrinksOrdered(drinks) |> NonEmptyList.singleton ) |>  NonEmptyList.append (FoodOrdered(foods) |> NonEmptyList.singleton)
                                                                    | false, true -> FoodOrdered(foods) |> NonEmptyList.singleton
                                                                    | true, false -> DrinksOrdered(drinks) |> NonEmptyList.singleton
                                                                    | true, true -> failwith "Nothing ordered"                         
                                                                    )}
        | MarkDrinkServed(Items = items) -> choose {
                                            let validation s i = returnM i <* sequence [ Assert.isOpen s; Assert.areDrinksOutstanding i s ]
                                            let! items' = validation state items
                                            return items' |> (fun i -> DrinksServed(i) |> NonEmptyList.singleton)}
        | MarkFoodPrepared(Items = items) -> choose {
                                            let validation s i = returnM i <* sequence [Assert.isOpen s; Assert.areFoodOutstanding i s]
                                            let! items' = validation state items
                                            return items' |> (fun i -> FoodPrepared(i) |> NonEmptyList.singleton)  }
        | MarkFoodServed(Items = items) -> choose {
                                            let validation s i = returnM i <* sequence [Assert.isOpen s; Assert.areFoodPrepared i s ]
                                            let! items' = validation state items
                                            return items' |> (fun i -> FoodServed(i)|> NonEmptyList.singleton) 
                                            }
        | CloseTab(PaidAmmount = ammount) -> choose {
                                            let validation s a = returnM a <* sequence [Assert.isOpen s; Assert.hasUnservedItems s; Assert.hasPayedEnough a s  ]
                                            let! am = validation state ammount
                                            return am |> (fun i -> TabClosed(i, state.servedItemsValue, i - state.servedItemsValue) |> NonEmptyList.singleton) }
     
    let Aggragate : Aggregate.Aggregate<State, Command, Event, Error> = {zero = State.Zero; apply = apply; exec = exec }