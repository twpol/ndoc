<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<!-- section tag templates -->
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
	
	<!-- inline tag templates -->
	<xsl:template match="null" mode="slashdoc">
		<xsl:text> null reference (Nothing in Visual Basic) </xsl:text>
	</xsl:template>
	<xsl:template match="static" mode="slashdoc">
		<xsl:text> static (Shared in Visual Basic) </xsl:text>
	</xsl:template>
	<xsl:template match="true" mode="slashdoc">
		<b>true</b>
	</xsl:template>
	<xsl:template match="false" mode="slashdoc">
		<b>false</b>
	</xsl:template>
</xsl:stylesheet>

  