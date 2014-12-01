namespace FSharpRestaurantSample.Domain.Tab

[<AutoOpen>]
module OrderedItem = 

    type OrderedItem = {
        MenuNumber: int;
        Description: string;
        IsDrink: bool;
        Price: decimal;
    }