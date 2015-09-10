PreStorm ![PreStorm](http://jshirota.com/PreStorm/PreStorm.png "PreStorm")
========

A Parallel REST Client for ArcGIS Server

Do you have Visual Studio?  Please try:

http://jshirota.com/PreStorm/

PreStorm is an ultra-lightweight HTTP client for ArcGIS Server feature services.

Here are some of the key features:

- super-minimal object-relational mapping
- intuitive and optimized CRUD operations
- generic (and strict) geometry typing
- sophisticated memoization optimizations
- optional automatic coded-value domain mapping
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
- z coordinates support
- support for extra ArcGIS Rest query string parameters
- Unicode tested
- no dependencies on IDE tools or XML mapping files
- no external libraries
- works well with Esri Runtime APIs
- designed with WPF UI binding in mind (i.e. MVVM)
- strong support for fluent programming in C# and Visual Basic

========

The ClickOnce installer is here:

http://jshirota.github.io/PreStorm/PreStorm.Tool.application

The library reference:

http://jshirota.github.io/PreStorm/Help/

The NuGet package is here:

http://nuget.org/packages/PreStorm

========

## Examples

You can download features like this.

![code](/images/p01.png)

Notice the city has x and y.

![code](/images/p02.png)

The highway doesn't.  But, it has paths instead.  Basic geometry functions (i.e. Length) are built in.  Please note these functions are based on 2D planar calculations.

![code](/images/p03.png)

So, how does the framework know a county is polygon?  It doesn't.  It's based on how you define the entity.

![code](/images/p04.png)

Here's how you can export features as a KML file.  If you set keepQuerying to true, all features are returned.  You can also set the degreeOfParallelism for parallel downloading via PLINQ.

![code](/images/p05.png)

This is one way of updating.

![code](/images/p06.png)

![code](/images/p07.png)

![code](/images/p08.png)

![code](/images/p09.png)

![code](/images/p10.png)

![code](/images/p11.png)

![code](/images/p12.png)

![code](/images/p13.png)

![code](/images/p14.png)

![code](/images/p15.png)

![code](/images/p16.png)

![code](/images/p17.png)

![code](/images/p18.png)

![code](/images/p19.png)

![code](/images/p20.png)

![code](/images/p21.png)

![code](/images/p22.png)

![code](/images/p23.png)

![code](/images/p24.png)

![code](/images/p25.png)

![code](/images/p26.png)

![code](/images/p27.png)

![code](/images/p28.png)
