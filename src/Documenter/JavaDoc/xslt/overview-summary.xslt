<?xml version="1.0" encoding="utf-8" ?>
<xsl:transform version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<!-- -->
	<xsl:include href="javadoc.xslt" />
	<!-- -->
	<xsl:param name="global-path-to-root" />
	<xsl:param name="global-title" select="'My Project'" />
	<!-- -->
	<xsl:template match="/">
		<html>
			<xsl:call-template name="output-head" />
			<body>
				<xsl:call-template name="output-navigation-bar">
					<xsl:with-param name="select" select="'Overview'" />
				</xsl:call-template>
				<hr />
				<h2 class="title">
					<xsl:value-of select="$global-title" />
					<xsl:text> API Specification</xsl:text>
				</h2>
				<!-- IE doesn't support the border-spacing CSS property so we have to set the cellspacing attribute here. -->
				<table class="table" cellspacing="0">
					<thead>
						<tr>
							<th colspan="2">
								<xsl:value-of select="$global-title" />
								<xsl:text> Namespaces</xsl:text>
							</th>
						</tr>
					</thead>
					<xsl:apply-templates select="ndoc/assembly/module/namespace"></xsl:apply-templates>
				</table>
				<br />
				<hr />
				<xsl:call-template name="output-navigation-bar">
					<xsl:with-param name="select" select="'Overview'" />
				</xsl:call-template>
			</body>
		</html>
	</xsl:template>
	<!-- -->
	<xsl:template match="namespace">
		<xsl:if test="count(*)">
			<tr>
				<td class="name">
					<a>
						<xsl:attribute name="href">
							<xsl:call-template name="get-href-to-namespace-summary">
								<xsl:with-param name="namespace-name" select="@name" />
							</xsl:call-template>
						</xsl:attribute>
						<xsl:value-of select="@name" />
					</a>
				</td>
				<td class="namespace">
					<xsl:choose>
						<xsl:when test="documentation/summary">
							<xsl:apply-templates select="documentation/summary" mode="doc" />
						</xsl:when>
						<xsl:otherwise>&#160;</xsl:otherwise>
					</xsl:choose>
				</td>
			</tr>
		</xsl:if>
	</xsl:template>
	<!-- -->
</xsl:transform>
