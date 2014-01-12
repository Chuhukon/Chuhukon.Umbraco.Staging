#Chuhukon Umbraco Staging
###Disclaimer: This is in an experimental fase!

Goal: an auto package creator / deployment / staging kind of thingy for Umbraco  

##What it currently does:
The Staging library creates a package.xml in the "/_staging" folder. The idea is that xml files can be used to create deployment packages for staging. So the library / dll is only needed on the development enviroment and not on T(est)A(cceptance)P(roduction).
The file contains:
* Templates based on what's in your view folder
* Documenttypes created in Umbraco
* Macro's created in Umbraco

##How to get the solution working?
Use the Website project to move some sand in the sandbox. The website project does only use the Umbraco Core libraries. 
To run your website follow these instructions to setup and run your project, and don't overwrite any file:

http://www.ben-morris.com/using-umbraco-6-to-create-an-asp-net-mvc-4-web-applicatio  

- The example database username is "admin" and the password is "password".  
- THe project does use the Macaw.Umbraco.Foundation see: [the github repo](https://github.com/MacawNL/Macaw.Umbraco.Foundation) for more details.

