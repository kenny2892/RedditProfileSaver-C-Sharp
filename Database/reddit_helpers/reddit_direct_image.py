import praw

def check_direct_post(reddit_post, url):
    keys = reddit_post.media_metadata.keys()

    if(len(keys) != 1):
        return url

    meta = reddit_post.media_metadata[list(keys)[0]]

    if meta is None or meta['status'] == 'failed':
        return url
    
    elif meta['e'] == 'Image':
        source = meta['s']
        return source['u']
        
    elif meta['e'] == 'AnimatedImage':
        source = meta['s']
        return source['gif']

    return url