<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <!-- Document attributes? -->
  <xsl:param name="ndoc-document-attributes" />
  <!-- Which attributes shoulde be documented -->
  <xsl:param name="ndoc-documented-attributes" />
  <!-- C# Type syntax -->
  <xsl:template name="cs-type-syntax">
    <div class="syntax">
      <!-- VB syntax? -->
      <xsl:if test="$ndoc-vb-syntax">
        <span class="lang">[C#]</span>
      </xsl:if>
      <!-- Write attributes -->
      <xsl:call-template name="attributes" />
      <div>
        <!-- Does this type hide another type -->
        <xsl:if test="@hiding">
          <xsl:text>new&#160;</xsl:text>
        </xsl:if>
        <!-- Write type access modifier -->
        <xsl:call-template name="type-access">
          <xsl:with-param name="access" select="@access" />
          <xsl:with-param name="type" select="local-name()" />
        </xsl:call-template>
        <xsl:text>&#160;</xsl:text>
        <!-- Is this abstract? -->
        <xsl:if test="local-name() != 'interface' and @abstract = 'true'">abstract&#160;</xsl:if>
        <!-- Is this sealed? -->
        <xsl:if test="@sealed = 'true'">
          <xsl:text>sealed&#160;</xsl:text>
        </xsl:if>
        <xsl:choose>
          <!-- Is this a structure? -->
          <xsl:when test="local-name()='structure'">
            <xsl:text>struct</xsl:text>
          </xsl:when>
          <!-- Is this a enumeration? -->
          <xsl:when test="local-name()='enumeration'">
            <xsl:text>enum</xsl:text>
          </xsl:when>
          <!-- Otherwise just write the localname -->
          <xsl:otherwise>
            <xsl:value-of select="local-name()" />
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>&#160;</xsl:text>
        <!-- Is this a delegate? -->
        <xsl:if test="local-name()='delegate'">
          <!-- Writes datatype -->
          <xsl:call-template name="get-datatype">
            <xsl:with-param name="datatype" select="@returnType" />
          </xsl:call-template>
          <xsl:text>&#160;</xsl:text>
        </xsl:if>
        <!-- Writes name -->
        <xsl:value-of select="@displayName" />
        <!-- Write generic constraints if there are any -->
        <xsl:if test="@genericconstraints">
          <xsl:text>&#160;</xsl:text>
          <xsl:value-of select="@genericconstraints"/>
        </xsl:if>
        <!-- Is not a enumeration and not a delegate? -->
        <xsl:if test="local-name() != 'enumeration' and local-name() != 'delegate'">
          <!-- Handel derivation -->
          <xsl:call-template name="derivation" />
        </xsl:if>
        <!-- If this is a delegate -->
        <xsl:if test="local-name() = 'delegate'">
          <!-- Write parameters -->
          <xsl:call-template name="parameters">
            <xsl:with-param name="version">long</xsl:with-param>
            <xsl:with-param name="namespace-name" select="../@name" />
          </xsl:call-template>
        </xsl:if>
      </div>
    </div>
  </xsl:template>
  <!-- -->
  <xsl:template name="derivation">
    <!-- Is this a derived class? Either from a class or an interface -->
    <xsl:if test="@baseType!='' or implements[not(@inherited)]">
      <b>
        <xsl:text> : </xsl:text>
        <xsl:if test="@baseType!=''">
          <a>
            <!-- Write link to baseclass -->
            <xsl:attribute name="href">
              <xsl:call-template name="get-filename-for-type-name">
                <xsl:with-param name="type-name" select="./base/@type" />
              </xsl:call-template>
            </xsl:attribute>
            <xsl:call-template name="get-datatype">
              <xsl:with-param name="datatype" select="@baseType" />
            </xsl:call-template>
          </a>
          <!-- If we also implements an interface -->
          <xsl:if test="implements[not(@inherited)]">
            <xsl:text>, </xsl:text>
          </xsl:if>
        </xsl:if>
        <!-- Iterate through all implemented interfaces -->
        <xsl:for-each select="implements[not(@inherited)]">
          <a>
            <!-- Write link to interface -->
            <xsl:attribute name="href">
              <xsl:call-template name="get-filename-for-type-name">
                <xsl:with-param name="type-name" select="@type" />
              </xsl:call-template>
            </xsl:attribute>
            <xsl:call-template name="get-datatype">
              <xsl:with-param name="datatype" select="@type" />
            </xsl:call-template>
          </a>
          <!-- If there are more interfaces implemented -->
          <xsl:if test="position()!=last()">
            <xsl:text>, </xsl:text>
          </xsl:if>
        </xsl:for-each>
      </b>
    </xsl:if>
  </xsl:template>
  <!-- C# Member syntax -->
  <xsl:template name="cs-member-syntax">
    <div class="syntax">
      <!-- If VB syntax also should be written -->
      <xsl:if test="$ndoc-vb-syntax">
        <span class="lang">[C#]</span>
        <br />
      </xsl:if>
      <!-- Write attributes -->
      <xsl:call-template name="attributes" />
      <!-- If this member hides another member -->
      <xsl:if test="@hiding">
        <xsl:text>new&#160;</xsl:text>
      </xsl:if>
      <xsl:if test="not(parent::interface or @interface)">
        <!-- If the member is not a constructor or is not static -->
        <xsl:if test="(local-name()!='constructor') or (@contract!='Static')">
          <!-- Write metod accessmodifier -->
          <xsl:call-template name="method-access">
            <xsl:with-param name="access" select="@access" />
          </xsl:call-template>
          <xsl:text>&#160;</xsl:text>
        </xsl:if>
        <!-- If the member is not final or normal -->
        <xsl:if test="@contract and @contract!='Normal' and @contract!='Final'">
          <!-- Write contract -->
          <xsl:call-template name="contract">
            <xsl:with-param name="contract" select="@contract" />
          </xsl:call-template>
          <xsl:text>&#160;</xsl:text>
        </xsl:if>
      </xsl:if>
      <xsl:choose>
        <!-- If this a constructor -->
        <xsl:when test="local-name()='constructor'">
          <xsl:value-of select="../@displayName" />
        </xsl:when>
        <xsl:otherwise>
          <!-- If the name is different from op_Explicit and op_Implicit -->
          <xsl:if test="@name != 'op_Explicit' and @name != 'op_Implicit'">
            <!-- Write link to datatype -->
            <a>
              <xsl:attribute name="href">
                <xsl:call-template name="get-filename-for-type-name">
                  <xsl:with-param name="type-name" select="@returnType" />
                </xsl:call-template>
              </xsl:attribute>
              <xsl:call-template name="get-datatype">
                <xsl:with-param name="datatype" select="@returnType" />
              </xsl:call-template>
            </a>
            <xsl:text>&#160;</xsl:text>
          </xsl:if>
          <xsl:choose>
            <!-- If localname is operator -->
            <xsl:when test="local-name()='operator'">
              <xsl:choose>
                <!-- If this is a explicit conversion operator -->
                <xsl:when test="@name='op_Explicit'">
                  <xsl:text>explicit operator </xsl:text>
                  <!-- Write link to datatype -->
                  <a>
                    <xsl:attribute name="href">
                      <xsl:call-template name="get-filename-for-type-name">
                        <xsl:with-param name="type-name" select="@returnType" />
                      </xsl:call-template>
                    </xsl:attribute>
                    <xsl:call-template name="get-datatype">
                      <xsl:with-param name="datatype" select="@returnType" />
                    </xsl:call-template>
                  </a>
                </xsl:when>
                <!-- If this is a implicit conversion operator -->
                <xsl:when test="@name='op_Implicit'">
                  <xsl:text>implicit operator </xsl:text>
                  <!-- Write link to datatype -->
                  <a>
                    <xsl:attribute name="href">
                      <xsl:call-template name="get-filename-for-type-name">
                        <xsl:with-param name="type-name" select="@returnType" />
                      </xsl:call-template>
                    </xsl:attribute>
                    <xsl:call-template name="get-datatype">
                      <xsl:with-param name="datatype" select="@returnType" />
                    </xsl:call-template>
                  </a>
                </xsl:when>
                <!-- Otherwise write C# operator name -->
                <xsl:otherwise>
                  <xsl:call-template name="csharp-operator-name">
                    <xsl:with-param name="name" select="@name" />
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <!-- Hvis det ikke er en operator write the name -->
            <xsl:otherwise>
              <xsl:value-of select="@name" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:otherwise>
      </xsl:choose>
      <!-- Write parameters -->
      <xsl:call-template name="parameters">
        <xsl:with-param name="version">long</xsl:with-param>
        <xsl:with-param name="namespace-name" select="../../@name" />
      </xsl:call-template>
    </div>
  </xsl:template>
  <!-- C# Member Syntax 2 -->
  <xsl:template name="member-syntax2">
    <!-- If this member hides another member -->
    <xsl:if test="@hiding">
      <xsl:text>new&#160;</xsl:text>
    </xsl:if>
    <!-- If the does not implement an interface -->
    <xsl:if test="not(parent::interface)">
      <!-- If this is not a constructor or the contract is not static -->
      <xsl:if test="(local-name()!='constructor') or (@contract!='Static')">
        <!-- Write method accessmodifier -->
        <xsl:call-template name="method-access">
          <xsl:with-param name="access" select="@access" />
        </xsl:call-template>
        <xsl:text>&#160;</xsl:text>
      </xsl:if>
      <!-- If the member has a contract and it is not final or normal -->
      <xsl:if test="@contract and @contract!='Normal' and @contract!='Final'">
        <!-- Write contract -->
        <xsl:call-template name="contract">
          <xsl:with-param name="contract" select="@contract" />
        </xsl:call-template>
        <xsl:text>&#160;</xsl:text>
      </xsl:if>
    </xsl:if>
    <xsl:choose>
      <!-- If the member is a constructor -->
      <xsl:when test="local-name()='constructor'">
        <xsl:value-of select="../@name" />
      </xsl:when>
      <!-- If the member is a operator -->
      <xsl:when test="local-name()='operator'">
        <!-- Write datatype -->
        <xsl:call-template name="get-datatype">
          <xsl:with-param name="datatype" select="@returnType" />
        </xsl:call-template>
        <xsl:text>&#160;</xsl:text>
        <!-- Write operator name-->
        <xsl:call-template name="operator-name">
          <xsl:with-param name="name">
            <xsl:value-of select="@name" />
          </xsl:with-param>
          <xsl:with-param name="from">
            <xsl:value-of select="parameter/@type" />
          </xsl:with-param>
          <xsl:with-param name="to">
            <xsl:value-of select="@returnType" />
          </xsl:with-param>
        </xsl:call-template>
      </xsl:when>
      <!-- Otherwise write datatype and name of the member -->
      <xsl:otherwise>
        <xsl:call-template name="get-datatype">
          <xsl:with-param name="datatype" select="@returnType" />
        </xsl:call-template>
        <xsl:text>&#160;</xsl:text>
        <xsl:value-of select="@name" />
      </xsl:otherwise>
    </xsl:choose>
    <!-- If the member is not a conversion operator, write parameters in short mode -->
    <xsl:if test="@name!='op_Implicit' and @name!='op_Explicit'">
      <xsl:call-template name="parameters">
        <xsl:with-param name="version">short</xsl:with-param>
        <xsl:with-param name="namespace-name" select="../../@name" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
  <!-- Field or event syntax -->
  <xsl:template name="cs-field-or-event-syntax">
    <div class="syntax">
      <!-- Should VB syntax also be written -->
      <xsl:if test="$ndoc-vb-syntax">
        <span class="lang">[C#]</span>
        <br />
      </xsl:if>
      <!-- Write attributes -->
      <xsl:call-template name="attributes" />
      <!-- If it hides annother field or event -->
      <xsl:if test="@hiding">
        <xsl:text>new&#160;</xsl:text>
      </xsl:if>
      <!-- If class does not implement an interface -->
      <xsl:if test="not(parent::interface)">
        <!-- Write method accessmodifier -->
        <xsl:call-template name="method-access">
          <xsl:with-param name="access" select="@access" />
        </xsl:call-template>
        <xsl:text>&#160;</xsl:text>
      </xsl:if>
      <!-- If contract is static -->
      <xsl:if test="@contract='Static'">
        <xsl:choose>
          <!-- If this is a constant -->
          <xsl:when test="@literal='true'">
            <xsl:text>const&#160;</xsl:text>
          </xsl:when>
          <!-- Otherwise this is just static -->
          <xsl:otherwise>
            <xsl:text>static&#160;</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
      <!-- If this is readonly -->
      <xsl:if test="@initOnly='true'">
        <xsl:text>readonly&#160;</xsl:text>
      </xsl:if>
      <!-- If this is an event -->
      <xsl:if test="local-name() = 'event'">
        <xsl:text>event&#160;</xsl:text>
      </xsl:if>
      <!-- Write link to datatype -->
      <xsl:choose>
        <!-- If this is not a generic type-->
        <xsl:when test="not(genericargument) or local-name() = 'event'">
          <a>
            <xsl:attribute name="href">
              <xsl:call-template name="get-filename-for-type-name">
                <xsl:with-param name="type-name" select="@type" />
              </xsl:call-template>
            </xsl:attribute>
            <xsl:call-template name="get-datatype">
              <xsl:with-param name="datatype" select="@type" />
            </xsl:call-template>
          </a>
        </xsl:when>
        <!-- If this is a generic type-->
        <xsl:otherwise>
          <a>
            <xsl:attribute name="href">
              <xsl:call-template name="get-filename-for-type-name">
                <xsl:with-param name="type-name" select="@type" />
              </xsl:call-template>
            </xsl:attribute>
            <xsl:call-template name="get-datatype">
              <xsl:with-param name="datatype" select="substring-before(@type, '{')" />
            </xsl:call-template>
          </a>
          <xsl:text>&lt;</xsl:text>
          <xsl:call-template name="generic-field"/>
          <xsl:text>&gt;</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:text>&#160;</xsl:text>
      <!-- Write name -->
      <xsl:value-of select="@name" />
      <!-- If this is a constant write assigned value-->
      <xsl:if test="@literal='true'">
        <xsl:text> = </xsl:text>
        <!-- If the value is a String write " -->
        <xsl:if test="@type='System.String'">
          <xsl:text>"</xsl:text>
        </xsl:if>
        <xsl:value-of select="@value" />
        <!-- If the value is a String write " -->
        <xsl:if test="@type='System.String'">
          <xsl:text>"</xsl:text>
        </xsl:if>
      </xsl:if>
      <xsl:text>;</xsl:text>
    </div>
  </xsl:template>
  <!-- C# Property Syntax -->
  <xsl:template name="cs-property-syntax">
    <xsl:param name="indent" select="true()" />
    <xsl:param name="display-names" select="true()" />
    <xsl:param name="link-types" select="true()" />
    <!-- Write attributes -->
    <xsl:call-template name="attributes" />
    <!-- If this property hides another -->
    <xsl:if test="@hiding">
      <xsl:text>new&#160;</xsl:text>
    </xsl:if>
    <!-- If the class does not implement an interface -->
    <xsl:if test="not(parent::interface)">
      <!-- Write accessmodifier -->
      <xsl:call-template name="method-access">
        <xsl:with-param name="access" select="@access" />
      </xsl:call-template>
      <xsl:text>&#160;</xsl:text>
    </xsl:if>
    <!-- If this is static -->
    <xsl:if test="@contract='Static'">
      <xsl:text>static&#160;</xsl:text>
    </xsl:if>
    <!-- If the class does not implement an interface -->
    <xsl:if test="not(parent::interface)">
      <!-- If contract is not normal, static or final -->
      <xsl:if test="@contract!='Normal' and @contract!='Static' and @contract!='Final'">
        <!-- Write contract -->
        <xsl:call-template name="contract">
          <xsl:with-param name="contract" select="@contract" />
        </xsl:call-template>
        <xsl:text>&#160;</xsl:text>
      </xsl:if>
    </xsl:if>
    <xsl:choose>
      <!-- If we shoule write links to types -->
      <xsl:when test="$link-types">
        <a>
          <xsl:attribute name="href">
            <xsl:call-template name="get-filename-for-type-name">
              <xsl:with-param name="type-name" select="@type" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:call-template name="value">
            <xsl:with-param name="type" select="@type" />
          </xsl:call-template>
        </a>
      </xsl:when>
      <!-- Otherwise just write the type of the property -->
      <xsl:otherwise>
        <xsl:call-template name="value">
          <xsl:with-param name="type" select="@type" />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text>&#160;</xsl:text>
    <xsl:choose>
      <!-- If the property has parameters -->
      <xsl:when test="parameter">
        <xsl:text>this[</xsl:text>
        <xsl:if test="$indent">
          <br />
        </xsl:if>
        <!-- Write all parameters -->
        <xsl:for-each select="parameter">
          <xsl:if test="$indent">
            <xsl:text>&#160;&#160;&#160;</xsl:text>
          </xsl:if>
          <xsl:choose>
            <!-- If we should write links to types -->
            <xsl:when test="$link-types">
              <a>
                <xsl:attribute name="href">
                  <xsl:call-template name="get-filename-for-type-name">
                    <xsl:with-param name="type-name" select="@type" />
                  </xsl:call-template>
                </xsl:attribute>
                <xsl:call-template name="csharp-type">
                  <xsl:with-param name="runtime-type" select="@type" />
                </xsl:call-template>
              </a>
            </xsl:when>
            <!-- Otherwise just write the type name -->
            <xsl:otherwise>
              <xsl:call-template name="csharp-type">
                <xsl:with-param name="runtime-type" select="@type" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
          <!-- If we should write the parameters names -->
          <xsl:if test="$display-names">
            <xsl:text>&#160;</xsl:text>
            <i>
              <xsl:value-of select="@name" />
            </i>
          </xsl:if>
          <!-- If have not written all parameters -->
          <xsl:if test="position() != last()">
            <xsl:text>,&#160;</xsl:text>
            <xsl:if test="$indent">
              <br />
            </xsl:if>
          </xsl:if>
        </xsl:for-each>
        <xsl:if test="$indent">
          <br />
        </xsl:if>
        <xsl:text>]</xsl:text>
      </xsl:when>
      <!-- Otherwise just write the name of the property-->
      <xsl:otherwise>
        <xsl:value-of select="@name" />
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text>&#160;{</xsl:text>
    <!-- If property has a get -->
    <xsl:if test="@get='true'">
      <xsl:text>get;</xsl:text>
      <!-- If property has a set -->
      <xsl:if test="@set='true'">
        <xsl:text>&#160;</xsl:text>
      </xsl:if>
    </xsl:if>
    <!-- If property has a set -->
    <xsl:if test="@set='true'">
      <xsl:text>set;</xsl:text>
    </xsl:if>
    <xsl:text>}</xsl:text>
  </xsl:template>
  <!-- Parameters -->
  <xsl:template name="parameters">
    <xsl:param name="version" />
    <xsl:param name="namespace-name" />
    <!-- Write parameters -->
    <xsl:text>(</xsl:text>
    <xsl:if test="parameter">
      <xsl:for-each select="parameter">
        <!-- If this should be written in long mode -->
        <xsl:if test="$version='long'">
          <br />
          <xsl:text>&#160;&#160;&#160;</xsl:text>
        </xsl:if>
        <!-- Write modifiers -->
        <xsl:choose>
          <xsl:when test="@direction = 'ref'">ref&#160;</xsl:when>
          <xsl:when test="@direction = 'out'">out&#160;</xsl:when>
          <xsl:when test="@isParamArray = 'true'">params&#160;</xsl:when>
        </xsl:choose>
        <xsl:choose>
          <!-- If this should be written in long mode, write a link to the datatype -->
          <xsl:when test="$version='long'">
            <a>
              <xsl:attribute name="href">
                <xsl:call-template name="get-filename-for-type-name">
                  <xsl:with-param name="type-name" select="@type" />
                </xsl:call-template>
              </xsl:attribute>
              <xsl:call-template name="get-datatype">
                <xsl:with-param name="datatype" select="@type" />
              </xsl:call-template>
            </a>
          </xsl:when>
          <!-- Otherwise just write the datatype -->
          <xsl:otherwise>
            <xsl:call-template name="get-datatype">
              <xsl:with-param name="datatype" select="@type" />
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
        <!-- If this should be written in long mode, write name i italic -->
        <xsl:if test="$version='long'">
          <xsl:text>&#160;</xsl:text>
          <i>
            <xsl:value-of select="@name" />
          </i>
        </xsl:if>
        <!-- Has all parameters been written -->
        <xsl:if test="position()!= last()">
          <xsl:text>,</xsl:text>
        </xsl:if>
      </xsl:for-each>
      <!-- If this should be written in long mode -->
      <xsl:if test="$version='long'">
        <br />
      </xsl:if>
    </xsl:if>
    <xsl:text>);</xsl:text>
  </xsl:template>
  <!--  -->
  <xsl:template name="get-datatype">
    <xsl:param name="datatype" />
    <!-- Removes namespace -->
    <xsl:call-template name="strip-namespace">
      <xsl:with-param name="name">
        <!-- Gets the C# type (System.String is string)-->
        <xsl:call-template name="csharp-type">
          <xsl:with-param name="runtime-type" select="$datatype" />
        </xsl:call-template>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>
  <!-- -->
  <!-- member.xslt is using this for title and h1.  should try and use parameters template above. -->
  <xsl:template name="get-param-list">
    <xsl:text>(</xsl:text>
    <xsl:for-each select="parameter">
      <xsl:call-template name="strip-namespace">
        <xsl:with-param name="name" select="@type" />
      </xsl:call-template>
      <xsl:if test="position()!=last()">
        <xsl:text>, </xsl:text>
      </xsl:if>
    </xsl:for-each>
    <xsl:text>)</xsl:text>
  </xsl:template>
  <!-- -->
  <!-- Attributes -->
  <xsl:template name="attributes">
    <!-- Should attributes be documented? -->
    <xsl:if test="$ndoc-document-attributes">
      <!-- Check if this is an attribute, and if so iterate over all of them -->
      <xsl:if test="attribute">
        <xsl:for-each select="attribute">
          <div class="attribute">
            <!-- Attribute template called with the name of the attribute -->
            <xsl:call-template name="attribute">
              <xsl:with-param name="attname" select="@name" />
            </xsl:call-template>
          </div>
        </xsl:for-each>
      </xsl:if>
    </xsl:if>
  </xsl:template>
  <!-- Attribute -->
  <xsl:template name="attribute">
    <!-- Name of attribute -->
    <xsl:param name="attname" />
    <xsl:text>[</xsl:text>
    <!-- Write target to outputstream -->
    <xsl:if test="@target">
      <xsl:value-of select="@target" /> :
    </xsl:if>
    <!-- Removes namespace and attributes -->
    <xsl:call-template name="strip-namespace-and-attribute">
      <xsl:with-param name="name" select="@name" />
    </xsl:call-template>

    <xsl:if test="count(property | field) > 0">
      <xsl:text>(</xsl:text>
      <xsl:for-each select="property | field">
        <xsl:value-of select="@name" />
        <xsl:text>=</xsl:text>
        <xsl:choose>
          <xsl:when test="@value">
            <xsl:if test="@type='System.String'">
              <xsl:text>"</xsl:text>
            </xsl:if>
            <xsl:value-of select="@value" />
            <xsl:if test="@type='System.String'">
              <xsl:text>"</xsl:text>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>**UNKNOWN**</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:if test="position()!=last()">
          <xsl:text>, </xsl:text>
        </xsl:if>
      </xsl:for-each>
      <xsl:text>)</xsl:text>
    </xsl:if>
    <xsl:text>]</xsl:text>
  </xsl:template>
  <!-- Removes namespace and attribute -->
  <xsl:template name="strip-namespace-and-attribute">
    <xsl:param name="name" />
    <xsl:choose>
      <xsl:when test="contains($name, '.')">
        <xsl:call-template name="strip-namespace-and-attribute">
          <xsl:with-param name="name" select="substring-after($name, '.')" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="substring-before(concat($name, '_____'), 'Attribute_____')" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- -->
</xsl:stylesheet>
