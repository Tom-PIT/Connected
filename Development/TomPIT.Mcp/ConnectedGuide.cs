using Microsoft.Extensions.Configuration;

namespace TomPIT.Mcp;

internal static class ConnectedGuide
{
	private static string SourceFilesFolder
		=> Shell.Configuration.GetRequiredSection("sourceFiles").GetValue<string>("folder") ?? "(unknown)";

	internal static string Content => $"""
# TomPIT Connected — AI Coding Guide

## Session Setup (do this first)

1. Call **`tool_help`** to get the URL for full tool descriptions and this guide.
2. Ask the user for the **local mount path** of the source files folder.
   The server stores source files under: `{SourceFilesFolder}`
   That path is from inside the Docker container — ask:
   > "What is the local path where `{SourceFilesFolder}` is mounted on your machine?"
3. Store the answer. Whenever a tool returns a `filePath`, replace the `{SourceFilesFolder}`
   prefix with the local path before reading the file.

---


## Overview

TomPIT Connected is a multi-tenant microservice platform where:
- **Code lives in a database**, not on the filesystem. Use MCP tools to read/write it.
- C# scripts are compiled on-demand using **Roslyn Scripting** with custom `#load` directives.
- Each microservice is an independently deployed unit containing typed components.
- The `Url` field (short slug) is the canonical identifier used in `#load` paths and most tool arguments.

---

## Core Concepts

### Microservices
| Field | Description |
|---|---|
| `token` | GUID — stable identifier, use for lookups when you have it |
| `name` | Display name (e.g., `"Acme Orders"`) |
| `url` | URL slug (e.g., `"acme-orders"`) — used in `#load` paths |
| `version` | Semver string |
| `resourceGroup` | Isolation group for multi-tenancy |

### Components
A component is the unit of functionality. Key fields:
| Field | Description |
|---|---|
| `token` | GUID — use this in all tool calls |
| `name` | Human name (e.g., `"OrderService"`) |
| `category` | Type: Api, Script, View, Queue, etc. (see table below) |
| `nameSpace` | Compilation group: PublicScript, InternalScript, View, Data, Resource, etc. |
| `folder` | GUID of parent folder, or null for root |

---

## Component Categories

### PublicScript Namespace
These are C# script components compiled together. They can `#load` each other freely.

| Category | Description |
|---|---|
| `Api` | Callable API. Each **operation** is a separate source element (e.g., `GetOrder.csx`). |
| `Script` | Reusable utility library. Usually one `.csx` file. |
| `Model` | Data model / query definitions. |
| `Settings` | Configuration settings component. |
| `Entity` | Business entity definition. |

### InternalScript Namespace
Background/event-driven components. Same compilation rules as PublicScript.

| Category | Description |
|---|---|
| `Queue` | Background queue worker. Processes items asynchronously. |
| `Subscription` | Event subscription handler. |
| `DistributedEvent` | Cross-service distributed event handler. |
| `HostedWorker` | Long-running background worker (IHostedService). |
| `IoCContainer` | Dependency injection container configuration. |
| `Middleware` | Request middleware. |

### View Namespace
Razor (.cshtml) components. Not compiled with C# scripting engine.

| Category | Description |
|---|---|
| `View` | Razor view. Rendered for HTTP requests. |
| `MasterView` | Master layout page. Views reference this for the outer HTML shell. |
| `Partial` | Reusable view fragment rendered inside other views. |
| `MailTemplate` | Email body template. |

### Other Categories
| Category | Namespace | Description |
|---|---|---|
| `Connection` | Data | Database connection string configuration. |
| `Theme` | Resource | CSS/LESS theme. |
| `ScriptBundle` | Resource | JavaScript/TypeScript bundle for the browser. |
| `StringTable` | Resource | Localization string resources. |
| `Media` | Resource | Binary media assets. |
| `UnitTest` | Quality | Unit test component. |
| `Reference` | Reference | Declares a dependency on another microservice. |

---

## Source Elements (IText)

A component may contain **one or more source elements**, each with:
- `elementName` — logical name (file name without extension, e.g., `GetOrder`)
- `fileName` — full file name including extension (e.g., `GetOrder.csx`)
- `source` — the actual source code string

**Api components**: each public operation is a separate element:
```
OrderApi/
  GetOrder.csx        ← operation: GetOrder
  CreateOrder.csx     ← operation: CreateOrder
  CancelOrder.csx     ← operation: CancelOrder
```

**Script/Model/etc.**: typically one element matching the component name.

---

## C# Script System

### #load Directives
TomPIT C# scripts use custom `#load` paths — **not filesystem paths**:

```csharp
#load "microServiceUrl/ComponentName"              // loads all elements of a component
#load "microServiceUrl/ComponentName/ElementName"  // loads a specific element
#load "ComponentName"                              // loads from current microservice (no prefix)
```

- `microServiceUrl` = the microservice's **`url` field** (slug), not its name
- `ComponentName` = the component's **`name`** (case-insensitive)
- `.csx` extension is optional and stripped automatically
- Resolution is done at compile time by the Roslyn `ScriptResolver`
- **Cross-service `#load` requires a `Reference` component** in the caller's microservice

### Following Dependencies
When you see `#load "appservice/Helpers"` in a script:
1. Call `script_load_resolve(path: "appservice/Helpers")` to read that script's source
2. The response includes `componentToken` — use it to call `component_source_read` or `component_dependencies` for deeper analysis
3. Recurse as needed

### #r Directives (Assembly/NuGet References)
```csharp
#r "microServiceUrl/NuGetPackageName"   // NuGet package registered in that microservice
#r "SomeAssembly"                       // System or preloaded assembly
```

### C# Script Base Classes

| Category | Typical Base Class | Notes |
|---|---|---|
| Api operation (with result) | `Api<TResult>` | `return value;` from Execute() |
| Api operation (void) | `Api` | No return value |
| Queue worker | `QueueWorker<TModel>` | Processes one item per Execute() call |
| Subscription | `SubscriptionWorker<TEvent>` | Receives event payload |
| Hosted worker | `HostedWorker` | Runs in a loop; override ExecuteAsync() |

Access platform services inside scripts:
```csharp
// From middleware/script context:
var svc = Context.GetService<IMyService>();

// Static access (available anywhere):
var svc = Tenant.GetService<IMyService>();
```

---

## Microservice References

A `Reference` component in microservice A pointing at microservice B means:
- A's scripts can `#load` B's components
- At runtime, A can invoke B's APIs directly

Query the graph with `microservice_references(microService: "my-service")`:
- `dependsOn` — services this one references (can `#load` from these)
- `usedBy` — services that reference this one

---

## JavaScript / TypeScript

- Stored as `.js`, `.ts`, or `.mjs` text blobs (same IText model as C# scripts)
- Standard ES6 module syntax — **no custom TomPIT resolution** for JS imports
- JS interop with C# happens via HTTP: JS calls published TomPIT API endpoints
- ScriptBundle components bundle JS files for browser delivery
- View components can embed JS inline or reference bundles

---

## Razor Views

- Standard Razor / `.cshtml` syntax
- `@using Namespace` — imports from C# compiled components in the same tenant
- `@inject IService service` — injects compiled services
- MasterView sets the outer HTML; Views extend it via `Layout = "master"` (or similar)
- Partials are rendered with `@Html.Partial("partialComponentName")`

---

## Recommended Tool Workflow

### Explore a microservice
```
1. microservice_list                         — see all microservices
2. microservice_get(microService: "slug")    — get details on one
3. microservice_references(microService: "slug")  — see its dependency graph
4. component_list(microService: "slug")      — list all components
5. component_list(microService: "slug", category: "Api")  — filter by category
```

### Read source and follow dependencies
```
1. component_source_read(componentToken: "guid")
   → read all source elements
2. component_dependencies(componentToken: "guid")
   → see #load paths and what they resolve to
3. script_load_resolve(path: "appservice/Helpers")
   → read source of a #load dependency directly
```

### Edit source
```
component_source_write(componentToken: "guid", elementName: "GetOrder", content: "...")
```
- Omit `elementName` only if the component has exactly one source element
- For Api components, always specify the operation name as `elementName`
- Writes trigger automatic recompilation on the server

### Test / invoke an API
```
api_invoke(microService: "slug", operation: "ComponentName.OperationName", payload: <json>)
```

---

## Key Rules for AI Code Generation

1. **File paths from tools are container paths** — translate using the local mount path obtained at session setup before reading them.
2. **`#load` paths use the `url` slug**, not the component `token` or display `name`.
3. **Cross-service `#load` requires a Reference** — check `microservice_references` before assuming a cross-service load is valid.
4. **Api operation classes must match their file** — a file `GetOrder.csx` must contain a class that the runtime can instantiate. The class name does not need to match the file, but convention is to use the same name.
5. **`IoCContainer` (InternalScript) is for DI wiring** — don't put business logic there.
6. **Namespace rewriting is automatic** — do not worry about namespace declarations in `.csx` files; the compiler wraps them appropriately.
7. **`Context` vs `Tenant`** — inside script execution use `Context.GetService<T>()`. In static/infrastructure code use `Tenant.GetService<T>()`.
""";
}
