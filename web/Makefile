.SUFFIXES: .xml

.xml.html:
	msxsl $< ndoc.xslt > $@

FILES = index.html screenshots.html cvs.html

all: $(FILES)

clean:
	del /q $(FILES)

$(FILES): ndoc.xslt layout.xml releases.xml developers.xml
