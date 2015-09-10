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

Here's how you can export features as a KML file.  If you set keepQuerying to true, all features are returned.  You can also set the degreeOfParallelism for parallel downloading via PLINQ (hence the name PLINQ REST ORM).

![code](/images/p05.png)

This is one way of updating.  This ends up with multiple HTTP calls.

![code](/images/p06.png)

Here's another way.  You create an array first, mutate the content and invoke Update.  Only the features (and fields) that actually changed are committed.

![code](/images/p07.png)

LINQ where does not get translated to SQL.  If you need server-side filtering, you have to sepcify the where clause yourself.

![code](/images/p08.png)

If you don't want to hard code the where clause (or you simply don't know the underlying field names), you can do something like this as well.

![code](/images/p09.png)

Exporting a CSV file is also 1 line.

![code](/images/p10.png)

Downloading happens lazily and is not limited to server max return count.  So, if you're downloading millions of records, you would want to write to a stream.

![code](/images/p11.png)

Instead of mapping a field to a property, you can also access it directly via the indexer.  This is generally not recommended, but it can be useful in certain scenarios.

![code](/images/p12.png)

In an extreme case, you don't need to define a type.  You can use Download instead of the generic Download&lt;T&gt;.  If you do this, Geometry is dynamically determined.

![code](/images/p13.png)

Here's a basic insert operation.

![code](/images/p14.png)

Again, create an array first and invoke InsertInto.  This uploads multiple features via a single HTTP call.

![code](/images/p15.png)

This is the result of the above code.

![code](/images/p16.png)

InsertInto actually returns the result object, which also provides the inserted features.  These features are bound to the service and have server-generated field values (i.e. OID).

![code](/images/p17.png)

If you don't care about the inserted features, but you just want to know if the batch insert was successful, you can do this.  The inserted features are not downloaded until you ask for them.

![code](/images/p18.png)

Here's one way of batch deleting.

![code](/images/p19.png)

Instead of the layer name, you can specify the layer id.  However, layer id can change when you modify the underlying map document.

![code](/images/p20.png)

This means there's a map of layer ids to layer names.  This is stored in the instace of Service.  However, even if you create many instances of this Service class with the same url, the schema is fetched only once and memoized.  The following code results in 1 HTTP call to fetch the service schema.

![code](/images/p21.png)

The Service class provides the basic schema for the map service, so you can do general dynalic handling of data.

![code](/images/p22.png)

Here's an example of binding features in an Windows Forms application.  Notice if you edit any of the (editable) fields, IsDirty becomes true.  This is because the mapped properties are automatically raising the PropertyChanged event.  PreStorm uses this mechanism internally to keep track of which fields are changing.  This way, we can send only the fields that changed during the update call.  This also works well with WPF binding.

![code](/images/p23.png)

![code](/images/p24.png)

Here's a typical type definition used by PreStorm.  The tool generates this for you.  Mapped properties must be defined as public virtual.  At runtime, PreStorm creates a new type inheriting from this defined type and these properties are overridden.  The use defined type itself inherits from Feature or Feature&lt;T&gt; where T is the strict geometry type.  This how we declare that the city has x and y and the county has an area.

![code](/images/p25.png)

If the field is nullable, the tool maps it to a nullable property (i.e. int?).  If the field is not nullable, the tool adds the Required attribute, which works with validation mechanisms used in .NET (i.e. ASP.NET).  If the field is read-only, the tool will define the property as "protected set".  This prevents the developer from accidentally changing the value of it.

![code](/images/p26.png)

Non-spatial tables are mapped to types that inherit from the non-generic Feature type, which doesn't have geometry.  You can also treat a spatial layer as a non-spatial table as well by simply removing the generic parameter.

![code](/images/p27.png)

If you do this, the Geometry property becomes no longer available.  If you use it, your code simply won't compile.  When the defined type is non-spatial, PreStorm automatically sets returnGeometry to false even if the underlying table is spatial, which results in further optimization.

![code](/images/p28.png)

PreStorm provides many dynamic features but generally guides you in statically typed space where it becomes easy to write the code and difficult to make mistakes.
