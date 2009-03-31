<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:NUtil="urn:NDocUtil"
	xmlns:NHtmlProvider="urn:NDocExternalHtml" xmlns:ndoc="urn:ndoc-schema"
  exclude-result-prefixes="NUtil NHtmlProvider">
	<!-- -->
  <xsl:include href="filenames.xslt" />
  <xsl:include href="syntax.xslt" />
  <xsl:include href="vb-syntax.xslt" />
  <xsl:include href="tags.xslt" />
  <!--
	 | no-op extensibility templates
	 +-->
  <xsl:template match="node()|@*|text()" mode="preliminary-section" />
  <xsl:template match="node()|@*|text()" mode="seealso-section" />
  <xsl:template match="node()|@*|text()" mode="thread-safety-section" />
  <xsl:template match="node()|@*|text()" mode="overloads-remarks-section" />
  <xsl:template match="node()|@*|text()" mode="overloads-example-section" />
  <xsl:template match="node()|@*|text()" mode="overloads-summary-section" />
  <xsl:template match="node()|@*|text()" mode="requirements-section" />
  <xsl:template match="node()|@*|text()" mode="enumeration-members-section" />
  <xsl:template match="node()|@*|text()" mode="example-section" />
  <xsl:template match="node()|@*|text()" mode="exceptions-section" />
  <xsl:template match="node()|@*|text()" mode="events-section" />
  <xsl:template match="node()|@*|text()" mode="value-section" />
  <xsl:template match="node()|@*|text()" mode="remarks-section" />
  <xsl:template match="node()|@*|text()" mode="implements-section" />
  <xsl:template match="node()|@*|text()" mode="returnvalue-section" />
  <xsl:template match="node()|@*|text()" mode="parameter-section" />
  <xsl:template match="node()|@*|text()" mode="generictypeparam-section" />
  <xsl:template match="node()|@*|text()" mode="summary-section" />
  <xsl:template match="node()|@*|text()" mode="obsolete-section" />
  <xsl:template match="node()|@*|text()" mode="footer-row" />
  <xsl:template match="node()|@*|text()" mode="title-row" />
  <xsl:template match="node()|@*|text()" mode="header-section" />
  <xsl:template match="node()|@*|text()" mode="after-remarks-section"/>
  <!-- Link to generic class -->
  <xsl:template name="genericclasslink">
    <xsl:param name="genericarguments" />
    <xsl:param name="type" />
    <xsl:call-template name="get-filename-for-type-name">
      <xsl:with-param name="type-name">
        <xsl:value-of select="concat(substring-before($type, '{'), '`', count($genericarguments))"/>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>
  <!-- Generic Returntype -->
  <xsl:template name="generic-returnType">
    <xsl:param name="arguments" select="ndoc:returnType"/>
    <xsl:for-each select="$arguments/ndoc:genericargument">
      <xsl:choose>
        <xsl:when test="contains(@name, '.')">
          <a>
            <xsl:attribute name="href">
              <xsl:call-template name="genericclasslink">
                <xsl:with-param name="genericarguments" select="ndoc:genericargument"/>
                <xsl:with-param name="type" select="@name"/>
              </xsl:call-template>
            </xsl:attribute>
            <xsl:call-template name="get-datatype">
              <xsl:with-param name="datatype" select="substring-before(@name, '{')" />
            </xsl:call-template>
          </a>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="get-datatype">
            <xsl:with-param name="datatype" select="@name" />
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:if test="ndoc:genericargument">
        <xsl:text>&lt;</xsl:text>
        <xsl:call-template name="generic-returnType">
          <xsl:with-param name="arguments" select="current()"/>
        </xsl:call-template>
        <xsl:text>&gt;</xsl:text>
      </xsl:if>
      <xsl:if test="position()!=last()">
        <xsl:text>,</xsl:text>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>
  <!-- Generic Field -->
  <xsl:template name="generic-field">
    <xsl:for-each select="ndoc:genericargument">
      <xsl:choose>
        <xsl:when test="contains(@name, '.')">
          <a>
            <xsl:attribute name="href">
              <xsl:call-template name="get-filename-for-type-name">
                <xsl:with-param name="type-name" select="@name" />
              </xsl:call-template>
            </xsl:attribute>
            <xsl:call-template name="get-datatype">
              <xsl:with-param name="datatype" select="@name" />
            </xsl:call-template>
          </a>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="get-datatype">
            <xsl:with-param name="datatype" select="@name" />
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:if test="ndoc:genericargument">
        <xsl:text>&lt;</xsl:text>
      </xsl:if>
      <xsl:if test="ndoc:genericargument">
        <xsl:call-template name="generic-field"/>
      </xsl:if>
      <xsl:if test="ndoc:genericargument">
        <xsl:text>&gt;</xsl:text>
      </xsl:if>
      <xsl:if test="position()!=last()">
        <xsl:text>,</xsl:text>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>
  <!-- C# Types -->
  <xsl:template name="csharp-type">
    <xsl:param name="runtime-type" />
    <xsl:variable name="old-type">
      <xsl:choose>
        <!-- If it contains a [ remove it -->
        <xsl:when test="contains($runtime-type, '[')">
          <xsl:value-of select="substring-before($runtime-type, '[')" />
        </xsl:when>
        <!-- If it contains a & remove it -->
        <xsl:when test="contains($runtime-type, '&amp;')">
          <xsl:value-of select="substring-before($runtime-type, '&amp;')" />
        </xsl:when>
        <!-- Otherwise just the name -->
        <xsl:otherwise>
          <xsl:value-of select="$runtime-type" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="new-type">
      <!-- Convert known types to short form -->
      <xsl:choose>
        <xsl:when test="$old-type='System.Byte'">byte</xsl:when>
        <xsl:when test="$old-type='Byte'">byte</xsl:when>
        <xsl:when test="$old-type='System.SByte'">sbyte</xsl:when>
        <xsl:when test="$old-type='SByte'">sbyte</xsl:when>
        <xsl:when test="$old-type='System.Int16'">short</xsl:when>
        <xsl:when test="$old-type='Int16'">short</xsl:when>
        <xsl:when test="$old-type='System.UInt16'">ushort</xsl:when>
        <xsl:when test="$old-type='UInt16'">ushort</xsl:when>
        <xsl:when test="$old-type='System.Int32'">int</xsl:when>
        <xsl:when test="$old-type='Int32'">int</xsl:when>
        <xsl:when test="$old-type='System.UInt32'">uint</xsl:when>
        <xsl:when test="$old-type='UInt32'">uint</xsl:when>
        <xsl:when test="$old-type='System.Int64'">long</xsl:when>
        <xsl:when test="$old-type='Int64'">long</xsl:when>
        <xsl:when test="$old-type='System.UInt64'">ulong</xsl:when>
        <xsl:when test="$old-type='UInt64'">ulong</xsl:when>
        <xsl:when test="$old-type='System.Single'">float</xsl:when>
        <xsl:when test="$old-type='Single'">float</xsl:when>
        <xsl:when test="$old-type='System.Double'">double</xsl:when>
        <xsl:when test="$old-type='Double'">double</xsl:when>
        <xsl:when test="$old-type='System.Decimal'">decimal</xsl:when>
        <xsl:when test="$old-type='Decimal'">decimal</xsl:when>
        <xsl:when test="$old-type='System.String'">string</xsl:when>
        <xsl:when test="$old-type='String'">string</xsl:when>
        <xsl:when test="$old-type='System.Char'">char</xsl:when>
        <xsl:when test="$old-type='Char'">char</xsl:when>
        <xsl:when test="$old-type='System.Boolean'">bool</xsl:when>
        <xsl:when test="$old-type='Boolean'">bool</xsl:when>
        <xsl:when test="$old-type='System.Void'">void</xsl:when>
        <xsl:when test="$old-type='Void'">void</xsl:when>
        <xsl:when test="$old-type='System.Object'">object</xsl:when>
        <xsl:when test="$old-type='Object'">object</xsl:when>
        <xsl:otherwise>
          <xsl:choose>
            <xsl:when test="contains($old-type, '`')">
              <xsl:value-of select="concat(substring-before(ndoc:returnType/@type, '`'), string('&lt;'), substring-after(ndoc:returnType/@type, '&lt;'))"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$old-type" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="contains($runtime-type, '[')">
        <xsl:value-of select="concat($new-type, '[', substring-after($runtime-type, '['))" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$new-type" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- Type access modifier -->
  <xsl:template name="type-access">
    <xsl:param name="access" />
    <xsl:param name="type" />
    <xsl:choose>
      <xsl:when test="$access='Public'">public</xsl:when>
      <xsl:when test="$access='NotPublic'">internal</xsl:when>
      <xsl:when test="$access='NestedPublic'">public</xsl:when>
      <xsl:when test="$access='NestedFamily'">protected</xsl:when>
      <xsl:when test="$access='NestedFamilyOrAssembly'">protected internal</xsl:when>
      <xsl:when test="$access='NestedAssembly'">internal</xsl:when>
      <xsl:when test="$access='NestedPrivate'">private</xsl:when>
      <xsl:otherwise>/* unknown */</xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- Method access modifier -->
  <xsl:template name="method-access">
    <xsl:param name="access" />
    <xsl:choose>
      <xsl:when test="$access='Public'">public</xsl:when>
      <xsl:when test="$access='Family'">protected</xsl:when>
      <xsl:when test="$access='FamilyOrAssembly'">protected internal</xsl:when>
      <xsl:when test="$access='Assembly'">internal</xsl:when>
      <xsl:when test="$access='Private'">private</xsl:when>
      <xsl:otherwise>/* unknown */</xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- Contract -->
  <xsl:template name="contract">
    <xsl:param name="contract" />
    <xsl:choose>
      <xsl:when test="$contract='Static'">static</xsl:when>
      <xsl:when test="$contract='Abstract'">abstract</xsl:when>
      <xsl:when test="$contract='Final'"></xsl:when>
      <xsl:when test="$contract='Virtual'">virtual</xsl:when>
      <xsl:when test="$contract='Override'">override</xsl:when>
      <xsl:when test="$contract='Normal'"></xsl:when>
      <xsl:otherwise>/* unknown */</xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- Generic template parameter -->
  <xsl:template name="generictypeparam-topic">
    <dl>
      <xsl:for-each select="ndoc:documentation/ndoc:typeparam">
        <xsl:variable name="name" select="@name" />
        <dt>
          <i>
            <xsl:value-of select="@name"/>
          </i>
        </dt>
        <dd>
          <xsl:apply-templates select="node()" mode="slashdoc" />
        </dd>
      </xsl:for-each>
    </dl>
  </xsl:template>
  <!-- Parameter -->
  <xsl:template name="parameter-topic">
    <dl>
      <xsl:for-each select="ndoc:parameter">
        <xsl:variable name="name" select="@name" />
        <dt>
          <i>
            <xsl:value-of select="@name" />
          </i>
        </dt>
        <dd>
          <xsl:apply-templates select="parent::node()/ndoc:documentation/ndoc:param[@name=$name]/node()" mode="slashdoc" />
        </dd>
      </xsl:for-each>
    </dl>
  </xsl:template>
  <!-- -->
  <xsl:template name="type-mixed">
    <xsl:choose>
      <xsl:when test="local-name()='constructor' or local-name()='property' or local-name()='method' or local-name()='event' or local-name()='operator'">
        <xsl:choose>
          <xsl:when test="local-name(..)='interface'">Interface</xsl:when>
          <xsl:otherwise>Class</xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test="local-name()='interface'">Interface</xsl:when>
          <xsl:otherwise>Class</xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- -->
  <xsl:template name="type-element">
    <xsl:choose>
      <xsl:when test="local-name()='constructor' or local-name()='field' or local-name()='property' or local-name()='method' or local-name()='event' or local-name()='operator'">
        <xsl:value-of select="local-name(..)" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="local-name()" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- -->
  <xsl:template name="type-name">
    <xsl:choose>
      <xsl:when test="local-name()='constructor' or local-name()='field' or local-name()='property' or local-name()='method' or local-name()='event' or local-name()='operator'">
        <xsl:call-template name="get-displayname-csharp">
          <xsl:with-param name="node" select=".." />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="get-displayname-csharp">
          <xsl:with-param name="node" select="." />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- -->
  <xsl:template name="type-id">
    <xsl:choose>
      <xsl:when test="local-name()='constructor' or local-name()='field' or local-name()='property' or local-name()='method' or local-name()='event' or local-name()='operator'">
        <xsl:value-of select="../@id" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="@id" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- -->
  <xsl:template name="namespace-name">
    <xsl:choose>
      <xsl:when test="local-name()='constructor' or local-name()='field' or local-name()='property' or local-name()='method' or local-name()='event' or local-name()='operator'">
        <xsl:value-of select="../../@name" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="../@name" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- -->
  <xsl:template name="seealso-section">
    <xsl:param name="page" />
    <xsl:variable name="typeMixed">
      <xsl:call-template name="type-mixed" />
    </xsl:variable>
    <xsl:variable name="typeElement">
      <xsl:call-template name="type-element" />
    </xsl:variable>
    <xsl:variable name="typeName">
      <xsl:call-template name="type-name" />
    </xsl:variable>
    <xsl:variable name="typeID">
      <xsl:call-template name="type-id" />
    </xsl:variable>
    <xsl:variable name="namespaceName">
      <xsl:call-template name="namespace-name" />
    </xsl:variable>
    <h4 class="dtH4">See Also</h4>
    <p>
      <xsl:if test="$page!='type' and $page!='enumeration' and $page!='delegate'">
        <xsl:variable name="type-filename">
          <xsl:call-template name="get-filename-for-type">
						<xsl:with-param name="assemblyName" select="ancestor::ndoc:assembly/@name" />
						<xsl:with-param name="id" select="$typeID" />
          </xsl:call-template>
        </xsl:variable>
        <a href="{$type-filename}">
          <xsl:value-of select="concat($typeName, ' ', $typeMixed)" />
        </a>
        <xsl:text> | </xsl:text>
      </xsl:if>
      <xsl:if test="(ndoc:constructor|ndoc:field|ndoc:property|ndoc:method|ndoc:operator|ndoc:event) and $page!='members' and $page!='enumeration' and $page!='delegate' and $page!='methods' and $page!='properties' and $page!='fields' and $page!='events'">
        <a>
          <xsl:attribute name="href">
            <xsl:call-template name="get-filename-for-type-members">
              <xsl:with-param name="type" select="." />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:value-of select="$typeName" />
          <xsl:text> Members</xsl:text>
        </a>
        <xsl:text> | </xsl:text>
      </xsl:if>
      <a>
        <xsl:attribute name="href">
          <xsl:call-template name="get-filename-for-namespace">
            <xsl:with-param name="namespace" select="$namespaceName" />
          </xsl:call-template>
        </xsl:attribute>
        <xsl:value-of select="$namespaceName" />
        <xsl:text> Namespace</xsl:text>
      </a>
      <xsl:if test="$page='member' or $page='property'">
        <xsl:variable name="memberName" select="@name" />
        <xsl:if test="count(parent::node()/*[@name=$memberName]) > 1">
          <xsl:choose>
            <xsl:when test="local-name()='constructor'">
              <xsl:text> | </xsl:text>
              <a>
                <xsl:attribute name="href">
									<xsl:call-template name="get-filename-for-constructors">
										<xsl:with-param name="constructor" select="." />
									</xsl:call-template>
									</xsl:attribute>
                <xsl:value-of select="$typeName" />
                <xsl:text> Constructor Overload List</xsl:text>
              </a>
            </xsl:when>
            <xsl:when test="local-name()='operator'">
              <xsl:if test="@name!='op_Implicit' and @name!='op_Explicit'">
                <xsl:text> | </xsl:text>
                <a>
                  <xsl:attribute name="href">
                    <xsl:call-template name="get-filename-for-current-method-overloads" />
                  </xsl:attribute>
                  <xsl:value-of select="$typeName" />
                  <xsl:text></xsl:text>
                  <xsl:call-template name="operator-name">
                    <xsl:with-param name="name">
                      <xsl:value-of select="@name" />
                    </xsl:with-param>
                  </xsl:call-template>
                  <xsl:text> Overload List</xsl:text>
                </a>
              </xsl:if>
            </xsl:when>
            <xsl:when test="local-name()='property'">
              <xsl:text> | </xsl:text>
              <a>
                <xsl:attribute name="href">
                  <xsl:call-template name="get-filename-for-current-property-overloads" />
                </xsl:attribute>
                <xsl:value-of select="concat($typeName, '.', @name)" />
                <xsl:text> Overload List</xsl:text>
              </a>
            </xsl:when>
            <xsl:when test="local-name()='method'">
              <xsl:text> | </xsl:text>
              <a>
                <xsl:attribute name="href">
                  <xsl:call-template name="get-filename-for-current-method-overloads" />
                </xsl:attribute>
                <xsl:value-of select="concat($typeName, '.', @name)" />
                <xsl:text> Overload List</xsl:text>
              </a>
            </xsl:when>
          </xsl:choose>
        </xsl:if>
      </xsl:if>
      <xsl:if test="ndoc:documentation//ndoc:seealso">
        <xsl:for-each select="ndoc:documentation//ndoc:seealso">
          <xsl:text> | </xsl:text>
          <xsl:choose>
            <xsl:when test="@cref">
              <xsl:call-template name="get-a-href">
                <xsl:with-param name="cref" select="@cref" />
              </xsl:call-template>
            </xsl:when>
            <xsl:when test="@href">
              <a href="{@href}">
                <xsl:value-of select="." />
              </a>
            </xsl:when>
          </xsl:choose>
        </xsl:for-each>
      </xsl:if>
      <xsl:apply-templates select="ndoc:documentation/node()" mode="seealso-section" />
    </p>
  </xsl:template>
  <!-- -->
  <xsl:template name="output-paragraph">
    <xsl:param name="nodes" />
    <xsl:choose>
      <xsl:when test="not($nodes/self::ndoc:para | $nodes/self::ndoc:p)">
        <p>
          <xsl:apply-templates select="$nodes" mode="slashdoc" />
        </p>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="$nodes" mode="slashdoc" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- -->
  <xsl:template match="ndoc:para | ndoc:p" mode="no-para">
    <xsl:apply-templates mode="slashdoc" />
  </xsl:template>
  <!-- -->
  <xsl:template match="ndoc:note" mode="no-para">
    <blockquote class="dtBlock">
      <b>Note</b>
      <xsl:text>&#160;&#160;&#160;</xsl:text>
      <xsl:apply-templates mode="slashdoc" />
    </blockquote>
  </xsl:template>
  <!-- -->
  <xsl:template match="node()" mode="no-para">
    <xsl:apply-templates select="." mode="slashdoc" />
  </xsl:template>
  <!-- -->
  <xsl:template name="obsolete-section">
    <xsl:if test="./ndoc:obsolete">
      <P>
        <FONT color="red">
          <B>
            NOTE: This <xsl:value-of select="name()" /> is now obsolete.
          </B>
        </FONT>
      </P>
      <xsl:if test="./ndoc:obsolete!=''">
        <P>
          <B>
            <xsl:value-of select="ndoc:obsolete" />
          </B>
        </P>
      </xsl:if>
      <xsl:if test="./ndoc:documentation/ndoc:obsolete!=''">
        <xsl:call-template name="output-paragraph">
          <xsl:with-param name="nodes" select="(ndoc:documentation/ndoc:obsolete)[1]/node()" />
        </xsl:call-template>
      </xsl:if>
      <xsl:apply-templates select="ndoc:documentation/node()" mode="obsolete-section" />
      <HR />
    </xsl:if>
  </xsl:template>
  <!-- -->
  <xsl:template name="obsolete-inline">
    <xsl:if test="./ndoc:obsolete">
      <FONT color="red">
        <B>Obsolete. </B>
      </FONT>
    </xsl:if>
  </xsl:template>
  <!-- -->
  <xsl:template name="summary-section">
    <xsl:if test="ancestor-or-self::node()/ndoc:documentation/ndoc:preliminary | /ndoc:ndoc/ndoc:preliminary">
      <xsl:call-template name="preliminary-section" />
    </xsl:if>
    <xsl:call-template name="obsolete-section" />
    <xsl:for-each select="ndoc:documentation/ndoc:summary">
      <xsl:call-template name="output-paragraph">
        <xsl:with-param name="nodes" select="node()" />
      </xsl:call-template>
    </xsl:for-each>
    <xsl:apply-templates select="ndoc:documentation/node()" mode="summary-section" />
  </xsl:template>
  <!-- -->
  <xsl:template name="summary-with-no-paragraph">
    <xsl:param name="member" select="." />
    <xsl:for-each select="$member/ndoc:documentation/ndoc:summary">
      <xsl:apply-templates select="node()" mode="no-para" />
      <xsl:if test="not(node())">&#160;</xsl:if>
    </xsl:for-each>
  </xsl:template>
  <!-- -->
  <xsl:template name="overloads-summary-section">
    <xsl:variable name="memberName" select="@name" />
    <xsl:choose>
      <xsl:when test="parent::node()/*[@name=$memberName]/ndoc:documentation/ndoc:overloads/ndoc:summary">
        <xsl:call-template name="output-paragraph">
          <xsl:with-param name="nodes" select="(parent::node()/*[@name=$memberName]/ndoc:documentation/ndoc:overloads/ndoc:summary)[1]/node()" />
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="parent::node()/*[@name=$memberName]/ndoc:documentation/ndoc:overloads">
        <xsl:call-template name="output-paragraph">
          <xsl:with-param name="nodes" select="(parent::node()/*[@name=$memberName]/ndoc:documentation/ndoc:overloads)[1]/node()" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="summary-section" />
      </xsl:otherwise>
    </xsl:choose>
    <xsl:apply-templates select="ndoc:documentation/node()" mode="overloads-summary-section" />
  </xsl:template>
  <!-- -->
  <xsl:template name="overloads-summary-with-no-paragraph">
    <xsl:param name="overloads" select="." />
    <xsl:variable name="memberName" select="@name" />
    <xsl:choose>
      <xsl:when test="$overloads/../*[@name=$memberName]/ndoc:documentation/ndoc:overloads/summary">
        <xsl:apply-templates select="($overloads/../*[@name=$memberName]/ndoc:documentation/ndoc:overloads/ndoc:summary)[1]/node()"
					mode="no-para" />
      </xsl:when>
      <xsl:when test="$overloads/../*[@name=$memberName]/ndoc:documentation/ndoc:overloads">
        <xsl:apply-templates select="($overloads/../*[@name=$memberName]/ndoc:documentation/ndoc:overloads)[1]/node()"
					mode="no-para" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="summary-with-no-paragraph">
          <xsl:with-param name="member" select="$overloads" />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- -->
  <xsl:template name="overloads-remarks-section">
    <xsl:variable name="memberName" select="@name" />
    <xsl:if test="parent::node()/*[@name=$memberName]/documentation/ndoc:overloads/ndoc:remarks">
      <h4 class="dtH4">Remarks</h4>
      <p>
        <xsl:call-template name="output-paragraph">
          <xsl:with-param name="nodes" select="(parent::node()/*[@name=$memberName]/ndoc:documentation/ndoc:overloads/ndoc:remarks)[1]/node()" />
        </xsl:call-template>
      </p>
    </xsl:if>
  </xsl:template>
  <!-- Overloads example section -->
  <xsl:template name="overloads-example-section">
    <xsl:variable name="memberName" select="@name" />
    <xsl:if test="parent::node()/*[@name=$memberName]/ndoc:documentation/ndoc:overloads/ndoc:example">
      <h4 class="dtH4">Example</h4>
      <p>
        <xsl:call-template name="output-paragraph">
          <xsl:with-param name="nodes" select="(parent::node()/*[@name=$memberName]/ndoc:documentation/ndoc:overloads/ndoc:example)[1]/node()" />
        </xsl:call-template>
      </p>
    </xsl:if>
  </xsl:template>
  <!-- Parameter section -->
  <xsl:template name="parameter-section">
    <xsl:if test="ndoc:documentation/ndoc:param">
      <h4 class="dtH4">Parameters</h4>
      <xsl:call-template name="parameter-topic" />
      <xsl:apply-templates select="ndoc:documentation/node()" mode="parameter-section" />
    </xsl:if>
  </xsl:template>
  <!-- Generic template parameter section -->
  <xsl:template name="generictypeparam-section">
    <xsl:if test="ndoc:documentation/ndoc:typeparam">
      <h4 class="dtH4">Generic template parameters</h4>
      <xsl:call-template name="generictypeparam-topic" />
      <xsl:apply-templates select="ndoc:documentation/node()" mode="generictypeparam-section"/>
    </xsl:if>
  </xsl:template>
  <!-- Returnvalue section -->
  <xsl:template name="returnvalue-section">
    <xsl:if test="ndoc:documentation/ndoc:returns">
      <h4 class="dtH4">Return Value</h4>
      <p>
        <xsl:apply-templates select="ndoc:documentation/ndoc:returns/node()" mode="slashdoc" />
      </p>
      <xsl:apply-templates select="ndoc:documentation/node()" mode="returnvalue-section" />
    </xsl:if>
  </xsl:template>
  <!-- Implements section -->
  <xsl:template name="implements-section">
    <xsl:if test="ndoc:implements">
      <h4 class="dtH4">Implements</h4>
      <xsl:for-each select="ndoc:implements">
        <p>
          <xsl:call-template name="implements-member"/>
        </p>
      </xsl:for-each>
      <xsl:apply-templates select="ndoc:documentation/node()" mode="implements-section" />
    </xsl:if>
  </xsl:template>
  <!-- -->
  <xsl:template name="implements-member">
    <xsl:variable name="href" select="string(NUtil:GetHRef(ancestor::ndoc:assembly/@name, @id))" />
    <a>
      <xsl:attribute name="href">
        <xsl:value-of select="$href" />
      </xsl:attribute>
      <xsl:value-of select="@interface" />
      <xsl:text>.</xsl:text>
      <xsl:value-of select="@name" />
    </a>
  </xsl:template>
  <!-- Remarks Section -->
  <xsl:template name="remarks-section">
    <xsl:if test="ndoc:documentation/ndoc:remarks">
      <h4 class="dtH4">Remarks</h4>
      <xsl:variable name="first-element" select="local-name(ndoc:documentation/ndoc:remarks/*[1])" />
      <xsl:choose>
        <xsl:when test="$first-element!='para' and $first-element!='p'">
          <p>
            <xsl:apply-templates select="ndoc:documentation/ndoc:remarks/node()" mode="slashdoc" />
          </p>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="ndoc:documentation/ndoc:remarks/node()" mode="slashdoc" />
        </xsl:otherwise>
      </xsl:choose>
      <xsl:apply-templates select="ndoc:documentation/node()" mode="remarks-section" />
    </xsl:if>
  </xsl:template>
  <!-- Value Section -->
  <xsl:template name="value-section">
    <xsl:if test="ndoc:documentation/ndoc:value">
      <h4 class="dtH4">Property Value</h4>
      <p>
        <xsl:apply-templates select="ndoc:documentation/ndoc:value/node()" mode="slashdoc" />
      </p>
      <xsl:apply-templates select="ndoc:documentation/node()" mode="value-section" />
    </xsl:if>
  </xsl:template>
  <!-- Events Section -->
  <xsl:template name="events-section">
    <xsl:if test="ndoc:documentation/ndoc:event">
      <h4 class="dtH4">Events</h4>
      <div class="tablediv">
        <table class="dtTABLE" cellspacing="0">
          <tr valign="top">
            <th width="50%">Event Type</th>
            <th width="50%">Reason</th>
          </tr>
          <xsl:for-each select="ndoc:documentation/ndoc:event">
            <xsl:sort select="@name" />
            <tr valign="top">
              <td width="50%">
                <xsl:call-template name="get-a-href-with-name">
                  <xsl:with-param name="cref" select="@cref" />
                </xsl:call-template>
              </td>
              <td width="50%">
                <xsl:apply-templates select="./node()" mode="slashdoc" />
                <xsl:if test="not(./node())">&#160;</xsl:if>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </div>
      <xsl:apply-templates select="ndoc:documentation/node()" mode="events-section" />
    </xsl:if>
  </xsl:template>
  <!-- Exceptions Sectio -->
  <xsl:template name="exceptions-section">
    <xsl:if test="ndoc:documentation/ndoc:exception">
      <h4 class="dtH4">Exceptions</h4>
      <div class="tablediv">
        <table class="dtTABLE" cellspacing="0">
          <tr valign="top">
            <th width="50%">Exception Type</th>
            <th width="50%">Condition</th>
          </tr>
          <xsl:for-each select="ndoc:documentation/ndoc:exception">
            <xsl:sort select="@name" />
            <tr valign="top">
              <td width="50%">
                <xsl:call-template name="get-a-href-with-name">
                  <xsl:with-param name="cref" select="@cref" />
                </xsl:call-template>
              </td>
              <td width="50%">
                <xsl:apply-templates select="./node()" mode="slashdoc" />
                <xsl:if test="not(./node())">&#160;</xsl:if>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </div>
      <xsl:apply-templates select="ndoc:documentation/node()" mode="exceptions-section" />
    </xsl:if>
  </xsl:template>
  <!-- Thread safety section -->
  <xsl:template name="thread-safety-section">
    <xsl:choose>
      <xsl:when test="ndoc:documentation/ndoc:threadsafety">
        <xsl:apply-templates select="ndoc:documentation/ndoc:threadsafety" />
      </xsl:when>
      <xsl:otherwise>
        <!-- document the project default theadsafety tag -->
        <xsl:apply-templates select="/ndoc:ndoc/ndoc:threadsafety" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- Threadsafty text-->
  <xsl:template match="ndoc:threadsafety">
    <H4 class="dtH4">Thread Safety</H4>
    <xsl:choose>
      <xsl:when test="@static='true' and @instance='true'">
        <P>This type is safe for multithreaded operations.</P>
      </xsl:when>
      <xsl:when test="@static='false' and @instance='false'">
        <P>
          This type is <b>not</b> safe for multithreaded operations.
        </P>
      </xsl:when>
      <xsl:when test="@static='true' and @instance='false'">
        <P>
          Public static (<b>Shared</b> in Visual Basic) members of this type are
          safe for multithreaded operations. Instance members are <b>not</b> guaranteed to be
          thread-safe.
        </P>
      </xsl:when>
      <xsl:when test="@static='false' and @instance='true'">
        <P>
          Public static (<b>Shared</b> in Visual Basic) members of this type are <b>not</b> guaranteed
          to be safe for multithreaded operations. Instance members are thread-safe.
        </P>
      </xsl:when>
    </xsl:choose>
    <xsl:apply-templates select="node()" mode="slashdoc" />
    <xsl:apply-templates select="." mode="thread-safety-section" />
  </xsl:template>
  <!-- Example section -->
  <xsl:template name="example-section">
    <xsl:if test="ndoc:documentation/ndoc:example">
      <h4 class="dtH4">Example</h4>
      <p>
        <xsl:apply-templates select="ndoc:documentation/ndoc:example/node()" mode="slashdoc" />
      </p>
      <xsl:apply-templates select="ndoc:documentation/node()" mode="example-section" />
    </xsl:if>
  </xsl:template>
  <!-- Enumeration members section -->
  <xsl:template name="enumeration-members-section">
    <xsl:if test="ndoc:field">
      <xsl:variable name="asflags">
        <xsl:choose>
          <xsl:when test="@flags">true</xsl:when>
          <xsl:otherwise>false</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <h4 class="dtH4">Members</h4>
      <div class="tablediv">
        <table class="dtTABLE" cellspacing="0">
          <tr valign="top">
            <xsl:choose>
              <xsl:when test="$asflags='true'">
                <th width="40%">Member Name</th>
                <th width="40%">Description</th>
                <th width="20%">Value</th>
              </xsl:when>
              <xsl:otherwise>
                <th width="50%">Member Name</th>
                <th width="50%">Description</th>
              </xsl:otherwise>
            </xsl:choose>
          </tr>
          <xsl:for-each select="ndoc:field">
            <xsl:text>&#10;</xsl:text>
            <tr valign="top">
              <td>
                <b>
                  <xsl:value-of select="@name" />
                </b>
              </td>
              <td>
                <xsl:if test="./ndoc:obsolete">
                  <FONT color="red">
                    <B>Obsolete.</B>&#160;
                  </FONT>
                  <xsl:if test="./ndoc:obsolete!=''">
                    <B>
                      <xsl:value-of select="ndoc:obsolete" />
                    </B>
                    <br />
                  </xsl:if>
                </xsl:if>
                <xsl:apply-templates select="ndoc:documentation/ndoc:summary/node()" mode="slashdoc" />
                <xsl:if test="not(ndoc:documentation/ndoc:summary/node())">&#160;</xsl:if>
              </td>
              <xsl:if test="$asflags='true'">
                <td>
                  <xsl:value-of select="@value" />
                </td>
              </xsl:if>
            </tr>
          </xsl:for-each>
        </table>
      </div>
      <xsl:apply-templates select="ndoc:documentation/node()" mode="enumeration-members-section" />
    </xsl:if>
  </xsl:template>
  <!-- Get language -->
  <xsl:template name="get-lang">
    <xsl:param name="lang" />
    <xsl:choose>
      <xsl:when test="$lang = 'VB' or $lang='Visual Basic'">
        <xsl:value-of select="'Visual&#160;Basic'" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$lang" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- Get a link from a cref -->
  <xsl:template name="get-a-href">
    <xsl:param name="cref" />
    <xsl:variable name="href" select="string(NUtil:GetHRef(ancestor::ndoc:assembly/@name, $cref))" />
    <xsl:choose>
      <xsl:when test="$href=''">
        <b>
          <xsl:value-of select="string(NUtil:GetName(ancestor::ndoc:assembly/@name, $cref))" />
        </b>
      </xsl:when>
      <xsl:otherwise>
        <a>
          <xsl:attribute name="href">
            <xsl:value-of select="$href" />
          </xsl:attribute>
          <xsl:choose>
            <xsl:when test="node()">
              <xsl:value-of select="." />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="string(NUtil:GetName(ancestor::ndoc:assembly/@name, $cref))" />
            </xsl:otherwise>
          </xsl:choose>
        </a>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- get-a-href-with-name -->
  <xsl:template name="get-a-href-with-name">
    <xsl:param name="cref" />
    <xsl:variable name="href" select="string(NUtil:GetHRef(ancestor::ndoc:assembly/@name, $cref))" />
    <xsl:choose>
      <xsl:when test="$href=''">
        <b>
          <xsl:value-of select="string(NUtil:GetName(ancestor::ndoc:assembly/@name, $cref))" />
        </b>
      </xsl:when>
      <xsl:otherwise>
        <a>
          <xsl:attribute name="href">
            <xsl:value-of select="$href" />
          </xsl:attribute>
          <xsl:value-of select="string(NUtil:GetName(ancestor::ndoc:assembly/@name, $cref))" />
        </a>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- -->
  <xsl:template name="value">
    <xsl:param name="type" />
    <!-- Get namespace -->
    <xsl:variable name="namespace">
      <xsl:value-of select="concat(../../@name, '.')" />
    </xsl:variable>
    <!-- If type contains the namespace, remove namespace -->
    <xsl:choose>
      <xsl:when test="contains($type, $namespace)">
        <xsl:value-of select="substring-after($type, $namespace)" />
      </xsl:when>
      <!-- Otherwise just write the type -->
      <xsl:otherwise>
        <xsl:call-template name="csharp-type">
          <xsl:with-param name="runtime-type" select="$type" />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- HTML Head -->
  <xsl:template name="html-head">
    <xsl:param name="title" />
    <head>
      <meta http-equiv="Content-Type" content="{NUtil:GetContentType()}" />
      <meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5" />
      <title>
        <xsl:value-of select="$title" />
      </title>
      <xml></xml>
      <link rel="stylesheet" type="text/css" href="MSDN.css" />
      <xsl:apply-templates select="/ndoc:ndoc" mode="header-section" />
    </head>
  </xsl:template>
  <!-- Title row -->
  <xsl:template name="title-row">
    <xsl:param name="type-name" />
    <div id="nsbanner">
      <xsl:variable name="headerHtml" select="NHtmlProvider:GetHeaderHtml(string($type-name))" />
      <xsl:choose>
        <xsl:when test="$headerHtml=''">
          <div id="bannerrow1">
            <table class="bannerparthead" cellspacing="0">
              <tr id="hdr">
                <td class="runninghead">
                  <xsl:value-of select="$ndoc-title" />
                </td>
                <td class="product"></td>
              </tr>
            </table>
          </div>
          <div id="TitleRow">
            <h1 class="dtH1">
              <xsl:value-of select="$type-name" />
            </h1>
          </div>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$headerHtml" disable-output-escaping="yes" />
        </xsl:otherwise>
      </xsl:choose>
      <xsl:apply-templates select="ndoc:documentation/node()" mode="title-row" />
    </div>
  </xsl:template>
  <!-- Footer row -->
  <xsl:template name="footer-row">
    <xsl:param name="type-name" />
    <xsl:variable name="assembly-name">
      <xsl:value-of select="ancestor-or-self::ndoc:assembly/./@name" />
    </xsl:variable>
    <xsl:variable name="assembly-version">
      <xsl:value-of select="ancestor-or-self::ndoc:assembly/./@version" />
    </xsl:variable>
    <xsl:variable name="footerHtml" select="NHtmlProvider:GetFooterHtml(string($assembly-name), string($assembly-version), string($type-name))" />
    <xsl:variable name="copyright-rtf">
      <xsl:call-template name="copyright-notice" />
    </xsl:variable>
    <xsl:variable name="version-rtf">
      <xsl:call-template name="generated-from-assembly-version" />
    </xsl:variable>
    <xsl:if test="string($copyright-rtf) or string($version-rtf) or string($footerHtml) or /ndoc:ndoc/ndoc:feedbackEmail">
      <hr />
      <div id="footer">
        <xsl:apply-templates select="/ndoc:ndoc/ndoc:feedbackEmail">
          <xsl:with-param name="page" select="$type-name" />
        </xsl:apply-templates>
        <xsl:choose>
          <xsl:when test="$footerHtml=''">
            <p>
              <xsl:copy-of select="$copyright-rtf" />
            </p>
            <p>
              <xsl:copy-of select="$version-rtf" />
            </p>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$footerHtml" disable-output-escaping="yes" />
          </xsl:otherwise>
        </xsl:choose>
        <xsl:apply-templates select="ndoc:documentation/node()" mode="footer-row" />
      </div>
    </xsl:if>
  </xsl:template>
  <!-- Preliminary section -->
  <xsl:template name="preliminary-section">
    <p class="topicstatus">
      <xsl:choose>
        <xsl:when test="ndoc:documentation/ndoc:preliminary[text()]">
          <xsl:value-of select="ndoc:documentation/ndoc:preliminary" />
        </xsl:when>
        <xsl:when test="ancestor::node()/ndoc:documentation/ndoc:preliminary[text()]">
          <xsl:value-of select="ancestor::node()/ndoc:documentation/ndoc:preliminary" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>[This is preliminary documentation and subject to change.]</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </p>
    <xsl:apply-templates select="ndoc:documentation/node()" mode="preliminary-section" />
  </xsl:template>
  <!-- Copyright notice -->
  <xsl:template name="copyright-notice">
    <xsl:variable name="copyright-text">
      <xsl:value-of select="/ndoc:ndoc/ndoc:copyright/@text" />
    </xsl:variable>
    <xsl:variable name="copyright-href">
      <xsl:value-of select="/ndoc:ndoc/ndoc:copyright/@href" />
    </xsl:variable>
    <xsl:if test="$copyright-text != ''">
      <a>
        <xsl:if test="$copyright-href != ''">
          <xsl:attribute name="href">
            <xsl:value-of select="$copyright-href" />
          </xsl:attribute>
        </xsl:if>
        <xsl:value-of select="/ndoc:ndoc/ndoc:copyright/@text" />
      </a>
    </xsl:if>
  </xsl:template>
  <!-- Generated from assembly version -->
  <xsl:template name="generated-from-assembly-version">
    <xsl:variable name="assembly-name">
      <xsl:value-of select="ancestor-or-self::ndoc:assembly/./@name" />
    </xsl:variable>
    <xsl:variable name="assembly-version">
      <xsl:value-of select="ancestor-or-self::ndoc:assembly/./@version" />
    </xsl:variable>
    <xsl:if test="$assembly-version != ''">
      <xsl:text>Generated from assembly </xsl:text>
      <xsl:value-of select="$assembly-name" />
      <xsl:text> [</xsl:text>
      <xsl:value-of select="$assembly-version" />
      <xsl:text>]</xsl:text>
    </xsl:if>
  </xsl:template>
  <!-- Operator name -->
  <xsl:template name="operator-name">
    <xsl:param name="name" />
    <xsl:param name="from" />
    <xsl:param name="to" />
    <xsl:choose>
      <xsl:when test="$name='op_Decrement'">Decrement Operator</xsl:when>
      <xsl:when test="$name='op_Increment'">Increment Operator</xsl:when>
      <xsl:when test="$name='op_UnaryNegation'">Unary Negation Operator</xsl:when>
      <xsl:when test="$name='op_UnaryPlus'">Unary Plus Operator</xsl:when>
      <xsl:when test="$name='op_LogicalNot'">Logical Not Operator</xsl:when>
      <xsl:when test="$name='op_True'">True Operator</xsl:when>
      <xsl:when test="$name='op_False'">False Operator</xsl:when>
      <xsl:when test="$name='op_AddressOf'">Address Of Operator</xsl:when>
      <xsl:when test="$name='op_OnesComplement'">Ones Complement Operator</xsl:when>
      <xsl:when test="$name='op_PointerDereference'">Pointer Dereference Operator</xsl:when>
      <xsl:when test="$name='op_Addition'">Addition Operator</xsl:when>
      <xsl:when test="$name='op_Subtraction'">Subtraction Operator</xsl:when>
      <xsl:when test="$name='op_Multiply'">Multiplication Operator</xsl:when>
      <xsl:when test="$name='op_Division'">Division Operator</xsl:when>
      <xsl:when test="$name='op_Modulus'">Modulus Operator</xsl:when>
      <xsl:when test="$name='op_ExclusiveOr'">Exclusive Or Operator</xsl:when>
      <xsl:when test="$name='op_BitwiseAnd'">Bitwise And Operator</xsl:when>
      <xsl:when test="$name='op_BitwiseOr'">Bitwise Or Operator</xsl:when>
      <xsl:when test="$name='op_LogicalAnd'">LogicalAnd Operator</xsl:when>
      <xsl:when test="$name='op_LogicalOr'">Logical Or Operator</xsl:when>
      <xsl:when test="$name='op_Assign'">Assignment Operator</xsl:when>
      <xsl:when test="$name='op_LeftShift'">Left Shift Operator</xsl:when>
      <xsl:when test="$name='op_RightShift'">Right Shift Operator</xsl:when>
      <xsl:when test="$name='op_SignedRightShift'">Signed Right Shift Operator</xsl:when>
      <xsl:when test="$name='op_UnsignedRightShift'">Unsigned Right Shift Operator</xsl:when>
      <xsl:when test="$name='op_Equality'">Equality Operator</xsl:when>
      <xsl:when test="$name='op_GreaterThan'">Greater Than Operator</xsl:when>
      <xsl:when test="$name='op_LessThan'">Less Than Operator</xsl:when>
      <xsl:when test="$name='op_Inequality'">Inequality Operator</xsl:when>
      <xsl:when test="$name='op_GreaterThanOrEqual'">Greater Than Or Equal Operator</xsl:when>
      <xsl:when test="$name='op_LessThanOrEqual'">Less Than Or Equal Operator</xsl:when>
      <xsl:when test="$name='op_UnsignedRightShiftAssignment'">Unsigned Right Shift Assignment Operator</xsl:when>
      <xsl:when test="$name='op_MemberSelection'">Member Selection Operator</xsl:when>
      <xsl:when test="$name='op_RightShiftAssignment'">Right Shift Assignment Operator</xsl:when>
      <xsl:when test="$name='op_MultiplicationAssignment'">Multiplication Assignment Operator</xsl:when>
      <xsl:when test="$name='op_PointerToMemberSelection'">Pointer To Member Selection Operator</xsl:when>
      <xsl:when test="$name='op_SubtractionAssignment'">Subtraction Assignment Operator</xsl:when>
      <xsl:when test="$name='op_ExclusiveOrAssignment'">Exclusive Or Assignment Operator</xsl:when>
      <xsl:when test="$name='op_LeftShiftAssignment'">Left Shift Assignment Operator</xsl:when>
      <xsl:when test="$name='op_ModulusAssignment'">Modulus Assignment Operator</xsl:when>
      <xsl:when test="$name='op_AdditionAssignment'">Addition Assignment Operator</xsl:when>
      <xsl:when test="$name='op_BitwiseAndAssignment'">Bitwise And Assignment Operator</xsl:when>
      <xsl:when test="$name='op_BitwiseOrAssignment'">Bitwise Or Assignment Operator</xsl:when>
      <xsl:when test="$name='op_Comma'">Comma Operator</xsl:when>
      <xsl:when test="$name='op_DivisionAssignment'">Division Assignment Operator</xsl:when>
      <!-- If the operator is an implicit conversion operator -->
      <xsl:when test="$name='op_Implicit'">
        <xsl:text>Implicit </xsl:text>
        <!-- Remove namespace -->
        <xsl:call-template name="strip-namespace">
          <xsl:with-param name="name" select="$from" />
        </xsl:call-template>
        <xsl:text> to </xsl:text>
        <!-- Remove namespace -->
        <xsl:call-template name="strip-namespace">
          <xsl:with-param name="name" select="$to" />
        </xsl:call-template>
        <xsl:text> Conversion</xsl:text>
      </xsl:when>
      <!-- If the operator is an explicit conversion operator -->
      <xsl:when test="$name='op_Explicit'">
        <xsl:text>Explicit </xsl:text>
        <!-- Remove namespace -->
        <xsl:call-template name="strip-namespace">
          <xsl:with-param name="name" select="$from" />
        </xsl:call-template>
        <xsl:text> to </xsl:text>
        <!-- Remove namespace -->
        <xsl:call-template name="strip-namespace">
          <xsl:with-param name="name" select="$to" />
        </xsl:call-template>
        <xsl:text> Conversion</xsl:text>
      </xsl:when>
      <!-- Otherwise unknown operator -->
      <xsl:otherwise>ERROR</xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- C# Operator name -->
  <xsl:template name="csharp-operator-name">
    <xsl:param name="name" />
    <xsl:choose>
      <xsl:when test="$name='op_UnaryPlus'">operator +</xsl:when>
      <xsl:when test="$name='op_UnaryNegation'">operator -</xsl:when>
      <xsl:when test="$name='op_LogicalNot'">operator !</xsl:when>
      <xsl:when test="$name='op_OnesComplement'">operator ~</xsl:when>
      <xsl:when test="$name='op_Increment'">operator ++</xsl:when>
      <xsl:when test="$name='op_Decrement'">operator --</xsl:when>
      <xsl:when test="$name='op_True'">operator true</xsl:when>
      <xsl:when test="$name='op_False'">operator false</xsl:when>
      <xsl:when test="$name='op_Addition'">operator +</xsl:when>
      <xsl:when test="$name='op_Subtraction'">operator -</xsl:when>
      <xsl:when test="$name='op_Multiply'">operator *</xsl:when>
      <xsl:when test="$name='op_Division'">operator /</xsl:when>
      <xsl:when test="$name='op_Modulus'">operator %</xsl:when>
      <xsl:when test="$name='op_BitwiseAnd'">operator &amp;</xsl:when>
      <xsl:when test="$name='op_BitwiseOr'">operator |</xsl:when>
      <xsl:when test="$name='op_ExclusiveOr'">operator ^</xsl:when>
      <xsl:when test="$name='op_LeftShift'">operator &lt;&lt;</xsl:when>
      <xsl:when test="$name='op_RightShift'">operator >></xsl:when>
      <xsl:when test="$name='op_Equality'">operator ==</xsl:when>
      <xsl:when test="$name='op_Inequality'">operator !=</xsl:when>
      <xsl:when test="$name='op_LessThan'">operator &lt;</xsl:when>
      <xsl:when test="$name='op_GreaterThan'">operator ></xsl:when>
      <xsl:when test="$name='op_LessThanOrEqual'">operator &lt;=</xsl:when>
      <xsl:when test="$name='op_GreaterThanOrEqual'">operator >=</xsl:when>
      <xsl:otherwise>ERROR</xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- Get namespace -->
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
  <!-- Strip namespace -->
  <xsl:template name="strip-namespace">
    <xsl:param name="name" />
    <xsl:choose>
      <xsl:when test="contains($name, '`')">
        <xsl:call-template name="strip-namespace">
          <xsl:with-param name="name" select="substring-before($name, '`')" />
        </xsl:call-template>
      </xsl:when>
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
  <!-- Requirements section -->
  <xsl:template name="requirements-section">
    <xsl:if test="ndoc:documentation/ndoc:permission">
      <h4 class="dtH4">Requirements</h4>
      <p>
        <b>.NET Framework Security: </b>
        <ul class="permissions">
          <xsl:for-each select="ndoc:documentation/ndoc:permission">
            <li>
              <a>
                <xsl:attribute name="href">
                  <xsl:call-template name="get-filename-for-type-name">
                    <xsl:with-param name="type-name" select="substring-after(@cref, 'T:')" />
                  </xsl:call-template>
                </xsl:attribute>
                <xsl:value-of select="substring-after(@cref, 'T:')" />
              </a>
              <xsl:text>&#160;</xsl:text>
              <xsl:apply-templates mode="slashdoc" />
            </li>
          </xsl:for-each>
        </ul>
      </p>
      <xsl:apply-templates select="ndoc:documentation/node()" mode="requirements-section" />
    </xsl:if>
  </xsl:template>
  <!-- Feedback email -->
  <xsl:template match="ndoc:feedbackEmail">
    <xsl:param name="page" />
    <xsl:variable name="href">
      <xsl:text>mailto:</xsl:text>
      <xsl:value-of select="." />
      <xsl:text>?subject=</xsl:text>
      <xsl:value-of select="$ndoc-title" />
      <xsl:text> Documentation Feedback: </xsl:text>
      <xsl:value-of select="$page" />
    </xsl:variable>
    <p>
      <a href="{NUtil:Replace( $href, ' ', '%20' )}">
        <xsl:text>Send comments on this topic.</xsl:text>
      </a>
    </p>
  </xsl:template>
</xsl:stylesheet>
