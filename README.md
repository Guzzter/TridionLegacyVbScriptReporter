# TridionLegacyVbScriptReporter
A tool to scan a Tridion CMS for template based legacy VbScript code

For detecting the VB script based templates, this console application has been developed. It utilizes the Core Service and iterates through all the publications in the Tridion instance. 
It scans for Page Templates and Component Templates that are based on VBScript code. The tool outputs a text report with webdav paths and TCMid's (vbscriptreport.txt). Also saves the template code in text files (configurable).

- Tridion 2011 Core Service Console Application
- Uses config-less code from http://code.google.com/p/tridion-practice/wiki/GetCoreServiceClientWithoutConfigFile
- Built using Tridion 2011 SP1
- Need to add the Tridion DLL Reference.  The Tridion DLL Tridion.ContentManager.CoreService.Client is located in the Tridion\bin\client folder.
- Update the CoreServiceHandler.config and add your web server details