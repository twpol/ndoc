<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:template match="custom" mode="summary-section">
		<h1 style="color:green">
			<xsl:value-of select="."/>
		</h1>
	</xsl:template>
	<xsl:template match="mySeeAlso" mode="seealso-section">
		<i style="color:green">
			<xsl:value-of select="."/>
		</i>
	</xsl:template>	
</xsl:stylesheet>

  