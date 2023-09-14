
![logo-dark](https://github.com/Excustic/Optimeet/assets/47672175/934b33a6-9b5e-4f8d-8b86-fdb5889b1639)
# Optimeet
Optimeet is a desktop app that provides a solution for suggesting meeting places that are suitable and convenient for all of your friends and colleagues, with the option to easily invite everyone!
## Demo Screenshots
![snip2](https://github.com/Excustic/Optimeet/assets/47672175/2917fc61-3955-4741-9dfd-65b5180b3b6a)
![snip1](https://github.com/Excustic/Optimeet/assets/47672175/d372491f-a0a9-4a76-b881-d2a9defd7132)
![snip3](https://github.com/Excustic/Optimeet/assets/47672175/ae73fe29-1a84-4cd3-bb02-42a9d2a5117b)
![snip4](https://github.com/Excustic/Optimeet/assets/47672175/b24925e7-b9fd-4e7f-8661-b17f9be774c8)
![snip5](https://github.com/Excustic/Optimeet/assets/47672175/ea4cf047-343e-4d36-b2e6-a18c8d835175)
![snip6](https://github.com/Excustic/Optimeet/assets/47672175/e8453c4b-8c2f-4285-b97a-11747214ddf0)

## Motivation 
Optimeet was an old idea I had in mind a couple of years ago that didn't come to life because I decided to develop [OnPoint](https://github.com/Excustic/OnPointML-public-) instead. 
The idea was revived about half a year ago when I was still in service in the military and wanted to make a comeback in programming and dust off my skills. I decided to give it a go as I thought it was a 
great opportunity to learn how to make an executable desktop program with an interactive UI. The project turned out to be more exciting than I imagined and was well worth the work.

## Features 
+ Create meetings with date, time, location, and participants and store them locally
+ Optimal location search algorithm based on participants' location
+ View and delete your past, upcoming and future meetings
+ View, add, edit, search and delete contacts through a contact book
+ Adjust application settings in a designated settings menu
+ Invite people to meetings using Google Calendar via signing into a Google account with OAuth 2.0 ID Client

## How the algorithm works 

**Problem definition** - *For a set of N  XY points, calculate the weighted centroid.*

### Method

**Define:**

*N* - number of points

*a* - mean of points

*𝞂* - standard deviation

*x* - latitude

*y* - longitude

Using the weighted average mean:

![image](https://github.com/Excustic/Optimeet/assets/47672175/12a355fb-d662-4e85-ac63-c708aa7252f6)

And writing our weights as Gaussian distribution function:

![image](https://github.com/Excustic/Optimeet/assets/47672175/fb846162-2f5f-4e84-ba1b-7a14ceeef295)

Switching the coefficient of _e_ we finally get an expression which will give us coordinates that are calculated with more bias towards the central points, this will achieve a stable centroid that outlier points won't compromise.

![image](https://github.com/Excustic/Optimeet/assets/47672175/a4ceaf3d-7d61-46ac-ac3c-5ae827f8c069)

### Example of a real-world situation of the algorithm's application against a regular centroid

Suppose there are 4 people who live on the outskirts of London and another person who lives far away from London in Birmingham.

If a simple centroid calculation were applied, the red dot would've been the suggested meeting spot for everyone which is situated somewhere at Watford - which honestly would waste everyone's time and isn't the best geographical place either.
Using the weights method, we get a more reasonable spot marked by the green dot not too far from London's centre. An improvement to be sure, which also turns out to be more accessible for the person who lives in Birmingham! 

You can fact-check me.

![WeightedCentroid](https://github.com/Excustic/Optimeet/assets/47672175/fd340f4c-3f23-4a5a-9559-ac9d3dd5c397)

## Installation guide

How to install:

1. Download the zip file
2. Extract the zip to a destinated folder
3. Open the folder and click on setup.exe - this will open a setup wizard
4. Finish setup and the app will open and be automatically added to your start menu as well

**Please note that the application is not intended for public use and won't run properly without making your own API keys**
If you want to use the application properly you will have to follow the instructions below:
_Creating the essential API keys and credentials_
1. Sign in or create an account at [https://console.cloud.google.com/](https://console.cloud.google.com/)
2. Create a new project
3. Open the project's dashboard and go to the APIs and services section
4. Enable Maps API service
5. Return to APIs and services and go to the Credentials section
6. Create a new OAuth ClientID and a consent screen by adding a tester mail, choose Desktop App, input 'Optimeet' in the name form and click Create. When you're done visit the APIs and keys section again and you'll see your OAuth 2.0 Client ID, download it and save it on your local computer.
7. Create a new account at [https://positionstack.com/](https://positionstack.com/)
8. Generate an API key for free.
9. Generate a Bing Maps key following [these quick steps](https://learn.microsoft.com/en-us/bingmaps/getting-started/bing-maps-dev-center-help/getting-a-bing-maps-key)
    
_Storing the credentials on your local application_ 

After you've finished creating all of the essential keys and credentials, open the application and travel to the settings menu. You can input the needed keys and the OAuth JSON's full path. 
You're all set! Don't hesitate to open up a new issue in the repository if you run into any additional issues.

