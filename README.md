A starter project to get started with Razor.Servicestack, Windows Azure,
SQL Azure, (Azure) blob storage, social sign-ins, and AngularJS. 

### Project Overview

This project was started in order to experiment and road test both the
Razor.ServiceStack project and Windows Azure, and to build an web app
using them. The idea is simple, this app is an online photo manager,
like a simple Dropbox for your photos. Can can sign in using your social
accounts such as Twitter or Facebook, and allows you to store your
photos in the cloud. The solution file is a Visual Studio 2012 Windows Azure cloud service project.

##### Built into this project

*We’ve cooked in some useful things:

-   Generic functions for managing files in Azure Storage.
-   A generic repository for working with OrmLite.
-   A customizable user entity and authentication session handling.
-   Utilizes SquishIt to compress the Javascript and Css files on the
    fly, configured to work with Azure too!
-   Uses Twitter Bootstrap.
-   Wired up for AngularJS, including routing.

### Built with ServiceStack

This project utilizes the core ServiceStack webservice framework to
expose a simple but robust RESTful API. It then leverages the
Razor.ServiceStack MVC Razor Engine to power the entire web
application’s UI. The two provide a direct replacement or ASP.NET MVC
and Web API. On top of this it utilizes the high perfomance parts that
come with ServiceStack such as ServiceStack.Ormlite for the ORM,
ServiceStack.Text for fast serialization, Funq for the DI framework, and
ServiceStack.Caching for performance caching.

 *The immediate benefits we have seen are:*

-   The MVC views and REStful API are the same service implementation
    making one unified HTTP stack.
-   The ServiceStack service implementation also supports SOAP and CSV
    as well as XML and JSON.
-   Razor.ServiceStack has more flexible routing, and smarter view pages
    than ASP.NET MVC.
-   Its also lighter and easier to configure than ASP.NET MVC.
-   The components all work together out of the box: Ormlite, Funq,
    Caching and Text.
-   ServiceStack supports social sign ins using Facebook and Twitter, as
    well as traditional manual user sign ups.
-   Extendable authentication model allowing for many different types of
    authentication implementations, works across both the API and UI
    layers.

### Runs on Windows Azure

This was built from the ground up as an Azure Cloud Service application.
We also wanted it to be completely Azure ready and to that end it needed
to work with SQL Azure and Azure blob storage. The database can work off
a local SQL Server database, and seamlessly deploys itself and
transitions to SQL Azure with no configuration change at all on
deployment. You can connect to, and upload the photos into Azure blob
storage from you local machine as well as from within Azure, so you know
from the start that it will work, and will continue to work once you
have deployed to Windows Azure. ServiceStack.Caching also supports Azure
In-Memory DataCaching.

### Twitter Bootstrapped

Cross browser, extendable, and responsive design powered by the Twitter
Bootstrap front-end framework. No customization has been done here, so
if you are familar with Twitter Bootstrap then you can jump straight in
and start working with it easily.