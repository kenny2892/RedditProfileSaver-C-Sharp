import praw
import json
import time
import traceback
import os.path
import pathlib
import requests
from content_url_filter import * # pylint: disable=unused-wildcard-import

overwrite_old_file = False

upvoted_posts = {}
old_upvoted_posts = {}
fields = ["Post Name", "Author", "Subreddit", "Created (in Seconds)", "Url to Content", "Url to Post", "Url to Thumbnail", "Is Saved", "Is NSFW", "Number"]

# Create Reddit Instance and Start Processing
def collect_upvoted_posts(bot_id, bot_secret, token, user, imgur_id):
    reddit = praw.Reddit(
        client_id = bot_id,
        client_secret = bot_secret,
        refresh_token = token,
        user_agent = user
    )
    
    print_to_file("User being checked: " + reddit.user.me().name + "\n")
    
    load_file(reddit)
    store_upvoted_posts(reddit, imgur_id)
    save_file(reddit)
    
    print("Upvotes have been saved!")
    print("Total of " + str(len(upvoted_posts)) + " upvoted posts archived.")

# Check if Upvoted File Exists
def load_file(reddit):
    file_path = str(pathlib.Path(__file__).parent.absolute()) + "/" + reddit.user.me().name + " - Upvoted.json"
    if(not overwrite_old_file & os.path.isfile(file_path)):
        with open(file_path) as file:
            global old_upvoted_posts 
            old_upvoted_posts = json.load(file)

# Begin Storing Upvoted Posts
def store_upvoted_posts(reddit, imgur_id):
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
                print_to_file("Post Already in Archive. " + str((10 - matches)) + " more posts till termination.\n")
                time.sleep(0.2)
                
                if(matches >= 10):
                    break
                
                else:
                    continue
            
            # Reset Matches Counter
            matches = 0
            
            # Print the storing message
            print_to_file("Post " + str(post_count) + " has been stored.\n")
            
            # Get All Variables
            # print(vars(item))
            
            # Fill in Post Entries
            post_entry = {}
            post_entry[fields[0]] = title
            post_entry[fields[1]] = author
            post_entry[fields[2]] = item.subreddit.display_name
            post_entry[fields[3]] = item.created_utc
            post_entry[fields[4]] = filter_content_url(item.url, item, reddit, imgur_id)            
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
        print_to_file("Something went wrong!\nError Msg:\n" + str(e) + "\n")
        traceback.print_exc()
        
def save_file(reddit):
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

    print_to_file("Completed")
    
def print_to_file(text):
    file_path = str(pathlib.Path(__file__).parent.absolute()) + "/Results.txt"
    resultsFile = open(file_path, "a")
    
    resultsFile.write(text)
    resultsFile.close()
    
    print(text)
    
def main():
    # Get Config File
    config = {}
    config_path = str(pathlib.Path(__file__).parent.absolute()) + "/Config.json"
    
    if(os.path.isfile(config_path)):
        with open(config_path) as config_file:
            config = json.load(config_file)
            
    bot_id = config.get("Id", "")
    bot_secret = config.get("Secret", "")
    token = config.get("Refresh Token", "")
    user = config.get("User Agent", "")
    imgur_id = config.get("ImgurClientId", "")
    
    collect_upvoted_posts(bot_id, bot_secret, token, user, imgur_id)
    
main()
    
