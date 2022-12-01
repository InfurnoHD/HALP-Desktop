"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
connection.start();

function removeTicket(id)
{
    connection.invoke("RemovedByUser", id).catch(function (err) {
        return console.error(err.toString());
    });

    redirect("/");
}

connection.on("RemoveFromHelplist",
    (id) => updateQueue())

var count = 0;
function setCount(c){
    count = c + 1;
}
function updateQueue()
{
    count--;
    return count
}

function redirect(url)
{
    location.replace(url);
}

