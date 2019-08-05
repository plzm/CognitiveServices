## Receipt Ingestion Proof of Concept (PoC)

This PoC shows ingestion of typical point of sale receipt images, with analysis and content extraction. Two Azure AI Cognitive Services are used:
1. Computer Vision: this service is Generally Available, and is a general-purpose image analysis service. It is used to show its capabilities here for general images.
2. Form Recognizer: this service is currently in Preview. It has specific receipt ingestion capability. Results from Form Recognizer will be added here as soon as available.

The architecture for this PoC (see below for diagram) starts with Azure blob storage. A storage access policy is set on storage so that images can be stored securely (not publicly accessible), but URLs with policy-based access tokens can be generated so that services like the Azure AI Cognitive Services used in this PoC can still access the images via secured URLs.

![PoC Architecture](architecture-receipts.png)

When a receipt image is uploaded to blob storage, an Event Grid event is raised. This triggers an Azure Function, which gets metadata about the image, including its secured URL; invokes the Cognitive Services (Computer Vision and Form Recognizer), and stores the secured image URL as well as the analysis results from these services in an Azure SQL database.

Currently, the Azure Function interacts with the two Cognitive Services directly via their respective REST APIs. There are also SDKs available.

This PoC uses receipt images (in the TestImages folder). All of these were sourced from searches on bing.com/images. All images are believed to be in the public domain and free of copyright or other constraints. Please advise if this is incorrect.

TestImages/results1.txt contains an initial set of results from running the images in TestImages/set1 and TestImages/set2 through the Computer Vision Azure AI Cognitive Service. Form Recognizer results are not yet in this set, but will be added as soon as possible.

TestImages/results1.xlsx is an Excel version of TestImages/results1.txt.

In the data:

ImageUrl is the secured URL to the image, stored in Azure blob storage. The storage account is not publicly accessible; a storage access policy is in place. The ImageUrls in the data have an access token included in the URL, to show how images can be stored securely against general public access, but be made accessible via URL for APIs like the Azure AI cognitive services.

JsonCustomVision is the JSON output from the Computer Vision Azure AI Cognitive Service. It can be made more easily readable in a good JSON editor like Visual Studio Code with a JSON extension, or in other text editors with appropriate JSON capabilities. Alternately, to avoid tool extension downloads/installs, the JSON can be pasted into an online JSON renderer like that at https://jsonformatter.curiousconcept.com/.

Resources:
* [Form Recognizer Azure AI Cognitive Service Overview](https://docs.microsoft.com/azure/cognitive-services/form-recognizer/overview)
* [Form Recognizer API Docs](https://westus2.dev.cognitive.microsoft.com/docs/services/form-recognizer-api)
* [Computer Vision Azure AI Cognitive Service Overview](https://docs.microsoft.com/azure/cognitive-services/computer-vision/home)
* [Computer Vision API Docs](https://westus.dev.cognitive.microsoft.com/docs/services/5adf991815e1060e6355ad44)

---

### PLEASE NOTE FOR THE ENTIRETY OF THIS REPOSITORY AND ALL ASSETS
#### 1. No warranties or guarantees are made or implied.
#### 2. All assets here are provided by me "as is". Use at your own risk. Validate before use.
#### 3. I am not representing my employer with these assets, and my employer assumes no liability whatsoever, and will not provide support, for any use of these assets.
#### 4. Use of the assets in this repo in your Azure environment may or will incur Azure usage and charges. You are completely responsible for monitoring and managing your Azure usage.

---

Unless otherwise noted, all assets here are authored by me. Feel free to examine, learn from, comment, and re-use (subject to the above) as needed and without intellectual property restrictions.

If anything here helps you, attribution and/or a quick note is much appreciated.