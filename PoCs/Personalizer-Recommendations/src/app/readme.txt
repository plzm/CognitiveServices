Approach 1
Generate actions
Generate user contexts
For each user context
	Get recommended action
	Calculate score for recommended action
	Send score back to API
Concerns: this approach does not guide the API clearly in WHICH action is best, only how much the user "likes" the top recommendation.

Approach 2
Generate actions
Generate user contexts
For each user context
	Get actions
	For each action, calculate a like score for the user
	If the top action recommended by the service also has the top like score, send 1 back to the API.
		Variant a: send back 0 to the API otherwise.
		Variant b: if the top action recommended by the service has the second-highest like score, send 0.5 back to the API.
