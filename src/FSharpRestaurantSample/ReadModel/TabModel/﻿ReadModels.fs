namespace FSharpRestaurantSample.ReadModel.TabModel

module ReadModels = 

    [<CLIMutable>]
    type TabItem = {
        MenuNumber: int;
        Description: string;
        Price: decimal;
    }

    [<CLIMutable>]
    type TabStatus = {
        TableNumber: int;
        ToServe: TabItem list;
        InPreparation: TabItem list;
        Served: TabItem list;
    }

    [<CLIMutable>]
    type TabInvocie = {
        TableNumber: int;
        Items: TabItem list;
        Total: decimal;
    }