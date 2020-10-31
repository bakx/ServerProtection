## Ignore notes below
When developing plug-ins, Custom1, 2 and 3 have to be set to default\actual values. 
Should not stay null.


# 
For Serilog to work correct while running under a service account, the logSettings.json needs to have a full path.
C:\\Your installation path\\logs\\


Session "IIS-ETW" failed to start with the following error: 0xC0000035

Application: SP.Core.exe
CoreCLR Version: 4.700.20.47201
.NET Core Version: 3.1.9
Description: The process was terminated due to an unhandled exception.
Exception Info: Grpc.Core.RpcException: Status(StatusCode="Internal", Detail="Error starting gRPC call. HttpRequestException: A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond. SocketException: A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond.", DebugException="System.Net.Http.HttpRequestException: A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond.
 ---> System.Net.Sockets.SocketException (10060): A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond.
   at System.Net.Http.ConnectHelper.ConnectAsync(String host, Int32 port, CancellationToken cancellationToken)
   --- End of inner exception stack trace ---
   at System.Net.Http.ConnectHelper.ConnectAsync(String host, Int32 port, CancellationToken cancellationToken)
   at System.Net.Http.HttpConnectionPool.ConnectAsync(HttpRequestMessage request, Boolean allowHttp2, CancellationToken cancellationToken)
   at System.Net.Http.HttpConnectionPool.GetHttp2ConnectionAsync(HttpRequestMessage request, CancellationToken cancellationToken)
   at System.Net.Http.HttpConnectionPool.SendWithRetryAsync(HttpRequestMessage request, Boolean doRequestAuth, CancellationToken cancellationToken)
   at System.Net.Http.RedirectHandler.SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
   at Grpc.Net.Client.Internal.GrpcCall`2.RunCall(HttpRequestMessage request, Nullable`1 timeout)")
   at Plugins.ApiGrpc.GetUnblock(Int32 minutes) in C:\Users\gideo\source\repos\Server Protection\SP.Core\Plugins\Api\Api.gRPC\ApiGRPC.cs:line 136
   at SP.Core.CoreService.UnblockTask(Object _) in C:\Users\gideo\source\repos\Server Protection\SP.Core\SP.Core\CoreService.cs:line 78
   at SP.Core.CoreService.<ExecuteAsync>b__32_0(Object state) in C:\Users\gideo\source\repos\Server Protection\SP.Core\SP.Core\CoreService.cs:line 307
   at System.Threading.Tasks.Task.<>c.<ThrowAsync>b__139_1(Object state)
   at System.Threading.QueueUserWorkItemCallbackDefaultContext.Execute()
   at System.Threading.ThreadPoolWorkQueue.Dispatch()
   at System.Threading._ThreadPoolWaitCallback.PerformWaitCallback()


------------------
https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli

Add-Migration InitialCreate
update-database

Add-Migration AddAttackTypeToBlocks
update-database

Add-Migration AddCustomFieldsToAccessAttempts
update-database

Add-Migration AddSourceToAccessAttempts
update-database

Add-Migration IpRanges
update-database
