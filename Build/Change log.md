# Connected change log

## 2.3.1117.3
- Add SaaS client base library for SaaS service utilization.\
Implement general client capable of connecting to the connected SaaS services.
- Add health monitoring SaaS client library\
The library provides REST client functionality, capable of connecting to and manipulating with the cloud based Health Monitoring service. 
- Add health monitoring support via heartbeat to all runtimes and Sys.
- Add health monitoring support via heartbeat to individual workers, running withing the Worker runtime.
- Add CORS address support to static files, served by the App runtime.\
This resolves and CORS errors encountered in browsers when webpages have multiple bindings.
- Add support for unsafe mail servers.\
The SMTP client now tries a primary and secondary DNS address to resolve a secure route. If unable, it reverts to base MTP.
- Optimize memory consumption when compiling code.
- Remove random keys from user membership cache.\
This fixed several bugs regarding multiple duplicated instances of a membership in cache.
- Add additional checks for null user values when resolving users.
This fixes a nullreferenceexception error when a resolution was attempted on an empty username value.

## 2.3.1118.1
- Fix bearer authentication provider failing when no rest token was supplied in healthmonitoring configuration.

## 2.3.1122.1
- Add additional timeout check to queue updates.\
This should prevent race conditions when updating queue values. Is a known fix for an issue where print jobs had their tokens replaced prematurely.

## 2.3.1123.1
- Add worker restart code to Sys initialization.\
This fixes a known bug where workers would remain in the "queued" state on forced restarts.
- Fix health monitoring timing out after five minutes.
- Fix health monitoring not initializing when starting up Sys.

## 2.3.1201.1
- Add check for disposed lock in SynchronizedRepository\
This fixes a repository initialization bug, affecting caching and other memory cached services.
- Fix LanguageService so initialization does not lock up before first query is called.
- Rewrite exception rethrows in exceptionmiddlewares so original stack trace is preserved where possible.
- Add null check to event message data load.\
When message is not found, processing is stopped.

## 3.1.0105.1
- Add support for contextual system-wide locks.\ The same process now retrieves a system lock for a key instead of failing.
- Fix bug where BigData would still attempt to process data after failing to resolve a partition.
- Fix bug in Sys where long-polling a web socket would throw an exception.\
This fixes the infamous "Unexpected character at the end of JSON" error. The ToJObject method no longer crashes with an invalid payload as a consequence.
- Fix workerJob concurrency issue when the worker is dosposing in the middle of execution.
- Fix cache Remove function crashing when two concurrent threads attempted to remove same object.

## 3.1.0202.1
- Fix queued worker freezes
- Stabilize cache storage