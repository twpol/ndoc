<?xml version="1.0" encoding="utf-8" ?>
<xsl:transform version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<!-- -->
	<xsl:template name="output-head">
		<head>
			<link rel="stylesheet" type="text/css" href="{$global-path-to-root}JavaDoc.css" />
		</head>
	</xsl:template>
	<!-- -->
	<xsl:template name="output-navigation-bar">
		<xsl:param name="select" />
		<xsl:param name="link-namespace" />
		<xsl:param name="prev-next-what" />
		<xsl:param name="type-node" />
		<table class="nav">
			<tr>
				<td class="nav1" colspan="2">
					<table cellspacing="3">
						<tr>
							<td>
								<xsl:choose>
									<xsl:when test="$select='Overview'">
										<xsl:attribute name="class">nav1sel</xsl:attribute>
										<xsl:text>&#160;Overview&#160;</xsl:text>
									</xsl:when>
									<xsl:otherwise>
										<a href="{$global-path-to-root}overview-summary.html">&#160;Overview&#160;</a>
									</xsl:otherwise>
								</xsl:choose>
							</td>
							<td>
								<xsl:choose>
									<xsl:when test="$select='Namespace'">
										<xsl:attribute name="class">nav1sel</xsl:attribute>
										<xsl:text>&#160;Namespace&#160;</xsl:text>
									</xsl:when>
									<xsl:otherwise>
										<xsl:choose>
											<xsl:when test="$link-namespace">
												<a href="namespace-summary.html">Namespace</a>
												<xsl:text>&#160;</xsl:text>
											</xsl:when>
											<xsl:otherwise>
												<xsl:text>Namespace&#160;</xsl:text>
											</xsl:otherwise>
										</xsl:choose>
									</xsl:otherwise>
								</xsl:choose>
							</td>
							<td>
								<xsl:if test="$select='Type'">
									<xsl:attribute name="class">nav1sel</xsl:attribute>
									<xsl:text>&#160;</xsl:text>
								</xsl:if>
								<xsl:text>Type&#160;</xsl:text>
							</td>
							<td>
								<xsl:if test="$select='Use'">
									<xsl:attribute name="class">nav1sel</xsl:attribute>
									<xsl:text>&#160;</xsl:text>
								</xsl:if>
								<xsl:text>Use&#160;</xsl:text>
							</td>
							<td>
								<xsl:if test="$select='Tree'">
									<xsl:attribute name="class">nav1sel</xsl:attribute>
									<xsl:text>&#160;</xsl:text>
								</xsl:if>
								<xsl:text>Tree&#160;</xsl:text>
							</td>
							<td>
								<xsl:if test="$select='Deprecated'">
									<xsl:attribute name="class">nav1sel</xsl:attribute>
									<xsl:text>&#160;</xsl:text>
								</xsl:if>
								<xsl:text>Deprecated&#160;</xsl:text>
							</td>
							<td>
								<xsl:if test="$select='Index'">
									<xsl:attribute name="class">nav1sel</xsl:attribute>
									<xsl:text>&#160;</xsl:text>
								</xsl:if>
								<xsl:text>Index&#160;</xsl:text>
							</td>
							<td>
								<xsl:if test="$select='Help'">
									<xsl:attribute name="class">nav1sel</xsl:attribute>
									<xsl:text>&#160;</xsl:text>
								</xsl:if>
								<xsl:text>Help&#160;</xsl:text>
							</td>
						</tr>
					</table>
				</td>
				<td class="logo" rowspan="2">.NET Framework<br />Beta 2</td>
			</tr>
			<tr class="nav2">
				<td>
					<xsl:text>PREV</xsl:text>
					<xsl:if test="$prev-next-what">
						<xsl:text>&#160;</xsl:text>
						<xsl:value-of select="$prev-next-what" />
					</xsl:if>
					<xsl:text>&#160;&#160;&#160;&#160;NEXT</xsl:text>
					<xsl:if test="$prev-next-what">
						<xsl:text>&#160;</xsl:text>
						<xsl:value-of select="$prev-next-what" />
					</xsl:if>
				</td>
				<td>FRAMES&#160;&#160;&#160;&#160;NO FRAMES</td>
			</tr>
			<xsl:if test="$type-node">
				<tr class="nav2">
					<td>
						<xsl:text>SUMMARY: </xsl:text>
						<xsl:text>INNER</xsl:text>
						<xsl:text> | </xsl:text>
						<xsl:choose>
							<xsl:when test="$type-node/field">
								<a href="#field-summary">FIELD</a>
							</xsl:when>
							<xsl:otherwise>FIELD</xsl:otherwise>
						</xsl:choose>
						<xsl:text> | </xsl:text>
						<xsl:choose>
							<xsl:when test="$type-node/constructor">
								<a href="#constructor-summary">CONST</a>
							</xsl:when>
							<xsl:otherwise>CONST</xsl:otherwise>
						</xsl:choose>
						<xsl:text> | </xsl:text>
						<xsl:choose>
							<xsl:when test="$type-node/property">
								<a href="#property-summary">PROP</a>
							</xsl:when>
							<xsl:otherwise>PROP</xsl:otherwise>
						</xsl:choose>
						<xsl:text> | </xsl:text>
						<xsl:choose>
							<xsl:when test="$type-node/method">
								<a href="#method-summary">METHOD</a>
							</xsl:when>
							<xsl:otherwise>METHOD</xsl:otherwise>
						</xsl:choose>
						<xsl:text> | </xsl:text>
						<xsl:choose>
							<xsl:when test="$type-node/operator">
								<a href="#operator-summary">OP</a>
							</xsl:when>
							<xsl:otherwise>OP</xsl:otherwise>
						</xsl:choose>
						<xsl:text> | </xsl:text>
						<xsl:choose>
							<xsl:when test="$type-node/event">
								<a href="#event-summary">EVENT</a>
							</xsl:when>
							<xsl:otherwise>EVENT</xsl:otherwise>
						</xsl:choose>
					</td>
					<td>DETAIL: FIELD | CONST | PROP | METHOD | OP | EVENT</td>
				</tr>
			</xsl:if>
		</table>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-href-to-namespace-summary">
		<xsl:param name="namespace-name" />
		<xsl:value-of select="concat(translate($namespace-name, '.', '/'), '/namespace-summary.html')" />
	</xsl:template>
	<!-- -->
	<xsl:template name="get-href-to-type">
		<xsl:param name="type-name" />
		<xsl:value-of select="concat($type-name, '.html')" />
	</xsl:template>
	<!-- -->
	<xsl:template name="get-href-to-constructor">
		<xsl:param name="constructor-node" />
		<xsl:value-of select="concat('#.ctor', $constructor-node/@overload)" />
	</xsl:template>
	<!-- -->
	<xsl:template name="get-href-to-field">
		<xsl:param name="field-node" />
		<xsl:value-of select="concat('#', $field-node/@name)" />
	</xsl:template>
	<!-- -->
	<xsl:template name="get-href-to-property">
		<xsl:param name="property-node" />
		<xsl:value-of select="concat('#', $property-node/@name, $property-node/@overload)" />
	</xsl:template>
	<!-- -->
	<xsl:template name="get-href-to-method">
		<xsl:param name="method-node" />
		<xsl:value-of select="concat('#', $method-node/@name, $method-node/@overload)" />
	</xsl:template>
	<!-- -->
	<xsl:template name="get-href-to-event">
		<xsl:param name="event-node" />
		<xsl:value-of select="concat('#', $event-node/@name)" />
	</xsl:template>
	<!-- -->
	<xsl:template match="*" mode="doc">
		<xsl:apply-templates mode="doc" />
	</xsl:template>
	<!-- -->
	<xsl:template match="summary" mode="doc">
		<xsl:apply-templates mode="doc" />
	</xsl:template>
	<!-- -->
	<xsl:template match="remarks" mode="doc">
		<xsl:apply-templates mode="doc" />
	</xsl:template>
	<!-- -->
	<xsl:template name="get-namespace">
		<xsl:param name="name" />
		<xsl:param name="namespace" />
		<xsl:choose>
			<xsl:when test="contains($name, '.')">
				<xsl:call-template name="get-namespace">
					<xsl:with-param name="name" select="substring-after($name, '.')" />
					<xsl:with-param name="namespace" select="concat($namespace, substring-before($name, '.'), '.')" />
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="substring($namespace, 1, string-length($namespace) - 1)" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<!-- -->
	<xsl:template name="strip-namespace">
		<xsl:param name="name" />
		<xsl:choose>
			<xsl:when test="contains($name, '.')">
				<xsl:call-template name="strip-namespace">
					<xsl:with-param name="name" select="substring-after($name, '.')" />
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$name" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-href-to-cref">
		<xsl:param name="cref" />
		<xsl:choose>
			<xsl:when test="starts-with($cref, 'T:')">
				<xsl:variable name="namespace">
					<xsl:call-template name="get-namespace">
						<xsl:with-param name="name" select="substring-after($cref, 'T:')" />
					</xsl:call-template>
				</xsl:variable>
				<xsl:variable name="name">
					<xsl:call-template name="strip-namespace">
						<xsl:with-param name="name" select="substring-after($cref, 'T:')" />
					</xsl:call-template>
				</xsl:variable>
				<xsl:value-of select="concat($global-path-to-root, translate($namespace, '.', '/'), '/', $name, '.html')" />
			</xsl:when>
			<xsl:when test="starts-with($cref, 'M:')">
				<xsl:variable name="type">
					<xsl:call-template name="get-namespace">
						<xsl:with-param name="name" select="substring-after($cref, 'M:')" />
					</xsl:call-template>
				</xsl:variable>
				<xsl:variable name="path">
					<xsl:call-template name="get-href-to-cref">
						<xsl:with-param name="cref" select="concat('T:', $type)" />
					</xsl:call-template>
				</xsl:variable>
				<xsl:variable name="member">
					<xsl:call-template name="strip-namespace">
						<xsl:with-param name="name" select="substring-after($cref, 'M:')" />
					</xsl:call-template>
				</xsl:variable>
				<xsl:value-of select="concat($path, '#', $member)" />
			</xsl:when>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<xsl:template match="see[@cref]" mode="doc">
		<a>
			<xsl:attribute name="href">
				<xsl:call-template name="get-href-to-cref">
					<xsl:with-param name="cref" select="@cref" />
				</xsl:call-template>
			</xsl:attribute>
			<xsl:value-of select="substring(@cref, 3)" />
		</a>
	</xsl:template>
	<!-- -->
</xsl:transform>
