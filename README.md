# SignalR Chat Demo

Building a SignalR chat application with C# and .NET, then scaling with a Redis backplane and multiple dynos on Heroku, with session affinity (sticky sessions)

## Prerequisites

- [.NET 9 SDK](https://learn.microsoft.com/en-us/dotnet/core/install/)

## How I created this project

### .NET CLI to create a new web application

```
dotnet new webapp -o SignalRChat
```

### Install LibMan for managing JS dependencies

The SignalR server library is included in the ASP.NET Core shared framework, but the JavaScript client library isn't automatically included. Use LibMan to get it.

```
dotnet tool install -g Microsoft.Web.LibraryManager.Cli
```

Note: You may need to add `$HOME/.dotnet/tools` to your `PATH`

### Install SignalR JS library

```
libman install @microsoft/signalr@latest -p unpkg -d wwwroot/js/signalr --files dist/browser/signalr.js
```

### Code

Created the following files:

* `hubs/ChatHub.cs`: The hub class that serves as a high-level pipeline that handles client-server communication
* `Pages/Index.cshtml`: The main Razor file, combining HTML and embedded C# with Razor syntax.
* `wwwroot/js/chat.js`: The chat logic for the application

Updated the main program (`Program.cs`) to use SignalR. (The initial commit has the version prior to introducing the Redis backplane).

## Build the app

```
dotnet build
```

## Run the app

```
dotnet run
```

## Test

Open two browsers, navigate to application. Send message in one, see it show up in the other in real time.

## Set up the Redis backplane

This assumes you have an instance of Redis running locally, listening on port `6379`. (You could also spin up a Docker container and port forward your `6379`.)

### Install the Redis package for .NET

[StackExchange.Redis Package information](https://github.com/StackExchange/StackExchange.Redis)

```
dotnet add package Microsoft.AspNetCore.SignalR.StackExchangeRedis
```

### Update `Program.cs`

See the updated code for `Program.cs` in the latest commit, which configures the app to use Redis as a backplane.

Connection information was tailored for deployment to Heroku, expecting a Redis connection string at `REDIS_URL`. See [Heroku documentation on connecting to Key-Value Store in .NET](https://devcenter.heroku.com/articles/connecting-heroku-redis#connecting-in-net).

### Test

Start application. Connect via Redis CLI. Run `pubsub channels` to see list of channels. Run `subscribe SignalRChatSignalRChat.Hubs.ChatHub:all` to subscribe.

Reload application in the browser. Send messages. See messages in Redis CLI.

## Deploying to Heroku

### Login

`heroku login`

### Create app

`heroku create my-signalr-demo-app`

### Add .NET buildpack 

`heroku buildpacks:add heroku/dotnet`

### Add the Redis add-on

`heroku addons:add heroku-redis`

### Create the Procfile

See `Procfile` in project root folder.

### Push code to Heroku

`git push heroku main`

### Test

Test in browsers, this time pointing to Heroku app URL instead of localhost.

## Scale up Heroku dynos

### Upgrade dyno type (Eco/Basic cannot scale out)

`heroku ps:type web=standard-1x`

### Scale out to 3 dynos

`heroku ps:scale web=3`

### Test (expect failure)

Reload application in the browser. Note in network inspector that WebSocket connections are failing.

## Enable Heroku session affinity

More on the session affinity feature [here](https://devcenter.heroku.com/articles/session-affinity).

`heroku features:enable http-session-affinity`

### Test (expect success)

Reload application in the browser. WebSocket connections should be maintained, and chat messages should come across in real time.

