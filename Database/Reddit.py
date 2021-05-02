import praw
import json
import time
import traceback
import os.path
import pathlib
import requests
import sys
from datetime import datetime
from content_url_filter import * # pylint: disable=unused-wildcard-import
from sqlite_helper import * # pylint: disable=unused-wildcard-import

upvoted_posts = {}
fields = ["Post Name", "Author", "Subreddit", "Created (in Seconds)", "Url to Content", "Url to Post", "Url to Thumbnail", "Is Saved", "Is NSFW", "Number"]

# Create Reddit Instance and Start Processing
def collect_upvoted_posts(bot_id, bot_secret, token, user, imgur_id, connection):
    reddit = praw.Reddit(
        client_id = bot_id,
        client_secret = bot_secret,
        refresh_token = token,
        user_agent = user
    )
    
    print_to_system("User being checked: " + reddit.user.me().name)
    
    store_upvoted_posts(reddit, imgur_id, connection)
    save_file(reddit, connection)
    
    print_to_system("Upvotes have been saved!")
    print_to_system("Total of " + str(len(upvoted_posts)) + " upvoted posts archived.")

# Begin Storing Upvoted Posts
def store_upvoted_posts(reddit, imgur_id, connection):
    try:
        post_count = 0
        matches = 0 # Match Counter (counts up to 10, if it reaches 10 it breaks the loop)
        old_post_keys = select_all_post_keys(connection)

        # Add Posts to List
        for item in reddit.user.me().upvoted(limit=None):
            # Increase Post Count
            post_count = post_count + 1
    
            title = item.title

            if post_count >= 82:
                print()

            author = item.author.name if item.author is not None else "Deleted"
            
            # Make Post Entry Name
            post_name = title + " - " + author + " - " + format_time(item.created)
            post_name_utc = title + " - " + author + " - " + format_time(item.created_utc)
            
            if post_name in old_post_keys or post_name_utc in old_post_keys:
                matches = matches + 1
                print_to_system("Post Already in Archive. " + str((10 - matches)) + " more posts till termination.")
                time.sleep(0.1)
                
                if(matches >= 10):
                    break
                
                else:
                    continue
            
            # Reset Matches Counter
            matches = 0
            
            # Print the storing message
            print_to_system("Post " + str(post_count) + " has been stored.")

            # Get All Variables
            # print(vars(item))
            
            # Fill in Post Entries
            post_entry = {}
            post_entry[fields[0]] = title
            post_entry[fields[1]] = author
            post_entry[fields[2]] = item.subreddit.display_name
            post_entry[fields[3]] = format_time(item.created_utc)
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
        print_to_system("Something went wrong!\nError Msg:\n" + str(e))
        traceback.print_exc()

def format_time(seconds):
    return datetime.utcfromtimestamp(seconds).strftime("%Y-%m-%d %H:%M:%S")
        
def save_file(reddit, connection):
    og_post_count = find_post_count(connection)
    
    # Correct The Post Numbers
    to_subtract = len(upvoted_posts)
    for entry in upvoted_posts:
        upvoted_posts[entry][fields[9]] = -1 * (upvoted_posts[entry][fields[9]] - to_subtract)
        upvoted_posts[entry][fields[9]] = upvoted_posts[entry][fields[9]] + og_post_count + 1
        post = (upvoted_posts[entry][fields[9]], upvoted_posts[entry][fields[0]], upvoted_posts[entry][fields[1]], upvoted_posts[entry][fields[2]], '0', upvoted_posts[entry][fields[3]], upvoted_posts[entry][fields[4]], upvoted_posts[entry][fields[5]], upvoted_posts[entry][fields[6]], upvoted_posts[entry][fields[7]], upvoted_posts[entry][fields[8]])
        create_post(connection, post)

    print_to_system("Completed")
    
def print_to_system(text):
    print(text)
    sys.stdout.flush()
    
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
    
    database_path = str(pathlib.Path(__file__).parent.absolute()) + "/RedditPosts.db"
    connection = create_connection(database_path)
    
    collect_upvoted_posts(bot_id, bot_secret, token, user, imgur_id, connection)
    
main()
    
