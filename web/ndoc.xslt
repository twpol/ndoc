<xsl:transform
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns="http://www.w3.org/1999/xhtml"
>

<xsl:output
  method="xml"
  encoding="utf-8"
  doctype-public="-//W3C//DTD XHTML 1.0 Strict//EN"
  doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"
/>

<xsl:param name="page" select="'index'"/>
<xsl:param name="title" select="'NDoc (formerly DOC.NET)'"/>

<xsl:template match="/">

<html>

<head>
  <title><xsl:value-of select="$title"/></title>
  <style type="text/css" media="screen">@import "ndoc.css";</style>
</head>

<body>

<div id="Header"><a href="http://ndoc.sourceforge.net/" title="NDoc Home">NDoc</a> : an extensible documentation generation tool for .NET developers</div>

<div id="Content">
  <h1><xsl:value-of select="$title"/></h1>

  <xsl:copy-of select="/ndoc/description[@page=$page]/*/*"/>

  <p class="images">
    <a href="http://sourceforge.net/"><img src="http://sourceforge.net/sflogo.php?group_id=13899&amp;type=1" alt="SourceForge Logo" width="88" height="31"/></a>
    <xsl:text> </xsl:text>
    <a href="http://validator.w3.org/check/referer"><img src="http://www.w3.org/Icons/valid-xhtml10" alt="Valid XHTML 1.0!" height="31" width="88"/></a>
  </p>

</div>

<div id="Menu">

  <h4>releases</h4>

  <xsl:choose>
    <xsl:when test="/ndoc/release">
      <xsl:for-each select="/ndoc/release">
        <a
          href="{@link}"
          title="Download {@name} (released on {@date})"
        >
          <xsl:value-of select="@name"/>
        </a>
        <br/>
      </xsl:for-each>
    </xsl:when>
    <xsl:otherwise>
      <p>none yet</p>
    </xsl:otherwise>
  </xsl:choose>

  <h4>developers</h4>

  <xsl:for-each select="/ndoc/developer">
    <a
      href="mailto:{@email}"
      title="Email {@short-name}"
    >
      <xsl:value-of select="@full-name"/>
    </a>
    <br/>
  </xsl:for-each>

  <h4>resources</h4>

  <a
    href="http://sourceforge.net/projects/ndoc/"
    title="Visit Our SourceForge Project Page"
  >
    <xsl:text>sourceforge</xsl:text>
  </a>
  <br/>

  <a
    href="http://nunit.sourceforge.net/"
    title="Test Your Code With NUnit!"
  >
    <xsl:text>nunit</xsl:text>
  </a>
  <br/>

  <a
    href="http://nant.sourceforge.net/"
    title="Build Your Code With NAnt!"
  >
    <xsl:text>nant</xsl:text>
  </a>
  <br/>

</div>

</body>
</html>

</xsl:template>

</xsl:transform>
