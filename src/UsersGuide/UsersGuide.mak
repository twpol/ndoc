
#change the value of this macro to where you keep the html help compiler
HHC= "C:\Program Files\HTML Help Workshop\hhc.exe"

!IF !EXIST ($(HHC))
!ERROR Could not find the html help compiler
!ENDIF

.SUFFIXES :
.SUFFIXES : .chm .hhp .htm .html .css

	
# hhc returns 1 on success, which nmake interprets as an error
# ignore non-zero exit codes
.IGNORE :
NDocUsersGuide.chm: UsersGuide.hhp
	$(HHC) UsersGuide.hhp

# the UsersGuide.hhp pseudotarget is dependent on all of the html files in the content directory
UsersGuide.hhp: {content}*.htm 
