<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<!-- -->
	<xsl:output method="html" indent="yes" />
	<!-- -->
	<xsl:include href="common.xslt" />
	<!-- -->
	<xsl:param name='member-id' />
	<!-- -->
	<xsl:template match="/">
		<xsl:apply-templates select="ndoc/assembly/module/namespace/*/*[@id=$member-id]" />
	</xsl:template>
	<!-- -->
	<xsl:template match="method | constructor | operator">
		<xsl:variable name="type">
			<xsl:choose>
				<xsl:when test="local-name(..)='interface'">Interface</xsl:when>
				<xsl:otherwise>Class</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="childType">
			<xsl:choose>
				<xsl:when test="local-name()='method'">Method</xsl:when>
				<xsl:otherwise>Constructor</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="memberName" select="@name" />
		<html dir="LTR">
			<xsl:call-template name="html-head">
				<xsl:with-param name="title">
					<xsl:value-of select="../@name" />
					<xsl:if test="local-name()='method'">
						<xsl:text>.</xsl:text>
						<xsl:value-of select="@name" />
					</xsl:if>
					<xsl:text>&#160;</xsl:text>
					<xsl:value-of select="$childType" />
					<xsl:text>&#160;</xsl:text>
					<xsl:if test="count(parent::node()/*[@name=$memberName]) &gt; 1">
						<xsl:call-template name="get-param-list" />
					</xsl:if>
				</xsl:with-param>
			</xsl:call-template>
			<body>
				<xsl:call-template name="title-row">
					<xsl:with-param name="type-name">
						<xsl:value-of select="../@name" />
						<xsl:if test="local-name()='method'">
							<xsl:text>.</xsl:text>
							<xsl:value-of select="@name" />
						</xsl:if>
						<xsl:text>&#160;</xsl:text>
						<xsl:value-of select="$childType" />
						<xsl:text>&#160;</xsl:text>
						<xsl:if test="count(parent::node()/*[@name=$memberName]) &gt; 1">
							<xsl:call-template name="get-param-list" />
						</xsl:if>
					</xsl:with-param>
				</xsl:call-template>
				<div id="content">
					<xsl:call-template name="summary-section" />
					<xsl:call-template name="member-syntax" />
					<xsl:call-template name="parameter-section" />
					<xsl:call-template name="returnvalue-section" />
					<xsl:call-template name="remarks-section" />
					<xsl:call-template name="exceptions-section" />
					<xsl:call-template name="example-section" />
					<xsl:call-template name="requirements-section" />
					<xsl:call-template name="seealso-section">
						<xsl:with-param name="page">member</xsl:with-param>
					</xsl:call-template>
					<xsl:if test="local-name()='method'">
						<object type="application/x-oleobject" classid="clsid:1e2a7bd0-dab9-11d0-b93a-00c04fc99f9e" viewastext="viewastext">
							<xsl:element name="param">
								<xsl:attribute name="name">Keyword</xsl:attribute>
								<xsl:attribute name="value"><xsl:value-of select='@name' /> method</xsl:attribute>
							</xsl:element>
							<xsl:element name="param">
								<xsl:attribute name="name">Keyword</xsl:attribute>
								<xsl:attribute name="value"><xsl:value-of select='@name' /> method, <xsl:value-of select='../@name' /> class</xsl:attribute>
							</xsl:element>
							<xsl:element name="param">
								<xsl:attribute name="name">Keyword</xsl:attribute>
								<xsl:attribute name="value"><xsl:value-of select='../@name' />.<xsl:value-of select='@name' /> method</xsl:attribute>
							</xsl:element>
						</object>
					</xsl:if>
				</div>
			</body>
		</html>
	</xsl:template>
	<!-- -->
</xsl:stylesheet>
