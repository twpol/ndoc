<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:MSHelp="http://msdn.microsoft.com/mshelp"
	xmlns:NUtil="urn:ndoc-sourceforge-net:documenters.NativeHtmlHelp2.xsltUtilities" 
	exclude-result-prefixes="NUtil">
	<!-- -->
	<xsl:output method="xml" indent="yes" encoding="utf-8" omit-xml-declaration="yes" />
	<!-- -->
	<xsl:include href="common.xslt" />
	<!-- -->
	<xsl:param name='namespace' />
	<!-- -->
	<xsl:template match="/">
		<xsl:variable name="ns" select="ndoc/assembly/module/namespace[@name=$namespace]" />
		<html dir="LTR">
			<xsl:call-template name="html-head">
				<xsl:with-param name="title" select="concat($ns/@name, 'Hierarchy')" />
				<xsl:with-param name="page-type" select="'hierarchy'" />
			</xsl:call-template>
			<body topmargin="0" id="bodyID" class="dtBODY">
				<object id="obj_cook" classid="clsid:59CC0C20-679B-11D2-88BD-0800361A1803" style="display:none;"></object>
				<xsl:call-template name="title-row">
					<xsl:with-param name="type-name" select="concat($ns/@name, ' Hierarchy')" />
				</xsl:call-template>
				<div id="nstext" valign="bottom">
					<xsl:apply-templates select="$ns/typeHierarchy" />
					<h4 class="dtH4">See Also</h4>
					<p>
						<a>
							<xsl:attribute name="href">
								<xsl:value-of select="NUtil:GetNamespaceHRef( string( $namespace ) )" />
							</xsl:attribute>
							<xsl:value-of select="$namespace" /> Namespace
						</a>
					</p>
					<xsl:call-template name="footer-row">
						<xsl:with-param name="type-name" select="concat($ns/@name, ' Hierarchy')" />
					</xsl:call-template>
				</div>
			</body>
		</html>
	</xsl:template>
	<!-- -->
	<xsl:template match="typeHierarchy">
		<xsl:for-each select="type">
			<div>
				<xsl:call-template name="get-link-for-type">
					<xsl:with-param name="type" select="@id" />
					<xsl:with-param name="link-text" select="substring-after(@id, ':' )" />
				</xsl:call-template>
				<xsl:apply-templates mode="hierarchy" />
			</div>
			<xsl:if test="interfaces">
				<h4 class="dth4">Interfaces</h4>
				<xsl:apply-templates select="./interfaces/interface" mode="hierarchy" />
			</xsl:if>
		</xsl:for-each>
	</xsl:template>
	<!-- -->
	<xsl:template match="type" mode="hierarchy">
		<div class="Hierarchy">
			<xsl:call-template name="get-link-for-type">
				<xsl:with-param name="type" select="@id" />
				<xsl:with-param name="link-text" select="substring-after(@id, ':' )" />
			</xsl:call-template>
			<xsl:if test="interfaces">
				<xsl:text>&#160;---- </xsl:text>
				<xsl:apply-templates select="./interfaces/interface" mode="baseInterfaces" />
			</xsl:if>
			<xsl:apply-templates select="type" mode="hierarchy" />
		</div>
	</xsl:template>
	<!-- -->
	<xsl:template match="interface" mode="baseInterfaces">
		<xsl:call-template name="get-link-for-type">
			<xsl:with-param name="type" select="@id" />
			<xsl:with-param name="link-text" select="substring-after(@id, ':' )" />
		</xsl:call-template>
		<xsl:if test="position() != last()">
			<xsl:text>, </xsl:text>
		</xsl:if>
	</xsl:template>
	<!-- -->
</xsl:stylesheet>
