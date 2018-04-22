# TurtleCoinAPI

An asynchronous wrapper API for loading and accessing the TurtleCoin daemon and wallet applications. Built to ease the creation of wallets and applications that want to pull data from the network.

# Using the API

To begin using the API, you must create a session

```C#
TurtleCoin _session = new TurtleCoin();
```

From here you can create a daemon connection

```C#
_session.Daemon.RefreshRate = 30000; // How often the daemon should update its info in milliseconds
_session.Daemon.Log += DaemonLog; // Assign event handlers you want to use
_session.Daemon.Error += DaemonError;
_session.Daemon.OnReady += OnDaemonReady;
await _session.Daemon.Initialize("Local path, host address, or IP", Port); // Initialize the daemon connection, port defaults to 11898 if not defined
await _session.Daemon.BeginUpdateAsync(); // Begin the internal update loop
```

After a connection is established and the update loop is begun, you can do your processing and requests

```C#
public void OnDaemonReady(object sender, EventArgs e) // Triggers when daemon is ready to accept requests
{
    (sender as Daemon).SendRequestAsync(RequestMethod.GET_CURRENCY_ID, new RequestParams { }, out JObject Result);
    Console.WriteLine("Currency ID: {0}", (string)Result["currency_id_blob"]);
}
```
