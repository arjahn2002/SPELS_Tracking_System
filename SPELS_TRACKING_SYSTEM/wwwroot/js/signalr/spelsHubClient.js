const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.on("ReceiveMessage", function (message) {
    location.reload(); // Refresh the page to get updated data
    console.log("Message received: " + message);
});

connection.start()
    .then(() => console.log("SignalR connected!"))
    .catch(err => console.error(err.toString()));

