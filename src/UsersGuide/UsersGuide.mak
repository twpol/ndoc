#change the value of this macro to where you keep the html help compiler
HHC= "C:\Program Files\HTML Help Workshop\hhc.exe"


!IF !EXIST ($(HHC))
!ERROR Could not find the html help compiler
!ENDIF


.SUFFIXES :
.SUFFIXES : .chm .hhp .htm .html .css .jpg .png .gif .js

	
# hhc returns 1 on success, which NMAKE interprets as an error
# ignore non-zero exit codes
.IGNORE :
NDocUsersGuide.chm: UsersGuide.hhp
	$(HHC) UsersGuide.hhp
	copy NDocUsersGuide.chm ..\Gui\bin\$(CONFIG) /y


# these are the various file types that the chm is dependent on
IMAGES = {content\images}*.gif {content\images}*.png {content\images}*.jpg
SCRIPTS = {content\script}*.js
CSS = {content\css}*.css
HTML = {content}*.htm 


# the UsersGuide.hhp pseudotarget is dependent on all of the html files in the content directory
# as well as the css, image, and script files
UsersGuide.hhp: $(HTML) $(CSS) $(SCRIPTS) $(IMAGES)
