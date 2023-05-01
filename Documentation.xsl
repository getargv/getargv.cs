<?xml version="1.0" encoding="ISO-8859-1"?>
<!-- Based off of work (c)2005 by Emma Burrows -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:variable name="lowercase" select="'abcdefghijklmnopqrstuvwxyz'" />
  <xsl:variable name="uppercase" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />
  <xsl:template match="/">
    <HTML>
      <head>
        <style type="text/css">
:root {
  color-scheme: light dark;
  --fonts: ui-sans-serif, system-ui, -system-ui, -apple-system, BlinkMacSystemFont, Roboto, Helvetica, Arial, sans-serif, "Apple Color Emoji";
}

@media (prefers-color-scheme: light) {
:root {
  --link-color: rgb(54, 54, 204);
  --bg-color: #f0f0f0;
  --paper-bg: #f8f8f8;
  --shadow-color: rgba(0,0,0, 0.1);
  --edge-color: #0001;
  --text: #5b5b5b;
}
}
@media (prefers-color-scheme: dark) {
:root {
  --link-color: rgb(140, 140, 255);
  --bg-color: #080808;
  --paper-bg: #0f0f0f;
  --shadow-color: black;
  --edge-color: #fff1;
  --text: #979797;
}
}

body{
  padding: 10px 40px;
  font-family: var(--fonts);
  background-color: var(--bg-color);
  color: var(--text);
}

section {
  border-image-source: linear-gradient(to bottom left, var(--edge-color), transparent 50%);
  border-image-slice: 1 1;
  border-width: 1px 1px 0px 0px;
  border-style: solid;
  padding: 0px 15px 10px 15px;
  box-shadow: -5px 5px 5px 0px var(--shadow-color);
  margin: 10px 5px;
  background-color: var(--paper-bg);
}

a{
  text-decoration: none;
  color: var(--link-color);
}
a:visited{
  color: var(--link-color);
}
.value:before{
  content:"Value:";
  margin-right:1em;
  font-family: var(--fonts);
}
[id] > .name:before {
  margin-right:1em;
}
[id^="type-"] > .name:before {
  content:"Type:";
}
[id^="field-"] > .name:before {
  content:"Field:";
}
[id^="method-"] > .name:before {
  content:"Method:";
}
[id^="assembly-"] > .name:before {
  content:"Assembly:";
}
[id^="property-"] > .name:before {
  content:"Property:";
}

dl {
  display: grid;
  grid-template-columns: max-content auto;
}
dt {  grid-column-start: 1;  }
dd {  grid-column-start: 2;  }
        </style>
      </head>
      <BODY>
        <xsl:apply-templates select="//assembly"/>
      </BODY>
    </HTML>
  </xsl:template>

  <xsl:template match="assembly">
    <section>
      <xsl:attribute name="id">assembly-<xsl:value-of select="name" /></xsl:attribute>
      <H1 class="name"><xsl:value-of select="name"/></H1>
      <xsl:apply-templates select="//member[contains(@name,'T:')]"/>
    </section>
  </xsl:template>

  <xsl:template match="//member[contains(@name,'T:')]">

    <xsl:variable name="MemberName" select="substring-after(@name, '.')"/>
    <xsl:variable name="FullMemberName" select="substring-after(@name, ':')"/>

    <section>
      <xsl:attribute name="id">type-<xsl:value-of select="$FullMemberName" /></xsl:attribute>

      <H2 class="name"><xsl:value-of select="$MemberName"/></H2>
      <xsl:apply-templates/>

      <xsl:if test="//member[contains(@name,concat('F:',$FullMemberName))]">
        <section class="fields">
          <H3>Fields</H3>

          <xsl:for-each select="//member[contains(@name,concat('F:',$FullMemberName))]">
            <xsl:variable name="FullFieldName" select="substring-after(@name, ':')"/>
            <section>
              <xsl:attribute name="id">field-<xsl:value-of select="$FullFieldName" /></xsl:attribute>
              <H4 class="name"><xsl:value-of select="substring-after(@name, concat('F:',$FullMemberName,'.'))"/></H4>
              <xsl:apply-templates/>
            </section>
          </xsl:for-each>
        </section>
      </xsl:if>

      <xsl:if test="//member[contains(@name,concat('P:',$FullMemberName))]">
        <section class="properties">
          <H3>Properties</H3>

          <xsl:for-each select="//member[contains(@name,concat('P:',$FullMemberName))]">
            <xsl:variable name="FullPropertyName" select="substring-after(@name, ':')"/>
            <section>
              <xsl:attribute name="id">property-<xsl:value-of select="$FullPropertyName"/></xsl:attribute>
              <H4 class="name"><xsl:value-of select="substring-after(@name, concat('P:',$FullMemberName,'.'))"/></H4>
              <xsl:apply-templates/>
            </section>
          </xsl:for-each>
        </section>
      </xsl:if>

      <xsl:if test="//member[contains(@name,concat('M:',$FullMemberName))]">
        <section>
          <H3>Methods</H3>

          <xsl:for-each select="//member[contains(@name,concat('M:',$FullMemberName))]">
            <xsl:variable name="FullMethodName" select="substring-before(substring-after(@name, ':'), '(')"/>
            <section>
              <!-- If this is a constructor, display the type name (instead of "#ctor"), or display the method name -->
              <xsl:attribute name="id">method-<xsl:value-of select="$FullMethodName"/></xsl:attribute>
              <H4 class="name">
                <xsl:choose>
                  <xsl:when test="contains(@name, '#ctor')">
                    Constructor:
                    <xsl:value-of select="$MemberName"/>
                    <xsl:value-of select="substring-after(@name, '#ctor')"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="substring-after(@name, concat('M:',$FullMemberName,'.'))"/>
                  </xsl:otherwise>
                </xsl:choose>
              </H4>

              <xsl:apply-templates select="summary"/>

              <xsl:if test="count(param)!=0">
                <section class="parameters">
                  <H5>Parameters</H5>
                  <dl>
                    <xsl:apply-templates select="param"/>
                  </dl>
                </section>
              </xsl:if>

              <xsl:if test="count(returns)!=0">
                <section class="returns">
                  <H5>Return Value</H5>
                  <xsl:apply-templates select="returns"/>
                </section>
              </xsl:if>

              <xsl:if test="count(exception)!=0">
                <section class="exceptions">
                  <H5>Exceptions</H5>
                  <dl>
                    <xsl:apply-templates select="exception"/>
                  </dl>
                </section>
              </xsl:if>

              <xsl:if test="count(example)!=0">
                <section class="examples">
                  <H5>Example</H5>
                  <xsl:apply-templates select="example"/>
                </section>
              </xsl:if>

            </section>
          </xsl:for-each>
        </section>
      </xsl:if>
    </section>
  </xsl:template>

  <!-- OTHER TEMPLATES -->
  <!-- Templates for other tags -->
  <xsl:template match="c">
    <CODE><xsl:apply-templates /></CODE>
  </xsl:template>

  <xsl:template match="code">
    <PRE><xsl:apply-templates /></PRE>
  </xsl:template>

  <xsl:template match="example">
    <P><STRONG>Example: </STRONG><xsl:apply-templates /></P>
  </xsl:template>

  <xsl:template match="exception">
    <dt class="exception"><xsl:value-of select="substring-after(@cref,'T:')"/></dt>
    <dd><xsl:apply-templates /></dd>
  </xsl:template>

  <xsl:template match="include">
    <A HREF="{@file}">External file</A>
  </xsl:template>

  <xsl:template match="para">
    <P><xsl:apply-templates /></P>
  </xsl:template>

  <xsl:template match="param">
    <xsl:variable name="ParentName" select="parent::member/@name"/>
    <dt class="param">
      <xsl:attribute name="id">param-<xsl:value-of select="substring-before(substring-after($ParentName, ':'), '(')"/>-<xsl:value-of select="@name" /></xsl:attribute>
      <xsl:value-of select="@name"/>
    </dt>
    <dd><xsl:apply-templates /></dd>
  </xsl:template>

  <xsl:template match="paramref">
    <xsl:variable name="ParentName" select="ancestor::member[@name]/@name"/>
    <a class="paramref">
      <xsl:attribute name="href">#param-<xsl:value-of select="substring-before(substring-after($ParentName, ':'), '(')" />-<xsl:value-of select="@name" /></xsl:attribute>
      <xsl:value-of select="@name" />
    </a>
  </xsl:template>

  <xsl:template match="permission">
    <P><STRONG>Permission: </STRONG><EM><xsl:value-of select="@cref" /> </EM><xsl:apply-templates /></P>
  </xsl:template>

  <xsl:template match="remarks">
    <P class="remarks"><xsl:apply-templates /></P>
  </xsl:template>

  <xsl:template match="returns">
    <P class="returns">This method returns: <xsl:apply-templates /></P>
  </xsl:template>

  <xsl:template match="see[not(contains(@cref, 'System.'))]">
    <a>
      <xsl:attribute name="href">#<xsl:call-template name="string-replace-all">
      <xsl:with-param name="text" select="@cref" />
      <xsl:with-param name="replace" select="'F:'" />
      <xsl:with-param name="by" select="'field-'" />
      </xsl:call-template></xsl:attribute>
      <xsl:value-of select="substring-after(@cref,':')" />
    </a>
  </xsl:template>

  <xsl:template match="see[contains(@cref, 'System.')]">
    <a>
      <xsl:attribute name="href">https://learn.microsoft.com/en-us/dotnet/api/<xsl:value-of select="translate(substring-after(@cref,':'), $uppercase, $lowercase)" /></xsl:attribute>
      <xsl:value-of select="substring-after(@cref,':')" />
    </a>
  </xsl:template>

  <xsl:template match="see[@href]">
    <a>
      <xsl:attribute name="href">
        <xsl:value-of select="@href" />
      </xsl:attribute>
      <xsl:apply-templates />
    </a>
  </xsl:template>


  <xsl:template match="seealso">
    <EM>See also: <xsl:value-of select="@cref" /></EM>
  </xsl:template>

  <xsl:template match="summary">
    <P class="summary"><xsl:apply-templates /></P>
  </xsl:template>

  <xsl:template match="value">
    <code class="value"><xsl:apply-templates /></code>
  </xsl:template>

  <xsl:template match="list">
    <xsl:choose>
      <xsl:when test="@type='bullet'">
        <UL>
          <xsl:for-each select="listheader">
            <LI><strong><xsl:value-of select="term"/>: </strong><xsl:value-of select="definition"/></LI>
          </xsl:for-each>
          <xsl:for-each select="list">
            <LI><strong><xsl:value-of select="term"/>: </strong><xsl:value-of select="definition"/></LI>
          </xsl:for-each>
        </UL>
      </xsl:when>
      <xsl:when test="@type='number'">
        <OL>
          <xsl:for-each select="listheader">
            <LI><strong><xsl:value-of select="term"/>: </strong><xsl:value-of select="definition"/></LI>
          </xsl:for-each>
          <xsl:for-each select="list">
            <LI><strong><xsl:value-of select="term"/>: </strong><xsl:value-of select="definition"/></LI>
          </xsl:for-each>
        </OL>
      </xsl:when>
      <xsl:when test="@type='table'">
        <TABLE>
          <xsl:for-each select="listheader">
            <TH>
              <TD><xsl:value-of select="term"/></TD>
              <TD><xsl:value-of select="definition"/></TD>
            </TH>
          </xsl:for-each>
          <xsl:for-each select="list">
            <TR>
              <TD><strong><xsl:value-of select="term"/>: </strong></TD>
              <TD><xsl:value-of select="definition"/></TD>
            </TR>
          </xsl:for-each>
        </TABLE>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="string-replace-all">
    <xsl:param name="text" />
    <xsl:param name="replace" />
    <xsl:param name="by" />
    <xsl:choose>
      <xsl:when test="contains($text, $replace)">
        <xsl:value-of select="substring-before($text,$replace)" />
        <xsl:value-of select="$by" />
        <xsl:call-template name="string-replace-all">
          <xsl:with-param name="text" select="substring-after($text,$replace)" />
          <xsl:with-param name="replace" select="$replace" />
          <xsl:with-param name="by" select="$by" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$text" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>
