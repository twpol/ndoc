<?xml version="1.0" encoding="utf-8" ?>
<xsl:transform version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<!-- -->
	<xsl:include href="javadoc.xslt" />
	<!-- -->
	<xsl:param name="global-path-to-root" />
	<xsl:param name="global-type-id" />
	<!-- -->
	<xsl:template match="/">
		<xsl:variable name="type" select="ndoc/assembly/module/namespace/*[@id=$global-type-id]" />
		<html>
			<xsl:call-template name="output-head" />
			<body>
				<xsl:call-template name="output-navigation-bar">
					<xsl:with-param name="select" select="'Type'" />
					<xsl:with-param name="link-namespace" select="true()" />
					<xsl:with-param name="prev-next-what" select="'TYPE'" />
					<xsl:with-param name="type-node" select="$type" />
				</xsl:call-template>
				<hr />
				<h2>
					<span class="namespaceName">
						<xsl:value-of select="$type/parent::namespace/@name" />
					</span>
					<br />
					<span class="className">
						<xsl:choose>
							<xsl:when test="local-name($type)='interface'">
								<xsl:text>Interface </xsl:text>
							</xsl:when>
							<xsl:when test="local-name($type)='class'">
								<xsl:text>Class </xsl:text>
							</xsl:when>
							<xsl:when test="local-name($type)='structure'">
								<xsl:text>Structure </xsl:text>
							</xsl:when>
						</xsl:choose>
						<xsl:value-of select="$type/@name" />
					</span>
				</h2>
				<xsl:if test="$type/documentation/summary">
					<p>
						<xsl:apply-templates select="$type/documentation/summary" mode="doc" />
					</p>
				</xsl:if>
				<xsl:if test="$type/documentation/remarks">
					<p>
						<xsl:apply-templates select="$type/documentation/remarks" mode="doc" />
					</p>
				</xsl:if>
				<xsl:variable name="constructors" select="$type/constructor" />
				<xsl:if test="$constructors">
					<a name="constructor-summary" />
					<!-- IE doesn't support the border-spacing CSS property so we have to set the cellspacing attribute here. -->
					<table class="table" cellspacing="0">
						<thead>
							<tr>
								<th colspan="2">Constructor Summary</th>
							</tr>
						</thead>
						<xsl:apply-templates select="$constructors" />
					</table>
					<br />
				</xsl:if>
				<xsl:variable name="fields" select="$type/field[not(@declaringType)]" />
				<xsl:if test="$fields">
					<a name="field-summary" />
					<!-- IE doesn't support the border-spacing CSS property so we have to set the cellspacing attribute here. -->
					<table class="table" cellspacing="0">
						<thead>
							<tr>
								<th colspan="2">Field Summary</th>
							</tr>
						</thead>
						<xsl:apply-templates select="$fields">
							<xsl:sort select="@name" />
						</xsl:apply-templates>
					</table>
					<br />
				</xsl:if>
				<xsl:variable name="properties" select="$type/property[not(@declaringType)]" />
				<xsl:if test="$properties">
					<a name="property-summary" />
					<!-- IE doesn't support the border-spacing CSS property so we have to set the cellspacing attribute here. -->
					<table class="table" cellspacing="0">
						<thead>
							<tr>
								<th colspan="2">Property Summary</th>
							</tr>
						</thead>
						<xsl:apply-templates select="$properties">
							<xsl:sort select="@name" />
						</xsl:apply-templates>
					</table>
					<br />
				</xsl:if>
				<xsl:variable name="methods" select="$type/method[not(@declaringType)]" />
				<xsl:if test="$methods">
					<a name="method-summary" />
					<!-- IE doesn't support the border-spacing CSS property so we have to set the cellspacing attribute here. -->
					<table class="table" cellspacing="0">
						<thead>
							<tr>
								<th colspan="2">Method Summary</th>
							</tr>
						</thead>
						<xsl:apply-templates select="$methods">
							<xsl:sort select="@name" />
						</xsl:apply-templates>
					</table>
					<br />
				</xsl:if>
				<xsl:for-each select="$type/descendant::base">
					<xsl:call-template name="inherited-members">
						<xsl:with-param name="type" select="$type" />
						<xsl:with-param name="base-id" select="substring-after(@id, 'T:')" />
					</xsl:call-template>
				</xsl:for-each>
				<xsl:call-template name="inherited-members">
					<xsl:with-param name="type" select="$type" />
					<xsl:with-param name="base-id" select="'System.Object'" />
				</xsl:call-template>
				<xsl:variable name="events" select="$type/event[not(@declaringType)]" />
				<xsl:if test="$events">
					<a name="event-summary" />
					<!-- IE doesn't support the border-spacing CSS property so we have to set the cellspacing attribute here. -->
					<table class="table" cellspacing="0">
						<thead>
							<tr>
								<th colspan="2">Event Summary</th>
							</tr>
						</thead>
						<xsl:apply-templates select="$events">
							<xsl:sort select="@name" />
						</xsl:apply-templates>
					</table>
					<br />
				</xsl:if>
				<hr />
				<xsl:call-template name="output-navigation-bar">
					<xsl:with-param name="select" select="'Type'" />
					<xsl:with-param name="link-namespace" select="true()" />
					<xsl:with-param name="prev-next-what" select="'TYPE'" />
					<xsl:with-param name="type-node" select="$type" />
				</xsl:call-template>
			</body>
		</html>
	</xsl:template>
	<!-- -->
	<xsl:template match="constructor">
		<tr>
			<!-- Is there a CSS property that can emulate the valign attribute? -->
			<td class="constructor" valign="top">
				<a>
					<xsl:attribute name="href">
						<xsl:call-template name="get-href-to-constructor">
							<xsl:with-param name="constructor-node" select="." />
						</xsl:call-template>
					</xsl:attribute>
					<xsl:value-of select="../@name" />
				</a>
				<xsl:text>(</xsl:text>
				<xsl:for-each select="parameter">
					<xsl:value-of select="@type" />
					<xsl:text>&#160;</xsl:text>
					<xsl:value-of select="@name" />
					<xsl:if test="position()!=last()">
						<xsl:text>,&#160;</xsl:text>
					</xsl:if>
				</xsl:for-each>
				<xsl:text>)</xsl:text>
				<xsl:if test="documentation/summary">
					<br />
					<xsl:text>&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;</xsl:text>
					<xsl:apply-templates select="documentation/summary" mode="doc" />
				</xsl:if>
			</td>
		</tr>
	</xsl:template>
	<!-- -->
	<xsl:template match="field">
		<tr>
			<!-- Is there a CSS property that can emulate the valign attribute? -->
			<td class="fieldType" valign="top">
				<xsl:value-of select="@type" />
			</td>
			<td class="field">
				<a>
					<xsl:attribute name="href">
						<xsl:call-template name="get-href-to-field">
							<xsl:with-param name="field-node" select="." />
						</xsl:call-template>
					</xsl:attribute>
					<xsl:value-of select="@name" />
				</a>
				<xsl:if test="documentation/summary">
					<br />
					<xsl:text>&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;</xsl:text>
					<xsl:apply-templates select="documentation/summary" mode="doc" />
				</xsl:if>
			</td>
		</tr>
	</xsl:template>
	<!-- -->
	<xsl:template match="property">
		<tr>
			<!-- Is there a CSS property that can emulate the valign attribute? -->
			<td class="propertyType" valign="top">
				<xsl:value-of select="@type" />
			</td>
			<td class="property">
				<a>
					<xsl:attribute name="href">
						<xsl:call-template name="get-href-to-property">
							<xsl:with-param name="property-node" select="." />
						</xsl:call-template>
					</xsl:attribute>
					<xsl:value-of select="@name" />
				</a>
				<xsl:if test="parameter">
					<xsl:text>[</xsl:text>
					<xsl:for-each select="parameter">
						<xsl:value-of select="@type" />
						<xsl:text>&#160;</xsl:text>
						<xsl:value-of select="@name" />
						<xsl:if test="position()!=last()">
							<xsl:text>,&#160;</xsl:text>
						</xsl:if>
					</xsl:for-each>
					<xsl:text>]</xsl:text>
				</xsl:if>
				<xsl:if test="documentation/summary">
					<br />
					<xsl:text>&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;</xsl:text>
					<xsl:apply-templates select="documentation/summary" mode="doc" />
				</xsl:if>
			</td>
		</tr>
	</xsl:template>
	<!-- -->
	<xsl:template match="method">
		<tr>
			<!-- Is there a CSS property that can emulate the valign attribute? -->
			<td class="returnType" valign="top">
				<xsl:value-of select="@returnType" />
			</td>
			<td class="method">
				<a>
					<xsl:attribute name="href">
						<xsl:call-template name="get-href-to-method">
							<xsl:with-param name="method-node" select="." />
						</xsl:call-template>
					</xsl:attribute>
					<xsl:value-of select="@name" />
				</a>
				<xsl:text>(</xsl:text>
				<xsl:for-each select="parameter">
					<xsl:value-of select="@type" />
					<xsl:text>&#160;</xsl:text>
					<xsl:value-of select="@name" />
					<xsl:if test="position()!=last()">
						<xsl:text>,&#160;</xsl:text>
					</xsl:if>
				</xsl:for-each>
				<xsl:text>)</xsl:text>
				<xsl:if test="documentation/summary">
					<br />
					<xsl:text>&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;</xsl:text>
					<xsl:apply-templates select="documentation/summary" mode="doc" />
				</xsl:if>
			</td>
		</tr>
	</xsl:template>
	<!-- -->
	<xsl:template match="event">
		<tr>
			<!-- Is there a CSS property that can emulate the valign attribute? -->
			<td class="eventType" valign="top">
				<xsl:value-of select="@type" />
			</td>
			<td class="event">
				<a>
					<xsl:attribute name="href">
						<xsl:call-template name="get-href-to-event">
							<xsl:with-param name="event-node" select="." />
						</xsl:call-template>
					</xsl:attribute>
					<xsl:value-of select="@name" />
				</a>
				<xsl:if test="documentation/summary">
					<br />
					<xsl:text>&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;</xsl:text>
					<xsl:apply-templates select="documentation/summary" mode="doc" />
				</xsl:if>
			</td>
		</tr>
	</xsl:template>
	<!-- -->
	<xsl:template name="inherited-members">
		<xsl:param name="type" />
		<xsl:param name="base-id" />
		<xsl:variable name="inherited-methods" select="$type/method[@declaringType=$base-id]" />
		<xsl:if test="$inherited-methods">
			<!-- IE doesn't support the border-spacing CSS property so we have to set the cellspacing attribute here. -->
			<table class="subtable" cellspacing="0">
				<thead>
					<tr>
						<th>
							<xsl:text>Methods inherited from class </xsl:text>
							<xsl:value-of select="$base-id" />
						</th>
					</tr>
				</thead>
				<tr>
					<td>
						<xsl:variable name="methods-count" select="count($inherited-methods)" />
						<xsl:for-each select="$inherited-methods">
							<xsl:sort select="@name" />
							<a>
								<xsl:attribute name="href">
									<xsl:call-template name="get-href-to-cref">
										<xsl:with-param name="cref" select="@id" />
									</xsl:call-template>
								</xsl:attribute>
								<xsl:value-of select="@name" />
							</a>
							<xsl:if test="position() &lt; $methods-count">, </xsl:if>
						</xsl:for-each>
					</td>
				</tr>
			</table>
			<br />
		</xsl:if>
	</xsl:template>
	<!-- -->
</xsl:transform>
