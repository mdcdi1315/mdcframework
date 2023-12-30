

# About the `System.Resources.Extensions` Project:

(Note: This project is only avaliable for .NET Framework builds. For .NET , use the normal 
[`System.Resources.Extensions`](https://nuget.org/packages/System.Resources.Extensions) package.)

This Project is actually a clone of the original `System.Resources.Extensions` DLL , 
but with a small difference:

It works by not using the `System.Memory` package (and any dependency that it carries out)
but instead using MDCFR , which does already have the required code to do that.

This automatically allows you to not have a bunch of dependencies , just MDCFR itself.

This project is located here because it was historically embedded in MDCFR in a try
to embed the `System.Resources.Extensions` DLL to MDCFR for more less dependencies.

However , the code provided there was useless and was not used for it's real purpose , which is
to preserialise the resources. However , even correctly building it with the proper information ,
you will be lead to a problem where the `System.Resources.Extensions` DLL could not be found.

To resolve this error is simple : Just make sure to import a file into your project called
`System.Resources.Extensions.targets`. This will give to the compiler the correct information
so as to correctly provide the assembly information.

This file comes together with the custom built `System.Resources.Extensions` DLL and you can find it from there.

Example to reference the file:
~~~XML
<Project Sdk="Microsoft.NET.Sdk">

	<Import Project="System.Resources.Extensions.targets" />

	<!-- the rest of your project file here -->

</Project>
~~~

Specifically you need to add this line:
~~~XML
<Import Project="System.Resources.Extensions.targets" />
~~~
which will provide the correct information to the compiler.

Note: When you generate your .NET Framework application , the `.exe.config` is REQUIRED so as to resolve the 
`System.Resources.Extensions` assembly correctly.