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

