## Making a new request for adding you own Project:

To make a new request to add your own Project in the MDCFR library , it has some requirements and restrictions.

I strongly believe that any project that would be useful for everyone should be actually added to the MDC
Framework.

### Requirements:

1. Your project code should be written to target at least .NET Framework 4.6.2 version or greater.

If your project code is targeted for .NET >= 6 , then check out if it can be __FULLY__ ported to .NET Framework.

(Known restriction: ref members in classes or strctures are unsupported by .NET Framework)

__IF__ your project depends on API's that are of .NET >= 6 , then check also out if any of the embedded 
packages cover your project's needs.

The only exception to this rule is the package `System.Runtime.CompilerServices.Unsafe` , which is needed for
numerous code applications in the library , and therefore it is mandatorily included as a reference to the 
MDCFR Library , and therefore , you can also consider it as an 'globally' included shipped package.

2. It is of course implied that your project must be of __Library__ type , and __NOT__ an executable.

3. Your project must include a license , or at least something that indicates you , as the Author of the project _You_ submit.

4. Your project must be written in the C# language. I do not care in which version your C# code is ,
I care if it is compilable to .NET Framework >= 4.6.2.

5. You have to accept any namespace name change or source file name change done before your code is embedded in a new major or minor release of the MDCFR Library.

6. _If_ your project needs also some resources to work correctly , then add them in the Resources.resx 
file provided , but if these are __NOT__ string resources , then you have to notify me to review your code
and provide you the next steps/moves you need to follow.

7. Finally , the source code that _YOU_ will provide must be of your project's latest stable major or minor release.

(This is done so as to avoid common errors/issues in your Project.)

### Guide to create a new request for your Project addition:

__Step 1.__ Clone the MDCFR Repository to your local computer

(You can also just download the ZIP from GitHub)

__Step 2.__ Add your project's source code in a single new file and give it a name. Be noted that the filename will be changed.

__NOTE!!!__ Your Project code must be in only one source file. 
However , if your code size exceeds 2.5 ~ 2.7 MB , then you can add 
a new source file containing the rest of your code.

__Step 3.__ Add a reference to your newly added source file to the project file.

See an example:
~~~XML
<ItemGroup>
	<Compile Include=".\MySourceFileName.cs" />
	<Compile Include=".\MDCFR-DEFAULT.cs" />
	<!-- This list goes on... -->
</ItemGroup>
~~~
Where `MySourceFileName` the temporary filename of your source file.

__Step 4.__ Create a new Pull Request.

Additionally write or copy exactly this prefix to the Pull Request description: `"Project Addition:"`
(Without the double quotes please).

__NOTE!!__ Any Pull Request that does not have that prefix and intends a Project Addition 
will be turned down immediately.

__Step 5.__ Write a small description of your Project , (e.g. what it does and how it would help the other developers which using it)
, and send the Pull Request.

__Step 6.__ You have to now just wait. You will recieve an answer whether your project will be included or not in the next minor release as
an experimental feature. If yes , the Pull Request will remain open until the next major release of the MDCFR Library.

If your project sent is confirmed to be added for the next major release , it will be closed; otherwise , it will not be included to the MDCFR Library.

## What happens if I want to update the source code of my project that is also in the MDCFR Library?

First , your project must have been added sucessfully to the MDCFR Library two 
major versions before of the MDCFR latest major version.

Secondly , the project code you update , add or delete must also be from the latest
stable major or minor version of _Your_ project.

Then just create a new Pull Request that describes which project components you updated
(There is __NO__ need to describe your changes analytically) or added and submit it.

This kind of request can be only rejected if regressions are found that
evolves your project's source code.





