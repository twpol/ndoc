<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:NUtil="urn:NDocUtil" xmlns:ndoc="urn:ndoc-schema">
	<!-- -->
	<xsl:include href="global_arguments.xslt" />
	<!-- -->
	<xsl:template name="get-filename-for-assembly">
		<xsl:param name="assemblyName" select="ancestor::ndoc:assembly/@name" />
		<!--<xsl:value-of select="concat(translate($name, '[,]', ''), '.html')" />-->
		<xsl:value-of select="NUtil:GetFilenameForAssembly($assemblyName)"/>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-namespace-hierarchy">
		<xsl:param name="assemblyName" select="ancestor::ndoc:assembly/@name" />
		<xsl:param name="namespace" />
		<!--<xsl:value-of select="concat(translate($namespace, '[,]', ''), 'Hierarchy.html')" />-->
		<xsl:value-of select="NUtil:GetFilenameForNamespaceHierarchy($assemblyName, $namespace)"/>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-namespace">
		<xsl:param name="assemblyName" select="ancestor::ndoc:assembly/@name" />
		<xsl:param name="namespace" />
		<!--<xsl:value-of select="concat(translate($name, '[,]', ''), '.html')" />-->
		<xsl:value-of select="NUtil:GetFilenameForNamespace($assemblyName, $namespace)"/>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-type">
		<xsl:param name="assemblyName" />
		<xsl:param name="id" />
		<!--<xsl:value-of select="concat(translate(substring-after($id, 'T:'), '[,]', ''), '.html')" />-->
		<xsl:value-of select="NUtil:GetFilenameForId($assemblyName, $id)"/>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-type-hierarchy">
		<xsl:param name="assemblyName" />
		<xsl:param name="id" />
		<!--<xsl:value-of select="concat(translate(substring-after($id, 'T:'), '[,]', ''), 'Hierarchy.html')" />-->
		<xsl:value-of select="NUtil:GetFilenameForTypeHierarchy($assemblyName, $id)"/>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-constructors">
		<xsl:param name="constructor" select="." />
		<!--<xsl:variable name="type-part" select="translate(substring-after(../@id, 'T:'), '[,]', '')" />
    <xsl:value-of select="concat($type-part, 'Constructor.html')" />-->
		<xsl:value-of select="NUtil:GetFilenameForConstructors($constructor/ancestor::ndoc:assembly/@name, $constructor/../@id)"/>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-constructor">
		<xsl:param name="assemblyName" />
		<xsl:param name="id" />
		<!--<xsl:variable name="type-part" select="translate(substring-after(../@id, 'T:'), '[,]', '')" />
    <xsl:value-of select="concat($type-part, 'Constructor.html')" />-->
		<xsl:value-of select="NUtil:GetFilenameForId($assemblyName, $id)"/>
	</xsl:template>
	<!-- -->
	<!--<xsl:template name="get-filename-for-current-constructor">
    -->
	<!-- .#ctor or .#cctor -->
	<!--
    <xsl:choose>
		<xsl:when test="@contract != 'Static'">
		    <xsl:value-of select="concat(translate(substring-after(substring-before(@id, '.#c'), 'M:'), '[,]', ''), 'Constructor', @overload, '.html')" />
		</xsl:when>
		<xsl:otherwise>
		    <xsl:value-of select="concat(translate(substring-after(substring-before(@id, '.#c'), 'M:'), '[,]', ''), 'StaticConstructor', @overload, '.html')" />
		</xsl:otherwise>
    </xsl:choose>
  </xsl:template>-->
	<!-- -->
	<xsl:template name="get-filename-for-type-members">
		<xsl:param name="type" select="." />
		<!--<xsl:value-of select="concat(translate(substring-after($id, 'T:'), '[,]', ''), 'Members.html')" />-->
		<xsl:value-of select="NUtil:GetFilenameForTypeMembers(ancestor::ndoc:assembly/@name, @id)"/>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-current-field">
		<!--<xsl:value-of select="concat(translate(translate(substring-after(@id, 'F:'), '[,]', ''), '#', '.'), '.html')" />-->
		<xsl:value-of select="NUtil:GetFilenameForId(ancestor::ndoc:assembly/@name, @id)"/>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-current-event">
		<!--<xsl:value-of select="concat(translate(translate(substring-after(@id, 'E:'), '[,]', ''), '#', '.'), '.html')" />-->
		<xsl:value-of select="NUtil:GetFilenameForId(ancestor::ndoc:assembly/@name, @id)"/>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-current-property-overloads">
		<!--<xsl:variable name="type-part" select="translate(translate(substring-after(../@id, 'T:'), '[,]', ''), '#', '.')" />
    <xsl:value-of select="concat($type-part, @name, '.html')" />-->
		<xsl:value-of select="NUtil:GetFilenameForPropertyOverloads(ancestor::ndoc:assembly/@name,../@id, @name)"/>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-current-property">
		<!--<xsl:choose>
      <xsl:when test="contains(@id, '(')">
        <xsl:value-of select="concat(translate(translate(substring-after(substring-before(@id, '('), 'P:'), '[,]', ''), '#', '.'), @overload, '.html')" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="concat(translate(translate(substring-after(@id, 'P:'), '[,]', ''), '#', '.'), @overload, '.html')" />
      </xsl:otherwise>
    </xsl:choose>-->
		<xsl:value-of select="NUtil:GetFilenameForId(ancestor::ndoc:assembly/@name, @id)"/>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-property">
		<xsl:param name="property" select="." />
		<!--<xsl:choose>
      <xsl:when test="contains($property/@id, '(')">
        <xsl:value-of select="concat(translate(translate(substring-after(substring-before($property/@id, '('), 'P:'), '[,]', ''), '#', '.'), $property/@overload, '.html')" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="concat(translate(translate(substring-after($property/@id, 'P:'), '[,]', ''), '#', '.'), $property/@overload, '.html')" />
      </xsl:otherwise>
    </xsl:choose>-->
		<xsl:value-of select="NUtil:GetFilenameForId($property/ancestor::ndoc:assembly/@name, $property/@id)"/>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-event">
		<xsl:param name="event" select="." />
		<!--<xsl:choose>
      <xsl:when test="contains($event/@id, '(')">
        <xsl:value-of select="concat(translate(translate(substring-after(substring-before($event/@id, '('), 'E:'), '[,]', ''), '#', '.'), $event/@overload, '.html')" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="concat(translate(translate(substring-after($event/@id, 'E:'), '[,]', ''), '#', '.'), $event/@overload, '.html')" />
      </xsl:otherwise>
    </xsl:choose>-->
		<xsl:value-of select="NUtil:GetFilenameForId($event/ancestor::ndoc:assembly/@name, $event/@id)"/>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-field">
		<xsl:param name="field" select="." />
		<!--<xsl:choose>
      <xsl:when test="contains($field/@id, '(')">
        <xsl:value-of select="concat(translate(translate(substring-after(substring-before($field/@id, '('), 'F:'), '[,]', ''), '#', '.'), $field/@overload, '.html')" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="concat(translate(translate(substring-after($field/@id, 'F:'), '[,]', ''), '#', '.'), $field/@overload, '.html')" />
      </xsl:otherwise>
    </xsl:choose>-->
		<xsl:value-of select="NUtil:GetFilenameForId($field/ancestor::ndoc:assembly/@name, $field/@id)"/>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-current-method-overloads">
		<!--<xsl:variable name="type-part" select="translate(translate(substring-after(../@id, 'T:'), '[,]', ''), '#', '.')" />
    <xsl:value-of select="concat($type-part, '.', @name, '_overloads.html')" />-->
		<xsl:value-of select="NUtil:GetFilenameForMethodOverloads(ancestor::ndoc:assembly/@name,../@id, @name)"/>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-inherited-method-overloads">
		<xsl:param name="declaring-assembly" />
		<xsl:param name="declaring-type" />
		<xsl:param name="method-name" />
		<!--<xsl:variable name="type-part" select="translate(translate($declaring-type, '[,]', ''), '#', '.')" />
    <xsl:value-of select="concat($type-part, '.', $method-name, '_overloads.html')" />-->
		<xsl:value-of select="NUtil:GetFilenameForMethodOverloads($declaring-assembly, $declaring-type, @method-name)"/>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-method">
		<xsl:param name="method" select="." />
		<xsl:value-of select="NUtil:GetFilenameForId($method/ancestor::ndoc:assembly/@name, $method/@id)"/>
		<!--<xsl:choose>
      <xsl:when test="contains($method/@id, '(')">
		<xsl:choose>
			<xsl:when test="string-length($method/@overload) &gt; 0">
		        <xsl:value-of select="concat(translate(translate(substring-after(substring-before($method/@id, '('), 'M:'), '[,]', ''), '#', '.'), '_overload_', $method/@overload, '.html')" />
			</xsl:when>
			<xsl:otherwise>
		        <xsl:value-of select="concat(translate(translate(substring-after(substring-before($method/@id, '('), 'M:'), '[,]', ''), '#', '.'), '.html')" />
			</xsl:otherwise>
		</xsl:choose>
      </xsl:when>
      <xsl:otherwise>
		<xsl:choose>
			<xsl:when test="string-length($method/@overload) &gt; 0">
		        <xsl:value-of select="concat(translate(translate(substring-after($method/@id, 'M:'), '[,]', ''), '#', '.'), '_overload_', $method/@overload, '.html')" />
			</xsl:when>
			<xsl:otherwise>
		        <xsl:value-of select="concat(translate(translate(substring-after($method/@id, 'M:'), '[,]', ''), '#', '.'), '.html')" />
			</xsl:otherwise>
		</xsl:choose>
      </xsl:otherwise>
    </xsl:choose>-->
	</xsl:template>
	<!-- Get the filename for an operator -->
	<xsl:template name="get-filename-for-operator">
		<xsl:param name="operator" select="." />
		<xsl:value-of select="NUtil:GetFilenameForId($operator/ancestor::ndoc:assembly/@name, $operator/@id)"/>
		<!--<xsl:variable name="filename">
      <xsl:choose>
        <xsl:when test="contains($operator/@id, '(')">
          <xsl:value-of select="concat(translate(translate(substring-after(substring-before($operator/@id, '('), 'M:'), '[,]', ''), '#', '.'), $operator/@overload, '.html')" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="concat(translate(translate(substring-after($operator/@id, 'M:'), '[,]', ''), '#', '.'), $operator/@overload, '.html')" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="$filename" />-->
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-system-cref">
		<xsl:param name="cref" />
		<xsl:value-of select="NUtil:FormatOnlineSDKLink($cref)"/>
	</xsl:template>

	<xsl:template name="get-filename-for-system-namespace">
		<xsl:param name="namespace-name" />
		<xsl:value-of select="concat($ndoc-sdk-doc-base-url, translate($namespace-name, '.', ''), $ndoc-sdk-doc-file-ext)" />
	</xsl:template>

	<xsl:template name="get-filename-for-system-type">
		<xsl:param name="type-name" />
		<xsl:value-of select="NUtil:FormatOnlineSDKLink($type-name)"/>
	</xsl:template>

	<xsl:template name="get-filename-for-system-property">
		<xsl:value-of select="concat($ndoc-sdk-doc-base-url, translate(@declaringType, '.[,]', ''), 'Class', translate(@name, '.[,]', ''), 'Topic', $ndoc-sdk-doc-file-ext)" />
	</xsl:template>

	<xsl:template name="get-filename-for-system-field">
		<xsl:value-of select="concat($ndoc-sdk-doc-base-url, translate(@declaringType, '.[,]', ''), 'Class', translate(@name, '.[,]', ''), 'Topic', $ndoc-sdk-doc-file-ext)" />
	</xsl:template>

	<xsl:template name="get-filename-for-system-method">
		<xsl:value-of select="concat($ndoc-sdk-doc-base-url, translate(@declaringType, '.[,]', ''), 'Class', translate(@name, '.[,]', ''), 'Topic', $ndoc-sdk-doc-file-ext)" />
	</xsl:template>

	<xsl:template name="get-filename-for-system-event">
		<xsl:value-of select="concat($ndoc-sdk-doc-base-url, translate(@declaringType, '.[,]', ''), 'Class', translate(@name, '.[,]', ''), 'Topic', $ndoc-sdk-doc-file-ext)" />
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
		<xsl:call-template name="get-filename-for-cref-overload">
			<xsl:with-param name="cref" select="$cref" />
			<xsl:with-param name="overload" select="''" />
		</xsl:call-template>
	</xsl:template>
	<!-- -->
	<xsl:template name="get-filename-for-cref-overload">
		<xsl:param name="cref" />
		<xsl:param name="overload" />
		<xsl:value-of select="NUtil:GetFilenameForCRefOverload(ancestor::ndoc:assembly/@name, $cref, $overload)"/>
	</xsl:template>
	
	<xsl:template name="get-filename-for-cref-overload-UNUSED">
		<xsl:param name="cref" />
		<xsl:param name="overload" />
		<!--<xsl:value-of select="NUtil:GetFilenameForCRefOverload($cref, $overload)"/>-->

		<xsl:choose>
			<xsl:when test="starts-with($cref, 'T:')">
				<xsl:call-template name="get-filename-for-type-name">
					<xsl:with-param name="type-name" select="substring-after($cref, 'T:')" />
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="starts-with($cref, 'M:')">
				<xsl:choose>
					<xsl:when test="contains($cref, '.#c')">
						<xsl:value-of select="concat(translate(substring-after(substring-before($cref, '.#c'), 'M:'), '[,]', ''), 'Constructor', $overload, '.html')" />
					</xsl:when>
					<xsl:when test="contains($cref, '(')">
						<xsl:choose>
							<xsl:when test="string-length($overload) &gt; 0">
								<xsl:value-of select="concat(translate(substring-after(substring-before($cref, '('), 'M:'), '[,]', ''), '_overload_', $overload, '.html')" />
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="concat(translate(substring-after(substring-before($cref, '('), 'M:'), '[,]', ''), '.html')" />
							</xsl:otherwise>
						</xsl:choose>
					</xsl:when>
					<xsl:otherwise>
						<xsl:choose>
							<xsl:when test="string-length($overload) &gt; 0">
								<xsl:value-of select="concat(translate(substring-after($cref, 'M:'), '[,]', ''), '_overload_', $overload, '.html')" />
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="concat(translate(substring-after($cref, 'M:'), '[,]', ''), '.html')" />
							</xsl:otherwise>
						</xsl:choose>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:when test="starts-with($cref, 'E:')">
				<xsl:value-of select="concat(translate(substring-after($cref, 'E:'), '[,]', ''), $overload, '.html')" />
			</xsl:when>
			<xsl:when test="starts-with($cref, 'F:')">
				<xsl:variable name="enum" select="/ndoc/assembly/module/namespace//enumeration[field/@id = $cref]" />
				<xsl:choose>
					<xsl:when test="$enum">
						<xsl:call-template name="get-filename-for-type-name">
							<xsl:with-param name="type-name" select="substring-after($enum/@id, 'T:')" />
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="concat(translate(substring-after($cref, 'F:'), '[,]', ''), $overload, '.html')" />
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:when test="starts-with($cref, 'P:')">
				<xsl:choose>
					<xsl:when test="contains($cref, '(')">
						<xsl:value-of select="concat(translate(substring-after(substring-before($cref, '('), 'P:'), '[,]', ''), $overload, '.html')" />
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="concat(translate(substring-after($cref, 'P:'), '[,]', ''), $overload, '.html')" />
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$cref" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- Get a filename from a type name -->
	<xsl:template name="get-filename-for-type-name">
		<xsl:param name="type-name" />
		<xsl:value-of select="NUtil:GetFilenameForTypename(ancestor::ndoc:assembly/@name, $type-name)"/>

		<!-- Variable to handle generic type-->
		<!--
    <xsl:variable name="name">
      <xsl:choose>
        <xsl:when test="contains($type-name, '&lt;')">
          <xsl:value-of select="substring-before($type-name, '&lt;')"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$type-name"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="starts-with($name, 'System.')">
        <xsl:call-template name="get-filename-for-system-type">
          <xsl:with-param name="type-name" select="$name" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="concat(translate($name, '[,]', ''), '.html')" />
      </xsl:otherwise>
    </xsl:choose>-->
	</xsl:template>
	<!-- -->
</xsl:stylesheet>
