import time
import praw

def convert_vreddit(post, url):
    try:
        new_url = url
        post_video = post.media['reddit_video']

        new_url = post_video['fallback_url']
        new_url = new_url.split("?")[0]
        new_url = url + " " + new_url + " " + str(post_video['width']) + " " + str(post_video['height'])

        # Sleep for 100 Millisec
        time.sleep(0.1)

        return new_url
        
    except:
        return url
