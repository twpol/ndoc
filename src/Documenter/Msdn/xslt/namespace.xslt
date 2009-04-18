<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" 
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
				xmlns:ndoc="urn:ndoc-schema"
				xmlns:NDocUtil="urn:NDocUtil"
				xmlns:msxsl="urn:schemas-microsoft-com:xslt"
				exclude-result-prefixes="xsl ndoc NDocUtil msxsl"
>
	<!-- -->
	<xsl:output method="xml" indent="yes"  encoding="utf-8" omit-xml-declaration="yes"/>
	<!-- -->
	<xsl:include href="common.xslt" />
	<!-- -->
	<xsl:param name='merge-assemblies' />
	<xsl:param name='assembly-name' />
	<xsl:param name='namespace' />
	<!-- -->
	<xsl:template match="/">
		<xsl:apply-templates select="ndoc:ndoc" />
	</xsl:template>
	<!-- -->
	<xsl:template match="ndoc:ndoc">
		<xsl:variable name="condition">
			<xsl:choose>
				<xsl:when test="$merge-assemblies">
					<xsl:value-of select="'ndoc:assembly/ndoc:module/ndoc:namespace[@name=$namespace]'"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="'ndoc:assembly[@name=$assembly-name]/ndoc:module/ndoc:namespace[@name=$namespace]'"/>
				</xsl:otherwise>
			</xsl:choose>			
		</xsl:variable>
		
		<xsl:variable name="classes" select="evaluate($condition)/ndoc:class" />
		<xsl:variable name="interfaces" select="evaluate($condition)/ndoc:interface" />
		<xsl:variable name="structures" select="evaluate($condition)/ndoc:structures" />
		<xsl:variable name="delegates" select="evaluate($condition)/ndoc:delegates" />
		<xsl:variable name="enumerations" select="evaluate($condition)/ndoc:enumerations" />
		
		<html dir="LTR">
			<xsl:call-template name="html-head">
				<xsl:with-param name="title" select="$namespace" />
			</xsl:call-template>
			<body id="bodyID" class="dtBODY">
				<xsl:call-template name="title-row">
					<xsl:with-param name="type-name">
						<xsl:value-of select="$namespace" />
						<xsl:text> Namespace</xsl:text>
					</xsl:with-param>
				</xsl:call-template>
				<div id="nstext">
					<!-- get the first available namespace documentation -->
					<xsl:apply-templates select="(ndoc:assembly[@name=$assembly-name]/ndoc:module/ndoc:namespace[(@name=$namespace) and ndoc:documentation])[1]" />
					<p>
						<a>
							<xsl:attribute name="href">
								<xsl:call-template name="get-filename-for-namespace-hierarchy">
									<xsl:with-param name="namespace">
										<xsl:value-of select="$namespace" />
									</xsl:with-param>
								</xsl:call-template>
							</xsl:attribute>
							<xsl:text>Namespace Hierarchy</xsl:text>
						</a>
					</p>

					<xsl:call-template name="element-list">
						<xsl:with-param name="element-name-plural">Classes</xsl:with-param>
						<xsl:with-param name="element-name">Class</xsl:with-param>
						<xsl:with-param name="elements" select="$classes" />
					</xsl:call-template>

					<xsl:call-template name="element-list">
						<xsl:with-param name="element-name-plural">Interfaces</xsl:with-param>
						<xsl:with-param name="element-name">Interface</xsl:with-param>
						<xsl:with-param name="elements" select="$interfaces" />
					</xsl:call-template>

					<xsl:call-template name="element-list">
						<xsl:with-param name="element-name-plural">Structures</xsl:with-param>
						<xsl:with-param name="element-name">Structure</xsl:with-param>
						<xsl:with-param name="elements" select="$structures" />
					</xsl:call-template>

					<xsl:call-template name="element-list">
						<xsl:with-param name="element-name-plural">Delegates</xsl:with-param>
						<xsl:with-param name="element-name">Delegate</xsl:with-param>
						<xsl:with-param name="elements" select="$delegates" />
					</xsl:call-template>

					<xsl:call-template name="element-list">
						<xsl:with-param name="element-name-plural">Enumerations</xsl:with-param>
						<xsl:with-param name="element-name">Enumeration</xsl:with-param>
						<xsl:with-param name="elements" select="$enumerations" />
					</xsl:call-template>

					<xsl:call-template name="footer-row">
						<xsl:with-param name="type-name">
							<xsl:value-of select="$namespace" />
							<xsl:text> Namespace</xsl:text>
						</xsl:with-param>
					</xsl:call-template>
				</div>
			</body>
		</html>
	</xsl:template>
	
	<!-- -->
	<xsl:template match="ndoc:namespace">
		<!--  the namespace template just gets the summary. -->
		<xsl:call-template name="summary-section" />
	</xsl:template>
	
	<!-- -->
	<xsl:template name="element-list">
		<xsl:param name="elements" />
		<xsl:param name="element-name" />
		<xsl:param name="element-name-plural" />

		<xsl:if test="$elements">
			<h3 class="dtH3"><xsl:value-of select="$element-name-plural"/></h3>
			<div class="tablediv">
				<table class="dtTABLE" cellspacing="0">
					<tr valign="top">
						<th width="50%"><xsl:value-of select="$element-name"/></th>
						<th width="50%">Description</th>
					</tr>
					<xsl:apply-templates select="$elements">
						<xsl:sort select="@name" />
					</xsl:apply-templates>
				</table>
			</div>
		</xsl:if>

	</xsl:template>
	<!-- -->
	<xsl:template match="ndoc:enumeration | ndoc:delegate | ndoc:structure | ndoc:interface | ndoc:class">
		<tr valign="top">
			<td width="50%">
				<a>
					<xsl:attribute name="href">
						<xsl:call-template name="get-filename-for-type">
							<xsl:with-param name="assemblyName" select="ancestor::ndoc:assembly/@name" />
							<xsl:with-param name="id" select="@id" />
						</xsl:call-template>
					</xsl:attribute>
					<xsl:value-of select="@displayName" />
				</a>
			</td>
			<td width="50%">
				<xsl:call-template name="obsolete-inline"/>
				<xsl:apply-templates select="(ndoc:documentation/ndoc:summary)[1]/node()" mode="slashdoc" />
				<xsl:if test="not((ndoc:documentation/ndoc:summary)[1]/node())">&#160;</xsl:if>
			</td>
		</tr>
	</xsl:template>
	<!-- -->
</xsl:stylesheet>
