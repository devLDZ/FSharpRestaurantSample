
var emitReadModel = function (s, e)
{
    var streamId = "TabInvoiceProjection-" + e.streamId.replace("Tab-", "");
    var eventType = e.eventType + "_TabInvoiceProjection";
    emit(streamId, eventType, s);
};

fromCategory('Tab')
.foreachStream()
.when({
    "TabOpened": function (state, ev)
    {
        state = { TableNumber: 0, Items: [], Total: 0 };
        state.TableNumber = ev.data.Fields[0];
        emitReadModel(state, ev);
        return state;
    },
    "FoodOrdered": function (state, ev)
    {
        ev.data.Fields[0].forEach(function (entry)
        {
            var item = {
                MenuNumber: entry.MenuNumber,
                Description: entry.Description,
                Price: entry.Price
            };
            state.Items.push(item);
            state.Total += item.Price;
        });
        emitReadModel(state, ev);
        return state;
    },
    "DrinksOrdered": function (state, ev)
    {
        ev.data.Fields[0].forEach(function (entry)
        {
            var item = {
                MenuNumber: entry.MenuNumber,
                Description: entry.Description,
                Price: entry.Price
            };
            state.Items.push(item);
            state.Total += item.Price;
        });
        emitReadModel(state, ev);
        return state;
    }
});