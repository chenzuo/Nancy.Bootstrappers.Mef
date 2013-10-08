## Managed Extensibility Framework (MEF) Bootstrapper

This is a bootstrapper for using MEF with Nancy.  A couple of things are worth noting about this bootstrapper that differ from the others. MEF, and most users of MEF, are quite comfortable with explicitly decorating exports, or making the concious decision to use the RegistrationBuilder to register parts. Nancy by default wants to scan the entire AppDomain, including on disk locations. Nancy's assemblies, both the core Nancy distribution, and any associated plugins: such as custom ViewEngines, of course don't provide these attributes. And they cannot be added.

MEF can solve this by introducing Export and Import attributes at runtime using a ReflectionContext which is provided to a MEF catalog. These contexts introduce "fake" attributes. Thus, I have provide a ReflectionContext as part of this distribution which takes care of this.

MEF also does not support constructor selection based on available types. This can be solved, but I have opted to not solve it just yet. Instead, the constructor with the most parameters is chosen. This works fine for Nancy, as it stands now. If this changes in the future, it will also have to be fixed.

Most IoC containers also support importing single instances where multiple instances are available. MEF does not. Instead, MEF requires you to import an enumerable, or a set, of the appropriate type. There are a few places in Nancy that expect the container to operate this way. This has been solved through the use of a special NancyExportProvider class which alters the import definitions of Nancy parts to select the first discovered component. In order to not leak this feature into the rest of your MEF code (which would cause you to make incorrect assumptions in other places your MEF container is used) I opted to implement this functionality as a custom ExportProvider, instead of requiring your MEF container to be altered to operate in this fashion.

This introduces another burden: your container must have a NancyExportProvider available, in which NancyTypeCatalogs will be added; instead of those catalogs being added to the container itself. The default bootstrapper creates a CompositionContainer with this NancyExportProvider added. If you are going to provide your own container instance you will have to do one of two things:

1) Ensure a NancyExportProvider is added to your container.
2) Override the AddCatalog method on the bootstrapper to add NancyTypeCatalogs to a NancyExportProvider in whatever manner you see fit.

The same applies to the per-request container.

This brings up AddCatalog. This method is available on the Nancy MEF bootstrapper base class. It is invoked when Nancy discovers a set of classes that it wants to add to the container. These classes are wrapped in a NancyTypeCatalog (which includes the appropriate NancyReflectionContext) and handed to AddCatalog. AddCatalog is expected to add this provided catalog to the NancyExportProvider. The default implementation looks for a NancyExportProvider on the container.

Most of the Nancy expectations are upheld. Nancy continues to use its own AppDomain assembly scanner code. It continues to implement it's own logic for deciding which types to inject into the container. These types are merely added to the container on the fly and wrapped with special classes.

One more caveat. Before adding a Nancy type to a NancyTypeCatalog and invoke AddCatalog, the container is checked to see if the type is already available. This allows you to add the Nancy types to the MEF container yourself, if you choose, and the bootstrapper will not add them twice. In this way, if you desire, you can register all of the parts in the container yourself (taking care to properly implement the policy provided by NancyExportProvider), and use your own catalog, or ReflectionContext. To ignore all of Nancy's decisions about what to add, simply override AddCatalog and do nothing; or disable its assembly scanner in some other fashion.

## Contributors

* [Jerome Haltom](http://github.com/wasabii)

## Copyright

Copyright © 2013 Jerome Haltom

## License

Nancy.Bootstrappers.Mef is licensed under [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form"). Refer to license.txt for more information.
