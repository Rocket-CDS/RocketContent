# Handlebars Reference

RocketCDS uses Razor and handlebars.js to display data.  This document defines the handlerbars helpers (functions) to help display data.

https://handlebarsjs.com/  

RocketCDS uses XML to store data, Handlebars.js uses Json.  A conversion from XML to json is done for handlebars.  You can therefore use the standard json syntax to get data.  However, we have also introduced a number of helpers to make displaying data more friendly.

### general session

**{{displayData this}}** - This helper is used to display json string, this is helpful in understanding what data you are using.  
```
{{moduleref @root}}
{{engineurl @root}}
{{culturecode @root}}
{{culturecodeedit @root}}
```

### article

```
{{article @root "name" @index}}
{{article @root "genxml/textbox/mytextbox" @index}}

```

### articlerow

```
{{#each genxml.data.genxml.rows.genxml}}

    {{articlerow @root "rowkey" @index}}
    {{articlerow @root "genxml/textbox/mytextbox" @index}}

{{/each}}
```


### image
```
{{#each genxml.data.genxml.rows.genxml}}
    {{#each imagelist.genxml}}

        {{image @root "thumburl" @../index @index 640 200}}
        {{image @root "alt" @../index @index}}
        {{image @root "summary" @../index @index}}
        {{image @root "relpath" @../index @index}}
        {{image @root "height" @../index @index}}
        {{image @root "width" @../index @index}}
        {{image @root "url" @../index @index}}
        {{image @root "urltext" @../index @index}}
        {{image @root "fieldid" @../index @index}}
        {{image @root "count"}}
        {{image @root "genxml/textbox/mytextbox" @../index @index}}

    {{/each}}
{{/each}}
```
### doc
```
{{#each genxml.data.genxml.rows.genxml}}
    {{#each imagelist.genxml}}

        {{doc @root "key" @../index @index}}
        {{doc @root "name" @../index @index}}
        {{doc @root "hidden" @../index @index}}
        {{doc @root "url" @../index @index}}
        {{doc @root "relpath" @../index @index}}
        {{doc @root "fieldid" @../index @index}}
        {{doc @root "count"}}
        {{doc @root "genxml/textbox/mytextbox" @../index @index}}

    {{/each}}
{{/each}}
```
### link
```
{{#each genxml.data.genxml.rows.genxml}}
    {{#each imagelist.genxml}}

        {{link @root "key" @../index @index 640 200}}
        {{link @root "name" @../index @index 640 200}}
        {{link @root "hidden" @../index @index 640 200}}
        {{link @root "fieldid" @../index @index}}
        {{link @root "count"}}
        {{link @root "ref" @../index @index}}
        {{link @root "type" @../index @index}}
        {{link @root "target" @../index @index}}
        {{link @root "anchor" @../index @index}}
        {{link @root "url" @../index @index}}
        {{link @root "genxml/textbox/mytextbox" @../index @index}}

    {{/each}}
{{/each}}
```
