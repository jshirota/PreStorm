PreStorm
========

A Parallel REST Client for ArcGIS Server

PreStorm is an ultra-lightweight HTTP client for ArcGIS Server feature services.

Here are some of the key features:

- super-minimal object-relational mapping
- intuitive and optimized CRUD operations
- generic (and strict) geometry typing
- sophisticated memoization optimizations
- server max return count overriding
- querying using layer name (as well as layer id)
- parallel downloading via PLINQ
- server-side spatial filter
- built-in local geometry functions (i.e. spatial predicates)
- dynamic field access via indexers
- internally managed self-renewing token
- "geodatabase version" support
- automatic handling of INotifyPropertyChanged (via Reflection.Emit but memoized)
- flexible and succinct KML generation
- delimiter-separated text dump
- implicit conversion operators for JSON geometries
- supports geometry conversion to well-known text (WKT)
- z coordinates support
- support for extra ArcGIS Rest query string parameters
- Unicode tested
- no dependencies on IDE tools or XML mapping files
- works well with Esri Runtime APIs
- designed with WPF UI binding in mind (i.e. MVVM)
- supports ArcGIS Server 10.0 and up
- supports .NET 4.0 and up
- supports .NET Core and Windows 10 UWP
- runs on OS X via Mono or CoreCLR
- strong support for fluent programming in C# and Visual Basic

========

The ClickOnce installer is here:

http://jshirota.github.io/PreStorm/PreStorm.Tool.application

The library reference:

http://jshirota.github.io/PreStorm/Help/

The NuGet package is here:

http://nuget.org/packages/PreStorm

========
