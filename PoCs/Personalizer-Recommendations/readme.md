## Personalizer Cognitive Service Proof of Concept

Microsoft's new [Personalizer Azure Cognitive Service](https://docs.microsoft.com/azure/cognitive-services/personalizer/) entered public preview at Build 2019. This service is a recommender with reinforcement learning.

This Proof of Concept (PoC) simulates retrieving "read this next" recommendations for thousands of users from the Personalizer service.

For each user, the relevance of the recommendation from Personalizer is evaluated, a reward is computed, and sent back to Personalizer. Periodically, the app pauses to wait for Personalizer to train with the updated recommendation/reward data received from the app.

This app uses contrived data sets for users (really, user contexts - Personalizer does not need individually identifying personal data, but it does need information about the context within which a user will decide whether to follow Personalizer's recommendation) and for resources to recommend - here, "articles" or "websites" for a personalized reading list.

The app uses the C# SDK and a couple of my other .NET libraries on Nuget (pelazem.rndgen and pelazem.util, whose source is in my github account here) but of course any language/platform can be used as long as it can interact with HTTP REST APIs, or for which there is an SDK.

Resources:
* [Personalizer Samples](https://github.com/Azure-Samples/cognitive-services-personalizer-samples)
* [Personalizer API Docs](https://westus2.dev.cognitive.microsoft.com/docs/services/personalizer-api/)
* [Recommendation Systems best practices and examples](https://github.com/microsoft/recommenders)


---

### PLEASE NOTE FOR THE ENTIRETY OF THIS REPOSITORY AND ALL ASSETS
#### 1. No warranties or guarantees are made or implied.
#### 2. All assets here are provided by me "as is". Use at your own risk. Validate before use.
#### 3. I am not representing my employer with these assets, and my employer assumes no liability whatsoever, and will not provide support, for any use of these assets.
#### 4. Use of the assets in this repo in your Azure environment may or will incur Azure usage and charges. You are completely responsible for monitoring and managing your Azure usage.

---

Unless otherwise noted, all assets here are authored by me. Feel free to examine, learn from, comment, and re-use (subject to the above) as needed and without intellectual property restrictions.

If anything here helps you, attribution and/or a quick note is much appreciated.