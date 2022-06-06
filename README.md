# WatchDog.Echo


## Introduction

WatchDog.Echo is a minimalistic C# package that helps to validate interoperability between services. This package aims at notifying developers or project owners when a particular service B is not reachable from a service A, or a list of services are not reachable from a service. It leverages on both/either gRPC and REST protocols to communicate between these services. Notifications can be sent to a slack, teams, discord channels, or as an email. This package currently targets .Net Core 3.1 and .Net 6.

## General Features
- Check interoperability between services
- Notification to slack, discord and teams channel
- Notification to email
- Set echo ( service availablity check ) intervals

## Support
- .NET Core 3.1 and newer

## Installation

Install via .NET CLI

```bash
dotnet add package WatchDog.Echo --version 1.0.0
```
Install via Package Manager

```bash
Install-Package WatchDog.Echo --version 1.0.0
```

## Usage
To enable WatchDog.Echo echo other services, 

Add WatchDog.Echo Namespace in `Startup.cs` or `Program.cs`

```c#
using WatchDog;
```

### Register WatchDog.Echo service in `Startup.cs` or `Program.cs`
#### For .Net Core 3.1 

```c#
services.AddWatchDogEchoServices();
```

#### For .Net 6

```c#
builder.Services.AddWatchDogEchoServices();
```

### Setup Echo Options

```c#
builder.Services.AddWatchDogEchoServices(opt =>
{
    opt.EchoIntervalInMinutes = 0; 
    opt.ClientHost = "https://localhost:7068"; 
    opt.EchoTargetURLs = "https://localhost:44362"; 
    opt.Protocol = ProtocolEnum.REST; 
    opt.EmailAddresses = "something@gmail.com, nothing@outlook.com";
    opt.MailConfig = new WatchDog.Echo.src.Models.MailSettings
    {
        MailFrom = "test",
        MailHost = "test",
        MailPort = 455,
        MailPubKey = "test",   
        MailSecKey = "test", 
    };
    opt.WebhookURLs = "https://hooks.slack.com/services/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";
});
```
>**NOTE**
>EchoIntervalInMinutes // Interval you want echo to be perfomed in minutes `Required`
>ClientHost // Host(url) of service originating echo `Required`
>EchoTargetURLs // urls of target services, seperated by commas
>Protocol // Echo protocol to use for communication, REST or gRPC
>EmailAddresses // addressess of users you want to send notification to, seperated by commas
>MailConfig // Mail cofiguration for sending email notification
>WebhookURLs // urls of webhook for teams, slack, discord channels seperated by commas

### Setup in-app event notification `Optional`
If you decide to perform other actions during failed echos, you can subscribe to an `OnEchoFailedEvent` that is been sent for every failed echo.

Create a background service that will listen for this event by creating a class that implements the C# `BackgroundService` and subscribe to the event in the `ExecuteAsync()` method.

Add 
```c#
using WatchDog.Echo.src.Events;
```

```c#
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            EchoEventPublisher.Instance.OnEchoFailedEvent += e_OnEventFailed;
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                await Task.Delay(1000, stoppingToken);
            }
        }
        
        static void e_OnEventFailed(object sender, EchoEventsArgs e)
        {
            //Handle Echo Failed Event
            Console.WriteLine("The host {0} couldnt reach {1}.", e.FromHost, e.ToHost);
        }
```



## Contribution
Feel like something is missing? Fork the repo and send a PR.

Encountered a bug? Fork the repo and send a PR.

Alternatively, open an issue and we'll get to it as soon as we can.

## Credit
Kelechi Onyekwere -  [Github](https://github.com/Khelechy) [Twitter](https://twitter.com/khelechy1337)

Israel Ulelu - [Github](https://github.com/IzyPro) [Twitter](https://twitter.com/IzyPro_)
