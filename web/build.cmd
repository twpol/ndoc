@echo off
saxon ndoc.xml ndoc.xslt > index.html
saxon ndoc.xml ndoc.xslt page=screenshots "title=NDoc Screenshots" > screenshots.html
saxon ndoc.xml ndoc.xslt page=cvs "title=NDoc CVS Instructions" > cvs.html
