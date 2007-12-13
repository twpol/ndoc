Instructions For Updating the Web Site
--------------------------------------

To build the HTML files locally, type 'nmake'. You need to have MSXML3 or 
higher registered and MSXSL.exe in your path.

To upload the HTML files to the web site, type 'deploy username' where
'username' is your SourceForge username. If you don't have an SSH key on 
SourceForge then you'll be prompted for your password.

If it says permission denied while copying any of the files then let me 
(jason) know so that I can update the permissions. (Usually the first person
to copy the file to the web site is the only user who has write access.)

If you want to modify the text of any of the pages, edit the XML source for
that page and then rebuild. It should be fairly easy once you look inside
the XML files.

If you want to add a new release or a new developer, then open up 
releases.xml or developers.xml, follow the examples you see in there, and
rebuild.

If you want to modify the layout, take a look at layout.xml. That file is a
template for all the other files. It's just normal XHTML with some extra 
'instructions' to insert some variables here and there. This keeps our
pages consistent across the site. If you need help adding new or modifying
any of the old instructions then let me (jason) know.
