@echo off
echo Hit ENTER when prompted for your CVS password.
cvs -d:pserver:anonymous@cvs.ndoc.sourceforge.net:/cvsroot/ndoc login
cvs -d:pserver:anonymous@cvs.ndoc.sourceforge.net:/cvsroot/ndoc -z3 co ndoc
cvs -d:pserver:anonymous@cvs.ndoc.sourceforge.net:/cvsroot/ndoc logout
