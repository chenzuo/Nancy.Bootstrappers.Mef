## Managed Extensibility Framework (MEF) Bootstrapper

This is a bootstrapper for using MEF with Nancy.  A couple of things are worth noting about this bootstrapper that differ from the others. MEF, and most users of MEF, are quite comfortable with explicitly decorating exports, or making the concious decision to use the RegistrationBuilder to register parts. Nancy by default wants to scan the entire AppDomain, including on disk locations. I've turned this off. It doesn't fit well into a MEF workflow. MEF after all already provides things like ApplicationCatalog to accomplish this same thing, except with more flexibility.

But this does present something of a problem. Nancy, and all of Nancy's related plugins, don't go covering their assemblies with Export attributes. So now the container has no way to find them. Except it does: Nancy.Bootstrappers.Mef.NancyAssemblyCatalog and NancyTypeCatalog, both based on NancyReflectionContext. These are special versions of catalogs that expose the stuff that Nancy would otherwise register in the container for you, using MEF itself.

This is fine when consuming Nancy extension assemblies from other people. If you write your own, please be kind and drop CatalogReflectionContextAttribute on your assembly, or continue to decorate them with MEF attributes properly.

## Contributors

* [Jerome Haltom](http://github.com/wasabii)

## Copyright

Copyright © 2013 Jerome Haltom

## License

Nancy.Bootstrappers.Mef is licensed under [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form"). Refer to license.txt for more information.
