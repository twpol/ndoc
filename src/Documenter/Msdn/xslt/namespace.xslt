<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ndoc="urn:ndoc-schema">
	<!-- -->
	<xsl:output method="xml" indent="yes"  encoding="utf-8" omit-xml-declaration="yes"/>
	<!-- -->
	<xsl:include href="common.xslt" />
	<!-- -->
	<xsl:param name='assembly-name' />
	<xsl:param name='namespace' />
	<!-- -->
	<xsl:template match="/">
		<xsl:apply-templates select="ndoc:ndoc" />
	</xsl:template>
	<!-- -->
	<xsl:template match="ndoc:ndoc">
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
					<!-- the namespace template just gets the summary. -->
					<xsl:apply-templates select="(ndoc:assembly/ndoc:module/ndoc:namespace[(@name=$namespace) and ndoc:documentation])[1]" />
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
					<xsl:if test="ndoc:assembly/ndoc:module/ndoc:namespace[@name=$namespace]/ndoc:class">
						<h3 class="dtH3">Classes</h3>
						<div class="tablediv">
							<table class="dtTABLE" cellspacing="0">
								<tr valign="top">
									<th width="50%">Class</th>
									<th width="50%">Description</th>
								</tr>
								<xsl:apply-templates select="ndoc:assembly/ndoc:module/ndoc:namespace[@name=$namespace]/ndoc:class">
									<xsl:sort select="@displayName" />
								</xsl:apply-templates>
							</table>
						</div>
					</xsl:if>
					<xsl:if test="ndoc:assembly/ndoc:module/ndoc:namespace[@name=$namespace]/ndoc:interface">
						<h3 class="dtH3">Interfaces</h3>
						<div class="tablediv">
							<table class="dtTABLE" cellspacing="0">
								<tr valign="top">
									<th width="50%">Interface</th>
									<th width="50%">Description</th>
								</tr>
								<xsl:apply-templates select="ndoc:assembly/ndoc:module/ndoc:namespace[@name=$namespace]/ndoc:interface">
									<xsl:sort select="@name" />
								</xsl:apply-templates>
							</table>
						</div>
					</xsl:if>
					<xsl:if test="ndoc:assembly/ndoc:module/ndoc:namespace[@name=$namespace]/ndoc:structure">
						<h3 class="dtH3">Structures</h3>
						<div class="tablediv">
							<table class="dtTABLE" cellspacing="0">
								<tr valign="top">
									<th width="50%">Structure</th>
									<th width="50%">Description</th>
								</tr>
								<xsl:apply-templates select="ndoc:assembly/ndoc:module/ndoc:namespace[@name=$namespace]/ndoc:structure">
									<xsl:sort select="@name" />
								</xsl:apply-templates>
							</table>
						</div>
					</xsl:if>
					<xsl:if test="ndoc:assembly/ndoc:module/ndoc:namespace[@name=$namespace]/ndoc:delegate">
						<h3 class="dtH3">Delegates</h3>
						<div class="tablediv">
							<table class="dtTABLE" cellspacing="0">
								<tr valign="top">
									<th width="50%">Delegate</th>
									<th width="50%">Description</th>
								</tr>
								<xsl:apply-templates select="ndoc:assembly/ndoc:module/ndoc:namespace[@name=$namespace]/ndoc:delegate">
									<xsl:sort select="@name" />
								</xsl:apply-templates>
							</table>
						</div>
					</xsl:if>
					<xsl:if test="ndoc:assembly/ndoc:module/ndoc:namespace[@name=$namespace]/ndoc:enumeration">
						<h3 class="dtH3">Enumerations</h3>
						<div class="tablediv">
							<table class="dtTABLE" cellspacing="0">
								<tr valign="top">
									<th width="50%">Enumeration</th>
									<th width="50%">Description</th>
								</tr>
								<xsl:apply-templates select="ndoc:assembly/ndoc:module/ndoc:namespace[@name=$namespace]/ndoc:enumeration">
									<xsl:sort select="@name" />
								</xsl:apply-templates>
							</table>
						</div>
					</xsl:if>
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
		<xsl:call-template name="summary-section" />
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
