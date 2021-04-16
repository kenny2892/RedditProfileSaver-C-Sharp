import praw
import json
import time
import traceback
import os.path
import pathlib
import requests

overwrite_old_file = False
imgurId = ""

upvoted_posts = {}
old_upvoted_posts = {}
fields = ["Post Name", "Author", "Subreddit", "Created (in Seconds)", "Url to Content", "Url to Post", "Url to Thumbnail", "Is Saved", "Is NSFW", "Number"]

# Create Reddit Instance and Start Processing
def collectUpvotedPosts(bot_id, bot_secret, token, user):
    reddit = praw.Reddit(
        client_id = bot_id,
        client_secret = bot_secret,
        refresh_token = token,
        user_agent = user
    )
    
    printToFile("User being checked: " + reddit.user.me().name + "\n")
    
    loadFile(reddit)
    storeUpvotedPosts(reddit)
    saveFile(reddit)
    
    print("Upvotes have been saved!")
    print("Total of " + str(len(upvoted_posts)) + " upvoted posts archived.")

# Check if Upvoted File Exists
def loadFile(reddit):
    file_path = str(pathlib.Path(__file__).parent.absolute()) + "/" + reddit.user.me().name + " - Upvoted.json"
    if(not overwrite_old_file & os.path.isfile(file_path)):
        with open(file_path) as file:
            global old_upvoted_posts 
            old_upvoted_posts = json.load(file)

# Begin Storing Upvoted Posts
def storeUpvotedPosts(reddit):
    try:
        post_count = 0
        matches = 0 # Match Counter (counts up to 10, if it reaches 10 it breaks the loop)

        # Add Posts to List
        for item in reddit.user.me().upvoted(limit=None):
            # Increase Post Count
            post_count = post_count + 1
    
            title = item.title
            author = item.author.name if item.author is not None else "Deleted"
            
            # Make Post Entry Name
            post_name = title + " - " + author + " - " + str(item.created)
            post_name_utc = title + " - " + author + " - " + str(item.created_utc)
            
            if post_name in old_upvoted_posts.keys() or post_name_utc in old_upvoted_posts.keys():
                matches = matches + 1
                printToFile("Post Already in Archive. " + str((10 - matches)) + " more posts till termination.\n")
                time.sleep(0.2)
                
                if(matches >= 10):
                    break;
                
                else:
                    continue
            
            # Reset Matches Counter
            matches = 0
            
            # Print the storing message
            printToFile("Post " + str(post_count) + " has been stored.\n")
            
            # Get All Variables
            # print(vars(item))
            
            # Fill in Post Entries
            post_entry = {}
            post_entry[fields[0]] = title
            post_entry[fields[1]] = author
            post_entry[fields[2]] = item.subreddit.display_name
            post_entry[fields[3]] = item.created_utc
            
            contentUrl = item.url
            ogUrl = contentUrl
            
            if("reddit.com/gallery/" in contentUrl):
                contentUrl = getGalleryUrls(contentUrl, reddit)
                
            elif("redgifs" in contentUrl):
                contentUrl = convertRedGifsUrl(contentUrl)
                
            elif("v.redd.it" in contentUrl):
                contentUrl = convertVredditUrl(item, contentUrl)
                
            elif("imgur.com/a/" in contentUrl):
                try:
                    contentUrl = convertImgurGallery(contentUrl)
                except:
                    contentUrl = ogUrl
                
            elif("//imgur.com/" in contentUrl and "/gallery/" not in contentUrl and "/r/" not in contentUrl):
                numOfPeriods = 0
                for i in contentUrl:
                    if(i == '.'):
                        numOfPeriods = numOfPeriods + 1

                if(numOfPeriods == 1): ## Meaning there are no extensions like .png or .jpg
                    try:
                        contentUrl = convertImgurImage(contentUrl)
                    except:
                        contentUrl = ogUrl
            
            post_entry[fields[4]] = contentUrl            
            post_entry[fields[5]] = "https://www.reddit.com" + item.permalink
            post_entry[fields[6]] = item.thumbnail # If there is no thumbnail (the post is text) it will say "self" instead
            post_entry[fields[7]] = item.saved
            post_entry[fields[8]] = item.over_18
            post_entry[fields[9]] = post_count
            
            # Store post entries in the upvotes entry
            upvoted_posts[post_name] = post_entry

            # Sleep for 100 Millisec
            time.sleep(0.1)

            if(post_count >= 1100):
                break

    except Exception as e:
        printToFile("Something went wrong!\nError Msg:\n" + str(e) + "\n")
        traceback.print_exc()

def convertImgurImage(contentUrl):
    # Sleep for 1 Sec
    time.sleep(1)

    id = ""
    newContentUrl = contentUrl
    if("//imgur.com/" in newContentUrl):
        index = newContentUrl.index("com/") + 4
        id = newContentUrl[index:]

    else:
        return newContentUrl

    global imgurId
    clientId = "Client-ID " + imgurId
    response = requests.get(
        "https://api.imgur.com/3/image/" + id,
        headers={"Authorization": clientId},
    )

    json_response = response.json()
    success = json_response["success"]
    status = json_response["status"]

    if(success and status == 200):
        data = json_response["data"]

        for item in data:
            newContentUrl = newContentUrl + " " + item["link"]

    return newContentUrl

def convertImgurGallery(contentUrl):
    # Sleep for 1 Sec
    time.sleep(1)

    id = ""
    newContentUrl = contentUrl
    if("imgur.com/a/" in newContentUrl):
        index = newContentUrl.index("/a/") + 3
        id = newContentUrl[index:]

    else:
        return newContentUrl

    global imgurId
    clientId = "Client-ID " + imgurId
    response = requests.get(
        "https://api.imgur.com/3/album/" + id + "/images",
        headers={"Authorization": clientId},
    )

    json_response = response.json()
    success = json_response["success"]
    status = json_response["status"]

    if(success and status == 200):
        data = json_response["data"]

        for item in data:
            newContentUrl = newContentUrl + " " + item["link"]

    return newContentUrl
        
def convertVredditUrl(post, contentUrl):
    try:
        newUrl = contentUrl
        postVideo = post.media['reddit_video']

        newUrl = postVideo['fallback_url']
        newUrl = newUrl.split("?")[0]
        newUrl = contentUrl + " " + newUrl + " " + str(postVideo['width']) + " " + str(postVideo['height'])

        # Sleep for 100 Millisec
        time.sleep(0.1)

        return newUrl
        
    except:
        return contentUrl
        
def convertRedGifsUrl(contentUrl):
    try:
        html = requests.get(contentUrl).text
    
        startIndex = html.index("og:video\" content=\"") + 19
        html = html[startIndex:]
        endIndex = html.index("\" /><meta data")
        newUrl = html[:endIndex]
        newUrl = newUrl.replace("-mobile.mp4", ".mp4")
        
        # Sleep for 1 Sec
        time.sleep(1)
    
        return newUrl
        
    except:
        return contentUrl
        
def getGalleryUrls(contentUrl, reddit):
    newContentUrl = contentUrl
    imageLinks = convertGalleryToUrls(contentUrl, reddit)
    
    if(imageLinks is not None or len(imageLinks) != 0):
        for index in imageLinks:
            newContentUrl = newContentUrl + " " + imageLinks[index]
        
    return newContentUrl
    
def convertGalleryToUrls(contentUrl, reddit):
    imageLinks = {}
    submission = reddit.submission(url=contentUrl)
    
    if(submission is None):
        imageLinks = {}
        return imageLinks
    
    try:
        i = 0;
        for item in sorted(submission.gallery_data['items'], key=lambda x: x['id']):
            media_id = item['media_id']
            meta = submission.media_metadata[media_id]

            if meta is None or meta['status'] == 'failed':
                continue
            
            elif meta['e'] == 'Image':
                source = meta['s']
                imageLinks[i] = '%s' % (source['u'])
                i = i + 1
                
            elif meta['e'] == 'AnimatedImage':
                source = meta['s']
                imageLinks[i] = '%s' % (source['gif'])
                i = i + 1
            
            # Sleep for 1 Sec
            time.sleep(1)
        
    except:
        imageLinks = {}
        
    return imageLinks
        
def saveFile(reddit):
    # Correct The Post Numbers
    to_subtract = len(upvoted_posts)
    for entry in upvoted_posts:
        upvoted_posts[entry][fields[9]] = -1 * (upvoted_posts[entry][fields[9]] - to_subtract)
        
    # If merging, correct the numbers again
    if(not overwrite_old_file):
        to_add = len(old_upvoted_posts)
        for entry in upvoted_posts:
            upvoted_posts[entry][fields[9]] = upvoted_posts[entry][fields[9]] + to_add + 1
    
    # Check if file is to be overwritten or not
    if(not overwrite_old_file):
        upvoted_posts.update(old_upvoted_posts)
    
    # Save to File
    file_path = str(pathlib.Path(__file__).parent.absolute()) + "/" + reddit.user.me().name + " - Upvoted.json"
    out_file = open(file_path, "w") 
    json.dump(upvoted_posts, out_file, indent = 4) 
    out_file.close()

    printToFile("Completed")
    
def printToFile(text):
    file_path = str(pathlib.Path(__file__).parent.absolute()) + "/Results.txt"
    resultsFile = open(file_path, "a")
    
    resultsFile.write(text)
    resultsFile.close()
    
    print(text)
    
def main():
    # Get Config File
    config = {}
    config_path = str(pathlib.Path(__file__).parent.absolute()) + "\Config.json"
    
    if(os.path.isfile(config_path)):
        with open(config_path) as config_file:
            config = json.load(config_file)
            
    bot_id = config.get("Id", "")
    bot_secret = config.get("Secret", "")
    token = config.get("Refresh Token", "")
    user = config.get("User Agent", "")

    global imgurId
    imgurId = config.get("ImgurClientId", "")
    
    collectUpvotedPosts(bot_id, bot_secret, token, user)
    
main()
    
