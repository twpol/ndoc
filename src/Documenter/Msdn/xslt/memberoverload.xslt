<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<!-- -->
	<xsl:output method="html" indent="no" />
	<!-- -->
	<xsl:include href="common.xslt" />
	<!-- -->
	<xsl:param name='member-id' />
	<!-- -->
	<xsl:template match="/">
		<xsl:apply-templates select="ndoc/assembly/module/namespace/*/*[@id=$member-id][1]" />
	</xsl:template>
	<!-- -->
	<xsl:template match="method | constructor | property">
		<xsl:variable name="type">
			<xsl:choose>
				<xsl:when test="local-name(..)='interface'">Interface</xsl:when>
				<xsl:otherwise>Class</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="childType">
			<xsl:choose>
				<xsl:when test="local-name()='method'">Method</xsl:when>
				<xsl:when test="local-name()='constructor'">Constructor</xsl:when>
				<xsl:otherwise>Property</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="memberName" select="@name" />
		<html dir="LTR">
			<xsl:call-template name="html-head">
				<xsl:with-param name="title">
					<xsl:choose>
						<xsl:when test="local-name()!='constructor'">
							<xsl:value-of select="@name" />
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="../@name" />
						</xsl:otherwise>
					</xsl:choose>
					<xsl:text>&#32;</xsl:text>
					<xsl:value-of select="$childType" />
				</xsl:with-param>
			</xsl:call-template>
			<body>
				<xsl:call-template name="title-row">
					<xsl:with-param name="type-name">
						<xsl:value-of select="../@name" />
						<xsl:if test="local-name()!='constructor'">
							<xsl:text>.</xsl:text>
							<xsl:value-of select="@name" />
						</xsl:if>
						<xsl:text>&#160;</xsl:text>
						<xsl:value-of select="childType" />
					</xsl:with-param>
				</xsl:call-template>
				<div id="content">
					<xsl:call-template name="overloads-summary-section" />
					<h4>Overload List</h4>
					<xsl:for-each select="parent::node()/*[@name=$memberName]">
						<p class="i1">
							<xsl:choose>
								<xsl:when test="@declaringType">
									<xsl:apply-templates select="//class[@id=concat('T:', current()/@declaringType)]/*[@name=$memberName]/documentation/summary/node()" mode="slashdoc" />
								</xsl:when>
								<xsl:otherwise>
									<xsl:apply-templates select="documentation/summary/node()" mode="slashdoc" />
								</xsl:otherwise>
							</xsl:choose>
						</p>
						<p class="i2">
							<a>
								<xsl:attribute name="href">
									<xsl:choose>
										<xsl:when test="local-name()='constructor'">
											<xsl:call-template name="get-filename-for-current-constructor" />
										</xsl:when>
										<xsl:when test="local-name()='method'">
											<xsl:call-template name="get-filename-for-method" />
										</xsl:when>
										<xsl:otherwise>
											<xsl:call-template name="get-filename-for-current-property" />
										</xsl:otherwise>
									</xsl:choose>
								</xsl:attribute>
								<xsl:apply-templates select="self::node()" mode="syntax" />
							</a>
						</p>
					</xsl:for-each>
					<xsl:call-template name="overloads-example-section" />
					<xsl:call-template name="seealso-section">
						<xsl:with-param name="page">memberoverload</xsl:with-param>
					</xsl:call-template>
				</div>
				<xsl:call-template name="footer-row" />
			</body>
		</html>
	</xsl:template>
	<!-- -->
	<xsl:template match="constructor | method" mode="syntax">
		<xsl:call-template name="member-syntax2" />
	</xsl:template>
	<!-- -->
	<xsl:template match="property" mode="syntax">
		<xsl:call-template name="cs-property-syntax">
			<xsl:with-param name="indent" select="false()" />
			<xsl:with-param name="display-names" select="false()" />
			<xsl:with-param name="link-types" select="false()" />
		</xsl:call-template>
	</xsl:template>
	<!-- -->
</xsl:stylesheet>
