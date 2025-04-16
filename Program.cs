using SignalRChat.Hubs;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var redisUrl = Environment.GetEnvironmentVariable("REDIS_URL") ?? "localhost:6379";

if (redisUrl == "localhost:6379") {
    builder.Services.AddSignalR().AddStackExchangeRedis(redisUrl, options =>
    {
        options.Configuration.ChannelPrefix = RedisChannel.Literal("SignalRChat");
        options.Configuration.Ssl = redisUrl.StartsWith("rediss://");
        options.Configuration.AbortOnConnectFail = false;
    });
} else {
    var uri = new Uri(redisUrl);
    var userInfoParts = uri.UserInfo.Split(':');
    if (userInfoParts.Length != 2)
    {
        throw new InvalidOperationException("REDIS_URL is not in the expected format ('redis://user:password@host:port')");
    }

    var configurationOptions = new ConfigurationOptions
    {
        EndPoints = { { uri.Host, uri.Port } },
        Password = userInfoParts[1],
        Ssl = true,
    };
    configurationOptions.CertificateValidation += (sender, cert, chain, errors) => true;

    builder.Services.AddSignalR(options =>
    {
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(60); // default is 30
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);     // default is 15
    }).AddStackExchangeRedis(redisUrl, options => {
        options.Configuration = configurationOptions;
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapHub<ChatHub>("/chatHub");

app.Run();
