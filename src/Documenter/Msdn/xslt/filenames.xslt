<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<!-- -->
	<xsl:template name="get-filename-for-current-namespace-hierarchy">
		<xsl:value-of select="concat(translate($namespace, '.[]', ''), 'Hierarchy.html')" />
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-namespace">
		<xsl:param name="name" />
		<xsl:value-of select="concat(translate($name, '.[]', ''), '.html')" />
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-type">
		<xsl:param name="id" />
		<xsl:value-of select="concat(translate(substring-after($id, 'T:'), '.[]', ''), '.html')" />
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-current-constructor-overloads">
		<xsl:variable name="type-part" select="translate(substring-after(../@id, 'T:'), '.[]', '')" />
		<xsl:value-of select="concat($type-part, 'ConstructorOverloads.html')" />
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-current-constructor">
		<!-- .#ctor or .#cctor -->
		<xsl:value-of select="concat(translate(substring-after(substring-before(@id, '.#c'), 'M:'), '.[]', ''), 'Constructor', @overload, '.html')" />
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-type-members">
		<xsl:param name="id" />
		<xsl:value-of select="concat(translate(substring-after($id, 'T:'), '.[]', ''), 'Members.html')" />
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-current-field">
		<xsl:value-of select="concat(translate(substring-after(@id, 'F:'), '.[]', ''), 'FieldOrEvent.html')" />
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-current-event">
		<xsl:value-of select="concat(translate(substring-after(@id, 'E:'), '.[]', ''), 'FieldOrEvent.html')" />
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-current-property-overloads">
		<xsl:variable name="type-part" select="translate(substring-after(../@id, 'T:'), '.[]', '')" />
		<xsl:value-of select="concat($type-part, @name, 'Overloads.html')" />
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-current-property">
		<xsl:choose>
			<xsl:when test="contains(@id, '(')">
				<xsl:value-of select="concat(translate(substring-after(substring-before(@id, '('), 'P:'), '.[]', ''), 'Property', @overload, '.html')" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="concat(translate(substring-after(@id, 'P:'), '.[]', ''), 'Property', @overload, '.html')" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-property">
		<xsl:param name="property" select="." />
		<xsl:choose>
			<xsl:when test="contains($property/@id, '(')">
				<xsl:value-of select="concat(translate(substring-after(substring-before($property/@id, '('), 'P:'), '.[]', ''), 'Property', $property/@overload, '.html')" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="concat(translate(substring-after($property/@id, 'P:'), '.[]', ''), 'Property', $property/@overload, '.html')" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-current-method-overloads">
		<xsl:variable name="type-part" select="translate(substring-after(../@id, 'T:'), '.[]', '')" />
		<xsl:value-of select="concat($type-part, @name, 'Overloads.html')" />
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-inherited-method-overloads">
		<xsl:param name="declaring-type" />
		<xsl:param name="method-name" />
		<xsl:variable name="type-part" select="translate($declaring-type, '.[]', '')" />
		<xsl:value-of select="concat($type-part, $method-name, 'Overloads.html')" />
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-method">
		<xsl:param name="method" select="." />
		<xsl:choose>
			<xsl:when test="contains($method/@id, '(')">
				<xsl:value-of select="concat(translate(substring-after(substring-before($method/@id, '('), 'M:'), '.[]', ''), 'Method', $method/@overload, '.html')" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="concat(translate(substring-after($method/@id, 'M:'), '.[]', ''), 'Method', $method/@overload, '.html')" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-system-property">
		<!-- Beta 2 Example:  ms-help://MS.VSCC/MS.MSDNVS/cpref/html/frlrfSystemExceptionClassInnerExceptionTopic.htm -->
		<xsl:value-of select="concat('ms-help://MS.VSCC/MS.MSDNVS/cpref/html/frlrf', translate(@declaringType, '.[]', ''), 'Class', @name, 'Topic.htm')" />
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-system-method">
		<!-- EXAMPLE:  ms-help://MSDNVS/cpref/html_hh2/frlrfSystemObjectClassEqualsTopic.htm -->
		<!-- Beta 2 Example:  ms-help://MS.VSCC/MS.MSDNVS/cpref/html/frlrfSystemObjectClassEqualsTopic.htm -->
		<xsl:value-of select="concat('ms-help://MS.VSCC/MS.MSDNVS/cpref/html/frlrf', translate(@declaringType, '.[]', ''), 'Class', @name, 'Topic.htm')" />
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-system-class">
		<xsl:param name="class-name" />
		<!-- Beta 2 Example:  ms-help://MS.VSCC/MS.MSDNVS/cpref/html/frlrfSystemObjectClassTopic.htm -->
		<xsl:value-of select="concat('ms-help://MS.VSCC/MS.MSDNVS/cpref/html/frlrf', translate($class-name, '.[]', ''), 'ClassTopic.htm')" />
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-individual-member">
		<xsl:param name="member" />
		<xsl:choose>
			<xsl:when test="$member = 'field'">
				<xsl:call-template name="get-filename-for-current-field" />
			</xsl:when>
			<xsl:when test="$member = 'property'">
				<xsl:call-template name="get-filename-for-current-property" />
			</xsl:when>
			<xsl:when test="$member = 'event'">
				<xsl:call-template name="get-filename-for-current-event" />
			</xsl:when>
			<xsl:when test="$member = 'operator'">
				<xsl:call-template name="get-filename-for-operator">
					<xsl:with-param name="operator" select="." />
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="get-filename-for-method" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-individual-member-overloads">
		<xsl:param name="member" />
		<xsl:choose>
			<xsl:when test="$member = 'property'">
				<xsl:call-template name="get-filename-for-current-property-overloads" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="get-filename-for-current-method-overloads" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-cref">
		<xsl:param name="cref" />
		<xsl:choose>
			<xsl:when test="starts-with($cref, 'T:')">
				<xsl:call-template name="get-filename-for-type-name">
					<xsl:with-param name="type-name" select="substring-after($cref, 'T:')" />
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="starts-with($cref, 'M:')">
				<xsl:choose>
					<xsl:when test="contains($cref, '(')">
						<xsl:value-of select="concat(translate(substring-after(substring-before($cref, '('), 'M:'), '.[]', ''), 'Method.html')" />
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="concat(translate(substring-after($cref, 'M:'), '.[]', ''), 'Method.html')" />
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:when test="starts-with($cref, 'E:')">
				<xsl:value-of select="concat(translate(substring-after($cref, 'E:'), '.[]', ''), 'FieldOrEvent.html')" />
			</xsl:when>
			<xsl:when test="starts-with($cref, 'F:')">
				<xsl:value-of select="concat(translate(substring-after($cref, 'F:'), '.[]', ''), 'FieldOrEvent.html')" />
			</xsl:when>
			<xsl:when test="starts-with($cref, 'P:')">
				<xsl:choose>
					<xsl:when test="contains($cref, '(')">
						<xsl:value-of select="concat(translate(substring-after(substring-before($cref, '('), 'P:'), '.[]', ''), 'Property', '.html')" />
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="concat(translate(substring-after($cref, 'P:'), '.[]', ''), 'Property', '.html')" />
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$cref" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-type-name">
		<xsl:param name="type-name" />
		<xsl:choose>
			<xsl:when test="starts-with($type-name, 'System.')">
				<xsl:call-template name="get-filename-for-system-class">
					<xsl:with-param name="class-name" select="$type-name" />
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="concat(translate($type-name, '.[]', ''), '.html')" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-operator">
		<xsl:param name="operator" select="." />
		<xsl:variable name="filename">
			<xsl:choose>
				<xsl:when test="contains($operator/@id, '(')">
					<xsl:value-of select="concat(translate(substring-after(substring-before($operator/@id, '('), 'M:'), '.[]', ''), 'Operator', $operator/@overload, '.html')" />
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="concat(translate(substring-after($operator/@id, 'M:'), '.[]', ''), 'Operator', $operator/@overload, '.html')" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:value-of select="concat(substring-before($filename, 'op_'), substring-after($filename, 'op_'))" />
	</xsl:template>
	<!-- -->
</xsl:stylesheet>
