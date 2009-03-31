<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ndoc="urn:ndoc-schema">
	<!-- -->
	<xsl:output method="xml" indent="yes"  encoding="utf-8" omit-xml-declaration="yes"/>
	<!-- -->
	<xsl:include href="common.xslt" />
	<!-- -->
	<xsl:param name='assembly-name' />
	<!-- -->
	<xsl:template match="/">
		<xsl:apply-templates select="ndoc:ndoc/ndoc:assembly[@name=$assembly-name]" />
	</xsl:template>
	<!-- -->
	<xsl:template match="ndoc:assembly">
		<html dir="LTR">
			<xsl:call-template name="html-head">
				<xsl:with-param name="title" select="$assembly-name" />
			</xsl:call-template>
			<body id="bodyID" class="dtBODY">
				<xsl:call-template name="title-row">
					<xsl:with-param name="type-name">
						<xsl:value-of select="@name" />
						<xsl:text> Assembly</xsl:text>
					</xsl:with-param>
				</xsl:call-template>
				<div id="nstext">
					<xsl:variable name="thisAssembly" select="@name" />
					<xsl:call-template name="summary-section" />
					<xsl:if test="ndoc:assemblyReference">
						<h3 class="dtH3">Referenced Assemblies</h3>
						<div class="tablediv">
							<table class="dtTABLE" cellspacing="0">
								<tr valign="top">
									<th width="50%">Assembly Reference</th>
									<th width="50%">Description</th>
								</tr>
								<xsl:apply-templates select="ndoc:assemblyReference[not(@name=preceding-sibling::assemblyReference/@name)]">
									<xsl:sort select="@name" />
								</xsl:apply-templates>
							</table>
						</div>
					</xsl:if>
					<xsl:if test="../ndoc:assembly[ndoc:assemblyReference[@name=$thisAssembly]]">
						<h3 class="dtH3">Dependent Assemblies</h3>
						<div class="tablediv">
							<table class="dtTABLE" cellspacing="0">
								<tr valign="top">
									<th width="50%">Assembly</th>
									<th width="50%">Description</th>
								</tr>
								<xsl:apply-templates select="../ndoc:assembly[ndoc:assemblyReference[@name=$thisAssembly]]" mode="dependencyList">
									<xsl:sort select="@name" />
								</xsl:apply-templates>
							</table>
						</div>
					</xsl:if>
					<xsl:if test="ndoc:module/ndoc:namespace">
						<h3 class="dtH3">Namespaces</h3>
						<div class="tablediv">
							<table class="dtTABLE" cellspacing="0">
								<tr valign="top">
									<th width="50%">Namespace</th>
									<th width="50%">Description</th>
								</tr>
								<xsl:apply-templates select="ndoc:module/ndoc:namespace">
									<xsl:sort select="@name" />
								</xsl:apply-templates>
							</table>
						</div>
					</xsl:if>
					<xsl:if test="ndoc:module/ndoc:namespace/ndoc:class">
						<h3 class="dtH3">Classes</h3>
						<div class="tablediv">
							<table class="dtTABLE" cellspacing="0">
								<tr valign="top">
									<th width="50%">Class</th>
									<th width="50%">Description</th>
								</tr>
								<xsl:apply-templates select="ndoc:module/ndoc:namespace/ndoc:class">
									<xsl:sort select="@displayName" />
								</xsl:apply-templates>
							</table>
						</div>
					</xsl:if>
					<xsl:if test="ndoc:module/ndoc:namespace/ndoc:interface">
						<h3 class="dtH3">Interfaces</h3>
						<div class="tablediv">
							<table class="dtTABLE" cellspacing="0">
								<tr valign="top">
									<th width="50%">Interface</th>
									<th width="50%">Description</th>
								</tr>
								<xsl:apply-templates select="ndoc:module/ndoc:namespace/ndoc:interface">
									<xsl:sort select="@name" />
								</xsl:apply-templates>
							</table>
						</div>
					</xsl:if>
					<xsl:if test="ndoc:module/ndoc:namespace/ndoc:structure">
						<h3 class="dtH3">Structures</h3>
						<div class="tablediv">
							<table class="dtTABLE" cellspacing="0">
								<tr valign="top">
									<th width="50%">Structure</th>
									<th width="50%">Description</th>
								</tr>
								<xsl:apply-templates select="ndoc:module/ndoc:namespace/ndoc:structure">
									<xsl:sort select="@name" />
								</xsl:apply-templates>
							</table>
						</div>
					</xsl:if>
					<xsl:if test="ndoc:module/ndoc:namespace/ndoc:delegate">
						<h3 class="dtH3">Delegates</h3>
						<div class="tablediv">
							<table class="dtTABLE" cellspacing="0">
								<tr valign="top">
									<th width="50%">Delegate</th>
									<th width="50%">Description</th>
								</tr>
								<xsl:apply-templates select="ndoc:module/ndoc:namespace/ndoc:delegate">
									<xsl:sort select="@name" />
								</xsl:apply-templates>
							</table>
						</div>
					</xsl:if>
					<xsl:if test="ndoc:module/ndoc:namespace/ndoc:enumeration">
						<h3 class="dtH3">Enumerations</h3>
						<div class="tablediv">
							<table class="dtTABLE" cellspacing="0">
								<tr valign="top">
									<th width="50%">Enumeration</th>
									<th width="50%">Description</th>
								</tr>
								<xsl:apply-templates select="ndoc:module/ndoc:namespace/ndoc:enumeration">
									<xsl:sort select="@name" />
								</xsl:apply-templates>
							</table>
						</div>
					</xsl:if>
					<xsl:call-template name="footer-row">
						<xsl:with-param name="type-name">
							<xsl:value-of select="@name" />
							<xsl:text> Assembly</xsl:text>
						</xsl:with-param>
					</xsl:call-template>
				</div>
			</body>
		</html>
	</xsl:template>
	<xsl:template match="ndoc:assembly" mode="dependencyList">
		<xsl:variable name="assemblyName" select="@name" />
		<xsl:variable name="assemblyNode" select="." />
		<tr valign="top">
			<td width="50%">
				<a>
					<xsl:attribute name="href">
						<xsl:call-template name="get-filename-for-assembly">
							<xsl:with-param name="assemblyName" select="$assemblyName" />
						</xsl:call-template>
					</xsl:attribute>
					<xsl:value-of select="$assemblyName" />
				</a>
			</td>
			<td width="50%">
				<xsl:call-template name="obsolete-inline"/>
				<xsl:apply-templates select="($assemblyNode/ndoc:documentation/ndoc:summary)[1]/node()" mode="slashdoc" />
				<xsl:if test="not(($assemblyNode/ndoc:documentation/ndoc:summary)[1]/node())">&#160;</xsl:if>
			</td>
		</tr>
	</xsl:template>
	<!-- -->
	<xsl:template match="ndoc:assemblyReference">
			<xsl:variable name="assemblyReferenceName" select="@name" />
			<xsl:variable name="assemblyNode" select="../../ndoc:assembly[@name=$assemblyReferenceName]" />
		<tr valign="top">
			<td width="50%">
				<xsl:choose>
					<xsl:when test="$assemblyNode">
						<a>
							<xsl:attribute name="href">
								<xsl:call-template name="get-filename-for-assembly">
									<xsl:with-param name="assemblyName" select="$assemblyReferenceName" />
								</xsl:call-template>
							</xsl:attribute>
							<xsl:value-of select="$assemblyReferenceName" />
						</a>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="$assemblyReferenceName" />
					</xsl:otherwise>
				</xsl:choose>
			</td>
			<td width="50%">
				<xsl:call-template name="obsolete-inline"/>
				<xsl:apply-templates select="($assemblyNode/ndoc:documentation/ndoc:summary)[1]/node()" mode="slashdoc" />
				<xsl:if test="not(($assemblyNode/ndoc:documentation/ndoc:summary)[1]/node())">&#160;</xsl:if>
			</td>
		</tr>
	</xsl:template>
	<!-- -->
	<xsl:template match="ndoc:namespace">
		<tr valign="top">
			<td width="50%">
				<a>
					<xsl:attribute name="href">
						<xsl:call-template name="get-filename-for-namespace">
							<xsl:with-param name="assemblyName" select="ancestor::ndoc:assembly/@name" />
							<xsl:with-param name="namespace" select="@name" />
						</xsl:call-template>
					</xsl:attribute>
					<xsl:value-of select="@name" />
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
