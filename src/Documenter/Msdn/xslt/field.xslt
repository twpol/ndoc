﻿<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<!-- -->
	<xsl:output method="html" indent="no" />
	<!-- -->
	<xsl:include href="common.xslt" />
	<!-- -->
	<xsl:param name='field-id' />
	<!-- -->
	<xsl:template match="/">
		<xsl:apply-templates select="ndoc/assembly/module/namespace/*/field[@id=$field-id]" />
	</xsl:template>
	<!-- -->
	<xsl:template match="field">
		<html dir="LTR">
			<xsl:call-template name="html-head">
				<xsl:with-param name="title" select="concat(../@name, '.', @name, ' Field')" />
			</xsl:call-template>
			<body>
				<xsl:call-template name="title-row">
					<xsl:with-param name="type-name">
						<xsl:value-of select="../@name" />.<xsl:value-of select="@name" /> Field
					</xsl:with-param>
				</xsl:call-template>
				<div id="content">
					<xsl:call-template name="summary-section" />
					<xsl:call-template name="field-or-event-syntax" />
					<p></p>
					<xsl:call-template name="remarks-section" />
					<xsl:call-template name="example-section" />
					<xsl:call-template name="seealso-section">
						<xsl:with-param name="page">field</xsl:with-param>
					</xsl:call-template>
					<object type="application/x-oleobject" classid="clsid:1e2a7bd0-dab9-11d0-b93a-00c04fc99f9e" viewastext="viewastext">
						<xsl:element name="param">
							<xsl:attribute name="name">Keyword</xsl:attribute>
							<xsl:attribute name="value"><xsl:value-of select='@name' /> field</xsl:attribute>
						</xsl:element>
						<xsl:element name="param">
							<xsl:attribute name="name">Keyword</xsl:attribute>
							<xsl:attribute name="value"><xsl:value-of select='@name' /> field, <xsl:value-of select='../@name' /> class</xsl:attribute>
						</xsl:element>
						<xsl:element name="param">
							<xsl:attribute name="name">Keyword</xsl:attribute>
							<xsl:attribute name="value"><xsl:value-of select='../@name' />.<xsl:value-of select='@name' /> field</xsl:attribute>
						</xsl:element>
					</object>
				</div>
				<xsl:call-template name="footer-row" />
			</body>
		</html>
	</xsl:template>
	<!-- -->
</xsl:stylesheet>
