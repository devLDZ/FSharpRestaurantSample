module ``Unit Tests``

open Helpers
open FSharpRestaurantSample.Domain.Tab
open NUnit.Framework

let testDrink1 = {MenuNumber = 4; Description = "Sprite"; IsDrink = true; Price = 4m}
let testDrink2 = {MenuNumber = 10; Description = "Beer"; IsDrink  = true; Price = 2.5m}
let testFood1 = {MenuNumber = 16; Description = "Beef Noodles"; IsDrink = false; Price = 7.5m}
let testFood2 = {MenuNumber = 25; Description = "Vegetable Curry"; IsDrink = false; Price = 6m}
let tabNo = 1
let waiterName = "test"

[<Test>]
let ``Can open tab``() =
    Given (Tab.Aggragate, Seq.empty) |> 
    When  (Tab.OpenTab(tabNo, waiterName)) |>
    Then  (Tab.TabOpened(tabNo, waiterName))

[<Test>]
let ``Can't opened already opened tab``() =
    Given (Tab.Aggragate, [Tab.TabOpened(tabNo, waiterName)]) |> 
    When  (Tab.OpenTab(tabNo, waiterName)) |>
    Fail  (Tab.TabAlreadyOpened)

[<Test>]
let ``Can't order with unopened tab``() = 
    Given (Tab.Aggragate, Seq.empty) |>
    When  (Tab.PlaceOrder([testFood1])) |>
    Fail  (Tab.TabNotOpened)

[<Test>]
let ``Can order drinks``() = 
    Given (Tab.Aggragate, [Tab.TabOpened(tabNo, waiterName)] ) |>
    When  (Tab.PlaceOrder([testDrink1; testDrink2])) |>
    Then  (Tab.DrinksOrdered([testDrink1; testDrink2]))

[<Test>]
let ``Can order food``() = 
    Given (Tab.Aggragate,[Tab.TabOpened(tabNo, waiterName)] ) |>
    When  (Tab.PlaceOrder([testFood1; testFood2])) |>
    Then  (Tab.FoodOrdered([testFood1; testFood2]))

[<Test>]
let ``Can order food and drinks``() = 
    let test = Given (Tab.Aggragate,[Tab.TabOpened(tabNo, waiterName)] ) |>
               When  (Tab.PlaceOrder([testFood1; testDrink1]))

    test |> Then (Tab.FoodOrdered([testFood1]))
    test |> Then (Tab.DrinksOrdered([testDrink1]))

[<Test>]
let ``Can serve ordered drinks``() = 
    Given (Tab.Aggragate, [Tab.TabOpened(tabNo, waiterName); Tab.DrinksOrdered([testDrink1; testDrink2])]) |>
    When  (Tab.MarkDrinkServed([testDrink1.MenuNumber; testDrink2.MenuNumber])) |>
    Then  (Tab.DrinksServed([testDrink1.MenuNumber; testDrink2.MenuNumber]))

[<Test>]
let ``Can't serve not ordered drinks``() =
    Given (Tab.Aggragate, [Tab.TabOpened(tabNo, waiterName); Tab.DrinksOrdered([testDrink1])]) |>
    When  (Tab.MarkDrinkServed([testDrink2.MenuNumber])) |>
    Fail  (Tab.DrinksNotOutstanding)

[<Test>]
let ``Can't serve ordered drink twice``() = 
    Given (Tab.Aggragate, [Tab.TabOpened(tabNo, waiterName); Tab.DrinksOrdered([testDrink2]); Tab.DrinksServed([testDrink2.MenuNumber]) ]) |>
    When  (Tab.MarkDrinkServed([testDrink2.MenuNumber])) |>
    Fail  (Tab.DrinksNotOutstanding)

[<Test>]
let ``Can mark as prepared ordered food``() = 
    Given (Tab.Aggragate, [Tab.TabOpened(tabNo, waiterName); Tab.FoodOrdered([testFood1; testFood2])]) |>
    When  (Tab.MarkFoodPrepared([testFood1.MenuNumber; testFood2.MenuNumber])) |>
    Then  (Tab.FoodPrepared([testFood1.MenuNumber; testFood2.MenuNumber]))

[<Test>]
let ``Can't mark as prepared not ordered food``() = 
    Given (Tab.Aggragate, [Tab.TabOpened(tabNo, waiterName); Tab.FoodOrdered([testFood1])]) |>
    When  (Tab.MarkFoodPrepared([ testFood2.MenuNumber])) |>
    Fail  (Tab.FoodNotOutstanding)

[<Test>]
let ``Can't mark as prepared same food twice``() = 
    Given (Tab.Aggragate, [Tab.TabOpened(tabNo, waiterName); Tab.FoodOrdered([testFood1]); Tab.FoodPrepared([testFood1.MenuNumber]) ]) |>
    When  (Tab.MarkFoodPrepared([ testFood1.MenuNumber])) |>
    Fail  (Tab.FoodNotOutstanding)

[<Test>]
let ``Can serve prepared food``() = 
    Given (Tab.Aggragate, [Tab.TabOpened(tabNo, waiterName); Tab.FoodOrdered([testFood1]); Tab.FoodPrepared([testFood1.MenuNumber]) ]) |>
    When  (Tab.MarkFoodServed([testFood1.MenuNumber])) |>
    Then  (Tab.FoodServed([testFood1.MenuNumber]))

[<Test>]
let ``Can't serve not prepared food``() = 
    Given (Tab.Aggragate, [Tab.TabOpened(tabNo, waiterName); Tab.FoodOrdered([testFood1; testFood2]); Tab.FoodPrepared([testFood1.MenuNumber]) ]) |>
    When  (Tab.MarkFoodServed([testFood2.MenuNumber])) |>
    Fail  (Tab.FoodNotPrepared)

[<Test>]
let ``Can't serve not ordered food``() = 
    Given (Tab.Aggragate, [Tab.TabOpened(tabNo, waiterName); Tab.FoodOrdered([testFood1]); Tab.FoodPrepared([testFood1.MenuNumber]) ]) |>
    When  (Tab.MarkFoodServed([testFood2.MenuNumber])) |>
    Fail  (Tab.FoodNotPrepared)

[<Test>]
let ``Can't serve same ordered food twice``() = 
    Given (Tab.Aggragate, [Tab.TabOpened(tabNo, waiterName); Tab.FoodOrdered([testFood1]); Tab.FoodPrepared([testFood1.MenuNumber]); Tab.FoodServed([testFood1.MenuNumber]) ]) |>
    When  (Tab.MarkFoodServed([testFood1.MenuNumber])) |>
    Fail  (Tab.FoodNotPrepared)

[<Test>]
let ``Can close tab paying exact ammount``() =
    Given (Tab.Aggragate, [Tab.TabOpened(tabNo, waiterName); Tab.FoodOrdered([testFood1]); Tab.FoodPrepared([testFood1.MenuNumber]); Tab.FoodServed([testFood1.MenuNumber]) ]) |>
    When  (Tab.CloseTab(testFood1.Price)) |>
    Then  (Tab.TabClosed(testFood1.Price, testFood1.Price, 0m))

[<Test>]
let ``Can close tab with a tip``() = 
    Given (Tab.Aggragate, [Tab.TabOpened(tabNo, waiterName); Tab.FoodOrdered([testFood1]); Tab.FoodPrepared([testFood1.MenuNumber]); Tab.FoodServed([testFood1.MenuNumber]) ]) |>
    When  (Tab.CloseTab(testFood1.Price + 0.5m)) |>
    Then  (Tab.TabClosed(testFood1.Price + 0.5m, testFood1.Price, 0.5m))

[<Test>]
let ``Can't close tab paying not enough``() = 
    Given (Tab.Aggragate, [Tab.TabOpened(tabNo, waiterName); Tab.FoodOrdered([testFood1]); Tab.FoodPrepared([testFood1.MenuNumber]); Tab.FoodServed([testFood1.MenuNumber]) ]) |>
    When  (Tab.CloseTab(testFood1.Price - 0.5m)) |>
    Fail  (Tab.MustPayEnough)

[<Test>]
let ``Can't close tab with not prepared items``() = 
    Given (Tab.Aggragate, [Tab.TabOpened(tabNo, waiterName); Tab.FoodOrdered([testFood1; testFood2]); Tab.FoodPrepared([testFood1.MenuNumber]); Tab.FoodServed([testFood1.MenuNumber]) ]) |>
    When  (Tab.CloseTab(testFood1.Price)) |>
    Fail  (Tab.TabHasUnservedItems)

[<Test>]
let ``Can't close tab with not served items``() = 
    Given (Tab.Aggragate, [Tab.TabOpened(tabNo, waiterName); Tab.FoodOrdered([testFood1; testFood2]); Tab.FoodPrepared([testFood1.MenuNumber; testFood2.MenuNumber]); Tab.FoodServed([testFood1.MenuNumber]) ]) |>
    When  (Tab.CloseTab(testFood1.Price)) |>
    Fail  (Tab.TabHasUnservedItems)

let ``Can't close tab twice``() = 
    Given (Tab.Aggragate, [Tab.TabOpened(tabNo, waiterName); Tab.FoodOrdered([testFood1]); Tab.FoodPrepared([testFood1.MenuNumber]); Tab.FoodServed([testFood1.MenuNumber]); Tab.TabClosed(testFood1.Price, testFood1.Price, 0m) ]) |>
    When  (Tab.CloseTab(testFood1.Price)) |>
    Fail  (Tab.TabNotOpened)