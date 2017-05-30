Title: Building A Cloud-Based Web App From Scratch
Lead: Azure Cosmos DB + Azure Functions + Vue.js = Awesome
Image: /images/clouds.jpg
Published: 5/25/2017
Tags:
  - Azure
  - Azure Cosmos DB
  - Azure Functions
  - Vue.js
---
It seems like it's impossible to stay away from the hype surrounding cloud services recently. All the major providers are shipping updates at what seems like an insane pace and buzzwords like *serverless* and *NoSQL* are everywhere. It's been a while since I've done any major work on a public cloud with the last time being four or five years ago on AWS. While I've done a little hosting here and there on Azure, I figured I was overdue for a real deep-dive. Since I was starting from scratch with everything else, both implementation and knowledge-wise, it also seemed like a good opportunity to learn Vue.js, which I've had my eye on for a while. This blog series is my experience setting up a completely cloud-based web application using nothing but Azure services and open source libraries. In this first part, I'll introduce the project and we'll get the database set up. The next post will set up the Azure services for the API and front-end, and then following posts will build out the UI.

Note that I'm coming at this knowing almost nothing about any of the services or libraries we're going to make use of. I think that's important to point out because the way I do things may be pretty novice. That's okay, in fact it's kind of the point. This will be a learning experience for us both.

# What We're Building

I wanted to build something that wasn't trivial but was also easy to reason about and figure out. My first time through this process was actually work-related, so we'll build something similar to the PoC I put together for my day job. In this case we're going to create a simple customer management system that holds contact information and allows you to edit and update it. It'll be created as a single-page application (SPA), but that's more by virtue of being relatively simple than by a specific desire to create a SPA. The end result will also be a static site (more on that in a following post).

# What We'll Be Using

This project will make use of a number of services, tools, and libraries. Here's a list:

## Services

* **[Azure Cosmos DB](https://azure.microsoft.com/en-us/services/cosmos-db/)** for holding our data
* **[Azure Functions](https://azure.microsoft.com/en-us/services/functions/)** to provide the API that our frontend will talk to
* **[Azure App Service](https://azure.microsoft.com/en-us/services/app-service/)** or another static host such as [Netlify](https://www.netlify.com/) or [GitHub Pages](https://pages.github.com/) for hosting the app

## Frontend

* **[Vue.js](https://vuejs.org/)** for the client logic and managing the app
* **[axios](https://github.com/mzabriskie/axios)** a JavaScript library for managing Ajax calls between our frontend and our API
* **[Bulma](http://bulma.io/)** for styling because I'm tired of [Bootstrap](http://getbootstrap.com/) and this project all about learning new things

## Tools

* **[Vue.js Chrome extension](https://chrome.google.com/webstore/detail/vuejs-devtools/nhdogjmejiglipccpnnnanhbledajbpd)** for peeking under the hood
* **[VS Code](https://code.visualstudio.com/)** to edit our HTML (of course you can use whatever editor you want)
* **[LINQPad](https://www.linqpad.net/)** for quickly testing the logic in our Azure Functions

Notice how Visual Studio isn't listed here at all. I still love and use Visual Studio daily, but I wanted to see what would happen if I ignored it for this project. Part of the goal here is to see how light-weight we can make this process.

# Creating The Database Account

The first thing we'll do is [create a document database account](https://docs.microsoft.com/en-us/azure/cosmos-db/create-documentdb-dotnet#create-a-database-account) in Cosmos DB. Find the *Azure Cosmos DB* service and open it:

<img src="/posts/images/cosmos1.png" class="img-responsive"></img>

Now create a new Cosmos DB account. I'm calling my app *TrapperKeeper* so that's what I'll call my Cosmos DB account, but you can call your app whatever you want. Select *SQL (DocumentDB)* for the API. You'll also need to select an Azure subscription, resource group, and location:

<img src="/posts/images/cosmos2.png" class="img-responsive"></img>

It'll take a minute or two to deploy and when it's done you should see your new Cosmos DB account listed (you may need to hit refresh):

<img src="/posts/images/cosmos3.png" class="img-responsive"></img>

# Creating The Document Database and Collection

Before we create any documents we need to [create a database]() and then [create a collection](https://docs.microsoft.com/en-us/azure/cosmos-db/create-documentdb-dotnet#add-a-collection). A [Cosmos DB collection](https://docs.microsoft.com/en-us/azure/documentdb/documentdb-faq#what-is-a-collection) is a group of documents and their associated logic and is also used for billing purposes. To create the collection, go to the Overview blade of the Cosmos DB account and select "Add Collection":

<img src="/posts/images/cosmos4.png" class="img-responsive"></img>

You need to give the collection and ID. Since this collection is going to be storing our customer documents I'm going to name it "customers". We also have to select a partitioning key that can be used for scaling the collection by distributing documents to other servers. I'm not that interested in this right now, so I'll just use "id" for my partitioning key as well as the document ID. And since we don't have a database yet (just a database *account* which can hold multiple *databases*) we'll also need to create one of those. I'm going to name the database the same as the account since it's the only database we'll need.

<img src="/posts/images/cosmos5.png" class="img-responsive"></img>

# Adding Some Documents

Now we're going to go ahead and add some initial documents. This will make developing the API and frontend easier because we'll actually have some data to test against. Once the collection is finished provisioning, open the "Document Explorer" and then click "Create":

<img src="/posts/images/cosmos6.png" class="img-responsive"></img>

This will open a nice text window where we can type in (or paste) our initial document. There's even a template for the document that includes the required ID:

<img src="/posts/images/cosmos7.png" class="img-responsive"></img>

I'm going to give our customers GUID IDs, so you can generate those however you like. Here's my first customer document:

```
{
  "id": "1d8babe6-3fa3-46b2-acc5-769e6d5db3b4",
  "firstName": "Jane",
  "lastName": "Austen",
  "phoneNumbers": [
    {
      "label": "Home",
      "number": "123-456-7890"
    },
    {
      "label": "Cell",
      "number": "987-654-3210"
    }
  ],
  "emailAddress": "jane.austen@gmail.com",
  "address": {
    "street": "Winchester Road",
    "city": "Chawton",
    "state": "Hampshire",
    "postal": "GU34 1SD",
    "country": "UK"
  }
}
```

Using that same template, let's go ahead and create a second customer document too:

```
{
  "id": "e1d645f9-6836-47e7-8ec2-97f4ba2556f7",
  "firstName": "Langston",
  "lastName": "Hughes",
  "phoneNumbers": [],
  "emailAddress": "lhughes@gmail.com",
  "address": {
    "street": "20 East 127th Street",
    "city": "New York",
    "state": "New York",
    "postal": "10035",
    "country": "US"
  }
}
```

Note that your documents can have whatever formatting and structure you like. There is no schema to enforce anything. That puts the onus on you and your code to make sure that documents in your collection follow whatever conventions you require. While this may seem icky to you at first (it did to me), it also provides a great deal of flexibility when evolving the document structure later.

# Next Steps

If you've followed along this far you should have a Cosmos DB database and collection with two customer documents. The next post in this series will set up our Azure Functions API and front-end Azure App Service. In the meantime, you can familiarize yourself with the [Cosmos DB DocumentDB SQL syntax](https://docs.microsoft.com/en-us/azure/cosmos-db/tutorial-query-documentdb) for issuing queries. The easiest way to play around with this is by going to the "Query Explorer" in Azure Cosmos DB and trying out some queries:

<img src="/posts/images/cosmos8.png" class="img-responsive"></img>

Until next time!