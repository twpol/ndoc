<?xml version="1.0" encoding="utf-8"?>
<xsl:transform version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:output
		method="xml"
		encoding="utf-8"
		indent="no"
		doctype-public="-//W3C//DTD XHTML 1.0 Strict//EN"
		doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"
	/>

	<xsl:variable name="page" select="/page" />
	<xsl:variable name="layout" select="document('layout.xml')" />
	<xsl:variable name="links" select="document('links.xml')/links" />
	<xsl:variable name="releases" select="document('releases.xml')/releases" />
	<xsl:variable name="developers" select="document('developers.xml')/developers" />

	<xsl:template match="/">
		<xsl:apply-templates select="$layout/html" />
	</xsl:template>

	<xsl:template match="*">
		<xsl:element name="{name()}" namespace="http://www.w3.org/1999/xhtml">
			<xsl:apply-templates select="@*|node()" />
		</xsl:element>
	</xsl:template>

	<xsl:template match="@*">
		<xsl:attribute name="{name()}" namespace="">
			<xsl:value-of select="." />
		</xsl:attribute>
	</xsl:template>

	<xsl:template match="insert-title">
		<xsl:value-of select="$page/title" />
	</xsl:template>

	<xsl:template match="insert-body">
		<xsl:apply-templates select="$page/body/node()" />
	</xsl:template>

	<xsl:template match="insert-links">
		<xsl:apply-templates select="$links/link" />
	</xsl:template>

	<!-- be explicit and match "/links/link" so that we don't accidentally match "/html/head/link". -->

	<xsl:template match="/links/link">
		<p><a href="{@href}"><xsl:value-of select="." /></a></p>
	</xsl:template>

	<xsl:template match="insert-releases">
		<xsl:apply-templates select="$releases/release">
			<xsl:sort select="@date" order="descending" />
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="release">
		<p><a href="{@link}"><xsl:value-of select="@name" /></a></p>
	</xsl:template>

	<xsl:template match="insert-latest-release">
		<xsl:apply-templates select="$releases/release[last()]" />
	</xsl:template>

</xsl:transform>
