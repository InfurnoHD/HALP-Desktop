"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/helplisthub").build();
connection.start();

function removeTicket(id)
{
    connection.invoke("RemovedByUser", id).catch(function (err) {
        return console.error(err.toString());
    });

    redirect("/");
}

connection.on("RemoveFromHelplist", (ticketID) => updateQueue)
    {
        console.log("test")
    }
    
var count = 0;
function setCount(c){
    console.log("Count pre: ", count);
    count = c + 1;
    console.log("Count post: ", count);
}
function updateQueue()
{
    count--;
    console.log(count);
    document.write(count);
}

function redirect(url)
{
    location.replace(url);
}

