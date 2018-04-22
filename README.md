# TurtleCoinAPI

An asynchronous wrapper API for loading and accessing the TurtleCoin daemon and wallet applications. Built to ease the creation of wallets and applications that want to pull data from the network.

# Using the API

To begin using the API, you must create a session

```C#
TurtleCoin _session = new TurtleCoin();
```

From here you can create a daemon connection

```C#
// How often the daemon should update its info in milliseconds
_session.Daemon.RefreshRate = 30000;

// Assign event handlers you want to use
_session.Daemon.Log += DaemonLog;
_session.Daemon.Error += DaemonError;
_session.Daemon.OnReady += OnDaemonReady;

// Initialize the daemon connection, port defaults to 11898 if not defined
await _session.Daemon.Initialize("Local path, host address, or IP", Port);

// Begin the internal update loop
await _session.Daemon.BeginUpdateAsync();
```

After a connection is established and the update loop is begun, you can do your processing and requests

```C#
// Triggers when daemon is ready to accept requests
public void OnDaemonReady(object sender, EventArgs e)
{
    // Send a request to the daemon
    (sender as Daemon).SendRequestAsync(RequestMethod.GET_BLOCK_HEADER_BY_HEIGHT,
        new RequestParams { ["height"] = 12345 }, out JObject Result);
    
    // Output response to console
    Console.WriteLine("Currency ID: {0}", (string)Result["currency_id_blob"]);
}
```
