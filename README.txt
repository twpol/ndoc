NDoc
----

NDoc merges the assemblies and XML documentation files produced by csc.exe 
and creates a compiled HTML Help file enabling you to browse your own 
libraries just like the .NET Framework Class Library.

The documentation generator uses XSLT to produce the HTML. Future versions 
will make it possible to customize the XSLT to produce documentation with 
any desired look and feel.

The NDoc source code is freely available under a certified Open Source 
license. We welcome everyone to use our software in the hopes that they can 
provide feedback, submit bug reports and fixes, or even join us as a 
developer!

Status
------

As there's still quite a bit of work to be done, this is only a developer
release. You'll have to build it yourself by following the directions
below. It is, however, very usable as evidenced by the many developers
using it every day. There's no documentation but the GUI is simple enough
that you should have no trouble figuring it. You are a developer, right?

See http://ndoc.sf.net/ for the latest information on releases.

Building NDoc
-------------

If you have Visual Studio .NET, you can build NDoc using the .sln file
located in the src directory.

If you don't have VS.NET but do have the .NET Framework SDK installed
then you can build NDoc using the Makefile or the NAnt .build file both
of which are also located in the src directory. Just type nmake or nant 
from within that directory and you should be good to go.

If you don't have the .NET Framework SDK installed then why do you even
want to use this?

Using NDoc
----------

Start NDocGui.exe. You can add your assembly/doc files manually or import
them from a VS.NET .sln file. If this UI's not intuitive enough then feel
free to add a bug report to our project as described below.

You can also invoke NDoc from the command line. Try typing NDocConsole.exe
with no options to see how it's supposed to be used. This is great for 
automated builds.

Support
-------

Since this is an Open Source project which we work on in our spare time, you
won't be getting any support. But if you do find a bug or have a comment and
you post it to our mailing list or one of our trackers on SourceForge then
we'll try really hard to help you out. Some bugs have been fixed and features
added within meer hours. Others have taken a few weeks. (That's still faster
than most closed source products!)

You can subscribe to any of our mailing lists here:

http://sourceforge.net/mail/?group_id=36057

You can submit a bug report or feature request here:

http://sourceforge.net/tracker/?group_id=36057

If you do submit something to our tracker, please make sure you're logged in
to SourceForge or leave your email address in the submission if you don't 
want a SourceForge account. That way we can can contact you for further 
questions or let you know when we fix whatever it is you need.

We love getting feedback so don't hesitate to contact us!

-- The NDoc Team
