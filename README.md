![WatchDog.Echo Logo](https://i.ibb.co/thnngfd/Echo-Signature.png)
# WatchDog.Echo

[![Version](https://img.shields.io/nuget/vpre/WatchDog.Echo)](https://www.nuget.org/packages/WatchDog.NET#versions-tab)
[![Downloads](https://img.shields.io/nuget/dt/WatchDog.Echo?color=orange)](https://www.nuget.org/packages/WatchDog.NET#versions-tab)


## Introduction

WatchDog.Echo is a light-weight monitoring and observability tool that helps to validate interoperability between services by notifying developers or project owners when a particular service B is not reachable from a service A, or a list of services are not reachable from a service. It leverages both/either gRPC and REST protocols to send echos between these services and sends alert/notification via Email or to Slack, Microsoft Teams and Discord channels on the event that a particular service is not reachable, enabling developers/projects owners detect service downtime promptly. This package currently targets .Net Core 3.1 and .Net 6.

## General Features
- Check interoperability between services
- Webhook Notification (Slack, Discord, Microsoft Teams etc)
- Email Notification
- Customizable Echo and alert intervals
- Protocol Options (REST or gRPC)

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

## Initialization
To enable WatchDog.Echo communicate with other services, 

Add WatchDog.Echo Namespace in `Startup.cs` or `Program.cs`

```c#
using WatchDog.Echo;
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
## Usage Configurations

### Client Host - `Required`
URL of the current service
```c#
builder.Services.AddWatchDogEchoServices(opt =>
{
    opt.ClientHost = "https://localhost:7068";
});
```

### Protocol - `Optional`
Communication Protocol for the service
```c#
builder.Services.AddWatchDogEchoServices(opt =>
{
    opt.Protocol = ProtocolEnum.REST; 
});
```
`Default = gRPC`
>**NOTE:**
>.NET 3.x service hosted on IIS should utilize REST as gRPC is not supported on IIS for .NET 3.x


### Echo Interval - `Optional`
Time interval between each echo measured in minutes e.g. If set to 5, it sends an echo to all the services listed in the `EchoTargetURLs` every 5 mins
```c#
builder.Services.AddWatchDogEchoServices(opt =>
{
    opt.EchoIntervalInMinutes = 3; 
});
```
`Default = 5 minutes`

### Failed Echo Interval - `Optional`
Time interval between each failed echo alert measured in minutes e.g. If set to 60, when an attempt to echo Service A fails, it sends an alert immediately and then if service A is still down after 60 minutes, it sends another alert. It continues sending an alert every 60 minutes until Service A is back up.
```c#
builder.Services.AddWatchDogEchoServices(opt =>
{
    opt.FailedEchoAlertIntervalInMinutes = 60;
});
```
`Default = 45 minutes`


### Target URLs - `Optional`
Comma separated list of service URLs to be "Echoed" by the current service
```c#
builder.Services.AddWatchDogEchoServices(opt =>
{
    opt.EchoTargetURLs = "https://localhost:44362, https://payment.myserver.com"; 
});
```

>**NOTE:**
>Package must be Installed on both caller host and target host
>You don't necessarily need to configure explicit settings on the target host

### WebhookURLs - `Optional`
Comma separated list of Webhook URLs for channels where echo alerts will be sent e.g. Slack, Microsoft Teams, Discord etc.
```c#
builder.Services.AddWatchDogEchoServices(opt =>
{
    opt.WebhookURLs = "https://hooks.slack.com/services/T00000/B000000/xxxxx, https://discord.com/api/webhooks/{id}/{token}";
});
```
>**NOTE:**
>Follow this guide to create webhooks for [Slack](https://api.slack.com/messaging/webhooks), [Microsoft Teams](https://docs.microsoft.com/en-us/microsoftteams/platform/webhooks-and-connectors/how-to/add-incoming-webhook) and [Discord](https://support.discord.com/hc/en-us/articles/228383668-Intro-to-Webhooks)

### Email Addresses - `Optional`
Comma separated list of email addresses to receive alerts when a particular service is down
```c#
builder.Services.AddWatchDogEchoServices(opt =>
{
    opt.EmailAddresses = "something@gmail.com, nothing@outlook.com";
});
```

### Mail Config - `Optional`
SMTP configuration to be used in sending email alerts
```c#
builder.Services.AddWatchDogEchoServices(opt =>
{
    opt.MailConfig = new WatchDog.Echo.src.Models.MailSettings
    {
        MailFrom = "nothing@gmail.com",
        MailHost = "in-v3.mailjet.com",,
        MailPort = 465,
        MailPubKey = "YOUR PUBLIC KEY",   
        MailSecKey = "YOUR SECRET KEY", 
    };
});
```


### Custom Alert Webhook URL - `Optional`
Your custom web endpoint to be called if you wish to perform an external action when a service is down.
```c#
builder.Services.AddWatchDogEchoServices(opt =>
{
    opt.CustomAlertWebhookURL = "https://myserver.com/ProvisionNewServer)";
});
```

Json body to be received by your endpoint
```json
{ 
    "Description": "https://myserver.com failed to respond to echo from https://server-b.com)", 
    "Response": "Unable to reach server", 
    "Server": "https://myserver.com", 
    "HappenedAt": "09/06/2022 11:38" 
}
```


### Sample Echo Options

```c#
builder.Services.AddWatchDogEchoServices(opt =>
{
    opt.EchoIntervalInMinutes = 3; 
    opt.FailedEchoAlertIntervalInMinutes = 60;
    opt.ClientHost = "https://localhost:7068"; 
    opt.EchoTargetURLs = "https://localhost:44362"; 
    opt.Protocol = ProtocolEnum.REST; 
    opt.WebhookURLs = "https://hooks.slack.com/services/T00000/B000000/xxxxx, https://discord.com/api/webhooks/{id}/{token}";
    opt.CustomAlertWebhookURL = "https://myserver.com/ProvisionNewServer)"
    opt.EmailAddresses = "something@gmail.com, nothing@outlook.com";
    opt.MailConfig = new WatchDog.Echo.src.Models.MailSettings
    {
        MailFrom = "test",
        MailHost = "test",
        MailPort = 465,
        MailPubKey = "test",   
        MailSecKey = "test", 
    };
});
```


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


### Side Notes
To manually check if your server is up and running, head to `/echo`, you should get a message that says "Echo is listening".
>**Example:**
>https://myserver.com/echo

## Contribution
Feel like something is missing? Fork the repo and send a PR.

Encountered a bug? Fork the repo and send a PR.

Alternatively, open an issue and we'll get to it as soon as we can.

## Credit
Kelechi Onyekwere -  [Github](https://github.com/Khelechy) [Twitter](https://twitter.com/khelechy1337)

Israel Ulelu - [Github](https://github.com/IzyPro) [Twitter](https://twitter.com/IzyPro_)
