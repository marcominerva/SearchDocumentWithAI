# Search Documents With AI
A sample that shows how to use ChatGPT to get answers about any PDF documents

You need to set the required values in the [appsettings.json](https://github.com/marcominerva/SearchDocumentWithAI/blob/master/src/SearchGpt/appsettings.json) file:

    "ChatGPT": {
        "Provider": "OpenAI",           // Optional. Allowed values: OpenAI (default) or Azure
        "ApiKey": "",                   // Required
        "Organization": "",             // Optional, used only by OpenAI
        "ResourceName": "",             // Required when using Azure OpenAI Service
        "AuthenticationType": "ApiKey", // Optional, used only by Azure OpenAI Service. Allowed values : ApiKey (default) or ActiveDirectory
        "DefaultModel": "my-model"      // Required  
    }
