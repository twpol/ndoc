<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<!-- -->
	<xsl:output method="html" indent="yes" />
	<!-- -->
	<xsl:include href="common.xslt" />
	<!-- -->
	<xsl:param name='namespace' />
	<!-- -->
	<xsl:template match="/">
		<xsl:apply-templates select="ndoc" />
	</xsl:template>
	<!-- -->
	<xsl:template match="ndoc">
		<html dir="LTR">
			<xsl:call-template name="html-head">
				<xsl:with-param name="title" select="$namespace" />
			</xsl:call-template>
			<body>
				<xsl:call-template name="title-row">
					<xsl:with-param name="type-name">
						<xsl:value-of select="$namespace" />
						<xsl:text> Namespace</xsl:text>
					</xsl:with-param>
				</xsl:call-template>
				<div id="content">
					<p class="i1">
						<a>
							<xsl:attribute name="href">
								<xsl:call-template name="get-filename-for-current-namespace-hierarchy" />
							</xsl:attribute>
							<xsl:text>Namespace hierarchy</xsl:text>
						</a>
					</p>
					<br />
					<!-- the namespace template just gets the summary. -->
					<xsl:apply-templates select="assembly/module/namespace[@name=$namespace][1]" />
					<xsl:if test="assembly/module/namespace[@name=$namespace]/class">
						<h3>Classes</h3>
						<div class="table">
							<table cellspacing="0">
								<tr valign="top">
									<th width="50%">Class</th>
									<th width="50%">Description</th>
								</tr>
								<xsl:apply-templates select="assembly/module/namespace[@name=$namespace]/class">
									<xsl:sort select="@name" />
								</xsl:apply-templates>
							</table>
						</div>
					</xsl:if>
					<xsl:if test="assembly/module/namespace[@name=$namespace]/interface">
						<h3>Interfaces</h3>
						<div class="table">
							<table cellspacing="0">
								<tr valign="top">
									<th width="50%">Interface</th>
									<th width="50%">Description</th>
								</tr>
								<xsl:apply-templates select="assembly/module/namespace[@name=$namespace]/interface">
									<xsl:sort select="@name" />
								</xsl:apply-templates>
							</table>
						</div>
					</xsl:if>
					<xsl:if test="assembly/module/namespace[@name=$namespace]/structure">
						<h3>Structures</h3>
						<div class="table">
							<table cellspacing="0">
								<tr valign="top">
									<th width="50%">Structure</th>
									<th width="50%">Description</th>
								</tr>
								<xsl:apply-templates select="assembly/module/namespace[@name=$namespace]/structure">
									<xsl:sort select="@name" />
								</xsl:apply-templates>
							</table>
						</div>
					</xsl:if>
					<xsl:if test="assembly/module/namespace[@name=$namespace]/delegate">
						<h3>Delegates</h3>
						<div class="table">
							<table cellspacing="0">
								<tr valign="top">
									<th width="50%">Delegate</th>
									<th width="50%">Description</th>
								</tr>
								<xsl:apply-templates select="assembly/module/namespace[@name=$namespace]/delegate">
									<xsl:sort select="@name" />
								</xsl:apply-templates>
							</table>
						</div>
					</xsl:if>
					<xsl:if test="assembly/module/namespace[@name=$namespace]/enumeration">
						<h3>Enumerations</h3>
						<div class="table">
							<table cellspacing="0">
								<tr valign="top">
									<th width="50%">Enumeration</th>
									<th width="50%">Description</th>
								</tr>
								<xsl:apply-templates select="assembly/module/namespace[@name=$namespace]/enumeration">
									<xsl:sort select="@name" />
								</xsl:apply-templates>
							</table>
						</div>
					</xsl:if>
				</div>
				<xsl:call-template name="footer-row" />
			</body>
		</html>
	</xsl:template>
	<!-- -->
	<xsl:template match="namespace">
		<xsl:call-template name="summary-section" />
	</xsl:template>
	<!-- -->
	<xsl:template match="class">
		<tr valign="top">
			<td width="50%">
				<a>
					<xsl:attribute name="href">
						<xsl:call-template name="get-filename-for-type">
							<xsl:with-param name="id" select="@id" />
						</xsl:call-template>
					</xsl:attribute>
					<xsl:value-of select="@name" />
				</a>
			</td>
			<td width="50%">
				<xsl:apply-templates select="documentation/summary/node()" mode="slashdoc" />
			</td>
		</tr>
	</xsl:template>
	<!-- -->
	<xsl:template match="interface">
		<tr valign="top">
			<td width="50%">
				<a>
					<xsl:attribute name="href">
						<xsl:call-template name="get-filename-for-type">
							<xsl:with-param name="id" select="@id" />
						</xsl:call-template>
					</xsl:attribute>
					<xsl:value-of select="@name" />
				</a>
			</td>
			<td width="50%">
				<xsl:apply-templates select="documentation/summary/node()" mode="slashdoc" />
			</td>
		</tr>
	</xsl:template>
	<!-- -->
	<xsl:template match="structure">
		<tr valign="top">
			<td width="50%">
				<a>
					<xsl:attribute name="href">
						<xsl:call-template name="get-filename-for-type">
							<xsl:with-param name="id" select="@id" />
						</xsl:call-template>
					</xsl:attribute>
					<xsl:value-of select="@name" />
				</a>
			</td>
			<td width="50%">
				<xsl:apply-templates select="documentation/summary/node()" mode="slashdoc" />
			</td>
		</tr>
	</xsl:template>
	<!-- -->
	<xsl:template match="delegate">
		<tr valign="top">
			<td width="50%">
				<a>
					<xsl:attribute name="href">
						<xsl:call-template name="get-filename-for-type">
							<xsl:with-param name="id" select="@id" />
						</xsl:call-template>
					</xsl:attribute>
					<xsl:value-of select="@name" />
				</a>
			</td>
			<td width="50%">
				<xsl:apply-templates select="documentation/summary/node()" mode="slashdoc" />
			</td>
		</tr>
	</xsl:template>
	<!-- -->
	<xsl:template match="enumeration">
		<tr valign="top">
			<td width="50%">
				<a>
					<xsl:attribute name="href">
						<xsl:call-template name="get-filename-for-type">
							<xsl:with-param name="id" select="@id" />
						</xsl:call-template>
					</xsl:attribute>
					<xsl:value-of select="@name" />
				</a>
			</td>
			<td width="50%">
				<xsl:apply-templates select="documentation/summary/node()" mode="slashdoc" />
			</td>
		</tr>
	</xsl:template>
	<!-- -->
</xsl:stylesheet>
