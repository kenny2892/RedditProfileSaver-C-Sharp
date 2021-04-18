import praw
import time
from reddit_helpers import * # pylint: disable=unused-wildcard-import

def filter_content_url(url, reddit_post, reddit, imgur_id):
    try:
        new_url = url

        if("reddit.com/gallery/" in url):
            new_url = convert_reddit_gallery(url, reddit)
                
        elif("redgifs" in url):
            new_url = convert_redgif(url)
            
        elif("v.redd.it" in url):
            new_url = convert_vreddit(reddit_post, url)
            
        elif("imgur.com/a/" in url):
            new_url = convert_imgur_gallery(url, imgur_id)
            
        elif("//imgur.com/" in url and "/gallery/" not in url and "/r/" not in url):
            numOfPeriods = 0
            for i in url:
                if(i == '.'):
                    numOfPeriods = numOfPeriods + 1

            if(numOfPeriods == 1): ## Meaning there are no extensions like .png or .jpg
                new_url = convert_imgur_image(url, imgur_id)

        return new_url

    except:
        return url

